using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Pdf
{
    /// <displayName>Extract PDF Pages</displayName>
    /// <summary>Extracts an inclusive range of pages from a Base64-encoded PDF.</summary>
    /// <param name="InputPdf" type="string">PDF file contents encoded as Base64.</param>
    /// <param name="PageFrom" type="integer">One-based number of the first page to extract, inclusive.</param>
    /// <param name="PageTo" type="integer">One-based number of the last page to extract, inclusive.</param>
    /// <param name="Results" type="string" direction="output">Base64-encoded PDF containing the extracted pages.</param>
    public class ExtractPages : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);
            string inputPdf = ctx.GetInputParameter<string>("InputPdf", true, null, false);
            int pageFrom = ctx.GetInputParameter<int>("PageFrom", true);
            int pageTo = ctx.GetInputParameter<int>("PageTo", true);

            ctx.Trace($"ExtractPages received: inputPdfIsNull={inputPdf == null}, inputPdfLength={inputPdf?.Length ?? 0}, pageFrom={pageFrom}, pageTo={pageTo}");

            string results;
            try
            {
                results = SparkCode.Pdf.ExtractPages.Extract(inputPdf, pageFrom, pageTo);
            }
            catch (Exception exception)
            {
                ctx.Trace($"ExtractPages failed: {exception}");
                throw;
            }

            ctx.Trace($"ExtractPages completed: resultsLength={results.Length}");
            ctx.SetOutputParameter("Results", results);
        }
    }
}
