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
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the alert.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the alert.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the condition type, e.g. '<c>Metric</c>' or '<c>Event</c>'.
        /// </summary>
        [JsonProperty("conditionType")]
        public string ConditionType { get; set; }

        /// <summary>
        /// Gets or sets the Azure subscription ID.
        /// </summary>
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the time at which the alert was triggered. The alert is triggered as soon as 
        /// the metric is read from the diagnostics storage.
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets information about the condition causing the event.
        /// </summary>
        public AzureAlertCondition Condition { get; set; }

        /// <summary>
        /// Gets or sets the resource group name of the impacted resource causing the alert.
        /// </summary>
        [JsonProperty("resourceGroupName")]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource causing the alert.
        /// </summary>
        [JsonProperty("resourceName")]
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the type of the impacted resource.
        /// </summary>
        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the resource.
        /// </summary>
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the region where the resource is located.
        /// </summary>
        [JsonProperty("resourceRegion")]
        public string ResourceRegion { get; set; }

        /// <summary>
        /// Gets or sets a direct link to the resource summary page on the Azure portal.
        /// </summary>
        [JsonProperty("portalLink")]
        public string PortalLink { get; set; }
    }
}
