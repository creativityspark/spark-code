using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Pdf
{
    /// <displayName>PDF Info</displayName>
    /// <summary>Extracts page count, PDF version, and available document metadata from a Base64-encoded PDF.</summary>
    /// <param name="InputPdf" type="string">PDF file contents encoded as Base64.</param>
    /// <param name="Results" type="expando" direction="output">PDF page count, version, and available document metadata.</param>
    public class Info : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);
            string inputPdf = ctx.GetInputParameter<string>("InputPdf", true, null, false);
            ctx.Trace($"InputPdf received: isNull={inputPdf == null}, length={inputPdf?.Length ?? 0}");

            try
            {
                Entity results = SparkCode.Pdf.Info.Extract(inputPdf);
                ctx.SetOutputParameter("Results", results);
                ctx.SetOutputParameter("ResultsJson", results.ToJson());
                ctx.Trace($"PDF information extracted: pages={results["Pages"]}, version={results["Version"]}, fields={results.Attributes.Count}");
            }
            catch (ArgumentException exception)
            {
                ctx.Trace("Failed to extract PDF information: " + exception);
                throw;
            }
        }
    }
}
