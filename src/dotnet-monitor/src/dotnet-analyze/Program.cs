// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Internal.Utilities;

namespace Microsoft.Diagnostics.Tools.Analyze
{
    [Command(Name = "dotnet-analyze", Description = "Analyzes a crash dump for known issues")]
    internal class Program
    {
        [FileExists(ErrorMessage = "The dump file could not be found.")]
        [Required(ErrorMessage = "You must provide a dump file to be analyzed.")]
        [Argument(0, "<DUMP>", Description = "The path to the dump file to analyze.")]
        public string DumpPath { get; set; }

        public int OnExecute(IConsole console, CommandLineApplication app)
        {
            // Load the dump
            console.WriteLine($"Loading crash dump {DumpPath}...");
            using (var target = DataTarget.LoadCrashDump(DumpPath))
            {
                console.WriteLine("CLR Versions:");
                foreach (var clr in target.ClrVersions)
                {
                    console.WriteLine($"  {GetFlavorName(clr.Flavor)}: {clr.Version}");
                }

                // Assume there's only one
                if(target.ClrVersions.Count > 1)
                {
                    console.Error.WriteLine("Multiple CLR versions are present, select one to analyze with (TODO).");
                    return 1;
                }

                var runtime = target.ClrVersions[0].CreateRuntime();

                // Run analyzers
                AsyncHangAnalyzer.Run(console, runtime);
            }
            return 0;
        }

        private string GetFlavorName(ClrFlavor flavor)
        {
#pragma warning disable 0612, 0618
            switch (flavor)
            {
                case ClrFlavor.Desktop:
                    return ".NET Framework";
                case ClrFlavor.CoreCLR:
                    return "Silverlight";
                case ClrFlavor.Native:
                    return ".NET Native";
                case ClrFlavor.Core:
                    return ".NET Core";
                default:
                    return "Unknown CLR";
            }
#pragma warning restore 0612, 0618
        }

        private static int Main(string[] args)
        {
            DebugUtil.WaitForDebuggerIfRequested(ref args);

            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (OperationCanceledException)
            {
                return 0;
            }
        }
    }
}
