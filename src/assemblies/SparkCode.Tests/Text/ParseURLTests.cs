using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.Tests.Text
{
    public class ParseURLTests
    {
        [Fact]
        public void ParseURL_ValidUrl_Returns_Parsed_Components()
        {
            var ctx = new Context();
            var expectedId = Guid.NewGuid();

            string url = $"http://www.example.com/path?id={expectedId}&etc=123#fragment";
            var results = SparkCode.Text.ParseURL.Parse(ctx, url);

            var query = (Entity)results["query"];
            
            Assert.Equal("http", (string)results["scheme"]);
            Assert.Equal("www.example.com", (string)results["host"]);
            Assert.Equal(80, (int)results["port"]);
            Assert.Equal("/path", (string)results["absolutePath"]);
            Assert.Equal("fragment", (string)results["fragment"]);
            Assert.Equal(expectedId, Guid.Parse((string)query["id"]));
            Assert.Equal(123, int.Parse((string)query["etc"]));
        }

        [Fact]
        public void ParseURL_InvalidURL_Throws_Exception()
        {
            var ctx = new Context();
            string url = "abc123";
            Assert.Throws<UriFormatException>(() =>
                SparkCode.Text.ParseURL.Parse(ctx, url)
            );
        }
    }
}
