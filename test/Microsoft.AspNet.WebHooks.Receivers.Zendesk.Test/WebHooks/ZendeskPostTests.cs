// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PostTests", Justification = "Correct term.")]
    public class ZendeskPostTests
    {
        [Fact]
        public void ZendeskPost_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.ZendeskPostMessage.json");
            ZendeskPost expected = new ZendeskPost
            {
                Notification = new ZendeskNotification
                {
                    Body = "Agent replied something something",
                    Title = "Agent replied",
                    TicketId = "5"
                }
            };
            expected.Devices.Add(new ZendeskDevice
            {
                Identifier = "oiuytrdsdfghjk",
                DeviceType = "ios"
            });
            expected.Devices.Add(new ZendeskDevice
            {
                Identifier = "iuytfrdcvbnmkl",
                DeviceType = "android"
            });

            // Act
            ZendeskPost actual = data.ToObject<ZendeskPost>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
