using PdfSharp.Pdf.IO;
using PdfTestDocument = SparkCode.Pdf.TestSupport.Pdf;
using PdfImageTestPayload = SparkCode.Pdf.TestSupport.PdfImageTestPayload;
using System;
using System.IO;
using Xunit;

namespace SparkCode.Tests.Pdf
{
    public class ExtractPagesTests
    {
        [Fact]
        public void Extract_SamplePdfFile_ReturnsValidSinglePagePdf()
        {
            PdfImageTestPayload sample = PdfTestDocument.LoadSamplePdf();

            string result = SparkCode.PDF.ExtractPages.Extract(sample.PdfBase64, 1, 1);

            Assert.Equal(1, GetPageCount(result));
        }

        [Fact]
        public void Extract_InclusivePageRange_ReturnsRequestedPagesInOrder()
        {
            string inputPdf = PdfTestDocument.CreatePageRangeTestPdf(4);

            string result = SparkCode.PDF.ExtractPages.Extract(inputPdf, 2, 3);

            using (var document = PdfReader.Open(
                new MemoryStream(Convert.FromBase64String(result)),
                PdfDocumentOpenMode.ReadOnly))
            {
                Assert.Equal(2, document.PageCount);
                Assert.Equal(102, document.Pages[0].Width.Point);
                Assert.Equal(103, document.Pages[1].Width.Point);
            }
        }

        [Fact]
        public void Extract_EmptyInput_ThrowsClearArgumentException()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => SparkCode.PDF.ExtractPages.Extract(string.Empty, 1, 1));

            Assert.Contains("required", exception.Message);
        }

        [Fact]
        public void Extract_InvalidBase64_ThrowsClearArgumentException()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => SparkCode.PDF.ExtractPages.Extract("not base64!", 1, 1));

            Assert.Contains("valid Base64", exception.Message);
        }

        [Fact]
        public void Extract_InvalidPdf_ThrowsClearArgumentException()
        {
            string invalidPdf = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("not a PDF"));

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => SparkCode.PDF.ExtractPages.Extract(invalidPdf, 1, 1));

            Assert.Contains("valid PDF", exception.Message);
        }

        [Theory]
        [InlineData(0, 1, "pageFrom")]
        [InlineData(1, 0, "pageTo")]
        [InlineData(1, 4, "pageTo")]
        public void Extract_InvalidPageRange_ThrowsClearArgumentOutOfRangeException(
            int pageFrom,
            int pageTo,
            string parameterName)
        {
            string inputPdf = PdfTestDocument.CreatePageRangeTestPdf(3);

            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => SparkCode.PDF.ExtractPages.Extract(inputPdf, pageFrom, pageTo));

            Assert.Equal(parameterName, exception.ParamName);
        }

        [Fact]
        public void Extract_ReversedPageRange_ThrowsClearArgumentException()
        {
            string inputPdf = PdfTestDocument.CreatePageRangeTestPdf(3);

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => SparkCode.PDF.ExtractPages.Extract(inputPdf, 3, 2));

            Assert.Contains("less than or equal", exception.Message);
        }

        [Fact]
        public void Extract_PageFromBeyondDocument_ThrowsClearArgumentOutOfRangeException()
        {
            string inputPdf = PdfTestDocument.CreatePageRangeTestPdf(3);

            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => SparkCode.PDF.ExtractPages.Extract(inputPdf, 4, 4));

            Assert.Equal("pageFrom", exception.ParamName);
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
