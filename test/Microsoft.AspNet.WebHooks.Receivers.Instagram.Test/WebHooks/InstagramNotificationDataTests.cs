// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class InstagramNotificationDataTests
    {
        private InstagramNotificationData _notificationData = new InstagramNotificationData();

        [Fact]
        public void MediaId_Roundtrips()
        {
            PropertyAssert.Roundtrips(_notificationData, n => n.MediaId, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }
    }
}
