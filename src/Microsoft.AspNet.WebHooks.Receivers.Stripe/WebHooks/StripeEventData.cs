// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if NETSTANDARD2_0
namespace Microsoft.AspNetCore.WebHooks
#else
namespace Microsoft.AspNet.WebHooks
#endif
{
    /// <summary>
    /// Contains information sent in a WebHook notification from Stripe, see
    /// '<c>https://stripe.com/docs/api#event_object</c>' for details.
    /// </summary>
    public class StripeEventData
    {
        /// <summary>
        /// Gets or sets the event data object.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The JObject has to be settable.")]
        [JsonProperty("object")]
        public JObject Object { get; set; }

        /// <summary>
        /// Gets or sets the hash containing the names of the attributes that have changed
        /// and their previous values (only sent along with *.updated events).
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The JObject has to be settable.")]
        [JsonProperty("previous_attributes")]
        public JObject PreviousAttributes { get; set; }
    }
}