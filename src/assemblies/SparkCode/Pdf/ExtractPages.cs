using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.IO;

namespace SparkCode.Pdf
{
    /// <summary>
    /// Extracts an inclusive range of pages from a Base64-encoded PDF document.
    /// </summary>
    public static class ExtractPages
    {
        /// <summary>
        /// Extracts pages from the source PDF and returns them as a Base64-encoded PDF.
        /// </summary>
        /// <param name="inputPdf">The source PDF document encoded as Base64.</param>
        /// <param name="pageFrom">The one-based number of the first page to extract, inclusive.</param>
        /// <param name="pageTo">The one-based number of the last page to extract, inclusive.</param>
        /// <returns>A Base64-encoded PDF containing the requested pages in their original order.</returns>
        /// <exception cref="ArgumentException">
        /// The PDF input is empty, is not valid Base64, does not contain a valid PDF, or the page range is reversed.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A page number is less than one or exceeds the number of pages in the source PDF.
        /// </exception>
        public static string Extract(string inputPdf, int pageFrom, int pageTo)
        {
            if (string.IsNullOrWhiteSpace(inputPdf))
            {
                throw new ArgumentException("The PDF Base64 input is required.", nameof(inputPdf));
            }

            if (pageFrom < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageFrom), "PageFrom must be a 1-based page number.");
            }

            if (pageTo < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageTo), "PageTo must be a 1-based page number.");
            }

            if (pageFrom > pageTo)
            {
                throw new ArgumentException("PageFrom must be less than or equal to PageTo.", nameof(pageFrom));
            }

            byte[] pdfBytes;
            try
            {
                pdfBytes = Convert.FromBase64String(inputPdf);
            }
            catch (FormatException exception)
            {
                throw new ArgumentException("The PDF input must be valid Base64.", nameof(inputPdf), exception);
            }

            if (pdfBytes.Length == 0)
            {
                throw new ArgumentException("The PDF input must not be empty.", nameof(inputPdf));
            }

            using (var pdfStream = new MemoryStream(pdfBytes))
            using (var sourceDocument = OpenDocument(pdfStream))
            {
                if (pageFrom > sourceDocument.PageCount)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(pageFrom),
                        $"PageFrom must not exceed the PDF page count ({sourceDocument.PageCount}).");
                }

                if (pageTo > sourceDocument.PageCount)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(pageTo),
                        $"PageTo must not exceed the PDF page count ({sourceDocument.PageCount}).");
                }

                using (var resultDocument = new PdfDocument())
                {
                    for (int pageIndex = pageFrom - 1; pageIndex < pageTo; pageIndex++)
                    {
                        resultDocument.AddPage(sourceDocument.Pages[pageIndex]);
                    }

                    using (var resultStream = new MemoryStream())
                    {
                        resultDocument.Save(resultStream, false);
                        return Convert.ToBase64String(resultStream.ToArray());
                    }
                }
            }
        }

        private static PdfDocument OpenDocument(Stream pdfStream)
        {
            try
            {
                return PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);
            }
            catch (PdfReaderException exception)
            {
                throw new ArgumentException("The Base64 input does not contain a valid PDF.", "inputPdf", exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new ArgumentException("The Base64 input does not contain a valid PDF.", "inputPdf", exception);
            }
        }
    }
}
