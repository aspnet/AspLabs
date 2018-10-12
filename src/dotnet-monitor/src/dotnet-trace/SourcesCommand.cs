// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Diagnostics.Tools.Trace
{
    [Command(Name = Name, Description = "Lists Event Sources that exist in the target process at the time the connection is made, and lists new ones as they are created.")]
    internal class SourcesCommand : TargetCommandBase
    {
        public const string Name = "sources";

        public async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            var cancellationToken = console.GetCtrlCToken();

            if (!TryCreateClient(console, out var client))
            {
                return 1;
            }

            console.WriteLine("Connecting to application...");

            client.OnEventSourceCreated += (eventSource) =>
            {
                console.WriteLine($"* {eventSource.Name} [{eventSource.Guid}] (settings: {eventSource.Settings})");
            };

            await client.ConnectAsync();

            console.WriteLine("Connected, press Ctrl-C to terminate...");
            await cancellationToken.WaitForCancellationAsync();

            return 0;
        }
    }
}
