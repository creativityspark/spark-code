using Microsoft.Xrm.Sdk;
using PdfSharp.Pdf.IO;
using SparkCode.Pdf.TestSupport;
using System;
using System.IO;
using Xunit;

namespace SparkCode.API.Tests.Pdf
{
    public class ExtractPagesTests
    {
        [Fact]
        public void ExtractPages_SamplePdfFile_ReturnsValidSinglePagePdf()
        {
            PdfImageTestPayload sample = PdfTestDocument.LoadSamplePdf();
            OrganizationResponse response = ExecuteExtractPages(sample.PdfBase64, 1, 1);

            string results = Assert.IsType<string>(response["Results"]);

            Assert.Equal(1, GetPageCount(results));
        }

        [Fact]
        public void ExtractPages_InclusivePageRange_ReturnsRequestedPagesInOrder()
        {
            string inputPdf = PdfTestDocument.CreatePageRangeTestPdf(4);

            OrganizationResponse response = ExecuteExtractPages(inputPdf, 2, 3);

            using (var document = PdfReader.Open(
                new MemoryStream(Convert.FromBase64String(Assert.IsType<string>(response["Results"]))),
                PdfDocumentOpenMode.ReadOnly))
            {
                Assert.Equal(2, document.PageCount);
                Assert.Equal(102, document.Pages[0].Width.Point);
                Assert.Equal(103, document.Pages[1].Width.Point);
            }
        }

        [Fact]
        public void ExtractPages_InvalidBase64_ThrowsClearError()
        {
            Exception exception = Assert.ThrowsAny<Exception>(
                () => ExecuteExtractPages("not base64!", 1, 1));

            Assert.Contains("valid Base64", exception.ToString());
        }

        [Fact]
        public void ExtractPages_InvalidPageRange_ThrowsClearError()
        {
            string inputPdf = PdfTestDocument.CreatePageRangeTestPdf(3);

            Exception exception = Assert.ThrowsAny<Exception>(
                () => ExecuteExtractPages(inputPdf, 0, 1));

            Assert.Contains("PageFrom must be a 1-based page number", exception.ToString());
        }

        private static OrganizationResponse ExecuteExtractPages(string inputPdf, int pageFrom, int pageTo)
        {
            var service = new Context().Service;
            return service.Execute(new OrganizationRequest("csp_Pdf_ExtractPages")
            {
                Parameters = new ParameterCollection
                {
                    { "InputPdf", inputPdf },
                    { "PageFrom", pageFrom },
                    { "PageTo", pageTo }
                }
            });
        }

        private static int GetPageCount(string pdfBase64)
        {
            using (var document = PdfReader.Open(
                new MemoryStream(Convert.FromBase64String(pdfBase64)),
                PdfDocumentOpenMode.ReadOnly))
            {
                return document.PageCount;
            }
        }
    }
}
