// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class InstagramNotificationCollectionTests
    {
        [Fact]
        public void InstagramNotificationCollection_Roundtrips()
        {
            // Arrange
            JArray data = EmbeddedResource.ReadAsJArray("Microsoft.AspNet.WebHooks.Messages.NotificationCollectionMessage.json");
            InstagramNotification[] expected = new InstagramNotification[]
            {
                new InstagramNotification
                {
                    ChangedAspect = "media",
                    Object = "user",
                    UserId = "2174967354",
                    SubscriptionId = "22362655",
                    Data = new InstagramNotificationData
                    {
                        MediaId = "1213184719641169505_2174967354"
                    }
                },
                new InstagramNotification
                {
                    ChangedAspect = "media",
                    Object = "user",
                    UserId = "3174967354",
                    SubscriptionId = "22362655",
                    Data = new InstagramNotificationData
                    {
                        MediaId = "1213184719641169515_3174967354"
                    }
                },
            };

            // Act
            InstagramNotificationCollection actual = data.ToObject<InstagramNotificationCollection>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
