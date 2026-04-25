using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Data
{
    /// <displayName>Parse CSV</displayName>
    /// <summary>Parses CSV data and returns rows as an expando object.</summary>
    /// <param name="Csv" type="string">CSV content including a header row.</param>
    /// <param name="Delimiter" type="string">Column delimiter to use while parsing. Defaults to comma when omitted.</param>
    /// <param name="FieldsEnclosedInQuotes" type="bool">Indicates whether fields can be enclosed in quotes. Defaults to true when omitted.</param>
    /// <param name="Results" type="expando" direction="output">Parsed CSV result including rows and rowCount.</param>
    public class ParseCsv : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string csv = ctx.GetInputParameter<string>("Csv", true);

            string delimiter = ",";
            if (ctx.PluginContext.InputParameters.Contains("Delimiter"))
            {
                delimiter = (string)ctx.PluginContext.InputParameters["Delimiter"];
            }

            bool fieldsEnclosedInQuotes = true;
            if (ctx.PluginContext.InputParameters.Contains("FieldsEnclosedInQuotes"))
            {
                fieldsEnclosedInQuotes = (bool)ctx.PluginContext.InputParameters["FieldsEnclosedInQuotes"];
            }

            // Run Logic
            var results = SparkCode.Data.ParseCsv.Parse(ctx, csv, delimiter, fieldsEnclosedInQuotes);

            // API Outputs
            ctx.SetOutputParameter("Results", results);
            ctx.SetOutputParameter("ResultsJson", results.ToJson());
        }
    }
}