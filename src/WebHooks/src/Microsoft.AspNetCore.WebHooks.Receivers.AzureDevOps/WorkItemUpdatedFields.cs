// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes fields of the WorkItem that was updated
    /// </summary>
    public class WorkItemUpdatedFields
    {
        /// <summary>
        /// Gets the change information for the field '<c>System.Rev</c>'.
        /// </summary>
        [JsonProperty("System.Rev")]
        public WorkItemUpdatedFieldValue<string> SystemRev { get; set; }

        /// <summary>
        /// Gets the change information for the field '<c>System.AuthorizedDate</c>'.
        /// </summary>
        [JsonProperty("System.AuthorizedDate")]
        public WorkItemUpdatedFieldValue<DateTime> SystemAuthorizedDate { get; set; }

        /// <summary>
        /// Gets the change information for the field '<c>System.RevisedDate</c>'.
        /// </summary>
        [JsonProperty("System.RevisedDate")]
        public WorkItemUpdatedFieldValue<DateTime> SystemRevisedDate { get; set; }

        /// <summary>
        /// Gets the change information for the field '<c>System.State</c>'.
        /// </summary>
        [JsonProperty("System.State")]
        public WorkItemUpdatedFieldValue<string> SystemState { get; set; }

        /// <summary>
        /// Gets the change information for the field '<c>System.Reason</c>'.
        /// </summary>
        [JsonProperty("System.Reason")]
        public WorkItemUpdatedFieldValue<string> SystemReason { get; set; }

        /// <summary>
        /// Gets the change information for the field '<c>System.AssignedTo</c>'.
        /// </summary>
        [JsonProperty("System.AssignedTo")]
        public WorkItemUpdatedFieldValue<string> SystemAssignedTo { get; set; }

        /// <summary>
        /// Gets the change information for the field '<c>System.ChangedDate</c>'.
        /// </summary>
        [JsonProperty("System.ChangedDate")]
        public WorkItemUpdatedFieldValue<DateTime> SystemChangedDate { get; set; }

        /// <summary>
        /// Gets the change information for the field '<c>System.Watermark</c>'.
        /// </summary>
        [JsonProperty("System.Watermark")]
        public WorkItemUpdatedFieldValue<string> SystemWatermark { get; set; }

        /// <summary>
        /// Gets the change information for the field '<c>Microsoft.VSTS.Common.Severity</c>'.
        /// </summary>
        [JsonProperty("Microsoft.Vsts.Common.Severity")]
        public WorkItemUpdatedFieldValue<string> MicrosoftCommonSeverity { get; set; }
    }    
}
