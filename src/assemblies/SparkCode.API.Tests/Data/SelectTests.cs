using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.Data
{
	public class SelectTests
	{
		[Fact]
		public void Select_ValidQuery_Returns_ExpectedResults()
		{
			var service = new Context().Service;
			var data = "{ \"items\": [{ \"id\": 1 }, { \"id\": 2 }] }";
			var query = "items[0].id";
			var output = service.Execute(new OrganizationRequest("csp_Data_Select")
			{
				Parameters = new ParameterCollection
				{
					{ "Data", data },
					{ "Query", query }
				}
			});
			var results = (string)output["Results"];
			Assert.Equal("1", results);
		}

		[Fact]
		public void Select_MultipleTokens_Returns_JoinedResults()
		{
			var service = new Context().Service;
			var data = "{ \"items\": [{ \"id\": 1 }, { \"id\": 2 }] }";
			var query = "items[*].id";
			var output = service.Execute(new OrganizationRequest("csp_Data_Select")
			{
				Parameters = new ParameterCollection
				{
					{ "Data", data },
					{ "Query", query }
				}
			});
			var results = (string)output["Results"];
			Assert.Equal("1,2", results);
		}

		[Fact]
		public void Select_InvalidQuery_Returns_Null()
		{
			var service = new Context().Service;
			var data = "{ \"items\": [{ \"id\": 1 }] }";
			var query = "not_a_valid_query";
            var output = service.Execute(new OrganizationRequest("csp_Data_Select")
            {
                Parameters = new ParameterCollection
                {
                    { "Data", data },
                    { "Query", query }
                }
			});
			var results = (string)output["Results"];
			Assert.Null(results);
		}

		[Fact]
		public void Select_InvalidJson_Throws_Exception()
		{
			var service = new Context().Service;
			var data = "not_a_valid_json";
			var query = "items[0].id";
			Assert.ThrowsAny<Exception>(() =>
			{
				service.Execute(new OrganizationRequest("csp_Data_Select")
				{
					Parameters = new ParameterCollection
					{
						{ "Data", data },
						{ "Query", query }
					}
				});
			});
		}
	}
}
