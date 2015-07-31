// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class PusherNotificationTests
    {
        private PusherNotification _notification;

        public PusherNotificationTests()
        {
            _notification = new PusherNotification();
        }

        [Fact]
        public void CreatedAt_Roundtrips()
        {
            PropertyAssert.Roundtrips(_notification, c => c.CreatedAt, defaultValue: 0, roundtripValue: long.MaxValue);
        }

        [Fact]
        public void Events_Roundtrips()
        {
            // Arrange
            JObject data = new JObject();

            // Act
            _notification.Events.Add("Test", data);
            JObject actual = _notification.Events["test"];

            // Assert
            Assert.Same(data, actual);
        }
    }
}
