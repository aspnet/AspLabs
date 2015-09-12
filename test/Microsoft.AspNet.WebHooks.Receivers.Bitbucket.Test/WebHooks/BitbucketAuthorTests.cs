// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class BitbucketAuthorTests
    {
        [Fact]
        public void BitbucketAuthor_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.PushMessage.json");
            BitbucketAuthor expectedAuthor = new BitbucketAuthor
            {
                Raw = "Henrik Frystyk Nielsen <henrikn@microsoft.com>",
                User = new BitbucketUser
                {
                    UserType = "user",
                    DisplayName = "HenrikN",
                    UserName = "HenrikN",
                    UserId = "{534d978b-53c8-401b-93b7-ee1f98716edd}",
                }
            };
            expectedAuthor.User.Links.Add("html", new BitbucketLink { Reference = "https://bitbucket.org/HenrikN/" });
            expectedAuthor.User.Links.Add("avatar", new BitbucketLink { Reference = "https://bitbucket.org/account/HenrikN/avatar/32/" });
            expectedAuthor.User.Links.Add("self", new BitbucketLink { Reference = "https://api.bitbucket.org/2.0/users/HenrikN" });

            // Act
            BitbucketAuthor actualAuthor = data["push"]["changes"][0]["new"]["target"]["author"].ToObject<BitbucketAuthor>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expectedAuthor);
            string actualJson = JsonConvert.SerializeObject(actualAuthor);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
