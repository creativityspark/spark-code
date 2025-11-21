using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Microsoft.Xrm.Sdk.Metadata;
using System.Linq;
using System.Xml.Linq;

namespace SparkCode.CustomAPIs
{
    /// <summary>
    /// Extensions for the IOrganizationService to work with Dataverse metadata and data retrieval.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Retrieves the logical name of an entity given its Object Type Code (ETC).
        /// </summary>
        /// <param name="service">Service Reference used to connect to Dataverse</param>
        /// <param name="etc">Entity Type Code wich table must be retrieved</param>
        /// <returns>The logical name of the table</returns>
        public static string GetTableLogicalName(this IOrganizationService service, int etc)
        {
            var logicalName = string.Empty;
            var entityFilter = new MetadataFilterExpression(LogicalOperator.And);
            entityFilter.Conditions.Add(new MetadataConditionExpression("ObjectTypeCode", MetadataConditionOperator.Equals, etc));

            var propertyExpression = new MetadataPropertiesExpression { AllProperties = false };
            propertyExpression.PropertyNames.Add("LogicalName");

            var entityQueryExpression = new EntityQueryExpression()
            {
                Criteria = entityFilter,
                Properties = propertyExpression
            };

            var retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest()
            {
                Query = entityQueryExpression
            };

            var response = (RetrieveMetadataChangesResponse)service.Execute(retrieveMetadataChangesRequest);

            if (response.EntityMetadata.Count == 1)
            {
                logicalName = response.EntityMetadata[0].LogicalName;
            }

            return logicalName;
        }

        /// <summary>
        /// Retrieves the Entity Type Code (ETC) for a given entity logical name.
        /// </summary>
        /// <param name="service">Service Reference used to connect to Dataverse</param>
        /// <param name="entityLogicalName">Logical name of the entity whose ETC is to be retrieved.</param>
        /// <returns></returns>
        private static int GetEntityTypeCode(this IOrganizationService service, string entityLogicalName)
        {
            // Use RetrieveEntityRequest to get metadata
            var request = new RetrieveEntityRequest
            {
                LogicalName = entityLogicalName,
                EntityFilters = EntityFilters.Entity
            };

            var response = (RetrieveEntityResponse)service.Execute(request);
            return response.EntityMetadata.ObjectTypeCode.Value;
        }

        /// <summary>
        /// Retrieves data from a specified view and returns it in JSON format.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="viewId"></param>
        /// <param name="tableName"></param>
        /// <param name="viewName"></param>
        /// <param name="friendlyNames"></param>
        /// <returns></returns>
        public static string GetViewData(this IOrganizationService service, Guid? viewId, string tableName, string viewName, bool friendlyNames)
        {
            var etc = GetEntityTypeCode(service, tableName);

            // Retrieve the view from SavedQuery
            Entity savedQuery = GetSavedQuery(service, viewId, etc, viewName);

            // Execute the FetchXML query from the view
            string fetchXml = savedQuery.GetAttributeValue<string>("fetchxml");
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            // Get column metadata if friendly names are requested
            Dictionary<string, string> columnDisplayNames = friendlyNames
                ? GetQueryDisplayNames(service, fetchXml, tableName)
                : null;

            // Convert results to a list of dictionaries
            var jsonResults = new List<Dictionary<string, object>>();
            foreach (var entity in results.Entities)
            {
                var record = new Dictionary<string, object>();
                foreach (var attribute in entity.Attributes)
                {
                    string key = attribute.Key;
                    // If friendly names are requested, use the display name
                    if (friendlyNames && columnDisplayNames != null && columnDisplayNames.ContainsKey(key))
                    {
                        key = columnDisplayNames[key];
                    }
                    record[key] = FormatAttributeValue(attribute.Value);
                }
                jsonResults.Add(record);
            }

            // Serialize to JSON
            return JsonConvert.SerializeObject(jsonResults, Formatting.Indented);
        }

        /// <summary>
        /// Retrieves a SavedQuery (view) based on view ID, or table and view name.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="viewId"></param>
        /// <param name="objectTypeCode"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidPluginExecutionException"></exception>
        public static Entity GetSavedQuery(this IOrganizationService service, Guid? viewId, int objectTypeCode, string viewName)
        {
            // If ViewId is provided, retrieve the view directly by ID
            if (viewId.HasValue && viewId.Value != Guid.Empty)
            {
                return service.Retrieve("savedquery", viewId.Value, new ColumnSet(true));
            }

            // Retrieve the specific view by name and objectTypeCode
            var query = new QueryExpression("savedquery")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("returnedtypecode", ConditionOperator.Equal, objectTypeCode),
                        new ConditionExpression("name", ConditionOperator.Equal, viewName)
                    }
                }
            };

            var views = service.RetrieveMultiple(query);

            if (views.Entities.Count == 0)
            {
                throw new InvalidPluginExecutionException($"View '{viewName}' not found for etc '{objectTypeCode}'.");
            }

            return views.Entities[0]; // Return the first matching view
        }

        /// <summary>
        /// Retrieves a mapping of attribute logical names to their display names based on the FetchXML query.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="fetchXmlQuery"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetQueryDisplayNames(this IOrganizationService service, string fetchXmlQuery, string tableName)
        {
            var displayNames = new Dictionary<string, string>();

            // Parse FetchXML to get attributes
            XDocument fetchDoc = XDocument.Parse(fetchXmlQuery);
            var attributes = fetchDoc.Descendants("attribute")
                .Select(attr => attr.Attribute("name")?.Value)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            // Retrieve metadata for the entity
            var metadataRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = tableName
            };
            var metadataResponse = (RetrieveEntityResponse)service.Execute(metadataRequest);
            var attributeMetadata = metadataResponse.EntityMetadata.Attributes;

            // Map logical names to display names
            foreach (var attrName in attributes)
            {
                var attribute = attributeMetadata.FirstOrDefault(a => a.LogicalName == attrName);
                if (attribute != null && attribute.DisplayName?.UserLocalizedLabel != null &&
                    !string.IsNullOrEmpty(attribute.DisplayName.UserLocalizedLabel.Label))
                {
                    displayNames[attrName] = attribute.DisplayName.UserLocalizedLabel.Label;
                }
                else
                {
                    displayNames[attrName] = attrName; // Fallback to logical name
                }
            }

            return displayNames;
        }

        // This method formats attribute values based on their type
        public static object FormatAttributeValue(object value)
        {
            if (value is EntityReference er)
                return er.Name ?? er.Id.ToString();
            if (value is OptionSetValue osv)
                return osv.Value;
            if (value is Money money)
                return money.Value;
            if (value is AliasedValue av)
                return FormatAttributeValue(av.Value);
            return value;
        }

    }
}
