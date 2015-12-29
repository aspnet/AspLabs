// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Xunit;

namespace Microsoft.Azure.Applications.Storage
{
    public class StorageManagerTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly IStorageManager _manager;

        public StorageManagerTests()
        {
            _loggerMock = new Mock<ILogger>();
            _manager = new StorageManager(_loggerMock.Object);
        }

        public static TheoryData<string> InvalidConnectionStringData
        {
            get
            {
                return new TheoryData<string>
                {
                    null,
                    string.Empty,
                    "invalid",
                    "你好世界",
                };
            }
        }

        public static TheoryData<string> ValidConnectionStringData
        {
            get
            {
                return new TheoryData<string>
                {
                    "DefaultEndpointsProtocol=https;AccountName=invalid;AccountKey=7gd3Ln88FpnYtpRxqMYNr/qBjlo1x8+0NU69Rd5XbB1tVQ1Ty+5QVoCw2fwoSxUq046mDDZiUf3CwdPTfNvaBw==",
                    "UseDevelopmentStorage=true;"
                };
            }
        }

        public static TheoryData<string, string, string> PartitionKeyConstraintData
        {
            get
            {
                return new TheoryData<string, string, string>
                {
                    { string.Empty, string.Empty, "PartitionKey eq ''" },
                    { string.Empty, "12345", "PartitionKey eq '12345'" },
                    { "RowKey eq 'a'", "12345", "(RowKey eq 'a') and (PartitionKey eq '12345')" },
                    { string.Empty, "abcde", "PartitionKey eq 'abcde'" },
                    { "RowKey eq 'a'", "abcde", "(RowKey eq 'a') and (PartitionKey eq 'abcde')" },
                    { string.Empty, "ABCDE", "PartitionKey eq 'ABCDE'" },
                    { "RowKey eq 'a'", "ABCDE", "(RowKey eq 'a') and (PartitionKey eq 'ABCDE')" },
                    { string.Empty, "你好世界", "PartitionKey eq '你好世界'" },
                    { "RowKey eq 'a'", "你好世界", "(RowKey eq 'a') and (PartitionKey eq '你好世界')" },
                };
            }
        }

        public static TheoryData<Exception, string> StorageErrorMessageData
        {
            get
            {
                return new TheoryData<Exception, string>
                {
                    { null, string.Empty },
                    { new Exception("你好"), "你好" },
                    { new InvalidOperationException("你好"), "你好" },
                    { new StorageException("你好"), "你好" },
                    { new StorageException(new RequestResult { }, "你好", inner: null), string.Empty },
                };
            }
        }

        [Fact]
        public void GetAzureStorageConnectionString_ThrowsIfNotFound()
        {
            // Arrange
            SettingsDictionary settings = new SettingsDictionary();

            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _manager.GetAzureStorageConnectionString(settings));

            // Assert
            Assert.Equal("Please provide a Microsoft Azure Storage connection string with name 'MS_AzureStoreConnectionString' in the configuration string section of the 'Web.Config' file.", ex.Message);
        }

        [Fact]
        public void GetAzureStorageConnectionString_FindsConnectionString()
        {
            // Arrange
            SettingsDictionary settings = new SettingsDictionary();
            ConnectionSettings connection = new ConnectionSettings("MS_AzureStoreConnectionString", "connectionString");
            settings.Connections.Add("MS_AzureStoreConnectionString", connection);

            // Act
            string actual = _manager.GetAzureStorageConnectionString(settings);

            // Assert
            Assert.Equal("connectionString", actual);
        }

        [Theory]
        [MemberData("InvalidConnectionStringData")]
        public void GetCloudStorageAccount_Handles_InvalidConnectionStrings(string connectionString)
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _manager.GetCloudStorageAccount(connectionString));
            Assert.Contains("The connection string is invalid", ex.Message);
        }

        [Theory]
        [MemberData("ValidConnectionStringData")]
        public void GetCloudStorageAccount_Handles_ValidConnectionStrings(string connectionString)
        {
            // Act
            CloudStorageAccount actual = _manager.GetCloudStorageAccount(connectionString);

            // Assert
            Assert.NotNull(actual);
        }

        [Fact]
        public void GetCloudTable_HandlesInvalidTableName()
        {
            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _manager.GetCloudTable("UseDevelopmentStorage=true;", "I n v a l i d / N a m e"));

            // Assert
            Assert.StartsWith("Could not initialize connection to Microsoft Azure Storage: Bad Request", ex.Message);
        }

        [Theory]
        [MemberData("PartitionKeyConstraintData")]
        public void AddPartitionKeyConstraint_CreatesExpectedQuery(string filter, string partitionKey, string expected)
        {
            // Arrange
            TableQuery actual = new TableQuery { FilterString = filter };

            // Act
            _manager.AddPartitionKeyConstraint(actual, partitionKey);

            // Assert
            Assert.Equal(expected, actual.FilterString);
        }

        [Theory]
        [MemberData("StorageErrorMessageData")]
        public void GetStorageErrorMessage_ExtractsMessage(Exception exception, string expected)
        {
            // Act
            string actual = _manager.GetStorageErrorMessage(exception);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
