// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an event loop which dequeues messages from a Microsoft Azure Queue and then sends the
    /// WebHook to the recipients. If the delivery success then the message is removed from the queue, otherwise it remains so that another
    /// attempt can be made. After a given number of attempts the message is discarded without being delivered.
    /// </summary>
    public class AzureWebHookDequeueManager : IDisposable
    {
        internal const string QueueMessageKey = "MS_QueueMessage";
        internal const int MaxDequeuedMessages = 32;

        internal static readonly TimeSpan _DefaultFrequency = TimeSpan.FromMinutes(5);
        internal static readonly TimeSpan _DefaultMessageTimeout = TimeSpan.FromMinutes(2);

        private const string WorkItemKey = "MS_WebHookWorkItem";
        private const int DefaultMaxDequeueCount = 3;

        private readonly ILogger _logger;
        private readonly TimeSpan _messageTimeout;
        private readonly TimeSpan _pollingFrequency;
        private readonly int _maxAttempts;

        private readonly IStorageManager _storageManager;
        private readonly HttpClient _httpClient;
        private readonly WebHookSender _sender;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings();
        private readonly CloudQueue _queue;

        private CancellationTokenSource _tokenSource;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureWebHookDequeueManager"/> using the given <paramref name="connectionString"/>
        /// to identify the Microsoft Azure Storage Queue.
        /// </summary>
        /// <param name="connectionString">The Microsoft Azure Storage Queue connection string.</param>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging errors and warnings.</param>
        public AzureWebHookDequeueManager(string connectionString, ILogger logger)
            : this(connectionString, logger, _DefaultFrequency, _DefaultMessageTimeout, DefaultMaxDequeueCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureWebHookDequeueManager"/> using the given <paramref name="connectionString"/>
        /// to identify the Microsoft Azure Storage Queue.
        /// </summary>
        /// <param name="connectionString">The Microsoft Azure Storage Queue connection string.</param>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging errors and warnings.</param>
        /// <param name="pollingFrequency">The polling frequency by which we request messages from the queue.</param>
        /// <param name="messageTimeout">The time allotted to send out the WebHooks request and get a response. If
        /// the request has not completed within this time frame, the message will be considered to have failed and
        /// it will be attempted again.</param>
        /// <param name="maxAttempts">The maximum number of attempts to deliver the WebHook.</param>
        public AzureWebHookDequeueManager(string connectionString, ILogger logger, TimeSpan pollingFrequency, TimeSpan messageTimeout, int maxAttempts)
            : this(connectionString, logger, pollingFrequency, messageTimeout, maxAttempts, httpClient: null, storageManager: null, sender: null)
        {
        }

        /// <summary>
        /// Intended for unit test purposes
        /// </summary>
        internal AzureWebHookDequeueManager(string connectionString, ILogger logger, TimeSpan pollingFrequency, TimeSpan messageTimeout, int maxAttempts, HttpClient httpClient, IStorageManager storageManager, WebHookSender sender)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (pollingFrequency <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(pollingFrequency));
            }
            if (messageTimeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(messageTimeout));
            }

            _logger = logger;
            _pollingFrequency = pollingFrequency;
            _messageTimeout = messageTimeout;
            _maxAttempts = maxAttempts;

            _httpClient = httpClient ?? new HttpClient();
            _storageManager = storageManager ?? new StorageManager(logger);
            _sender = sender ?? new QueuedSender(this, logger);

            _queue = _storageManager.GetCloudQueue(connectionString, AzureWebHookSender.WebHookQueue);
        }

        internal WebHookSender WebHookSender
        {
            get
            {
                return _sender;
            }
        }

        /// <summary>
        /// Start the event loop of requesting messages from the queue and send them out as WebHooks.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to terminate the event loop.</param>
        /// <returns>An awaitable <see cref="Task"/> representing the event loop.</returns>
        public Task Start(CancellationToken cancellationToken)
        {
            if (_tokenSource != null)
            {
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.DequeueManager_Started, this.GetType().Name);
                _logger.Error(message);
                throw new InvalidOperationException(message);
            }

            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return DequeueAndSendWebHooks(_tokenSource.Token);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dequeues available WebHooks and sends them out to each WebHook recipient.
        /// </summary>
        protected virtual async Task DequeueAndSendWebHooks(CancellationToken cancellationToken)
        {
            var isEmpty = false;
            while (true)
            {
                try
                {
                    do
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        // Dequeue messages from Azure queue
                        var messages = await _storageManager.GetMessagesAsync(_queue, MaxDequeuedMessages, _messageTimeout);

                        // Extract the work items
                        ICollection<WebHookWorkItem> workItems = messages.Select(m =>
                        {
                            var workItem = JsonConvert.DeserializeObject<WebHookWorkItem>(m.AsString, _serializerSettings);
                            workItem.Properties[QueueMessageKey] = m;
                            return workItem;
                        }).ToArray();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        // Submit work items to be sent to WebHook receivers
                        if (workItems.Count > 0)
                        {
                            await _sender.SendWebHookWorkItemsAsync(workItems);
                        }
                        isEmpty = workItems.Count == 0;
                    }
                    while (!isEmpty);
                }
                catch (Exception ex)
                {
                    var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.DequeueManager_ErrorDequeueing, _queue.Name, ex.Message);
                    _logger.Error(message, ex);
                }

                try
                {
                    await Task.Delay(_pollingFrequency, cancellationToken);
                }
                catch (OperationCanceledException oex)
                {
                    _logger.Error(oex.Message, oex);
                    return;
                }
            }
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    if (_tokenSource != null)
                    {
                        _tokenSource.Cancel();
                        _tokenSource.Dispose();
                    }
                    if (_httpClient != null)
                    {
                        _httpClient.Dispose();
                    }
                    if (_sender != null)
                    {
                        _sender.Dispose();
                    }
                }
            }
        }

        private class QueuedSender : WebHookSender
        {
            private readonly AzureWebHookDequeueManager _parent;

            public QueuedSender(AzureWebHookDequeueManager parent, ILogger logger)
                : base(logger)
            {
                if (parent == null)
                {
                    throw new ArgumentNullException(nameof(parent));
                }
                _parent = parent;
            }

            /// <inheritdoc />
            public override async Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems)
            {
                if (workItems == null)
                {
                    throw new ArgumentNullException(nameof(workItems));
                }

                // Keep track of which queued messages should be deleted because processing has completed.
                var deleteMessages = new List<CloudQueueMessage>();

                // Keep track of the response messages
                var responses = new List<HttpResponseMessage>();
                foreach (var workItem in workItems)
                {
                    var request = CreateWebHookRequest(workItem);
                    request.Properties[WorkItemKey] = workItem;

                    try
                    {
                        // Await for responses, so that invalid response exceptions can be caught.
                        var requestTask = await _parent._httpClient.SendAsync(request);
                        responses.Add(requestTask);
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.DequeueManager_SendFailure, request.RequestUri, ex.Message);
                        Logger.Info(message);

                        var queueMessage = GetMessage(workItem);
                        if (DiscardMessage(workItem, queueMessage))
                        {
                            deleteMessages.Add(queueMessage);
                        }
                    }
                }

                // Loop through the responses to see which messages should be deleted from the queue based on the response statuses.
                foreach (var response in responses)
                {
                    var workItem = response.RequestMessage.Properties.GetValueOrDefault<WebHookWorkItem>(WorkItemKey);
                    var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.DequeueManager_WebHookStatus, workItem.WebHook.Id, response.StatusCode, workItem.Offset);
                    Logger.Info(message);

                    // If success or 'gone' HTTP status code then we remove the message from the Azure queue.
                    // If error then we leave it in the queue to be consumed once it becomes visible again or we give up
                    var queueMessage = GetMessage(workItem);
                    if (response.IsSuccessStatusCode ||
                        response.StatusCode == HttpStatusCode.Gone ||
                        DiscardMessage(workItem, queueMessage))
                    {
                        deleteMessages.Add(queueMessage);
                    }
                }

                // Delete successfully delivered messages and messages that have been attempted delivered too many times.
                await _parent._storageManager.DeleteMessagesAsync(_parent._queue, deleteMessages);
            }

            private CloudQueueMessage GetMessage(WebHookWorkItem workItem)
            {
                var queueMessage = workItem?.Properties.GetValueOrDefault<CloudQueueMessage>(QueueMessageKey);
                if (queueMessage == null)
                {
                    var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.DequeueManager_NoProperty, QueueMessageKey, workItem?.Id);
                    Logger.Error(message);
                    throw new InvalidOperationException(message);
                }
                return queueMessage;
            }

            private bool DiscardMessage(WebHookWorkItem workItem, CloudQueueMessage message)
            {
                if (message.DequeueCount >= _parent._maxAttempts)
                {
                    var error = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.DequeueManager_GivingUp, workItem.WebHook.Id, message.DequeueCount);
                    Logger.Error(error);
                    return true;
                }
                return false;
            }
        }
    }
}
