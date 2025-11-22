using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace SparkCode.CustomAPIs
{
    /// <summary>
    /// Performs a JSONQuery select on the specified data and returns the results
    /// Based on: https://www.rfc-editor.org/rfc/rfc9535.html
    /// Library used: https://www.newtonsoft.com/json/help/html/QueryJsonSelectToken.htm
    /// Input Parameters:
    /// Data: Data in JSON format.
    /// Query: JSON Queuery select string.
    /// Outoput Parameters:
    /// Data: Data returned by the query.
    /// </summary>
    public class Select : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // API Inputs
            string data = context.InputParameters["Data"] as string ?? throw new ArgumentNullException($"Data is required");
            string query = context.InputParameters["Query"] as string ?? throw new ArgumentNullException($"Query is required");

            string results = RunQuery(data, query);

            // API Outputs
            context.OutputParameters["Results"] = results;
        }

        public string RunQuery(string data, string query)
        {
            // Trace input parameters
            ctx.Trace($"Data: {data}");
            ctx.Trace($"Query: {query}");

            // API Outputs
            string results = null;
            try
            {
                results = JObject.Parse(data).SelectToken(query)?.ToString();
            }
            catch (Newtonsoft.Json.JsonException)
            {
                // This exception is thrown if the query does not match a single token.
                ctx.Trace($"Query did not match a single token, trying to select multiple tokens.");
                var outputList = JObject.Parse(data).SelectTokens(query)?.Select(x=>x.ToString());
                results = String.Join(",",outputList);
            }

            ctx.Trace($"Results: {results}");
            return results;
        }
    }
}
