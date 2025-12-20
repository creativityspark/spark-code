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
        Console.WriteLine("Connected to Dataverse!");
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
    string solutionPrefix = "csp_";
    string assemblyName = "SparkCode.API";
    string packageName = solutionPrefix+assemblyName;
    var solutionName = "SparkCode";
    var documentationFile = "..\\..\\..\\..\\SparkCode.API\\bin\\Debug\\net462\\SparkCode.API.xml";

    var apiSpec = LoadSpecification(documentationFile);
    var pluginPackage = GetPluginPackage(client, packageName);
    var pluginAssembly = GetPluginAssembly(client, pluginPackage.Id, assemblyName);

    NormalizeSpecification(apiSpec);

    foreach (var api in apiSpec.Members)
    {
        var existingAPI = Upsert(client, pluginAssembly.Id, api);
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

        var hasEpando = false;
        if (member.Parameters != null)
        {
            foreach (var param in member.Parameters)
            {
                var direction = "in";
                if(param.Direction == "out" || param.Direction == "output")
                {
                    direction = "out";
                }
                param.Direction = direction;
                param.UniqueName = param.Name;
                param.DisplayName = String.IsNullOrEmpty(param.DisplayName)?param.Name:param.DisplayName;
                param.Name = member.Name + "-" + direction + "-" + param.Name;
                param.TypeValue = GetParameterType(param.Type);

                if (param.Type != null && param.Type.Contains("expando"))
                {
                    hasEpando = true;
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
                        UniqueName = param.UniqueName,
                        DisplayName = param.DisplayName,
                        Type = param.Type,
                        TypeValue = param.TypeValue
                    };
                    if (param.Type != null && param.Type.Contains("expando"))
                    {
                        newParam.Name = param.Name + "Json";
                        newParam.Type = "string";
                        newParam.TypeValue = GetParameterType("string");
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

OptionSetValue GetParameterType(string? type)
{
    switch (type)
    {
        case "bool":
        case "boolean":
            return new OptionSetValue(0);
        case "datetime":
            return new OptionSetValue(1);
        case "decimal":
            return new OptionSetValue(2);
        case "entity":
            return new OptionSetValue(3);
        case "entitycollection":
            return new OptionSetValue(4);
        case "entityreference":
            return new OptionSetValue(5);
        case "float":
            return new OptionSetValue(6);
        case "int":
        case "integer":
            return new OptionSetValue(7);
        case "money":
            return new OptionSetValue(8);
        case "picklist":
            return new OptionSetValue(9);
        case "string":
            return new OptionSetValue(10);
        case "stringarray":
            return new OptionSetValue(11);
        case "guid":
            return new OptionSetValue(12);
        default:
            return new OptionSetValue(10); // default to string
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

Entity Upsert(ServiceClient client, Guid assemblyId, Member api)
{
    Entity existingAPI = GetApi(client, api.UniqueName);
    Entity pluginType = GetPluginType(client, assemblyId, api.TypeName);
    if (existingAPI == null)
    {
        existingAPI = CreateApi(client, pluginType, api);
        Console.WriteLine($"Created Custom API: {api.Name}");
    }
    else
    {
        Console.WriteLine($"Custom API already exists: {api.Name}");
    }
    UpsertParameters(client, api, existingAPI);
    return existingAPI;
}

void UpsertParameters(ServiceClient client, Member api, Entity apiRecord)
{
    foreach (var parameter in api.Parameters)
    {
        if (parameter.Direction == "in")
        {
            var inputParameter = GetInputParameter(client, apiRecord.Id, parameter.UniqueName);
            if (inputParameter == null)
            {
                inputParameter = CreateInputParameter(client, api, parameter, apiRecord.Id);
                Console.WriteLine($"Created Input Parameter: {parameter.Name}");
            }
            else
            {
                Console.WriteLine($"Input Parameter already exists: {parameter.Name}");
            }
        }
        else
        {
        }
    }
}

Entity CreateInputParameter(ServiceClient client, Member api, Parameter parameter, Guid apiId)
{
    // create a customapirequestparameter record
    var newParam = new Entity("customapirequestparameter");
    newParam["uniquename"] = parameter.UniqueName;
    newParam["name"] = parameter.Name;
    newParam["displayname"] = parameter.DisplayName;
    newParam["description"] = parameter.Description?.Trim();
    newParam["customapiid"] = new EntityReference("customapi", apiId);
    newParam["isoptional"] = parameter.IsOptional;
    newParam["type"] = parameter.TypeValue;
    newParam["iscustomizable"] = true;
    var newParamId = client.Create(newParam);
    return client.Retrieve("customapirequestparameter", newParamId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
}

Entity GetInputParameter(ServiceClient client, Guid apiId, string uniqueName)
{
    var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("customapirequestparameter")
    {
        ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("customapirequestparameterid"),
        Criteria =
        {
            Conditions =
            {
                new Microsoft.Xrm.Sdk.Query.ConditionExpression("uniquename", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, uniqueName),
                new Microsoft.Xrm.Sdk.Query.ConditionExpression("customapiid", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, apiId)
            }
        }
    };
    var results = client.RetrieveMultiple(query);
    return results.Entities.FirstOrDefault();
}

Entity CreateApi(ServiceClient client, Entity pluginType, Member api)
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
    newApi["plugintypeid"] = pluginType.ToEntityReference();
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

Entity GetPluginAssembly(ServiceClient client, Guid packageId, string name)
{
    // query the pluginassembly to retrieve the api assembly
    var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("pluginassembly")
    {
        ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("pluginassemblyid"),
        Criteria =
        {
            Conditions =
            {
                new Microsoft.Xrm.Sdk.Query.ConditionExpression("name", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, name),
                new Microsoft.Xrm.Sdk.Query.ConditionExpression("packageid", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, packageId)
            }
        }
    };
    var results = client.RetrieveMultiple(query);
    return results.Entities.FirstOrDefault();
}

Entity GetPluginType(ServiceClient client, Guid pluginAssemblyId, string name)
{
    // query the plugintype to retrieve the api assembly
    var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("plugintype")
    {
        ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("plugintypeid"),
        Criteria =
        {
            Conditions =
            {
                new Microsoft.Xrm.Sdk.Query.ConditionExpression("typename", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, name),
                new Microsoft.Xrm.Sdk.Query.ConditionExpression("pluginassemblyid", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, pluginAssemblyId)
            }
        }
    };
    var results = client.RetrieveMultiple(query);
    return results.Entities.FirstOrDefault();
}