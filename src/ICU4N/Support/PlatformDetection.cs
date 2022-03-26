using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ICU4N
{
    internal static class PlatformDetection
    {
        private static readonly bool isWindows = LoadIsWindows();

        private static bool LoadIsWindows()
        {
#if FEATURE_RUNTIMEINFORMATION_ISOSPLATFORM
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
            PlatformID p = Environment.OSVersion.Platform;
            return p == PlatformID.Win32NT || p == PlatformID.Win32S || p == PlatformID.Win32Windows || p == PlatformID.WinCE;
#endif
        }

        public static string BaseDirectory
        {
            get
            {
#if FEATURE_APPCONTEXT_BASEDIRECTORY
                return AppContext.BaseDirectory;
#else
                return AppDomain.CurrentDomain.BaseDirectory;
#endif
            }
        }

        public static bool IsWindows => isWindows;

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
