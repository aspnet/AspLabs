// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class BitbucketUserTests
    {
        [Fact]
        public void Bitbucket_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.PushMessage.json");
            BitbucketUser expectedUser = new BitbucketUser
            {
                UserType = "user",
                DisplayName = "Henrik Nielsen",
                UserName = "henrikfrystyknielsen",
                UserId = "{73498d6a-711f-4d29-90cd-a13281674474}",
            };
            expectedUser.Links.Add("html", new BitbucketLink { Reference = "https://bitbucket.org/henrikfrystyknielsen/" });
            expectedUser.Links.Add("avatar", new BitbucketLink { Reference = "https://bitbucket.org/account/henrikfrystyknielsen/avatar/32/" });
            expectedUser.Links.Add("self", new BitbucketLink { Reference = "https://api.bitbucket.org/2.0/users/henrikfrystyknielsen" });

            // Act
            BitbucketUser actualUser = data["actor"].ToObject<BitbucketUser>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expectedUser);
            string actualJson = JsonConvert.SerializeObject(actualUser);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
