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

        [Fact]
        public void ParseTemplate_WithInvalidLiquid_ThrowsInvalidPluginExecutionException()
        {
            var parser = new FluidParser();

            Assert.Throws<InvalidPluginExecutionException>(() => TemplateRenderer.ParseTemplate("Hello {{name", parser));
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
            var template = TemplateRenderer.ParseTemplate("Hello {{ name }}", parser);
            var model = new ExpandoObject();
            ((IDictionary<string, object>)model)["name"] = "Cris";

            var result = TemplateRenderer.Render(template, model);

            Assert.Equal("Hello Cris", result);
        }

        [Fact]
        public void BuildDataverseModel_WithAdditionalContext_MergesAndOverridesValues()
        {
            var service = new Context().Service;
            var recordId = GetFirstAccountId();

            var model = TemplateRenderer.BuildDataverseModel(
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
        public void RegisterCustomTags_WithEnvVar_RendersEnvironmentVariableValue()
        {
            var service = new Context().Service;
            var envVarSchemaName = GetFirstEnvironmentVariableValue();
            var expectedValue = service.GetEnvironmentVariableValue(envVarSchemaName);

            var parser = new FluidParser();
            TemplateRenderer.RegisterCustomTags(parser, service);
            var template = TemplateRenderer.ParseTemplate($"{{% _envVar {envVarSchemaName} %}}", parser);

            var model = new ExpandoObject();
            var result = TemplateRenderer.Render(template, model);

            Assert.Equal(expectedValue, result);
        }
    }
}