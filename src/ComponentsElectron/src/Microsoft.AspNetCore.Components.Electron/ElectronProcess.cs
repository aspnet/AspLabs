// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Components.Electron
{
    internal class ElectronProcess
    {
        public static ElectronProcess Start(ProcessStartInfo info)
        {
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            var process = Process.Start(info);
            return new ElectronProcess(process);
        }

        public ElectronProcess(Process process)
        {
            Process = process;

            Process.OutputDataReceived += Process_OutputDataReceived;
            Process.ErrorDataReceived += Process_ErrorDataReceived;

            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();
        }

        public Process Process { get; }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            // TODO - this isn't the right layer to do this but it's good enough for now.
            Console.Error.WriteLine($"[electron:{Process.Id}] out:" + e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            // TODO - this isn't the right layer to do this but it's good enough for now.
            Console.Error.WriteLine($"[electron:{Process.Id}] err:" + e.Data);
        }
    }
}
