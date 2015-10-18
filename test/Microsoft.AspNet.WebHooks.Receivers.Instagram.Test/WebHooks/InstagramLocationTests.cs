// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class InstagramLocationTests
    {
        private InstagramLocation _location = new InstagramLocation();

        [Fact]
        public void Id_Roundtrips()
        {
            PropertyAssert.Roundtrips(_location, l => l.Id, defaultValue: 0, roundtripValue: 1024);
        }

        [Fact]
        public void Name_Roundtrips()
        {
            PropertyAssert.Roundtrips(_location, l => l.Name, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void Latitude_Roundtrips()
        {
            PropertyAssert.Roundtrips(_location, l => l.Latitude, defaultValue: 0, roundtripValue: 1.2345);
        }

        [Fact]
        public void Longitude_Roundtrips()
        {
            PropertyAssert.Roundtrips(_location, l => l.Longitude, defaultValue: 0, roundtripValue: 1.2345);
        }
    }
}
