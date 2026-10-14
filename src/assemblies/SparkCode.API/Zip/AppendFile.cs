using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Zip
{
    /// <displayName>Append File To Zip</displayName>
    /// <summary>Adds a file to a zip file. If no zip file is provided, a new one is created.</summary>
    /// <param name="File" type="string">Base64 string containing the file that will be added to the zip file.</param>
    /// <param name="FileNameAndPath" type="string">Name and relative path of the file inside the zip file.</param>
    /// <param name="ZipFile" type="string" optional="true">Base64 string of the existing zip file where the file will be appended. If not provided, a new zip file is created.</param>
    /// <param name="Output" type="string" direction="output">Base64 string of the resulting zip file.</param>
    public class AppendFile : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            var file = ctx.GetInputParameter<string>("File", true, null, false);
            var fileNameAndPath = ctx.GetInputParameter<string>("FileNameAndPath", true);
            var zipFile = ctx.GetInputParameter<string>("ZipFile", false, null, false);

            var output = SparkCode.Zip.AppendFile.Append(file, fileNameAndPath, zipFile);

            ctx.SetOutputParameter("Output", output);
        }
    }
}
