// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class InstagramUserTests
    {
        private InstagramUser _user = new InstagramUser();

        [Fact]
        public void Id_Roundtrips()
        {
            PropertyAssert.Roundtrips(_user, u => u.Id, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void UserName_Roundtrips()
        {
            PropertyAssert.Roundtrips(_user, u => u.UserName, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void FullName_Roundtrips()
        {
            PropertyAssert.Roundtrips(_user, u => u.FullName, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void ProfilePicture_Roundtrips()
        {
            PropertyAssert.Roundtrips(_user, u => u.ProfilePicture, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }
    }
}
