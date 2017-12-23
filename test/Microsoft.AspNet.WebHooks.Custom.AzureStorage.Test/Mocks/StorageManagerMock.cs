// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks.Mocks
{
    public static class StorageManagerMock
    {
        private const string ConnectionString = "UseDevelopmentStorage=true;";

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.None };
        private static readonly CloudQueue Queue = new CloudQueue(new Uri("http://localhost"));

        public static CloudQueue CloudQueue
        {
            get
            {
                return Queue;
            }
        }

        public static Mock<IStorageManager> Create()
        {
            var storageMock = new Mock<IStorageManager>();

            storageMock.Setup(s => s.GetAzureStorageConnectionString(It.IsAny<SettingsDictionary>()))
                .Returns(ConnectionString)
                .Verifiable();
            storageMock.Setup(s => s.GetCloudQueue(ConnectionString, AzureWebHookSender.WebHookQueue))
                .Returns(CloudQueue)
                .Verifiable();
            return storageMock;
        }

        public static IEnumerable<WebHookWorkItem> CreateWorkItems(int count)
        {
            var workItems = new WebHookWorkItem[count];
            for (var i = 0; i < count; i++)
            {
                var webHook = new WebHook
                {
                    WebHookUri = new Uri("http://localhost/path/" + count),
                    Secret = "0123456789012345678901234567890123456789" + count
                };
                var notification = new NotificationDictionary("a" + i, i);
                var workItem = new WebHookWorkItem(webHook, new[] { notification });
                workItem.Properties[AzureWebHookDequeueManager.QueueMessageKey] = new CloudQueueMessage("content");
                workItems[i] = workItem;
            }
            return workItems;
        }

        public static IEnumerable<CloudQueueMessage> CreateQueueMessages(int count)
        {
            return CreateWorkItems(count).Select(item =>
            {
                var content = JsonConvert.SerializeObject(item, SerializerSettings);
                var message = new CloudQueueMessage(content);
                return message;
            }).ToArray();
        }
    }
}
