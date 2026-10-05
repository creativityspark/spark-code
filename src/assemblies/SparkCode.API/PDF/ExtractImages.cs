using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.PDF
{
    /// <displayName>Extract PDF Images</displayName>
    /// <summary>Extracts embedded JPEG images from a Base64-encoded PDF.</summary>
    /// <param name="PdfBase64" type="string">PDF file contents encoded as Base64.</param>
    /// <param name="Images" type="stringarray" direction="output">Base64-encoded contents of each unique embedded JPEG image, in document order.</param>
    public class ExtractImages : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            string pdfBase64 = ctx.GetInputParameter<string>("PdfBase64", true, null, false);
            string preview = GetBase64Preview(pdfBase64);
            ctx.Trace(
                $"PdfBase64 received: isNull={pdfBase64 == null}, length={pdfBase64?.Length ?? 0}, containsWhitespace={ContainsWhitespace(pdfBase64)}, preview=\"{preview}\"");

            string[] images = SparkCode.PDF.ExtractImages.Extract(pdfBase64);

            ctx.SetOutputParameter("Images", images);
        }

        private static string GetBase64Preview(string value)
        {
            const int previewLength = 24;
            if (value == null)
            {
                return "<null>";
            }

            if (value.Length <= previewLength * 2)
            {
                return value;
            }

            return value.Substring(0, previewLength)
                + "..."
                + value.Substring(value.Length - previewLength);
        }

        private static bool ContainsWhitespace(string value)
        {
            if (value == null)
            {
                return false;
            }

            foreach (char character in value)
            {
                if (char.IsWhiteSpace(character))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
