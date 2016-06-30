// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Describes the resource that associated with <see cref="GitPullRequestCreatedPayload"/>
    /// </summary>
    public class GitPullRequestResource : BaseResource
    {
        private readonly Collection<GitReviewer> _reviewers = new Collection<GitReviewer>();
        private readonly Collection<GitCommit> _commits = new Collection<GitCommit>();

        /// <summary>
        /// The repository being updated
        /// </summary>
        [JsonProperty("repository")]
        public GitRepository Repository { get; set; }

        /// <summary>
        /// The Id of the Pull Request
        /// </summary>
        [JsonProperty("pullRequestId")]
        public int PullRequestId { get; set; }

        /// <summary>
        /// The Status of the Pull Request
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// The user creating the Pull Request
        /// </summary>
        [JsonProperty("createdBy")]
        public GitUser CreatedBy { get; set; }

        /// <summary>
        /// The date the Pull Request was created.
        /// </summary>
        [JsonProperty("creationDate")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The title of the Pull Request.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// The Description of the Pull Request
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Source Reference Name
        /// </summary>
        [JsonProperty("sourceRefName")]
        public string SourceRefName { get; set; }

        /// <summary>
        /// Target Reference Name
        /// </summary>
        [JsonProperty("targetRefName")]
        public string TargetRefName { get; set; }

        /// <summary>
        /// Merge Status
        /// </summary>
        [JsonProperty("mergeStatus")]
        public string MergeStatus { get; set; }

        /// <summary>
        /// Merge Id
        /// </summary>
        [JsonProperty("mergeId")]
        public string MergeId { get; set; }

        /// <summary>
        /// Last Merge Source Commit
        /// </summary>
        [JsonProperty("lastMergeSourceCommit")]
        public GitMergeCommit LastMergeSourceCommit { get; set; }

        /// <summary>
        /// Last Merge Target Commit
        /// </summary>
        [JsonProperty("lastMergeTargetCommit")]
        public GitMergeCommit LastMergeTargetCommit { get; set; }

        /// <summary>
        /// Last Merge Commit
        /// </summary>
        [JsonProperty("lastMergeCommit")]
        public GitMergeCommit LastMergeCommit { get; set; }

        /// <summary>
        /// Pull Request Reviewers
        /// </summary>
        [JsonProperty("reviewers")]
        public Collection<GitReviewer> Reviewers
        {
            get { return _reviewers; }
        }

        /// <summary>
        /// Pull Request Url
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }

        /// <summary>
        /// Commit Links
        /// </summary>
        [JsonProperty("_links")]
        public GitPullLinks Links { get; set; }

        /// <summary>
        /// A list of commits in the pull request
        /// </summary>
        [JsonProperty("commits")]
        public Collection<GitCommit> Commits
        {
            get { return _commits; }
        }
    }
}