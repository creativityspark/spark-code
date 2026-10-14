using System;
using System.IO;
using System.IO.Compression;

namespace SparkCode.Zip
{
    /// <summary>
    /// Adds a file to a zip archive, creating the archive when none is supplied.
    /// </summary>
    public static class AppendFile
    {
        /// <summary>
        /// Appends a file to a zip archive and returns the resulting archive.
        /// If an entry with the same name and path already exists it is replaced.
        /// </summary>
        /// <param name="fileBase64">Contents of the file to add, encoded as Base64.</param>
        /// <param name="fileNameAndPath">Name and relative path of the entry inside the zip file, for example "docs/readme.txt".</param>
        /// <param name="zipFileBase64">Existing zip file encoded as Base64. When null or empty a new zip file is created.</param>
        /// <returns>The resulting zip file encoded as Base64.</returns>
        /// <exception cref="ArgumentException">
        /// The file or file name is empty, an input is not valid Base64, or the existing zip is not a valid zip file.
        /// </exception>
        public static string Append(string fileBase64, string fileNameAndPath, string zipFileBase64)
        {
            if (string.IsNullOrWhiteSpace(fileBase64))
            {
                throw new ArgumentException("The file Base64 input is required.", nameof(fileBase64));
            }

            if (string.IsNullOrWhiteSpace(fileNameAndPath))
            {
                throw new ArgumentException("The file name and path is required.", nameof(fileNameAndPath));
            }

            string entryName = fileNameAndPath.Trim().Replace('\\', '/').TrimStart('/');
            if (entryName.Length == 0 || entryName.EndsWith("/"))
            {
                throw new ArgumentException("The file name and path must include a file name.", nameof(fileNameAndPath));
            }

            byte[] fileBytes = DecodeBase64(fileBase64, nameof(fileBase64));
            bool hasZip = !string.IsNullOrWhiteSpace(zipFileBase64);
            byte[] zipBytes = hasZip ? DecodeBase64(zipFileBase64, nameof(zipFileBase64)) : null;

            using (var stream = new MemoryStream())
            {
                if (hasZip)
                {
                    stream.Write(zipBytes, 0, zipBytes.Length);
                    stream.Position = 0;
                }

                try
                {
                    using (var archive = new ZipArchive(stream, hasZip ? ZipArchiveMode.Update : ZipArchiveMode.Create, true))
                    {
                        if (hasZip)
                        {
                            archive.GetEntry(entryName)?.Delete();
                        }

                        ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                        using (Stream entryStream = entry.Open())
                        {
                            entryStream.Write(fileBytes, 0, fileBytes.Length);
                        }
                    }
                }
                catch (InvalidDataException ex)
                {
                    throw new ArgumentException("The ZipFile input is not a valid zip file.", nameof(zipFileBase64), ex);
                }

                return Convert.ToBase64String(stream.ToArray());
            }
        }

        private static byte[] DecodeBase64(string value, string parameterName)
        {
            try
            {
                return Convert.FromBase64String(value);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException($"The {parameterName} input is not valid Base64.", parameterName, ex);
            }
        }
    }
}
