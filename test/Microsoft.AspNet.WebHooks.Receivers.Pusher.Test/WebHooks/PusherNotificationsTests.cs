// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestUtilities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class PusherNotificationsTests
    {
        private JsonSerializerSettings _settings = new JsonSerializerSettings();
        private PusherNotifications _notifications;

        public PusherNotificationsTests()
        {
            _notifications = new PusherNotifications { CreatedAt = 34359738368 };
            _notifications.Events.Add(new Dictionary<string, object> { { "k1", "v1" } });
            _notifications.Events.Add(new Dictionary<string, object> { { "k2", "v2" } });
            _notifications.Events.Add(new Dictionary<string, object> { { "k3", "v3" } });
        }

        [Fact]
        public void Serializes_AsExpected()
        {
            SerializationAssert.SerializesAs<PusherNotifications>(_notifications, _settings, "{\"time_ms\":34359738368,\"events\":[{\"k1\":\"v1\"},{\"k2\":\"v2\"},{\"k3\":\"v3\"}]}");
        }

        [Fact]
        public void Serialization_Roundtrips()
        {
            // Act
            string ser = JsonConvert.SerializeObject(_notifications, _settings);
            PusherNotifications actual = JsonConvert.DeserializeObject<PusherNotifications>(ser, _settings);

            // Assert
            Assert.Equal(34359738368, actual.CreatedAt);
            Assert.Equal(3, actual.Events.Count);
            Assert.Equal("v1", actual.Events.ElementAt(0)["k1"]);
            Assert.Equal("v2", actual.Events.ElementAt(1)["k2"]);
            Assert.Equal("v3", actual.Events.ElementAt(2)["k3"]);
        }
    }
}