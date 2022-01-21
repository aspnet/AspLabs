// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using HttpApi;
using Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests
{
    public class JsonConverterTests
    {
        private readonly ITestOutputHelper _output;

        public JsonConverterTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void RepeatedStrings()
        {
            var helloRequest = new HelloRequest
            {
                Name = "test",
                RepeatedStrings =
                {
                    "One",
                    "Two",
                    "Three"
                }
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void MapStrings()
        {
            var helloRequest = new HelloRequest
            {
                MapStrings =
                {
                    ["name1"] = "value1",
                    ["name2"] = "value2"
                }
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void MapKeyBool()
        {
            var helloRequest = new HelloRequest
            {
                MapKeybool =
                {
                    [true] = "value1",
                    [false] = "value2"
                }
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void MapKeyInt()
        {
            var helloRequest = new HelloRequest
            {
                MapKeyint =
                {
                    [-1] = "value1",
                    [0] = "value2",
                    [0] = "value3"
                }
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void MapMessages()
        {
            var helloRequest = new HelloRequest
            {
                MapMessage =
                {
                    ["name1"] = new HelloRequest.Types.SubMessage { Subfield = "value1" },
                    ["name2"] = new HelloRequest.Types.SubMessage { Subfield = "value2" }
                }
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void NullableWrappers_NaN()
        {
            var wrappers = new HelloRequest.Types.Wrappers
            {
                DoubleValue = double.NaN
            };

            AssertWrittenJson(wrappers);
        }

        [Fact]
        public void NullableWrappers()
        {
            var wrappers = new HelloRequest.Types.Wrappers
            {
                BoolValue = true,
                BytesValue = ByteString.CopyFrom(Encoding.UTF8.GetBytes("Hello world")),
                DoubleValue = 1.1,
                FloatValue = 1.2f,
                Int32Value = 1,
                Int64Value = 2l,
                StringValue = "A string",
                Uint32Value = 3u,
                Uint64Value = 4ul
            };

            AssertWrittenJson(wrappers);
        }

        [Fact]
        public void Any()
        {
            var helloRequest = new HelloRequest
            {
                Name = "In any!"
            };
            var any = Google.Protobuf.WellKnownTypes.Any.Pack(helloRequest);

            AssertWrittenJson(any);
        }

        [Fact]
        public void Any_WellKnownType()
        {
            var timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UnixEpoch);
            var any = Google.Protobuf.WellKnownTypes.Any.Pack(timestamp);

            AssertWrittenJson(any);
        }

        [Fact]
        public void Enum()
        {
            var dataTypes = new HelloRequest.Types.DataTypes
            {
                SingleEnum = HelloRequest.Types.DataTypes.Types.NestedEnum.Neg
            };

            AssertWrittenJson(dataTypes);
        }

        private void AssertWrittenJson<TValue>(TValue value, JsonSettings? settings = null) where TValue : IMessage
        {
            var typeRegistery = TypeRegistry.FromFiles(
                HelloRequest.Descriptor.File,
                Timestamp.Descriptor.File);

            settings = settings ?? new JsonSettings { TypeRegistry = typeRegistery };
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                },
                WriteIndented = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
            var converter = new JsonConverterFactoryForMessage(settings, jsonSerializerOptions);
            jsonSerializerOptions.Converters.Add(new AnyConverter(settings, jsonSerializerOptions));
            jsonSerializerOptions.Converters.Add(new TimestampConverter());
            jsonSerializerOptions.Converters.Add(converter);
            jsonSerializerOptions.Converters.Add(new ByteStringConverter());
            jsonSerializerOptions.Converters.Add(new Int64Converter());
            jsonSerializerOptions.Converters.Add(new UInt64Converter());
            jsonSerializerOptions.Converters.Add(new EnumConverter(settings));
            jsonSerializerOptions.Converters.Add(new BoolConverter());

            var jsonNew = JsonSerializer.Serialize(value, jsonSerializerOptions);

            _output.WriteLine("New:");
            _output.WriteLine(jsonNew);

            var formatter = new JsonFormatter(new JsonFormatter.Settings(
                formatDefaultValues: false,
                typeRegistery));

            var jsonOld = formatter.Format(value);

            _output.WriteLine("Old:");
            _output.WriteLine(jsonOld);

            using var doc1 = JsonDocument.Parse(jsonNew);
            using var doc2 = JsonDocument.Parse(jsonOld);

            var comparer = new JsonElementComparer();
            Assert.True(comparer.Equals(doc1.RootElement, doc2.RootElement));
        }
    }
}
