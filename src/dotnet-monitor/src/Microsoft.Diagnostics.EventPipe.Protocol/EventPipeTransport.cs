// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Transport
{
    public abstract class EventPipeTransport
    {
        private static readonly string PipePrefix = "dotnet-diag-pipe_";

        public static EventPipeTransport Create(Uri uri)
        {
            if (uri.Scheme.Equals("process"))
            {
                return new NamedPipeEventPipeTransport(GetPipeNameForProcess(uri.Host));
            }
            else if (uri.Scheme.Equals("pipe"))
            {
                return new NamedPipeEventPipeTransport(uri.Host);
            }
            else if (uri.Scheme.Equals("tcp"))
            {
                return new TcpTransport(new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port));
            }
            else
            {
                throw new InvalidOperationException($"Unsupported URI scheme '{uri.Scheme}://'");
            }
        }

        public abstract EventPipeServerTransport CreateServer();
        public abstract EventPipeClientTransport CreateClient();

        private static string GetPipeNameForProcess(string processId) => string.IsNullOrEmpty(processId) ? $"{PipePrefix}{Process.GetCurrentProcess().Id}" : $"{PipePrefix}{processId}";
    }
}
