// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class SlackResponseTests
    {
        private const string Text = "This is a test";

        private SlackResponse _response;

        public SlackResponseTests()
        {
            _response = new SlackResponse(Text);
        }

        [Fact]
        public void Text_Roundtrips()
        {
            PropertyAssert.Roundtrips(_response, c => c.Text, PropertySetter.NullThrows, defaultValue: Text, roundtripValue: "你好世界");
        }
    }
}
