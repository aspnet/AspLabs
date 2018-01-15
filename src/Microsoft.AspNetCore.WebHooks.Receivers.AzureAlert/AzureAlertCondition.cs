// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Provides information about the condition under which the WebHook notification was sent from Azure Alert Service.
    /// </summary>
    public class AzureAlertCondition
    {
        /// <summary>
        /// Gets or sets the name of the metric that defines what the rule monitors.
        /// </summary>
        [JsonProperty("metricName")]
        public string MetricName { get; set; }

        /// <summary>
        /// Gets or sets the units allowed in the metric, e.g. 'Bytes' and 'Percent'.
        /// See '<c>https://msdn.microsoft.com/en-us/library/microsoft.azure.insights.models.unit.aspx</c>'
        /// for details.
        /// </summary>
        [JsonProperty("metricUnit")]
        public string MetricUnit { get; set; }

        /// <summary>
        /// Gets or sets the actual value of the metric that caused the event.
        /// </summary>
        [JsonProperty("metricValue")]
        public string MetricValue { get; set; }

        /// <summary>
        /// Gets or sets the threshold value that activates the event.
        /// </summary>
        [JsonProperty("threshold")]
        public string Threshold { get; set; }

        /// <summary>
        /// Gets or sets the period of time that is used to monitor alert activity based on
        /// the threshold. The value is between 5 minutes and 1 day.
        /// </summary>
        [JsonProperty("windowSize")]
        public string WindowSize { get; set; }

        /// <summary>
        /// Gets or sets how the data is collection, e.g. 'Average' and 'Last'.
        /// See '<c>https://msdn.microsoft.com/en-us/library/microsoft.azure.insights.models.aggregationtype.aspx</c>' for details.
        /// </summary>
        [JsonProperty("timeAggregation")]
        public string TimeAggregation { get; set; }

        /// <summary>
        /// Gets or sets the operator used to compare the data and the threshold.
        /// </summary>
        [JsonProperty("operator")]
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets details of an Availability (Web Test) failure.
        /// </summary>
        /// <remarks>Set in Availability (Web Test) alerts but not Metric alerts.</remarks>
        [JsonProperty("failureDetails")]
        public string FailureDetails { get; set; }

        /// <summary>
        /// Gets or sets the name of an Availability (Web Test) alert.
        /// </summary>
        /// <remarks>Set in Availability (Web Test) alerts but not Metric alerts.</remarks>
        [JsonProperty("webTestName")]
        public string WebTestName { get; set; }
    }
}
