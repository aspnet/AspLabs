// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class BitbucketTargetTests
    {
        [Fact]
        public void BitbucketTarget_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.PushMessage.json");
            BitbucketUser expectedUser = new BitbucketUser
            {
                UserType = "user",
                DisplayName = "HenrikN",
                UserName = "HenrikN",
                UserId = "{534d978b-53c8-401b-93b7-ee1f98716edd}",
            };
            expectedUser.Links.Add("html", new BitbucketLink { Reference = "https://bitbucket.org/HenrikN/" });
            expectedUser.Links.Add("avatar", new BitbucketLink { Reference = "https://bitbucket.org/account/HenrikN/avatar/32/" });
            expectedUser.Links.Add("self", new BitbucketLink { Reference = "https://api.bitbucket.org/2.0/users/HenrikN" });

            BitbucketAuthor expectedAuthor = new BitbucketAuthor
            {
                User = expectedUser,
                Raw = "Henrik Frystyk Nielsen <henrikn@microsoft.com>",
            };

            BitbucketTarget expectedTarget = new BitbucketTarget
            {
                Message = "update\n",
                Operation = "commit",
                Hash = "8339b7affbd7c70bbacd0276f581d1ca44df0853",
                Author = expectedAuthor,
                Date = "somedate",
            };

            BitbucketParent expectedParent = new BitbucketParent
            {
                Operation = "commit",
                Hash = "b05057cd04921697c0f119ca40fe4a5afa481074",
            };
            expectedParent.Links.Add("html", new BitbucketLink { Reference = "https://bitbucket.org/henrikfrystyknielsen/henrikntest01/commits/b05057cd04921697c0f119ca40fe4a5afa481074" });
            expectedParent.Links.Add("self", new BitbucketLink { Reference = "https://api.bitbucket.org/2.0/repositories/henrikfrystyknielsen/henrikntest01/commit/b05057cd04921697c0f119ca40fe4a5afa481074" });

            expectedTarget.Parents.Add(expectedParent);
            expectedTarget.Links.Add("html", new BitbucketLink { Reference = "https://bitbucket.org/henrikfrystyknielsen/henrikntest01/commits/8339b7affbd7c70bbacd0276f581d1ca44df0853" });
            expectedTarget.Links.Add("self", new BitbucketLink { Reference = "https://api.bitbucket.org/2.0/repositories/henrikfrystyknielsen/henrikntest01/commit/8339b7affbd7c70bbacd0276f581d1ca44df0853" });

            // Act
            BitbucketTarget actualTarget = data["push"]["changes"][0]["new"]["target"].ToObject<BitbucketTarget>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expectedTarget);
            string actualJson = JsonConvert.SerializeObject(actualTarget);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
