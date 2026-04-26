using Microsoft.Xrm.Sdk;
using System;

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
            string renderedTemplate = SparkCode.Templates.TemplateRenderer.Render(templateSource, jsonValues);

            // API Outputs
            ctx.SetOutputParameter("Results", renderedTemplate);
        }
    }
}
