using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
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

    /// <summary>
    /// Retrieves an environment variable value by schema name.
    /// </summary>
    /// <param name="service">Dataverse organization service instance.</param>
    /// <param name="name">Schema name of the environment variable definition.</param>
    /// <returns>
    /// The current environment variable value when present; otherwise the definition default value.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when the environment variable definition is not found, or when both current and default values are empty.
    /// </exception>
        public static string GetEnvironmentVariableValue(this IOrganizationService service, string name)
        {
            // Create a query to fetch the environment variable definition based on the identifier (schemaname)
            var definitionQuery = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname", "defaultvalue"),
                Criteria =
                {
                    Conditions =
                    {
                        // Filter the records to find the environment variable definition with the matching schema name (identifier)
                        new ConditionExpression("schemaname", ConditionOperator.Equal, name)
                    }
                }
            };

            // Add a link to the 'environmentvariablevalue' entity using a left outer join on the environmentvariabledefinitionid field
            var link = definitionQuery.AddLink(
                "environmentvariablevalue", // The linked entity (environmentvariablevalue)
                "environmentvariabledefinitionid", // The field in the definition entity to join on
                "environmentvariabledefinitionid", // The field in the linked entity to join on
                JoinOperator.LeftOuter
            );
            link.Columns = new ColumnSet("value");
            link.EntityAlias = "envVarValue";

            // Execute the query to retrieve matching environment variable definitions.
            var result = service.RetrieveMultiple(definitionQuery);

            if (result.Entities.Count == 0)
            {
                throw new Exception($"Environment variable definition with identifier '{name}' not found.");
            }

            var definition = result.Entities[0];

            // Retrieve the 'value' field from the linked entity using the alias ('envVarValue').
            var value = definition.GetAttributeValue<AliasedValue>("envVarValue.value")?.Value as string;

            // Retrieve the default value (if any) from the environment variable definition.
            var defaultValue = definition.GetAttributeValue<string>("defaultvalue");

            // If no specific value is found, check if there is a default value available.
            if (string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    return defaultValue;
                }
                // If neither a value nor a default value exists, throw an exception.
                throw new Exception($"Environment variable value for identifier '{name}' not found.");
            }

            // Return the value
            return value;
        }

        /// <summary>
        /// Retrieves a single organization detail value by key.
        /// </summary>
        /// <param name="service">Dataverse organization service instance.</param>
        /// <param name="detailName">The organization detail name (for example, <c>FriendlyName</c> or <c>UniqueName</c>).</param>
        /// <returns>The organization detail value as a string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="detailName"/> is null or whitespace.</exception>
        /// <exception cref="Exception">Thrown when the requested detail does not exist.</exception>
        public static string GetOrganizationDetails(this IOrganizationService service, string detailName)
        {
            if (string.IsNullOrWhiteSpace(detailName))
            {
                throw new ArgumentNullException(nameof(detailName));
            }

            var details = service.GetOrganizationDetails();

            var jsonDetails = JsonConvert.SerializeObject(details);
            var detailsDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonDetails);


            var detail = detailsDictionary.FirstOrDefault(kvp => string.Equals(kvp.Key, detailName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(detail.Key) || detail.Value == null)
            {
                throw new Exception($"Organization detail '{detailName}' not found.");
            }

            return detail.Value.ToString();
        }


        /// <summary>
        /// Retrieves an organization endpoint URL by endpoint type name.
        /// </summary>
        /// <param name="service">Dataverse organization service instance.</param>
        /// <param name="urlType">
        /// Endpoint type name from <see cref="Microsoft.Xrm.Sdk.Organization.EndpointType"/>.
        /// When null or whitespace, <c>WebApplication</c> is used.
        /// </param>
        /// <returns>The endpoint URL for the requested endpoint type.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="urlType"/> is not a valid endpoint type name.</exception>
        public static string GetOrganizationUrl(this IOrganizationService service, string urlType)
        {
            if (string.IsNullOrWhiteSpace(urlType))
            {
                urlType = "WebApplication";
            }

            if (!Enum.TryParse<Microsoft.Xrm.Sdk.Organization.EndpointType>(urlType, true, out var endpointKey)) 
            { 
                throw new ArgumentException($"Invalid endpoint name '{urlType}'. Valid values are: {string.Join(", ", Enum.GetNames(typeof(Microsoft.Xrm.Sdk.Organization.EndpointType)))}", nameof(urlType));
            }

            var details = service.GetOrganizationDetails();

            return details.Endpoints[endpointKey];
        }

        /// <summary>
        /// Retrieves all current organization details as a key-value dictionary.
        /// </summary>
        /// <param name="service">Dataverse organization service instance.</param>
        /// <returns>A dictionary containing organization detail keys and values.</returns>
        /// <exception cref="Exception">Thrown when organization details cannot be retrieved from Dataverse.</exception>
        private static Microsoft.Xrm.Sdk.Organization.OrganizationDetail GetOrganizationDetails(this IOrganizationService service)
        {
            var request = new RetrieveCurrentOrganizationRequest();
            var response = (RetrieveCurrentOrganizationResponse)service.Execute(request);

            if (response?.Detail == null)
            {
                throw new Exception("Unable to retrieve organization details.");
            }

            return response.Detail;
        }

    }
}
