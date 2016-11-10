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
            builder.AddCheck($"AddAzureBlobStorageCheck {storageAccount.BlobStorageUri} {containerName}", async () =>
            {
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    var properties = await blobClient.GetServicePropertiesAsync();

                    if (!String.IsNullOrWhiteSpace(containerName))
                    {
                        var container = blobClient.GetContainerReference(containerName);

                        return await container.ExistsAsync();
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });

            return builder;
        }

        public static HealthCheckBuilder AddAzureTableStorageCheck(this HealthCheckBuilder builder, string accountName, string accountKey, string tableName = null)
        {
            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, true);
            return AddAzureTableStorageCheck(builder, storageAccount);
        }
        public static HealthCheckBuilder AddAzureTableStorageCheck(HealthCheckBuilder builder, CloudStorageAccount storageAccount, string tableName = null)
        {
            builder.AddCheck($"AddAzureTableStorageCheck {storageAccount.BlobStorageUri} {tableName}", async () =>
            {
                try
                {
                    var tableClient = storageAccount.CreateCloudTableClient();

                    var properties = await tableClient.GetServicePropertiesAsync();

                    if (String.IsNullOrWhiteSpace(tableName))
                    {
                        var table = tableClient.GetTableReference(tableName);

                        return await table.ExistsAsync();
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                
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
            builder.AddCheck($"AddAzureFileStorageCheck {storageAccount.BlobStorageUri} {shareName}", async () =>
            {
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    var properties = await blobClient.GetServicePropertiesAsync();

                    if (!String.IsNullOrWhiteSpace(shareName))
                    {
                        var share = blobClient.GetContainerReference(shareName);

                        return await share.ExistsAsync();
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
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
            builder.AddCheck($"AddAzureQueueStorageCheck {storageAccount.BlobStorageUri} {queueName}", async () =>
            {
                try
                {
                    var tableClient = storageAccount.CreateCloudTableClient();

                    var properties = await tableClient.GetServicePropertiesAsync();

                    if (String.IsNullOrWhiteSpace(queueName))
                    {
                        var queue = tableClient.GetTableReference(queueName);

                        return await queue.ExistsAsync();
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            });

            return builder;
        }
    }
}
