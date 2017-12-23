// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Storage
{
    [Collection("StoreCollection")]
    public class StorageManagerTests
    {
        private const string TestPartition = "12345";
        private const int MaxDataEntries = 16;

        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);
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
            var settings = new SettingsDictionary();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => _manager.GetAzureStorageConnectionString(settings));

            // Assert
            Assert.Equal("Please provide a Microsoft Azure Storage connection string with name 'MS_AzureStoreConnectionString' in the configuration string section of the 'Web.Config' file.", ex.Message);
        }

        [Fact]
        public void GetAzureStorageConnectionString_FindsConnectionString()
        {
            // Arrange
            var settings = new SettingsDictionary();
            var connection = new ConnectionSettings("MS_AzureStoreConnectionString", "connectionString");
            settings.Connections.Add("MS_AzureStoreConnectionString", connection);

            // Act
            var actual = _manager.GetAzureStorageConnectionString(settings);

            // Assert
            Assert.Equal("connectionString", actual);
        }

        [Theory]
        [MemberData(nameof(InvalidConnectionStringData))]
        public void GetCloudStorageAccount_Handles_InvalidConnectionStrings(string connectionString)
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _manager.GetCloudStorageAccount(connectionString));
            Assert.Contains("The connection string is invalid", ex.Message);
        }

        [Theory]
        [MemberData(nameof(ValidConnectionStringData))]
        public void GetCloudStorageAccount_Handles_ValidConnectionStrings(string connectionString)
        {
            // Act
            var actual = _manager.GetCloudStorageAccount(connectionString);

            // Assert
            Assert.NotNull(actual);
        }

        [Fact]
        public void GetCloudTable_HandlesInvalidTableName()
        {
            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => _manager.GetCloudTable("UseDevelopmentStorage=true;", "I n v a l i d / N a m e"));

            // Assert
            Assert.StartsWith("Could not initialize connection to Microsoft Azure Storage: Bad Request", ex.Message);
        }

        [Fact]
        public void GetCloudQueue_HandlesInvalidQueueName()
        {
            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => _manager.GetCloudQueue("UseDevelopmentStorage=true;", "I n v a l i d / N a m e"));

            // Assert
            Assert.StartsWith("Could not initialize connection to Microsoft Azure Storage: The requested URI does not represent any resource on the server. ", ex.Message);
        }

        [Theory]
        [MemberData(nameof(PartitionKeyConstraintData))]
        public void AddPartitionKeyConstraint_CreatesExpectedQuery(string filter, string partitionKey, string expected)
        {
            // Arrange
            var actual = new TableQuery { FilterString = filter };

            // Act
            _manager.AddPartitionKeyConstraint(actual, partitionKey);

            // Assert
            Assert.Equal(expected, actual.FilterString);
        }

        [Fact]
        public async Task ExecuteRetrieval_ReturnsNotFound_IfInvalidTable()
        {
            // Arrange
            var storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true;");
            var client = storageAccount.CreateCloudTableClient();
            var table = client.GetTableReference("unknown");

            // Act
            var actual = await _manager.ExecuteRetrievalAsync(table, TestPartition, "data");

            // Assert
            Assert.Equal(404, actual.HttpStatusCode);
        }

        [Fact]
        public async Task ExecuteRetrieval_ReturnsNotFound_IfNotFoundRowKey()
        {
            // Arrange
            var table = InitializeTable();
            await CreateTableRows(table);

            // Act
            var actual = await _manager.ExecuteRetrievalAsync(table, TestPartition, "unknown");

            // Assert
            Assert.Equal(404, actual.HttpStatusCode);
        }

        [Fact]
        public async Task ExecuteRetrieval_ReturnsNotFound_IfNotFoundPartitionKey()
        {
            // Arrange
            var table = InitializeTable();
            await CreateTableRows(table);

            // Act
            var actual = await _manager.ExecuteRetrievalAsync(table, "unknown", "data 0");

            // Assert
            Assert.Equal(404, actual.HttpStatusCode);
        }

        [Fact]
        public async Task ExecuteRetrieval_ReturnsData_IfFound()
        {
            // Arrange
            var table = InitializeTable();
            await CreateTableRows(table);

            // Act
            var actual = await _manager.ExecuteRetrievalAsync(table, TestPartition, "data 0");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(200, actual.HttpStatusCode);
            Assert.IsType<DynamicTableEntity>(actual.Result);
            var actualEntity = (DynamicTableEntity)actual.Result;
            Assert.Equal("data 0", actualEntity.RowKey);
            Assert.Equal(TestPartition, actualEntity.PartitionKey);
        }

        [Fact]
        public async Task ExecuteQuery_ReturnsEmptyCollection_IfNotFoundRowKey()
        {
            // Arrange
            var table = InitializeTable();
            await CreateTableRows(table);
            var query = new TableQuery { FilterString = "PartitionKey eq 'Unknown'" };

            // Act
            var actual = await _manager.ExecuteQueryAsync(table, query);

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public async Task ExecuteQuery_ReturnsCollection_IfFound()
        {
            // Arrange
            var table = InitializeTable();
            await CreateTableRows(table, rowCount: 1024);
            await CreateTableRows(table, partitionKey: "Other");
            var query = new TableQuery { FilterString = "PartitionKey eq '" + TestPartition + "'" };

            // Act
            var actual = await _manager.ExecuteQueryAsync(table, query);

            // Assert
            Assert.Equal(1024, actual.Count());
        }

        [Fact]
        public async Task Execute_ReturnsStatus_IfError()
        {
            // Arrange
            var table = InitializeTable();
            await CreateTableRows(table);
            ITableEntity entity = new DynamicTableEntity(TestPartition, "data 0");
            var operation = TableOperation.Insert(entity);

            // Act
            var actual = await _manager.ExecuteAsync(table, operation);

            // Assert
            Assert.Equal(409, actual.HttpStatusCode);
        }

        [Fact]
        public async Task Execute_ReturnsStatus_IfSuccess()
        {
            // Arrange
            var table = InitializeTable();
            await CreateTableRows(table);
            ITableEntity entity = new DynamicTableEntity(TestPartition, "new entry");
            var operation = TableOperation.Insert(entity);

            // Act
            var actual = await _manager.ExecuteAsync(table, operation);

            // Assert
            Assert.Equal(204, actual.HttpStatusCode);
        }

        [Fact]
        public async Task ExecuteBatch_ReturnsEmptyList_IfError()
        {
            // Arrange
            var table = InitializeTable();

            ITableEntity entity = new DynamicTableEntity(TestPartition, "data");
            var operation = TableOperation.Insert(entity);

            var batch = new TableBatchOperation
            {
                operation,
                operation
            };

            // Act
            IEnumerable<TableResult> actual = await _manager.ExecuteBatchAsync(table, batch);

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public async Task ExecuteBatch_ReturnsResults_OnSuccess()
        {
            // Arrange
            var table = InitializeTable();

            ITableEntity entity1 = new DynamicTableEntity(TestPartition, "data A");
            var operation1 = TableOperation.Insert(entity1);

            ITableEntity entity2 = new DynamicTableEntity(TestPartition, "data B");
            var operation2 = TableOperation.Insert(entity2);

            var batch = new TableBatchOperation
            {
                operation1,
                operation2
            };

            // Act
            ICollection<TableResult> actual = await _manager.ExecuteBatchAsync(table, batch);

            // Assert
            Assert.Equal(2, actual.Count);
            Assert.Equal(204, actual.ElementAt(0).HttpStatusCode);
            Assert.Equal(204, actual.ElementAt(1).HttpStatusCode);
        }

        [Fact]
        public async Task ExecuteDeleteAll_HandlesEmptySet()
        {
            // Arrange
            var table = InitializeTable();
            await CreateTableRows(table);

            // Act
            var actual = await _manager.ExecuteDeleteAllAsync(table, TestPartition, filter: "RowKey eq 'Unknown'");

            // Assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public async Task ExecuteDeleteAll_ReturnsResults_OnSuccess()
        {
            // Arrange
            var table = InitializeTable();
            await CreateTableRows(table, rowCount: 1024);
            await CreateTableRows(table, partitionKey: "Other");

            // Act
            var actual = await _manager.ExecuteDeleteAllAsync(table, TestPartition, filter: null);
            var remaining = await _manager.ExecuteQueryAsync(table, new TableQuery { FilterString = "PartitionKey eq 'Other'" });

            // Assert
            Assert.Equal(1024, actual);
            Assert.Equal(16, remaining.Count());
        }

        [Fact]
        public async Task GetMessages_HandlesEmptyQueue()
        {
            // Arrange
            var queue = InitializeQueue();

            // Act
            var actual = await _manager.GetMessagesAsync(queue, 16, _timeout);

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public async Task AddMessagesGetMessages_Roundtrips()
        {
            // Arrange
            var queue = InitializeQueue();
            var expected = CreateQueueMessages();

            // Act
            await _manager.AddMessagesAsync(queue, expected);
            var actual = await _manager.GetMessagesAsync(queue, 16, _timeout);

            // Assert
            Assert.Equal(expected.Count(), actual.Count());
        }

        [Fact]
        public async Task DeleteMessages_EmptiesQueue()
        {
            // Arrange
            var queue = InitializeQueue();
            var messages = CreateQueueMessages();
            await _manager.AddMessagesAsync(queue, messages);

            // Act
            var initial = await _manager.GetMessagesAsync(queue, MaxDataEntries, _timeout);
            await _manager.DeleteMessagesAsync(queue, initial);
            var final = await _manager.GetMessagesAsync(queue, MaxDataEntries, _timeout);

            // Assert
            Assert.Equal(MaxDataEntries, initial.Count());
            Assert.Equal(0, final.Count());
        }

        [Fact]
        public async Task DeleteMessages_HandlesDoubleDeletion()
        {
            // Arrange
            var queue = InitializeQueue();
            var messages = CreateQueueMessages();
            await _manager.AddMessagesAsync(queue, messages);

            // Act
            var initial = await _manager.GetMessagesAsync(queue, MaxDataEntries, _timeout);
            await _manager.DeleteMessagesAsync(queue, initial);
            await _manager.DeleteMessagesAsync(queue, initial);
            var final = await _manager.GetMessagesAsync(queue, MaxDataEntries, _timeout);

            // Assert
            Assert.Equal(MaxDataEntries, initial.Count());
            Assert.Equal(0, final.Count());
        }

        [Fact]
        public async Task DeleteMessages_HandlesInvalidMessage()
        {
            // Arrange
            var queue = InitializeQueue();
            var message = new CloudQueueMessage("invalid");

            // Act
            await _manager.DeleteMessagesAsync(queue, new[] { message });
        }

        [Theory]
        [MemberData(nameof(StorageErrorMessageData))]
        public void GetStorageErrorMessage_ExtractsMessage(Exception exception, string expected)
        {
            // Act
            var actual = _manager.GetStorageErrorMessage(exception);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetInstance_ReturnsSingletonInstance()
        {
            // Act
            var actual1 = StorageManager.GetInstance(_loggerMock.Object);
            var actual2 = StorageManager.GetInstance(_loggerMock.Object);

            // Assert
            Assert.NotNull(actual1);
            Assert.Same(actual1, actual2);
        }

        private CloudTable InitializeTable()
        {
            var table = _manager.GetCloudTable("UseDevelopmentStorage=true;", "storagetest");
            table.DeleteIfExists();
            table.Create();
            return table;
        }

        private async Task CreateTableRows(CloudTable table, string partitionKey = TestPartition, int rowCount = MaxDataEntries)
        {
            var count = 0;
            var segmentCount = 0;
            do
            {
                var batchCount = Math.Min(rowCount - segmentCount, 100);

                var batch = new TableBatchOperation();
                for (var i = 0; i < batchCount; i++)
                {
                    ITableEntity entity = new DynamicTableEntity(partitionKey, "data " + count++);
                    var operation = TableOperation.Insert(entity);
                    batch.Add(operation);
                }

                await _manager.ExecuteBatchAsync(table, batch);

                segmentCount += batchCount;
            }
            while (segmentCount < rowCount);
        }

        private CloudQueue InitializeQueue()
        {
            var queue = _manager.GetCloudQueue("UseDevelopmentStorage=true;", "test");
            queue.DeleteIfExists();
            queue.Create();
            return queue;
        }

        private IEnumerable<CloudQueueMessage> CreateQueueMessages()
        {
            var messages = new CloudQueueMessage[MaxDataEntries];
            for (var i = 0; i < MaxDataEntries; i++)
            {
                messages[i] = new CloudQueueMessage("data " + i);
            }
            return messages;
        }
    }
}
