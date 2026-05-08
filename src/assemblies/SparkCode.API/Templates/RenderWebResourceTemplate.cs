using Fluid;
using Microsoft.Xrm.Sdk;
using SparkCode;
using System;
using System.Linq;

namespace SparkCode.API.Templates
{
    /// <displayName>Render Web Resource Template</displayName>
    /// <summary>
    /// Renders a Liquid template stored in a Dataverse web resource by sourcing context values
    /// from an optional Dataverse record and optional additional context.
    /// </summary>
    /// <param name="WebResourceName" type="string">Unique name of the web resource containing the template text.</param>
    /// <param name="RecordId" type="string" optional="true">Optional GUID of the Dataverse record to use as context. Must be provided together with RecordType.</param>
    /// <param name="RecordType" type="string" optional="true">Optional logical name of the table the record belongs to. Must be provided together with RecordId.</param>
    /// <param name="AdditionalContext" type="string" optional="true">Optional JSON object merged into the template model before rendering.</param>
    /// <param name="Results" type="string" direction="output">Rendered template text.</param>
    /// <param name="FrontMatter" type="expando" direction="output">Front matter extracted from the web resource template.</param>
    /// <example>
    /// To render a template from a web resource named "csp_/templates/welcome.liquid",
    /// set WebResourceName to that value, RecordId to a contact GUID, and RecordType to "contact".
    /// The Results output parameter will return the rendered body, and FrontMatter will contain
    /// the parsed front matter attributes when present.
    /// </example>
    public class RenderWebResourceTemplate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string webResourceName = ctx.GetInputParameter<string>("WebResourceName", true);
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

            // Retrieve web resource template source.
            var templateSource = ctx.Service.GetWebResourceContent(webResourceName);

            // Split front matter and template body.
            var frontMatterResults = SparkCode.Templates.GetFrontMatter.Parse(templateSource);
            var frontMatter = (Entity)frontMatterResults["frontMatter"];
            var templateBody = (string)frontMatterResults["body"];

            // Run Logic
            var parser = new FluidParser();
            SparkCode.Templates.TemplateRenderer.RegisterCustomTags(parser, ctx.Service);
            var parsedTemplate = SparkCode.Templates.TemplateRenderer.ParseTemplate(templateBody, parser);
            var visitor = new SparkCode.Templates.IdentifierVisitor();
            visitor.VisitTemplate(parsedTemplate);
            var identifiers = visitor.Identifiers.ToArray();

            var model = SparkCode.Templates.TemplateRenderer.BuildDataverseModel(
                ctx.Service,
                recordType,
                recordIdStr,
                additionalContext,
                identifiers
            );

            string renderedTemplate = SparkCode.Templates.TemplateRenderer.Render(parsedTemplate, model);

            // API Outputs
            ctx.SetOutputParameter("Results", renderedTemplate);
            ctx.SetOutputParameter("FrontMatter", frontMatter);
            ctx.SetOutputParameter("FrontMatterJson", frontMatter.ToJson());
        }
    }
}