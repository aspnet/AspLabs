// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides context information for a WebHook notification sent from Azure Alert Service.
    /// </summary>
    public class AzureAlertContext
    {
        /// <summary>
        /// Gets or sets the unique ID for this alert.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the alert.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the alert.
        /// </summary>
        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the condition type, e.g. '<c>Metric</c>' or '<c>Event</c>'.
        /// </summary>
        [JsonProperty("conditionType", Required = Required.Always)]
        public string ConditionType { get; set; }

        /// <summary>
        /// Gets or sets the Azure subscription ID.
        /// </summary>
        [JsonProperty("subscriptionId", Required = Required.Always)]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the time at which the alert was triggered. The alert is triggered as soon as 
        /// the metric is read from the diagnostics storage.
        /// </summary>
        [JsonProperty("timestamp", Required = Required.Always)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets information about the condition causing the event.
        /// </summary>
        [JsonProperty("condition", Required = Required.Always)]
        public AzureAlertCondition Condition { get; set; }

        /// <summary>
        /// Gets or sets the resource group name of the impacted resource causing the alert.
        /// </summary>
        [JsonProperty("resourceGroupName", Required = Required.Always)]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource causing the alert.
        /// </summary>
        [JsonProperty("resourceName", Required = Required.Always)]
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the type of the impacted resource.
        /// </summary>
        [JsonProperty("resourceType", Required = Required.Always)]
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the resource.
        /// </summary>
        [JsonProperty("resourceId", Required = Required.Always)]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the geographic location of the resource.
        /// </summary>
        [JsonProperty("resourceRegion", Required = Required.Always)]
        public string ResourceRegion { get; set; }

        /// <summary>
        /// Gets or sets a direct link to the resource summary page on the Azure portal.
        /// </summary>
        [JsonProperty("portalLink", Required = Required.Always)]
        public string PortalLink { get; set; }
    }
}
