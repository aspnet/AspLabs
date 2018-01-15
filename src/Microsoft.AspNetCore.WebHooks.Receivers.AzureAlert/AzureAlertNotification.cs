// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Contains information sent in a WebHook notification from Azure Alert Service.
    /// </summary>
    public class AzureAlertNotification
    {
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the kind of alert. Azure automatically sends activated and resolved alerts for the condition sets.
        /// Examples of values include '<c>Activated</c>' and '<c>Resolved</c>'.
        /// </summary>
        [JsonProperty("status", Required = Required.Always)]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets context information for this alert.
        /// </summary>
        [JsonProperty("context", Required = Required.Always)]
        public AzureAlertContext Context { get; set; }

        /// <summary>
        /// Gets a collection of additional properties for this alert.
        /// </summary>
        [JsonProperty("properties")]
        public IDictionary<string, object> Properties
        {
            get { return _properties; }
        }
    }
}
