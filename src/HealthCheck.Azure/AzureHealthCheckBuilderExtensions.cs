using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;

namespace HealthChecks
{
    public static class AzureHealthCheckBuilderExtensions
    {
        public static HealthCheckBuilder AddAzureBlobStorageCheck(this HealthCheckBuilder builder, string connectionString, string containerName = null)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            return AddAzureBlobStorageCheck(builder, storageAccount, containerName);
        }
        public static HealthCheckBuilder AddAzureBlobStorageCheck(this HealthCheckBuilder builder, string accountName, string accountKey, string containerName = null)
        {
            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, true);
            return AddAzureBlobStorageCheck(builder, storageAccount, containerName);
        }
        public static HealthCheckBuilder AddAzureBlobStorageCheck(HealthCheckBuilder builder, CloudStorageAccount storageAccount, string containerName = null)
        {
            builder.AddCheck($"UrlCheck ({storageAccount.BlobStorageUri})", async () =>
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

        public static HealthCheckBuilder AddAzureTableStorageCheck(this HealthCheckBuilder builder, string connectionString, string tableName = null)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            return AddAzureTableStorageCheck(builder, storageAccount);
        }
        public static HealthCheckBuilder AddAzureTableStorageCheck(this HealthCheckBuilder builder, string accountName, string accountKey, string tableName = null)
        {
            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, true);
            return AddAzureTableStorageCheck(builder, storageAccount);
        }
        public static HealthCheckBuilder AddAzureTableStorageCheck(HealthCheckBuilder builder, CloudStorageAccount storageAccount, string tableName = null)
        {
            builder.AddCheck($"UrlCheck ({storageAccount.BlobStorageUri})", async () =>
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
    }
}
