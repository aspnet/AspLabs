// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookHandlerTests
    {
        private WebHookHandler _handler;

        public WebHookHandlerTests()
        {
            _handler = new TestWebHookHandler();
        }

        [Fact]
        public void Order_Roundtrips()
        {
            PropertyAssert.Roundtrips(_handler, h => h.Order, defaultValue: WebHookHandler.DefaultOrder, roundtripValue: 100);
        }

        [Fact]
        public void Receiver_Roundtrips()
        {
            PropertyAssert.Roundtrips(_handler, h => h.Receiver, PropertySetter.NullRoundtrips, roundtripValue: "你好世界");
        }

        private class TestWebHookHandler : WebHookHandler
        {
            public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
