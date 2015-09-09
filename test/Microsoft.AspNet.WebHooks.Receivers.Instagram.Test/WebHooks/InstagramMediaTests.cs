// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class InstagramMediaTests
    {
        private InstagramMedia _media = new InstagramMedia();

        [Fact]
        public void Address_Roundtrips()
        {
            PropertyAssert.Roundtrips(_media, m => m.Address, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void Width_Roundtrips()
        {
            PropertyAssert.Roundtrips(_media, m => m.Width, defaultValue: 0, roundtripValue: 1024);
        }

        [Fact]
        public void Height_Roundtrips()
        {
            PropertyAssert.Roundtrips(_media, m => m.Height, defaultValue: 0, roundtripValue: 1024);
        }
    }
}
