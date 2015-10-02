// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    public class BuildFinishedPayloadTests
    {
        [Fact]
        public void BuildFinishedPayload_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.BuildFinishedMessage.json");
            BuildFinishedPayload expected = new BuildFinishedPayload
            {
                Result = "success",
                FeedIdentifier = "sample-feed",
                FeedUrl = new Uri("https://www.myget.org/F/sample-feed/"),
                Name = "SampleBuild",
                Branch = "master",
                BuildLogUrl = new Uri("https://www.myget.org/BuildSource/List/sample-feed#d510be3d-7803-43cc-8d15-e327ba999ba7"),
            };
            expected.Packages.Add(new Package
            {
                PackageType = "NuGet",
                PackageIdentifier = "GoogleAnalyticsTracker.Core",
                PackageVersion = "1.0.0-CI00002",
            });
            expected.Packages.Add(new Package
            {
                PackageType = "NuGet",
                PackageIdentifier = "GoogleAnalyticsTracker.MVC4",
                PackageVersion = "1.0.0-CI00002",
            });

            // Act
            BuildFinishedPayload actual = data["Payload"].ToObject<BuildFinishedPayload>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
