using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace SparkCode.Tests.Zip
{
    public class AppendFileTests
    {
        private static string B64(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        private static ZipArchive Open(string zipBase64)
        {
            return new ZipArchive(new MemoryStream(Convert.FromBase64String(zipBase64)), ZipArchiveMode.Read);
        }

        private static string ReadEntry(ZipArchive archive, string name)
        {
            using (var reader = new StreamReader(archive.GetEntry(name).Open(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        [Fact]
        public void Append_NoZip_CreatesNewZip()
        {
            string result = SparkCode.Zip.AppendFile.Append(B64("hello"), "a.txt", null);

            using (var archive = Open(result))
            {
                Assert.Single(archive.Entries);
                Assert.Equal("hello", ReadEntry(archive, "a.txt"));
            }
        }

        [Fact]
        public void Append_ExistingZip_KeepsExistingEntries()
        {
            string zip = SparkCode.Zip.AppendFile.Append(B64("one"), "a.txt", "");

            string result = SparkCode.Zip.AppendFile.Append(B64("two"), "folder\\b.txt", zip);

            using (var archive = Open(result))
            {
                Assert.Equal(2, archive.Entries.Count);
                Assert.Equal("one", ReadEntry(archive, "a.txt"));
                Assert.Equal("two", ReadEntry(archive, "folder/b.txt"));
            }
        }

        [Fact]
        public void Append_DuplicateName_ReplacesEntry()
        {
            string zip = SparkCode.Zip.AppendFile.Append(B64("old"), "a.txt", null);

            string result = SparkCode.Zip.AppendFile.Append(B64("new"), "a.txt", zip);

            using (var archive = Open(result))
            {
                Assert.Single(archive.Entries);
                Assert.Equal("new", ReadEntry(archive, "a.txt"));
            }
        }

        [Fact]
        public void Append_InvalidFileBase64_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => SparkCode.Zip.AppendFile.Append("not base64!", "a.txt", null));
            Assert.Contains("valid Base64", ex.Message);
        }

        [Fact]
        public void Append_InvalidZipBase64_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => SparkCode.Zip.AppendFile.Append(B64("x"), "a.txt", "not base64!"));
            Assert.Contains("valid Base64", ex.Message);
        }

        [Fact]
        public void Append_InvalidZip_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => SparkCode.Zip.AppendFile.Append(B64("x"), "a.txt", B64("this is not a zip file at all")));
            Assert.Contains("valid zip", ex.Message);
        }

        [Fact]
        public void Append_EmptyFile_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => SparkCode.Zip.AppendFile.Append("", "a.txt", null));
            Assert.Contains("required", ex.Message);
        }

        [Fact]
        public void Append_EmptyName_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => SparkCode.Zip.AppendFile.Append(B64("x"), " ", null));
            Assert.Contains("required", ex.Message);
        }
    }
}
