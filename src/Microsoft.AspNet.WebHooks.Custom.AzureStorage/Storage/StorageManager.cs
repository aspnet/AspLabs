// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageManager"/> class with the given <paramref name="logger"/>.
        /// </summary>
        public StorageManager(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            _logger = logger;
        }

        /// <inheritdoc />
        public string GetAzureStorageConnectionString(SettingsDictionary settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            ConnectionSettings connection;
            if (!settings.Connections.TryGetValue(AzureStoreConnectionStringName, out connection) || connection == null || string.IsNullOrEmpty(connection.ConnectionString))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.AzureStore_NoConnectionString, AzureStoreConnectionStringName);
                throw new InvalidOperationException(msg);
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
                    string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_NoCloudStorageAccount, typeof(CloudStorageAccount).Name);
                    throw new ArgumentException(msg);
                }
            }
            catch (Exception ex)
            {
                string msg = AzureStorageResources.StorageManager_InvalidConnectionString;
                _logger.Error(msg, ex);
                throw new InvalidOperationException(msg, ex);
            }
            return storageAccount;
        }

        /// <inheritdoc />
        public CloudTable GetCloudTable(string connectionString, string tableName)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            string tableKey = GetLookupKey(connectionString, tableName);
            CloudStorageAccount account = TableAccounts.GetOrAdd(
                tableKey,
                key =>
                {
                    CloudStorageAccount storageAccount = GetCloudStorageAccount(connectionString);
                    try
                    {
                        // Ensure that table exists
                        CloudTableClient client = storageAccount.CreateCloudTableClient();
                        CloudTable cloudTable = client.GetTableReference(tableName);
                        cloudTable.CreateIfNotExists();
                    }
                    catch (Exception ex)
                    {
                        string error = GetStorageErrorMessage(ex);
                        string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_InitializationFailure, error);
                        _logger.Error(msg, ex);
                        throw new InvalidOperationException(msg, ex);
                    }

                    return storageAccount;
                });

            CloudTableClient cloudClient = account.CreateCloudTableClient();
            return cloudClient.GetTableReference(tableName);
        }

        /// <inheritdoc />
        public CloudQueue GetCloudQueue(string connectionString, string queueName)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }

            string queueKey = GetLookupKey(connectionString, queueName);
            CloudStorageAccount account = QueueAccounts.GetOrAdd(
                queueKey,
                key =>
                {
                    CloudStorageAccount storageAccount = GetCloudStorageAccount(connectionString);
                    try
                    {
                        // Ensure that queue exists
                        CloudQueueClient client = storageAccount.CreateCloudQueueClient();
                        CloudQueue cloudQueue = client.GetQueueReference(queueName);
                        cloudQueue.CreateIfNotExists();
                    }
                    catch (Exception ex)
                    {
                        string error = GetStorageErrorMessage(ex);
                        string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_InitializationFailure, error);
                        _logger.Error(msg, ex);
                        throw new InvalidOperationException(msg, ex);
                    }

                    return storageAccount;
                });

            CloudQueueClient cloudClient = account.CreateCloudQueueClient();
            return cloudClient.GetQueueReference(queueName);
        }

        /// <inheritdoc />
        public void AddPartitionKeyConstraint(TableQuery query, string partitionKey)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (partitionKey == null)
            {
                throw new ArgumentNullException("partitionKey");
            }

            string partitionKeyFilter = string.Format(CultureInfo.InvariantCulture, "{0} eq '{1}'", PartitionKey, partitionKey);
            AddQueryConstraint(query, partitionKeyFilter);
        }

        /// <inheritdoc />
        public async Task<TableResult> ExecuteRetrievalAsync(CloudTable table, string partitionKey, string rowKey)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (partitionKey == null)
            {
                throw new ArgumentNullException("partitionKey");
            }
            if (rowKey == null)
            {
                throw new ArgumentNullException("rowKey");
            }

            try
            {
                TableOperation operation = TableOperation.Retrieve(partitionKey, rowKey);
                TableResult result = await table.ExecuteAsync(operation);
                return result;
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_ErrorRetrieving, ex.Message);
                _logger.Error(msg, ex);
            }
            return null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DynamicTableEntity>> ExecuteQueryAsync(CloudTable table, TableQuery query)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            try
            {
                List<DynamicTableEntity> result = new List<DynamicTableEntity>();
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
                return segment;
            }
            catch (Exception ex)
            {
                string errorMessage = GetStorageErrorMessage(ex);
                string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_QueryFailed, errorMessage);
                _logger.Error(msg, ex);
                throw new InvalidOperationException(msg, ex);
            }
        }

        /// <inheritdoc />
        public async Task<TableResult> ExecuteAsync(CloudTable table, TableOperation operation)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            try
            {
                TableResult result = await table.ExecuteAsync(operation);
                return result;
            }
            catch (Exception ex)
            {
                string errorMessage = GetStorageErrorMessage(ex);
                int statusCode = GetStorageStatusCode(ex);
                string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, statusCode, errorMessage);
                _logger.Error(msg, ex);

                return new TableResult { HttpStatusCode = statusCode };
            }
        }

        /// <inheritdoc />
        public async Task<IList<TableResult>> ExecuteBatchAsync(CloudTable table, TableBatchOperation batch)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (batch == null)
            {
                throw new ArgumentNullException("batch");
            }

            try
            {
                IList<TableResult> results = await table.ExecuteBatchAsync(batch);
                return results;
            }
            catch (Exception ex)
            {
                string errorMessage = GetStorageErrorMessage(ex);
                int statusCode = GetStorageStatusCode(ex);
                string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, statusCode, errorMessage);
                _logger.Error(msg, ex);
                return new List<TableResult>();
            }
        }

        /// <inheritdoc />
        public async Task<long> ExecuteDeleteAllAsync(CloudTable table, string partitionKey, string filter)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (partitionKey == null)
            {
                throw new ArgumentNullException("partitionKey");
            }

            // Build query for retrieving exiting entries. We only ask for PK and RK.
            TableQuery query = new TableQuery()
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
                    DynamicTableEntity[] webHooks = (await ExecuteQueryAsync(table, query)).ToArray();
                    if (webHooks.Length == 0)
                    {
                        break;
                    }

                    // Delete query results in max of 100-count batches
                    int totalSegmentCount = webHooks.Length;
                    int segmentCount = 0;
                    do
                    {
                        TableBatchOperation batch = new TableBatchOperation();
                        int batchCount = Math.Min(totalSegmentCount - segmentCount, MaxBatchSize);
                        for (int cnt = 0; cnt < batchCount; cnt++)
                        {
                            DynamicTableEntity entity = webHooks[segmentCount + cnt];
                            entity.ETag = "*";
                            TableOperation operation = TableOperation.Delete(entity);
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
                string errorMessage = GetStorageErrorMessage(ex);
                int statusCode = GetStorageStatusCode(ex);
                string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, statusCode, errorMessage);
                _logger.Error(msg, ex);
                throw new InvalidOperationException(msg, ex);
            }
        }

        /// <inheritdoc />
        public string GetStorageErrorMessage(Exception ex)
        {
            StorageException se = ex as StorageException;
            if (se != null && se.RequestInformation != null)
            {
                string status = se.RequestInformation.HttpStatusMessage != null ? se.RequestInformation.HttpStatusMessage + " " : string.Empty;
                string errorCode = se.RequestInformation.ExtendedErrorInformation != null ? "(" + se.RequestInformation.ExtendedErrorInformation.ErrorMessage + ")" : string.Empty;
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
            StorageException se = ex as StorageException;
            return se != null && se.RequestInformation != null ? se.RequestInformation.HttpStatusCode : 500;
        }

        internal static void AddQueryConstraint(TableQuery query, string constraint)
        {
            query.FilterString = string.IsNullOrEmpty(query.FilterString)
                ? constraint
                : string.Format(CultureInfo.InvariantCulture, "({0}) and ({1})", query.FilterString, constraint);
        }

        internal static string GetLookupKey(string connectionString, string identifier)
        {
            string key = Hasher.GetFnvHash32AsString(connectionString) + "$" + identifier;
            return key;
        }
    }
}
