using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Organization;
using System;
using System.Globalization;

namespace SparkCode
{
    /// <summary>
    /// Common conversions between Dataverse values and the types exposed by the custom APIs.
    /// </summary>
    public static class Conversions
    {
        /// <summary>
        /// Converts an organization detail into an entity.
        /// </summary>
        /// <param name="details">The organization detail returned by Dataverse.</param>
        /// <returns>
        /// An entity holding the organization properties. Endpoints are returned as a nested entity
        /// keyed by endpoint type name.
        /// </returns>
        public static Entity ToEntity(OrganizationDetail details)
        {
            if (details == null)
            {
                throw new ArgumentNullException(nameof(details));
            }

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

        /// <summary>
        /// Converts an attribute value to its string representation.
        /// </summary>
        /// <param name="value">
        /// The value to convert. Option sets return their numeric value, entity references their id,
        /// money its amount and dates an ISO 8601 string.
        /// </param>
        /// <returns>The string representation of the value, or <c>null</c> when the value is <c>null</c>.</returns>
        public static string ToStringValue(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case string text:
                    return text;
                case OptionSetValue optionSet:
                    return optionSet.Value.ToString(CultureInfo.InvariantCulture);
                case EntityReference reference:
                    return reference.Id.ToString();
                case Money money:
                    return money.Value.ToString(CultureInfo.InvariantCulture);
                case DateTime date:
                    return date.ToString("o", CultureInfo.InvariantCulture);
                case IFormattable formattable:
                    return formattable.ToString(null, CultureInfo.InvariantCulture);
                default:
                    return value.ToString();
            }
        }
    }
}
