// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestUtilities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class CustomNotificationsTests
    {
        private JsonSerializerSettings _settings = new JsonSerializerSettings();
        private CustomNotifications _notifications;

        public CustomNotificationsTests()
        {
            _notifications = new CustomNotifications { Attempt = 1, Id = "123456" };
            _notifications.Properties.Add("pk1", "pv1");
            _notifications.Properties.Add("pk2", "pv2");
            _notifications.Notifications.Add(new Dictionary<string, object> { { "nk1", "nv1" } });
            _notifications.Notifications.Add(new Dictionary<string, object> { { "nk2", "nv2" } });
        }

        [Fact]
        public void Serializes_AsExpected()
        {
            SerializationAssert.SerializesAs<CustomNotifications>(_notifications, _settings, "{\"Id\":\"123456\",\"Attempt\":1,\"Properties\":{\"pk1\":\"pv1\",\"pk2\":\"pv2\"},\"Notifications\":[{\"nk1\":\"nv1\"},{\"nk2\":\"nv2\"}]}");
        }

        [Fact]
        public void Serialization_Roundtrips()
        {
            // Act
            string ser = JsonConvert.SerializeObject(_notifications, _settings);
            CustomNotifications actual = JsonConvert.DeserializeObject<CustomNotifications>(ser, _settings);

            // Assert
            Assert.Equal("123456", actual.Id);
            Assert.Equal(1, actual.Attempt);
            Assert.Equal("pv1", actual.Properties["pk1"]);
            Assert.Equal("pv2", actual.Properties["pk2"]);
            Assert.Equal("nv1", actual.Notifications.ElementAt(0)["nk1"]);
            Assert.Equal("nv2", actual.Notifications.ElementAt(1)["nk2"]);
        }
    }
}