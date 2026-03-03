using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace SparkCode.Data
{
    public static class Select
    {
        public static string RunQuery(Context ctx, string data, string query)
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
                var outputList = JToken.Parse(data).SelectTokens(query)?.Select(x => x.ToString());
                results = String.Join(",", outputList);
            }
            return results;
        }
    }
}
