#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal
{
    internal class HttpContextStreamWriter<TResponse> : IServerStreamWriter<TResponse>
        where TResponse : class
    {
        private readonly HttpApiServerCallContext _context;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly object _writeLock;
        private Task? _writeTask;
        private bool _completed;

        public HttpContextStreamWriter(HttpApiServerCallContext context, JsonSerializerOptions serializerOptions)
        {
            _context = context;
            _serializerOptions = serializerOptions;
            _writeLock = new object();
        }

        public WriteOptions WriteOptions
        {
            get => _context.WriteOptions;
            set => _context.WriteOptions = value;
        }

        public Task WriteAsync(TResponse message)
        {
            if (message == null)
            {
                return Task.FromException(new ArgumentNullException(nameof(message)));
            }

            if (_completed || _context.CancellationToken.IsCancellationRequested)
            {
                return Task.FromException(new InvalidOperationException("Can't write the message because the request is complete."));
            }

            lock (_writeLock)
            {
                // Pending writes need to be awaited first
                if (IsWriteInProgressUnsynchronized)
                {
                    return Task.FromException(new InvalidOperationException("Can't write the message because the previous write is in progress."));
                }

                // Save write task to track whether it is complete. Must be set inside lock.
                _writeTask = WriteMessageAndDelimiter(message);
            }

            return _writeTask;
        }

        private async Task WriteMessageAndDelimiter(TResponse message)
        {
            await JsonRequestHelpers.SendMessage(_context, _serializerOptions, message);
            await _context.HttpContext.Response.Body.WriteAsync(GrpcProtocolConstants.StreamingDelimiter);
        }

        public void Complete()
        {
            _completed = true;
        }

        /// <summary>
        /// A value indicating whether there is an async write already in progress.
        /// Should only check this property when holding the write lock.
        /// </summary>
        private bool IsWriteInProgressUnsynchronized
        {
            get
            {
                var writeTask = _writeTask;
                return writeTask != null && !writeTask.IsCompleted;
            }
        }
    }
}
