// Connection string
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using SparkCode.APIRegistrationTool;
using System.Xml.Linq;
using System.Xml.Serialization;

Console.WriteLine("Creativity Spark's API Registration Tool");
Console.Write("Environment URL:");
var url = Console.ReadLine();

string connectionString =
    $@"AuthType=OAuth;
      Url={url};
      AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;
      RedirectUri=http://localhost;
      LoginPrompt=Auto";

// Initialize the service client
using (var serviceClient = new ServiceClient(connectionString))
{
    if (serviceClient.IsReady)
    {
        Console.WriteLine("Successfully connected to Dataverse!");
        Console.WriteLine($"Organization: {serviceClient.ConnectedOrgFriendlyName}");
        RegisterCustomAPIs(serviceClient);
    }
    else
    {
        Console.WriteLine("Failed to connect to Dataverse.");
        Console.WriteLine(serviceClient.LastError);
    }
}

void RegisterCustomAPIs(ServiceClient client)
{
    string packageName = "csp_SparkCode.API";
    var solutionName = "SparkCode";
    var documentationFile = "..\\..\\..\\..\\SparkCode\\bin\\Debug\\net462\\SparkCode.API.xml";

    var apiSpec = LoadSpecification(documentationFile);
    var pluginPackage = GetPluginPackage(client, packageName);

    NormalizeSpecification(apiSpec);

    foreach (var api in apiSpec.Members)
    {
        var existingAPI = Upsert(client, api);
        AddToSolution(client, existingAPI, solutionName);
    }

}

void NormalizeSpecification(APISpecification spec)
{
    const string apiPrefix = "csp_";
    const string assemblyPrefix = "T:";
    const string memberPrefix = "SparkCode.API.";

    var newMembers = new List<Member>();

    foreach (var member in spec.Members)
    {
        member.TypeName = member.TypeName.Replace(assemblyPrefix, "");
        member.Name = member.TypeName.Replace(memberPrefix, "");
        member.UniqueName = apiPrefix + member.TypeName.Replace(memberPrefix, "");
        member.EnabledForWorkflow = true;

        // check if the member has any parameter type expando
        var hasEpando = false;
        if (member.Parameters != null)
        {
            foreach (var param in member.Parameters)
            {
                if (param.Type != null && param.Type.Contains("expando"))
                {
                    hasEpando = true;
                    break;
                }
            }
        }

        if (hasEpando)
        {
            member.EnabledForWorkflow = false;
            // copy this member to a new member with "json" suffix
            var newMember = new Member
            {
                Name = member.Name + "Json",
                TypeName = member.TypeName,
                UniqueName = member.UniqueName + "Json",
                DisplayName = member.DisplayName + " (JSON)",
                Description = member.Description,
                EnabledForWorkflow = true
            };

            if (member.Parameters != null)
            {
                newMember.Parameters = new List<Parameter>();
                foreach (var param in member.Parameters)
                {
                    var newParam = new Parameter
                    {
                        Name = param.Name,
                        Description = param.Description,
                        Direction = param.Direction,
                        Type = param.Type
                    };
                    if (param.Type != null && param.Type.Contains("expando"))
                    {
                        newParam.Name = param.Name + "Json";
                        newParam.Type = "string";
                    }
                    newMember.Parameters.Add(newParam);
                }
            }
            newMembers.Add(newMember);
        }
    }
    if(newMembers.Count > 0)
    {
        spec.Members.AddRange(newMembers);
    }
}

void AddToSolution(ServiceClient client, Entity existingAPI, string solutionName)
{
    // check if the custom api is already in the solution
    var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("solutioncomponent")
    {
        ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("solutioncomponentid"),
        Criteria =
        {
            Conditions =
            {
                new Microsoft.Xrm.Sdk.Query.ConditionExpression("objectid", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, existingAPI.Id),
            }
        },
        LinkEntities =         
        {
            new Microsoft.Xrm.Sdk.Query.LinkEntity
            {
                LinkFromEntityName = "solutioncomponent",
                LinkFromAttributeName = "solutionid",
                LinkToEntityName = "solution",
                LinkToAttributeName = "solutionid",
                LinkCriteria =
                {
                    Conditions =
                    {
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("uniquename", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, solutionName)
                    }
                }
            }
        }
    };
    var results = client.RetrieveMultiple(query);
    if (results.Entities.Count == 0)
    {
        // add the custom api to the solution
        var addRequest = new AddSolutionComponentRequest
        {
            ComponentType = 10023, // Custom API
            ComponentId = existingAPI.Id,
            SolutionUniqueName = solutionName
        };
        client.Execute(addRequest);
    }

}

Entity Upsert(ServiceClient client, Member api)
{
    Entity existingAPI = GetApi(client, api.UniqueName);
    if (existingAPI == null)
    {
        var apiAssembly = GetAssembly(client, api.TypeName);
        existingAPI = CreateApi(client, api, apiAssembly);
        Console.WriteLine($"Created Custom API: {api.Name}");
    }
    else
    {
        Console.WriteLine($"Custom API already exists: {api.Name}");
    }
    UpsertParameters(client, api);
    return existingAPI;
}

void UpsertParameters(ServiceClient client, Member api)
{
    foreach (var parameter in api.Parameters)
    {
        var existingParameter = GetParameter(client, api.UniqueName, parameter.Name);
        if (existingParameter == null)
        {
            CreateParameter(client, api, parameter);
            Console.WriteLine($"  Created Parameter: {parameter.Name}");
        }
        else
        {
            Console.WriteLine($"  Parameter already exists: {parameter.Name}");
        }
    }
}

object GetParameter(ServiceClient client, string uniqueName, string name)
{
    throw new NotImplementedException();
}

Entity GetAssembly(ServiceClient client, string assemblyName)
{
    // query the plugintype to retrieve the api assembly
    var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("plugintype")
    {
        ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("plugintypeid"),
        Criteria =
        {
            Conditions =
            {
                new Microsoft.Xrm.Sdk.Query.ConditionExpression("typename", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, assemblyName)
            }
        }
    };
    var results = client.RetrieveMultiple(query);
    return results.Entities.FirstOrDefault();
}

Entity CreateApi(ServiceClient client, Member api, Entity pluginPackage)
{
    if (api.DisplayName == null)
    {
        throw new Exception($"API {api.TypeName} is missing a display name.");
    }
    var newApi = new Entity("customapi");
    newApi["uniquename"] = api.UniqueName;
    newApi["displayname"] = api.DisplayName.Trim();
    newApi["name"] = api.Name;
    newApi["description"] = api.Description?.Trim();
    newApi["plugintypeid"] = pluginPackage.ToEntityReference();
    newApi["allowedcustomprocessingsteptype"] = new OptionSetValue(0); // None
    newApi["bindingtype"] = new OptionSetValue(0); // Global
    newApi["isfunction"] = false;
    newApi["isprivate"] = false;
    newApi["iscustomizable"] = true;
    newApi["workflowsdkstepenabled"] = api.EnabledForWorkflow;

    var apiId = client.Create(newApi);

    return client.Retrieve("customapi", apiId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
}

Entity GetApi(ServiceClient client, string apiName)
{
    // Searches for an existing Custom API by uniquename
    var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("customapi")
    {
        ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("customapiid"),
        Criteria =
        {
            Conditions =
            {
                new Microsoft.Xrm.Sdk.Query.ConditionExpression("uniquename", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, apiName)
            }
        }
    };
    var results = client.RetrieveMultiple(query);
    return results.Entities.FirstOrDefault();
}

APISpecification LoadSpecification(string documentationFile)
{
    APISpecification result;
    XDocument doc = XDocument.Load(documentationFile);
    using (var reader = doc.Root.CreateReader())
    {
        XmlSerializer serializer = new XmlSerializer(typeof(APISpecification));
        result = (APISpecification)serializer.Deserialize(reader);
    }
    return result;
}

Entity GetPluginPackage (ServiceClient client, string name)
{
    // Gets the package entity reference for the specified name
    var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("pluginpackage")
    {
        ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("pluginpackageid"),
        Criteria =
        {
            Conditions =
            {
                new Microsoft.Xrm.Sdk.Query.ConditionExpression("name", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, name)
            }
        }
    };
    var results = client.RetrieveMultiple(query);
    return results.Entities.FirstOrDefault();
}
