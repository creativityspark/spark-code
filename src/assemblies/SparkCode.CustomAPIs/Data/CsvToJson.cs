using Microsoft.VisualBasic.FileIO;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SparkCode.CustomAPIs.Data
{
    /// <summary>
    /// Converts CSV data to JSON format.
    /// Based on: https://learn.microsoft.com/en-us/dotnet/visual-basic/developing-apps/programming/drives-directories-files/how-to-read-from-comma-delimited-text-files
    /// </summary>
    public class CsvToJson
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // Custom API Inputs
            string xml = context.InputParameters["Csv"] as string ?? throw new ArgumentNullException($"Csv is required");
            string delimiter = context.InputParameters["Delimiter"] as string ?? ",";
            bool fieldsEnclosedInQuotes = context.InputParameters.Contains("FieldsEnclosedInQuotes") ? (bool)context.InputParameters["FieldsEnclosedInQuotes"] : true;

            string json = Convert(xml, delimiter,fieldsEnclosedInQuotes);

            // Set OutputParameters values
            context.OutputParameters["Json"] = json;
        }

        public string Convert(string csv, string delimiter,bool fieldsEnclosedInQuotes)
        {
            // Trace input parameters
            ctx.Trace($"csv: {csv}");
            ctx.Trace($"delimiter: {delimiter}");

            // create csv stream from string
            var csvStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));
            var data = new JArray();

            var columnNames = new List<string>();

            using (TextFieldParser parser = new TextFieldParser(csvStream))
            {
                bool isFirstRow = true;
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(delimiter);
                parser.HasFieldsEnclosedInQuotes = fieldsEnclosedInQuotes; 

                while (!parser.EndOfData)
                {
                    //Process row
                    var row = new JObject();

                    string[] fields = parser.ReadFields();

                    if (isFirstRow)
                    {
                        isFirstRow = false;
                        foreach (string field in fields)
                        {
                            columnNames.Add(field);
                        }
                    }
                    else
                    {
                        var colIndex = 0;
                        foreach (string fieldValue in fields)
                        {
                            row.Add(columnNames[colIndex], GetValue(fieldValue));
                            colIndex++;
                        }
                        data.Add(row);
                    }
                }
            }

            string json = data.ToString();

            ctx.Trace($"JSon: {json}");
            return json;
        }

        private JToken GetValue(string fieldValue)
        {
            if(string.IsNullOrEmpty(fieldValue))
            {
                return JValue.CreateNull();
            }
            if (int.TryParse(fieldValue, out int intNumber))
            {
                return new JValue(intNumber);
            }
            if (double.TryParse(fieldValue, out double dblNumber))
            {
                return new JValue(dblNumber);
            }
            if (bool.TryParse(fieldValue, out bool boolean))
            {
                return new JValue(boolean);
            }
            if (DateTime.TryParse(fieldValue, out DateTime dateTime))
            {
                return new JValue(dateTime);
            }
            return new JValue(fieldValue);
        }
    }
}
