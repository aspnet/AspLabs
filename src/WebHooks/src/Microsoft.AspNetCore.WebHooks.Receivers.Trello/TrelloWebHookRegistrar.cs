// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebHooks.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// <para>
    /// The <see cref="TrelloWebHookRegistrar"/> provides support for creating and deleting Trello WebHooks
    /// programmatically. For more information about Trello WebHooks, see
    /// <see href="https://developers.trello.com/page/webhooks"/>. For more information about the specific Trello REST
    /// API used here, see <see href="https://developers.trello.com/reference/#webhooks"/>.
    /// </para>
    /// <para>
    /// The Trello Developer Sandbox is an alternative to this class. Go to <see href="https://trello.com/app-key"/>
    /// for a link to the Developer Sandbox and click on <c>Create Webhook</c> or <c>Get Webhooks</c>. Edit the
    /// <c>Code</c> to delete one of the WebHooks <c>Get Webhooks</c> lists.
    /// </para>
    /// </summary>
    public class TrelloWebHookRegistrar : IDisposable
    {
        private const string IdKey = "id";
        private const string DescriptionKey = "description";
        private const string CallbackKey = "callbackURL";
        private const string ModelIdKey = "idModel";

        private const string CreateWebHookApiTemplate = "https://trello.com/1/tokens/{0}/webhooks/?key={1}";
        private const string DeleteWebHookApiTemplate = "https://trello.com/1/tokens/{0}/webhooks/{{0}}?key={1}";

        private readonly HttpClient _httpClient;
        private readonly Uri _createWebHookUri;
        private readonly string _deleteWebHookUriTemplate;

        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="TrelloWebHookRegistrar"/> instance with the given <paramref name="userToken"/>
        /// and <paramref name="applicationKey"/>. See
        /// <see href="https://developers.trello.com/reference/#api-key-tokens"/> for information about application
        /// (API) keys and user tokens. See <see href="https://trello.com/app-key"/> for your developer API key and to
        /// generate a new token.
        /// </summary>
        /// <param name="userToken">
        /// The user token obtained when authenticating with Trello. A user token can be generated using a link from
        /// <see href="https://trello.com/app-key"/> or obtained using the <c>Trello.Net</c> or
        /// <c>Owin.Security.Providers.Trello</c> NuGet packages. A sample user token is
        /// <c>0d3cce724413cba6d42084bb1c6bd7a285446deccb3ee2259152acd1eb6418a2</c>. Note that the token expires so it
        /// must be renewed on a regular basis.
        /// </param>
        /// <param name="applicationKey">
        /// Your Trello application (API) key as obtained from <see href="https://trello.com/app-key"/>. A sample
        /// application key is <c>85446deccb3ee2259152acd1eb6418a2</c>.
        /// </param>
        public TrelloWebHookRegistrar(string userToken, string applicationKey)
            : this(userToken, applicationKey, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="TrelloWebHookRegistrar"/> instance with the given
        /// <paramref name="httpClient"/>. This constructor is intended for unit testing purposes.
        /// </summary>
        internal TrelloWebHookRegistrar(string userToken, string applicationKey, HttpClient httpClient)
        {
            if (userToken == null)
            {
                throw new ArgumentNullException(nameof(userToken));
            }
            if (applicationKey == null)
            {
                throw new ArgumentNullException(nameof(applicationKey));
            }

            _createWebHookUri = new Uri(
                string.Format(CultureInfo.InvariantCulture, CreateWebHookApiTemplate, userToken, applicationKey));
            _deleteWebHookUriTemplate =
                string.Format(CultureInfo.InvariantCulture, DeleteWebHookApiTemplate, userToken, applicationKey);
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// Creates a WebHook subscription for Trello to send WebHooks when changes happen to a given
        /// <paramref name="modelId"/>. If the operation fails, an <see cref="Exception"/> is thrown.
        /// </summary>
        /// <param name="callback">
        /// The URI where WebHooks for the given <paramref name="modelId"/> will be received. Typically this will be
        /// of the form <c>https://{host}/api/webhooks/incoming/trello</c>.
        /// </param>
        /// <param name="modelId">
        /// The ID of a model to watch. This can be the ID of a member, card, board, or anything that actions apply
        /// to. Any event involving this model will trigger the WebHook. An example model ID is
        /// <c>4d5ea62fd76aa1136000000c</c>.
        /// </param>
        /// <param name="description">A description of the WebHook, for example <c>My Trello WebHook!</c>.</param>
        public virtual async Task<string> CreateAsync(Uri callback, string modelId, string description)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            if (!callback.IsAbsoluteUri)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Registrar_NotAbsoluteCallback,
                    "https://<host>/api/webhooks/incoming/trello");
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
            var content = new StringContent(parameters.ToString(), Encoding.UTF8, "application/json");

            using (var response = await _httpClient.PostAsync(_createWebHookUri, content))
            {
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = string.Empty;
                    if (response.Content != null)
                    {
                        errorMessage = await response.Content.ReadAsStringAsync();
                    }

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Registrar_CreateFailure,
                        response.StatusCode,
                        errorMessage.Trim());
                    throw new InvalidOperationException(message);
                }

                var responseStream = await response.Content.ReadAsStreamAsync();
                var responseContent = await JObject.LoadAsync(new JsonTextReader(new StreamReader(responseStream)));
                var id = responseContent.Value<string>(IdKey);

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
            if (!_disposed)
            {
                _disposed = true;
                _httpClient.Dispose();
            }
        }
    }
}
