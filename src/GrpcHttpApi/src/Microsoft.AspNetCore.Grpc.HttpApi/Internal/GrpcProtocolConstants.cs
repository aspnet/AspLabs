// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal
{
    internal static class GrpcProtocolConstants
    {
        internal const string TimeoutHeader = "grpc-timeout";
        internal const string MessageEncodingHeader = "grpc-encoding";
        internal const string MessageAcceptEncodingHeader = "grpc-accept-encoding";
        internal static readonly ReadOnlyMemory<byte> StreamingDelimiter = new byte[] { (byte)'\n' };

        internal static readonly HashSet<string> FilteredHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            MessageEncodingHeader,
            MessageAcceptEncodingHeader,
            TimeoutHeader,
            HeaderNames.ContentType,
            HeaderNames.TE,
            HeaderNames.Host,
            HeaderNames.AcceptEncoding
        };
    }
}
