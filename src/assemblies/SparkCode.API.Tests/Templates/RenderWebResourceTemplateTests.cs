using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using SparkCode.TestSupport;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SparkCode.API.Tests.Templates
{
    public class RenderWebResourceTemplateTests : IDisposable
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

        private readonly List<TemporaryWebResource> _temporaryWebResources = new List<TemporaryWebResource>();

        public void Dispose()
        {
            foreach (var webResource in _temporaryWebResources)
            {
                webResource.Dispose();
            }
        }

        private string GetRenderableWebResourceName(IOrganizationService service)
        {
            var webResource = TemporaryWebResource.Create(service);
            _temporaryWebResources.Add(webResource);
            return webResource.Name;
        }
    }
}