// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.WebHooks.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class GitPullRequestUpdatedPayloadTests
    {
        [Fact]
        public void GitPullRequestUpdatedPayload_Roundtrips()
        {
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.git.pullrequest.updated.json");
            var expected = new GitPullRequestUpdatedPayload()
            {
                SubscriptionId = "00000000-0000-0000-0000-000000000000",
                NotificationId = 2,
                Id = "af07be1b-f3ad-44c8-a7f1-c4835f2df06b",
                EventType = "git.pullrequest.updated",
                PublisherId = "tfs",
                Message = new PayloadMessage
                {
                    Text = "Jamal Hartnett marked the pull request as completed",
                    Html = "Jamal Hartnett marked the pull request as completed",
                    Markdown = "Jamal Hartnett marked the pull request as completed"
                },
                DetailedMessage = new PayloadMessage
                {
                    Text = "Jamal Hartnett marked the pull request as completed\r\n\r\n- Merge status: Succeeded\r\n- Merge commit: eef717(https://fabrikam.visualstudio.com/DefaultCollection/_apis/git/repositories/4bc14d40-c903-45e2-872e-0462c7748079/commits/eef717f69257a6333f221566c1c987dc94cc0d72)\r\n",
                    Html = "Jamal Hartnett marked the pull request as completed\r\n<ul>\r\n<li>Merge status: Succeeded</li>\r\n<li>Merge commit: <a href=\"https://fabrikam.visualstudio.com/DefaultCollection/_apis/git/repositories/4bc14d40-c903-45e2-872e-0462c7748079/commits/eef717f69257a6333f221566c1c987dc94cc0d72\">eef717</a></li>\r\n</ul>",
                    Markdown = "Jamal Hartnett marked the pull request as completed\r\n\r\n+ Merge status: Succeeded\r\n+ Merge commit: [eef717](https://fabrikam.visualstudio.com/DefaultCollection/_apis/git/repositories/4bc14d40-c903-45e2-872e-0462c7748079/commits/eef717f69257a6333f221566c1c987dc94cc0d72)\r\n"
                },
                Resource = new GitPullRequestUpdatedResource
                {
                    Repository = new GitRepository
                    {
                        Id = "4bc14d40-c903-45e2-872e-0462c7748079",
                        Name = "Fabrikam",
                        Url = new Uri("https://fabrikam.visualstudio.com/DefaultCollection/_apis/git/repositories/4bc14d40-c903-45e2-872e-0462c7748079"),
                        Project = new GitProject
                        {
                            Id = "6ce954b1-ce1f-45d1-b94d-e6bf2464ba2c",
                            Name = "Fabrikam",
                            Url = new Uri("https://fabrikam.visualstudio.com/DefaultCollection/_apis/projects/6ce954b1-ce1f-45d1-b94d-e6bf2464ba2c"),
                            State = "wellFormed"
                        },
                        DefaultBranch = "refs/heads/master",
                        RemoteUrl = new Uri("https://fabrikam.visualstudio.com/DefaultCollection/_git/Fabrikam")
                    },
                    PullRequestId = 1,
                    Status = "completed",
                    CreatedBy = new GitUser()
                    {
                        Id = "54d125f7-69f7-4191-904f-c5b96b6261c8",
                        DisplayName = "Jamal Hartnett",
                        UniqueName = "fabrikamfiber4@hotmail.com",
                        Url = new Uri("https://fabrikam.vssps.visualstudio.com/_apis/Identities/54d125f7-69f7-4191-904f-c5b96b6261c8"),
                        ImageUrl = new Uri("https://fabrikam.visualstudio.com/DefaultCollection/_api/_common/identityImage?id=54d125f7-69f7-4191-904f-c5b96b6261c8")
                    },
                    CreationDate = "2014-06-17T16:55:46.589889Z".ToDateTime(),
                    ClosedDate = "2014-06-30T18:59:12.3660573Z".ToDateTime(),
                    Title = "my first pull request",
                    Description = " - test2\r\n",
                    SourceRefName = "refs/heads/mytopic",
                    TargetRefName = "refs/heads/master",
                    MergeStatus = "succeeded",
                    MergeId = "a10bb228-6ba6-4362-abd7-49ea21333dbd",
                    LastMergeSourceCommit = new GitMergeCommit
                    {
                        CommitId = "53d54ac915144006c2c9e90d2c7d3880920db49c",
                        Url = new Uri("https://fabrikam.visualstudio.com/DefaultCollection/_apis/git/repositories/4bc14d40-c903-45e2-872e-0462c7748079/commits/53d54ac915144006c2c9e90d2c7d3880920db49c")
                    },
                    LastMergeTargetCommit = new GitMergeCommit
                    {
                        CommitId = "a511f535b1ea495ee0c903badb68fbc83772c882",
                        Url = new Uri("https://fabrikam.visualstudio.com/DefaultCollection/_apis/git/repositories/4bc14d40-c903-45e2-872e-0462c7748079/commits/a511f535b1ea495ee0c903badb68fbc83772c882")
                    },
                    LastMergeCommit = new GitMergeCommit
                    {
                        CommitId = "eef717f69257a6333f221566c1c987dc94cc0d72",
                        Url = new Uri("https://fabrikam.visualstudio.com/DefaultCollection/_apis/git/repositories/4bc14d40-c903-45e2-872e-0462c7748079/commits/eef717f69257a6333f221566c1c987dc94cc0d72")
                    },
                    Url = new Uri("https://fabrikam.visualstudio.com/DefaultCollection/_apis/git/repositories/4bc14d40-c903-45e2-872e-0462c7748079/pullRequests/1")
                },
                CreatedDate = "2016-06-27T03:29:23.346286Z".ToDateTime()
            };
            expected.Resource.Reviewers.Add(
                new GitReviewer
                {
                    Vote = 0,
                    Id = "2ea2d095-48f9-4cd6-9966-62f6f574096c",
                    DisplayName = "[Mobile]\\Mobile Team",
                    UniqueName = "vstfs:///Classification/TeamProject/f0811a3b-8c8a-4e43-a3bf-9a049b4835bd\\Mobile Team",
                    Url = new Uri("https://fabrikam.vssps.visualstudio.com/_apis/Identities/2ea2d095-48f9-4cd6-9966-62f6f574096c"),
                    ImageUrl = new Uri("https://fabrikam.visualstudio.com/DefaultCollection/_api/_common/identityImage?id=2ea2d095-48f9-4cd6-9966-62f6f574096c"),
                    IsContainer = true
                });
            expected.Resource.Commits.Add(
                new GitCommit
                {
                    CommitId = "53d54ac915144006c2c9e90d2c7d3880920db49c",
                    Url = new Uri("https://fabrikam.visualstudio.com/DefaultCollection/_apis/git/repositories/4bc14d40-c903-45e2-872e-0462c7748079/commits/53d54ac915144006c2c9e90d2c7d3880920db49c")
                });

            // Actual
            var actual = data.ToObject<GitPullRequestUpdatedPayload>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
