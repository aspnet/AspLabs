// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class SlackAttachmentTests
    {
        [Fact]
        public void Text_Roundtrips()
        {
            SlackAttachment att = new SlackAttachment("Some text", "MyFallback");
            PropertyAssert.Roundtrips(att, a => a.Text, PropertySetter.NullThrows, defaultValue: "Some text", roundtripValue: "你好世界");
        }

        [Fact]
        public void Fallback_Roundtrips()
        {
            SlackAttachment att = new SlackAttachment("Some text", "MyFallback");
            PropertyAssert.Roundtrips(att, a => a.Fallback, PropertySetter.NullThrows, defaultValue: "MyFallback", roundtripValue: "你好世界");
        }
    }
}
