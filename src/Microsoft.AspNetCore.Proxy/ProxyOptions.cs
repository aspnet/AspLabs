// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Options to configure host, scheme, and port settings
    /// </summary>
    public class ProxyOptions
    {
        private int? _webSocketBufferSize;

        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public HttpMessageHandler BackChannelMessageHandler { get; set; }
        public TimeSpan? WebSocketKeepAliveInterval { get; set; }
        public int? WebSocketBufferSize
        {
            get => _webSocketBufferSize;
            set 
            {
                if (value.HasValue && value.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _webSocketBufferSize = value;
            }
        }
    }
}
