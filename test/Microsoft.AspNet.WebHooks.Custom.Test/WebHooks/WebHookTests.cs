// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookTests
    {
        private WebHook _webHook;

        public WebHookTests()
        {
            _webHook = new WebHook();
        }

        [Fact]
        public void Id_Roundtrips()
        {
            PropertyAssert.Roundtrips(_webHook, w => w.Id, PropertySetter.NullDoesNotRoundtrip, roundtripValue: "你好世界");
        }

        [Fact]
        public void WebHookUri_Roundtrips()
        {
            PropertyAssert.Roundtrips(_webHook, w => w.WebHookUri, PropertySetter.NullRoundtrips, roundtripValue: "你好世界");
        }

        [Fact]
        public void Secret_Roundtrips()
        {
            PropertyAssert.Roundtrips(_webHook, w => w.Secret, PropertySetter.NullRoundtrips, roundtripValue: "你好世界");
        }

        [Fact]
        public void Description_Roundtrips()
        {
            PropertyAssert.Roundtrips(_webHook, w => w.Description, PropertySetter.NullRoundtrips, roundtripValue: "你好世界");
        }

        [Fact]
        public void IsPaused_Roundtrips()
        {
            PropertyAssert.Roundtrips(_webHook, w => w.IsPaused, defaultValue: false, roundtripValue: true);
        }

        [Theory]
        [MemberData("CaseInsensitiveDataSet", parameters: null, MemberType = typeof(TestDataSets))]
        public void Filters_AreCaseInsensitive(string input, string lookup)
        {
            _webHook.Filters.Add(input);
            bool actual = _webHook.Filters.Contains(lookup);
            Assert.True(actual);
        }

        [Theory]
        [MemberData("CaseInsensitiveDataSet", parameters: null, MemberType = typeof(TestDataSets))]
        public void Headers_AreCaseInsensitive(string input, string lookup)
        {
            _webHook.Headers.Add(input, "value");
            bool actual = _webHook.Headers.Keys.Contains(lookup);
            Assert.True(actual);
        }

        [Theory]
        [MemberData("CaseInsensitiveDataSet", parameters: null, MemberType = typeof(TestDataSets))]
        public void Properties_AreCaseInsensitive(string input, string lookup)
        {
            _webHook.Properties.Add(input, 1234);
            bool actual = _webHook.Properties.Keys.Contains(lookup);
            Assert.True(actual);
        }
    }
}
