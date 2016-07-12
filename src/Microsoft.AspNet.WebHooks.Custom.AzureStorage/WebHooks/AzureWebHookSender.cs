// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookSender"/> sending WebHooks to a Microsoft Azure Queue for later processing.
    /// </summary>
    [CLSCompliant(false)]
    public class AzureWebHookSender : IWebHookSender
    {
        internal const string WebHookQueue = "webhooks";

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings() { Formatting = Formatting.None };
        private readonly IStorageManager _manager;
        private readonly string _connectionString;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureWebHookStore"/> class with the given <paramref name="manager"/>,
        /// <paramref name="settings"/>, and <paramref name="logger"/>.
        /// </summary>
        public AzureWebHookSender(IStorageManager manager, SettingsDictionary settings, ILogger logger)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _manager = manager;
            _connectionString = manager.GetAzureStorageConnectionString(settings);
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems)
        {
            if (workItems == null)
            {
                throw new ArgumentNullException(nameof(workItems));
            }

            // Serialize WebHook requests and convert to queue messages
            IEnumerable<CloudQueueMessage> messages = null;
            try
            {
                messages = workItems.Select(item =>
                    {
                        string content = JsonConvert.SerializeObject(item, _serializerSettings);
                        CloudQueueMessage message = new CloudQueueMessage(content);
                        return message;
                    }).ToArray();
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.AzureSender_SerializeFailure, ex.Message);
                _logger.Error(msg, ex);
                throw new InvalidOperationException(msg);
            }

            // Insert queue messages into queue.
            CloudQueue queue = _manager.GetCloudQueue(_connectionString, WebHookQueue);
            await _manager.AddMessagesAsync(queue, messages);
        }
    }
}
