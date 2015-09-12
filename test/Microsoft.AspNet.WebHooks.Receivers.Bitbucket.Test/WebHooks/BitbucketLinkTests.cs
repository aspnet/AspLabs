// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class BitbucketLinkTests
    {
        [Fact]
        public void BitbucketLink_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.PushMessage.json");
            BitbucketLink expectedLink = new BitbucketLink
            {
                Reference = "https://bitbucket.org/henrikfrystyknielsen/henrikntest01"
            };

            // Act
            BitbucketLink actualLink = data["repository"]["links"]["html"].ToObject<BitbucketLink>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expectedLink);
            string actualJson = JsonConvert.SerializeObject(actualLink);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
