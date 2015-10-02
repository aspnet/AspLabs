// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    public class PackagePushedPayloadTests
    {
        [Fact]
        public void PackagePushedPayload_Roundtrips()
        {
            // Arrange
            JObject data = EmbeddedResource.ReadAsJObject("Microsoft.AspNet.WebHooks.Messages.PackagePushedMessage.json");
            PackagePushedPayload expected = new PackagePushedPayload
            {
                PackageType = "NuGet",
                PackageIdentifier = "GoogleAnalyticsTracker.Simple",
                PackageVersion = "1.0.0-CI00002",
                PackageDetailsUrl = new Uri("https://www.myget.org/feed/sample-feed/package/GoogleAnalyticsTracker.Simple/1.0.0-CI00002"),
                PackageDownloadUrl = new Uri("https://www.myget.org/F/sample-feed/api/v2/package/GoogleAnalyticsTracker.Simple/1.0.0-CI00002"),
                PackageMetadata = new PackageMetadata
                {
                    IconUrl = new Uri("/Content/images/packageDefaultIcon.png", UriKind.Relative),
                    Size = 5542,
                    Authors = "Maarten Balliauw",
                    Description = "GoogleAnalyticsTracker was created to have a means of tracking specific URL's directly from C#. For example, it enables you to log API calls to Google Analytics.",
                    LicenseUrl = new Uri("http://github.com/maartenba/GoogleAnalyticsTracker/blob/master/LICENSE.md"),
                    LicenseNames = "MS-PL",
                    ProjectUrl = new Uri("http://github.com/maartenba/GoogleAnalyticsTracker"),
                    Tags = "google analytics ga mvc api rest client tracker stats statistics",
                },
                TargetPackageSourceName = "Other Feed",
                TargetPackageSourceUrl = new Uri("https://www.myget.org/F/other-feed/"),
                FeedIdentifier = "sample-feed",
                FeedUrl = new Uri("https://www.myget.org/F/sample-feed/")
            };
            expected.PackageMetadata.Dependencies.Add(new Package
            {
                PackageIdentifier = "GoogleAnalyticsTracker.Core",
                PackageVersion = "(? 2.0.5364.25176)",
                TargetFramework = ".NETFramework,Version=v4.0.0.0"
            });

            // Act
            PackagePushedPayload actual = data["Payload"].ToObject<PackagePushedPayload>();

            // Assert
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
