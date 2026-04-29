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
            var template = ParseTemplate(templateSource, parser);
            return Render(template, model);
        }

        /// <summary>
        /// Parses Liquid template source into a compiled Fluid template instance.
        /// </summary>
        /// <param name="templateSource">The Liquid template source text.</param>
        /// <param name="parser">The Fluid parser instance used to parse the template.</param>
        /// <returns>The parsed <see cref="IFluidTemplate"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parser"/> is null.</exception>
        /// <exception cref="InvalidPluginExecutionException">
        /// Thrown when the provided template source is invalid Liquid syntax.
        /// </exception>
        public static IFluidTemplate ParseTemplate(string templateSource, FluidParser parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (parser.TryParse(templateSource, out IFluidTemplate template, out string errorMessage))
            {
                return template;
            }

            throw new InvalidPluginExecutionException($"Invalid Liquid template: {errorMessage}");
        }


        /// <summary>
        /// Registers custom Fluid identifier tags supported by SparkCode templates.
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
        /// Builds a template model by retrieving a Dataverse record and merging additional context values.
        /// </summary>
        /// <param name="service">The Dataverse organization service.</param>
        /// <param name="recordType">The logical name of the target Dataverse table.</param>
        /// <param name="recordIdStr">The identifier of the record to retrieve.</param>
        /// <param name="additionalContext">Optional JSON object merged into the returned model.</param>
        /// <param name="identifiers">Identifiers referenced in the template used to limit retrieved columns.</param>
        /// <returns>An <see cref="ExpandoObject"/> containing Dataverse and additional context values.</returns>
        public static ExpandoObject BuildDataverseModel(IOrganizationService service, string recordType, string recordIdStr, string additionalContext, string[] identifiers)
        {
            var additionalValuesDictionary = string.IsNullOrWhiteSpace(additionalContext)
                ? new Dictionary<string, object>()
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(additionalContext) ?? new Dictionary<string, object>();

            // ensure we don't try to retrieve columns that are provided in the additional context
            var filteredIdentifiers = new HashSet<string>(
                identifiers.Where(id => !additionalValuesDictionary.ContainsKey(id))
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
            var model = JsonConvert.DeserializeObject<ExpandoObject>(record.ToJson());
            var modelDictionary = (IDictionary<string, object>)model;

            foreach (var kvp in additionalValuesDictionary)
            {
                modelDictionary[kvp.Key] = kvp.Value;
            }

            return model;
        }
    }
}