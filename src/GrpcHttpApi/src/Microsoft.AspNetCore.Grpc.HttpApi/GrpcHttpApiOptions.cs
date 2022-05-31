// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    /// <summary>
    /// Options used to configure gRPC HTTP API service instances.
    /// </summary>
    public class GrpcHttpApiOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="HttpApi.JsonSettings"/> used to serialize messages.
        /// </summary>
        public JsonSettings JsonSettings { get; set; } = new JsonSettings();
    }
}
