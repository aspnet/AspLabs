// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.AspNet.WebHooks.Storage
{
    /// <summary>
    /// Provides utilities for managing connection strings and related information for Microsoft Azure Table Storage.
    /// </summary>
    [CLSCompliant(false)]
    public class StorageManager : IStorageManager
    {
        private const string AzureStoreConnectionStringName = "MS_AzureStoreConnectionString";
        private const string PartitionKey = "PartitionKey";
        private const string RowKey = "RowKey";

        private const string QuerySeparator = "&";
        private const int MaxBatchSize = 100;

        private static readonly ConcurrentDictionary<string, CloudStorageAccount> TableAccounts = new ConcurrentDictionary<string, CloudStorageAccount>();
        private static readonly ConcurrentDictionary<string, CloudStorageAccount> QueueAccounts = new ConcurrentDictionary<string, CloudStorageAccount>();

        private static IStorageManager _storageManager;

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageManager"/> class with the given <paramref name="logger"/>.
        /// </summary>
        public StorageManager(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            _logger = logger;
        }

        /// <inheritdoc />
        public string GetAzureStorageConnectionString(SettingsDictionary settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (!settings.Connections.TryGetValue(AzureStoreConnectionStringName, out var connection) || connection == null || string.IsNullOrEmpty(connection.ConnectionString))
            {
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.AzureStore_NoConnectionString, AzureStoreConnectionStringName);
                throw new InvalidOperationException(message);
            }
            return connection.ConnectionString;
        }

        /// <inheritdoc />
        public CloudStorageAccount GetCloudStorageAccount(string connectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(connectionString);
                if (storageAccount == null)
                {
                    var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_NoCloudStorageAccount, typeof(CloudStorageAccount).Name);
                    throw new ArgumentException(message);
                }
            }
            catch (Exception ex)
            {
                var message = AzureStorageResources.StorageManager_InvalidConnectionString;
                _logger.Error(message, ex);
                throw new InvalidOperationException(message, ex);
            }
            return storageAccount;
        }

        /// <inheritdoc />
        public CloudTable GetCloudTable(string connectionString, string tableName)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            var tableKey = GetLookupKey(connectionString, tableName);
            var account = TableAccounts.GetOrAdd(
                tableKey,
                key =>
                {
                    var storageAccount = GetCloudStorageAccount(connectionString);
                    try
                    {
                        // Ensure that table exists
                        var client = storageAccount.CreateCloudTableClient();
                        var cloudTable = client.GetTableReference(tableName);
                        cloudTable.CreateIfNotExists();
                    }
                    catch (Exception ex)
                    {
                        var error = GetStorageErrorMessage(ex);
                        var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_InitializationFailure, error);
                        _logger.Error(message, ex);
                        throw new InvalidOperationException(message, ex);
                    }

                    return storageAccount;
                });

            var cloudClient = account.CreateCloudTableClient();
            return cloudClient.GetTableReference(tableName);
        }

        /// <inheritdoc />
        public CloudQueue GetCloudQueue(string connectionString, string queueName)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            if (queueName == null)
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            var queueKey = GetLookupKey(connectionString, queueName);
            var account = QueueAccounts.GetOrAdd(
                queueKey,
                key =>
                {
                    var storageAccount = GetCloudStorageAccount(connectionString);
                    try
                    {
                        // Ensure that queue exists
                        var client = storageAccount.CreateCloudQueueClient();
                        var cloudQueue = client.GetQueueReference(queueName);
                        cloudQueue.CreateIfNotExists();
                    }
                    catch (Exception ex)
                    {
                        var error = GetStorageErrorMessage(ex);
                        var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_InitializationFailure, error);
                        _logger.Error(message, ex);
                        throw new InvalidOperationException(message, ex);
                    }

                    return storageAccount;
                });

            var cloudClient = account.CreateCloudQueueClient();
            return cloudClient.GetQueueReference(queueName);
        }

        /// <inheritdoc />
        public void AddPartitionKeyConstraint(TableQuery query, string partitionKey)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            if (partitionKey == null)
            {
                throw new ArgumentNullException(nameof(partitionKey));
            }

            var partitionKeyFilter = string.Format(CultureInfo.InvariantCulture, "{0} eq '{1}'", PartitionKey, partitionKey);
            AddQueryConstraint(query, partitionKeyFilter);
        }

        /// <inheritdoc />
        public async Task<TableResult> ExecuteRetrievalAsync(CloudTable table, string partitionKey, string rowKey)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }
            if (partitionKey == null)
            {
                throw new ArgumentNullException(nameof(partitionKey));
            }
            if (rowKey == null)
            {
                throw new ArgumentNullException(nameof(rowKey));
            }

            try
            {
                var operation = TableOperation.Retrieve(partitionKey, rowKey);
                var result = await table.ExecuteAsync(operation);
                return result;
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_ErrorRetrieving, ex.Message);
                _logger.Error(message, ex);
            }
            return null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DynamicTableEntity>> ExecuteQueryAsync(CloudTable table, TableQuery query)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            try
            {
                var result = new List<DynamicTableEntity>();
                TableQuerySegment<DynamicTableEntity> segment;
                TableContinuationToken continuationToken = null;
                do
                {
                    segment = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                    if (segment == null)
                    {
                        break;
                    }
                    result.AddRange(segment);
                    continuationToken = segment.ContinuationToken;
                }
                while (continuationToken != null);
                return result;
            }
            catch (Exception ex)
            {
                var errorMessage = GetStorageErrorMessage(ex);
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_QueryFailed, errorMessage);
                _logger.Error(message, ex);
                throw new InvalidOperationException(message, ex);
            }
        }

        /// <inheritdoc />
        public async Task<TableResult> ExecuteAsync(CloudTable table, TableOperation operation)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            try
            {
                var result = await table.ExecuteAsync(operation);
                return result;
            }
            catch (Exception ex)
            {
                var errorMessage = GetStorageErrorMessage(ex);
                var statusCode = GetStorageStatusCode(ex);
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, statusCode, errorMessage);
                _logger.Error(message, ex);

                return new TableResult { HttpStatusCode = statusCode };
            }
        }

        /// <inheritdoc />
        public async Task<IList<TableResult>> ExecuteBatchAsync(CloudTable table, TableBatchOperation batch)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            try
            {
                var results = await table.ExecuteBatchAsync(batch);
                return results;
            }
            catch (Exception ex)
            {
                var errorMessage = GetStorageErrorMessage(ex);
                var statusCode = GetStorageStatusCode(ex);
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, statusCode, errorMessage);
                _logger.Error(message, ex);
                return new List<TableResult>();
            }
        }

        /// <inheritdoc />
        public async Task<long> ExecuteDeleteAllAsync(CloudTable table, string partitionKey, string filter)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }
            if (partitionKey == null)
            {
                throw new ArgumentNullException(nameof(partitionKey));
            }

            // Build query for retrieving exiting entries. We only ask for PK and RK.
            var query = new TableQuery()
            {
                FilterString = filter,
                SelectColumns = new List<string> { PartitionKey, RowKey },
            };
            AddPartitionKeyConstraint(query, partitionKey);

            try
            {
                long totalCount = 0;
                TableContinuationToken continuationToken = null;
                do
                {
                    var webHooks = (await ExecuteQueryAsync(table, query)).ToArray();
                    if (webHooks.Length == 0)
                    {
                        break;
                    }

                    // Delete query results in max of 100-count batches
                    var totalSegmentCount = webHooks.Length;
                    var segmentCount = 0;
                    do
                    {
                        var batch = new TableBatchOperation();
                        var batchCount = Math.Min(totalSegmentCount - segmentCount, MaxBatchSize);
                        for (var i = 0; i < batchCount; i++)
                        {
                            var entity = webHooks[segmentCount + i];
                            entity.ETag = "*";
                            var operation = TableOperation.Delete(entity);
                            batch.Add(operation);
                        }

                        await ExecuteBatchAsync(table, batch);
                        segmentCount += batchCount;
                    }
                    while (segmentCount < totalSegmentCount);
                    totalCount += segmentCount;
                }
                while (continuationToken != null);
                return totalCount;
            }
            catch (Exception ex)
            {
                var errorMessage = GetStorageErrorMessage(ex);
                var statusCode = GetStorageStatusCode(ex);
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, statusCode, errorMessage);
                _logger.Error(message, ex);
                throw new InvalidOperationException(message, ex);
            }
        }

        /// <inheritdoc />
        public async Task AddMessagesAsync(CloudQueue queue, IEnumerable<CloudQueueMessage> messages)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            try
            {
                var addTasks = new List<Task>();
                foreach (var message in messages)
                {
                    var addTask = queue.AddMessageAsync(message);
                    addTasks.Add(addTask);
                }

                await Task.WhenAll(addTasks);
            }
            catch (Exception ex)
            {
                var errorMessage = GetStorageErrorMessage(ex);
                var statusCode = GetStorageStatusCode(ex);
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, statusCode, errorMessage);
                _logger.Error(message, ex);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CloudQueueMessage>> GetMessagesAsync(CloudQueue queue, int messageCount, TimeSpan timeout)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            try
            {
                var messages = await queue.GetMessagesAsync(messageCount, timeout, options: null, operationContext: null);
                return messages;
            }
            catch (Exception ex)
            {
                var errorMessage = GetStorageErrorMessage(ex);
                var statusCode = GetStorageStatusCode(ex);
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, statusCode, errorMessage);
                _logger.Error(message, ex);
                return Enumerable.Empty<CloudQueueMessage>();
            }
        }

        /// <inheritdoc />
        public async Task DeleteMessagesAsync(CloudQueue queue, IEnumerable<CloudQueueMessage> messages)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            try
            {
                var deleteTasks = new List<Task>();
                foreach (var message in messages)
                {
                    var deleteTask = queue.DeleteMessageAsync(message);
                    deleteTasks.Add(deleteTask);
                }

                await Task.WhenAll(deleteTasks);
            }
            catch (Exception ex)
            {
                var errorMessage = GetStorageErrorMessage(ex);
                var statusCode = GetStorageStatusCode(ex);
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, statusCode, errorMessage);
                _logger.Error(message, ex);
            }
        }

        /// <inheritdoc />
        public string GetStorageErrorMessage(Exception ex)
        {
            if (ex is StorageException storageException && storageException.RequestInformation != null)
            {
                var status = storageException.RequestInformation.HttpStatusMessage != null ?
                    storageException.RequestInformation.HttpStatusMessage + " " :
                    string.Empty;
                var errorCode = storageException.RequestInformation.ExtendedErrorInformation != null ?
                    "(" + storageException.RequestInformation.ExtendedErrorInformation.ErrorMessage + ")" :
                    string.Empty;
                return status + errorCode;
            }
            else if (ex != null)
            {
                return ex.Message;
            }
            return string.Empty;
        }

        /// <inheritdoc />
        public int GetStorageStatusCode(Exception ex)
        {
            return ex is StorageException se && se.RequestInformation != null ? se.RequestInformation.HttpStatusCode : 500;
        }

        internal static IStorageManager GetInstance(ILogger logger)
        {
            if (_storageManager != null)
            {
                return _storageManager;
            }

            IStorageManager instance = new StorageManager(logger);
            Interlocked.CompareExchange(ref _storageManager, instance, null);
            return _storageManager;
        }

        internal static void AddQueryConstraint(TableQuery query, string constraint)
        {
            query.FilterString = string.IsNullOrEmpty(query.FilterString)
                ? constraint
                : string.Format(CultureInfo.InvariantCulture, "({0}) and ({1})", query.FilterString, constraint);
        }

        internal static string GetLookupKey(string connectionString, string identifier)
        {
            var key = Hasher.GetFnvHash32AsString(connectionString) + "$" + identifier;
            return key;
        }
    }
}
