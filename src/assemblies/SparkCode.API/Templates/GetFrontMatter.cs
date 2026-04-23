using Microsoft.Xrm.Sdk;
using System;
using System.Text.RegularExpressions;

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
            string frontMatterPattern = @"^---\s*\n(.*?)\n---\s*\n";
            var frontMatter = new Entity();
            string body = inputText;
            var regex = new Regex(frontMatterPattern, RegexOptions.Singleline);
            Match match = regex.Match(inputText);

            if (match.Success && match.Groups.Count > 1)
            {
                string frontMatterContent = match.Groups[1].Value.Trim();
                body = regex.Replace(inputText, string.Empty).Trim();
                frontMatter = ParseYamlFrontMatter(frontMatterContent);
            }

            // API Outputs
            ctx.SetOutputParameter("FrontMatter", frontMatter);
            ctx.SetOutputParameter("FrontMatterJson", frontMatter.ToJson());
            ctx.SetOutputParameter("Body", body);
        }

        private Entity ParseYamlFrontMatter(string yamlContent)
        {
            var result = new Entity();
            string[] lines = yamlContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                {
                    continue;
                }

                int colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    string key = line.Substring(0, colonIndex).Trim();
                    string value = line.Substring(colonIndex + 1).Trim();

                    if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    result[key] = value;
                }
            }

            return result;
        }
    }
}