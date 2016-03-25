// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Describes an Instagram WebHook subscription. For details about Instagram WebHooks, please 
    /// see <c>https://www.instagram.com/developer/subscriptions/</c>.
    /// </summary>
    public class InstagramSubscription
    {
        /// <summary>
        /// Gets or sets the unique ID of this subscription.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the object type for this subscription. Currently, the only type provided
        /// by Instagram is 'user'.
        /// </summary>
        [JsonProperty("object")]
        public string Object { get; set; }

        /// <summary>
        /// Gets or sets the aspect of the object for this subscription. Currently only 'media' is supported, 
        /// but other types of subscriptions may be added in the future.
        /// </summary>
        [JsonProperty("aspect")]
        public string Aspect { get; set; }

        /// <summary>
        /// Gets or sets the callback URI where event notifications are sent.
        /// </summary>
        [JsonProperty("callback_url")]
        public Uri Callback { get; set; }
    }
}
