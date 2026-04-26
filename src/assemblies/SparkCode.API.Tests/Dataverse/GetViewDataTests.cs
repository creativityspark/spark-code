using System;

namespace SparkCode.API.Tests.Dataverse
{
    using Microsoft.Xrm.Sdk;
    using Xunit;

    public class GetViewDataTests
    {
        [Fact]
        public void GetViewData_ValidInputs_Returns_Results_Expando()
        {
            var service = new Context().Service;
            var output = service.Execute(new OrganizationRequest("csp_Dataverse_GetViewData")
            {
                Parameters = new ParameterCollection
                {
                    { "TableName", "account" },
                    { "ViewName", "Active Accounts" }
                }
            });

            var results = output["Results"];
            Assert.NotNull(results);
        }

        [Fact]
        public void GetViewData_ValidInputs_Returns_ResultsJson_Json()
        {
            var service = new Context().Service;
            var output = service.Execute(new OrganizationRequest("csp_Dataverse_GetViewDataJson")
            {
                Parameters = new ParameterCollection
                {
                    { "TableName", "account" },
                    { "ViewName", "Active Accounts" }
                }
            });

            var resultsJson = output["ResultsJson"];
            Assert.NotNull(resultsJson);
        }

        [Fact]
        public void GetViewData_MissingInputs_Throws_Exception()
        {
            var service = new Context().Service;

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Dataverse_GetViewData")
                {
                    Parameters = new ParameterCollection()
                });
            });
        }
    }
}
