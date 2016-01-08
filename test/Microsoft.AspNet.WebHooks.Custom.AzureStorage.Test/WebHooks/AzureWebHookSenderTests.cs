// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Mocks;
using Microsoft.AspNet.WebHooks.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class AzureWebHookSenderTests
    {
        private const string ConnectionString = "connectionString";

        private SettingsDictionary _settings;
        private Mock<IStorageManager> _storageMock;
        private AzureWebHookSender _sender;
        private ILogger _logger;

        public AzureWebHookSenderTests()
        {
            _settings = new SettingsDictionary();
            _logger = new Mock<ILogger>().Object;
            _storageMock = StorageManagerMock.Create();
            _sender = new AzureWebHookSender(_storageMock.Object, _settings, _logger);
        }

        [Fact]
        public async Task SendWebHookWorkItems_SendsMessages()
        {
            // Arrange
            IEnumerable<WebHookWorkItem> workItems = StorageManagerMock.CreateWorkItems(32);

            // Act
            await _sender.SendWebHookWorkItemsAsync(workItems);

            // Assert
            _storageMock.Verify(s => s.AddMessagesAsync(StorageManagerMock.CloudQueue, It.Is<IEnumerable<CloudQueueMessage>>(m => m.Count() == 32)));
            _storageMock.Verify();
        }

        [Fact]
        public async Task SendWebHookWorkItems_ThrowsException_IfInvalidWorkItem()
        {
            // Arrange
            WebHook webHook = new WebHook();
            NotificationDictionary notification = new NotificationDictionary();
            notification.Add("k", new SerializationFailure());
            WebHookWorkItem workItem = new WebHookWorkItem(webHook, new[] { notification });
            IEnumerable<WebHookWorkItem> workItems = new[] { workItem };

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sender.SendWebHookWorkItemsAsync(workItems));

            // Assert
            Assert.Equal("Could not serialize message: Error getting value from 'Fail' on 'Microsoft.AspNet.WebHooks.AzureWebHookSenderTests+SerializationFailure'.", ex.Message);
        }

        private class SerializationFailure
        {
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "For testing purposes")]
            public int Fail
            {
                get
                {
                    throw new Exception("Catch this!");
                }
            }
        }
    }
}
