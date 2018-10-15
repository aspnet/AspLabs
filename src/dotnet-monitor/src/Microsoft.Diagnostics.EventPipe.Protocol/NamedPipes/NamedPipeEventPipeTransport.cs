// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Transport
{
    internal class NamedPipeEventPipeTransport : EventPipeTransport
    {
        private readonly string _pipeName;
        private static readonly ReadOnlyMemory<byte> MagicNumber = new byte[] { 0x42, 0x24 };

        public NamedPipeEventPipeTransport(string pipeName)
        {
            _pipeName = pipeName;
        }

        public override EventPipeClientTransport CreateClient()
        {
            return new Client(_pipeName);
        }

        public override EventPipeServerTransport CreateServer()
        {
            return new Server(_pipeName);
        }

        private class Client : EventPipeClientTransport
        {
            private readonly string _pipeName;

            public Client(string pipeName)
            {
                _pipeName = pipeName;
            }

            public override async Task<IDuplexPipe> ConnectAsync(CancellationToken cancellationToken)
            {
                var stream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, System.IO.Pipes.PipeOptions.Asynchronous);

                // We connect with a timeout of 0 because if the server isn't available we want to immediately cancel.
                stream.Connect(timeout: 0);

                return stream.CreatePipe();
            }
        }

        private class Server : EventPipeServerTransport, IDisposable
        {
            private readonly string _pipeName;
            private Channel<NamedPipeServerStream> _connections = Channel.CreateUnbounded<NamedPipeServerStream>();
            private CancellationTokenSource _shutdownCts = new CancellationTokenSource();

            public Server(string pipeName)
            {
                _pipeName = pipeName;
            }

            public override async Task<IDuplexPipe> AcceptAsync(CancellationToken cancellationToken)
            {
                while (await _connections.Reader.WaitToReadAsync(cancellationToken))
                {
                    if (!cancellationToken.IsCancellationRequested && _connections.Reader.TryRead(out var stream))
                    {
                        var pipe = stream.CreatePipe();
                        return pipe;
                    }
                }

                throw new ObjectDisposedException("Transport has been disposed");
            }

            public void Dispose()
            {
                _shutdownCts.Cancel();
                _connections.Writer.TryComplete();
            }

            public override void Listen()
            {
                // Start listening in a background task
                _ = StartListeningAsync(_pipeName, _connections.Writer, _shutdownCts.Token);
            }

            private static async Task StartListeningAsync(string pipeName, ChannelWriter<NamedPipeServerStream> queue, CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Start listening for a client
                    var stream = new NamedPipeServerStream(
                        pipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        System.IO.Pipes.PipeOptions.Asynchronous);
                    await stream.WaitForConnectionAsync(cancellationToken);

                    // We've got a client, queue them for the next call to Accept to take and start listening again.
                    while (!queue.TryWrite(stream))
                    {
                        if (!await queue.WaitToWriteAsync(cancellationToken))
                        {
                            // The queue was completed, which means we're done.
                            return;
                        }
                    }
                }
            }
        }
    }
}
