using Microsoft.Xrm.Sdk;
using PdfTestData = SparkCode.Pdf.TestSupport.Pdf;
using PdfTestPayload = SparkCode.Pdf.TestSupport.PdfImageTestPayload;
using System;
using System.Drawing.Imaging;
using Xunit;

namespace SparkCode.API.Tests.Pdf
{
    public class ExtractImagesTests
    {
        [Fact]
        public void ExtractImages_SamplePdfFile_ReturnsJpegStringArray()
        {
            PdfTestPayload payload = PdfTestData.LoadSamplePdf();
            var service = new Context().Service;

            OrganizationResponse response = service.Execute(new OrganizationRequest("csp_Pdf_ExtractImages")
            {
                Parameters = new ParameterCollection
                {
                    { "PdfBase64", payload.PdfBase64 }
                }
            });

            string[] images = Assert.IsType<string[]>(response["Images"]);
            Assert.NotEmpty(images);
            Assert.All(images, image =>
            {
                byte[] imageBytes = Convert.FromBase64String(image);
                Assert.True(imageBytes.Length >= 3);
                Assert.Equal(0xFF, imageBytes[0]);
                Assert.Equal(0xD8, imageBytes[1]);
                Assert.Equal(0xFF, imageBytes[2]);
            });
        }

        [Fact]
        public void ExtractImages_JpegAndPngImages_ReturnsJpegStringArray()
        {
            PdfTestPayload payload = PdfTestData.Create(ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Jpeg);
            var service = new Context().Service;

            OrganizationResponse response = service.Execute(new OrganizationRequest("csp_Pdf_ExtractImages")
            {
                Parameters = new ParameterCollection
                {
                    { "PdfBase64", payload.PdfBase64 }
                }
            });

            Assert.True(response.Results.Contains("Images"), "Expected output parameter 'Images' was not returned.");
            Assert.Equal(payload.JpegImagesBase64, Assert.IsType<string[]>(response["Images"]));
        }

        [Fact]
        public void ExtractImages_PdfWithoutImages_ReturnsEmptyArray()
        {
            PdfTestPayload payload = PdfTestData.CreateWithoutImages();
            var service = new Context().Service;

            OrganizationResponse response = service.Execute(new OrganizationRequest("csp_Pdf_ExtractImages")
            {
                Parameters = new ParameterCollection
                {
                    { "PdfBase64", payload.PdfBase64 }
                }
            });

            Assert.Empty(Assert.IsType<string[]>(response["Images"]));
        }

        [Fact]
        public void ExtractImages_InvalidBase64_ThrowsClearError()
        {
            var service = new Context().Service;

            Exception exception = Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Pdf_ExtractImages")
                {
                    Parameters = new ParameterCollection
                    {
                        { "PdfBase64", "not base64!" }
                    }
                });
            });

            Assert.Contains("valid Base64", exception.ToString());
        }
    }
}
