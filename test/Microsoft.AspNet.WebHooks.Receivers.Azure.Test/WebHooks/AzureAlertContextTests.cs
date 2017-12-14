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
            var data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.AlertMessage1.json");
            var expected = new AzureAlertContext
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
            var actual = data["context"].ToObject<AzureAlertContext>();

            // Assert
            var expectedJson = JsonConvert.SerializeObject(expected, _serializerSettings);
            var actualJson = JsonConvert.SerializeObject(actual, _serializerSettings);
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void AlertContext_SubscriptionIdIsRequired()
        {
            // Arrange
            var data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.AlertMessage3.json");
            ((JObject)data["context"]).Property("subscriptionId").Remove();
            var json = JsonConvert.SerializeObject(data["context"], _serializerSettings);

            // Act / Assert
            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<AzureAlertContext>(json));
        }

        // ResourceRegion is not required.
        [Fact]
        public void AlertContextForWebTest_Roundtrips()
        {
            // Arrange
            var expectedContext = new AzureAlertContext
            {
                Condition = new AzureAlertCondition
                {
                    FailureDetails = string.Empty,
                    MetricName = "Failed Locations",
                    MetricUnit = "locations",
                    MetricValue = "0",
                    Operator = "GreaterThan",
                    Threshold = "3",
                    TimeAggregation = "Sum",
                    WebTestName = "testazuretestaboutpage-azuretest20170922040158",
                    WindowSize = "5",
                },
                ConditionType = "Webtest",
                Description = string.Empty,
                Id = "/subscriptions/6e0cb82e-37c4-473b-bb45-8b546cfc01b6/resourceGroups/resources/providers/" +
                "microsoft.insights/alertrules/testazuretestaboutpage-azuretest20170922040158-47970367-464e-" +
                "4bf0-b870-b72aa668b591",

                Name = "testazuretestaboutpage-azuretest20170922040158-47970367-464e-4bf0-b870-b72aa668b591",
                PortalLink = "https://go.microsoft.com/fwlink/?LinkID=615149&subscriptionId=6e0cb82e-37c4-473b-" +
                "bb45-8b546cfc01b6&resourceGroup=resources&resourceType=webtests&resourceName=" +
                "testazuretestaboutpage-azuretest20170922040158&tc=ZAMAAB-LCAAAAAAABADFkltLw0AQhc-fcV8kJdlN2-" +
                "RRBEH0SQuCb5umN7xEelHw1_vtpNY-FJ8EWWYye-ZyJieJcrrQtR40U6OlOs4TWMR6bIvf4B1xVK6pagXiRhlRVKWSqCQzk" +
                "idq8LXGRC21cxVU5xpiDbcZGUem004LkJ3xf_Jck5vsuTzVBZU5k7zdSkOGsLnDRvHQ94M0NnmrN-IFaHai6v-" +
                "mZ4aXRB4NKpTJTJvWciX3wtSsUKrlpPrInDnW0hPJFzbL6QO7AlnpGeZW57pluynIFqzTK9s4bEXujMkR75nU-7_" +
                "6lgO9w7BBg8ge6f131Cb-AR2dXvZ8HR0bOtbUH2-YkJUp19fdMPv4FFja5A7mALO3-5gzsr0moBVxbT7wzO0dLk_iv-_" +
                "SZye6N95gOvc8iTXYn_v9dTO8Nx9QPqG9xkNjCfalvB7pcPoC4ncYlWQDAAA1&aadTenantId=h",

                ResourceGroupName = "resources",
                ResourceId = "/subscriptions/6e0cb82e-37c4-473b-bb45-8b546cfc01b6/resourceGroups/resources/" +
                "providers/microsoft.insights/components/AzureTest20170922040158",

                ResourceName = "AzureTest20170922040158",
                ResourceType = "components",
                SubscriptionId = "6e0cb82e-37c4-473b-bb45-8b546cfc01b6",
                Timestamp = DateTime.Parse("12/13/2017 20:53:57Z"),
            };
            var expectedString = JsonConvert.SerializeObject(expectedContext, _serializerSettings);
            var json = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.AzureAlert.WebTest.json");

            // Act
            var actualContext = json["context"].ToObject<AzureAlertContext>();

            // Assert
            var actualString = JsonConvert.SerializeObject(actualContext, _serializerSettings);
            Assert.Equal(expectedString, actualString);
        }
    }
}
