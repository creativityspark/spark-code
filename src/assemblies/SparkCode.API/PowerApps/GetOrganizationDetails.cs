using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Organization;
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
            var output = Convert(details);

            // API Outputs
            ctx.SetOutputParameter("Output", output);
            ctx.SetOutputParameter("OutputJson", output.ToJson());
        }

        // Converts an organization detail into an entity. Endpoints are returned as a nested
        // entity keyed by endpoint type name. Not an XML doc comment: the registration tool
        // would treat a documented method as an API member.
        private static Entity Convert(OrganizationDetail details)
        {
            var entity = new Entity();
            entity["OrganizationId"] = details.OrganizationId.ToString();
            entity["FriendlyName"] = details.FriendlyName;
            entity["OrganizationVersion"] = details.OrganizationVersion;
            entity["EnvironmentId"] = details.EnvironmentId;
            entity["DatacenterId"] = details.DatacenterId.ToString();
            entity["Geo"] = details.Geo;
            entity["TenantId"] = details.TenantId;
            entity["UrlName"] = details.UrlName;
            entity["UniqueName"] = details.UniqueName;
            entity["State"] = details.State.ToString();
            entity["SchemaType"] = details.SchemaType;
            entity["OrganizationType"] = details.OrganizationType.ToString();

            var endpoints = new Entity();
            if (details.Endpoints != null)
            {
                foreach (var endpoint in details.Endpoints)
                {
                    endpoints[endpoint.Key.ToString()] = endpoint.Value;
                }
            }
            entity["Endpoints"] = endpoints;

            return entity;
        }
    }
}
