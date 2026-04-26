using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.Templates
{
    public class RenderTemplateTests
    {
        [Fact]
        public void RenderTemplate_ValidTemplate_Returns_RenderedText()
        {
            var service = new Context().Service;
            var template = "<ul>{%- for person in people -%}<li><a href=\"{{ person | prepend: \"https://example.com/\" }}\">{{ person | capitalize }}</a></li>{%- endfor -%}</ul>";
            var context = "{\"people\":[\"alice\",\"bob\",\"carol\"]}";

            var output = service.Execute(new OrganizationRequest("csp_Templates_RenderTemplate")
            {
                Parameters = new ParameterCollection
                {
                    { "Template", template },
                    { "Context", context }
                }
            });

            Assert.True(output.Results.Contains("Results"), "Expected output parameter 'Results' was not returned.");

            var expected = "<ul><li><a href=\"https://example.com/alice\">Alice</a></li><li><a href=\"https://example.com/bob\">Bob</a></li><li><a href=\"https://example.com/carol\">Carol</a></li></ul>";
            Assert.Equal(expected, (string)output["Results"]);
        }

        [Fact]
        public void RenderTemplate_InvalidLiquidTemplate_Throws_Exception()
        {
            var service = new Context().Service;
            var template = "Hello {{name";
            var context = "{\"name\":\"Cris\"}";

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Templates_RenderTemplate")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Template", template },
                        { "Context", context }
                    }
                });
            });
        }

        [Fact]
        public void RenderTemplate_InvalidJsonContext_Throws_Exception()
        {
            var service = new Context().Service;
            var template = "Hello {{name}}";
            var context = "{\"name\":\"Cris\"";

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Templates_RenderTemplate")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Template", template },
                        { "Context", context }
                    }
                });
            });
        }
    }
}
