using Microsoft.Xrm.Sdk;
using SparkCode.CustomAPIs.Dataverse;
using System;

namespace SparkCode.CustomAPIs.Other
{
    public class ParseURL : IPlugin
    {
        Context ctx = new Context();

        public ParseURL() {}
        public ParseURL(Context context)
        {
            ctx = context;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // Custom API Inputs
            string url = context.InputParameters["Url"] as string ?? throw new ArgumentNullException($"Url is required");

            var results = Parse(url);

            // Set OutputParameters values
            context.OutputParameters["Results"] = results;
        }

        public Entity Parse(string url)
        {
            // trace input values
            ctx.Trace($"Input Parameters: Url: {url}");

            var results = new Entity();
            Uri uri = new Uri(url);

            results = new Entity();
            var query = new Entity();
            results["scheme"] = uri.Scheme;
            results["host"] = uri.Host;
            results["port"] = uri.Port;
            results["absolutePath"] = uri.AbsolutePath;
            results["fragment"] = uri.Fragment.Length > 0 ? uri.Fragment.Substring(1) : string.Empty;

            // parse query parameters
            var queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
            foreach (string key in queryParameters)
            {
                query[key] = queryParameters[key];
            }

            results["query"] = query;

            // trace output values
            ctx.Trace($"Results: {results}");

            return results;
        }
    }
}
