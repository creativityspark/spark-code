using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.Text
{
    public static class ParseURL
    {
        public static Entity Parse(Context ctx, string url)
        {
            var results = new Entity();
            Uri uri = new Uri(url);

            results = new Entity();
            results["scheme"] = uri.Scheme;
            results["host"] = uri.Host;
            results["port"] = uri.Port;
            results["absolutePath"] = uri.AbsolutePath;
            if (uri.Fragment.Length > 0)
            {
                results["fragment"] = uri.Fragment.Substring(1);
            }

            // parse query parameters
            var queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (queryParameters.Count > 0)
            {
                var query = new Entity();
                foreach (string key in queryParameters)
                {
                    query[key] = queryParameters[key];
                }
                results["query"] = query;
            }

            return results;
        }
    }
}
