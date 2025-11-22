using SparkCode.CustomAPIs.Other;
using System;
using Xunit;

namespace SparkCode.CustomAPIs.Tests.Other
{
    public class ParseURLTests
    {
        [Fact]
        public void ParseURL_ValidUrl_Returns_Parsed_Components()
        {
            var ctx = new Context();
            var parseUrl = new ParseURL();
            var expectedId = Guid.NewGuid();
            string url = $"http://www.example.com/path?id={expectedId}&etc=123#fragment";
            var result = parseUrl.TryParse(url, out int etc, out Guid id);
            Assert.True(result);
            Assert.Equal(expectedId, id);
            Assert.Equal(123, etc);
        }

        [Fact]
        public void ParseURL_ValidUrl_Invalid_Values()
        {
            var ctx = new Context();
            var parseUrl = new ParseURL();
            string url = $"http://www.example.com/path?id=def&etc=abc#fragment";
            var result = parseUrl.TryParse(url, out int etc, out Guid id);
            Assert.False(result);
            Assert.Equal(Guid.Empty, id);
            Assert.Equal(0, etc);
        }
    }
}
