// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookManager"/> for managing notifications and mapping
    /// them to registered WebHooks.
    /// </summary>
    public class WebHookManager : IWebHookManager, IDisposable
    {
        internal const string EchoParameter = "echo";

        private readonly IWebHookStore _webHookStore;
        private readonly IWebHookSender _webHookSender;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        private bool _disposed;

        /// <summary>
        /// Initialize a new instance of the <see cref="WebHookManager"/> with a default retry policy.
        /// </summary>
        /// <param name="webHookStore">The current <see cref="IWebHookStore"/>.</param>
        /// <param name="webHookSender">The current <see cref="IWebHookSender"/>.</param>
        /// <param name="logger">The current <see cref="ILogger"/>.</param>
        public WebHookManager(IWebHookStore webHookStore, IWebHookSender webHookSender, ILogger logger)
            : this(webHookStore, webHookSender, logger, httpClient: null)
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="WebHookManager"/> with the given <paramref name="httpClient"/>. This 
        /// constructor is intended for unit testing purposes.
        /// </summary>
        internal WebHookManager(IWebHookStore webHookStore, IWebHookSender webHookSender, ILogger logger, HttpClient httpClient)
        {
            if (webHookStore == null)
            {
                throw new ArgumentNullException("webHookStore");
            }
            if (webHookSender == null)
            {
                throw new ArgumentNullException("webHookSender");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _webHookStore = webHookStore;
            _webHookSender = webHookSender;
            _logger = logger;

            _httpClient = httpClient ?? new HttpClient();
        }

        /// <inheritdoc />
        public async Task VerifyWebHookAsync(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            // Check that WebHook URI is either 'http' or 'https'
            if (!(webHook.WebHookUri.IsHttp() || webHook.WebHookUri.IsHttps()))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_NoHttpUri, webHook.WebHookUri);
                _logger.Error(msg);
                throw new InvalidOperationException(msg);
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
            await _webHookSender.SendWebHookWorkItemsAsync(workItems);
            return webHooks.Count;
        }

        /// <inheritdoc />
        public async Task<int> NotifyAllAsync(IEnumerable<NotificationDictionary> notifications, Func<WebHook, string, bool> predicate)
        {
            if (notifications == null)
            {
                throw new ArgumentNullException("notifications");
            }

            // Get all actions in this batch
            ICollection<NotificationDictionary> nots = notifications.ToArray();
            string[] actions = nots.Select(n => n.Action).ToArray();

            // Find all active WebHooks that matches at least one of the actions
            ICollection<WebHook> webHooks = await _webHookStore.QueryWebHooksAcrossAllUsersAsync(actions, predicate);

            // For each WebHook set up a work item with the right set of notifications
            IEnumerable<WebHookWorkItem> workItems = GetWorkItems(webHooks, nots);

            // Start sending WebHooks
            await _webHookSender.SendWebHookWorkItemsAsync(workItems);
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
                    if (_httpClient != null)
                    {
                        _httpClient.Dispose();
                    }
                }
            }
        }
    }
}
