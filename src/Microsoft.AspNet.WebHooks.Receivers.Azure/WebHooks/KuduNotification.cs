// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides information in a WebHook notification sent from Azure Kudu Service (Azure Web App Deployment).
    /// </summary>
    public class KuduNotification
    {
        /// <summary>
        /// Gets or sets the ID or the WebHook.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the status of the WebHook, e.g. <c>success</c>
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the status text of the WebHook.
        /// </summary>
        [JsonProperty("statusText")]
        public string StatusText { get; set; }

        /// <summary>
        /// Gets or sets the email of the author generating the WebHook.
        /// </summary>
        [JsonProperty("authorEmail")]
        public string AuthorEmail { get; set; }

        /// <summary>
        /// Gets or sets the name of the author generating the WebHook.
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; set; }

        /// <summary>
        /// Gets or set a message contained within the WebHook.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a progress indication.
        /// </summary>
        [JsonProperty("progress")]
        public string Progress { get; set; }

        /// <summary>
        /// Gets or sets the user name of the deployer causing the WebHook to be generated.
        /// </summary>
        [JsonProperty("deployer")]
        public string Deployer { get; set; }

        /// <summary>
        /// Gets or sets the time the operation was received by Kudu.
        /// </summary>
        [JsonProperty("receivedTime")]
        public DateTime ReceivedTime { get; set; }

        /// <summary>
        /// Gets or sets the time the operation was started by Kudu.
        /// </summary>
        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the time the operation was completed by Kudu.
        /// </summary>
        [JsonProperty("endTime")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the time the last successful operation was completed.
        /// </summary>
        [JsonProperty("lastSuccessEndTime")]
        public DateTime LastSuccessEndTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation is complete.
        /// </summary>
        [JsonProperty("complete")]
        public bool Complete { get; set; }

        /// <summary>
        /// Gets or sets the name of the site the operation is targeting.
        /// </summary>
        [JsonProperty("siteName")]
        public string SiteName { get; set; }
    }
}
