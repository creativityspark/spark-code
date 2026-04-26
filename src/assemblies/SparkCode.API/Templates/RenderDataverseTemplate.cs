using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Templates
{
    /// <displayName>Render Dataverse Template</displayName>
    /// <summary>Renders a Liquid template by sourcing context values from a Dataverse record.</summary>
    /// <param name="Template" type="string">Liquid template text to render.</param>
    /// <param name="RecordId" type="string">GUID of the Dataverse record to use as context.</param>
    /// <param name="RecordType" type="string">Logical name of the table the record belongs to.</param>
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
            string recordIdStr = ctx.GetInputParameter<string>("RecordId", true);
            string recordType = ctx.GetInputParameter<string>("RecordType", true);
            string additionalContext = ctx.PluginContext.InputParameters.Contains("AdditionalContext")
                ? ctx.PluginContext.InputParameters["AdditionalContext"] as string
                : null;

            // Run Logic
            var parsedTemplate = SparkCode.Templates.TemplateRenderer.ParseTemplate(templateSource);
            var model = SparkCode.Templates.TemplateRenderer.BuildDataverseModel(
                ctx.Service,
                recordType,
                recordIdStr,
                additionalContext,
                parsedTemplate
            );

            string renderedTemplate = SparkCode.Templates.TemplateRenderer.Render(parsedTemplate, model);

            // API Outputs
            ctx.SetOutputParameter("Results", renderedTemplate);
        }
    }
}
