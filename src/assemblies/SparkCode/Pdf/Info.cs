using Microsoft.Xrm.Sdk;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.IO;

namespace SparkCode.Pdf
{
    /// <summary>
    /// Reads page count, PDF version, and available document metadata from a Base64-encoded PDF.
    /// </summary>
    public static class Info
    {
        /// <summary>
        /// Extracts page count, PDF version, and available metadata from a Base64-encoded PDF.
        /// </summary>
        /// <param name="inputPdf">PDF file contents encoded as Base64.</param>
        /// <returns>An entity containing the PDF information supported by Dataverse expando output.</returns>
        /// <exception cref="ArgumentException">The input is missing, not valid Base64, or not a valid PDF.</exception>
        public static Entity Extract(string inputPdf)
        {
            if (string.IsNullOrWhiteSpace(inputPdf))
            {
                throw new ArgumentException("The PDF Base64 input is required.", nameof(inputPdf));
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
            using (PdfDocument document = OpenDocument(pdfStream))
            {
                var results = new Entity
                {
                    ["Pages"] = document.PageCount,
                    ["Version"] = FormatPdfVersion(document.Version)
                };

                AddTextMetadata(results, "Title", document.Info.Title);
                AddTextMetadata(results, "Author", document.Info.Author);
                AddTextMetadata(results, "Subject", document.Info.Subject);
                AddTextMetadata(results, "Keywords", document.Info.Keywords);
                AddTextMetadata(results, "Creator", document.Info.Creator);
                AddTextMetadata(results, "Producer", document.Info.Producer);
                AddDateMetadata(results, "Created", document.Info, "/CreationDate", document.Info.CreationDate);
                AddDateMetadata(results, "Modified", document.Info, "/ModDate", document.Info.ModificationDate);

                return results;
            }
        }

        /// <summary>
        /// Opens a PDF in read-only mode and translates PDFsharp parse failures into an input validation error.
        /// </summary>
        /// <param name="pdfStream">The PDF data stream.</param>
        /// <returns>The opened PDF document.</returns>
        private static PdfDocument OpenDocument(Stream pdfStream)
        {
            try
            {
                return PdfReader.Open(pdfStream, PdfDocumentOpenMode.ReadOnly);
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

        /// <summary>
        /// Converts PDFsharp's integer version value (for example, 17) to a dotted version (1.7).
        /// </summary>
        /// <param name="version">The PDF version as an integer.</param>
        /// <returns>The dotted PDF version.</returns>
        private static string FormatPdfVersion(int version)
        {
            return (version / 10) + "." + (version % 10);
        }

        /// <summary>
        /// Adds a text property only when the PDF contains a non-empty value.
        /// </summary>
        /// <param name="results">The expando-compatible result entity.</param>
        /// <param name="name">The result property name.</param>
        /// <param name="value">The metadata value read from the PDF.</param>
        private static void AddTextMetadata(Entity results, string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                results[name] = value;
            }
        }

        /// <summary>
        /// Adds a date property only when its corresponding PDF info entry is present and valid.
        /// </summary>
        /// <param name="results">The expando-compatible result entity.</param>
        /// <param name="name">The result property name.</param>
        /// <param name="info">The PDF document information dictionary.</param>
        /// <param name="elementName">The PDF dictionary key for the date.</param>
        /// <param name="value">The parsed date value.</param>
        private static void AddDateMetadata(
            Entity results,
            string name,
            PdfDocumentInformation info,
            string elementName,
            DateTime value)
        {
            if (info.Elements[elementName] != null && value != DateTime.MinValue)
            {
                results[name] = value;
            }
        }
    }
}
