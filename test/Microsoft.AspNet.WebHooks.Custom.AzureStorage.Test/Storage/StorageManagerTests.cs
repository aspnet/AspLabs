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

        [Fact]
        public void GetCloudQueue_HandlesInvalidQueueName()
        {
            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => _manager.GetCloudQueue("UseDevelopmentStorage=true;", "I n v a l i d / N a m e"));

            // Assert
            Assert.StartsWith("Could not initialize connection to Microsoft Azure Storage: The requested URI does not represent any resource on the server. ", ex.Message);
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

        [Fact]
        public async Task ExecuteRetrieval_ReturnsNotFound_IfInvalidTable()
        {
            // Arrange
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true;");
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference("unknown");

            // Act
            TableResult actual = await _manager.ExecuteRetrievalAsync(table, TestPartition, "data");

            // Assert
            Assert.Equal(404, actual.HttpStatusCode);
        }

        [Fact]
        public async Task ExecuteRetrieval_ReturnsNotFound_IfNotFoundRowKey()
        {
            // Arrange
            CloudTable table = InitializeTable();
            await CreateTableRows(table);

            // Act
            TableResult actual = await _manager.ExecuteRetrievalAsync(table, TestPartition, "unknown");

            // Assert
            Assert.Equal(404, actual.HttpStatusCode);
        }

        [Fact]
        public async Task ExecuteRetrieval_ReturnsNotFound_IfNotFoundPartitionKey()
        {
            // Arrange
            CloudTable table = InitializeTable();
            await CreateTableRows(table);

            // Act
            TableResult actual = await _manager.ExecuteRetrievalAsync(table, "unknown", "data 0");

            // Assert
            Assert.Equal(404, actual.HttpStatusCode);
        }

        [Fact]
        public async Task ExecuteRetrieval_ReturnsData_IfFound()
        {
            // Arrange
            CloudTable table = InitializeTable();
            await CreateTableRows(table);

            // Act
            TableResult actual = await _manager.ExecuteRetrievalAsync(table, TestPartition, "data 0");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(200, actual.HttpStatusCode);
            Assert.IsType<DynamicTableEntity>(actual.Result);
            DynamicTableEntity actualEntity = (DynamicTableEntity)actual.Result;
            Assert.Equal("data 0", actualEntity.RowKey);
            Assert.Equal(TestPartition, actualEntity.PartitionKey);
        }

        [Fact]
        public async Task ExecuteQuery_ReturnsEmptyCollection_IfNotFoundRowKey()
        {
            // Arrange
            CloudTable table = InitializeTable();
            await CreateTableRows(table);
            TableQuery query = new TableQuery { FilterString = "PartitionKey eq 'Unknown'" };

            // Act
            IEnumerable<DynamicTableEntity> actual = await _manager.ExecuteQueryAsync(table, query);

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public async Task ExecuteQuery_ReturnsCollection_IfFound()
        {
            // Arrange
            CloudTable table = InitializeTable();
            await CreateTableRows(table, rowCount: 1024);
            await CreateTableRows(table, partitionKey: "Other");
            TableQuery query = new TableQuery { FilterString = "PartitionKey eq '" + TestPartition + "'" };

            // Act
            IEnumerable<DynamicTableEntity> actual = await _manager.ExecuteQueryAsync(table, query);

            // Assert
            Assert.Equal(1024, actual.Count());
        }

        [Fact]
        public async Task Execute_ReturnsStatus_IfError()
        {
            // Arrange
            CloudTable table = InitializeTable();
            await CreateTableRows(table);
            ITableEntity entity = new DynamicTableEntity(TestPartition, "data 0");
            TableOperation operation = TableOperation.Insert(entity);

            // Act
            TableResult actual = await _manager.ExecuteAsync(table, operation);

            // Assert
            Assert.Equal(409, actual.HttpStatusCode);
        }

        [Fact]
        public async Task Execute_ReturnsStatus_IfSuccess()
        {
            // Arrange
            CloudTable table = InitializeTable();
            await CreateTableRows(table);
            ITableEntity entity = new DynamicTableEntity(TestPartition, "new entry");
            TableOperation operation = TableOperation.Insert(entity);

            // Act
            TableResult actual = await _manager.ExecuteAsync(table, operation);

            // Assert
            Assert.Equal(204, actual.HttpStatusCode);
        }

        [Fact]
        public async Task ExecuteBatch_ReturnsEmptyList_IfError()
        {
            // Arrange
            CloudTable table = InitializeTable();

            ITableEntity entity = new DynamicTableEntity(TestPartition, "data");
            TableOperation operation = TableOperation.Insert(entity);

            TableBatchOperation batch = new TableBatchOperation();
            batch.Add(operation);
            batch.Add(operation);

            // Act
            IEnumerable<TableResult> actual = await _manager.ExecuteBatchAsync(table, batch);

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public async Task ExecuteBatch_ReturnsResults_OnSuccess()
        {
            // Arrange
            CloudTable table = InitializeTable();

            ITableEntity entity1 = new DynamicTableEntity(TestPartition, "data A");
            TableOperation operation1 = TableOperation.Insert(entity1);

            ITableEntity entity2 = new DynamicTableEntity(TestPartition, "data B");
            TableOperation operation2 = TableOperation.Insert(entity2);

            TableBatchOperation batch = new TableBatchOperation();
            batch.Add(operation1);
            batch.Add(operation2);

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
            CloudTable table = InitializeTable();
            await CreateTableRows(table);

            // Act
            long actual = await _manager.ExecuteDeleteAllAsync(table, TestPartition, filter: "RowKey eq 'Unknown'");

            // Assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public async Task ExecuteDeleteAll_ReturnsResults_OnSuccess()
        {
            // Arrange
            CloudTable table = InitializeTable();
            await CreateTableRows(table, rowCount: 1024);
            await CreateTableRows(table, partitionKey: "Other");

            // Act
            long actual = await _manager.ExecuteDeleteAllAsync(table, TestPartition, filter: null);
            IEnumerable<DynamicTableEntity> remaining = await _manager.ExecuteQueryAsync(table, new TableQuery { FilterString = "PartitionKey eq 'Other'" });

            // Assert
            Assert.Equal(1024, actual);
            Assert.Equal(16, remaining.Count());
        }

        [Fact]
        public async Task GetMessages_HandlesEmptyQueue()
        {
            // Arrange
            CloudQueue queue = InitializeQueue();

            // Act
            IEnumerable<CloudQueueMessage> actual = await _manager.GetMessagesAsync(queue, 16, TimeSpan.FromMinutes(1));

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public async Task AddMessagesGetMessages_Roundtrips()
        {
            // Arrange
            CloudQueue queue = InitializeQueue();
            IEnumerable<CloudQueueMessage> expected = CreateQueueMessages();

            // Act
            await _manager.AddMessagesAsync(queue, expected);
            IEnumerable<CloudQueueMessage> actual = await _manager.GetMessagesAsync(queue, 16, TimeSpan.FromMinutes(1));

            // Assert
            Assert.Equal(expected.Count(), actual.Count());
        }

        [Fact]
        public async Task DeleteMessages_EmptiesQueue()
        {
            // Arrange
            CloudQueue queue = InitializeQueue();
            IEnumerable<CloudQueueMessage> messages = CreateQueueMessages();
            await _manager.AddMessagesAsync(queue, messages);

            // Act
            IEnumerable<CloudQueueMessage> initial = await _manager.GetMessagesAsync(queue, MaxDataEntries, TimeSpan.FromMinutes(1));
            await _manager.DeleteMessagesAsync(queue, initial);
            IEnumerable<CloudQueueMessage> final = await _manager.GetMessagesAsync(queue, MaxDataEntries, TimeSpan.FromMinutes(1));

            // Assert
            Assert.Equal(MaxDataEntries, initial.Count());
            Assert.Equal(0, final.Count());
        }

        [Fact]
        public async Task DeleteMessages_HandlesDoubleDeletion()
        {
            // Arrange
            CloudQueue queue = InitializeQueue();
            IEnumerable<CloudQueueMessage> messages = CreateQueueMessages();
            await _manager.AddMessagesAsync(queue, messages);

            // Act
            IEnumerable<CloudQueueMessage> initial = await _manager.GetMessagesAsync(queue, MaxDataEntries, TimeSpan.FromMinutes(1));
            await _manager.DeleteMessagesAsync(queue, initial);
            await _manager.DeleteMessagesAsync(queue, initial);
            IEnumerable<CloudQueueMessage> final = await _manager.GetMessagesAsync(queue, MaxDataEntries, TimeSpan.FromMinutes(1));

            // Assert
            Assert.Equal(MaxDataEntries, initial.Count());
            Assert.Equal(0, final.Count());
        }

        [Fact]
        public async Task DeleteMessages_HandlesInvalidMessage()
        {
            // Arrange
            CloudQueue queue = InitializeQueue();
            CloudQueueMessage message = new CloudQueueMessage("invalid");

            // Act
            await _manager.DeleteMessagesAsync(queue, new[] { message });
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

        [Fact]
        public void GetInstance_ReturnsSingletonInstance()
        {
            // Act
            IStorageManager actual1 = StorageManager.GetInstance(_loggerMock.Object);
            IStorageManager actual2 = StorageManager.GetInstance(_loggerMock.Object);

            // Assert
            Assert.NotNull(actual1);
            Assert.Same(actual1, actual2);
        }

        private CloudTable InitializeTable()
        {
            CloudTable table = _manager.GetCloudTable("UseDevelopmentStorage=true;", "storagetest");
            table.DeleteIfExists();
            table.Create();
            return table;
        }

        private async Task CreateTableRows(CloudTable table, string partitionKey = TestPartition, int rowCount = MaxDataEntries)
        {
            int count = 0;
            int segmentCount = 0;
            do
            {
                int batchCount = Math.Min(rowCount - segmentCount, 100);

                TableBatchOperation batch = new TableBatchOperation();
                for (int cnt = 0; cnt < batchCount; cnt++)
                {
                    ITableEntity entity = new DynamicTableEntity(partitionKey, "data " + count++);
                    TableOperation operation = TableOperation.Insert(entity);
                    batch.Add(operation);
                }

                await _manager.ExecuteBatchAsync(table, batch);

                segmentCount += batchCount;
            }
            while (segmentCount < rowCount);
        }

        private CloudQueue InitializeQueue()
        {
            CloudQueue queue = _manager.GetCloudQueue("UseDevelopmentStorage=true;", "test");
            queue.DeleteIfExists();
            queue.Create();
            return queue;
        }

        private IEnumerable<CloudQueueMessage> CreateQueueMessages()
        {
            CloudQueueMessage[] messages = new CloudQueueMessage[MaxDataEntries];
            for (int cnt = 0; cnt < MaxDataEntries; cnt++)
            {
                messages[cnt] = new CloudQueueMessage("data " + cnt);
            }
            return messages;
        }
    }
}
