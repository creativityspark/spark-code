using Fluid;
using Fluid.Ast;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace SparkCode.Templates
{
    /// <summary>
    /// Provides helpers to parse and render Liquid templates and build
    /// a template model from Dataverse records plus additional context values.
    /// </summary>
    public static class TemplateRenderer
    {
        /// <summary>
        /// Renders a Liquid template using a JSON object as the model.
        /// </summary>
        /// <param name="templateSource">The Liquid template source text.</param>
        /// <param name="jsonValues">A JSON object string used as the template model.</param>
        /// <returns>The rendered template output.</returns>
        public static string Render(string templateSource, string jsonValues)
        {
            var model = JsonConvert.DeserializeObject<ExpandoObject>(jsonValues);
            return Render(templateSource, model);
        }

        /// <summary>
        /// Renders a Liquid template using an <see cref="ExpandoObject"/> model.
        /// </summary>
        /// <param name="templateSource">The Liquid template source text.</param>
        /// <param name="model">The model passed to the template context.</param>
        /// <returns>The rendered template output.</returns>
        public static string Render(string templateSource, ExpandoObject model)
        {
            var parser = new FluidParser();
            var template = Parse(templateSource, parser);
            return Render(template, model);
        }

        /// <summary>
        /// Renders a parsed Fluid template using an <see cref="ExpandoObject"/> model.
        /// </summary>
        /// <param name="template">The parsed template to render.</param>
        /// <param name="model">The model passed to the template context.</param>
        /// <returns>The rendered template output.</returns>
        public static string Render(IFluidTemplate template, ExpandoObject model)
        {
            var templateContext = new TemplateContext(model);
            return template.Render(templateContext);
        }

        /// <summary>
        /// Renders a Liquid template using Dataverse context inputs from the Render Dataverse Template custom API.
        /// </summary>
        /// <param name="service">The Dataverse organization service used to resolve records and custom tags.</param>
        /// <param name="templateSource">Liquid template text to render.</param>
        /// <param name="recordIdStr">
        /// Optional GUID of the Dataverse record to use as context. Must be provided together with <paramref name="recordType"/>.
        /// </param>
        /// <param name="recordType">
        /// Optional logical name of the Dataverse table for <paramref name="recordIdStr"/>. Must be provided together with <paramref name="recordIdStr"/>.
        /// </param>
        /// <param name="additionalContext">Optional JSON object merged into the template model before rendering.</param>
        /// <returns>The rendered template output.</returns>
        /// <exception cref="Exception">
        /// Thrown when template syntax is invalid, when record inputs are incomplete, or when record retrieval fails.
        /// </exception>
        public static string Render(
            IOrganizationService service,
            string templateSource,
            string recordIdStr = null,
            string recordType = null,
            string additionalContext = null)
        {
            var parser = new FluidParser();
            RegisterCustomTags(parser, service);

            var parsedTemplate = Parse(templateSource, parser);
            var visitor = new IdentifierVisitor();
            visitor.VisitTemplate(parsedTemplate);
            var identifiers = visitor.Identifiers.ToArray();

            var model = BuildModel(
                service,
                recordType,
                recordIdStr,
                additionalContext,
                identifiers);

            return Render(parsedTemplate, model);
        }

        /// <summary>
        /// Retrieves a Liquid template from a Dataverse web resource, parses its front matter and body,
        /// and renders the body using optional Dataverse record and additional context values.
        /// </summary>
        /// <param name="service">The Dataverse organization service used to retrieve the web resource content.</param>
        /// <param name="webResourceName">Unique name of the web resource containing the template text.</param>
        /// <param name="recordIdStr">
        /// Optional GUID of the Dataverse record to use as context. Must be provided together with <paramref name="recordType"/>.
        /// </param>
        /// <param name="recordType">
        /// Optional logical name of the Dataverse table for <paramref name="recordIdStr"/>. Must be provided together with <paramref name="recordIdStr"/>.
        /// </param>
        /// <param name="additionalContext">Optional JSON object merged into the template model before rendering.</param>
        /// <returns>
        /// An <see cref="Entity"/> containing <c>frontMatter</c>, <c>renderedFrontMatter</c>, <c>body</c>,
        /// and <c>renderedTemplate</c> attributes.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        /// <exception cref="Exception">
        /// Thrown when the web resource cannot be retrieved, template syntax is invalid,
        /// or record inputs are incomplete.
        /// </exception>
        public static Entity Parse(
            IOrganizationService service,
            string webResourceName,
            string recordIdStr = null,
            string recordType = null,
            string additionalContext = null)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var templateSource = service.GetWebResourceContent(webResourceName);

            var parsedTemplate = GetFrontMatter.Parse(templateSource);
            var frontMatter = parsedTemplate.Contains("frontMatter")
                ? parsedTemplate["frontMatter"] as Entity
                : null;
            var renderedFrontMatter = RenderFrontMatter(
                service,
                frontMatter,
                recordIdStr,
                recordType,
                additionalContext);
            
            var templateBody = (string)parsedTemplate["body"];

            var renderedTemplate = Render(service, templateBody, recordIdStr, recordType, additionalContext);

            parsedTemplate["renderedFrontMatter"] = renderedFrontMatter;
            parsedTemplate["renderedTemplate"] = renderedTemplate;
            return parsedTemplate;
        }

        private static Entity RenderFrontMatter(
            IOrganizationService service,
            Entity frontMatter,
            string recordIdStr,
            string recordType,
            string additionalContext)
        {
            if (frontMatter == null)
            {
                throw new ArgumentNullException(nameof(frontMatter));
            }

            var renderedFrontMatter = new Entity();

            foreach (var attribute in frontMatter.Attributes)
            {
                if (attribute.Value is string frontMatterValue)
                {
                    renderedFrontMatter[attribute.Key] = Render(
                        service,
                        frontMatterValue,
                        recordIdStr,
                        recordType,
                        additionalContext);
                    continue;
                }

                renderedFrontMatter[attribute.Key] = attribute.Value;
            }

            return renderedFrontMatter;
        }

        /// <summary>
        /// Parses Liquid template source into a compiled Fluid template instance.
        /// </summary>
        /// <param name="templateSource">The Liquid template source text.</param>
        /// <param name="parser">The Fluid parser instance used to parse the template.</param>
        /// <returns>The parsed <see cref="IFluidTemplate"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parser"/> is null.</exception>
        /// <exception cref="Exception">
        /// Thrown when the provided template source is invalid Liquid syntax.
        /// </exception>
        public static IFluidTemplate Parse(string templateSource, FluidParser parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (parser.TryParse(templateSource, out IFluidTemplate template, out string errorMessage))
            {
                return template;
            }

            throw new Exception($"Invalid Liquid template: {errorMessage}");
        }


        /// <summary>
        /// Registers custom Fluid tags supported by SparkCode templates.
        /// Supported tags: <c>_envVar</c>, <c>_orgDetails</c>, <c>_orgUrl</c>, <c>_appUrl</c>, <c>_appName</c>, and <c>_webResource</c>.
        /// </summary>
        /// <param name="parser">The Fluid parser where custom tags are registered.</param>
        /// <param name="service">The Dataverse organization service used by tag resolvers.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parser"/> or <paramref name="service"/> is null.
        /// </exception>
        public static void RegisterCustomTags(FluidParser parser, IOrganizationService service)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            parser.RegisterIdentifierTag("_envVar", (identifier, writer, encoder, ctx) =>
            {
                string envVarValue = service.GetEnvironmentVariableValue(identifier);
                writer.Write(envVarValue);
                return Statement.Normal();
            });

            parser.RegisterIdentifierTag("_orgDetails", (identifier, writer, encoder, ctx) =>
            {
                string organizationDetail = service.GetOrganizationDetails(identifier);
                writer.Write(organizationDetail);
                return Statement.Normal();
            });

            parser.RegisterEmptyTag("_orgUrl", async (writer, encoder, ctx) =>
            {
                string organizationUrl = service.GetOrganizationUrl(null);
                writer.Write(organizationUrl);
                return Completion.Normal;
            });

            parser.RegisterIdentifierTag("_appUrl", (identifier, writer, encoder, ctx) =>
            {
                var appUrl = service.GetCanvasAppAttribute<string>(identifier, "appopenuri");

                if (string.IsNullOrWhiteSpace(appUrl))
                {
                    var appId = service.GetMDAAttribute<Guid>(identifier, "appmoduleid");
                    var appIdString = appId != Guid.Empty ? appId.ToString() : string.Empty;
                    var environmentUrl = service.GetOrganizationUrl(null)?.TrimEnd('/');
                    appUrl = $"{environmentUrl}/main.aspx?appid={appId}";
                }

                if (!string.IsNullOrWhiteSpace(appUrl))
                {

                    writer.Write(appUrl);
                }

                return Statement.Normal();
            });

            parser.RegisterIdentifierTag("_appName", (identifier, writer, encoder, ctx) =>
            {
                var appName = service.GetCanvasAppAttribute<string>(identifier, "name");

                if (string.IsNullOrWhiteSpace(appName))
                {
                    appName = service.GetMDAAttribute<string>(identifier, "displayname");
                }

                if (!string.IsNullOrWhiteSpace(appName))
                {
                    writer.Write(appName);
                }

                return Statement.Normal();
            });

            parser.RegisterIdentifierTag("_webResource", (identifier, writer, encoder, ctx) =>
            {
                var webResourceContent = service.GetWebResourceContent(identifier, decode: false);

                if (!string.IsNullOrWhiteSpace(webResourceContent))
                {
                    writer.Write(webResourceContent);
                }

                return Statement.Normal();
            });
        }


        /// <summary>
        /// Builds a template model by retrieving a Dataverse record and merging additional context values.
        /// </summary>
        /// <param name="service">The Dataverse organization service.</param>
        /// <param name="recordType">
        /// Optional logical name of the target Dataverse table. Must be provided together with <paramref name="recordIdStr"/>.
        /// </param>
        /// <param name="recordIdStr">
        /// Optional identifier of the record to retrieve. Must be provided together with <paramref name="recordType"/>.
        /// </param>
        /// <param name="additionalContext">Optional JSON object merged into the returned model.</param>
        /// <param name="identifiers">Optional identifiers referenced in the template used to limit retrieved columns.</param>
        /// <returns>An <see cref="ExpandoObject"/> containing Dataverse and additional context values.</returns>
        /// <exception cref="Exception">
        /// Thrown when only one of <paramref name="recordType"/> or <paramref name="recordIdStr"/> is provided.
        /// </exception>
        public static ExpandoObject BuildModel(
            IOrganizationService service,
            string recordType = null,
            string recordIdStr = null,
            string additionalContext = null,
            string[] identifiers = null)
        {
            var additionalValuesDictionary = string.IsNullOrWhiteSpace(additionalContext)
                ? new Dictionary<string, object>()
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(additionalContext) ?? new Dictionary<string, object>();

            var hasRecordType = !string.IsNullOrWhiteSpace(recordType);
            var hasRecordId = !string.IsNullOrWhiteSpace(recordIdStr);

            if (hasRecordType != hasRecordId)
            {
                throw new Exception("recordType and recordIdStr must both be provided together or both omitted.");
            }

            IDictionary<string, object> modelDictionary;

            if (!hasRecordType)
            {
                modelDictionary = new ExpandoObject();
            }
            else
            {
                var safeIdentifiers = identifiers ?? Array.Empty<string>();

                // ensure we don't try to retrieve columns that are provided in the additional context
                var filteredIdentifiers = new HashSet<string>(
                    safeIdentifiers.Where(id => !additionalValuesDictionary.ContainsKey(id))
                );
                var filteredIdentifiersArray = filteredIdentifiers.ToArray();

                // Apply an additional filter to ensure we only retrieve columns that are part of the entity
                var entityColumns = ServiceExtensions.GetTableColumnNames(service, recordType);
                filteredIdentifiersArray = filteredIdentifiersArray.Where(id => entityColumns.Contains(id)).ToArray();

                var recordId = new Guid(recordIdStr);
                var columnSet = filteredIdentifiersArray.Length > 0
                    ? new ColumnSet(filteredIdentifiersArray)
                    : new ColumnSet(false);

                var record = service.Retrieve(recordType, recordId, columnSet);
                var model = JsonConvert.DeserializeObject<ExpandoObject>(record.ToJson()) ?? new ExpandoObject();
                modelDictionary = model;
            }

            foreach (var kvp in additionalValuesDictionary)
            {
                modelDictionary[kvp.Key] = kvp.Value;
            }

            return (ExpandoObject)modelDictionary;
        }
    }
}