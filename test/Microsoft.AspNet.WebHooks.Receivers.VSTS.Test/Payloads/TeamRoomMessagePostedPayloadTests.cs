// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.WebHooks.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class TeamRoomMessagePostedPayloadTests
    {
        [Fact]
        public void TeamRoomMessagePostedPayload_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.message.posted.json");
            var expected = new TeamRoomMessagePostedPayload
            {
                SubscriptionId = "00000000-0000-0000-0000-000000000000",
                NotificationId = 3,
                Id = "daae438c-296b-4512-b08e-571910874e9b",
                EventType = "message.posted",
                PublisherId = "tfs",
                Message = new PayloadMessage
                {
                    Text = "Jamal Hartnett posted a message to Northward-Fiber-Git Team Room\r\nHello",
                    Html = "Jamal Hartnett posted a message to Northward-Fiber-Git Team Room\r\nHello",
                    Markdown = "Jamal Hartnett posted a message to Northward-Fiber-Git Team Room\r\nHello"
                },
                DetailedMessage = new PayloadMessage
                {
                    Text = "Jamal Hartnett posted a message to Northward-Fiber-Git Team Room\r\nHello",
                    Html = "Jamal Hartnett posted a message to Northward-Fiber-Git Team Room<p>Hello</p>",
                    Markdown = "Jamal Hartnett posted a message to Northward-Fiber-Git Team Room\r\nHello"
                },
                Resource = new TeamRoomMessagePostedResource
                {
                    Id = 0,
                    Content = "Hello",
                    MessageType = "normal",
                    PostedTime = "2014-05-02T19:17:13.3309587Z".ToDateTime(),
                    PostedRoomId = 1,
                    PostedBy = new ResourceUser
                    {
                        Id = "00067FFED5C7AF52@Live.com",
                        DisplayName = "Jamal Hartnett",
                        UniqueName = "Windows Live ID\\fabrikamfiber4@hotmail.com"
                    }
                },
                ResourceVersion = "1.0",
                ResourceContainers = new PayloadResourceContainers
                {
                    Collection = new PayloadResourceContainer { Id = "c12d0eb8-e382-443b-9f9c-c52cba5014c2" },
                    Account = new PayloadResourceContainer { Id = "f844ec47-a9db-4511-8281-8b63f4eaf94e" },
                    Project = new PayloadResourceContainer { Id = "be9b3917-87e6-42a4-a549-2bc06a7a878f" }
                },
                CreatedDate = "2016-05-02T19:13:40.8417653Z".ToDateTime()
            };

            // Act
            var actual = data.ToObject<TeamRoomMessagePostedPayload>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
