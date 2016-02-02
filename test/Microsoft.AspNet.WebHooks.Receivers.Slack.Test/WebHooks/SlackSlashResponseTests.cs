// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.TestUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class SlackSlashResponseTests
    {
        [Fact]
        public void Text_Roundtrips()
        {
            SlackSlashResponse response = new SlackSlashResponse("Some text");
            PropertyAssert.Roundtrips(response, a => a.Text, PropertySetter.NullThrows, defaultValue: "Some text", roundtripValue: "你好世界");
        }

        [Fact]
        public void ResponseType_Roundtrips()
        {
            SlackSlashResponse response = new SlackSlashResponse("Some text");
            PropertyAssert.Roundtrips(response, a => a.ResponseType, PropertySetter.NullRoundtrips, roundtripValue: "你好世界");
        }

        [Fact]
        public void SlackAttachment_InitializesAttachments()
        {
            // Arrange
            var expected = new SlackAttachment[]
            {
                new SlackAttachment("t1", "f1"),
                new SlackAttachment("t2", "f2"),
                new SlackAttachment("t3", "f3"),
            };

            // Act
            SlackSlashResponse rsp = new SlackSlashResponse("It's 80 degrees right now.", expected) { ResponseType = "in_channel" };

            // Assert
            Assert.Equal(expected, rsp.Attachments);
        }

        [Fact]
        public void SlackSlashResponse_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.SlashResponse.json");
            SlackAttachment att1 = new SlackAttachment
            {
                Fallback = "Required plain-text summary of the attachment.",
                Color = "#36a64f",
                Pretext = "Optional text that appears above the attachment block",
                AuthorName = "Bobby Tables",
                AuthorLink = new Uri("http://flickr.com/bobby/"),
                AuthorIcon = new Uri("http://flickr.com/icons/bobby.jpg"),
                Title = "Slack API Documentation",
                TitleLink = new Uri("https://api.slack.com/"),
                Text = "Optional text that appears within the attachment",
                ImageLink = new Uri("http://my-website.com/path/to/image.jpg"),
                ThumbLink = new Uri("http://example.com/path/to/thumb.png"),
            };
            att1.Fields.Add(new SlackField("Priority", "High") { Short = true });
            att1.Fields.Add(new SlackField("Importance", "Low") { Short = false });

            SlackAttachment att2 = new SlackAttachment
            {
                Fallback = "New ticket from Andrea Lee - Ticket #1943: Can't rest my password - https://groove.hq/path/to/ticket/1943",
                Color = "#7CD197",
                Pretext = "New ticket from Andrea Lee",
                Title = "Ticket #1943: Can't reset my password",
                TitleLink = new Uri("https://groove.hq/path/to/ticket/1943"),
                Text = "Help! I tried to reset my password but nothing happened!",
            };

            SlackSlashResponse expected = new SlackSlashResponse("It's 80 degrees right now.") { ResponseType = "in_channel" };
            expected.Attachments.Add(att1);
            expected.Attachments.Add(att2);

            // Act
            SlackSlashResponse actual = data.ToObject<SlackSlashResponse>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
