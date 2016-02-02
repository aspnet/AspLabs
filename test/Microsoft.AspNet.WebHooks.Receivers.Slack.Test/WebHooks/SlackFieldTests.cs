// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class SlackFieldTests
    {
        [Fact]
        public void Title_Roundtrips()
        {
            SlackField field = new SlackField("MyTitle", "MyValue");
            PropertyAssert.Roundtrips(field, a => a.Title, PropertySetter.NullThrows, defaultValue: "MyTitle", roundtripValue: "你好世界");
        }

        [Fact]
        public void Value_Roundtrips()
        {
            SlackField field = new SlackField("MyTitle", "MyValue");
            PropertyAssert.Roundtrips(field, a => a.Value, PropertySetter.NullThrows, defaultValue: "MyValue", roundtripValue: "你好世界");
        }
    }
}
