using System;
using Xunit;

namespace SparkCode.Tests.Data
{
    public class SelectTests
    {
        [Fact]
        public void RunQuery_ValidQuery_Returns_ExpectedResult()
        {
            var data = "{ \"items\": [{ \"id\": 1 }, { \"id\": 2 }] }";
            var query = "items[0].id";
            var result = SparkCode.Data.Select.RunQuery(data, query);
            Assert.Equal("1", result);
        }

        [Fact]
        public void RunQuery_MultipleTokens_Returns_JoinedResults()
        {
            var data = "{ \"items\": [{ \"id\": 1 }, { \"id\": 2 }] }";
            var query = "items[*].id";
            var result = SparkCode.Data.Select.RunQuery(data, query);
            Assert.Equal("1,2", result);
        }

        [Fact]
        public void RunQuery_InvalidQuery_Returns_Null()
        {
            var data = "{ \"items\": [{ \"id\": 1 }] }";
            var query = "not_a_valid_query";
            var result = SparkCode.Data.Select.RunQuery(data, query);
            Assert.Null(result);
        }

        [Fact]
        public void RunQuery_InvalidJson_Throws_Exception()
        {
            var data = "not_a_valid_json";
            var query = "items[0].id";
            Assert.ThrowsAny<Exception>(() =>
            {
                SparkCode.Data.Select.RunQuery(data, query);
            });
        }
    }
}
