// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    public class BuildStartedPayloadTests
    {
        [Fact]
        public void BuildStartedPayload_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.BuildStartedMessage.json");
            BuildStartedPayload expected = new BuildStartedPayload
            {
                FeedIdentifier = "sample-feed",
                FeedUrl = new Uri("https://www.myget.org/F/sample-feed/"),
                Name = "SampleBuild",
                Branch = "master"
            };

            // Act
            BuildStartedPayload actual = data["Payload"].ToObject<BuildStartedPayload>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
