//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Text;

//namespace ICU4JResourceConverter
//{
//    public static class AssemblyLinkerUtil
//    {
//        private static string[] DotNetToolsPathFallbackLocations = new string[]
//        {
//            "%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\",
//            "%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7.2 Tools\",
//            "%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.2 Tools\",
//            "%ProgramFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\",
//        };

//        private static readonly string DotNetToolsPath = LoadDotNetToolsPathPath();

//        private static string LoadDotNetToolsPathPath()
//        {
//            foreach (var path in DotNetToolsPathFallbackLocations)
//                if (Directory.Exists(path)) return path;

//            return null; // Not found (need to check before use)
//        }

//        public static LinkAssembly(string templateAssemblyPath, string dotNetLocale, string snkFile, IEnumerable<string> embeddedResources, string outputDirectory)
//        {
//            if (DotNetToolsPath is null)
//                throw new InvalidOperationException("Path to al.exe not found");
//            var command = DotNetToolsPath + "al.exe";
//            var sb = new StringBuilder();
//            const string Space = " ";
            
//            sb.Append("-target:lib");
//            sb.Append(Space);
//            sb.Append($"-culture:{dotNetLocale}");
//            sb.Append(Space);
//            sb.Append($"-out:{outputDirectory}");
//            sb.Append(Space);
//            sb.Append($"-template:{templateAssemblyPath}");
//            sb.Append(Space);

//            foreach (var embeddedResource in embeddedResources)
//            {
//                sb.Append($"-template:{templateAssemblyPath}");
//                sb.Append(Space);
//            }
//        }

//        // returns exit code
//        public static int RunCommand(string executable, string arguments, out string stdOut, out string stdErr)
//        {
//            using Process p = new Process();

//            p.StartInfo.UseShellExecute = false;
//            p.StartInfo.RedirectStandardOutput = true;
//            p.StartInfo.RedirectStandardError = true;
//            p.StartInfo.FileName = executable;
//            p.StartInfo.Arguments = arguments;
//            p.Start();

//            stdOut = p.StandardOutput.ReadToEnd();
//            stdErr = p.StandardError.ReadToEnd();
//            p.WaitForExit();

//            return p.ExitCode;
//        }
//    }
//}
