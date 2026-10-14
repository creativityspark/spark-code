using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace SparkCode.API.Tests.Zip
{
    public class AppendFileTests
    {
        private static string B64(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        private static OrganizationResponse Run(ParameterCollection parameters)
        {
            var service = new Context().Service;
            return service.Execute(new OrganizationRequest("csp_Zip_AppendFile") { Parameters = parameters });
        }

        private static string ReadEntry(string zipBase64, string name)
        {
            using (var archive = new ZipArchive(new MemoryStream(Convert.FromBase64String(zipBase64)), ZipArchiveMode.Read))
            using (var reader = new StreamReader(archive.GetEntry(name).Open(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        [Fact]
        public void AppendFile_NoZip_CreatesNewZip()
        {
            var output = Run(new ParameterCollection
            {
                { "File", B64("hello") },
                { "FileNameAndPath", "docs/a.txt" }
            });

            Assert.Equal("hello", ReadEntry(Assert.IsType<string>(output["Output"]), "docs/a.txt"));
        }

        [Fact]
        public void AppendFile_ExistingZip_AppendsFile()
        {
            string zip = Assert.IsType<string>(Run(new ParameterCollection
            {
                { "File", B64("one") },
                { "FileNameAndPath", "a.txt" }
            })["Output"]);

            var output = Run(new ParameterCollection
            {
                { "File", B64("two") },
                { "FileNameAndPath", "b.txt" },
                { "ZipFile", zip }
            });

            string result = Assert.IsType<string>(output["Output"]);
            Assert.Equal("one", ReadEntry(result, "a.txt"));
            Assert.Equal("two", ReadEntry(result, "b.txt"));
        }

        [Fact]
        public void AppendFile_MissingFile_ThrowsError()
        {
            Assert.ThrowsAny<Exception>(() => Run(new ParameterCollection
            {
                { "FileNameAndPath", "a.txt" }
            }));
        }

        [Fact]
        public void AppendFile_InvalidZip_ThrowsClearError()
        {
            Exception ex = Assert.ThrowsAny<Exception>(() => Run(new ParameterCollection
            {
                { "File", B64("x") },
                { "FileNameAndPath", "a.txt" },
                { "ZipFile", "not base64!" }
            }));

            Assert.Contains("valid Base64", ex.ToString());
        }
    }
}
