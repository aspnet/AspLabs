// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains information sent in a WebHook notification from Stripe. Describes the API request that caused the
    /// event. See <see href="https://stripe.com/docs/api/curl#event_object-request"/> for details.
    /// </summary>
    public class StripeRequestData
    {
        /// <summary>
        /// Gets or sets the ID of the API request that caused the event.
        /// </summary>
        /// <value><see langword="null"/> if the event was automatic. Otherwise, the API request ID.</value>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the idempotency key transmitted in the API request.
        /// </summary>
        /// <remarks>This property is only populated for events on or after May 23, 2017.</remarks>
        [JsonProperty("idempotency_key")]
        public string IdempotencyKey { get; set; }
    }
}
