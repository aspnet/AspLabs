// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.TestUtilities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookTests
    {
        private JsonSerializerSettings _settings = new JsonSerializerSettings();
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
            Uri roundTrip = new Uri("http://localhost/path");
            PropertyAssert.Roundtrips(_webHook, w => w.WebHookUri, PropertySetter.NullRoundtrips, roundtripValue: roundTrip);
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

        [Fact]
        public void Serializes_AsExpected()
        {
            // Arrange
            WebHook webHook = new WebHook
            {
                Description = "你好",
                Id = "1234567890",
                IsPaused = true,
                Secret = "世界",
                WebHookUri = new Uri("http://localhost/path"),
            };
            webHook.Filters.Add("*");
            webHook.Headers.Add(new KeyValuePair<string, string>("k1", "v1"));
            webHook.Properties.Add(new KeyValuePair<string, object>("p1", 1234));

            // Act/Assert
            SerializationAssert.SerializesAs<WebHook>(webHook, _settings, "{\"Id\":\"1234567890\",\"WebHookUri\":\"http://localhost/path\",\"Secret\":\"世界\",\"Description\":\"你好\",\"IsPaused\":true,\"Filters\":[\"*\"],\"Headers\":{\"k1\":\"v1\"},\"Properties\":{\"p1\":1234}}");
        }
    }
}
