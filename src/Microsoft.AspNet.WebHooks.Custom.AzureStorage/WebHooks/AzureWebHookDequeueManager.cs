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
        private const string QueueMessageKey = "MS_QueueMessage";
        private const string WorkItemKey = "MS_WebHookWorkItem";

        private const int DefaultMaxDequeueCount = 3;

        private static readonly TimeSpan _DefaultFrequency = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan _DefaultMessageTimeout = TimeSpan.FromMinutes(2);

        private readonly ILogger _logger;
        private readonly TimeSpan _messageTimeout;
        private readonly TimeSpan _pollingFrequency;
        private readonly int _maxAttempts;
        private readonly IStorageManager _storageManager;
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings();
        private readonly CloudQueue _queue;
        private readonly QueuedSender _sender;

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
            : this(connectionString, logger, pollingFrequency, messageTimeout, maxAttempts, httpClient: null, storageManager: null)
        {
        }

        /// <summary>
        /// Intended for unit test purposes
        /// </summary>
        internal AzureWebHookDequeueManager(string connectionString, ILogger logger, TimeSpan pollingFrequency, TimeSpan messageTimeout, int maxAttempts, HttpClient httpClient, IStorageManager storageManager)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            if (pollingFrequency <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("pollingFrequency");
            }
            if (messageTimeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("messageTimeout");
            }

            _logger = logger;
            _pollingFrequency = pollingFrequency;
            _messageTimeout = messageTimeout;
            _maxAttempts = maxAttempts;

            _httpClient = httpClient ?? new HttpClient();
            _storageManager = storageManager ?? new StorageManager(logger);

            _queue = _storageManager.GetCloudQueue(connectionString, AzureWebHookSender.WebHookQueue);
            _sender = new QueuedSender(this, logger);
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
                string msg = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.DequeueManager_Started, this.GetType().Name);
                _logger.Error(msg);
                throw new InvalidOperationException(msg);
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
            bool isEmpty = false;
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
                        IEnumerable<CloudQueueMessage> messages = await _queue.GetMessagesAsync(32, _messageTimeout, options: null, operationContext: null);

                        // Extract the work items
                        ICollection<WebHookWorkItem> workItems = messages.Select(m =>
                        {
                            WebHookWorkItem workItem = JsonConvert.DeserializeObject<WebHookWorkItem>(m.AsString, _serializerSettings);
                            workItem.Properties[QueueMessageKey] = m;
                            return workItem;
                        }).ToArray();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        // Submit work items to be sent to WebHook receivers
                        await _sender.SendWebHookWorkItemsAsync(workItems);

                        isEmpty = workItems.Count == 0;
                    }
                    while (!isEmpty);
                }
                catch (Exception ex)
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, "Encountered an error when dequeueing messages from the Azure Queue '{0}': '{1}' ", _queue.Name, ex.Message);
                    _logger.Error(msg, ex);
                }

                await Task.Delay(_pollingFrequency, cancellationToken);
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
                    throw new ArgumentNullException("parent");
                }
                _parent = parent;
            }

            /// <inheritdoc />
            public override async Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems)
            {
                if (workItems == null)
                {
                    throw new ArgumentNullException("workItems");
                }

                // Submit WebHook requests in parallel
                List<Task<HttpResponseMessage>> requestTasks = new List<Task<HttpResponseMessage>>();
                foreach (var workItem in workItems)
                {
                    HttpRequestMessage request = CreateWebHookRequest(workItem);
                    request.Properties[WorkItemKey] = workItem;
                    requestTasks.Add(_parent._httpClient.SendAsync(request));
                }

                // Wait for all responses and see which messages should be deleted from the queue.
                List<Task> deleteTasks = new List<Task>();
                HttpResponseMessage[] responses = await Task.WhenAll(requestTasks);
                foreach (HttpResponseMessage response in responses)
                {
                    WebHookWorkItem workItem = response.RequestMessage.Properties.GetValueOrDefault<WebHookWorkItem>(WorkItemKey);
                    string msg = string.Format(CultureInfo.CurrentCulture, "WebHook '{0}' resulted in status code '{1}' on attempt '{2}'.", workItem.WebHook.Id, response.StatusCode, workItem.Offset);
                    Logger.Info(msg);

                    // If success or 'gone' HTTP status code then we remove the message from the Azure queue.
                    // If error then we leave it in the queue to be consumed once it becomes visible again or we give up
                    CloudQueueMessage message = workItem.Properties.GetValueOrDefault<CloudQueueMessage>(QueueMessageKey);
                    if (message != null)
                    {
                        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Gone)
                        {
                            deleteTasks.Add(_parent._queue.DeleteMessageAsync(message));
                        }
                        else if (message.DequeueCount > _parent._maxAttempts)
                        {
                            string error = string.Format(CultureInfo.CurrentCulture, "Giving up sending WebHook '{0}' after '{1}' attempts.", workItem.WebHook.Id, message.DequeueCount);
                            Logger.Error(error);
                            deleteTasks.Add(_parent._queue.DeleteMessageAsync(message));
                        }
                    }
                }

                // Delete successfully delivered messages and messages that have been attempted delivered too many times.
                await Task.WhenAll(deleteTasks);
            }
        }
    }
}
