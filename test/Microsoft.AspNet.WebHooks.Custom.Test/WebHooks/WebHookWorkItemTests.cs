// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookWorkItemTests
    {
        private readonly WebHookWorkItem _workItem = new WebHookWorkItem();

        [Fact]
        public void Id_Roundtrips()
        {
            _workItem.Id = "id";
            PropertyAssert.Roundtrips(_workItem, s => s.Id, PropertySetter.NullRoundtrips, defaultValue: "id", roundtripValue: "value");
        }

        [Fact]
        public void WebHook_Roundtrips()
        {
            WebHook defaultValue = new WebHook();
            _workItem.Hook = defaultValue;
            PropertyAssert.Roundtrips(_workItem, s => s.Hook, PropertySetter.NullRoundtrips, defaultValue: defaultValue, roundtripValue: new WebHook());
        }

        [Fact]
        public void Offset_Roundtrips()
        {
            _workItem.Offset = 1024;
            PropertyAssert.Roundtrips(_workItem, s => s.Offset, defaultValue: 1024, roundtripValue: 2048);
        }

        [Fact]
        public void Actions_Roundtrips()
        {
            _workItem.Actions.Add("some filter");
            Assert.True(_workItem.Actions.Contains("some filter"));
        }

        [Fact]
        public void Data_Roundtrips()
        {
            _workItem.Data.Add("name", "value");
            Assert.True(_workItem.Data.ContainsKey("name"));
        }
    }
}
