// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter
{
    internal static class ConverterHelpers
    {
        private static readonly HashSet<string> WellKnownTypeNames = new HashSet<string>
        {
            "google/protobuf/any.proto",
            "google/protobuf/api.proto",
            "google/protobuf/duration.proto",
            "google/protobuf/empty.proto",
            "google/protobuf/wrappers.proto",
            "google/protobuf/timestamp.proto",
            "google/protobuf/field_mask.proto",
            "google/protobuf/source_context.proto",
            "google/protobuf/struct.proto",
            "google/protobuf/type.proto",
        };

        internal static bool IsWellKnownType(MessageDescriptor messageDescriptor) => messageDescriptor.File.Package == "google.protobuf" &&
            WellKnownTypeNames.Contains(messageDescriptor.File.Name);

        internal static bool IsWrapperType(MessageDescriptor messageDescriptor) => messageDescriptor.File.Package == "google.protobuf" &&
            messageDescriptor.File.Name == "google/protobuf/wrappers.proto";

        internal const int WrapperValueFieldNumber = Int32Value.ValueFieldNumber;
    }
}
