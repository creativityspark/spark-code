using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SparkCode
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
        public static int GetEntityTypeCode(this IOrganizationService service, string entityLogicalName)
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
        public static EntityCollection GetViewData(this IOrganizationService service, Guid? viewId, string tableName, string viewName)
        {
            var etc = 0;
            if (viewName != null)
            {
                 etc = GetEntityTypeCode(service, tableName);
            }

            // Retrieve the view from SavedQuery
            Entity savedQuery = GetSavedQuery(service, viewId, etc, viewName);

            // Execute the FetchXML query from the view
            string fetchXml = savedQuery.GetAttributeValue<string>("fetchxml");
            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            return results;
        }

        /// <summary>
        /// Retrieves a SavedQuery (view) based on view ID, or table and view name.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="viewId"></param>
        /// <param name="objectTypeCode"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public static Entity GetSavedQuery(this IOrganizationService service, Guid? viewId, int? objectTypeCode, string viewName)
        {
            Entity result;

            if (viewId.HasValue && viewId.Value != Guid.Empty)
            {
                result = service.Retrieve("savedquery", viewId.Value, new ColumnSet(true));
            }
            else
            {
                if (!objectTypeCode.HasValue)
                {
                    throw new ArgumentNullException(nameof(objectTypeCode), "objectTypeCode is required when viewId is not provided.");
                }

                if (string.IsNullOrWhiteSpace(viewName))
                {
                    throw new ArgumentNullException(nameof(viewName), "viewName is required when viewId is not provided.");
                }

                // Retrieve the specific view by name and objectTypeCode
                var query = new QueryExpression("savedquery")
                {
                    ColumnSet = new ColumnSet(true),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("returnedtypecode", ConditionOperator.Equal, objectTypeCode.Value),
                            new ConditionExpression("name", ConditionOperator.Equal, viewName)
                        }
                    }
                };

                var views = service.RetrieveMultiple(query);

                if (views.Entities.Count == 0)
                {
                    throw new Exception($"View '{viewName}' not found for etc '{objectTypeCode.Value}'.");
                }

                result = views.Entities[0];
            }

            return result;
        }

        /// <summary>
        /// Retrieves query column friendly names where each attribute logical name is a key and
        /// its localized label is the value.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="fetchXmlQuery"></param>
        /// <returns></returns>
        public static Entity GetFriendlyNames(this IOrganizationService service, string fetchXmlQuery)
        {
            var friendlyNames = new Entity();

            // Parse FetchXML to get attributes
            XDocument fetchDoc = XDocument.Parse(fetchXmlQuery);
            var tableName = fetchDoc.Descendants("entity").FirstOrDefault()?.Attribute("name")?.Value;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(fetchXmlQuery), "FetchXML must include an entity name.");
            }

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

            // Map logical names to localized labels
            foreach (var attrName in attributes)
            {
                var attribute = attributeMetadata.FirstOrDefault(a => a.LogicalName == attrName);
                if (attribute != null && attribute.DisplayName?.UserLocalizedLabel != null &&
                    !string.IsNullOrEmpty(attribute.DisplayName.UserLocalizedLabel.Label))
                {
                    friendlyNames[attrName] = attribute.DisplayName.UserLocalizedLabel.Label;
                }
                else
                {
                    friendlyNames[attrName] = attrName; // Fallback to logical name
                }
            }

            return friendlyNames;
        }

        /// <summary>
        /// Retrieves friendly names for all attributes present in an entity object,
        /// where each attribute logical name is a key and its localized label is the value.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entityObject"></param>
        /// <returns></returns>
        public static Entity GetFriendlyNames(this IOrganizationService service, Entity entityObject)
        {
            if (entityObject == null)
            {
                throw new ArgumentNullException(nameof(entityObject));
            }

            if (string.IsNullOrWhiteSpace(entityObject.LogicalName))
            {
                throw new ArgumentNullException(nameof(entityObject), "Entity must include a logical name.");
            }

            var friendlyNames = new Entity();
            var attributes = entityObject.Attributes.Keys.ToList();

            // Retrieve metadata for the entity
            var metadataRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = entityObject.LogicalName
            };
            var metadataResponse = (RetrieveEntityResponse)service.Execute(metadataRequest);
            var attributeMetadata = metadataResponse.EntityMetadata.Attributes;

            // Map logical names to localized labels
            foreach (var attrName in attributes)
            {
                var attribute = attributeMetadata.FirstOrDefault(a => a.LogicalName == attrName);
                if (attribute != null && attribute.DisplayName?.UserLocalizedLabel != null &&
                    !string.IsNullOrEmpty(attribute.DisplayName.UserLocalizedLabel.Label))
                {
                    friendlyNames[attrName] = attribute.DisplayName.UserLocalizedLabel.Label;
                }
                else
                {
                    friendlyNames[attrName] = attrName; // Fallback to logical name
                }
            }

            return friendlyNames;
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
