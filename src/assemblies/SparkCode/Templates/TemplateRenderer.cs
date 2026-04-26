using Fluid;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace SparkCode.Templates
{
    public static class TemplateRenderer
    {
        public static string Render(string templateSource, string jsonValues)
        {
            var model = JsonConvert.DeserializeObject<ExpandoObject>(jsonValues);
            return Render(templateSource, model);
        }

        public static string Render(string templateSource, ExpandoObject model)
        {
            var template = ParseTemplate(templateSource);
            return Render(template, model);
        }

        public static IFluidTemplate ParseTemplate(string templateSource)
        {
            var parser = new FluidParser();
            if (parser.TryParse(templateSource, out IFluidTemplate template, out string errorMessage))
            {
                return template;
            }

            throw new InvalidPluginExecutionException($"Invalid Liquid template: {errorMessage}");
        }

        public static string Render(IFluidTemplate template, ExpandoObject model)
        {
            var templateContext = new TemplateContext(model);
            return template.Render(templateContext);
        }

        public static ExpandoObject BuildDataverseModel(IOrganizationService service, string recordType, string recordIdStr, string additionalContext, IFluidTemplate parsedTemplate)
        {
            var additionalValuesDictionary = string.IsNullOrWhiteSpace(additionalContext)
                ? new Dictionary<string, object>()
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(additionalContext) ?? new Dictionary<string, object>();

            var visitor = new IdentifierVisitor();
            visitor.VisitTemplate(parsedTemplate);

            var filteredIdentifiers = new HashSet<string>(
                visitor.Identifiers.Where(id => !additionalValuesDictionary.ContainsKey(id))
            );
            var identifiers = filteredIdentifiers.ToArray();

            var recordId = new Guid(recordIdStr);
            var columnSet = identifiers.Length > 0
                ? new ColumnSet(identifiers)
                : new ColumnSet(false);

            var record = service.Retrieve(recordType, recordId, columnSet);
            var model = JsonConvert.DeserializeObject<ExpandoObject>(record.ToJson());
            var modelDictionary = (IDictionary<string, object>)model;

            foreach (var kvp in additionalValuesDictionary)
            {
                modelDictionary[kvp.Key] = kvp.Value;
            }

            return model;
        }
    }
}