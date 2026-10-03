using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace JavaResourceConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args is null || args.Length == 0)
                Usage();

            var arg0 = args[0];
            if (arg0.Equals("-help", StringComparison.OrdinalIgnoreCase) ||
                arg0.Equals("-?", StringComparison.OrdinalIgnoreCase) ||
                arg0.Equals("?", StringComparison.OrdinalIgnoreCase))
            {
                Usage();
            }

            // Route to the new command or fallback to the legacy command based on the first argument
            if (arg0.Equals("transform", StringComparison.OrdinalIgnoreCase))
            {
                ParseAndExecuteTransform(args);
            }
            else
            {
                ParseAndExecuteUnzipAndTransform(args);
            }
        }

        private static void ParseAndExecuteTransform(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Error: 'transform' command requires <data directory> and <output directory>.");
                Usage();
            }

            string dataDir = JarUtil.NormalizePath(args[1]);
            string outputDir = JarUtil.NormalizePath(args[2]);
            string timestampFile = null;

            for (int i = 3; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.Equals("-timestamp", StringComparison.OrdinalIgnoreCase))
                {
                    timestampFile = i + 1 < args.Length ? args[i + 1] : throw new ArgumentException("Missing value for timestamp file.");
                    i++;
                }
            }

            TransformCommand(dataDir, outputDir, timestampFile);
        }

        private static void TransformCommand(string dataDir, string outputDir, string timestampFile)
        {
            string mutexName = GetMutexName(outputDir);

            using (var mutex = new Mutex(false, mutexName))
            {
                bool acquired = false;
                try
                {
                    Console.WriteLine($"Waiting for lock on output directory '{outputDir}'...");
                    acquired = mutex.WaitOne();

                    // Check if another concurrent process completed the work while we were waiting for the mutex
                    if (!string.IsNullOrEmpty(timestampFile) && File.Exists(timestampFile))
                    {
                        Console.WriteLine($"Timestamp marker file '{timestampFile}' already exists. Skipping transformation.");
                        return;
                    }

                    // ****** TRANSFORM COMMAND ******

                    Console.WriteLine($"ICU Data is available at {dataDir}");

                    ResourceUtil.TransformResources(dataDir, outputDir);

                    Console.WriteLine($"Transforming resources completed. Data is available in {outputDir}.");

                    // Write timestamp file inside the mutex upon successful transformation
                    if (!string.IsNullOrEmpty(timestampFile))
                    {
                        var timestampDir = Path.GetDirectoryName(timestampFile);
                        if (!string.IsNullOrEmpty(timestampDir) && !Directory.Exists(timestampDir))
                        {
                            Directory.CreateDirectory(timestampDir);
                        }
                        File.WriteAllText(timestampFile, "Transform completed at " + DateTime.UtcNow.ToString("o"));
                    }
                }
                finally
                {
                    if (acquired)
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        private static void ParseAndExecuteUnzipAndTransform(string[] args)
        {
            string source = args[0]; // Source .jar file
            string workingDir = null;
            string outputDir = null;
            string timestampFile = null;

            for (int i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.Equals("-work", StringComparison.Ordinal))
                {
                    workingDir = i + 1 < args.Length ? args[i + 1] : throw new ArgumentException("Missing value for working directory.");
                    i++;
                }
                else if (arg.Equals("-out", StringComparison.Ordinal))
                {
                    outputDir = i + 1 < args.Length ? args[i + 1] : throw new ArgumentException("Missing value for output directory.");
                    i++;
                }
                else if (arg.Equals("-timestamp", StringComparison.OrdinalIgnoreCase))
                {
                    timestampFile = i + 1 < args.Length ? args[i + 1] : throw new ArgumentException("Missing value for timestamp file.");
                    i++;
                }
            }

            if (string.IsNullOrWhiteSpace(workingDir))
                workingDir = Path.GetTempPath() + Guid.NewGuid().ToString();

            workingDir = JarUtil.NormalizePath(workingDir);

            if (string.IsNullOrWhiteSpace(outputDir))
                outputDir = Path.Combine(workingDir, "resources");

            outputDir = JarUtil.NormalizePath(outputDir);

            UnzipAndTransformCommand(source, workingDir, outputDir, timestampFile);
        }

        private static void UnzipAndTransformCommand(string source, string workingDir, string outputDir, string timestampFile)
        {
            string mutexName = GetMutexName(outputDir);

            using (var mutex = new Mutex(false, mutexName))
            {
                bool acquired = false;
                try
                {
                    acquired = mutex.WaitOne();

                    // Check if another concurrent process completed the work while we were waiting for the mutex
                    if (!string.IsNullOrEmpty(timestampFile) && File.Exists(timestampFile))
                    {
                        Console.WriteLine($"Timestamp marker file '{timestampFile}' already exists. Skipping transformation.");
                        return;
                    }

                    //string source = @"F:\icu4j-60.1.jar";
                    //string workingDir = @"F:\icu4j-temp";
                    string icuDir = Path.Combine(workingDir, "icu-temp");

                    icuDir = JarUtil.NormalizePath(icuDir);

                    // ****** UNJAR COMMAND ******

                    // TODO: Allow a switch to force unpack (or make it the default and switch for off)
                    if (!Directory.Exists(icuDir))
                    {
                        // Unpack the .jar file
                        JarUtil.UnzipJar(source, icuDir);
                        Console.WriteLine($"Unzipped {source} to {icuDir}");
                    }
                    //string dataDir = UnJar(source, icuDir);

                    // ****** TRANSFORM COMMAND ******

                    string majorVersion = JarUtil.GetICU4JMajorVersion(source);
                    string dataDir = JarUtil.GetDataDirectory(icuDir, majorVersion);

                    Console.WriteLine($"ICU Data is available at {dataDir}");

                    ResourceUtil.TransformResources(dataDir, outputDir);

                    Console.WriteLine($"Transforming resources completed. Data is available in {outputDir}.");

                    // ****** PACK COMMAND ****** (does assembly linking)

                    // Write timestamp file inside the mutex upon successful transformation
                    if (!string.IsNullOrEmpty(timestampFile))
                    {
                        var timestampDir = Path.GetDirectoryName(timestampFile);
                        if (!string.IsNullOrEmpty(timestampDir) && !Directory.Exists(timestampDir))
                        {
                            Directory.CreateDirectory(timestampDir);
                        }
                        File.WriteAllText(timestampFile, "Transform completed at " + DateTime.UtcNow.ToString("o"));
                    }
                }
                finally
                {
                    if (acquired)
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        private static string GetMutexName(string outputDir)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(outputDir.ToLowerInvariant());
                var hash = sha256.ComputeHash(bytes);
                return "Local\\ICU4JResourceConverter_" + BitConverter.ToString(hash).Replace("-", "");
            }
        }

        private static void Usage()
        {
            var appName = Assembly.GetEntryAssembly().GetName().Name;
            Console.WriteLine("\nUsage:");
            // -out and -work are required for the legacy command, so brackets are removed.
            Console.WriteLine($"  {appName} <icu4j .jar file> -out <output directory> -work <working directory> [-timestamp <marker file>]");
            // transform requires the data directory and output directory, with an optional timestamp.
            Console.WriteLine($"  {appName} transform <data directory> <output directory> [-timestamp <marker file>]\n");
            Environment.Exit(1);
        }
    }
}
