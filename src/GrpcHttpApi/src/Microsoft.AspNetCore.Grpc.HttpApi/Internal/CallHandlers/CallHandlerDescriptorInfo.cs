// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Google.Protobuf.Reflection;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.CallHandlers
{
    internal class CallHandlerDescriptorInfo
    {
        public CallHandlerDescriptorInfo(
            FieldDescriptor? responseBodyDescriptor,
            MessageDescriptor? bodyDescriptor,
            bool bodyDescriptorRepeated,
            List<FieldDescriptor>? bodyFieldDescriptors,
            Dictionary<string, List<FieldDescriptor>> routeParameterDescriptors)
        {
            ResponseBodyDescriptor = responseBodyDescriptor;
            BodyDescriptor = bodyDescriptor;
            BodyDescriptorRepeated = bodyDescriptorRepeated;
            BodyFieldDescriptors = bodyFieldDescriptors;
            RouteParameterDescriptors = routeParameterDescriptors;
            if (BodyFieldDescriptors != null)
            {
                BodyFieldDescriptorsPath = string.Join('.', BodyFieldDescriptors.Select(d => d.Name));
            }
            PathDescriptorsCache = new ConcurrentDictionary<string, List<FieldDescriptor>?>();
        }

        public FieldDescriptor? ResponseBodyDescriptor { get; }
        public MessageDescriptor? BodyDescriptor { get; }
        [MemberNotNull(nameof(BodyFieldDescriptors))]
        public bool BodyDescriptorRepeated { get; }
        public List<FieldDescriptor>? BodyFieldDescriptors { get; }
        public Dictionary<string, List<FieldDescriptor>> RouteParameterDescriptors { get; }
        public ConcurrentDictionary<string, List<FieldDescriptor>?> PathDescriptorsCache { get; }
        public string? BodyFieldDescriptorsPath { get; }
    }
}
