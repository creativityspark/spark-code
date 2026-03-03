using System;

namespace SparkCode.API.Tests.Dataverse
{
    using Microsoft.Xrm.Sdk;
    using Xunit;

    public class RunSQLTests
    {
        [Fact]
        public void RunSQL_ValidQuery_Returns_Results_Entity()
        {
            var service = Context.GetService();
            var sql = "SELECT TOP 10 * FROM account";
            var output = service.Execute(new OrganizationRequest("csp_Dataverse.RunSQL")
            {
                Parameters = new ParameterCollection
                {
                    { "SQLQuery", sql }
                }
            });
            var results = output["Results"];
            Assert.NotNull(results);
        }

        [Fact]
        public void RunSQL_ValidQuery_Returns_ResultsJson_Json()
        {
            var service = Context.GetService();
            var sql = "SELECT TOP 10 * FROM account";
            var output = service.Execute(new OrganizationRequest("csp_Dataverse.RunSQLJson")
            {
                Parameters = new ParameterCollection
                {
                    { "SQLQuery", sql }
                }
            });
            var resultsJson = output["ResultsJson"];
            Assert.NotNull(resultsJson);
        }

        [Fact]
        public void RunSQL_InvalidQuery_Throws_Exception()
        {
            var service = Context.GetService();
            var sql = "SELECT * FROM non_existing_table";
            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Dataverse.RunSQL")
                {
                    Parameters = new ParameterCollection
                    {
                        { "SQLQuery", sql }
                    }
                });
            });
        }
    }
}
