using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Diagnostics.Tools.Dump
{
    public static partial class Dumper
    {
        private static class Windows
        {
            internal static Task CollectDumpAsync(Process process, string outputFile)
            {
                // We can't do this "asynchronously" so just Task.Run it. It shouldn't be "long-running" so this is fairly safe.
                return Task.Run(() =>
                {
                    // Open the file for writing
                    using (var stream = new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    {
                        // Dump the process!
                        var exceptionInfo = new NativeMethods.MINIDUMP_EXCEPTION_INFORMATION();
                        if (!NativeMethods.MiniDumpWriteDump(process.Handle, (uint)process.Id, stream.SafeFileHandle, NativeMethods.MINIDUMP_TYPE.MiniDumpWithFullMemory, ref exceptionInfo, IntPtr.Zero, IntPtr.Zero))
                        {
                            var err = Marshal.GetHRForLastWin32Error();
                            Marshal.ThrowExceptionForHR(err);
                        }
                    }
                });
            }

            private static class NativeMethods
            {
                [DllImport("Dbghelp.dll")]
                public static extern bool MiniDumpWriteDump(IntPtr hProcess, uint ProcessId, SafeFileHandle hFile, MINIDUMP_TYPE DumpType, ref MINIDUMP_EXCEPTION_INFORMATION ExceptionParam, IntPtr UserStreamParam, IntPtr CallbackParam);

                [StructLayout(LayoutKind.Sequential, Pack = 4)]
                public struct MINIDUMP_EXCEPTION_INFORMATION
                {
                    public uint ThreadId;
                    public IntPtr ExceptionPointers;
                    public int ClientPointers;
                }

                public enum MINIDUMP_TYPE : uint
                {
                    MiniDumpNormal,
                    MiniDumpWithDataSegs,
                    MiniDumpWithFullMemory,
                    MiniDumpWithHandleData,
                    MiniDumpFilterMemory,
                    MiniDumpScanMemory,
                    MiniDumpWithUnloadedModules,
                    MiniDumpWithIndirectlyReferencedMemory,
                    MiniDumpFilterModulePaths,
                    MiniDumpWithProcessThreadData,
                    MiniDumpWithPrivateReadWriteMemory,
                    MiniDumpWithoutOptionalData,
                    MiniDumpWithFullMemoryInfo,
                    MiniDumpWithThreadInfo,
                    MiniDumpWithCodeSegs,
                    MiniDumpWithoutAuxiliaryState,
                    MiniDumpWithFullAuxiliaryState,
                    MiniDumpWithPrivateWriteCopyMemory,
                    MiniDumpIgnoreInaccessibleMemory,
                    MiniDumpWithTokenInformation,
                    MiniDumpWithModuleHeaders,
                    MiniDumpFilterTriage,
                    MiniDumpWithAvxXStateContext,
                    MiniDumpWithIptTrace,
                    MiniDumpValidTypeFlags
                }
            }
        }
    }
}
