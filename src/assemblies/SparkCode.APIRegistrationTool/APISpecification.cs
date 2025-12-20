using Microsoft.Xrm.Sdk;
using System.Xml.Serialization;

namespace SparkCode.APIRegistrationTool
{
    [XmlRoot("doc")]
    public class APISpecification
    {
        [XmlElement("assembly")]
        public AssemblyInfo Assembly { get; set; }

        [XmlArray("members")]
        [XmlArrayItem("member")]
        public List<Member> Members { get; set; }
    }

    public class AssemblyInfo
    {
        [XmlElement("name")]
        public string Name { get; set; }
    }

    public class Member
    {
        [XmlAttribute("name")]
        public string TypeName { get; set; }

        public string UniqueName { get; set; }
        
        public string Name { get; set; }

        [XmlElement("displayName")]
        public string DisplayName { get; set; }

        [XmlElement("summary")]
        public string Description { get; set; }

        public bool EnabledForWorkflow { get; set; }

        [XmlElement("param")]
        public List<Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        public string UniqueName { get; set; }

        [XmlAttribute("displayName")]
        public string DisplayName { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        public OptionSetValue TypeValue { get; set; }

        [XmlAttribute("direction")]
        public string Direction { get; set; }

        [XmlText]
        public string Description { get; set; }

        [XmlAttribute("optional")]
        public bool IsOptional { get; set; }
    }
}
