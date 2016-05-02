// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.WebHooks.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WorkItemCommentedOnPayloadTests
    {
        [Fact]
        public void WorkItemCommentedOnPayload_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.workitem.commented.json");
            var expected = new WorkItemCommentedOnPayload
            {
                SubscriptionId = "00000000-0000-0000-0000-000000000000",
                NotificationId = 4,
                Id = "fb2617ed-60df-4518-81fa-749faa6c5cd6",
                EventType = "workitem.commented",
                PublisherId = "tfs",
                Message = new PayloadMessage
                {
                    Text = "Bug #5 (Some great new idea!) commented on by Jamal Hartnett.\r\n(http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&id=5)",
                    Html = "<a href=\"http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&amp;id=5\">Bug #5</a> (Some great new idea!) commented on by Jamal Hartnett.",
                    Markdown = "[Bug #5](http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&id=5) (Some great new idea!) commented on by Jamal Hartnett."
                },
                DetailedMessage = new PayloadMessage
                {
                    Text = "Bug #5 (Some great new idea!) commented on by Jamal Hartnett.\r\n(http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&id=5)\r\nThis is a great new idea",
                    Html = "<a href=\"http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&amp;id=5\">Bug #5</a> (Some great new idea!) commented on by Jamal Hartnett.<br/>This is a great new idea",
                    Markdown = "[Bug #5](http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&id=5) (Some great new idea!) commented on by Jamal Hartnett.\r\nThis is a great new idea"
                },
                Resource = new WorkItemCommentedOnResource
                {
                    Id = 5,
                    RevisionNumber = 4,
                    Fields = new WorkItemFields
                    {
                        SystemAreaPath = "GoodCompanyCloud",
                        SystemTeamProject = "GoodCompanyCloud",
                        SystemIterationPath = "GoodCompanyCloud\\Release 1\\Sprint 1",
                        SystemWorkItemType = "Bug",
                        SystemState = "New",
                        SystemReason = "New defect reported",
                        SystemCreatedDate = "2014-07-15T17:42:44.663Z".ToDateTime(),
                        SystemCreatedBy = "Jamal Hartnett",
                        SystemChangedDate = "2014-07-15T17:42:44.663Z".ToDateTime(),
                        SystemChangedBy = "Jamal Hartnett",
                        SystemTitle = "Some great new idea!",
                        MicrosoftCommonSeverity = "3 - Medium",
                        KanbanColumn = "New",
                        SystemHistory = "This is a great new idea"
                    },
                    Links = new WorkItemLinks
                    {
                        Self = new WorkItemLink { Href = "http://good-company.some.ssl.host/DefaultCollection/_apis/wit/workItems/5" },
                        WorkItemUpdates = new WorkItemLink { Href = "http://good-company.some.ssl.host/DefaultCollection/_apis/wit/workItems/5/updates" },
                        WorkItemRevisions = new WorkItemLink { Href = "http://good-company.some.ssl.host/DefaultCollection/_apis/wit/workItems/5/revisions" },
                        WorkItemType = new WorkItemLink { Href = "http://good-company.some.ssl.host/DefaultCollection/_apis/wit/ea830882-2a3c-4095-a53f-972f9a376f6e/workItemTypes/Bug" },
                        Fields = new WorkItemLink { Href = "http://good-company.some.ssl.host/DefaultCollection/_apis/wit/fields" }
                    },
                    Url = new Uri("http://good-company.some.ssl.host/DefaultCollection/_apis/wit/workItems/5")
                },
                ResourceVersion = "1.0",
                ResourceContainers = new PayloadResourceContainers
                {
                    Collection = new PayloadResourceContainer { Id = "c12d0eb8-e382-443b-9f9c-c52cba5014c2" },
                    Account = new PayloadResourceContainer { Id = "f844ec47-a9db-4511-8281-8b63f4eaf94e" },
                    Project = new PayloadResourceContainer { Id = "be9b3917-87e6-42a4-a549-2bc06a7a878f" }
                },
                CreatedDate = "2016-05-02T19:15:37.4638247Z".ToDateTime()
            };

            // Act
            var actual = data.ToObject<WorkItemCommentedOnPayload>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
