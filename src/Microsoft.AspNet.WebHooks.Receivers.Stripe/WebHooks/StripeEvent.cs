// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.WebHooks.Serialization;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains information sent in a WebHook notification from Stripe, see
    /// '<c>https://stripe.com/docs/api#event_object</c>' for details.
    /// </summary>
    public class StripeEvent
    {
        /// <summary>
        /// Gets or sets the id of the WebHook event
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the object. Value is "event".
        /// </summary>
        [JsonProperty("object")]
        public string Object { get; set; }

        /// <summary>
        /// Gets or sets the Stripe API version used to render data. 
        /// Note: this property is populated for events on or after October 31, 2014.
        /// </summary>
        [JsonProperty("api_version")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the time at which the alert was triggered. 
        /// </summary>
        [JsonConverter(typeof(UnixTimeConverter))]
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the hash containing data associated with the event.
        /// </summary>
        [JsonProperty("data")]
        public StripeEventData Data { get; set; }

        /// <summary>
        /// Gets or sets the boolean property to denote if this is a live event or not.
        /// </summary>
        [JsonProperty("livemode")]
        public bool LiveMode { get; set; }

        /// <summary>
        /// Gets or sets the number of WebHooks yet to be delivered successfully 
        /// (return a 20x response) to the URLs you’ve specified.
        /// </summary>
        [JsonProperty("pending_webhooks")]
        public int PendingWebHooks { get; set; }

        /// <summary>
        /// Gets or sets ID of the API request that caused the event. 
        /// If null, the event was automatic (e.g. Stripe’s automatic subscription handling). 
        /// Request logs are available in the dashboard but currently not in the API. 
        /// Note: this property is populated for events on or after April 23, 2013.
        /// </summary>
        [JsonProperty("request")]
        public string Request { get; set; }

        /// <summary>
        /// Gets or sets the description of the event: e.g. invoice.created, charge.refunded, etc.
        /// </summary>
        [JsonProperty("type")]
        public string EventType { get; set; }
    }
}
