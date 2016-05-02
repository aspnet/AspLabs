// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.WebHooks.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class CodeCheckedInPayloadTests
    {
        [Fact]
        public void CodeCheckedInPayload_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.tfvc.checkin.json");
            var expected = new CodeCheckedInPayload
            {
                SubscriptionId = "00000000-0000-0000-0000-000000000000",
                NotificationId = 2,
                Id = "f9b4c23e-88dd-4516-b04d-849787304e32",
                EventType = "tfvc.checkin",
                PublisherId = "tfs",
                Message = new PayloadMessage
                {
                    Text = "John Smith checked in changeset 18: Dropping in new Java sample",
                    Html = "John Smith checked in changeset <a href=\"https://good-company.some.ssl.host/web/cs.aspx?id=d81542e4-cdfa-4333-b082-1ae2d6c3ad16&amp;cs=18\">18</a>: Dropping in new Java sample",
                    Markdown = "John Smith checked in changeset [18](https://good-company.some.ssl.host/web/cs.aspx?id=d81542e4-cdfa-4333-b082-1ae2d6c3ad16&cs=18): Dropping in new Java sample"
                },
                DetailedMessage = new PayloadMessage
                {
                    Text = "John Smith checked in changeset 18: Dropping in new Java sample",
                    Html = "John Smith checked in changeset <a href=\"https://good-company.some.ssl.host/web/cs.aspx?id=d81542e4-cdfa-4333-b082-1ae2d6c3ad16&amp;cs=18\">18</a>: Dropping in new Java sample",
                    Markdown = "John Smith checked in changeset [18](https://good-company.some.ssl.host/web/cs.aspx?id=d81542e4-cdfa-4333-b082-1ae2d6c3ad16&cs=18): Dropping in new Java sample"
                },
                Resource = new CodeCheckedInResource
                {
                    ChangesetId = 18,
                    Url = new Uri("https://good-company.some.ssl.host/DefaultCollection/_apis/tfvc/changesets/18"),
                    Author = new ResourceUser
                    {
                        Id = "d6245f20-2af8-44f4-9451-8107cb2767db",
                        DisplayName = "John Smith",
                        UniqueName = "fabrikamfiber16@hotmail.com"
                    },
                    CheckedInBy = new ResourceUser
                    {
                        Id = "d6245f20-2af8-44f4-9451-8107cb2767db",
                        DisplayName = "John Smith",
                        UniqueName = "fabrikamfiber16@hotmail.com"
                    },
                    CreatedDate = "2014-05-12T22:41:16Z".ToDateTime(),
                    Comment = "Dropping in new Java sample"
                },
                ResourceVersion = "1.0",
                ResourceContainers = new PayloadResourceContainers
                {
                    Collection = new PayloadResourceContainer { Id = "c12d0eb8-e382-443b-9f9c-c52cba5014c2" },
                    Account = new PayloadResourceContainer { Id = "f844ec47-a9db-4511-8281-8b63f4eaf94e" },
                    Project = new PayloadResourceContainer { Id = "be9b3917-87e6-42a4-a549-2bc06a7a878f" }
                },
                CreatedDate = "2016-05-02T19:01:11.7056821Z".ToDateTime()
            };

            // Act
            var actual = data.ToObject<CodeCheckedInPayload>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
