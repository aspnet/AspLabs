// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using HttpApi;
using Microsoft.AspNetCore.Grpc.Swagger;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Grpc.AspNetCore.Server.Tests.HttpApi
{
    [TestFixture]
    public class SchemaGeneratorIntegrationTests
    {
        private (OpenApiSchema Schema, SchemaRepository SchemaRepository) GenerateSchema(System.Type type)
        {
            var dataContractResolver = new GrpcDataContractResolver(new JsonSerializerDataContractResolver(new JsonSerializerOptions()));
            var schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), dataContractResolver);
            var schemaRepository = new SchemaRepository();

            var schema = schemaGenerator.GenerateSchema(type, schemaRepository);

            return (schema, schemaRepository);
        }

        [Test]
        public void GenerateSchema_EnumValue_ReturnSchema()
        {
            // Arrange & Act
            var (schema, repository) = GenerateSchema(typeof(EnumMessage));

            // Assert
            schema = repository.Schemas[schema.Reference.Id];
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(1, schema.Properties.Count);

            var enumSchema = repository.Schemas[schema.Properties["enumValue"].Reference.Id];
            Assert.AreEqual("string", enumSchema.Type);
            Assert.AreEqual(5, enumSchema.Enum.Count);
            Assert.AreEqual("NESTED_ENUM_UNSPECIFIED", ((OpenApiString)enumSchema.Enum[0]).Value);
            Assert.AreEqual("FOO", ((OpenApiString)enumSchema.Enum[1]).Value);
            Assert.AreEqual("BAR", ((OpenApiString)enumSchema.Enum[2]).Value);
            Assert.AreEqual("BAZ", ((OpenApiString)enumSchema.Enum[3]).Value);
            Assert.AreEqual("NEG", ((OpenApiString)enumSchema.Enum[4]).Value);
        }

        [Test]
        public void GenerateSchema_BasicMessage_ReturnSchema()
        {
            // Arrange & Act
            var (schema, repository) = GenerateSchema(typeof(HelloReply));

            // Assert
            schema = repository.Schemas[schema.Reference.Id];
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(2, schema.Properties.Count);
            Assert.AreEqual("string", schema.Properties["message"].Type);
            var valuesSchema = schema.Properties["values"];
            Assert.AreEqual("array", valuesSchema.Type);
            Assert.NotNull(valuesSchema.Items);
            Assert.AreEqual("string", valuesSchema.Items.Type);
        }

        [Test]
        public void GenerateSchema_RecursiveMessage_ReturnSchema()
        {
            // Arrange & Act
            var (schema, repository) = GenerateSchema(typeof(RecursiveMessage));

            // Assert
            schema = repository.Schemas[schema.Reference.Id];
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(1, schema.Properties.Count);
            Assert.AreEqual("RecursiveMessage", schema.Properties["child"].Reference.Id);
        }

        [Test]
        public void GenerateSchema_BytesMessage_ReturnSchema()
        {
            // Arrange & Act
            var (schema, repository) = GenerateSchema(typeof(BytesMessage));

            // Assert
            schema = repository.Schemas[schema.Reference.Id];
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(2, schema.Properties.Count);
            Assert.AreEqual("string", schema.Properties["bytesValue"].Type);
            Assert.AreEqual("string", schema.Properties["bytesNullableValue"].Type);
        }

        [Test]
        public void GenerateSchema_ListValues_ReturnSchema()
        {
            // Arrange & Act
            var (schema, _) = GenerateSchema(typeof(ListValue));

            // Assert
            Assert.AreEqual("array", schema.Type);
            Assert.IsNotNull(schema.Items);
            Assert.AreEqual(null, schema.Items.Type);
        }

        [Test]
        public void GenerateSchema_Struct_ReturnSchema()
        {
            // Arrange & Act
            var (schema, _) = GenerateSchema(typeof(Struct));

            // Assert
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(0, schema.Properties.Count);
            Assert.IsNotNull(schema.AdditionalProperties);
            Assert.AreEqual(null, schema.AdditionalProperties.Type);
        }

        [Test]
        public void GenerateSchema_Any_ReturnSchema()
        {
            // Arrange & Act
            var (schema, repository) = GenerateSchema(typeof(Any));

            // Assert
            schema = repository.Schemas[schema.Reference.Id];
            Assert.AreEqual("object", schema.Type);
            Assert.IsNotNull(schema.AdditionalProperties);
            Assert.AreEqual(null, schema.AdditionalProperties.Type);
            Assert.AreEqual(1, schema.Properties.Count);
            Assert.AreEqual("string", schema.Properties["@type"].Type);
        }

        [Test]
        public void GenerateSchema_OneOf_ReturnSchema()
        {
            // Arrange & Act
            var (schema, repository) = GenerateSchema(typeof(OneOfMessage));

            // Assert
            schema = repository.Schemas[schema.Reference.Id];
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(4, schema.Properties.Count);
            Assert.AreEqual("string", schema.Properties["firstOne"].Type);
            Assert.AreEqual("string", schema.Properties["firstTwo"].Type);
            Assert.AreEqual("string", schema.Properties["secondOne"].Type);
            Assert.AreEqual("string", schema.Properties["secondTwo"].Type);
            Assert.IsNull(schema.AdditionalProperties);
        }

        [Test]
        public void GenerateSchema_Map_ReturnSchema()
        {
            // Arrange & Act
            var (schema, repository) = GenerateSchema(typeof(MapMessage));

            // Assert
            schema = repository.Schemas[schema.Reference.Id];
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(1, schema.Properties.Count);
            Assert.AreEqual("object", schema.Properties["mapValue"].Type);
            Assert.AreEqual("number", schema.Properties["mapValue"].AdditionalProperties.Type);
            Assert.AreEqual("double", schema.Properties["mapValue"].AdditionalProperties.Format);
        }
    }
}
