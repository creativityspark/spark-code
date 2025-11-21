using SparkCode.CustomAPIs;
using Xunit;

namespace SparkCode.CustomAPIs.Tests
{
    public class CsvToJsonTests
    {
        [Fact]
        public void Convert_ValidCsv_ReturnsJson()
        {
            var csvToJson = new CsvToJson();
            string csv = "name,age\nAlice,30\nBob,25";
            string expectedJson = @"[
  {
    ""name"": ""Alice"",
    ""age"": 30
  },
  {
    ""name"": ""Bob"",
    ""age"": 25
  }
]";
            string json = csvToJson.Convert(csv,",",false);
            Assert.Equal(expectedJson, json);
        }
    }
}
