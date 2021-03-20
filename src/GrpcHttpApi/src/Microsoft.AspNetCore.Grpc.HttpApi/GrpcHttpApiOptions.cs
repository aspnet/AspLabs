// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Google.Protobuf;

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    /// <summary>
    /// Options used to configure gRPC HTTP API service instances.
    /// </summary>
    public class GrpcHttpApiOptions
    {
        // grpc-gateway V2 writes default values by default
        // https://github.com/grpc-ecosystem/grpc-gateway/pull/1377
        private static readonly JsonFormatter DefaultFormatter = new JsonFormatter(new JsonFormatter.Settings(formatDefaultValues: true));

        /// <summary>
        /// Gets or sets the <see cref="Google.Protobuf.JsonFormatter"/> used to serialize outgoing messages.
        /// </summary>
        public JsonFormatter JsonFormatter { get; set; } = DefaultFormatter;

        /// <summary>
        /// Gets or sets the <see cref="Google.Protobuf.JsonParser"/> used to deserialize incoming messages.
        /// </summary>
        public JsonParser JsonParser { get; set; } = JsonParser.Default;
    }
}
