using System;

namespace SparkCode.API.Tests.Dataverse
{
    using Microsoft.Xrm.Sdk;
    using Xunit;

    public class CountFetchXmlTests
    {
        [Fact]
        public void CountFetchXml_ValidQuery_Returns_Count()
        {
            var service = new Context().Service;
            var fetchXml = "<fetch><entity name='account'><attribute name='accountid' /></entity></fetch>";
            var output = service.Execute(new OrganizationRequest("csp_Dataverse_CountFetchXml")
            {
                Parameters = new ParameterCollection
                {
                    { "FetchXml", fetchXml }
                }
            });
            var count = (int)output["Count"];
            Assert.True(count >= 0);
        }

        [Fact]
        public void CountFetchXml_QueryWithTop_Returns_CountUpToTop()
        {
            var service = new Context().Service;
            var fetchXml = "<fetch top='5'><entity name='account'><attribute name='accountid' /></entity></fetch>";
            var output = service.Execute(new OrganizationRequest("csp_Dataverse_CountFetchXml")
            {
                Parameters = new ParameterCollection
                {
                    { "FetchXml", fetchXml }
                }
            });
            var count = (int)output["Count"];
            Assert.True(count <= 5);
        }

        [Fact]
        public void CountFetchXml_UsuarioActivoPorCata_Returns_Count()
        {
            var service = new Context().Service;
            var fetchXml = @"<fetch>
  <entity name='blc_cata_usuario'>
    <attribute name='blc_cata_usuarioid' />
    <filter type='and'>
      <condition attribute='blc_cata_id' operator='eq' value='{b7a760a5-334c-f011-877a-6045bd9581c4}' />
      <condition attribute='statuscode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";
            var output = service.Execute(new OrganizationRequest("csp_Dataverse_CountFetchXml")
            {
                Parameters = new ParameterCollection
                {
                    { "FetchXml", fetchXml }
                }
            });
            var count = (int)output["Count"];
            Assert.True(count >= 0);
        }
    }
}
