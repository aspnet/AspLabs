// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class InstagramSubscriptionTests
    {
        private InstagramSubscription _sub = new InstagramSubscription();

        [Fact]
        public void Id_Roundtrips()
        {
            PropertyAssert.Roundtrips(_sub, s => s.Id, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void Object_Roundtrips()
        {
            PropertyAssert.Roundtrips(_sub, s => s.Object, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void ObjectId_Roundtrips()
        {
            PropertyAssert.Roundtrips(_sub, s => s.ObjectId, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }

        [Fact]
        public void Callback_Roundtrips()
        {
            PropertyAssert.Roundtrips(_sub, s => s.Callback, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }
    }
}
