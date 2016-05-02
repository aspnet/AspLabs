// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.WebHooks.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WorkItemUpdatedPayloadTests
    {
        [Fact]
        public void WorkItemUpdatedPayload_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.workitem.updated.json");
            var expected = new WorkItemUpdatedPayload
            {
                SubscriptionId = "00000000-0000-0000-0000-000000000000",
                NotificationId = 8,
                Id = "27646e0e-b520-4d2b-9411-bba7524947cd",
                EventType = "workitem.updated",
                PublisherId = "tfs",
                Message = new PayloadMessage
                {
                    Text = "Bug #5 (Some great new idea!) updated by Jamal Hartnett.\r\n(http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&id=5)",
                    Html = "<a href=\"http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&amp;id=5\">Bug #5</a> (Some great new idea!) updated by Jamal Hartnett.",
                    Markdown = "[Bug #5](http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&id=5) (Some great new idea!) updated by Jamal Hartnett."
                },
                DetailedMessage = new PayloadMessage
                {
                    Text = "Bug #5 (Some great new idea!) updated by Jamal Hartnett.\r\n(http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&id=5)\r\n\r\n- New State: Approved\r\n",
                    Html = "<a href=\"http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&amp;id=5\">Bug #5</a> (Some great new idea!) updated by Jamal Hartnett.<ul>\r\n<li>New State: Approved</li></ul>",
                    Markdown = "[Bug #5](http://good-company.some.ssl.host/web/wi.aspx?id=74e918bf-3376-436d-bd20-8e8c1287f465&id=5) (Some great new idea!) updated by Jamal Hartnett.\r\n\r\n* New State: Approved\r\n"
                },
                Resource = new WorkItemUpdatedResource
                {
                    Id = 2,
                    WorkItemId = 0,
                    RevisionNumber = 2,
                    RevisedBy = null,
                    RevisedDate = "0001-01-01T00:00:00".ToDateTime(),
                    Fields = new WorkItemUpdatedFields
                    {
                        SystemRev = new WorkItemUpdatedFieldValue<string>
                        {
                            OldValue = "1",
                            NewValue = "2"
                        },
                        SystemAuthorizedDate = new WorkItemUpdatedFieldValue<DateTime>
                        {
                            OldValue = "2014-07-15T16:48:44.663Z".ToDateTime(),
                            NewValue = "2014-07-15T17:42:44.663Z".ToDateTime()
                        },
                        SystemRevisedDate = new WorkItemUpdatedFieldValue<DateTime>
                        {
                            OldValue = "2014-07-15T17:42:44.663Z".ToDateTime(),
                            NewValue = "9999-01-01T00:00:00Z".ToDateTime()
                        },
                        SystemState = new WorkItemUpdatedFieldValue<string>
                        {
                            OldValue = "New",
                            NewValue = "Approved"
                        },
                        SystemReason = new WorkItemUpdatedFieldValue<string>
                        {
                            OldValue = "New defect reported",
                            NewValue = "Approved by the Product Owner"
                        },
                        SystemAssignedTo = new WorkItemUpdatedFieldValue<string>
                        {
                            NewValue = "Jamal Hartnet"
                        },
                        SystemChangedDate = new WorkItemUpdatedFieldValue<DateTime>
                        {
                            OldValue = "2014-07-15T16:48:44.663Z".ToDateTime(),
                            NewValue = "2014-07-15T17:42:44.663Z".ToDateTime()
                        },
                        SystemWatermark = new WorkItemUpdatedFieldValue<string>
                        {
                            OldValue = "2",
                            NewValue = "5"
                        },
                        MicrosoftCommonSeverity = new WorkItemUpdatedFieldValue<string>
                        {
                            OldValue = "3 - Medium",
                            NewValue = "2 - High"
                        }
                    },
                    Links = new WorkItemLinks
                    {
                        Self = new WorkItemLink { Href = "http://good-company.some.ssl.host/DefaultCollection/_apis/wit/workItems/5/updates/2" },
                        Parent = new WorkItemLink { Href = "http://good-company.some.ssl.host/DefaultCollection/_apis/wit/workItems/5" },
                        WorkItemUpdates = new WorkItemLink { Href = "http://good-company.some.ssl.host/DefaultCollection/_apis/wit/workItems/5/updates" }
                    },
                    Url = new Uri("http://good-company.some.ssl.host/DefaultCollection/_apis/wit/workItems/5/updates/2"),
                    Revision = new WorkItemUpdatedRevision
                    {
                        Id = 5,
                        Rev = 2,
                        Fields = new WorkItemFields
                        {
                            SystemAreaPath = "GoodCompanyCloud",
                            SystemTeamProject = "GoodCompanyCloud",
                            SystemIterationPath = "GoodCompanyCloud\\Release 1\\Sprint 1",
                            SystemWorkItemType = "Bug",
                            SystemState = "New",
                            SystemReason = "New defect reported",
                            SystemCreatedDate = "2014-07-15T16:48:44.663Z".ToDateTime(),
                            SystemCreatedBy = "Jamal Hartnett",
                            SystemChangedDate = "2014-07-15T16:48:44.663Z".ToDateTime(),
                            SystemChangedBy = "Jamal Hartnett",
                            SystemTitle = "Some great new idea!",
                            MicrosoftCommonSeverity = "3 - Medium",
                            KanbanColumn = "New"
                        },
                        Url = new Uri("http://good-company.some.ssl.host/DefaultCollection/_apis/wit/workItems/5/revisions/2")
                    }
                },
                ResourceVersion = "1.0",
                ResourceContainers = new PayloadResourceContainers
                {
                    Collection = new PayloadResourceContainer { Id = "c12d0eb8-e382-443b-9f9c-c52cba5014c2" },
                    Account = new PayloadResourceContainer { Id = "f844ec47-a9db-4511-8281-8b63f4eaf94e" },
                    Project = new PayloadResourceContainer { Id = "be9b3917-87e6-42a4-a549-2bc06a7a878f" }
                },
                CreatedDate = "2016-05-02T19:19:12.8836446Z".ToDateTime()
            };

            // Act
            var actual = data.ToObject<WorkItemUpdatedPayload>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
