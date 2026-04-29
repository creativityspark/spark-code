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
    /// Extension methods for <see cref="IOrganizationService"/> to retrieve Dataverse metadata,
    /// resolve saved views, and normalize query output values.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Retrieves the logical name of an entity by its object type code (ETC).
        /// </summary>
        /// <param name="service">Dataverse organization service instance.</param>
        /// <param name="etc">Object type code of the target table.</param>
        /// <returns>
        /// The table logical name when exactly one entity metadata record matches;
        /// otherwise an empty string.
        /// </returns>
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
        /// Retrieves the object type code (ETC) for a given table logical name.
        /// </summary>
        /// <param name="service">Dataverse organization service instance.</param>
        /// <param name="entityLogicalName">Logical name of the target table (for example, <c>account</c>).</param>
        /// <returns>The object type code for the specified table.</returns>
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
        /// Retrieves all logical column names for a Dataverse table.
        /// </summary>
        /// <param name="service">Dataverse organization service instance.</param>
        /// <param name="tableLogicalName">Logical name of the target table (for example, <c>account</c>).</param>
        /// <returns>A case-insensitive set of logical column names for the table.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableLogicalName"/> is null or whitespace.</exception>
        public static HashSet<string> GetTableColumnNames(this IOrganizationService service, string tableLogicalName)
        {
            if (string.IsNullOrWhiteSpace(tableLogicalName))
            {
                throw new ArgumentNullException(nameof(tableLogicalName));
            }

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableLogicalName,
                EntityFilters = EntityFilters.Attributes
            };

            var response = (RetrieveEntityResponse)service.Execute(request);
            return new HashSet<string>(
                response.EntityMetadata.Attributes
                    .Select(a => a.LogicalName)
                    .Where(name => !string.IsNullOrWhiteSpace(name)),
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Retrieves records returned by a Dataverse saved view.
        /// </summary>
        /// <param name="service">Dataverse organization service instance.</param>
        /// <param name="viewId">Saved query identifier. When provided, this value takes precedence over <paramref name="viewName"/>.</param>
        /// <param name="tableName">Logical table name used to resolve ETC when <paramref name="viewName"/> is used.</param>
        /// <param name="viewName">Saved query name to resolve when <paramref name="viewId"/> is not provided.</param>
        /// <returns>The collection of records returned by the view FetchXML.</returns>
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
        /// Retrieves a saved query (view) either by identifier or by ETC and name.
        /// </summary>
        /// <param name="service">Dataverse organization service instance.</param>
        /// <param name="viewId">Saved query identifier. When provided, this value is used directly.</param>
        /// <param name="objectTypeCode">Table object type code required when <paramref name="viewId"/> is not provided.</param>
        /// <param name="viewName">Saved query name required when <paramref name="viewId"/> is not provided.</param>
        /// <returns>The matching saved query record.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="objectTypeCode"/> or <paramref name="viewName"/> is missing and <paramref name="viewId"/> is not provided.
        /// </exception>
        /// <exception cref="Exception">Thrown when no saved query matches the provided ETC and view name.</exception>
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
        /// Builds a friendly-name map for attributes in a FetchXML query,
        /// where each attribute logical name is a key and the localized label is the value.
        /// </summary>
        /// <param name="service">Dataverse organization service instance.</param>
        /// <param name="fetchXmlQuery">FetchXML containing one entity and its requested attributes.</param>
        /// <returns>An <see cref="Entity"/> containing logical-name-to-friendly-name pairs.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the FetchXML does not include an entity name.</exception>
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
        /// Builds a friendly-name map for attributes present in an entity object,
        /// where each attribute logical name is a key and the localized label is the value.
        /// </summary>
        /// <param name="service">Dataverse organization service instance.</param>
        /// <param name="entityObject">Entity instance whose attribute keys are resolved to localized labels.</param>
        /// <returns>An <see cref="Entity"/> containing logical-name-to-friendly-name pairs.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entityObject"/> is null or has no logical name.</exception>
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
    }
}
