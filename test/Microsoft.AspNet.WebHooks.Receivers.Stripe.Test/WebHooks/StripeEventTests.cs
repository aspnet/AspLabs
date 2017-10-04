// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class StripeEventTests
    {
        private DateTime _testTime = new DateTime(1970, 1, 1, 1, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void StripeEvent_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.StripeEvent.json");
            StripeEvent expectedEvent = new StripeEvent
            {
                Id = "evt_17Y0a62eZvKYlo2CfDvB2QrJ",
                Object = "event",
                ApiVersion = "2015-10-16",
                Created = _testTime,
                Data = new StripeEventData
                {
                    Object = JObject.Parse("{ \"id\": \"12345\", \"object\": \"card\" }"),
                    PreviousAttributes = JObject.Parse("{ \"balance\": null, \"next\": 1340924237, \"closed\": false }")
                },
                LiveMode = true,
                PendingWebHooks = 10,
                RequestData = new StripeRequestData
                {
                    Id = "req_7nbnyKCObIkSXC",
                },
                EventType = "customer.source.created",
            };

            // Act
            StripeEvent actualEvent = data.ToObject<StripeEvent>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expectedEvent);
            string actualJson = JsonConvert.SerializeObject(actualEvent);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
