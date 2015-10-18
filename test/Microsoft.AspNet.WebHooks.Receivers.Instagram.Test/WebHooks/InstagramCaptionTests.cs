// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class InstagramCaptionTests
    {
        private InstagramCaption _caption = new InstagramCaption();

        [Fact]
        public void Id_Roundtrips()
        {
            PropertyAssert.Roundtrips(_caption, c => c.Id, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void CreatedTime_Roundtrips()
        {
            DateTime roundtrip = DateTime.UtcNow;
            PropertyAssert.Roundtrips(_caption, c => c.CreatedTime, defaultValue: DateTime.MinValue, roundtripValue: roundtrip);
        }

        [Fact]
        public void Text_Roundtrips()
        {
            PropertyAssert.Roundtrips(_caption, c => c.Text, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void From_Roundtrips()
        {
            InstagramUser roundtrip = new InstagramUser();
            PropertyAssert.Roundtrips(_caption, c => c.From, PropertySetter.NullRoundtrips, roundtripValue: roundtrip);
        }
    }
}
