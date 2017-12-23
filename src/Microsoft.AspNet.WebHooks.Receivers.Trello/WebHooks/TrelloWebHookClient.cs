// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// The <see cref="TrelloWebHookClient"/> provides support for creating and deleting Trello WebHooks programmatically.
    /// For more information about Trello WebHooks, please see <c>https://trello.com/docs/gettingstarted/webhooks.html</c>.
    /// </summary>
    public class TrelloWebHookClient : IDisposable
    {
        private const string IdKey = "id";
        private const string DescriptionKey = "description";
        private const string CallbackKey = "callbackURL";
        private const string ModelIdKey = "idModel";

        private const string CreateWebHookApiTemplate = "https://trello.com/1/tokens/{0}/webhooks/?key={1}";
        private const string DeleteWebHookApiTemplate = "https://trello.com/1/webhooks/{{0}}?key={0}&token={1}";

        private readonly HttpClient _httpClient;
        private readonly Uri _createWebHookUri;
        private readonly string _deleteWebHookUriTemplate;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrelloWebHookClient"/> which can be used to create and delete WebHooks
        /// with Trello. For more information about Trello WebHooks, please see <c>https://trello.com/docs/gettingstarted/webhooks.html</c>.
        /// </summary>
        /// <param name="userToken">The user token obtained when authenticating with Trello. The token can for example
        /// be obtained using either of the <c>Trello.Net</c> or <c>Owin.Security.Providers.Trello</c> Nuget packages. A sample user token is
        /// <c>0d3cce724413cba6d42084bb1c6bd7a285446deccb3ee2259152acd1eb6418a2</c>. Note that the token expires so it must be renewed on a regular basis.</param>
        /// <param name="applicationKey">Your Trello application key as obtained from <c>https://trello.com/app-key</c>.</param>
        public TrelloWebHookClient(string userToken, string applicationKey)
            : this(userToken, applicationKey, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrelloWebHookClient"/> with the given <paramref name="httpClient"/>.
        /// This constructor is intended for unit testing purposes.
        /// </summary>
        internal TrelloWebHookClient(string userToken, string applicationKey, HttpClient httpClient)
        {
            if (userToken == null)
            {
                throw new ArgumentNullException(nameof(userToken));
            }
            if (applicationKey == null)
            {
                throw new ArgumentNullException(nameof(applicationKey));
            }
            _createWebHookUri = new Uri(string.Format(CultureInfo.InvariantCulture, CreateWebHookApiTemplate, userToken, applicationKey));
            _deleteWebHookUriTemplate = string.Format(CultureInfo.InvariantCulture, DeleteWebHookApiTemplate, applicationKey, userToken);
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// Creates a WebHook subscription for Trello to send WebHooks when changes happen to a given <paramref name="modelId"/>.
        /// If the operation fails an exception is thrown.
        /// </summary>
        /// <param name="callback">The URI where WebHooks for the given <paramref name="modelId"/> will be received. Typically this will be of the form <c>https://&lt;host&gt;/api/webhooks/incoming/trello</c></param>
        /// <param name="modelId">The ID of a model to watch. This can be the ID of a member, card, board, or anything that actions apply to. Any event involving this model will trigger the WebHook. An example model ID is <c>4d5ea62fd76aa1136000000c</c>.</param>
        /// <param name="description">A description of the WebHook, for example <c>My Trello WebHook!</c>.</param>
        public virtual async Task<string> CreateAsync(Uri callback, string modelId, string description)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            if (!callback.IsAbsoluteUri)
            {
                var message = string.Format(CultureInfo.CurrentCulture, TrelloResources.Client_NotAbsoluteCallback, "https://<host>/api/webhooks/incoming/trello");
                throw new ArgumentException(message, nameof(callback));
            }
            if (modelId == null)
            {
                throw new ArgumentNullException(nameof(modelId));
            }
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            var parameters = new JObject
            {
                [DescriptionKey] = description,
                [CallbackKey] = callback,
                [ModelIdKey] = modelId
            };

            using (var response = await _httpClient.PostAsJsonAsync(_createWebHookUri, parameters))
            {
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = string.Empty;
                    if (response.Content != null)
                    {
                        errorMessage = await response.Content.ReadAsStringAsync();
                    }
                    var message = string.Format(CultureInfo.CurrentCulture, TrelloResources.Client_CreateFailure, response.StatusCode, errorMessage.Trim());
                    throw new InvalidOperationException(message);
                }

                var content = await response.Content.ReadAsAsync<JObject>();
                var id = content.Value<string>(IdKey);
                return id;
            }
        }

        /// <summary>
        /// Deletes an existing Trello WebHook with a given <paramref name="webHookId"/>.
        /// </summary>
        /// <param name="webHookId">The WebHook ID obtained when creating the WebHook.</param>
        /// <returns><c>true</c> is the WebHook was removed, otherwise <c>false</c>.</returns>
        public virtual async Task<bool> DeleteAsync(string webHookId)
        {
            if (webHookId == null)
            {
                throw new ArgumentNullException(nameof(webHookId));
            }

            var address = string.Format(CultureInfo.InvariantCulture, _deleteWebHookUriTemplate, webHookId);

            using (var response = await _httpClient.DeleteAsync(address))
            {
                return response.IsSuccessStatusCode;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
