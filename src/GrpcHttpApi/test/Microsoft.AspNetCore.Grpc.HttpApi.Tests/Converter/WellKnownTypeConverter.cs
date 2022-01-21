// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter
{
    public abstract class WellKnownTypeConverter : JsonConverter<IMessage>
    {
        protected abstract string WellKnownTypeName { get; }

        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeof(IMessage).IsAssignableFrom(typeToConvert))
            {
                return false;
            }

            var property = typeToConvert.GetProperty("Descriptor", BindingFlags.Static | BindingFlags.Public, binder: null, typeof(MessageDescriptor), Type.EmptyTypes, modifiers: null);
            if (property == null)
            {
                return false;
            }

            var descriptor = property.GetValue(null, null) as MessageDescriptor;
            if (descriptor == null)
            {
                return false;
            }

            if (descriptor.FullName != WellKnownTypeName)
            {
                return false;
            }

            return true;
        }
    }
}
