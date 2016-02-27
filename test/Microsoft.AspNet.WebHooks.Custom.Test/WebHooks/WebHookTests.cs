// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        public enum ValidationOutcome
        {
            Valid = 0,
            Required,
            Invalid
        }

        public static TheoryData<string, ValidationOutcome> WebHookSecretData
        {
            get
            {
                return new TheoryData<string, ValidationOutcome>
                {
                    { string.Empty, ValidationOutcome.Required },
                    { " ", ValidationOutcome.Required },
                    { "\r\n", ValidationOutcome.Required },
                    { new string('a', 31), ValidationOutcome.Invalid },
                    { new string('a', 65), ValidationOutcome.Invalid },
                    { new string('a', 32), ValidationOutcome.Valid },
                    { new string('a', 64), ValidationOutcome.Valid },
                    { "你好世界你好世界你好世界你好世界你好世界你好世界你好世界你好世界", ValidationOutcome.Valid },
                };
            }
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

        [Theory]
        [MemberData("WebHookSecretData")]
        public void Secret_Validates(string secret, ValidationOutcome expected)
        {
            // Arrange
            WebHook webHook = new WebHook { Secret = secret };
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(webHook) { MemberName = "Secret" };

            // Act
            bool actual = Validator.TryValidateProperty(webHook.Secret, context, validationResults);

            // Assert
            switch (expected)
            {
                case ValidationOutcome.Valid:
                    Assert.True(actual);
                    break;

                case ValidationOutcome.Required:
                    Assert.False(actual);
                    Assert.Equal("The Secret field is required.", validationResults.Single().ErrorMessage);
                    Assert.Equal("Secret", validationResults.Single().MemberNames.Single());
                    break;

                case ValidationOutcome.Invalid:
                    Assert.Equal("The WebHook secret key parameter must be between 32 and 64 characters long.", validationResults.Single().ErrorMessage);
                    Assert.Equal("Secret", validationResults.Single().MemberNames.Single());
                    break;
            }
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
