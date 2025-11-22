using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SparkCode.CustomAPIs
{
    public class RunFetchXml : IPlugin
    {
        Context ctx = new Context();
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // API Inputs
            string fetchXml = context.InputParameters["FetchXml"] as string ?? throw new ArgumentNullException($"FetchXml is required");

            // Trace Inputs
            ctx.Trace($"FetchXml: {fetchXml}");

            // API Outputs
            string results = string.Empty;

            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            // Execute FetchXml
            var fetchExpression = new FetchExpression(fetchXml);
            var retrievedRecords = service.RetrieveMultiple(fetchExpression);

            // Convert results into serializable format
            var formattedResults = new List<Dictionary<string, object>>();
            foreach (var entity in retrievedRecords.Entities)
            {
                var record = new Dictionary<string, object>();
                foreach (var attr in entity.Attributes)
                {
                    record[attr.Key] = attr.Value is AliasedValue av ? av.Value : attr.Value;
                }
                formattedResults.Add(record);
            }

            // Serialize results into JSON
            results = JsonSerializer.Serialize(formattedResults);

            // Trace Outputs
            ctx.Trace($"results: {results}");

            // API Outputs
            context.OutputParameters["Results"] = results;
        }
    }
}
