using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JavaResourceConverter
{
    public static class SatelliteUtil
    {
        public const string SatelliteAssemblyName = "ICU4N.resources.dll";

        public static void BuildSatteliteAssemblies(string inputDir, string outputDir, string targetFramework, string assemblyVersion, string fileVersion, string informationalVersion, string company, string copyright, string product, string title)
        {
            Console.WriteLine($"Generating Satellite Assemblies...");

            SyntaxTree assemblyInfo = GetAssemblyInfoSyntaxTree(assemblyVersion, fileVersion, informationalVersion, company, copyright, product, title);

            // 1. Process Localized Assemblies (Subdirectories)
            var subDirs = Directory.EnumerateDirectories(inputDir);
            foreach (var dir in subDirs)
            {
                var culture = Path.GetFileName(dir); // Folder name is the culture
                var resources = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly);

                if (resources.Length > 0)
                {
                    var cultureOutDir = Path.Combine(outputDir, culture);
                    var cultureOutPath = Path.Combine(cultureOutDir, SatelliteAssemblyName);
                    var cultureAssemblyInfo = $"[assembly: System.Reflection.AssemblyCulture(\"{culture}\")]";

                    BuildSatelliteAssembly(cultureOutPath, assemblyInfo, cultureAssemblyInfo, resources, targetFramework);
                    Console.WriteLine($"Built satellite assembly [{culture}]: {cultureOutPath}");
                }
            }

            // 2. Process the Neutral Assembly (Root Input Directory)
            var rootResources = Directory.EnumerateFiles(inputDir, "*.*", SearchOption.TopDirectoryOnly);
            if (rootResources.Any())
            {
                var neutralOutPath = Path.Combine(outputDir, SatelliteAssemblyName);
                BuildSatelliteAssembly(neutralOutPath, assemblyInfo, string.Empty, rootResources, targetFramework);
                Console.WriteLine($"Built neutral assembly: {neutralOutPath}");
            }

            Console.WriteLine($"[SUCCESS] All satellite assemblies built successfully.");
        }

        private static void BuildSatelliteAssembly(
            string outputPath,
            SyntaxTree assemblyInfoSyntaxTree,
            string cultureAssemblyInfo,
            IEnumerable<string> resourceFiles,
            string targetFramework)
        {
            // 1. Generate the C# Code In-Memory
            var assemblyCultureInfoSyntaxTree = CSharpSyntaxTree.ParseText(cultureAssemblyInfo);

            PortableExecutableReference reference = Basic.Reference.Assemblies.NetStandard20.References.netstandard;
            if (targetFramework == "net40")
            {
                reference = Basic.Reference.Assemblies.Net40.References.mscorlib;
            }
            else if (targetFramework != "netstandard2.0")
            {
                throw new NotSupportedException($"Unsupported target framework: {targetFramework}");
            }

            // 2. Prepare Resources without loading them all into memory at once
            var resourceDescriptions = resourceFiles.Select(resPath =>
                new ResourceDescription(
                    Path.GetFileName(resPath),
                    () => File.OpenRead(resPath),
                    isPublic: true)
            );

            // 3. Configure Roslyn
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithDeterministic(true);

            var compilation = CSharpCompilation.Create(
                "ICU4N.resources",
                new[] { assemblyInfoSyntaxTree, assemblyCultureInfoSyntaxTree },
                new[] { reference },
                options);

            // 4. Emit to Output Path
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            using (var peStream = File.Create(outputPath))
            {
                var result = compilation.Emit(peStream, manifestResources: resourceDescriptions);
                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                    var errorMsg = string.Join("\n", failures.Select(f => f.GetMessage()));
                    throw new Exception($"Failed to compile {outputPath}:\n{errorMsg}");
                }
            }
        }

        private static SyntaxTree GetAssemblyInfoSyntaxTree(string assemblyVersion, string fileVersion, string informationalVersion, string company, string copyright, string product, string title)
        {
            StringBuilder codeBuilder = new StringBuilder();
            codeBuilder.AppendLine("using System.Reflection;");
            codeBuilder.AppendLine("using System.Runtime.CompilerServices;");
            codeBuilder.AppendLine("using System.Diagnostics;");
            codeBuilder.AppendLine("[assembly: CompilationRelaxations(8)]");
            codeBuilder.AppendLine("[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]");
            codeBuilder.AppendLine("[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]");
            codeBuilder.AppendLine($"[assembly: AssemblyCompany(\"{company}\")]");
            codeBuilder.AppendLine($"[assembly: AssemblyCopyright(\"{copyright}\")]");
            codeBuilder.AppendLine($"[assembly: AssemblyFileVersion(\"{fileVersion}\")]");
            codeBuilder.AppendLine($"[assembly: AssemblyInformationalVersion(\"{informationalVersion}\")]");
            codeBuilder.AppendLine($"[assembly: AssemblyProduct(\"{product}\")]");
            codeBuilder.AppendLine($"[assembly: AssemblyTitle(\"{title}\")]");
            codeBuilder.AppendLine($"[assembly: AssemblyVersion(\"{assemblyVersion}\")]");
            return CSharpSyntaxTree.ParseText(codeBuilder.ToString());
        }
    }
}
