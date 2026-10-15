using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.PowerApps
{
    /// <summary>
    /// A plugin that returns the details of the current organization.
    /// </summary>
    /// <displayName>Get Organization Details</displayName>
    /// <param name="Output" type="expando" direction="output">Details of the current organization, including its endpoints.</param>
    public class GetOrganizationDetails : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // Run Logic
            var details = ctx.Service.GetOrganizationDetails();
            var output = Conversions.ToEntity(details);

            // API Outputs
            ctx.SetOutputParameter("Output", output);
            ctx.SetOutputParameter("OutputJson", output.ToJson());
        }
    }
}
