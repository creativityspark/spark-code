using Microsoft.VisualBasic.FileIO;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SparkCode.Data
{
    public static class ParseCsv
    {
        public static Entity Parse(string csv, string delimiter, bool fieldsEnclosedInQuotes)
        {
            if (string.IsNullOrWhiteSpace(csv))
            {
                throw new ArgumentNullException(nameof(csv));
            }

            if (string.IsNullOrEmpty(delimiter))
            {
                delimiter = ",";
            }

            var result = new Entity();
            var rows = new EntityCollection();
            var columnNames = new List<string>();

            using (var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            using (var parser = new TextFieldParser(csvStream))
            {
                var isFirstRow = true;
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(delimiter);
                parser.HasFieldsEnclosedInQuotes = fieldsEnclosedInQuotes;

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    if (fields == null)
                    {
                        continue;
                    }

                    if (isFirstRow)
                    {
                        isFirstRow = false;
                        columnNames.AddRange(fields);
                        continue;
                    }

                    var row = new Entity();
                    for (var i = 0; i < fields.Length && i < columnNames.Count; i++)
                    {
                        row[columnNames[i]] = GetValue(fields[i]);
                    }

                    rows.Entities.Add(row);
                }
            }

            result["rows"] = rows;
            result["rowCount"] = rows.Entities.Count;

            return result;
        }

        private static object GetValue(string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldValue))
            {
                return null;
            }

            if (int.TryParse(fieldValue, out int intNumber))
            {
                return intNumber;
            }

            if (double.TryParse(fieldValue, out double dblNumber))
            {
                return dblNumber;
            }

            if (bool.TryParse(fieldValue, out bool boolean))
            {
                return boolean;
            }

            if (DateTime.TryParse(fieldValue, out DateTime dateTime))
            {
                return dateTime;
            }

            return fieldValue;
        }
    }
}