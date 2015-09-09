// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Describes an Instagram WebHook event notification. For details about Instagram WebHooks, please 
    /// see <c>https://instagram.com/developer/realtime/</c>.
    /// </summary>
    public class InstagramNotification
    {
        /// <summary>
        /// Gets or sets the object type for this subscription. The basic types provided by Instagram
        /// are 'user', 'tag', 'location', and 'geography'.
        /// </summary>
        [JsonProperty("object")]
        public string Object { get; set; }

        /// <summary>
        /// Gets or sets an additional parameter for this subscription depending on whether it is a user,
        /// tag, location, or geography-based subscription. 
        /// </summary>
        [JsonProperty("object_id")]
        public string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the subscription ID.
        /// </summary>
        [JsonProperty("subscription_id")]
        public string SubscriptionId { get; set; }
    }
}
