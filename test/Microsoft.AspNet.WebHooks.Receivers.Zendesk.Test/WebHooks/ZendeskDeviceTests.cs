// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    public class ZendeskDeviceTests
    {
        [Fact]
        public void ZendeskDevice_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.ZendeskPostMessage.json");
            ZendeskDevice expected = new ZendeskDevice
            {
                Identifier = "oiuytrdsdfghjk",
                DeviceType = "ios"
            };

            // Act
            ZendeskDevice actual = data["devices"][0].ToObject<ZendeskDevice>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
