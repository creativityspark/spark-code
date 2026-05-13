using Fluid;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SparkCode.Templates;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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

        [Fact]
        public void ParseTemplate_WithInvalidLiquid_ThrowsException()
        {
            var parser = new FluidParser();

            Assert.Throws<Exception>(() => TemplateRenderer.Parse("Hello {{name", parser));
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
    }
}