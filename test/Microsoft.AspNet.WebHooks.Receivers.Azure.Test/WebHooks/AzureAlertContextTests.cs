// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class AzureAlertContextTests
    {
        private JsonSerializerSettings _serializerSettings;

        public AzureAlertContextTests()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            };
        }

        [Fact]
        public void AlertContext_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.AlertMessage1.json");
            AzureAlertContext expected = new AzureAlertContext
            {
                Id = "/subscriptions/aaaaaaa-bbbb-cccc-ddd-eeeeeeeeeeeee/resourceGroups/tests123/providers/microsoft.insights/alertrules/HenriknTest02",
                Name = "HenriknTest02",
                Description = "Test",
                ConditionType = "Metric",
                Condition = new AzureAlertCondition
                {
                    MetricName = "CPU percentage",
                    MetricUnit = "Count",
                    MetricValue = "2.716631",
                    Threshold = "10",
                    WindowSize = "5",
                    TimeAggregation = "Average",
                    Operator = "LessThan",
                },
                SubscriptionId = "aaaaaaa-bbbb-cccc-ddd-eeeeeeeeeeeee",
                ResourceGroupName = "tests123",
                Timestamp = DateTime.Parse("2015-09-30T03:55:30.7037012Z").ToUniversalTime(),
                ResourceName = "testmachine",
                ResourceType = "microsoft.classiccompute/virtualmachines",
                ResourceRegion = "West US",
                ResourceId = "/subscriptions/aaaaaaa-bbbb-cccc-ddd-eeeeeeeeeeeee/resourceGroups/tests123/providers/Microsoft.ClassicCompute/virtualMachines/testmachine",
                PortalLink = "https://portal.azure.com/#resource/subscriptions/aaaaaaa-bbbb-cccc-ddd-eeeeeeeeeeeee/resourceGroups/tests123/providers/Microsoft.ClassicCompute/virtualMachines/testmachine",
            };

            // Act
            AzureAlertContext actual = data["context"].ToObject<AzureAlertContext>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected, _serializerSettings);
            string actualJson = JsonConvert.SerializeObject(actual, _serializerSettings);
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void AlertContext_SubscriptionIdIsRequired()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.AlertMessage3.json");
            ((JObject)data["context"]).Property("subscriptionId").Remove();
            var json = JsonConvert.SerializeObject(data["context"], _serializerSettings);

            // Act / Assert
            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<AzureAlertContext>(json));
        }
    }
}
