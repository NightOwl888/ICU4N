using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace JavaResourceConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var appName = Assembly.GetEntryAssembly().GetName().Name;
            var notAlreadyRunning = true;
            using (var mutex = new Mutex(true, appName + "Singleton", out notAlreadyRunning))
            {
                if (notAlreadyRunning)
                {
                    //// do work here:
                    //Console.WriteLine("Running. Press any key to exit...");
                    //Console.ReadKey();

                    string source;
                    string workingDir = null;
                    string outputDir = null;

                    if (args is null || args.Length == 0)
                        Usage();

                    var arg0 = args[0];
                    if (arg0.Equals("-help", StringComparison.Ordinal) || arg0.Equals("-?", StringComparison.Ordinal) || arg0.Equals("?", StringComparison.Ordinal))
                        Usage();

                    source = arg0; // Source .jar file

                    for (int i = 1; i < args.Length; i++)
                    {
                        var arg = args[i];
                        if (arg.Equals("-work", StringComparison.Ordinal))
                            workingDir = i + 1 < args.Length ? args[i + 1] : throw new ArgumentException("Missing value for working directory.");
                        else if (arg.Equals("-out", StringComparison.Ordinal))
                            outputDir = i + 1 < args.Length ? args[i + 1] : throw new ArgumentException("Missing value for output directory.");
                    }




                    if (string.IsNullOrWhiteSpace(workingDir))
                        workingDir = Path.GetTempPath() + Guid.NewGuid().ToString();

                    workingDir = JarUtil.NormalizePath(workingDir);

                    if (string.IsNullOrWhiteSpace(outputDir))
                        outputDir = Path.Combine(workingDir, "resources");

                    outputDir = JarUtil.NormalizePath(outputDir);

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


                }
                else
                    Console.Error.WriteLine(appName + " is already running.");
            }
        }

        private static void Usage()
        {
            Console.WriteLine("\n\n" + "ICU4JResourceConverter <icu4j .jar file> [-out <output directory>] [-work <working directory>]");
            Environment.Exit(1);
        }
    }
}
