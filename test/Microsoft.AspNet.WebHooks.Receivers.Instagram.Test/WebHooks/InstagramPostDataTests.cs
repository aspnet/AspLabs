// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PostTests", Justification = "This is the right name.")]
    public class InstagramPostDataTests
    {
        private DateTime _testTime = new DateTime(1970, 1, 1, 1, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void InstagramPost_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.PostMessage.json");
            InstagramPostData expectedPost = new InstagramPostData
            {
                Id = "1077852647225486162_194771465",
                Link = new Uri("https://instagram.com/p/3sFga24da/"),
                CreatedTime = _testTime,
                MediaType = "video",
                Location = new InstagramLocation
                {
                    Id = 225623404,
                    Name = "Equinox At The High Line",
                    Latitude = 40.7437744,
                    Longitude = -74.0068283
                },
                Images = new InstagramImages
                {
                    Thumbnail = new InstagramMedia
                    {
                        Address = new Uri("https://scontent.cdninstagram.com/Thumbnail.jpg"),
                        Width = 150,
                        Height = 150,
                    },
                    LowResolution = new InstagramMedia
                    {
                        Address = new Uri("https://scontent.cdninstagram.com/LowRes.jpg"),
                        Width = 320,
                        Height = 320
                    },
                    StandardResolution = new InstagramMedia
                    {
                        Address = new Uri("https://scontent.cdninstagram.com/StdRes.jpg"),
                        Width = 640,
                        Height = 640
                    }
                },
                Videos = new InstagramVideos
                {
                    LowBandwidth = new InstagramMedia
                    {
                        Address = new Uri("https://scontent.cdninstagram.com/LowBw.mp4"),
                        Width = 480,
                        Height = 270,
                    },
                    LowResolution = new InstagramMedia
                    {
                        Address = new Uri("https://scontent.cdninstagram.com/LowRes.mp4"),
                        Width = 480,
                        Height = 270
                    },
                    StandardResolution = new InstagramMedia
                    {
                        Address = new Uri("https://scontent.cdninstagram.com/StdRes.mp4"),
                        Width = 640,
                        Height = 360
                    }
                },
                Caption = new InstagramCaption
                {
                    CreatedTime = _testTime,
                    Id = "1077852735856538330",
                    Text = "#handstand #drill #compilation",
                    From = new InstagramUser
                    {
                        UserName = "user",
                        ProfilePicture = new Uri("https://scontent.cdninstagram.com/userprofile.jpg"),
                        FullName = "Some User",
                        Id = "194771423"
                    }
                },
                User = new InstagramUser
                {
                    Id = "194771423",
                    FullName = "Some User",
                    UserName = "user",
                    ProfilePicture = new Uri("https://scontent.cdninstagram.com/userprofile.jpg"),
                }
            };
            expectedPost.Tags.Add("compilation");
            expectedPost.Tags.Add("handstand");
            expectedPost.Tags.Add("drill");

            // Act
            InstagramPostData actualPost = data.ToObject<InstagramPostData>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expectedPost);
            string actualJson = JsonConvert.SerializeObject(actualPost);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
