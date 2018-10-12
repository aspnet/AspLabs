// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
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

        public async Task<int> OnExecuteAsync(IConsole console, CommandLineApplication app)
        {
            // Load the dump
            console.WriteLine($"Loading crash dump {DumpPath}...");
            using (var target = DataTarget.LoadCrashDump(DumpPath))
            {
                // Assume there's only one
                if(target.ClrVersions.Count > 1)
                {
                    console.Error.WriteLine("Multiple CLR versions are present!");
                    return 1;
                }

                var runtime = target.ClrVersions[0].CreateRuntime();

                var session = new AnalysisSession(target, runtime);
                await CommandProcessor.RunAsync(console, session);
            }
            return 0;
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
