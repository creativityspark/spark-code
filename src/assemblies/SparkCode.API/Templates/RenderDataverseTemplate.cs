using Fluid;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace SparkCode.API.Templates
{
    /// <displayName>Render Dataverse Template</displayName>
    /// <summary>
    /// Renders a Liquid template by sourcing context values from an optional Dataverse record
    /// and optional additional context.
    /// </summary>
    /// <param name="Template" type="string">Liquid template text to render.</param>
    /// <param name="RecordId" type="string" optional="true">Optional GUID of the Dataverse record to use as context. Must be provided together with RecordType.</param>
    /// <param name="RecordType" type="string" optional="true">Optional logical name of the table the record belongs to. Must be provided together with RecordId.</param>
    /// <param name="AdditionalContext" type="string" optional="true">Optional JSON object merged into the template model before rendering.</param>
    /// <param name="Results" type="string" direction="output">Rendered template text.</param>
    /// <example>
    /// To render a greeting using a contact record, set Template to "Hello {{ firstname }} {{ lastname }}!",
    /// RecordId to the contact's GUID, and RecordType to "contact".
    /// The Results output parameter will return "Hello Jane Doe!".
    /// </example>
    public class RenderDataverseTemplate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string templateSource = ctx.GetInputParameter<string>("Template", true);
            string recordIdStr = ctx.GetInputParameter<string>("RecordId", false);
            string recordType = ctx.GetInputParameter<string>("RecordType", false);
            string additionalContext = ctx.PluginContext.InputParameters.Contains("AdditionalContext")
                ? ctx.PluginContext.InputParameters["AdditionalContext"] as string
                : null;

            var hasRecordId = !string.IsNullOrWhiteSpace(recordIdStr);
            var hasRecordType = !string.IsNullOrWhiteSpace(recordType);

            if (hasRecordId != hasRecordType)
            {
                throw new Exception("RecordId and RecordType must both be provided together or both omitted.");
            }

            // Run Logic
            var parser = new FluidParser();
            SparkCode.Templates.TemplateRenderer.RegisterCustomTags(parser, ctx.Service);
            var parsedTemplate = SparkCode.Templates.TemplateRenderer.Parse(templateSource, parser);
            var visitor = new SparkCode.Templates.IdentifierVisitor();
            visitor.VisitTemplate(parsedTemplate);
            var identifiers = visitor.Identifiers.ToArray();

            var model = SparkCode.Templates.TemplateRenderer.BuildModel(
                ctx.Service,
                recordType,
                recordIdStr,
                additionalContext,
                identifiers
            );

            string renderedTemplate = SparkCode.Templates.TemplateRenderer.Render(parsedTemplate, model);

            // API Outputs
            ctx.SetOutputParameter("Results", renderedTemplate);
        }
    }
}
