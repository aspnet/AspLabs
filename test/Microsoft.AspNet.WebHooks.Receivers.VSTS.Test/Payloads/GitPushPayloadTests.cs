// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.WebHooks.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class GitPushPayloadTests
    {
        [Fact]
        public void GitPushPayload_Roundtrips()
        {
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.git.push.json");
            var expected = new GitPushPayload
            {
                CreatedDate = "2016-06-26T18:10:31.3603573Z".ToDateTime(),
                DetailedMessage = new PayloadMessage()
                {
                    Html = "John Smith pushed 1 commit to branch <a href=\\\"https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/#version=GBmaster\\\">master</a> of <a href=\\\"https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/\\\">Project</a>\\r\\n<ul>\\r\\n<li>A meaningful commit message. <a href=\\\"https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/commit/c8e823a60a85381732726d6a9b6a276e71e6ce12\\\">c8e823a6</a></li>\\r\\n</ul>",
                    Markdown = "John Smith pushed 1 commit to branch [master](https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/#version=GBmaster) of [Project](https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/)\\r\\n* A meaningful commit message. [c8e823a6](https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/commit/c8e823a60a85381732726d6a9b6a276e71e6ce12)",
                    Text = "John Smith pushed 1 commit to branch master of Project\\r\\n - A meaningful commit message. c8e823a6 (https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/commit/c8e823a60a85381732726d6a9b6a276e71e6ce12)"
                },
                EventType = "git.push",
                Id = "cd159468-0509-48d9-960d-6f3ba627fd06",
                Message = new PayloadMessage()
                {
                    Html = "John Smith pushed updates to branch <a href=\\\"https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/#version=GBmaster\\\">master</a> of <a href=\\\"https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/\\\">Project</a>",
                    Markdown = "John Smith pushed updates to branch [master](https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/#version=GBmaster) of [Project](https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/)",
                    Text = "John Smith pushed updates to branch master of Project\\r\\n(https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project/#version=GBmaster)"
                },
                NotificationId = 9,
                PublisherId = "tfs",
                Resource = new GitPushResource()
                {
                    Date = "2016-06-26T18:10:30.065511Z".ToDateTime(),
                    Links = new GitPushLinks()
                    {
                        Commits = new GitLink()
                        {
                            Href = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_apis/git/repositories/7aa31685-abcf-40be-8c18-aaa45067d7bb/pushes/1168/commits")
                        },
                        Pusher = new GitLink()
                        {
                            Href = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_apis/Identities/458616a4-6252-4cd9-accd-38538e7c9c33")
                        },
                        Refs = new GitLink()
                        {
                            Href = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_apis/git/repositories/7aa31685-abcf-40be-8c18-aaa45067d7bb/refs")
                        },
                        Repository = new GitLink()
                        {
                            Href = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_apis/git/repositories/7aa31685-abcf-40be-8c18-aaa45067d7bb")
                        },
                        Self = new GitLink()
                        {
                            Href = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_apis/git/repositories/7aa31685-abcf-40be-8c18-aaa45067d7bb/pushes/1168")
                        }
                    },
                    PushedBy = new GitUser()
                    {
                        DisplayName = "John Smith",
                        Id = "458616a4-6252-4cd9-accd-38538e7c9c33",
                        ImageUrl = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_api/_common/identityImage?id=458616a4-6252-4cd9-accd-38538e7c9c33"),
                        UniqueName = "jsmith",
                        Url = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_apis/Identities/458616a4-6252-4cd9-accd-38538e7c9c33")
                    },
                    PushId = 1168,
                    Repository = new GitRepository()
                    {
                        DefaultBranch = "refs/heads/master",
                        Id = "7aa31685-abcf-40be-8c18-aaa45067d7bb",
                        Name = "Project",
                        Project = new GitProject()
                        {
                            Id = "65e40c52-3c5d-487c-8a45-6b852de287a8",
                            Name = "Project",
                            State = "wellFormed",
                            Url = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_apis/projects/65e40c52-3c5d-487c-8a45-6b852de287a8")
                        },
                        RemoteUrl = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_git/Project"),
                        Url = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_apis/git/repositories/7aa31685-abcf-40be-8c18-aaa45067d7bb")
                    },
                    Url = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_apis/git/repositories/7aa31685-abcf-40be-8c18-aaa45067d7bb/pushes/1168")
                },
                ResourceContainers = new PayloadResourceContainers()
                {
                    Collection = new PayloadResourceContainer()
                    {
                        Id = "d11e28a5-859e-4fd6-841d-a3ee54815568"
                    },
                    Project = new PayloadResourceContainer()
                    {
                        Id = "65e40c52-3c5d-487c-8a45-6b852de287a8"
                    }
                },
                ResourceVersion = "1.0",
                SubscriptionId = "00000000-0000-0000-0000-000000000000"
            };
            expected.Resource.Commits.Add(
                new GitCommit()
                {
                    Author = new GitUserInfo()
                    {
                        Date = "2016-06-26T18:10:21Z".ToDateTime(),
                        Email = "fabrikamfiber16@hotmail.com",
                        Name = "John Smith"
                    },
                    Comment = "A meaningful commit message.",
                    CommitId = "c8e823a60a85381732726d6a9b6a276e71e6ce12",
                    Committer = new GitUserInfo()
                    {
                        Date = "2016-06-26T18:10:21Z".ToDateTime(),
                        Email = "fabrikamfiber16@hotmail.com",
                        Name = "John Smith"
                    },
                    Url = new Uri("https://good-company.some.ssl.host/tfs/GoodCompany/_apis/git/repositories/7aa31685-abcf-40be-8c18-aaa45067d7bb/commits/c8e823a60a85381732726d6a9b6a276e71e6ce12")
                });
            expected.Resource.RefUpdates.Add(
                new GitRefUpdate()
                {
                    Name = "refs/heads/master",
                    NewObjectId = "c8e823a60a85381732726d6a9b6a276e71e6ce12",
                    OldObjectId = "61b7353aa151d2d7d4e4dac8f701b0d82ff87703"
                });

            // Actual
            var actual = data.ToObject<GitPushPayload>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
