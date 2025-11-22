using Microsoft.Xrm.Sdk;
using SparkCode.CustomAPIs.Dataverse;
using System;

namespace SparkCode.CustomAPIs.Other
{
    public class ParseURL : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // Custom API Inputs
            string url = context.InputParameters["Url"] as string ?? throw new ArgumentNullException($"Url is required");
            string typeName = null;

            if(TryParse(url, out int etc, out Guid id))
            {
                ctx.Trace("Getting type name...");
                typeName = ctx.Service.GetTableLogicalName(etc);
                ctx.Trace($"Type name: {typeName}");
            }
            else
            {
                throw new InvalidPluginExecutionException($"Couldn't find correct parameters in URL {url}");
            }


            // Set OutputParameters values
            context.OutputParameters["etc"] = etc;
            context.OutputParameters["id"] = id;
            context.OutputParameters["typeName"] = typeName;
        }

        public bool TryParse(string url, out int etc, out Guid id)
        {
            // Trace input parameters
            ctx.Trace($"Url: {url}");

            var uri = new Uri(url);
            int foundCount = 0;
            etc = 0;
            id = Guid.Empty;
            bool found = false;

            string[] parameters = uri.Query.TrimStart('?').Split('&');
            foreach (string param in parameters)
            {
                var nameValue = param.Split('=');
                switch (nameValue[0])
                {
                    case "etc":
                        if(int.TryParse(nameValue[1], out etc))
                        {
                            foundCount++;
                        }
                        else
                        {
                            ctx.Trace($"Failed to parse etc: {nameValue[1]}");
                        }
                        break;
                    case "id":

                        if(Guid.TryParse(nameValue[1], out id))
                        {
                            foundCount++;
                        }
                        else
                        {
                            ctx.Trace($"Failed to parse id: {nameValue[1]}");
                        }
                        break;
                }
                if (foundCount > 1) { 
                    found = true;
                    break; 
                }
            }

            ctx.Trace($"Found: {found}");
            ctx.Trace($"etc: {etc}");
            ctx.Trace($"id: {id}");

            return found;
        }
    }
}
