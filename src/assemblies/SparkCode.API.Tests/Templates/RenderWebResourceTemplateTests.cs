using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Xunit;

namespace SparkCode.API.Tests.Templates
{
    public class RenderWebResourceTemplateTests
    {
        [Fact]
        public void RenderWebResourceTemplate_ValidWebResource_Returns_Results_And_FrontMatter_Entity()
        {
            var service = new Context().Service;
            var webResourceName = GetRenderableWebResourceName(service);

            var output = service.Execute(new OrganizationRequest("csp_Templates_RenderWebResourceTemplate")
            {
                Parameters = new ParameterCollection
                {
                    { "WebResourceName", webResourceName },
                    { "AdditionalContext", "{\"name\":\"FromContext\"}" }
                }
            });

            Assert.True(output.Results.Contains("Results"), "Expected output parameter 'Results' was not returned.");
            Assert.True(output.Results.Contains("FrontMatter"), "Expected output parameter 'FrontMatter' was not returned.");

            var results = output["Results"] as string;
            Assert.NotNull(results);

            var frontMatter = output["FrontMatter"] as Entity;
            Assert.NotNull(frontMatter);
        }

        [Fact]
        public void RenderWebResourceTemplateJson_ValidWebResource_Returns_Results_And_FrontMatterJson_String()
        {
            var service = new Context().Service;
            var webResourceName = GetRenderableWebResourceName(service);

            var output = service.Execute(new OrganizationRequest("csp_Templates_RenderWebResourceTemplateJson")
            {
                Parameters = new ParameterCollection
                {
                    { "WebResourceName", webResourceName },
                    { "AdditionalContext", "{\"name\":\"FromContext\"}" }
                }
            });

            Assert.True(output.Results.Contains("Results"), "Expected output parameter 'Results' was not returned.");
            Assert.True(output.Results.Contains("FrontMatterJson"), "Expected output parameter 'FrontMatterJson' was not returned.");

            var results = output["Results"] as string;
            Assert.NotNull(results);

            var frontMatterJson = output["FrontMatterJson"] as string;
            Assert.NotNull(frontMatterJson);
            Assert.NotNull(JToken.Parse(frontMatterJson));
        }

        [Fact]
        public void RenderWebResourceTemplate_InvalidWebResource_Throws_Exception()
        {
            var service = new Context().Service;

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Templates_RenderWebResourceTemplate")
                {
                    Parameters = new ParameterCollection
                    {
                        { "WebResourceName", "missing_webresource_name_12345" }
                    }
                });
            });
        }

        [Fact]
        public void RenderWebResourceTemplate_WithOnlyRecordId_Throws_Exception()
        {
            var service = new Context().Service;
            var webResourceName = GetRenderableWebResourceName(service);

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Templates_RenderWebResourceTemplate")
                {
                    Parameters = new ParameterCollection
                    {
                        { "WebResourceName", webResourceName },
                        { "RecordId", Guid.NewGuid().ToString() }
                    }
                });
            });
        }

        private static string GetRenderableWebResourceName(IOrganizationService service)
        {
            // webresourcetype 1 = HTML; filter server-side and decode content client-side to find a Liquid template.
            var query = new QueryExpression("webresource")
            {
                ColumnSet = new ColumnSet("name", "content"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.NotNull),
                        new ConditionExpression("content", ConditionOperator.NotNull),
                        new ConditionExpression("webresourcetype", ConditionOperator.Equal, 1)
                    }
                },
                TopCount = 50
            };

            var webResources = service.RetrieveMultiple(query);

            foreach (var webResource in webResources.Entities)
            {
                var candidateName = webResource.GetAttributeValue<string>("name");
                var encodedContent = webResource.GetAttributeValue<string>("content");

                if (string.IsNullOrWhiteSpace(candidateName) || string.IsNullOrWhiteSpace(encodedContent))
                {
                    continue;
                }

                var decodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(encodedContent));
                if (decodedContent.Contains("{{"))
                {
                    return candidateName;
                }
            }

            throw new InvalidOperationException("No HTML web resource containing a Liquid template (with '{{') was found in the test environment.");
        }
    }
}