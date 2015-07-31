// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookFilterTests
    {
        private WebHookFilter _filter;

        public WebHookFilterTests()
        {
            _filter = new WebHookFilter();
        }

        [Fact]
        public void Name_Roundtrips()
        {
            PropertyAssert.Roundtrips(_filter, f => f.Name, PropertySetter.NullRoundtrips, roundtripValue: "你好世界");
        }

        [Fact]
        public void Description_Roundtrips()
        {
            PropertyAssert.Roundtrips(_filter, f => f.Description, PropertySetter.NullRoundtrips, roundtripValue: "你好世界");
        }
    }
}
