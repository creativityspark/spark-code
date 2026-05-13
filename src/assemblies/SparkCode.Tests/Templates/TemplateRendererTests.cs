using Fluid;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SparkCode.Templates;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Xunit;

namespace SparkCode.Tests.Templates
{
    public class TemplateRendererTests
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

        private static string GetFirstEnvironmentVariableValue()
        {
            var service = new Context().Service;
            var query = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("schemaname", "defaultvalue"),
                TopCount = 250
            };

            var link = query.AddLink(
                "environmentvariablevalue",
                "environmentvariabledefinitionid",
                "environmentvariabledefinitionid",
                JoinOperator.LeftOuter);
            link.Columns = new ColumnSet("value");
            link.EntityAlias = "envVarValue";

            var definitions = service.RetrieveMultiple(query).Entities;

            var definitionWithValue = definitions.FirstOrDefault(entity =>
            {
                var envValue = entity.GetAttributeValue<AliasedValue>("envVarValue.value")?.Value as string;
                var defaultValue = entity.GetAttributeValue<string>("defaultvalue");
                return !string.IsNullOrWhiteSpace(envValue) || !string.IsNullOrWhiteSpace(defaultValue);
            });

            if (definitionWithValue == null)
            {
                throw new InvalidOperationException("No environment variable with a current or default value was found in the test environment.");
            }

            return definitionWithValue.GetAttributeValue<string>("schemaname");
        }

        private static string GetCanvasAppNameForIdentifierTag()
        {
            var service = new Context().Service;
            var query = new QueryExpression("canvasapp")
            {
                ColumnSet = new ColumnSet("name"),
                TopCount = 250
            };

            var appName = service.RetrieveMultiple(query)
                .Entities
                .Select(entity => entity.GetAttributeValue<string>("name"))
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name) && !name.Contains(" "));

            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new InvalidOperationException("No canvas app name without spaces was found for _appUrl tag testing.");
            }

            return appName;
        }

        private static string GetModelDrivenUniqueNameForIdentifierTag()
        {
            var service = new Context().Service;
            var query = new QueryExpression("appmodule")
            {
                ColumnSet = new ColumnSet("uniquename"),
                TopCount = 250
            };

            var uniqueName = service.RetrieveMultiple(query)
                .Entities
                .Select(entity => entity.GetAttributeValue<string>("uniquename"))
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name) && !name.Contains(" "));

            if (string.IsNullOrWhiteSpace(uniqueName))
            {
                throw new InvalidOperationException("No model-driven app unique name without spaces was found for _appUrl tag testing.");
            }

            return uniqueName;
        }

        private static string GetRenderableWebResourceName(IOrganizationService service)
        {
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

        [Fact]
        public void ParseTemplate_WithInvalidLiquid_ThrowsException()
        {
            var parser = new FluidParser();

            Assert.Throws<Exception>(() => TemplateRenderer.Parse("Hello {{name", parser));
        }

        [Fact]
        public void Parse_WithWebResource_ReturnsFrontMatterBodyAndRenderedTemplate()
        {
            var service = new Context().Service;
            var webResourceName = GetRenderableWebResourceName(service);
            var additionalContext = "{\"name\":\"FromContext\"}";
            var expected = GetFrontMatter.Parse(service.GetWebResourceContent(webResourceName));
            var expectedBody = (string)expected["body"];
            var expectedRenderedTemplate = TemplateRenderer.Render(
                service,
                expectedBody,
                additionalContext: additionalContext);

            var result = TemplateRenderer.Parse(
                service,
                webResourceName,
                additionalContext: additionalContext);
            var expectedFrontMatter = (Entity)expected["frontMatter"];
            var actualFrontMatter = (Entity)result["frontMatter"];
            var actualRenderedFrontMatter = (Entity)result["renderedFrontMatter"];

            Assert.Equal(expectedBody, (string)result["body"]);
            Assert.Equal(expectedRenderedTemplate, (string)result["renderedTemplate"]);
            Assert.Equal(expectedFrontMatter.Attributes.Count, actualFrontMatter.Attributes.Count);
            Assert.Equal(expectedFrontMatter.Attributes.Count, actualRenderedFrontMatter.Attributes.Count);

            foreach (var attribute in expectedFrontMatter.Attributes)
            {
                Assert.True(actualFrontMatter.Attributes.Contains(attribute.Key));
                Assert.Equal(attribute.Value?.ToString(), actualFrontMatter[attribute.Key]?.ToString());

                Assert.True(actualRenderedFrontMatter.Attributes.Contains(attribute.Key));
                if (attribute.Value is string frontMatterString)
                {
                    var expectedRenderedValue = TemplateRenderer.Render(
                        service,
                        frontMatterString,
                        additionalContext: additionalContext);
                    Assert.Equal(expectedRenderedValue, actualRenderedFrontMatter[attribute.Key]?.ToString());
                }
                else
                {
                    Assert.Equal(attribute.Value, actualRenderedFrontMatter[attribute.Key]);
                }
            }
        }

        [Fact]
        public void Parse_WithFrontMatterLiquidPlaceholders_ReturnsRenderedFrontMatter()
        {
            var service = new Context().Service;
            var webResourceName = $"csp_/tests/template-renderer-{Guid.NewGuid():N}.html";
            var createdWebResourceId = Guid.Empty;

            try
            {
                var templateSource = string.Join("\n", new[]
                {
                    "---",
                    "title: Hello {{ name }}",
                    "priority: 5",
                    "---",
                    "Body: {{ name }}"
                });

                var webResource = new Entity("webresource")
                {
                    ["name"] = webResourceName,
                    ["displayname"] = webResourceName,
                    ["webresourcetype"] = new OptionSetValue(1),
                    ["content"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(templateSource))
                };
                createdWebResourceId = service.Create(webResource);

                var result = TemplateRenderer.Parse(
                    service,
                    webResourceName,
                    additionalContext: "{\"name\":\"FromContext\"}");

                var originalFrontMatter = (Entity)result["frontMatter"];
                var renderedFrontMatter = (Entity)result["renderedFrontMatter"];

                Assert.Equal("Hello {{ name }}", originalFrontMatter.GetAttributeValue<string>("title"));
                Assert.Equal("Hello FromContext", renderedFrontMatter.GetAttributeValue<string>("title"));
                Assert.Equal(originalFrontMatter["priority"], renderedFrontMatter["priority"]);
                Assert.Equal("Body: FromContext", (string)result["renderedTemplate"]);
            }
            finally
            {
                if (createdWebResourceId != Guid.Empty)
                {
                    service.Delete("webresource", createdWebResourceId);
                }
            }
        }

        [Fact]
        public void Parse_WithMissingWebResource_ThrowsException()
        {
            var service = new Context().Service;

            Assert.ThrowsAny<Exception>(() => TemplateRenderer.Parse(service, "missing_webresource_name_12345"));
        }

        [Fact]
        public void Render_WithJsonValues_RendersTemplate()
        {
            var template = "Hello {{ name }}";
            var jsonValues = "{\"name\":\"Cris\"}";

            var result = TemplateRenderer.Render(template, jsonValues);

            Assert.Equal("Hello Cris", result);
        }

        [Fact]
        public void Render_WithParsedTemplateAndExpandoModel_RendersTemplate()
        {
            var parser = new FluidParser();
            var template = TemplateRenderer.Parse("Hello {{ name }}", parser);
            var model = new ExpandoObject();
            ((IDictionary<string, object>)model)["name"] = "Cris";

            var result = TemplateRenderer.Render(template, model);

            Assert.Equal("Hello Cris", result);
        }

        [Fact]
        public void Render_WithDataverseInputs_RendersTemplate()
        {
            var service = new Context().Service;
            var recordId = GetFirstAccountId();

            var result = TemplateRenderer.Render(
                service,
                "Account: {{ name }}",
                recordId,
                "account");

            Assert.NotNull(result);
            Assert.StartsWith("Account: ", result);
        }

        [Fact]
        public void Render_WithAdditionalContext_MergesAndOverridesRecordValues()
        {
            var service = new Context().Service;
            var recordId = GetFirstAccountId();

            var result = TemplateRenderer.Render(
                service,
                "Account: {{ name }} | Prefix: {{ prefix }}",
                recordId,
                "account",
                "{\"prefix\":\"VIP\",\"name\":\"ABC\"}");

            Assert.Contains("Prefix: VIP", result);
            Assert.Contains("Account: ABC", result);
        }

        [Fact]
        public void Render_WithAdditionalContextOnly_UsesAdditionalContext()
        {
            var service = new Context().Service;

            var result = TemplateRenderer.Render(
                service,
                "Hello {{ name }}",
                additionalContext: "{\"name\":\"FromContext\"}");

            Assert.Equal("Hello FromContext", result);
        }

        [Fact]
        public void Render_WithOnlyRecordType_ThrowsException()
        {
            var service = new Context().Service;

            Assert.Throws<Exception>(() => TemplateRenderer.Render(
                service,
                "Hello {{ name }}",
                recordType: "account"));
        }

        [Fact]
        public void Render_WithOnlyRecordId_ThrowsException()
        {
            var service = new Context().Service;

            Assert.Throws<Exception>(() => TemplateRenderer.Render(
                service,
                "Hello {{ name }}",
                recordIdStr: Guid.NewGuid().ToString()));
        }

        [Fact]
        public void Render_WithInvalidLiquid_ThrowsException()
        {
            var service = new Context().Service;

            Assert.Throws<Exception>(() => TemplateRenderer.Render(
                service,
                "Account: {{ name"));
        }

        [Fact]
        public void BuildDataverseModel_WithAdditionalContext_MergesAndOverridesValues()
        {
            var service = new Context().Service;
            var recordId = GetFirstAccountId();

            var model = TemplateRenderer.BuildModel(
                service,
                "account",
                recordId,
                "{\"name\":\"Overridden Name\",\"prefix\":\"VIP\"}",
                new[] { "name", "prefix", "new_notrealattribute" });

            var modelDictionary = (IDictionary<string, object>)model;

            Assert.Equal("Overridden Name", modelDictionary["name"]?.ToString());
            Assert.Equal("VIP", modelDictionary["prefix"]?.ToString());
            Assert.False(modelDictionary.ContainsKey("new_notrealattribute"));
        }

        [Fact]
        public void BuildDataverseModel_WithoutRecordInputs_UsesOnlyAdditionalContext()
        {
            var service = new Context().Service;

            var model = TemplateRenderer.BuildModel(
                service,
                additionalContext: "{\"prefix\":\"VIP\",\"region\":\"NA\"}");

            var modelDictionary = (IDictionary<string, object>)model;

            Assert.Equal(2, modelDictionary.Count);
            Assert.Equal("VIP", modelDictionary["prefix"]?.ToString());
            Assert.Equal("NA", modelDictionary["region"]?.ToString());
        }

        [Fact]
        public void BuildDataverseModel_WithoutRecordInputsAndAdditionalContext_ReturnsEmptyObject()
        {
            var service = new Context().Service;

            var model = TemplateRenderer.BuildModel(service);

            var modelDictionary = (IDictionary<string, object>)model;

            Assert.Empty(modelDictionary);
        }

        [Fact]
        public void BuildDataverseModel_WithRecordTypeOnly_ThrowsException()
        {
            var service = new Context().Service;

            Assert.Throws<Exception>(() => TemplateRenderer.BuildModel(service, recordType: "account"));
        }

        [Fact]
        public void BuildDataverseModel_WithRecordIdOnly_ThrowsException()
        {
            var service = new Context().Service;
            var recordId = GetFirstAccountId();

            Assert.Throws<Exception>(() => TemplateRenderer.BuildModel(service, recordIdStr: recordId));
        }

        [Fact]
        public void RegisterCustomTags_WithEnvVar_RendersEnvironmentVariableValue()
        {
            var service = new Context().Service;
            var envVarSchemaName = GetFirstEnvironmentVariableValue();
            var expectedValue = service.GetEnvironmentVariableValue(envVarSchemaName);

            var parser = new FluidParser();
            TemplateRenderer.RegisterCustomTags(parser, service);
            var template = TemplateRenderer.Parse($"{{% _envVar {envVarSchemaName} %}}", parser);

            var model = new ExpandoObject();
            var result = TemplateRenderer.Render(template, model);

            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void RegisterCustomTags_WithOrgDetails_RendersOrganizationDetail()
        {
            var service = new Context().Service;
            var expectedValue = service.GetOrganizationDetails("FriendlyName");

            var parser = new FluidParser();
            TemplateRenderer.RegisterCustomTags(parser, service);
            var template = TemplateRenderer.Parse("{% _orgDetails FriendlyName %}", parser);

            var model = new ExpandoObject();
            var result = TemplateRenderer.Render(template, model);

            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void RegisterCustomTags_WithOrgUrl_RendersOrganizationUrls()
        {
            var service = new Context().Service;
            var expectedUrl = service.GetOrganizationUrl(null);

            var parser = new FluidParser();
            TemplateRenderer.RegisterCustomTags(parser, service);

            var template = TemplateRenderer.Parse("{% _orgUrl %}", parser);

            var model = new ExpandoObject();
            var result = TemplateRenderer.Render(template, model);

            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void RegisterCustomTags_WithAppUrlIdentifier_UsesCanvasAppUrlWhenPresent()
        {
            var service = new Context().Service;
            var appName = GetCanvasAppNameForIdentifierTag();
            var expectedUrl = service.GetCanvasAppAttribute<string>(appName, "appopenuri") ?? string.Empty;

            var parser = new FluidParser();
            TemplateRenderer.RegisterCustomTags(parser, service);
            var template = TemplateRenderer.Parse($"{{% _appUrl {appName} %}}", parser);

            var model = new ExpandoObject();
            var result = TemplateRenderer.Render(template, model);

            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void RegisterCustomTags_WithAppUrlIdentifier_FallsBackToModelDrivenAppOpenUri()
        {
            var service = new Context().Service;
            var appUniqueName = GetModelDrivenUniqueNameForIdentifierTag();
            var environmentUrl = service.GetOrganizationUrl(null)?.TrimEnd('/');
            var appId = service.GetMDAAttribute<Guid>(appUniqueName, "appmoduleid");
            var expectedUrl = $"{environmentUrl}/main.aspx?appid={appId.ToString()}";

            var parser = new FluidParser();
            TemplateRenderer.RegisterCustomTags(parser, service);
            var template = TemplateRenderer.Parse($"{{% _appUrl {appUniqueName} %}}", parser);

            var model = new ExpandoObject();
            var result = TemplateRenderer.Render(template, model);

            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void RegisterCustomTags_WithAppNameIdentifier_UsesCanvasNameWhenPresent()
        {
            var service = new Context().Service;
            var appNameIdentifier = GetCanvasAppNameForIdentifierTag();
            var expectedName = service.GetCanvasAppAttribute<string>(appNameIdentifier, "name") ?? string.Empty;

            var parser = new FluidParser();
            TemplateRenderer.RegisterCustomTags(parser, service);
            var template = TemplateRenderer.Parse($"{{% _appName {appNameIdentifier} %}}", parser);

            var model = new ExpandoObject();
            var result = TemplateRenderer.Render(template, model);

            Assert.Equal(expectedName, result);
        }

        [Fact]
        public void RegisterCustomTags_WithAppNameIdentifier_FallsBackToModelDrivenDisplayName()
        {
            var service = new Context().Service;
            var appUniqueName = GetModelDrivenUniqueNameForIdentifierTag();
            var expectedName = service.GetMDAAttribute<string>(appUniqueName, "displayname") ?? string.Empty;

            var parser = new FluidParser();
            TemplateRenderer.RegisterCustomTags(parser, service);
            var template = TemplateRenderer.Parse($"{{% _appName {appUniqueName} %}}", parser);

            var model = new ExpandoObject();
            var result = TemplateRenderer.Render(template, model);

            Assert.Equal(expectedName, result);
        }

        [Fact]
        public void RegisterCustomTags_WithWebResourceIdentifier_ReturnsRawContentWithoutDecoding()
        {
            var service = new Context().Service;
            var webResourceName = $"csp_/testwebresource_{Guid.NewGuid():N}.html";
            var decodedContent = "Hello from _webResource tag";
            var encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(decodedContent));
            var createdWebResourceId = Guid.Empty;

            try
            {
                var webResource = new Entity("webresource")
                {
                    ["name"] = webResourceName,
                    ["displayname"] = webResourceName,
                    ["webresourcetype"] = new OptionSetValue(1),
                    ["content"] = encodedContent
                };
                createdWebResourceId = service.Create(webResource);

                var parser = new FluidParser();
                TemplateRenderer.RegisterCustomTags(parser, service);
                var template = TemplateRenderer.Parse($"{{% _webResource '{webResourceName}' %}}", parser);

                var model = new ExpandoObject();
                var result = TemplateRenderer.Render(template, model);

                Assert.Equal(encodedContent, result);
                Assert.NotEqual(decodedContent, result);
            }
            finally
            {
                if (createdWebResourceId != Guid.Empty)
                {
                    service.Delete("webresource", createdWebResourceId);
                }
            }
        }

        [Fact]
        public void RegisterCustomTags_WithMissingWebResourceIdentifier_ThrowsException()
        {
            var service = new Context().Service;
            var parser = new FluidParser();
            TemplateRenderer.RegisterCustomTags(parser, service);
            var template = TemplateRenderer.Parse("{% _webResource \"csp_/missing_webresource_name_12345.ext\" %}", parser);

            var model = new ExpandoObject();

            Assert.ThrowsAny<Exception>(() => TemplateRenderer.Render(template, model));
        }
    }
}