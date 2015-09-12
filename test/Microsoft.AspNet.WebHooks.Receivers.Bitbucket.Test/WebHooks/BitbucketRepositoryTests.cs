// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class BitbucketRepositoryTests
    {
        [Fact]
        public void BitbucketRepository_Roundtrips()
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

            BitbucketRepository expectedRepository = new BitbucketRepository
            {
                FullName = "henrikfrystyknielsen/henrikntest01",
                Name = "henrikntest01",
                IsPrivate = true,
                ItemType = "repository",
                RepositoryType = "git",
                RepositoryId = "{d9898aea-edda-4f50-8f5f-5a8bfc819ab8}",
                Owner = expectedUser
            };
            expectedRepository.Links.Add("html", new BitbucketLink { Reference = "https://bitbucket.org/henrikfrystyknielsen/henrikntest01" });
            expectedRepository.Links.Add("avatar", new BitbucketLink { Reference = "https://bitbucket.org/henrikfrystyknielsen/henrikntest01/avatar/16/" });
            expectedRepository.Links.Add("self", new BitbucketLink { Reference = "https://api.bitbucket.org/2.0/repositories/henrikfrystyknielsen/henrikntest01" });

            // Act
            BitbucketRepository actualRepository = data["repository"].ToObject<BitbucketRepository>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expectedRepository);
            string actualJson = JsonConvert.SerializeObject(actualRepository);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
