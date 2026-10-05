using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SparkCode.PDF.TestSupport
{
    internal sealed class PdfImageTestPayload
    {
        public string PdfBase64 { get; set; }
        public string[] JpegImagesBase64 { get; set; }
    }

    internal static class PdfTestDocument
    {
        public static PdfImageTestPayload LoadSamplePdf()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PdfTestDocument.pdf");
            byte[] pdfBytes = File.ReadAllBytes(path);

            return new PdfImageTestPayload
            {
                PdfBase64 = System.Convert.ToBase64String(pdfBytes)
            };
        }

        public static PdfImageTestPayload Create(params ImageFormat[] imageFormats)
        {
            return CreatePages(new[] { imageFormats }, false);
        }

        public static PdfImageTestPayload CreateRepeatedJpegAcrossPages()
        {
            return CreatePages(
                new[]
                {
                    new[] { ImageFormat.Jpeg },
                    new[] { ImageFormat.Jpeg }
                },
                true);
        }

        public static PdfImageTestPayload CreateWithoutImages()
        {
            return CreatePages(new[] { new ImageFormat[0] }, false);
        }

        private static PdfImageTestPayload CreatePages(ImageFormat[][] pageFormats, bool reuseFirstImage)
        {
            var document = new PdfDocument();
            var imageStreams = new List<MemoryStream>();
            var images = new List<XImage>();
            var jpegImagesBase64 = new List<string>();
            byte[] reusableJpeg = null;
            XImage reusableImage = null;
            int imageNumber = 0;

            try
            {
                foreach (ImageFormat[] formats in pageFormats)
                {
                    PdfPage page = document.AddPage();
                    using (XGraphics graphics = XGraphics.FromPdfPage(page))
                    {
                        for (int index = 0; index < formats.Length; index++)
                        {
                            ImageFormat format = formats[index];
                            byte[] imageBytes;
                            XImage image;

                            if (reuseFirstImage && reusableImage != null)
                            {
                                imageBytes = reusableJpeg;
                                image = reusableImage;
                            }
                            else
                            {
                                imageBytes = CreateImageBytes(format, imageNumber++);
                                var imageStream = new MemoryStream(imageBytes);
                                imageStreams.Add(imageStream);
                                image = XImage.FromStream(imageStream);
                                images.Add(image);

                                if (reuseFirstImage)
                                {
                                    reusableJpeg = imageBytes;
                                    reusableImage = image;
                                }
                            }

                            graphics.DrawImage(image, index * 30, 0, 20, 20);
                            if (format.Guid == ImageFormat.Jpeg.Guid && !reuseFirstImage)
                            {
                                jpegImagesBase64.Add(System.Convert.ToBase64String(imageBytes));
                            }
                        }
                    }
                }

                if (reuseFirstImage)
                {
                    jpegImagesBase64.Add(System.Convert.ToBase64String(reusableJpeg));
                }

                using (var pdfStream = new MemoryStream())
                {
                    document.Save(pdfStream, false);
                    return new PdfImageTestPayload
                    {
                        PdfBase64 = System.Convert.ToBase64String(pdfStream.ToArray()),
                        JpegImagesBase64 = jpegImagesBase64.ToArray()
                    };
                }
            }
            finally
            {
                document.Dispose();
                foreach (XImage image in images)
                {
                    image.Dispose();
                }

                foreach (MemoryStream imageStream in imageStreams)
                {
                    imageStream.Dispose();
                }
            }
        }

        private static byte[] CreateImageBytes(ImageFormat format, int imageNumber)
        {
            using (var bitmap = new Bitmap(4, 4))
            using (var stream = new MemoryStream())
            {
                var color = Color.FromArgb(255, (imageNumber * 71) % 255, (imageNumber * 113 + 37) % 255, (imageNumber * 157 + 89) % 255);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(color);
                }

                bitmap.Save(stream, format);
                return stream.ToArray();
            }
        }
    }
}
