// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using HttpApi;
using Microsoft.AspNetCore.Grpc.HttpApi;
using NUnit.Framework;

namespace Grpc.AspNetCore.Server.Tests.HttpApi
{
    [TestFixture]
    public class MessageSchemaGeneratorTests
    {
        [Test]
        public void GenerateSchema_BasicMessage_ReturnSchema()
        {
            // Arrange
            var generator = new MessageSchemaGenerator(HelloReply.Descriptor);

            // Act
            var schema = generator.GenerateSchema();

            // Assert
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
            // Arrange
            var generator = new MessageSchemaGenerator(RecursiveMessage.Descriptor);

            // Act
            var schema = generator.GenerateSchema();

            // Assert
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(1, schema.Properties.Count);
            Assert.AreEqual(schema, schema.Properties["child"]);
        }

        [Test]
        public void GenerateSchema_BytesMessage_ReturnSchema()
        {
            // Arrange
            var generator = new MessageSchemaGenerator(BytesMessage.Descriptor);

            // Act
            var schema = generator.GenerateSchema();

            // Assert
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(2, schema.Properties.Count);
            Assert.AreEqual("string", schema.Properties["bytesValue"].Type);
            Assert.AreEqual("string", schema.Properties["bytesNullableValue"].Type);
        }

        [Test]
        public void GenerateSchema_ListValues_ReturnSchema()
        {
            // Arrange
            var generator = new MessageSchemaGenerator(ListValue.Descriptor);

            // Act
            var schema = generator.GenerateSchema();

            // Assert
            Assert.AreEqual("array", schema.Type);
            Assert.IsNull(schema.Items);
        }

        [Test]
        public void GenerateSchema_Struct_ReturnSchema()
        {
            // Arrange
            var generator = new MessageSchemaGenerator(Struct.Descriptor);

            // Act
            var schema = generator.GenerateSchema();

            // Assert
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(0, schema.Properties.Count);
        }

        [Test]
        public void GenerateSchema_Any_ReturnSchema()
        {
            // Arrange
            var generator = new MessageSchemaGenerator(Any.Descriptor);

            // Act
            var schema = generator.GenerateSchema();

            // Assert
            Assert.AreEqual("object", schema.Type);
            Assert.AreEqual(1, schema.Properties.Count);
            Assert.AreEqual("string", schema.Properties["@type"].Type);
        }
    }
}
