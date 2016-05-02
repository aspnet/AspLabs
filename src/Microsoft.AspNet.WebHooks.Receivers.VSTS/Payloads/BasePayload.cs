// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Root object of payload sent for all types of events.
    /// </summary>
    /// <typeparam name="T">Type of resource within payload which differs depending on '<c>eventType</c>' field</typeparam>
    public abstract class BasePayload<T> where T : BaseResource
    {
        /// <summary>
        /// Gets the subscription identifier which triggered the event.
        /// </summary>
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets the notification identifier within subscription.
        /// </summary>
        [JsonProperty("notificationId")]
        public int NotificationId { get; set; }

        /// <summary>
        /// Gets the identifier of HTTP request.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        [JsonProperty("eventType")]
        public string EventType { get; set; }

        /// <summary>
        /// Gets the publisher identifier.
        /// </summary>
        [JsonProperty("publisherId")]
        public string PublisherId { get; set; }

        /// <summary>
        /// Gets the message which describes the event.
        /// </summary>
        [JsonProperty("message")]
        public PayloadMessage Message { get; set; }

        /// <summary>
        /// Gets the detailed message which describes the event.
        /// </summary>
        [JsonProperty("detailedMessage")]
        public PayloadMessage DetailedMessage { get; set; }

        /// <summary>
        /// Gets the resource itself - data associated with corresponding event.
        /// </summary>
        [JsonProperty("resource")]
        public T Resource { get; set; }

        /// <summary>
        /// Gets the resource version.
        /// </summary>
        [JsonProperty("resourceVersion")]
        public string ResourceVersion { get; set; }

        /// <summary>
        /// Gets the resource containers.
        /// </summary>
        [JsonProperty("resourceContainers")]
        public PayloadResourceContainers ResourceContainers { get; set; }

        /// <summary>
        /// Gets the date when HTTP request was created.
        /// </summary>
        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }
    }
}
