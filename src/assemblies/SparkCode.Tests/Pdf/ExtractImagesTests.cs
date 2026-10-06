using PdfTestData = SparkCode.Pdf.TestSupport.Pdf;
using PdfTestPayload = SparkCode.Pdf.TestSupport.PdfImageTestPayload;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Xunit;

namespace SparkCode.Tests.Pdf
{
    public class ExtractImagesTests
    {
        [Fact]
        public void Extract_SamplePdfFile_ReturnsJpegImages()
        {
            PdfTestPayload payload = PdfTestData.LoadSamplePdf();

            string[] images = SparkCode.Pdf.ExtractImages.Extract(payload.PdfBase64);

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
        public void Extract_JpegAndPngImages_ReturnsOnlyJpegImages()
        {
            PdfTestPayload payload = PdfTestData.Create(ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Jpeg);

            string[] images = SparkCode.Pdf.ExtractImages.Extract(payload.PdfBase64);

            Assert.Equal(payload.JpegImagesBase64, images);
        }

        [Fact]
        public void Extract_ReusedJpegImage_ReturnsImageOnce()
        {
            PdfTestPayload payload = PdfTestData.CreateRepeatedJpegAcrossPages();

            string[] images = SparkCode.Pdf.ExtractImages.Extract(payload.PdfBase64);

            Assert.Equal(payload.JpegImagesBase64, images);
        }

        [Fact]
        public void Extract_PdfWithoutImages_ReturnsEmptyArray()
        {
            PdfTestPayload payload = PdfTestData.CreateWithoutImages();

            string[] images = SparkCode.Pdf.ExtractImages.Extract(payload.PdfBase64);

            Assert.Empty(images);
        }

        [Fact]
        public void Extract_InvalidBase64_ThrowsClearArgumentException()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => SparkCode.Pdf.ExtractImages.Extract("not base64!"));

            Assert.Contains("valid Base64", exception.Message);
        }

        [Fact]
        public void Extract_InvalidPdf_ThrowsClearArgumentException()
        {
            string invalidPdf = Convert.ToBase64String(Encoding.UTF8.GetBytes("not a PDF"));

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => SparkCode.Pdf.ExtractImages.Extract(invalidPdf));

            Assert.Contains("valid PDF", exception.Message);
        }

        [Fact]
        public void Extract_EmptyInput_ThrowsClearArgumentException()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => SparkCode.Pdf.ExtractImages.Extract(string.Empty));

            Assert.Contains("required", exception.Message);
        }
    }
}
