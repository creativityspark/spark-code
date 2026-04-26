using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Templates
{
    /// <displayName>Get Front Matter</displayName>
    /// <summary>Extracts YAML front matter from a text input and returns it together with the remaining body.</summary>
    /// <param name="InputText" type="string">Text that may start with YAML front matter delimited by --- markers.</param>
    /// <param name="FrontMatter" type="expando" direction="output">Front matter content as an expando object where each YAML key becomes an attribute.</param>
    /// <param name="Body" type="string" direction="output">Input text without the front matter block.</param>
    /// <example>
    /// To extract front matter from a markdown template, pass the InputText parameter as "---\ntitle: Hello\nauthor: Cris\n---\n# Body".
    /// The FrontMatter output parameter will contain attributes title="Hello" and author="Cris", and the Body output parameter will return "# Body".
    /// </example>
    public class GetFrontMatter : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string inputText = ctx.GetInputParameter<string>("InputText", true);

            // Run Logic
            var results = SparkCode.Templates.GetFrontMatter.Parse(inputText);
            var frontMatter = (Entity)results["frontMatter"];
            var body = (string)results["body"];

            // API Outputs
            ctx.SetOutputParameter("FrontMatter", frontMatter);
            ctx.SetOutputParameter("FrontMatterJson", frontMatter.ToJson());
            ctx.SetOutputParameter("Body", body);
        }
    }
}