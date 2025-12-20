using Microsoft.Xrm.Sdk;
using Xunit;

namespace SparkCode.API.Tests.Text
{
    public class ReplaceParamsTests
    {
        [Fact]
        public void ReplaceParams_AllParams_ReplacesAllPlaceholders()
        {
            var service = Context.GetService();
            var text = "{{param1}}-{{param2}}-{{param3}}-{{param4}}-{{param5}}-{{param6}}-{{param7}}-{{param8}}-{{param9}}";
            var output = service.Execute(new OrganizationRequest("csp_Text.ReplaceParams")
            {
                Parameters = new ParameterCollection
                {
                    { "Text", text },
                    { "Param1", "A" },
                    { "Param2", "B" },
                    { "Param3", "C" },
                    { "Param4", "D" },
                    { "Param5", "E" },
                    { "Param6", "F" },
                    { "Param7", "G" },
                    { "Param8", "H" },
                    { "Param9", "I" }
                }
            });
            var result = (string)output["Results"];
            Assert.Equal("A-B-C-D-E-F-G-H-I", result);
        }
    }
}
