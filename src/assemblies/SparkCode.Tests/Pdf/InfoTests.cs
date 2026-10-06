using Microsoft.Xrm.Sdk;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.IO;
using System.Text;
using Xunit;
using PdfTestDocument = SparkCode.Pdf.TestSupport.Pdf;
using PdfImageTestPayload = SparkCode.Pdf.TestSupport.PdfImageTestPayload;

namespace SparkCode.Tests.Pdf
{
    public class InfoTests
    {
        [Fact]
        public void Extract_SamplePdfFile_ReturnsPageCountVersionAndAvailableMetadata()
        {
            PdfImageTestPayload payload = PdfTestDocument.LoadSamplePdf();
            Entity results = SparkCode.Pdf.Info.Extract(payload.PdfBase64);

            using (var stream = new MemoryStream(Convert.FromBase64String(payload.PdfBase64)))
            using (PdfDocument document = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly))
            {
                Assert.Equal(document.PageCount, (int)results["Pages"]);
                Assert.Equal(FormatPdfVersion(document.Version), (string)results["Version"]);
                AssertTextMetadata(results, "Title", document.Info.Title);
                AssertTextMetadata(results, "Author", document.Info.Author);
                AssertTextMetadata(results, "Subject", document.Info.Subject);
                AssertTextMetadata(results, "Keywords", document.Info.Keywords);
                AssertTextMetadata(results, "Creator", document.Info.Creator);
                AssertTextMetadata(results, "Producer", document.Info.Producer);
                AssertDateMetadata(results, "Created", document.Info, "/CreationDate", document.Info.CreationDate);
                AssertDateMetadata(results, "Modified", document.Info, "/ModDate", document.Info.ModificationDate);
            }
        }

        [Fact]
        public void Extract_EmptyInput_ThrowsClearArgumentException()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => SparkCode.Pdf.Info.Extract(string.Empty));

            Assert.Contains("required", exception.Message);
        }

        [Fact]
        public void Extract_InvalidBase64_ThrowsClearArgumentException()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => SparkCode.Pdf.Info.Extract("not base64!"));

            Assert.Contains("valid Base64", exception.Message);
        }

        [Fact]
        public void Extract_InvalidPdf_ThrowsClearArgumentException()
        {
            string invalidPdf = Convert.ToBase64String(Encoding.UTF8.GetBytes("not a PDF"));

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => SparkCode.Pdf.Info.Extract(invalidPdf));

            Assert.Contains("valid PDF", exception.Message);
        }

        private static string FormatPdfVersion(int version)
        {
            return (version / 10) + "." + (version % 10);
        }

        private static void AssertTextMetadata(Entity results, string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Assert.False(results.Contains(name));
            }
            else
            {
                Assert.Equal(value, (string)results[name]);
            }
        }

        private static void AssertDateMetadata(
            Entity results,
            string name,
            PdfDocumentInformation info,
            string elementName,
            DateTime value)
        {
            if (info.Elements[elementName] == null || value == DateTime.MinValue)
            {
                Assert.False(results.Contains(name));
            }
            else
            {
                Assert.Equal(value, (DateTime)results[name]);
            }
        }
    }
}
