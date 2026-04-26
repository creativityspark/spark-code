using Fluid;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Dynamic;

namespace SparkCode.API.Templates
{
    /// <displayName>Render Template</displayName>
    /// <summary>Renders a Liquid template using the provided JSON context data.</summary>
    /// <param name="Template" type="string">Liquid template text to render.</param>
    /// <param name="Context" type="string">JSON string with values used by the template.</param>
    /// <param name="Results" type="string" direction="output">Rendered template text.</param>
    /// <example>
    /// To render "Hello {{name}}", set Template to "Hello {{name}}" and Context to "{\"name\":\"Cris\"}".
    /// The Results output parameter will return "Hello Cris".
    /// </example>
    public class RenderTemplate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string templateSource = ctx.GetInputParameter<string>("Template", true);
            string jsonValues = ctx.GetInputParameter<string>("Context", true);

            // Run Logic
            string renderedTemplate;
            var parser = new FluidParser();
            if (parser.TryParse(templateSource, out IFluidTemplate template, out string errorMessage))
            {
                var model = JsonConvert.DeserializeObject<ExpandoObject>(jsonValues);
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
