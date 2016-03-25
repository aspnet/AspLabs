// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.TestUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class InstagramSubscriptionTests
    {
        private InstagramSubscription _sub = new InstagramSubscription();

        [Fact]
        public void Id_Roundtrips()
        {
            PropertyAssert.Roundtrips(_sub, s => s.Id, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void Object_Roundtrips()
        {
            PropertyAssert.Roundtrips(_sub, s => s.Object, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void Aspect_Roundtrips()
        {
            PropertyAssert.Roundtrips(_sub, s => s.Aspect, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void Callback_Roundtrips()
        {
            Uri roundtrip = new Uri("http://localhost");
            PropertyAssert.Roundtrips(_sub, s => s.Callback, PropertySetter.NullRoundtrips, roundtripValue: roundtrip);
        }

        [Fact]
        public void InstagramSubscription_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.SubscriptionMessage.json");
            InstagramSubscription expectedSubscription = new InstagramSubscription
            {
                Id = "19985884",
                Object = "tag",
                Aspect = "media",
                Callback = new Uri("http://requestb.in/18jwdvk1"),
            };

            // Act
            InstagramSubscription actualPost = data.ToObject<InstagramSubscription>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expectedSubscription);
            string actualJson = JsonConvert.SerializeObject(actualPost);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
