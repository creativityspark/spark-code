using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace SparkCode.API.Dataverse
{
    public class RunFetchXml : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string fetchXml = ctx.GetInputParameter<string>("FetchXml", true);

            // Run Logic
            var results = ExecuteFetchXml(ctx, fetchXml);

            // API Outputs
            ctx.SetOutputParameter("Results", results);
            ctx.SetOutputParameter("ResultsJson", results.ToJson());
        }

        private EntityCollection ExecuteFetchXml(Context ctx, string fetchXml)
        {
            // Replace the record id in the FetchXML if specified
            var id = ctx.PluginContext.PrimaryEntityId;
            fetchXml = fetchXml.Replace("{{id}}", id.ToString());

            // Execute FetchXml
            var fetchExpression = new FetchExpression(fetchXml);
            var results = ctx.Service.RetrieveMultiple(fetchExpression);

            return results;
        }
    }
}
