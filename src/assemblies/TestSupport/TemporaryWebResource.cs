using Microsoft.Xrm.Sdk;
using System;
using System.Text;

namespace SparkCode.TestSupport
{
    // Creates an HTML web resource holding a Liquid template and deletes it on dispose,
    // so tests do not depend on pre-existing web resources in the environment.
    internal sealed class TemporaryWebResource : IDisposable
    {
        private const string TemplateSource = "---\ntitle: Hello {{ name }}\npriority: 5\n---\nBody: {{ name }}";

        private readonly IOrganizationService _service;

        public Guid Id { get; }
        public string Name { get; }
        public string Content { get; }

        private TemporaryWebResource(IOrganizationService service, Guid id, string name, string content)
        {
            _service = service;
            Id = id;
            Name = name;
            Content = content;
        }

        public static TemporaryWebResource Create(IOrganizationService service)
        {
            var name = $"csp_/tests/temporary-{Guid.NewGuid():N}.html";
            var content = Convert.ToBase64String(Encoding.UTF8.GetBytes(TemplateSource));
            var id = service.Create(new Entity("webresource")
            {
                ["name"] = name,
                ["displayname"] = name,
                ["webresourcetype"] = new OptionSetValue(1),
                ["content"] = content
            });

            return new TemporaryWebResource(service, id, name, content);
        }

        public void Dispose()
        {
            _service.Delete("webresource", Id);
        }
    }
}
