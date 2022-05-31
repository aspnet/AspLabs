// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using HttpApi;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.ConverterTests
{
    public class JsonConverterWriteTests
    {
        private readonly ITestOutputHelper _output;

        public JsonConverterWriteTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void NonAsciiString()
        {
            var helloRequest = new HelloRequest
            {
                Name = "This is a test 激光這兩個字是甚麼意思 string"
            };

            AssertWrittenJson(helloRequest, compareRawStrings: true);
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
        public void RepeatedDoubleValues()
        {
            var helloRequest = new HelloRequest
            {
                RepeatedDoubleValues =
                {
                    1,
                    1.1
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
        public void DataTypes_DefaultValues()
        {
            var wrappers = new HelloRequest.Types.DataTypes();

            AssertWrittenJson(wrappers, new JsonSettings { FormatDefaultValues = true });
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
        public void NullValue_Default()
        {
            var m = new NullValueContainer();

            AssertWrittenJson(m, new JsonSettings { FormatDefaultValues = true });
        }

        [Fact]
        public void NullValue_NonDefaultValue()
        {
            var m = new NullValueContainer
            {
                NullValue = (NullValue)1
            };

            AssertWrittenJson(m, new JsonSettings { FormatDefaultValues = true });
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
                Int64Value = 2L,
                StringValue = "A string",
                Uint32Value = 3U,
                Uint64Value = 4UL
            };

            AssertWrittenJson(wrappers);
        }

        [Fact]
        public void NullableWrapper_Root_Int32()
        {
            var v = new Int32Value { Value = 1 };

            AssertWrittenJson(v);
        }

        [Fact]
        public void NullableWrapper_Root_Int64()
        {
            var v = new Int64Value { Value = 1 };

            AssertWrittenJson(v);
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
        public void Any_WellKnownType_Timestamp()
        {
            var timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UnixEpoch);
            var any = Google.Protobuf.WellKnownTypes.Any.Pack(timestamp);

            AssertWrittenJson(any);
        }

        [Fact]
        public void Any_WellKnownType_Int32()
        {
            var value = new Int32Value() { Value = int.MaxValue };
            var any = Google.Protobuf.WellKnownTypes.Any.Pack(value);

            AssertWrittenJson(any);
        }

        [Fact]
        public void Timestamp_Nested()
        {
            var helloRequest = new HelloRequest
            {
                TimestampValue = Timestamp.FromDateTimeOffset(new DateTimeOffset(2020, 12, 1, 12, 30, 0, TimeSpan.FromHours(12)))
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void Timestamp_Root()
        {
            var ts = Timestamp.FromDateTimeOffset(new DateTimeOffset(2020, 12, 1, 12, 30, 0, TimeSpan.FromHours(12)));

            AssertWrittenJson(ts);
        }

        [Fact]
        public void Duration_Nested()
        {
            var helloRequest = new HelloRequest
            {
                DurationValue = Duration.FromTimeSpan(TimeSpan.FromHours(12))
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void Duration_Root()
        {
            var duration = Duration.FromTimeSpan(TimeSpan.FromHours(12));

            AssertWrittenJson(duration);
        }

        [Fact]
        public void Value_Nested()
        {
            var helloRequest = new HelloRequest
            {
                ValueValue = Value.ForStruct(new Struct
                {
                    Fields =
                    {
                        ["enabled"] = Value.ForBool(true),
                        ["metadata"] = Value.ForList(
                            Value.ForString("value1"),
                            Value.ForString("value2"))
                    }
                })
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void Value_Root()
        {
            var value = Value.ForStruct(new Struct
            {
                Fields =
                {
                    ["enabled"] = Value.ForBool(true),
                    ["metadata"] = Value.ForList(
                        Value.ForString("value1"),
                        Value.ForString("value2"))
                }
            });

            AssertWrittenJson(value);
        }

        [Fact]
        public void Struct_Nested()
        {
            var helloRequest = new HelloRequest
            {
                StructValue = new Struct
                {
                    Fields =
                    {
                        ["enabled"] = Value.ForBool(true),
                        ["metadata"] = Value.ForList(
                            Value.ForString("value1"),
                            Value.ForString("value2"))
                    }
                }
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void Struct_Root()
        {
            var value = new Struct
            {
                Fields =
                {
                    ["enabled"] = Value.ForBool(true),
                    ["metadata"] = Value.ForList(
                        Value.ForString("value1"),
                        Value.ForString("value2"))
                }
            };

            AssertWrittenJson(value);
        }

        [Fact]
        public void ListValue_Nested()
        {
            var helloRequest = new HelloRequest
            {
                ListValue = new ListValue
                {
                    Values =
                    {
                        Value.ForBool(true),
                        Value.ForString("value1"),
                        Value.ForString("value2")
                    }
                }
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void ListValue_Root()
        {
            var value = new ListValue
            {
                Values =
                {
                    Value.ForBool(true),
                    Value.ForString("value1"),
                    Value.ForString("value2")
                }
            };

            AssertWrittenJson(value);
        }

        [Fact]
        public void FieldMask_Nested()
        {
            var helloRequest = new HelloRequest
            {
                FieldMaskValue = FieldMask.FromString("value1,value2,value3.nested_value"),
            };

            AssertWrittenJson(helloRequest);
        }

        [Fact]
        public void FieldMask_Root()
        {
            var m = FieldMask.FromString("value1,value2,value3.nested_value");

            AssertWrittenJson(m);
        }

        [Theory]
        [InlineData(HelloRequest.Types.DataTypes.Types.NestedEnum.Unspecified)]
        [InlineData(HelloRequest.Types.DataTypes.Types.NestedEnum.Bar)]
        [InlineData(HelloRequest.Types.DataTypes.Types.NestedEnum.Neg)]
        [InlineData((HelloRequest.Types.DataTypes.Types.NestedEnum)100)]
        public void Enum(HelloRequest.Types.DataTypes.Types.NestedEnum value)
        {
            var dataTypes = new HelloRequest.Types.DataTypes
            {
                SingleEnum = value
            };

            AssertWrittenJson(dataTypes);
        }

        [Theory]
        [InlineData(HelloRequest.Types.DataTypes.Types.NestedEnum.Unspecified)]
        [InlineData(HelloRequest.Types.DataTypes.Types.NestedEnum.Bar)]
        [InlineData(HelloRequest.Types.DataTypes.Types.NestedEnum.Neg)]
        [InlineData((HelloRequest.Types.DataTypes.Types.NestedEnum)100)]
        public void Enum_WriteNumber(HelloRequest.Types.DataTypes.Types.NestedEnum value)
        {
            var dataTypes = new HelloRequest.Types.DataTypes
            {
                SingleEnum = value
            };

            AssertWrittenJson(dataTypes, new JsonSettings { FormatEnumsAsIntegers = true, FormatDefaultValues = false });
        }

        private void AssertWrittenJson<TValue>(TValue value, JsonSettings? settings = null, bool? compareRawStrings = null) where TValue : IMessage
        {
            var typeRegistery = TypeRegistry.FromFiles(
                HelloRequest.Descriptor.File,
                Timestamp.Descriptor.File);

            settings = settings ?? new JsonSettings { TypeRegistry = typeRegistery, FormatDefaultValues = false };

            var jsonSerializerOptions = CreateSerializerOptions(settings, typeRegistery);

            var formatterSettings = new JsonFormatter.Settings(
                formatDefaultValues: settings.FormatDefaultValues,
                typeRegistery);
            formatterSettings = formatterSettings.WithFormatEnumsAsIntegers(settings.FormatEnumsAsIntegers);
            var formatter = new JsonFormatter(formatterSettings);

            var jsonOld = formatter.Format(value);

            _output.WriteLine("Old:");
            _output.WriteLine(jsonOld);

            var jsonNew = JsonSerializer.Serialize(value, jsonSerializerOptions);

            _output.WriteLine("New:");
            _output.WriteLine(jsonNew);

            using var doc1 = JsonDocument.Parse(jsonNew);
            using var doc2 = JsonDocument.Parse(jsonOld);

            var comparer = new JsonElementComparer(maxHashDepth: -1, compareRawStrings: compareRawStrings ?? false);
            Assert.True(comparer.Equals(doc1.RootElement, doc2.RootElement));
        }

        internal static JsonSerializerOptions CreateSerializerOptions(JsonSettings? settings, TypeRegistry typeRegistery)
        {
            var resolvedSettings = settings ?? new JsonSettings { TypeRegistry = typeRegistery };
            return JsonConverterHelper.CreateSerializerOptions(resolvedSettings);
        }
    }
}
