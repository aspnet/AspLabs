// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Describes one or more event notifications received as a Pusher WebHook.
    /// For details about Pusher WebHooks, see <c>https://pusher.com/docs/webhooks</c>.
    /// </summary>
    public class PusherNotification
    {
        private readonly IDictionary<string, JObject> _events = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///  Gets or sets a Unix time stamp in milliseconds which can be used to determine the order in which 
        ///  Pusher events were generated. If desired, the time stamp can be converted using 
        ///  <see cref="M:DateTimeOffset.FromUnixTimeMilliseconds"/>.
        /// </summary>
        public long CreatedAt { get; set; }

        /// <summary>
        /// Gets the set of events contained in this notification from a Pusher WebHook.
        /// </summary>
        public IDictionary<string, JObject> Events
        {
            get { return _events; }
        }
    }
}
