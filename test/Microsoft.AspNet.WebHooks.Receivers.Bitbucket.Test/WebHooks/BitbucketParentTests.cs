// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class BitbucketParentTests
    {
        [Fact]
        public void BitbucketParent_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.PushMessage.json");
            BitbucketParent expectedParent = new BitbucketParent
            {
                Operation = "commit",
                Hash = "b05057cd04921697c0f119ca40fe4a5afa481074",
            };
            expectedParent.Links.Add("html", new BitbucketLink { Reference = "https://bitbucket.org/henrikfrystyknielsen/henrikntest01/commits/b05057cd04921697c0f119ca40fe4a5afa481074" });
            expectedParent.Links.Add("self", new BitbucketLink { Reference = "https://api.bitbucket.org/2.0/repositories/henrikfrystyknielsen/henrikntest01/commit/b05057cd04921697c0f119ca40fe4a5afa481074" });

            // Act
            BitbucketParent actualParent = data["push"]["changes"][0]["new"]["target"]["parents"][0].ToObject<BitbucketParent>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expectedParent);
            string actualJson = JsonConvert.SerializeObject(actualParent);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
