// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Diagnostics.Client;

namespace Microsoft.Diagnostics.Tools.Trace
{
    public abstract class TargetCommandBase
    {
        [Option("-t|--target <TARGET>", Description = "The target to connect to. Use '[processId]' as a short-hand to connect to a process using the default binding")]
        public string Target { get; }

        protected bool TryCreateClient(IConsole console, out DiagnosticsClient client)
        {
            if (string.IsNullOrEmpty(Target))
            {
                console.Error.WriteLine("Missing required option: --target");
                client = null;
                return false;
            }

            if(int.TryParse(Target, out var targetPid))
            {
                // Short form, just a process ID
                client = new DiagnosticsClient(new Uri($"process://{targetPid}"));
                return true;
            }

            if (!Uri.TryCreate(Target, UriKind.Absolute, out var targetUri))
            {
                console.Error.WriteLine($"Invalid URI: {Target}");
                client = null;
                return false;
            }

            client = new DiagnosticsClient(targetUri);
            return true;
        }
    }
}
