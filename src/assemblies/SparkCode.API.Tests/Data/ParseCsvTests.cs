using Microsoft.Xrm.Sdk;
using System;
using System.Text.Json;
using Xunit;

namespace SparkCode.API.Tests.Data
{
    public class ParseCsvTests
    {
        [Fact]
        public void ParseCsv_ValidCsv_Returns_ParsedRows()
        {
            var service = new Context().Service;
            var csv = "name,count,price,createdOn\nWidgetA,10,19.95,2026-04-24T10:15:00\nWidgetB,25,3.5,2026-04-25T00:00:00";

            var output = service.Execute(new OrganizationRequest("csp_Data_ParseCsv")
            {
                Parameters = new ParameterCollection
                {
                    { "Csv", csv },
                    { "Delimiter", "," },
                    { "FieldsEnclosedInQuotes", false }
                }
            });

            var results = (Entity)output["Results"];
            var rows = (EntityCollection)results["rows"];

            Assert.Equal(2, (int)results["rowCount"]);
            Assert.Equal(2, rows.Entities.Count);
            Assert.Equal("WidgetA", (string)rows.Entities[0]["name"]);
            Assert.Equal(10, (int)rows.Entities[0]["count"]);
            Assert.Equal(19.95, (double)rows.Entities[0]["price"]);
            Assert.Equal(new DateTime(2026, 4, 24, 10, 15, 0), (DateTime)rows.Entities[0]["createdOn"]);
        }

        [Fact]
        public void ParseCsvJson_WithSemicolonAndQuotedValues_Returns_ResultsJson()
        {
            var service = new Context().Service;
            var csv = "name;count;price;createdOn\n\"Widget A\";10;19.95;\"2026-04-24T10:15:00\"\n\"Widget B\";25;3.5;\"2026-04-25T00:00:00\"";

            var output = service.Execute(new OrganizationRequest("csp_Data_ParseCsvJson")
            {
                Parameters = new ParameterCollection
                {
                    { "Csv", csv },
                    { "Delimiter", ";" },
                    { "FieldsEnclosedInQuotes", true }
                }
            });

            var resultsJson = (string)output["ResultsJson"];
            var parsedJson = JsonDocument.Parse(resultsJson);

            Assert.Equal(2, parsedJson.RootElement.GetProperty("rowCount").GetInt32());
            var rows = parsedJson.RootElement.GetProperty("rows");
            Assert.Equal(2, rows.GetArrayLength());
            Assert.Equal("Widget A", rows[0].GetProperty("name").GetString());
            Assert.Equal(10, rows[0].GetProperty("count").GetInt32());
            Assert.Equal(19.95, rows[0].GetProperty("price").GetDouble());
            Assert.Equal("Widget B", rows[1].GetProperty("name").GetString());
        }

        [Fact]
        public void ParseCsv_InvalidCsv_Throws_Exception()
        {
            var service = new Context().Service;
            var invalidCsv = "name;count\n\"Widget A;10";

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Data_ParseCsv")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Csv", invalidCsv },
                        { "Delimiter", ";" },
                        { "FieldsEnclosedInQuotes", true }
                    }
                });
            });
        }
    }
}