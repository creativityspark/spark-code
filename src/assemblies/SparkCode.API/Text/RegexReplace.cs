using Microsoft.Xrm.Sdk;
using System;
using System.Text.RegularExpressions;

namespace SparkCode.API.Text
{
    /// <displayName>Regex Replace</displayName>
    /// <summary>
    /// Runs a regular expression replacement over the provided input text.
    /// </summary>
    /// <param name="Input" type="string">Text where the regular expression pattern will be applied.</param>
    /// <param name="Pattern" type="string">Regular expression pattern to find.</param>
    /// <param name="Replacement" type="string">Replacement text used for matching occurrences.</param>
    /// <param name="Results" type="string" direction="output">Input text after applying the regex replacement.</param>
    /// <example>
    /// To replace all digits in "Item-123" with "X", pass Input as "Item-123", Pattern as "\\d", and Replacement as "X".
    /// The Results output parameter will return "Item-XXX".
    /// </example>
    public class RegexReplace : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string input = ctx.GetInputParameter<string>("Input", true);
            string pattern = ctx.GetInputParameter<string>("Pattern", true);
            string replacement = ctx.GetInputParameter<string>("Replacement", true);

            // Run Logic
            string results = Regex.Replace(input, pattern, replacement);

            // API Outputs
            ctx.SetOutputParameter("Results", results);
        }
    }
}