// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    public class ZendeskNotificationTests
    {
        [Fact]
        public void ZendeskNotification_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.ZendeskPostMessage.json");
            ZendeskNotification expected = new ZendeskNotification
            {
                Body = "Agent replied something something",
                Title = "Agent replied",
                TicketId = "5"
            };

            // Act
            ZendeskNotification actual = data["notification"].ToObject<ZendeskNotification>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
