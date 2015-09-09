// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http.Routing;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Routes;

namespace System.Web.Http
{
    /// <summary>
    /// Provides various extensions for the <see cref="InstagramWebHookClient"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class InstagramWebHookClientExtensions
    {
        /// <summary>
        /// Subscribes to posts submitted by all users authenticated with this Instagram client.
        /// </summary>
        /// <param name="client">The current <see cref="InstagramWebHookClient"/> instance.</param>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this WebHook. This makes it possible to 
        /// support multiple WebHooks with individual configurations.</param>
        /// <param name="urlHelper">A <see cref="UrlHelper"/> for computing the callback URI.</param>
        /// <returns>A <see cref="InstagramSubscription"/> instance.</returns>
        public static Task<InstagramSubscription> SubscribeAsync(this InstagramWebHookClient client, string id, UrlHelper urlHelper)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            if (urlHelper == null)
            {
                throw new ArgumentNullException("urlHelper");
            }

            Uri callback = GetCallback(id, urlHelper);
            return client.SubscribeAsync(id, callback);
        }

        /// <summary>
        /// Subscribes to a particular tag. For example, For instance, a subscription for the tag <c>'nofilter</c>
        /// will receive event notifications every time anyone posts a new photo with the tag <c>'#nofilter'</c>.
        /// </summary>
        /// <param name="client">The current <see cref="InstagramWebHookClient"/> instance.</param>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this WebHook. This makes it possible to 
        /// support multiple WebHooks with individual configurations.</param>
        /// <param name="urlHelper">A <see cref="UrlHelper"/> for computing the callback URI.</param>
        /// <param name="tag">The tag to subscribe to (without a leading '#')</param>
        /// <returns>A <see cref="InstagramSubscription"/> instance.</returns>
        public static Task<InstagramSubscription> SubscribeAsync(this InstagramWebHookClient client, string id, UrlHelper urlHelper, string tag)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            if (urlHelper == null)
            {
                throw new ArgumentNullException("urlHelper");
            }

            Uri callback = GetCallback(id, urlHelper);
            return client.SubscribeAsync(id, callback, tag);
        }

        /// <summary>
        /// Subscribes to a geographic area identified by a center latitude and longitude and a radius extending from the center. 
        /// </summary>
        /// <param name="client">The current <see cref="InstagramWebHookClient"/> instance.</param>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this WebHook. This makes it possible to 
        /// support multiple WebHooks with individual configurations.</param>
        /// <param name="urlHelper">A <see cref="UrlHelper"/> for computing the callback URI.</param>
        /// <param name="latitude">The center latitude of the geo-area to subscribe to.</param>
        /// <param name="longitude">The center longitude of the geo-area to subscribe to.</param>
        /// <param name="radius">The radius of the geo-area in meters between 0 and 5000.</param>
        /// <returns>A <see cref="InstagramSubscription"/> instance.</returns>
        public static Task<InstagramSubscription> SubscribeAsync(this InstagramWebHookClient client, string id, UrlHelper urlHelper, double latitude, double longitude, int radius)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            if (urlHelper == null)
            {
                throw new ArgumentNullException("urlHelper");
            }

            Uri callback = GetCallback(id, urlHelper);
            return client.SubscribeAsync(id, callback, latitude, longitude, radius);
        }

        internal static Uri GetCallback(string id, UrlHelper urlHelper)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object> { { "webHookReceiver", InstagramWebHookReceiver.ReceiverName }, { "id", id } };
            string callbackLink = urlHelper.Link(WebHookReceiverRouteNames.ReceiversAction, parameters);
            Uri callback = new Uri(callbackLink);
            return callback;
        }
    }
}
