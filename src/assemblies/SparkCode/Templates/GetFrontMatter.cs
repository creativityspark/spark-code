using Microsoft.Xrm.Sdk;
using System;
using System.Text.RegularExpressions;

namespace SparkCode.Templates
{
    public static class GetFrontMatter
    {
        public static Entity Parse(Context ctx, string inputText)
        {
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

            var results = new Entity();
            results["frontMatter"] = frontMatter;
            results["body"] = body;

            return results;
        }

        private static Entity ParseYamlFrontMatter(string yamlContent)
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