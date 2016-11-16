using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;

namespace HealthChecks
{
    public static class AzureHealthCheckBuilderExtensions
    {
        public static HealthCheckBuilder AddAzureBlobStorageCheck(this HealthCheckBuilder builder, string accountName, string accountKey, string containerName = null)
        {
            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, true);
            return AddAzureBlobStorageCheck(builder, storageAccount, containerName);
        }
        public static HealthCheckBuilder AddAzureBlobStorageCheck(HealthCheckBuilder builder, CloudStorageAccount storageAccount, string containerName = null)
        {
            builder.AddCheck($"AzureBlobStorageCheck {storageAccount.BlobStorageUri} {containerName}", async () =>
            {
                bool result;
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    var properties = await blobClient.GetServicePropertiesAsync();

                    if (!String.IsNullOrWhiteSpace(containerName))
                    {
                        var container = blobClient.GetContainerReference(containerName);

                        result = await container.ExistsAsync();
                    }

                    result = true;
                }
                catch (Exception)
                {
                    result = false;
                }

                return result
                    ? HealthCheckResult.Healthy($"AzureBlobStorage {storageAccount.BlobStorageUri} is available")
                    : HealthCheckResult.Unhealthy($"AzureBlobStorage {storageAccount.BlobStorageUri} is unavailable");
            });

            return builder;
        }

        public static HealthCheckBuilder AddAzureTableStorageCheck(this HealthCheckBuilder builder, string accountName, string accountKey, string tableName = null)
        {
            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, true);
            return AddAzureTableStorageCheck(builder, storageAccount, tableName);
        }
        public static HealthCheckBuilder AddAzureTableStorageCheck(HealthCheckBuilder builder, CloudStorageAccount storageAccount, string tableName = null)
        {
            builder.AddCheck($"AzureTableStorageCheck {storageAccount.TableStorageUri} {tableName}", async () =>
            {
                bool result;
                try
                {
                    var tableClient = storageAccount.CreateCloudTableClient();

                    var properties = await tableClient.GetServicePropertiesAsync();

                    if (String.IsNullOrWhiteSpace(tableName))
                    {
                        var table = tableClient.GetTableReference(tableName);

                        result = await table.ExistsAsync();
                    }
                    result = true;
                }
                catch (Exception)
                {
                    result = false;
                }

                return result
                    ? HealthCheckResult.Healthy($"AzureTableStorage {storageAccount.BlobStorageUri} is available")
                    : HealthCheckResult.Unhealthy($"AzureTableStorage {storageAccount.BlobStorageUri} is unavailable");

            });

            return builder;
        }

        public static HealthCheckBuilder AddAzureFileStorageCheck(this HealthCheckBuilder builder, string accountName, string accountKey, string shareName = null)
        {
            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, true);
            return AddAzureFileStorageCheck(builder, storageAccount, shareName);
        }
        public static HealthCheckBuilder AddAzureFileStorageCheck(HealthCheckBuilder builder, CloudStorageAccount storageAccount, string shareName = null)
        {
            builder.AddCheck($"AzureFileStorageCheck {storageAccount.FileStorageUri} {shareName}", async () =>
            {
                bool result;
                try
                {
                    var fileClient = storageAccount.CreateCloudFileClient();

                    var properties = await fileClient.GetServicePropertiesAsync();

                    if (!String.IsNullOrWhiteSpace(shareName))
                    {
                        var share = fileClient.GetShareReference(shareName);

                        result = await share.ExistsAsync();
                    }

                    result = true;
                }
                catch (Exception)
                {
                    result = false;
                }

                return result
                    ? HealthCheckResult.Healthy($"AzureFileStorage {storageAccount.BlobStorageUri} is available")
                    : HealthCheckResult.Unhealthy($"AzureFileStorage {storageAccount.BlobStorageUri} is unavailable");
            });

            return builder;
        }

        public static HealthCheckBuilder AddAzureQueueStorageCheck(this HealthCheckBuilder builder, string accountName, string accountKey, string queueName = null)
        {
            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, true);
            return AddAzureQueueStorageCheck(builder, storageAccount, queueName);
        }
        public static HealthCheckBuilder AddAzureQueueStorageCheck(HealthCheckBuilder builder, CloudStorageAccount storageAccount, string queueName = null)
        {
            builder.AddCheck($"AzureQueueStorageCheck {storageAccount.QueueStorageUri} {queueName}", async () =>
            {
                bool result;
                try
                {
                    var queueClient = storageAccount.CreateCloudQueueClient();

                    var properties = await queueClient.GetServicePropertiesAsync();

                    if (String.IsNullOrWhiteSpace(queueName))
                    {
                        var queue = queueClient.GetQueueReference(queueName);

                        result = await queue.ExistsAsync();
                    }
                    result = true;
                }
                catch (Exception)
                {
                    result = false;
                }

                return result
                    ? HealthCheckResult.Healthy($"AzureFileStorage {storageAccount.BlobStorageUri} is available")
                    : HealthCheckResult.Unhealthy($"AzureFileStorage {storageAccount.BlobStorageUri} is unavailable");

            });

            return builder;
        }
    }
}
