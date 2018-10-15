// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Transport;

namespace Microsoft.Diagnostics.Server
{
    public class DiagnosticServer : IDisposable
    {
        private readonly Uri _listenUri;
        private readonly EventPipeTransport _transport;
        private readonly CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private readonly IDisposable _registration;

        private DiagnosticServer(Uri listenUri)
        {
            _listenUri = listenUri;
            _transport = EventPipeTransport.Create(listenUri);
        }

        // TODO: Consider the case where a user wants to "run under tracing"
        // The best we can do right now is launch the app and immediately try to connect, but
        // we'll miss any events emitted while the app was starting up. We need to consider a way to
        // allow the Monitor to set some state that causes the app to buffer these events and replay
        // them immediately on connect.
        // For CLR events, the Rundown Provider can be used to replay events, but only the runtime ones
        // Framework and Application EventSources don't have this feature by default.

        /// <summary>
        /// Starts the diagnostic server on any free port and registers that port with the list of available
        /// processes to monitor.
        /// </summary>
        /// <returns></returns>
        public static DiagnosticServer Start() => Start(new Uri("process://"));

        /// <summary>
        /// Starts the diagnostic server on the specified endpoint.
        /// </summary>
        /// <returns></returns>
        public static DiagnosticServer Start(Uri listenUri)
        {
            var server = new DiagnosticServer(listenUri);
            server.StartListening();
            return server;
        }

        public void Dispose()
        {
            _shutdownCts.Cancel();
        }

        private void StartListening()
        {
            _ = AcceptLoop(_transport.CreateServer(), _shutdownCts.Token);
        }

        private async Task AcceptLoop(EventPipeServerTransport transport, CancellationToken cancellationToken)
        {
            transport.Listen();

            while (!cancellationToken.IsCancellationRequested)
            {
                var pipe = await transport.AcceptAsync(cancellationToken);
                var server = new EventPipeServer(pipe);
                _ = server.RunAsync(cancellationToken);
            }
        }
    }
}
