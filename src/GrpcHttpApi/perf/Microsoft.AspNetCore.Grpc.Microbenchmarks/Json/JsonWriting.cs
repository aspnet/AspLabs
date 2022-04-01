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
    public class JsonWriting
    {
        private HelloRequest _request = default!;
        private JsonSerializerOptions _serializerOptions = default!;
        private JsonFormatter _jsonFormatter = default!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _request = new HelloRequest() { Name = "Hello world" };
            _serializerOptions = JsonConverterHelper.CreateSerializerOptions(new JsonSettings { WriteIndented = false });
            _jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(formatDefaultValues: false));
        }

        [Benchmark]
        public void WriteMessage_JsonSerializer()
        {
            JsonSerializer.Serialize(_request, _serializerOptions);
        }

        [Benchmark]
        public void WriteMessage_JsonFormatter()
        {
            _jsonFormatter.Format(_request);
        }
    }
}
