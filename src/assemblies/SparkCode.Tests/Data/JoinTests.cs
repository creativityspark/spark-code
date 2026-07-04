using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.Tests.Data
{
    public class JoinTests
    {
        [Fact]
        public void JoinCollections_ValidInnerJoin_ReturnsExpectedMergedRows()
        {
            var list1 = "[{\"id\":1,\"name\":\"order1\",\"customerid\":1},{\"id\":2,\"name\":\"order2\",\"customerid\":2}]";
            var list2 = "[{\"id\":1,\"name\":\"account1\"},{\"id\":2,\"name\":\"account2\"}]";

            var results = SparkCode.Data.Join.JoinCollections(list1, list2, "customerid", "id");

            Assert.Equal(2, results.Entities.Count);

            var first = results.Entities[0];
            Assert.Equal(1, (int)first["id"]);
            Assert.Equal("order1", (string)first["name"]);
            Assert.Equal(1, (int)first["customerid"]);
            Assert.Equal(1, (int)first["customerid_id"]);
            Assert.Equal("account1", (string)first["customerid_name"]);

            var second = results.Entities[1];
            Assert.Equal(2, (int)second["id"]);
            Assert.Equal("order2", (string)second["name"]);
            Assert.Equal(2, (int)second["customerid"]);
            Assert.Equal(2, (int)second["customerid_id"]);
            Assert.Equal("account2", (string)second["customerid_name"]);
        }

        [Fact]
        public void JoinCollections_LeftRowsMissingKey_AreSkipped()
        {
            var list1 = "[{\"id\":1,\"customerid\":1},{\"id\":2},{\"id\":3,\"customerid\":null}]";
            var list2 = "[{\"id\":1,\"name\":\"account1\"}]";

            var results = SparkCode.Data.Join.JoinCollections(list1, list2, "customerid", "id");

            Assert.Single(results.Entities);
            Assert.Equal(1, (int)results.Entities[0]["id"]);
        }

        [Fact]
        public void JoinCollections_NoMatches_ReturnsEmptyCollection()
        {
            var list1 = "[{\"id\":1,\"customerid\":99}]";
            var list2 = "[{\"id\":1,\"name\":\"account1\"}]";

            var results = SparkCode.Data.Join.JoinCollections(list1, list2, "customerid", "id");

            Assert.Empty(results.Entities);
        }

        [Fact]
        public void JoinCollections_DuplicateRightKeys_ProducesMultipleJoinedRows()
        {
            var list1 = "[{\"id\":1,\"customerid\":1}]";
            var list2 = "[{\"id\":1,\"name\":\"account1\"},{\"id\":1,\"name\":\"account1b\"}]";

            var results = SparkCode.Data.Join.JoinCollections(list1, list2, "customerid", "id");

            Assert.Equal(2, results.Entities.Count);
            Assert.Equal("account1", (string)results.Entities[0]["customerid_name"]);
            Assert.Equal("account1b", (string)results.Entities[1]["customerid_name"]);
        }

        [Fact]
        public void JoinCollectionsWithJson_UsesDottedKeysInJsonOutput()
        {
            var list1 = "[{\"id\":1,\"name\":\"order1\",\"customerid\":1}]";
            var list2 = "[{\"id\":1,\"name\":\"account1\"}]";

            var result = SparkCode.Data.Join.JoinCollectionsWithJson(list1, list2, "customerid", "id");

            Assert.Contains("\"customerid.id\":1", result.ResultsJson);
            Assert.Contains("\"customerid.name\":\"account1\"", result.ResultsJson);
        }

        [Fact]
        public void JoinCollections_NullOrWhitespaceInputs_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SparkCode.Data.Join.JoinCollections(null, "[]", "a", "b"));
            Assert.Throws<ArgumentNullException>(() => SparkCode.Data.Join.JoinCollections("[]", null, "a", "b"));
            Assert.Throws<ArgumentNullException>(() => SparkCode.Data.Join.JoinCollections("[]", "[]", "", "b"));
            Assert.Throws<ArgumentNullException>(() => SparkCode.Data.Join.JoinCollections("[]", "[]", "a", ""));
        }

        [Fact]
        public void JoinCollections_InvalidJson_ThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => SparkCode.Data.Join.JoinCollections("not_json", "[]", "a", "b"));
            Assert.ThrowsAny<Exception>(() => SparkCode.Data.Join.JoinCollections("[]", "not_json", "a", "b"));
        }
    }
}
