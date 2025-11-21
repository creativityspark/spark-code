using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SparkCode.CustomAPIs
{
    public class RunFetchXmlQuery : IPlugin
    {
        Context ctx = new Context();
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // Custom API Inputs
            string fetchXml = context.InputParameters["FetchXml"] as string;

            // Trace input parameters
            ctx.Trace($"FetchXml: {fetchXml}");

            // Custom API Outputs
            string FetchResults = string.Empty;

            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            // Execute FetchXml
            var fetchExpression = new FetchExpression(fetchXml);
            var results = service.RetrieveMultiple(fetchExpression);

            // Convert results into serializable format
            var formattedResults = new List<Dictionary<string, object>>();
            foreach (var entity in results.Entities)
            {
                var record = new Dictionary<string, object>();
                foreach (var attr in entity.Attributes)
                {
                    record[attr.Key] = attr.Value is AliasedValue av ? av.Value : attr.Value;
                }
                formattedResults.Add(record);
            }

            // Serialize results into JSON
            FetchResults = JsonSerializer.Serialize(formattedResults);

            // Set OutputParameters values
            context.OutputParameters["FetchResults"] = FetchResults;
        }
    }
}
