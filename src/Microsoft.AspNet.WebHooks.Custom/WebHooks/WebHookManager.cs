// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookManager"/> for submitting requests to 
    /// registered <see cref="WebHook"/> instances without external dependencies.
    /// </summary>
    public class WebHookManager : IWebHookManager, IDisposable
    {
        internal const string EchoParameter = "echo";
        internal const string SignatureHeaderKey = "sha256";
        internal const string SignatureHeaderValueTemplate = SignatureHeaderKey + "={0}";
        internal const string SignatureHeaderName = "ms-signature";

        private const int DefaultMaxConcurrencyLevel = 8;

        private const string BodyIdKey = "Id";
        private const string BodyAttemptKey = "Attempt";
        private const string BodyPropertiesKey = "Properties";
        private const string BodyNotificationsKey = "Notifications";

        private static readonly Collection<TimeSpan> DefaultRetries = new Collection<TimeSpan> { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(4) };

        private readonly IWebHookStore _webHookStore;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly ActionBlock<WebHookWorkItem>[] _launchers;
        private readonly Action<WebHookWorkItem> _onWebHookSuccess, _onWebHookFailure;

        private bool _disposed;

        /// <summary>
        /// Initialize a new instance of the <see cref="WebHookManager"/> with a default retry policy.
        /// </summary>
        /// <param name="webHookStore">The current <see cref="IWebHookStore"/>.</param>
        /// <param name="logger">The current <see cref="ILogger"/>.</param>
        public WebHookManager(IWebHookStore webHookStore, ILogger logger)
            : this(webHookStore, logger, retryDelays: null, options: null, httpClient: null, onWebHookSuccess: null, onWebHookFailure: null)
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="WebHookManager"/> with a given collection of <paramref name="retryDelays"/> and
        /// <paramref name="options"/> for how to manage the queuing policy for each transmission. The transmission model is as follows: each try
        /// and subsequent retries is managed by a separate <see cref="ActionBlock{T}"/> which controls the level of concurrency used to 
        /// send out WebHooks. The <paramref name="options"/> parameter can be used to control all <see cref="ActionBlock{T}"/> instances 
        /// by setting the maximum level of concurrency, length of queue, and more.
        /// </summary>
        /// <param name="webHookStore">The current <see cref="IWebHookStore"/>.</param>
        /// <param name="logger">The current <see cref="ILogger"/>.</param>
        /// <param name="retryDelays">A collection of <see cref="TimeSpan"/> instances indicating the delay between each retry. If <c>null</c>,
        /// then a default retry policy is used of one retry after one 1 minute and then again after 4 minutes. A retry is attempted if the 
        /// delivery fails or does not result in a 2xx HTTP status code. If the status code is 410 then no retry is attempted. If the collection
        /// is empty then no retries are attempted.</param>
        /// <param name="options">An <see cref="ExecutionDataflowBlockOptions"/> used to control the <see cref="ActionBlock{T}"/> instances.
        /// The default setting uses a maximum of 8 concurrent transmitters for each try or retry.</param>
        public WebHookManager(IWebHookStore webHookStore, ILogger logger, IEnumerable<TimeSpan> retryDelays, ExecutionDataflowBlockOptions options)
            : this(webHookStore, logger, retryDelays, options, httpClient: null, onWebHookSuccess: null, onWebHookFailure: null)
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="WebHookManager"/> with the given retry policy, <paramref name="options"/>,
        /// and <paramref name="httpClient"/>. This constructor is intended for unit testing purposes.
        /// </summary>
        internal WebHookManager(
            IWebHookStore webHookStore,
            ILogger logger,
            IEnumerable<TimeSpan> retryDelays,
            ExecutionDataflowBlockOptions options,
            HttpClient httpClient,
            Action<WebHookWorkItem> onWebHookSuccess,
            Action<WebHookWorkItem> onWebHookFailure)
        {
            if (webHookStore == null)
            {
                throw new ArgumentNullException("webHookStore");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _webHookStore = webHookStore;
            _logger = logger;

            retryDelays = retryDelays ?? DefaultRetries;

            options = options ?? new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = DefaultMaxConcurrencyLevel
            };

            _httpClient = httpClient ?? new HttpClient();

            // Create the launch processors with the given retry delays
            _launchers = new ActionBlock<WebHookWorkItem>[1 + retryDelays.Count()];

            int offset = 0;
            _launchers[offset++] = new ActionBlock<WebHookWorkItem>(async item => await LaunchWebHook(item), options);
            foreach (TimeSpan delay in retryDelays)
            {
                _launchers[offset++] = new ActionBlock<WebHookWorkItem>(async item => await DelayedLaunchWebHook(item, delay));
            }

            string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_Started, typeof(WebHookManager).Name, _launchers.Length);
            _logger.Info(msg);

            // Set handlers for testing purposes
            _onWebHookSuccess = onWebHookSuccess;
            _onWebHookFailure = onWebHookFailure;
        }

        /// <inheritdoc />
        public async Task VerifyWebHookAsync(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            // Create the echo query parameter that we want returned in response body as plain text.
            string echo = Guid.NewGuid().ToString("N");

            HttpResponseMessage response;
            try
            {
                // Get request URI with echo query parameter
                UriBuilder webHookUri = new UriBuilder(webHook.WebHookUri);
                webHookUri.Query = EchoParameter + "=" + echo;

                // Create request adding any additional request headers (not entity headers) from Web Hook
                HttpRequestMessage hookRequest = new HttpRequestMessage(HttpMethod.Get, webHookUri.Uri);
                foreach (var kvp in webHook.Headers)
                {
                    hookRequest.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }

                response = await _httpClient.SendAsync(hookRequest);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_VerifyFailure, ex.Message);
                _logger.Error(msg, ex);
                throw new InvalidOperationException(msg);
            }

            if (!response.IsSuccessStatusCode)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_VerifyFailure, response.StatusCode);
                _logger.Info(msg);
                throw new InvalidOperationException(msg);
            }

            // Verify response body
            if (response.Content == null)
            {
                string msg = CustomResources.Manager_VerifyNoBody;
                _logger.Error(msg);
                throw new InvalidOperationException(msg);
            }

            string actualEcho = await response.Content.ReadAsStringAsync();
            if (!string.Equals(actualEcho, echo, StringComparison.Ordinal))
            {
                string msg = CustomResources.Manager_VerifyBadEcho;
                _logger.Error(msg);
                throw new InvalidOperationException(msg);
            }
        }

        /// <inheritdoc />
        public async Task<int> NotifyAsync(string user, IEnumerable<NotificationDictionary> notifications)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (notifications == null)
            {
                throw new ArgumentNullException("notifications");
            }

            // Get all actions in this batch
            ICollection<NotificationDictionary> nots = notifications.ToArray();
            string[] actions = nots.Select(n => n.Action).ToArray();

            // Find all active WebHooks that matches at least one of the actions
            ICollection<WebHook> webHooks = await _webHookStore.QueryWebHooksAsync(user, actions);

            // For each WebHook set up a work item with the right set of notifications
            IEnumerable<WebHookWorkItem> workItems = GetWorkItems(webHooks, nots);

            // Start sending WebHooks
            foreach (WebHookWorkItem workItem in workItems)
            {
                _launchers[0].Post(workItem);
            }

            return webHooks.Count;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static IEnumerable<WebHookWorkItem> GetWorkItems(ICollection<WebHook> webHooks, ICollection<NotificationDictionary> notifications)
        {
            List<WebHookWorkItem> workItems = new List<WebHookWorkItem>();
            foreach (WebHook webHook in webHooks)
            {
                ICollection<NotificationDictionary> webHookNotifications;

                // Pick the notifications that apply for this particular WebHook. If we only got one notification
                // then we know that it applies to all WebHooks. Otherwise each notification may apply only to a subset.
                if (notifications.Count == 1)
                {
                    webHookNotifications = notifications;
                }
                else
                {
                    webHookNotifications = notifications.Where(n => webHook.MatchesAction(n.Action)).ToArray();
                    if (webHookNotifications.Count == 0)
                    {
                        continue;
                    }
                }

                WebHookWorkItem workItem = new WebHookWorkItem(webHook, webHookNotifications);
                workItems.Add(workItem);
            }
            return workItems;
        }

        internal static JObject CreateWebHookRequestBody(WebHookWorkItem workItem)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();

            // Set properties from work item
            body[BodyIdKey] = workItem.Id;
            body[BodyAttemptKey] = workItem.Offset + 1;

            // Set properties from WebHook
            IDictionary<string, object> properties = workItem.WebHook.Properties;
            if (properties != null)
            {
                body[BodyPropertiesKey] = new Dictionary<string, object>(properties);
            }

            // Set notifications
            body[BodyNotificationsKey] = workItem.Notifications;

            return JObject.FromObject(body);
        }

        internal static void SignWebHookRequest(WebHook webHook, HttpRequestMessage request, JObject body)
        {
            byte[] secret = Encoding.UTF8.GetBytes(webHook.Secret);
            using (var hasher = new HMACSHA256(secret))
            {
                string serializedBody = body.ToString();
                request.Content = new StringContent(serializedBody, Encoding.UTF8, "application/json");

                byte[] data = Encoding.UTF8.GetBytes(serializedBody);
                byte[] sha256 = hasher.ComputeHash(data);
                string headerValue = string.Format(CultureInfo.InvariantCulture, SignatureHeaderValueTemplate, EncodingUtilities.ToHex(sha256));
                request.Headers.Add(SignatureHeaderName, headerValue);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Request is disposed by caller.")]
        internal static HttpRequestMessage CreateWebHookRequest(WebHookWorkItem workItem, ILogger logger)
        {
            WebHook hook = workItem.WebHook;

            // Create WebHook request
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, hook.WebHookUri);

            // Fill in request body based on WebHook and work item data
            JObject body = CreateWebHookRequestBody(workItem);
            SignWebHookRequest(hook, request, body);

            // Add extra request or entity headers
            foreach (var kvp in hook.Headers)
            {
                if (!request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value))
                {
                    if (!request.Content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value))
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_InvalidHeader, kvp.Key, hook.Id);
                        logger.Error(msg);
                    }
                }
            }

            return request;
        }

        /// <summary>
        /// Releases the unmanaged resources and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    if (_launchers != null)
                    {
                        try
                        {
                            // Start shutting down launchers
                            Task[] completionTasks = new Task[_launchers.Length];
                            for (int cnt = 0; cnt < _launchers.Length; cnt++)
                            {
                                ActionBlock<WebHookWorkItem> launcher = _launchers[cnt];
                                launcher.Complete();
                                completionTasks[cnt] = launcher.Completion;
                            }

                            // Cancel any outstanding HTTP requests
                            if (_httpClient != null)
                            {
                                _httpClient.CancelPendingRequests();
                                _httpClient.Dispose();
                            }

                            // Wait for launchers to complete
                            Task.WaitAll(completionTasks);
                        }
                        catch (Exception ex)
                        {
                            ex = ex.GetBaseException();
                            string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_CompletionFailure, ex.Message);
                            _logger.Error(msg, ex);
                        }
                    }
                }
            }
        }

        private async Task DelayedLaunchWebHook(WebHookWorkItem item, TimeSpan delay)
        {
            await Task.Delay(delay);
            await LaunchWebHook(item);
        }

        /// <summary>
        /// Launch a <see cref="WebHook"/>.
        /// </summary>
        /// <remarks>We don't let exceptions propagate out from this method as it is used by the launchers
        /// and if they see an exception they shut down.</remarks>
        private async Task LaunchWebHook(WebHookWorkItem workItem)
        {
            try
            {
                // Setting up and send WebHook request 
                HttpRequestMessage request = CreateWebHookRequest(workItem, _logger);
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_Result, workItem.WebHook.Id, response.StatusCode, workItem.Offset);
                _logger.Info(msg);

                if (response.IsSuccessStatusCode)
                {
                    // If we get a successful response then we are done.
                    if (_onWebHookSuccess != null)
                    {
                        _onWebHookSuccess(workItem);
                    }
                    return;
                }
                else if (response.StatusCode == HttpStatusCode.Gone)
                {
                    // If we get a 410 Gone then we are also done.
                    if (_onWebHookFailure != null)
                    {
                        _onWebHookFailure(workItem);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_WebHookFailure, workItem.Offset, workItem.WebHook.Id, ex.Message);
                _logger.Error(msg, ex);
            }

            try
            {
                // See if we should retry the request with delay or give up
                workItem.Offset++;
                if (workItem.Offset < _launchers.Length)
                {
                    // Submit work item
                    _launchers[workItem.Offset].Post(workItem);
                }
                else
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_GivingUp, workItem.WebHook.Id, workItem.Offset);
                    _logger.Error(msg);
                    if (_onWebHookFailure != null)
                    {
                        _onWebHookFailure(workItem);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_WebHookFailure, workItem.Offset, workItem.WebHook.Id, ex.Message);
                _logger.Error(msg, ex);
            }
        }
    }
}
