using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace SparkCode.PDF
{
    public static class ExtractImages
    {
        public static string[] Extract(string pdfBase64)
        {
            if (string.IsNullOrWhiteSpace(pdfBase64))
            {
                throw new ArgumentException("The PDF Base64 input is required.", nameof(pdfBase64));
            }

            byte[] pdfBytes;
            try
            {
                pdfBytes = Convert.FromBase64String(pdfBase64);
            }
            catch (FormatException exception)
            {
                throw new ArgumentException("The PDF input must be valid Base64.", nameof(pdfBase64), exception);
            }

            if (pdfBytes.Length == 0)
            {
                throw new ArgumentException("The PDF input must not be empty.", nameof(pdfBase64));
            }

            using (var pdfStream = new MemoryStream(pdfBytes))
            using (var document = OpenDocument(pdfStream))
            {
                var imageBytes = new List<byte[]>();
                var visitedObjects = new HashSet<PdfObject>();

                foreach (PdfPage page in document.Pages)
                {
                    ExtractImagesFromResources(page.Resources, visitedObjects, imageBytes);
                }

                var result = new string[imageBytes.Count];
                for (int index = 0; index < imageBytes.Count; index++)
                {
                    result[index] = Convert.ToBase64String(imageBytes[index]);
                }

                return result;
            }
        }

        private static PdfDocument OpenDocument(Stream pdfStream)
        {
            try
            {
                return PdfReader.Open(pdfStream, PdfDocumentOpenMode.ReadOnly);
            }
            catch (PdfReaderException exception)
            {
                throw new ArgumentException("The Base64 input does not contain a valid PDF.", "pdfBase64", exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new ArgumentException("The Base64 input does not contain a valid PDF.", "pdfBase64", exception);
            }
        }

        private static void ExtractImagesFromResources(
            PdfDictionary resources,
            HashSet<PdfObject> visitedObjects,
            List<byte[]> imageBytes)
        {
            if (resources == null)
            {
                return;
            }

            PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
            if (xObjects == null)
            {
                return;
            }

            foreach (PdfItem item in xObjects.Elements.Values)
            {
                PdfReference reference = item as PdfReference;
                PdfObject pdfObject = reference != null ? reference.Value : item as PdfObject;
                PdfDictionary xObject = pdfObject as PdfDictionary;
                if (xObject == null || !visitedObjects.Add(pdfObject))
                {
                    continue;
                }

                string subtype = xObject.Elements.GetName("/Subtype");
                if (subtype == "/Image")
                {
                    if (IsJpegImage(xObject))
                    {
                        byte[] data = xObject.Stream == null ? null : xObject.Stream.Value;
                        if (data == null || data.Length == 0)
                        {
                            throw new InvalidDataException("A JPEG image in the PDF has no image data.");
                        }

                        imageBytes.Add(data);
                    }
                }
                else if (subtype == "/Form")
                {
                    ExtractImagesFromResources(
                        xObject.Elements.GetDictionary("/Resources"),
                        visitedObjects,
                        imageBytes);
                }
            }
        }

        private static bool IsJpegImage(PdfDictionary image)
        {
            PdfItem filter = image.Elements["/Filter"];
            PdfName filterName = filter as PdfName;
            if (filterName != null)
            {
                return filterName.Value == "/DCTDecode";
            }

            PdfArray filters = filter as PdfArray;
            return filters != null
                && filters.Elements.Count == 1
                && filters.Elements[0] is PdfName
                && ((PdfName)filters.Elements[0]).Value == "/DCTDecode";
        }
    }
}
