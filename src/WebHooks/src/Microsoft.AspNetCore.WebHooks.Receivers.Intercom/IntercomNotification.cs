// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.WebHooks.ModelBinding;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Contains information sent in a WebHook notification from Intercom.
    /// </summary>
    public class IntercomNotification
    {
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the type of notification. Value is 'notification_event'.
        /// </summary>
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the notification id.
        /// </summary>
        [JsonProperty("id", Required = Required.AllowNull)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Intercom defined URL for the subscription
        /// </summary>
        [JsonProperty("self")]
        public string Self { get; set; }

        /// <summary>
        /// Gets or sets the time the notification was created.
        /// </summary>
        [JsonConverter(typeof(UnixTimeConverter))]
        [JsonProperty("created_at", Required = Required.Always)]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the topic
        /// </summary>
        [JsonProperty("topic", Required = Required.Always)]
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets the number of times this notification has been attempted.
        /// </summary>
        [JsonProperty("delivery_attempts", Required = Required.Always)]
        public int DeliveryAttempts { get; set; }

        /// <summary>
        /// Gets or sets the first time the delivery was attempted.
        /// </summary>
        [JsonConverter(typeof(UnixTimeConverter))]
        [JsonProperty("first_sent_at", Required = Required.Always)]
        public DateTime FirstSentAt { get; set; }

         /// <summary>
        /// Gets or sets the data associated with the notification.
        /// </summary>
        [JsonProperty("data", Required = Required.Always)]
        public IntercomNotificationData Data { get; set; }
    }
}
