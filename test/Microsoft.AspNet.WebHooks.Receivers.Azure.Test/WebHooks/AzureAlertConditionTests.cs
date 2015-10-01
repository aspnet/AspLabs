// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class AzureAlertConditionsTests
    {
        private JsonSerializerSettings _serializerSettings;

        public AzureAlertConditionsTests()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            };
        }

        [Fact]
        public void AlertCondition_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.AlertMessage1.json");
            AzureAlertCondition expected = new AzureAlertCondition
            {
                MetricName = "CPU percentage",
                MetricUnit = "Count",
                MetricValue = "2.716631",
                Threshold = "10",
                WindowSize = "5",
                TimeAggregation = "Average",
                Operator = "LessThan",
            };

            // Act
            AzureAlertCondition actual = data["context"]["condition"].ToObject<AzureAlertCondition>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected, _serializerSettings);
            string actualJson = JsonConvert.SerializeObject(actual, _serializerSettings);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
