using Microsoft.Xrm.Sdk;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SparkCode.CustomAPIs
{
    /// <summary>
    /// Extracts YAML front matter from a given text input and returns it as a JSON object along with the remaining body of the text.
    /// </summary>
    public class GetFrontMatter : IPlugin
    {
        Context ctx = new Context();
        
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            string inputText = context.InputParameters["InputText"] as string;

            // Extract the frontMatter using regex
            // Regex pattern to capture YAML front matter enclosed between '---' delimiters at the very start of the string, allowing optional whitespace/newlines
            string frontMatterPattern = @"^---\s*\n(.*?)\n---\s*\n";
            string frontMatterContent = string.Empty;
            string templateWithoutFrontMatter = inputText;

            // Create the regex with RegexOptions.Singleline to make . match newlines
            var regex = new Regex(frontMatterPattern, RegexOptions.Singleline);
            Match match = regex.Match(inputText);

            if (match.Success && match.Groups.Count > 1)
            {
                // Extract front matter content
                frontMatterContent = match.Groups[1].Value.Trim();

                // Remove the front matter from the template
                templateWithoutFrontMatter = regex.Replace(inputText, string.Empty).Trim();

                // Parse YAML front matter into dictionary
                Dictionary<string, string> frontMatterDict = ParseYamlFrontMatter(frontMatterContent);

                // Convert dictionary to JSON
                string frontMatterJson = JsonConvert.SerializeObject(frontMatterDict);

                // Set output parameters
                context.OutputParameters["FrontMatter"] = frontMatterJson;
                context.OutputParameters["Body"] = templateWithoutFrontMatter;
            }
            else
            {
                // If no front matter is found, return empty JSON and original template
                context.OutputParameters["FrontMatter"] = "{}";
                context.OutputParameters["Body"] = inputText;
            }
        }

        private Dictionary<string, string> ParseYamlFrontMatter(string yamlContent)
        {
            var result = new Dictionary<string, string>();

            // Split the YAML content into lines
            string[] lines = yamlContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                // Find the first colon which separates key and value
                int colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    string key = line.Substring(0, colonIndex).Trim();
                    string value = line.Substring(colonIndex + 1).Trim();

                    // Remove quotes if they exist
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                        value = value.Substring(1, value.Length - 2);

                    result[key] = value;
                }
            }

            return result;
        }
    }
}