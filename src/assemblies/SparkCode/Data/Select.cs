using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace SparkCode.API.Data
{
    /// <summary>
    /// Performs a JSONQuery select on the specified data and returns the results
    /// Based on: https://www.rfc-editor.org/rfc/rfc9535.html
    /// Library used: https://www.newtonsoft.com/json/help/html/QueryJsonSelectToken.htm
    /// Input Parameters:
    /// Data: Type string. Data in JSON format.
    /// Query: Type string. JSON Queuery select string.
    /// Outoput Parameters:
    /// Data: Type string. Data returned by the query in JSON format.
    /// </summary>
    public class Select : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            Context ctx = new Context(serviceProvider);

            // API Inputs
            string data = ctx.GetInputParameter<string>("Data", true);
            string query = ctx.GetInputParameter<string>("Query", true);

            // Run Logic
            string results = RunQuery(ctx, data, query);

            // API Outputs
            ctx.SetOutputParameter("Results", results);
        }

        public string RunQuery(Context ctx, string data, string query)
        {
            string results = null;
            try
            {
                results = JToken.Parse(data).SelectToken(query)?.ToString();
            }
            catch (Newtonsoft.Json.JsonException)
            {
                // This exception is thrown if the query does not match a single token.
                ctx.Trace($"Query did not match a single token, trying to select multiple tokens.");
                var outputList = JToken.Parse(data).SelectTokens(query)?.Select(x=>x.ToString());
                results = String.Join(",",outputList);
            }
            return results;
        }
    }
}
