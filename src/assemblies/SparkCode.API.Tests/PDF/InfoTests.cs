using Microsoft.Xrm.Sdk;
using System;
using System.Text.Json;
using Xunit;
using PdfTestDocument = SparkCode.Pdf.TestSupport.Pdf;
using PdfImageTestPayload = SparkCode.Pdf.TestSupport.PdfImageTestPayload;

namespace SparkCode.API.Tests.Pdf
{
    public class InfoTests
    {
        [Fact]
        public void Info_SamplePdfFile_ReturnsExpandoAndResultsJson()
        {
            PdfImageTestPayload payload = PdfTestDocument.LoadSamplePdf();
            var service = new Context().Service;

            OrganizationResponse expandoResponse = service.Execute(new OrganizationRequest("csp_Pdf_Info")
            {
                Parameters = new ParameterCollection
                {
                    { "InputPdf", payload.PdfBase64 }
                }
            });

            Entity results = Assert.IsType<Entity>(expandoResponse["Results"]);
            Assert.True((int)results["Pages"] > 0);
            Assert.Contains(".", (string)results["Version"]);

            OrganizationResponse jsonResponse = service.Execute(new OrganizationRequest("csp_Pdf_InfoJson")
            {
                Parameters = new ParameterCollection
                {
                    { "InputPdf", payload.PdfBase64 }
                }
            });

            string resultsJson = Assert.IsType<string>(jsonResponse["ResultsJson"]);
            using (JsonDocument json = JsonDocument.Parse(resultsJson))
            {
                Assert.Equal((int)results["Pages"], json.RootElement.GetProperty("Pages").GetInt32());
                Assert.Equal((string)results["Version"], json.RootElement.GetProperty("Version").GetString());
            }
        }

        [Fact]
        public void Info_InvalidBase64_ThrowsClearError()
        {
            var service = new Context().Service;

            Exception exception = Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Pdf_Info")
                {
                    Parameters = new ParameterCollection
                    {
                        { "InputPdf", "not base64!" }
                    }
                });
            });

            Assert.Contains("valid Base64", exception.ToString());
        }
    }
}
