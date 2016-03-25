// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Describes an Instagram WebHook event notification. For details about Instagram WebHooks, please 
    /// see <c>https://www.instagram.com/developer/subscriptions/</c>.
    /// </summary>
    public class InstagramNotification
    {
        /// <summary>
        /// Gets or sets the aspect of the subscribed object that changed. Currently, the only type provided by Instagram is 'media'.
        /// </summary>
        [JsonProperty("changed_aspect")]
        public string ChangedAspect { get; set; }

        /// <summary>
        /// Gets or sets the object type for this subscription. Currently, the only type provided by Instagram
        /// is 'user'.
        /// </summary>
        [JsonProperty("object")]
        public string Object { get; set; }

        /// <summary>
        /// Gets or sets the User ID originating the notification.
        /// </summary>
        [JsonProperty("object_id")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the subscription causing this notification.
        /// </summary>
        [JsonProperty("subscription_id")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the data portion of the notification.
        /// </summary>
        [JsonProperty("data")]
        public InstagramNotificationData Data { get; set; }
    }
}
