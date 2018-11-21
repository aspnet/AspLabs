using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Tools.Collect
{
    internal static class ConfigPathDetector
    {
        private static readonly HashSet<string> _managedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".exe", ".dll" };

        // Known .NET Platform Assemblies
        private static readonly HashSet<string> _platformAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "System.Private.CoreLib.dll",
            "clrjit.dll",
        };

        internal static string TryDetectConfigPath(int processId)
        {
            var process = Process.GetProcessById(processId);

            var platform = CreatePlatformAbstractions();

            // Iterate over modules
            foreach(var module in process.Modules.Cast<ProcessModule>())
            {
                // Filter out things that aren't exes and dlls (useful on Unix/macOS to skip native libraries)
                var extension = Path.GetExtension(module.FileName);
                var name = Path.GetFileName(module.FileName);
                if (_managedExtensions.Contains(extension) && !platform.KnownNativeLibraries.Contains(name) && !_platformAssemblies.Contains(name))
                {
                    var candidateDir = Path.GetDirectoryName(module.FileName);
                    var appName = Path.GetFileNameWithoutExtension(module.FileName);

                    // Check for the deps.json file
                    // TODO: Self-contained apps?
                    if(File.Exists(Path.Combine(candidateDir, $"{appName}.deps.json")))
                    {
                        // This is an app!
                        return Path.Combine(candidateDir, $"{appName}.eventpipeconfig");
                    }
                }
            }

            return null;
        }

        private static PlatformAbstractions CreatePlatformAbstractions()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsPlatformAbstractions();
            }
            return new PlatformAbstractions();
        }

        internal class PlatformAbstractions
        {
            private static HashSet<string> _knownNativeLibraries = new HashSet<string>();
            public virtual HashSet<string> KnownNativeLibraries => _knownNativeLibraries;
        }

        internal class WindowsPlatformAbstractions : PlatformAbstractions
        {
            private static HashSet<string> _knownNativeLibraries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // .NET Core Host
                "dotnet.exe",
                "hostfxr.dll",
                "hostpolicy.dll",
                "coreclr.dll",

                // Windows Native Libraries
                "ntdll.dll",
                "kernel32.dll",
                "kernelbase.dll",
                "apphelp.dll",
                "ucrtbase.dll",
                "advapi32.dll",
                "msvcrt.dll",
                "sechost.dll",
                "rpcrt4.dll",
                "ole32.dll",
                "combase.dll",
                "bcryptPrimitives.dll",
                "gdi32.dll",
                "gdi32full.dll",
                "msvcp_win.dll",
                "user32.dll",
                "win32u.dll",
                "oleaut32.dll",
                "shlwapi.dll",
                "version.dll",
                "bcrypt.dll",
                "imm32.dll",
                "kernel.appcore.dll",
            };

            public override HashSet<string> KnownNativeLibraries => _knownNativeLibraries;
        }
    }
}
