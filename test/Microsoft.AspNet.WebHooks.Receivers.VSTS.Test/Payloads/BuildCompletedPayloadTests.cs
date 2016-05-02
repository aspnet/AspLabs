// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.WebHooks.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class BuildCompletedPayloadTests
    {
        [Fact]
        public void BuildCompletedPayload_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.build.complete.json");

            var expected = new BuildCompletedPayload
            {
                SubscriptionId = "00000000-0000-0000-0000-000000000000",
                NotificationId = 1,
                Id = "4a5d99d6-1c75-4e53-91b9-ee80057d4ce3",
                EventType = "build.complete",
                PublisherId = "tfs",
                Message = new PayloadMessage
                {
                    Text = "Build ConsumerAddressModule_20150407.2 succeeded",
                    Html = "Build <a href=\"https://good-company.some.ssl.host/web/build.aspx?id=5023c10b-bef3-41c3-bf53-686c4e34ee9e&amp;builduri=vstfs%3a%2f%2f%2fBuild%2fBuild%2f3\">ConsumerAddressModule_20150407.2</a> succeeded",
                    Markdown = "Build [ConsumerAddressModule_20150407.2](https://good-company.some.ssl.host/web/build.aspx?id=5023c10b-bef3-41c3-bf53-686c4e34ee9e&builduri=vstfs%3a%2f%2f%2fBuild%2fBuild%2f3) succeeded"
                },
                DetailedMessage = new PayloadMessage
                {
                    Text = "Build ConsumerAddressModule_20150407.2 succeeded",
                    Html = "Build <a href=\"https://good-company.some.ssl.host/web/build.aspx?id=5023c10b-bef3-41c3-bf53-686c4e34ee9e&amp;builduri=vstfs%3a%2f%2f%2fBuild%2fBuild%2f3\">ConsumerAddressModule_20150407.2</a> succeeded",
                    Markdown = "Build [ConsumerAddressModule_20150407.2](https://good-company.some.ssl.host/web/build.aspx?id=5023c10b-bef3-41c3-bf53-686c4e34ee9e&builduri=vstfs%3a%2f%2f%2fBuild%2fBuild%2f3) succeeded"
                },

                Resource = new BuildCompletedResource
                {
                    Uri = new Uri("vstfs:///Build/Build/2"),
                    Id = 2,
                    BuildNumber = "ConsumerAddressModule_20150407.1",
                    Url = new Uri("https://good-company.some.ssl.host/DefaultCollection/71777fbc-1cf2-4bd1-9540-128c1c71f766/_apis/build/Builds/2"),
                    StartTime = "2015-04-07T18:04:06.83Z".ToDateTime(),
                    FinishTime = "2015-04-07T18:06:10.69Z".ToDateTime(),
                    Reason = "manual",
                    Status = "succeeded",
                    DropLocation = "#/3/drop",
                    Drop = new BuildCompletedDrop
                    {
                        Location = "#/3/drop",
                        DropType = "container",
                        Url = new Uri("https://good-company.some.ssl.host/DefaultCollection/_apis/resources/Containers/3/drop"),
                        DownloadUrl = new Uri("https://good-company.some.ssl.host/DefaultCollection/_apis/resources/Containers/3/drop?api-version=1.0&$format=zip&downloadFileName=ConsumerAddressModule_20150407.1_drop")
                    },
                    Log = new BuildCompletedLog
                    {
                        LogType = "container",
                        Url = new Uri("https://good-company.some.ssl.host/DefaultCollection/_apis/resources/Containers/3/logs"),
                        DownloadUrl = new Uri("https://good-company.some.ssl.host/_apis/resources/Containers/3/logs?api-version=1.0&$format=zip&downloadFileName=ConsumerAddressModule_20150407.1_logs")
                    },
                    SourceGetVersion = "LG:refs/heads/master:600c52d2d5b655caa111abfd863e5a9bd304bb0e",
                    LastChangedBy = new ResourceUser
                    {
                        Id = "d6245f20-2af8-44f4-9451-8107cb2767db",
                        DisplayName = "John Smith",
                        UniqueName = "fabrikamfiber16@hotmail.com",
                        Url = new Uri("https://good-company.some.ssl.host/_apis/Identities/d6245f20-2af8-44f4-9451-8107cb2767db"),
                        ImageUrl = new Uri("https://good-company.some.ssl.host/DefaultCollection/_api/_common/identityImage?id=d6245f20-2af8-44f4-9451-8107cb2767db")
                    },
                    RetainIndefinitely = false,
                    HasDiagnostics = true,
                    Definition = new BuildCompletedDefinition
                    {
                        BatchSize = 1,
                        TriggerType = "none",
                        DefinitionType = "xaml",
                        Id = 2,
                        Name = "ConsumerAddressModule",
                        Url = new Uri("https://good-company.some.ssl.host/DefaultCollection/71777fbc-1cf2-4bd1-9540-128c1c71f766/_apis/build/Definitions/2")
                    },
                    Queue = new BuildCompletedQueueDefinition
                    {
                        QueueType = "buildController",
                        Id = 4,
                        Name = "Hosted Build Controller",
                        Url = new Uri("https://good-company.some.ssl.host/DefaultCollection/_apis/build/Queues/4")
                    }
                },
                ResourceVersion = "1.0",
                ResourceContainers = new PayloadResourceContainers
                {
                    Collection = new PayloadResourceContainer { Id = "c12d0eb8-e382-443b-9f9c-c52cba5014c2" },
                    Account = new PayloadResourceContainer { Id = "f844ec47-a9db-4511-8281-8b63f4eaf94e" },
                    Project = new PayloadResourceContainer { Id = "be9b3917-87e6-42a4-a549-2bc06a7a878f" }
                },
                CreatedDate = "2016-05-02T19:00:39.5893296Z".ToDateTime()
            };
            expected.Resource.Requests.Add(new BuildCompletedRequest
            {
                Id = 1,
                Url = new Uri("https://good-company.some.ssl.host/DefaultCollection/71777fbc-1cf2-4bd1-9540-128c1c71f766/_apis/build/Requests/1"),
                RequestedFor = new ResourceUser
                {
                    Id = "d6245f20-2af8-44f4-9451-8107cb2767db",
                    DisplayName = "John Smith",
                    UniqueName = "fabrikamfiber16@hotmail.com",
                    Url = new Uri("https://good-company.some.ssl.host/_apis/Identities/d6245f20-2af8-44f4-9451-8107cb2767db"),
                    ImageUrl = new Uri("https://good-company.some.ssl.host/DefaultCollection/_api/_common/identityImage?id=d6245f20-2af8-44f4-9451-8107cb2767db")
                }
            });

            // Act
            var actual = data.ToObject<BuildCompletedPayload>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
