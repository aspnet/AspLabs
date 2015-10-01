// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class KuduNotificationTests
    {
        private JsonSerializerSettings _serializerSettings;

        public KuduNotificationTests()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            };
        }

        [Fact]
        public void KuduNotification_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.KuduMessage.json");
            KuduNotification expected = new KuduNotification
            {
                Id = "ff17489fbcb7e2dda9012ec285811b9b751ebb5e",
                Status = "success",
                StatusText = string.Empty,
                AuthorEmail = "henrikn@microsoft.com",
                Author = "Henrik Frystyk Nielsen",
                Message = "initial commit\n",
                Progress = string.Empty,
                Deployer = "HenrikN",
                ReceivedTime = DateTime.Parse("2015-09-26T04:26:53.8736751Z"),
                StartTime = DateTime.Parse("2015-09-26T04:26:54.2486694Z"),
                EndTime = DateTime.Parse("2015-09-26T04:26:55.6393049Z"),
                LastSuccessEndTime = DateTime.Parse("2015-09-26T04:26:55.6393049Z"),
                Complete = true,
                SiteName = "test",
            };

            // Act
            KuduNotification actual = data.ToObject<KuduNotification>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected, _serializerSettings);
            string actualJson = JsonConvert.SerializeObject(actual, _serializerSettings);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
