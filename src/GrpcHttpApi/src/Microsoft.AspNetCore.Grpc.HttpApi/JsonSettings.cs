// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Google.Protobuf.Reflection;

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    // TODO - improve names. boolean property values should aim to be false
    public class JsonSettings
    {
        /// <summary>
        /// Whether fields which would otherwise not be included in the formatted data
        /// should be formatted even when the value is not present, or has the default value.
        /// This option only affects fields which don't support "presence" (e.g.
        /// singular non-optional proto3 primitive fields).
        /// </summary>
        public bool FormatDefaultValues { get; set; } = true;

        public bool FormatEnumsAsIntegers { get; set; }

        public TypeRegistry TypeRegistry { get; set; } = TypeRegistry.Empty;
    }
}
