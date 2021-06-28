// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes fields of the WorkItem
    /// </summary>
    public class WorkItemFields
    {
        /// <summary>
        /// Gets the value of field <c>System.AreaPath</c>.
        /// </summary>
        [JsonProperty("System.AreaPath")]
        public string SystemAreaPath { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.TeamProject</c>.
        /// </summary>
        [JsonProperty("System.TeamProject")]
        public string SystemTeamProject { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.IterationPath</c>.
        /// </summary>
        [JsonProperty("System.IterationPath")]
        public string SystemIterationPath { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.WorkItemType</c>.
        /// </summary>
        [JsonProperty("System.WorkItemType")]
        public string SystemWorkItemType { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.State</c>.
        /// </summary>
        [JsonProperty("System.State")]
        public string SystemState { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.Reason</c>.
        /// </summary>
        [JsonProperty("System.Reason")]
        public string SystemReason { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.AssignedTo</c>.
        /// </summary>
        [JsonProperty("System.AssignedTo")]
        public string SystemAssignedTo { get; set; }
        
        /// <summary>
        /// Gets the value of field <c>System.CreatedDate</c>.
        /// </summary>
        [JsonProperty("System.CreatedDate")]
        public DateTime SystemCreatedDate { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.CreatedBy</c>.
        /// </summary>
        [JsonProperty("System.CreatedBy")]
        public string SystemCreatedBy { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.ChangedDate</c>.
        /// </summary>
        [JsonProperty("System.ChangedDate")]
        public DateTime SystemChangedDate { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.ChangedBy</c>.
        /// </summary>
        [JsonProperty("System.ChangedBy")]
        public string SystemChangedBy { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.Title</c>.
        /// </summary>
        [JsonProperty("System.Title")]
        public string SystemTitle { get; set; }

        /// <summary>
        /// Gets the value of field <c>Microsoft.VSTS.Common.Severity</c>.
        /// </summary>
        [JsonProperty("Microsoft.VSTS.Common.Severity")]
        public string MicrosoftCommonSeverity { get; set; }

        /// <summary>
        /// Gets the value of field <c>WEF_EB329F44FE5F4A94ACB1DA153FDF38BA_Kanban.Column</c>.
        /// </summary>
        [JsonProperty("WEF_EB329F44FE5F4A94ACB1DA153FDF38BA_Kanban.Column")]
        public string KanbanColumn { get; set; }

        /// <summary>
        /// Gets the value of field <c>System.History</c>.
        /// </summary>
        [JsonProperty("System.History")]
        public string SystemHistory { get; set; }
    }
}
