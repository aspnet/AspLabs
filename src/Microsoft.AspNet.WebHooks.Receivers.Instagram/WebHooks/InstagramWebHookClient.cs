// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.WebHooks.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// The <see cref="InstagramWebHookClient"/> provides support for managing Instagram WebHook subscriptions programmatically.
    /// For more information about Instagram WebHooks, please see <c>https://www.instagram.com/developer/subscriptions/</c>.
    /// </summary>
    public class InstagramWebHookClient : IDisposable
    {
        internal const string DataKey = "data";
        internal const string InstagramApi = "https://api.instagram.com/v1/";
        internal const string SubscriptionAddress = InstagramApi + "subscriptions";
        internal const string SubscriptionAddressTemplate = SubscriptionAddress + "?client_id={0}&client_secret={1}{2}";

        private static readonly string ClientIdKey = InstagramWebHookReceiver.ReceiverName + "Id";

        private readonly HttpConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly Uri _subscriptionAddress;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstagramWebHookClient"/> which can be used to create and delete WebHooks
        /// with Instagram. For more information about Instagram WebHooks, please see <c>https://www.instagram.com/developer/subscriptions/</c>.
        /// Set the application settings '<c>MS_WebHookReceiverSecret_Instagram</c>' and '<c>MS_WebHookReceiverSecret_InstagramId</c>' to 
        /// the Instagram client ID and secret respectively, optionally using the 'id' syntax to accommodate multiple configurations 
        /// using the same model as for receivers.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>.</param>
        public InstagramWebHookClient(HttpConfiguration config)
            : this(config, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstagramWebHookClient"/> with the given <paramref name="httpClient"/>. 
        /// This constructor is intended for unit testing purposes.
        /// </summary>
        internal InstagramWebHookClient(HttpConfiguration config, HttpClient httpClient)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _config = config;
            _httpClient = httpClient ?? new HttpClient();
            _subscriptionAddress = new Uri(SubscriptionAddress);
        }

        /// <summary>
        /// Gets the current set of subscriptions for the given client.
        /// </summary>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this WebHook. This makes it possible to 
        /// support multiple WebHooks with individual configurations.</param>
        public virtual async Task<Collection<InstagramSubscription>> GetAllSubscriptionsAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            // Get client ID and secret for the given ID.
            Tuple<string, string> clientInfo = await GetClientConfig(id);

            string address = string.Format(CultureInfo.InvariantCulture, SubscriptionAddressTemplate, clientInfo.Item1, clientInfo.Item2, null);
            using (HttpResponseMessage response = await _httpClient.GetAsync(address))
            {
                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = await GetErrorContent(response);
                    string msg = string.Format(CultureInfo.CurrentCulture, InstagramReceiverResources.Client_GetSubscriptionsFailure, response.StatusCode, errorMessage);
                    throw new InvalidOperationException(msg);
                }

                JObject subscriptionData = await response.Content.ReadAsAsync<JObject>();
                JArray subs = subscriptionData.Value<JArray>(DataKey);
                Collection<InstagramSubscription> result = subs.ToObject<Collection<InstagramSubscription>>();
                return result;
            }
        }

        /// <summary>
        /// Subscribes to posts submitted by all users authenticated with this Instagram client.
        /// </summary>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this WebHook. This makes it possible to 
        /// support multiple WebHooks with individual configurations.</param>
        /// <param name="callback">The URI where WebHooks for the given subscription will be received. Typically this will 
        /// be of the form <c>https://&lt;host&gt;/api/webhooks/incoming/instagram/{id}</c>.</param>
        /// <returns>A <see cref="InstagramSubscription"/> instance.</returns>
        public virtual Task<InstagramSubscription> SubscribeAsync(string id, Uri callback)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "object", "user" },
                { "aspect", "media" }
            };
            return CreateSubscription(id, callback, parameters);
        }

        /// <summary>
        /// Deletes all active subscriptions for this client.
        /// </summary>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this WebHook. This makes it possible to 
        /// support multiple WebHooks with individual configurations.</param>
        public virtual Task UnsubscribeAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            string parameter = "&object=all";
            return DeleteSubscription(id, parameter);
        }

        /// <summary>
        /// Deletes the subscription with the given <paramref name="subscriptionId"/>.
        /// </summary>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this WebHook. This makes it possible to 
        /// support multiple WebHooks with individual configurations.</param>
        /// <param name="subscriptionId">The ID of the subscription to delete.</param>
        public virtual Task UnsubscribeAsync(string id, string subscriptionId)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (subscriptionId == null)
            {
                throw new ArgumentNullException("id");
            }

            string parameter = "&id=" + subscriptionId;
            return DeleteSubscription(id, parameter);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static void ValidateConfig(string key, string value, string id, int minLength, int maxLength)
        {
            if (value == null)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, InstagramReceiverResources.Receiver_BadSecret, key, id, minLength, maxLength);
                throw new InvalidOperationException(msg);
            }
        }

        internal static async Task<string> GetErrorContent(HttpResponseMessage response)
        {
            string errorMessage = string.Empty;
            if (response.Content != null)
            {
                errorMessage = await response.Content.ReadAsStringAsync();
            }
            return errorMessage;
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

        /// <summary>
        /// Gets the Client ID and Client Secret for the Instagram Client application.
        /// </summary>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this WebHook.</param>
        /// <returns>A <see cref="Tuple{T1,T2}"/> containing the Client ID and Client Secret.</returns>
        protected virtual async Task<Tuple<string, string>> GetClientConfig(string id)
        {
            IWebHookReceiverConfig receiverConfig = _config.DependencyResolver.GetReceiverConfig();

            string clientId = await receiverConfig.GetReceiverConfigAsync(ClientIdKey, id, InstagramWebHookReceiver.SecretMinLength, InstagramWebHookReceiver.SecretMaxLength);
            ValidateConfig(ClientIdKey, clientId, id, InstagramWebHookReceiver.SecretMinLength, InstagramWebHookReceiver.SecretMaxLength);

            string clientSecret = await receiverConfig.GetReceiverConfigAsync(InstagramWebHookReceiver.ReceiverName, id, InstagramWebHookReceiver.SecretMinLength, InstagramWebHookReceiver.SecretMaxLength);
            ValidateConfig(InstagramWebHookReceiver.ReceiverName, clientSecret, id, InstagramWebHookReceiver.SecretMinLength, InstagramWebHookReceiver.SecretMaxLength);

            return Tuple.Create(clientId, clientSecret);
        }

        private async Task<InstagramSubscription> CreateSubscription(string id, Uri callback, IDictionary<string, object> parameters)
        {
            if (!callback.IsAbsoluteUri)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, InstagramReceiverResources.Client_NotAbsoluteCallback, "https://<host>/api/webhooks/incoming/instagram");
                throw new ArgumentException(msg, "receiver");
            }

            if (!callback.IsHttps())
            {
                string msg = string.Format(CultureInfo.CurrentCulture, InstagramReceiverResources.Client_NoHttps, Uri.UriSchemeHttps);
                throw new InvalidOperationException(msg);
            }

            // Build up subscription request body
            MultipartFormDataContent content = new MultipartFormDataContent();

            // Add default properties
            Tuple<string, string> clientInfo = await GetClientConfig(id);
            parameters["client_id"] = clientInfo.Item1;
            parameters["client_secret"] = clientInfo.Item2;
            parameters["callback_url"] = callback.AbsoluteUri;

            // Add subscription specific properties
            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                StringContent p = new StringContent(parameter.Value.ToString());
                p.Headers.ContentType = null;
                content.Add(p, parameter.Key);
            }

            using (HttpResponseMessage response = await _httpClient.PostAsync(_subscriptionAddress, content))
            {
                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = await GetErrorContent(response);
                    string msg = string.Format(CultureInfo.CurrentCulture, InstagramReceiverResources.Client_SubscribeFailure, response.StatusCode, errorMessage);
                    throw new InvalidOperationException(msg);
                }

                JObject subscriptionData = await response.Content.ReadAsAsync<JObject>();
                InstagramSubscription subs = subscriptionData.Value<JObject>(DataKey).ToObject<InstagramSubscription>();
                return subs;
            }
        }

        private async Task DeleteSubscription(string id, string parameter)
        {
            Tuple<string, string> clientInfo = await GetClientConfig(id);
            string address = string.Format(CultureInfo.InvariantCulture, SubscriptionAddressTemplate, clientInfo.Item1, clientInfo.Item2, parameter);

            using (HttpResponseMessage response = await _httpClient.DeleteAsync(address))
            {
                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = await GetErrorContent(response);
                    string msg = string.Format(CultureInfo.CurrentCulture, InstagramReceiverResources.Client_UnsubscribeFailure, response.StatusCode, errorMessage);
                    throw new InvalidOperationException(msg);
                }
            }
        }
    }
}
