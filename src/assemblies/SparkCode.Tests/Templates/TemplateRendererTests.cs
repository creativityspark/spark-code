using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SparkCode.Templates;
using System;
using System.Collections.Generic;
using System.Dynamic;
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

        [Fact]
        public void ParseTemplate_WithInvalidLiquid_ThrowsInvalidPluginExecutionException()
        {
            Assert.Throws<InvalidPluginExecutionException>(() => TemplateRenderer.ParseTemplate("Hello {{name"));
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
            var template = TemplateRenderer.ParseTemplate("Hello {{ name }}");
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
    }
}