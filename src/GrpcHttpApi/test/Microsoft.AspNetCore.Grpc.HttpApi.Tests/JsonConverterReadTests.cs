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
    public class JsonConverterReadTests
    {
        private readonly ITestOutputHelper _output;

        public JsonConverterReadTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ReadObjectProperties()
        {
            var json = @"{
  ""name"": ""test"",
  ""age"": 1
}";

            AssertReadJson<HelloRequest>(json);
        }

        [Fact]
        public void RepeatedStrings()
        {
            var json = @"{
  ""name"": ""test"",
  ""repeatedStrings"": [
    ""One"",
    ""Two"",
    ""Three""
  ]
}";

            AssertReadJson<HelloRequest>(json);
        }

        [Fact]
        public void RepeatedDoubleValues()
        {
            var json = @"{
  ""repeatedDoubleValues"": [
    1,
    1.1
  ]
}";

            AssertReadJson<HelloRequest>(json);
        }

        [Fact]
        public void Any()
        {
            var json = @"{
  ""@type"": ""type.googleapis.com/http_api.HelloRequest"",
  ""name"": ""In any!""
}";

            AssertReadJson<Any>(json);
        }

        [Fact]
        public void Any_WellKnownType()
        {
            var json = @"{
  ""@type"": ""type.googleapis.com/google.protobuf.Timestamp"",
  ""value"": ""1970-01-01T00:00:00Z""
}";

            AssertReadJson<Any>(json);
        }

        [Fact]
        public void MapMessages()
        {
            var json = @"{
  ""mapMessage"": {
    ""name1"": {
      ""subfield"": ""value1""
    },
    ""name2"": {
      ""subfield"": ""value2""
    }
  }
}";

            AssertReadJson<HelloRequest>(json);
        }

        [Fact]
        public void MapKeyBool()
        {
            var json = @"{
  ""mapKeybool"": {
    ""true"": ""value1"",
    ""false"": ""value2""
  }
}";

            AssertReadJson<HelloRequest>(json);
        }

        [Fact]
        public void MapKeyInt()
        {
            var json = @"{
  ""mapKeyint"": {
    ""-1"": ""value1"",
    ""0"": ""value3""
  }
}";

            AssertReadJson<HelloRequest>(json);
        }

        [Fact]
        public void OneOf_Success()
        {
            var json = @"{
  ""oneofName1"": ""test""
}";

            AssertReadJson<HelloRequest>(json);
        }

        [Fact]
        public void OneOf_Failure()
        {
            var json = @"{
  ""oneofName1"": ""test"",
  ""oneofName2"": ""test""
}";

            AssertReadJsonError<HelloRequest>(json, ex => Assert.Equal("Multiple values specified for oneof oneof_test", ex.Message));
        }

        [Fact]
        public void NullableWrappers_NaN()
        {
            var json = @"{
  ""doubleValue"": ""NaN""
}";

            AssertReadJson<HelloRequest.Types.Wrappers>(json);
        }

        [Fact]
        public void NullableWrappers()
        {
            var json = @"{
  ""stringValue"": ""A string"",
  ""int32Value"": 1,
  ""int64Value"": ""2"",
  ""floatValue"": 1.2,
  ""doubleValue"": 1.1,
  ""boolValue"": true,
  ""uint32Value"": 3,
  ""uint64Value"": ""4"",
  ""bytesValue"": ""SGVsbG8gd29ybGQ=""
}";

            AssertReadJson<HelloRequest.Types.Wrappers>(json);
        }

        private TValue AssertReadJson<TValue>(string value, JsonSettings? settings = null) where TValue : IMessage, new()
        {
            var typeRegistery = TypeRegistry.FromFiles(
                HelloRequest.Descriptor.File,
                Timestamp.Descriptor.File);

            var jsonSerializerOptions = JsonConverterHelper.CreateSerializerOptions(settings, typeRegistery);

            var objectNew = JsonSerializer.Deserialize<TValue>(value, jsonSerializerOptions)!;

            _output.WriteLine("New:");
            _output.WriteLine(objectNew.ToString());

            var formatter = new JsonParser(new JsonParser.Settings(
                recursionLimit: int.MaxValue,
                typeRegistery));

            var objectOld = formatter.Parse<TValue>(value);

            _output.WriteLine("Old:");
            _output.WriteLine(objectOld.ToString());

            Assert.True(objectNew.Equals(objectOld));

            return objectNew;
        }

        private void AssertReadJsonError<TValue>(string value, Action<Exception> assertException, JsonSettings? settings = null) where TValue : IMessage, new()
        {
            var typeRegistery = TypeRegistry.FromFiles(
                HelloRequest.Descriptor.File,
                Timestamp.Descriptor.File);

            var jsonSerializerOptions = JsonConverterHelper.CreateSerializerOptions(settings, typeRegistery);

            var ex = Assert.ThrowsAny<Exception>(() => JsonSerializer.Deserialize<TValue>(value, jsonSerializerOptions));
            assertException(ex);

            var formatter = new JsonParser(new JsonParser.Settings(
                recursionLimit: int.MaxValue,
                typeRegistery));

            ex = Assert.ThrowsAny<Exception>(() => formatter.Parse<TValue>(value));
            assertException(ex);
        }
    }
}
