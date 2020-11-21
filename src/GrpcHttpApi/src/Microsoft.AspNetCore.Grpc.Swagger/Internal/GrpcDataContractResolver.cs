// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Swashbuckle.AspNetCore.SwaggerGen;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.Swagger.Internal
{
    internal class GrpcDataContractResolver : ISerializerDataContractResolver
    {
        private readonly ISerializerDataContractResolver _innerContractResolver;
        private readonly Dictionary<Type, MessageDescriptor> _messageTypeMapping;
        private readonly Dictionary<Type, EnumDescriptor> _enumTypeMapping;

        public GrpcDataContractResolver(ISerializerDataContractResolver innerContractResolver)
        {
            _innerContractResolver = innerContractResolver;
            _messageTypeMapping = new Dictionary<Type, MessageDescriptor>();
            _enumTypeMapping = new Dictionary<Type, EnumDescriptor>();
        }

        public DataContract GetDataContractForType(Type type)
        {
            if (!_messageTypeMapping.TryGetValue(type, out var messageDescriptor))
            {
                if (typeof(IMessage).IsAssignableFrom(type))
                {
                    var property = type.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static);
                    messageDescriptor = property?.GetValue(null) as MessageDescriptor;

                    if (messageDescriptor == null)
                    {
                        throw new InvalidOperationException($"Couldn't resolve message descriptor for {type}.");
                    }

                    _messageTypeMapping[type] = messageDescriptor;
                }
            }

            if (messageDescriptor != null)
            {
                return ConvertMessage(messageDescriptor);
            }

            if (type.IsEnum)
            {
                if (_enumTypeMapping.TryGetValue(type, out var enumDescriptor))
                {
                    var values = enumDescriptor.Values.Select(v => v.Name).ToList();
                    return DataContract.ForPrimitive(type, DataType.String, dataFormat: null, enumValues: values);
                }
            }

            return _innerContractResolver.GetDataContractForType(type);
        }

        private DataContract ConvertMessage(MessageDescriptor messageDescriptor)
        {
            if (IsWellKnownType(messageDescriptor))
            {
                if (IsWrapperType(messageDescriptor))
                {
                    var field = messageDescriptor.Fields[Int32Value.ValueFieldNumber];

                    return _innerContractResolver.GetDataContractForType(MessageDescriptorHelpers.ResolveFieldType(field));
                }
                if (messageDescriptor.FullName == Timestamp.Descriptor.FullName ||
                    messageDescriptor.FullName == Duration.Descriptor.FullName ||
                    messageDescriptor.FullName == FieldMask.Descriptor.FullName)
                {
                    return DataContract.ForPrimitive(messageDescriptor.ClrType, DataType.String, dataFormat: null);
                }
                if (messageDescriptor.FullName == Struct.Descriptor.FullName)
                {
                    return DataContract.ForObject(messageDescriptor.ClrType, Array.Empty<DataProperty>(), extensionDataType: typeof(Value));
                }
                if (messageDescriptor.FullName == ListValue.Descriptor.FullName)
                {
                    return DataContract.ForArray(messageDescriptor.ClrType, typeof(Value));
                }
                if (messageDescriptor.FullName == Value.Descriptor.FullName)
                {
                    return DataContract.ForPrimitive(messageDescriptor.ClrType, DataType.Unknown, dataFormat: null);
                }
                if (messageDescriptor.FullName == Any.Descriptor.FullName)
                {
                    var anyProperties = new List<DataProperty>
                    {
                        new DataProperty("@type", typeof(string), isRequired: true)
                    };
                    return DataContract.ForObject(messageDescriptor.ClrType, anyProperties, extensionDataType: typeof(Value));
                }
            }

            var properties = new List<DataProperty>();

            foreach (var field in messageDescriptor.Fields.InFieldNumberOrder())
            {
                // Enum type will later be used to call this contract resolver.
                // Register the enum type so we know to resolve its names from the descriptor.
                if (field.FieldType == FieldType.Enum)
                {
                    _enumTypeMapping.TryAdd(field.EnumType.ClrType, field.EnumType);
                }

                Type fieldType;
                if (field.IsMap)
                {
                    var mapFields = field.MessageType.Fields.InFieldNumberOrder();
                    var valueType = MessageDescriptorHelpers.ResolveFieldType(mapFields[1]);
                    fieldType = typeof(IDictionary<,>).MakeGenericType(typeof(string), valueType);
                }
                else if (field.IsRepeated)
                {
                    fieldType = typeof(IList<>).MakeGenericType(MessageDescriptorHelpers.ResolveFieldType(field));
                }
                else
                {
                    fieldType = MessageDescriptorHelpers.ResolveFieldType(field);
                }

                properties.Add(new DataProperty(field.JsonName, fieldType));
            }

            var schema = DataContract.ForObject(messageDescriptor.ClrType, properties: properties);

            return schema;
        }

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

        internal bool IsWellKnownType(MessageDescriptor messageDescriptor) => messageDescriptor.File.Package == "google.protobuf" &&
            WellKnownTypeNames.Contains(messageDescriptor.File.Name);

        internal bool IsWrapperType(MessageDescriptor messageDescriptor) => messageDescriptor.File.Package == "google.protobuf" &&
            messageDescriptor.File.Name == "google/protobuf/wrappers.proto";
    }
}
