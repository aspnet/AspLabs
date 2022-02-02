// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Google.Protobuf;
using Greet;
using Microsoft.AspNetCore.Grpc.HttpApi;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json;

namespace Microsoft.AspNetCore.Grpc.Microbenchmarks.Json
{
    public class JsonReading
    {
        private string _requestJson = default!;
        private JsonSerializerOptions _serializerOptions = default!;
        private JsonParser _jsonFormatter = default!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _requestJson = (new HelloRequest() { Name = "Hello world" }).ToString();
            _serializerOptions = JsonConverterHelper.CreateSerializerOptions(new JsonSettings { WriteIndented = false });
            _jsonFormatter = new JsonParser(new JsonParser.Settings(recursionLimit: 100));
        }

        [Benchmark]
        public void ReadMessage_JsonSerializer()
        {
            JsonSerializer.Deserialize(_requestJson, typeof(HelloRequest), _serializerOptions);
        }

        [Benchmark]
        public void ReadMessage_JsonFormatter()
        {
            _jsonFormatter.Parse(_requestJson, HelloRequest.Descriptor);
        }
    }
}
