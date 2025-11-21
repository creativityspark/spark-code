using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace SparkCode.CustomAPIs
{
    // Based on: https://www.rfc-editor.org/rfc/rfc9535.html
    // Library used: https://www.newtonsoft.com/json/help/html/QueryJsonSelectToken.htm
    public class JSonSelect : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // Custom API Inputs
            string inputObject = context.InputParameters["InputObject"] as string ?? throw new ArgumentNullException($"Url is required");
            string query = context.InputParameters["Query"] as string ?? throw new ArgumentNullException($"Query is required");

            string outputObject = Select(ctx, inputObject, query);

            // Set OutputParameters values
            context.OutputParameters["OutputObject"] = outputObject;
        }

        public string Select(Context ctx, string inputObject, string query)
        {
            // Trace input parameters
            ctx.Trace($"InputParameters: {inputObject}");
            ctx.Trace($"Query: {query}");

            // Custom API Outputs
            string outputObject = null;
            try
            {
                outputObject = JObject.Parse(inputObject).SelectToken(query)?.ToString();
            }
            catch (Newtonsoft.Json.JsonException)
            {
                // This exception is thrown if the query does not match a single token.
                ctx.Trace($"Query did not match a single token, trying to select multiple tokens.");
                var outputList = JObject.Parse(inputObject).SelectTokens(query)?.Select(x=>x.ToString());
                outputObject = String.Join(",",outputList);
            }

            ctx.Trace($"OutputObject: {outputObject}");
            return outputObject;
        }
    }
}
