using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JavaResourceConverter
{
    /// <summary>
    /// Utilities for working with ICU4J resources.
    /// </summary>
    public static class JarUtil
    {
        /// <summary>
        /// The location within the .jar file where the data can be found.
        /// </summary>
        public const string DataPath = @"com\ibm\icu\impl\data";
        public const string DataPrefix = "icudt";
        public const string DataSuffix = "b";


        public static void UnzipJar(string sourceFilePath, string destinationPath)
        {
            using var source = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            Unzip(source, new DirectoryInfo(destinationPath));
        }

        public static string GetDataDirectory(string icu4jHomePath, string majorVersion)
        {
            return Path.Combine(icu4jHomePath, DataPath, string.Concat(DataPrefix, majorVersion, DataSuffix));
        }

        //public static string GetICU4JMajorMinorVersion(string sourceFilePath)
        //{
        //    var attributes = File.GetAttributes(sourceFilePath);
        //    if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
        //        throw new ArgumentException($"The {nameof(sourceFilePath)} must be a Java .jar file.");
        //    if (Path.GetExtension(sourceFilePath) != ".jar")
        //        throw new ArgumentException($"The {nameof(sourceFilePath)} must be a Java .jar file.");
        //    var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
        //    if (!fileName.StartsWith("icu4j-", StringComparison.InvariantCultureIgnoreCase))
        //        throw new ArgumentException($"The {nameof(sourceFilePath)} must start with 'icu4j-'.");

        //    var match = Regex.Match(fileName, @"-(?<version>\d+\.\d+)$");
        //    if (match.Success)
        //        return match.Groups["version"].Value;
        //    throw new ArgumentException("The file name must end in a major.minor version number.");
        //}

        public static string GetICU4JMajorVersion(string sourceFilePath)
        {
            var attributes = File.GetAttributes(sourceFilePath);
            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                throw new ArgumentException($"The {nameof(sourceFilePath)} must be a Java .jar file.");
            if (Path.GetExtension(sourceFilePath) != ".jar")
                throw new ArgumentException($"The {nameof(sourceFilePath)} must be a Java .jar file.");
            var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            if (!fileName.StartsWith("icu4j-", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException($"The {nameof(sourceFilePath)} must start with 'icu4j-'.");

            var match = Regex.Match(fileName, @"-(?<version>\d+)\.\d+$");
            if (match.Success)
                return match.Groups["version"].Value;
            throw new ArgumentException("The file name must end in a major.minor version number.");
        }

        /// <summary>
        /// Deletes one or more files or directories (and everything underneath it).
        /// </summary>
        /// <exception cref="IOException">If any of the given files (or their subhierarchy files in case
        /// of directories) cannot be removed.</exception>
        public static void Rm(params FileSystemInfo[] locations)
        {
            ISet<FileSystemInfo> unremoved = Rm(new HashSet<FileSystemInfo>(), locations);
            if (unremoved.Count > 0)
            {
                StringBuilder b = new StringBuilder("Could not remove the following files (in the order of attempts):\n");
                foreach (var f in unremoved)
                {
                    b.Append("   ")
                     .Append(f.FullName)
                     .Append("\n");
                }
                throw new IOException(b.ToString());
            }
        }

        private static ISet<FileSystemInfo> Rm(ISet<FileSystemInfo> unremoved, params FileSystemInfo[] locations)
        {
            foreach (FileSystemInfo location in locations)
            {
                // LUCENENET: Refresh the state of the FileSystemInfo object so we can be sure
                // the Exists property is (somewhat) current.
                location.Refresh();

                if (location.Exists)
                {
                    if (location is DirectoryInfo directory)
                    {
                        // Try to delete all of the files and folders in the directory
                        Rm(unremoved, directory.GetFileSystemInfos());
                    }

                    try
                    {
                        location.Delete();
                    }
                    catch (Exception)
                    {
                        unremoved.Add(location);
                    }
                }
            }
            return unremoved;
        }

        /// <summary>
        /// Convenience method unzipping <paramref name="zipFileStream"/> into <paramref name="destDir"/>, cleaning up
        /// <paramref name="destDir"/> first. 
        /// </summary>
        public static void Unzip(Stream zipFileStream, DirectoryInfo destDir)
        {
            Rm(destDir);
            destDir.Create();

            using ZipArchive zip = new ZipArchive(zipFileStream);
            foreach (var entry in zip.Entries)
            {
                // Ignore internal folders - these are tacked onto the FullName anyway
                if (entry.FullName.EndsWith("/", StringComparison.Ordinal) || entry.FullName.EndsWith("\\", StringComparison.Ordinal))
                {
                    continue;
                }
                using Stream input = entry.Open();
                FileInfo targetFile = new FileInfo(NormalizePath(Path.Combine(destDir.FullName, entry.FullName)));
                if (!targetFile.Directory.Exists)
                {
                    targetFile.Directory.Create();
                }

                using Stream output = new FileStream(targetFile.FullName, FileMode.Create, FileAccess.Write);
                input.CopyTo(output);
            }
        }

        /// <summary>
        /// Normalize file path names
        /// for the current operating system.
        /// </summary>
        public static string NormalizePath(string input)
        {
            if (Path.DirectorySeparatorChar.Equals('/'))
            {
                return input.Replace('\\', '/');
            }
            return input.Replace('/', '\\');
        }
    }
}
