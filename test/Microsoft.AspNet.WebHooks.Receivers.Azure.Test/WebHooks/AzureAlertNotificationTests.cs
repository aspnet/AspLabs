// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class AzureAlertNotificationTests
    {
        private JsonSerializerSettings _serializerSettings;

        public AzureAlertNotificationTests()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            };
        }

        [Fact]
        public void AlertNotification_Roundtrips()
        {
            // Arrange
            var data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.AlertMessage2.json");
            var expected = new AzureAlertNotification
            {
                Status = "Activated",
                Context = new AzureAlertContext
                {
                    Id = "/subscriptions/aaaaaaa-bbbb-cccc-ddd-eeeeeeeeeeeee/resourceGroups/WebHookReceivers/providers/microsoft.insights/alertrules/henrikntest01",
                    Name = "henrikntest01",
                    Description = "Requests",
                    ConditionType = "Metric",
                    Condition = new AzureAlertCondition
                    {
                        MetricName = "Http 2xx",
                        MetricUnit = "Count",
                        MetricValue = "8",
                        Threshold = "1",
                        WindowSize = "5",
                        TimeAggregation = "Total",
                        Operator = "GreaterThan",
                    },
                    SubscriptionId = "aaaaaaa-bbbb-cccc-ddd-eeeeeeeeeeeee",
                    ResourceGroupName = "WebHookReceivers",
                    Timestamp = DateTime.Parse("2015-09-30T03:02:33.4147662Z").ToUniversalTime(),
                    ResourceName = "webhookreceivers",
                    ResourceType = "microsoft.web/sites",
                    ResourceId = "/subscriptions/aaaaaaa-bbbb-cccc-ddd-eeeeeeeeeeeee/resourceGroups/WebHookReceivers/providers/Microsoft.Web/sites/WebHookReceivers",
                    ResourceRegion = "West US",
                    PortalLink = "https://portal.azure.com/#resource/subscriptions/aaaaaaa-bbbb-cccc-ddd-eeeeeeeeeeeee/resourceGroups/WebHookReceivers/providers/Microsoft.Web/sites/WebHookReceivers",
                }
            };
            expected.Properties.Add("prop1", "value1");
            expected.Properties.Add("prop2", 12345.00);

            // Act
            var actual = data.ToObject<AzureAlertNotification>();

            // Assert
            var expectedJson = JsonConvert.SerializeObject(expected, _serializerSettings);
            var actualJson = JsonConvert.SerializeObject(actual, _serializerSettings);
            Assert.Equal(expectedJson, actualJson);
        }

        [Theory]
        [InlineData("AlertMessage1.json", "Activated")]
        [InlineData("AlertMessage2.json", "Activated")]
        [InlineData("AlertMessage3.json", "Activated")]
        [InlineData("AzureAlert.WebTest.json", "Resolved")]
        public void AlertContext_ParsesMessages(string fileName, string expected)
        {
            // Arrange
            var filePath = "Microsoft.AspNet.WebHooks.Messages." + fileName;
            var data = EmbeddedResource.ReadAsJObject(filePath);

            // Act
            var actual = data.ToObject<AzureAlertNotification>();

            // Assert
            Assert.Equal(expected, actual.Status);
        }
    }
}
