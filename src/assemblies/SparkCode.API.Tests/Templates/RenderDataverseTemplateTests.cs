using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using Xunit;

namespace SparkCode.API.Tests.Templates
{
    public class RenderDataverseTemplateTests
    {
        private static string GetFirstAccountId()
        {
            var service = new Context().Service;
            var results = service.RetrieveMultiple(new FetchExpression(
                "<fetch top='1'><entity name='account'><attribute name='accountid' /><attribute name='name' /></entity></fetch>"));

            if (results.Entities.Count == 0)
            {
                throw new InvalidOperationException("No account records found in the test environment.");
            }

            return results.Entities[0].Id.ToString();
        }

        [Fact]
        public void RenderDataverseTemplate_ValidTemplate_Returns_RenderedText()
        {
            var service = new Context().Service;
            var recordId = GetFirstAccountId();
            var template = "Account: {{ name }}";

            var output = service.Execute(new OrganizationRequest("csp_Templates_RenderDataverseTemplate")
            {
                Parameters = new ParameterCollection
                {
                    { "Template", template },
                    { "RecordId", recordId },
                    { "RecordType", "account" }
                }
            });

            Assert.True(output.Results.Contains("Results"), "Expected output parameter 'Results' was not returned.");
            var result = (string)output["Results"];
            Assert.NotNull(result);
            Assert.StartsWith("Account: ", result);
        }

        [Fact]
        public void RenderDataverseTemplate_InvalidLiquidTemplate_Throws_Exception()
        {
            var service = new Context().Service;
            var recordId = GetFirstAccountId();
            var template = "Account: {{ name";

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Templates_RenderDataverseTemplate")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Template", template },
                        { "RecordId", recordId },
                        { "RecordType", "account" }
                    }
                });
            });
        }

        [Fact]
        public void RenderDataverseTemplate_InvalidRecordId_Throws_Exception()
        {
            var service = new Context().Service;
            var recordId = Guid.NewGuid().ToString();
            var template = "Account: {{ name }}";

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Templates_RenderDataverseTemplate")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Template", template },
                        { "RecordId", recordId },
                        { "RecordType", "account" }
                    }
                });
            });
        }
    }
}
