using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace SparkCode.API.Dataverse
{
    /// <summary>
    /// A plugin that Publishes a webresource.
    /// </summary>
    /// <displayName>Publish Web Resource</displayName>
    /// <param name="WebResourceName" type="string">The Web Resource id to Publish.</param>

    public class PublishWebResource : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string webResourceName = ctx.GetInputParameter<string>("WebResourceName", true);


            //Run Logic

            var query = new QueryExpression("webresource")
            {
                ColumnSet = new ColumnSet("webresourceid", "name")
            };

            query.Criteria.AddCondition("name", ConditionOperator.Equal, webResourceName);

            var result = ctx.Service.RetrieveMultiple(query);

            if (result.Entities.Count == 0)
            {
                throw new InvalidPluginExecutionException($"No web resource found with name: {webResourceName}");
            }

            if (result.Entities.Count > 1)
            {
                throw new InvalidPluginExecutionException($"More than one web resource found with name: {webResourceName}");
            }

            var webResourceId = result.Entities[0].Id;

            //Busqueda por nombre y devuelvo el id, si no hay nada devuelvo error y si no sigo

            string publishXml = $"<importexportxml><webresources><webresource>{webResourceId}</webresource></webresources></importexportxml>";
                    
                    PublishXmlRequest publishRequest = new PublishXmlRequest
                    {
                        ParameterXml = publishXml
                    };

                ctx.Service.Execute(publishRequest);

                Console.WriteLine("Web Resource published successfully!");

        }
    }
}
