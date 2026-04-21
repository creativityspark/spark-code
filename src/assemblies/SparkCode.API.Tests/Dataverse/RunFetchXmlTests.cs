using System;

namespace SparkCode.API.Tests.Dataverse
{
    using Microsoft.Xrm.Sdk;
    using Xunit;

    public class RunFetchXmlTests
    {
        [Fact]
        public void RunFetchXml_ValidQuery_Returns_Results_EntityCollection()
        {
            var service = Context.GetService();
            var fetchXml = "<fetch top='10'><entity name='account'><attribute name='accountid' /><attribute name='name' /></entity></fetch>";
            var output = service.Execute(new OrganizationRequest("csp_Dataverse_RunFetchXml")
            {
                Parameters = new ParameterCollection
                {
                    { "FetchXml", fetchXml }
                }
            });
            var results = output["Results"];
            Assert.NotNull(results);
        }

        [Fact]
        public void RunFetchXml_ValidQuery_Returns_ResultsJson_Json()
        {
            var service = Context.GetService();
            var fetchXml = "<fetch top='10'><entity name='account'><attribute name='accountid' /><attribute name='name' /></entity></fetch>";
            var output = service.Execute(new OrganizationRequest("csp_Dataverse_RunFetchXmlJson")
            {
                Parameters = new ParameterCollection
                {
                    { "FetchXml", fetchXml }
                }
            });
            var resultsJson = output["ResultsJson"];
            Assert.NotNull(resultsJson);
        }

        [Fact]
        public void RunFetchXml_InvalidQuery_Throws_Exception()
        {
            var service = Context.GetService();
            var fetchXml = "<fetch><entity name='non_existing_table'><attribute name='name' /></entity></fetch>";
            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Dataverse_RunFetchXml")
                {
                    Parameters = new ParameterCollection
                    {
                        { "FetchXml", fetchXml }
                    }
                });
            });
        }
    }
}