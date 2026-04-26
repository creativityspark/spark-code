using Microsoft.Xrm.Sdk;
using System;
using System.Text.RegularExpressions;

namespace SparkCode.API.Text
{
    /// <displayName>Regex Matches</displayName>
    /// <summary>
    /// Captures all matches of a regular expression over an input text.
    /// </summary>
    /// <param name="Input" type="string">Text where the regular expression will be evaluated.</param>
    /// <param name="Pattern" type="string">Regular expression pattern to evaluate.</param>
    /// <param name="Options" type="int" optional="true">Regex options bitmask. Defaults to 0 when omitted.</param>
    /// <param name="Results" type="entitycollection" direction="output">Collection of matches with value, index, and length attributes.</param>
    /// <example>
    /// To capture all numbers from the text "A1 B22 C333", pass Input as "A1 B22 C333" and Pattern as "\\d+".
    /// The Results output parameter will return three items with values "1", "22", and "333".
    /// </example>
    public class RegexMatches : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string input = ctx.GetInputParameter<string>("Input", true);
            string pattern = ctx.GetInputParameter<string>("Pattern", true);

            int options = 0;
            if (ctx.PluginContext.InputParameters.Contains("Options"))
            {
                options = Convert.ToInt32(ctx.PluginContext.InputParameters["Options"]);
            }

            // Run Logic
            var results = GetMatches(input, pattern, options);

            // API Outputs
            ctx.SetOutputParameter("Results", results);
            ctx.SetOutputParameter("ResultsJson", results.ToJson());
        }

        private static EntityCollection GetMatches(string input, string pattern, int options)
        {
            var entityCollection = new EntityCollection();
            var regex = new Regex(pattern, (RegexOptions)options);
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                var matchEntity = new Entity();
                matchEntity["value"] = match.Value;
                matchEntity["index"] = match.Index;
                matchEntity["length"] = match.Length;
                entityCollection.Entities.Add(matchEntity);
            }

            return entityCollection;
        }
    }
}