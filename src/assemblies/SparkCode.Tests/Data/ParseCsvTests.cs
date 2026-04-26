using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.Tests.Data
{
    public class ParseCsvTests
    {
        [Fact]
        public void Parse_ValidCsv_ReturnsParsedRows()
        {
            var csv = "name,age\nAlice,30\nBob,25";

            var result = SparkCode.Data.ParseCsv.Parse(csv, ",", false);
            var rows = (EntityCollection)result["rows"];

            Assert.Equal(2, (int)result["rowCount"]);
            Assert.Equal(2, rows.Entities.Count);
            Assert.Equal("Alice", (string)rows.Entities[0]["name"]);
            Assert.Equal(30, (int)rows.Entities[0]["age"]);
            Assert.Equal("Bob", (string)rows.Entities[1]["name"]);
            Assert.Equal(25, (int)rows.Entities[1]["age"]);
        }

        [Fact]
        public void Parse_WithSemicolonAndQuotedValues_ReturnsExpectedFields()
        {
            var csv = "name;note\n\"Alice\";\"hello;world\"";

            var result = SparkCode.Data.ParseCsv.Parse(csv, ";", true);
            var rows = (EntityCollection)result["rows"];

            Assert.Single(rows.Entities);
            Assert.Equal("Alice", (string)rows.Entities[0]["name"]);
            Assert.Equal("hello;world", (string)rows.Entities[0]["note"]);
        }

        [Fact]
        public void Parse_ComplexCsv_MultipleRows_ParsesAllSupportedDataTypes()
        {
            var csv = string.Join("\n", new[]
            {
                "name,count,price,createdOn",
                "WidgetA,10,19.95,2026-04-24T10:15:00",
                "WidgetB,25,3.5,2026-04-25T00:00:00",
                "WidgetC,0,100.01,2026-04-26T23:59:59"
            });

            var result = SparkCode.Data.ParseCsv.Parse(csv, ",", false);
            var rows = (EntityCollection)result["rows"];

            Assert.Equal(3, (int)result["rowCount"]);
            Assert.Equal(3, rows.Entities.Count);

            Assert.Equal("WidgetA", (string)rows.Entities[0]["name"]);
            Assert.Equal(10, (int)rows.Entities[0]["count"]);
            Assert.Equal(19.95, (double)rows.Entities[0]["price"]);
            Assert.Equal(new DateTime(2026, 4, 24, 10, 15, 0), (DateTime)rows.Entities[0]["createdOn"]);

            Assert.Equal("WidgetB", (string)rows.Entities[1]["name"]);
            Assert.Equal(25, (int)rows.Entities[1]["count"]);
            Assert.Equal(3.5, (double)rows.Entities[1]["price"]);
            Assert.Equal(new DateTime(2026, 4, 25, 0, 0, 0), (DateTime)rows.Entities[1]["createdOn"]);

            Assert.Equal("WidgetC", (string)rows.Entities[2]["name"]);
            Assert.Equal(0, (int)rows.Entities[2]["count"]);
            Assert.Equal(100.01, (double)rows.Entities[2]["price"]);
            Assert.Equal(new DateTime(2026, 4, 26, 23, 59, 59), (DateTime)rows.Entities[2]["createdOn"]);
        }

        [Fact]
        public void Parse_ComplexCsv_WithSemicolonAndQuotedTextAndDateTime_ParsesAllSupportedDataTypes()
        {
            var csv = string.Join("\n", new[]
            {
                "name;count;price;createdOn",
                "\"Widget A\";10;19.95;\"2026-04-24T10:15:00\"",
                "\"Widget B\";25;3.5;\"2026-04-25T00:00:00\"",
                "\"Widget C\";0;100.01;\"2026-04-26T23:59:59\""
            });

            var result = SparkCode.Data.ParseCsv.Parse(csv, ";", true);
            var rows = (EntityCollection)result["rows"];

            Assert.Equal(3, (int)result["rowCount"]);
            Assert.Equal(3, rows.Entities.Count);

            Assert.Equal("Widget A", (string)rows.Entities[0]["name"]);
            Assert.Equal(10, (int)rows.Entities[0]["count"]);
            Assert.Equal(19.95, (double)rows.Entities[0]["price"]);
            Assert.Equal(new DateTime(2026, 4, 24, 10, 15, 0), (DateTime)rows.Entities[0]["createdOn"]);

            Assert.Equal("Widget B", (string)rows.Entities[1]["name"]);
            Assert.Equal(25, (int)rows.Entities[1]["count"]);
            Assert.Equal(3.5, (double)rows.Entities[1]["price"]);
            Assert.Equal(new DateTime(2026, 4, 25, 0, 0, 0), (DateTime)rows.Entities[1]["createdOn"]);

            Assert.Equal("Widget C", (string)rows.Entities[2]["name"]);
            Assert.Equal(0, (int)rows.Entities[2]["count"]);
            Assert.Equal(100.01, (double)rows.Entities[2]["price"]);
            Assert.Equal(new DateTime(2026, 4, 26, 23, 59, 59), (DateTime)rows.Entities[2]["createdOn"]);
        }

        [Fact]
        public void Parse_EmptyCsv_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                SparkCode.Data.ParseCsv.Parse(string.Empty, ",", true)
            );
        }
    }
}