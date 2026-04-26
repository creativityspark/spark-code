using Fluid;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Linq;

namespace SparkCode.API.Templates
{
    /// <displayName>Render Dataverse Template</displayName>
    /// <summary>Renders a Liquid template by sourcing context values from a Dataverse record.</summary>
    /// <param name="Template" type="string">Liquid template text to render.</param>
    /// <param name="RecordId" type="string">GUID of the Dataverse record to use as context.</param>
    /// <param name="RecordType" type="string">Logical name of the table the record belongs to.</param>
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

            // Run Logic
            string renderedTemplate;
            var parser = new FluidParser();
            if (parser.TryParse(templateSource, out IFluidTemplate template, out string errorMessage))
            {
                // Collect identifiers referenced by the template
                var visitor = new IdentifierVisitor();
                visitor.VisitTemplate(template);
                var identifiers = visitor.Identifiers.ToArray();

                // Retrieve only the needed columns from Dataverse
                var recordId = new Guid(recordIdStr);
                var columnSet = identifiers.Length > 0
                    ? new ColumnSet(identifiers)
                    : new ColumnSet(true);

                var record = ctx.Service.Retrieve(recordType, recordId, columnSet);

                // Convert the entity to ExpandoObject via JSON for use as template model
                var model = JsonConvert.DeserializeObject<ExpandoObject>(record.ToJson());

                var templateContext = new TemplateContext(model);
                renderedTemplate = template.Render(templateContext);
            }
            else
            {
                throw new InvalidPluginExecutionException($"Invalid Liquid template: {errorMessage}");
            }

            // API Outputs
            ctx.SetOutputParameter("Results", renderedTemplate);
        }
    }
}
