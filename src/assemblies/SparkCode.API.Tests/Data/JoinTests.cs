using Microsoft.Xrm.Sdk;
using System;
using System.Text.Json;
using Xunit;

namespace SparkCode.API.Tests.Data
{
    public class JoinTests
    {
        [Fact]
        public void Join_ValidInputs_ReturnsJoinedEntityCollection()
        {
            var service = new Context().Service;
            var list1 = "[{\"id\":1,\"name\":\"order1\",\"customerid\":1},{\"id\":2,\"name\":\"order2\",\"customerid\":2}]";
            var list2 = "[{\"id\":1,\"name\":\"account1\"},{\"id\":2,\"name\":\"account2\"}]";

            var output = service.Execute(new OrganizationRequest("csp_Data_Join")
            {
                Parameters = new ParameterCollection
                {
                    { "List1", list1 },
                    { "List2", list2 },
                    { "Field1", "customerid" },
                    { "Field2", "id" }
                }
            });

            var results = (EntityCollection)output["Results"];

            Assert.Equal(2, results.Entities.Count);
            Assert.Equal("account1", (string)results.Entities[0]["customerid_name"]);
            Assert.Equal("account2", (string)results.Entities[1]["customerid_name"]);
        }

        [Fact]
        public void JoinJson_ValidInputs_ReturnsJoinedJsonArray()
        {
            var service = new Context().Service;
            var list1 = "[{\"id\":1,\"name\":\"order1\",\"customerid\":1}]";
            var list2 = "[{\"id\":1,\"name\":\"account1\"}]";

            var output = service.Execute(new OrganizationRequest("csp_Data_JoinJson")
            {
                Parameters = new ParameterCollection
                {
                    { "List1", list1 },
                    { "List2", list2 },
                    { "Field1", "customerid" },
                    { "Field2", "id" }
                }
            });

            var resultsJson = (string)output["ResultsJson"];
            var parsed = JsonDocument.Parse(resultsJson);

            Assert.Equal(JsonValueKind.Array, parsed.RootElement.ValueKind);
            Assert.Single(parsed.RootElement.EnumerateArray());
            var first = parsed.RootElement[0];
            Assert.Equal("order1", first.GetProperty("name").GetString());
            Assert.Equal("account1", first.GetProperty("customerid.name").GetString());
        }

        [Fact]
        public void Join_InvalidJson_ThrowsException()
        {
            var service = new Context().Service;

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Data_Join")
                {
                    Parameters = new ParameterCollection
                    {
                        { "List1", "not_json" },
                        { "List2", "[]" },
                        { "Field1", "customerid" },
                        { "Field2", "id" }
                    }
                });
            });
        }

        [Fact]
        public void Join_MissingRequiredParameter_ThrowsException()
        {
            var service = new Context().Service;
            var list1 = "[{\"id\":1,\"customerid\":1}]";
            var list2 = "[{\"id\":1}]";

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Data_Join")
                {
                    Parameters = new ParameterCollection
                    {
                        { "List1", list1 },
                        { "List2", list2 },
                        { "Field1", "customerid" }
                    }
                });
            });
        }
    }
}
