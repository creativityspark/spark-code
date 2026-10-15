using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.PowerApps
{
    /// <summary>
    /// A plugin that returns the URL of the current environment for a given endpoint type.
    /// </summary>
    /// <displayName>Get Organization Url</displayName>
    /// <param name="Type" type="string" optional="true">Endpoint type. Any value of Microsoft.Xrm.Sdk.Organization.EndpointType (WebApplication, OrganizationService, OrganizationDataService). Defaults to WebApplication.</param>
    /// <param name="Output" type="string" direction="output">The url of the current environment for the specified endpoint type.</param>
    public class GetOrganizationUrl : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string type = ctx.PluginContext.InputParameters.Contains("Type")
                ? ctx.PluginContext.InputParameters["Type"] as string
                : null;

            // Run Logic
            var url = ctx.Service.GetOrganizationUrl(type);

            // API Outputs
            ctx.SetOutputParameter("Output", url);
        }
    }
}
