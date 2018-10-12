// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Diagnostics.Tools.Analyze.Commands;

namespace Microsoft.Diagnostics.Tools.Analyze
{
    internal static class CommandProcessor
    {
        private static readonly IDictionary<string, IAnalysisCommand> _commands = BuildCommandList(
            new HelpCommand());

        public static async Task RunAsync(IConsole console, AnalysisSession session, CancellationToken cancellationToken = default)
        {
            await console.Out.WriteLineAsync("Ready to process analysis commands. Type 'help' to list available commands or 'help [command]' to get detailed help on a command.");
            await console.Out.WriteLineAsync("Type 'quit' or 'exit' to exit the analysis session.");
            while (!cancellationToken.IsCancellationRequested)
            {
                await console.Out.WriteAsync("> ");
                var line = await console.In.ReadLineAsync();

                // Naive arg parsing
                var args = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if(string.Equals(args[0], "quit", StringComparison.OrdinalIgnoreCase) || string.Equals(args[0], "exit", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                if(_commands.TryGetValue(args[0], out var command))
                {
                    await command.RunAsync(console, args.Skip(1).ToArray(), session);
                }
                else
                {
                    console.Error.WriteLine($"Unknown command: {args[0]}");
                }
            }
        }

        private static IDictionary<string, IAnalysisCommand> BuildCommandList(params IAnalysisCommand[] commands)
        {
            return commands.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
        }
    }
}
