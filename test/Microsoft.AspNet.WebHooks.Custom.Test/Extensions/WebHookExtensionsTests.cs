// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNet.WebHooks.Extensions
{
    public class WebHookExtensionsTests
    {
        private WebHook _webHook;

        public WebHookExtensionsTests()
        {
            _webHook = new WebHook();
            _webHook.Filters.Add("action");
            _webHook.Filters.Add("你好");
            _webHook.Filters.Add("世界");
        }

        public static TheoryData<string, bool> ActionData
        {
            get
            {
                return new TheoryData<string, bool>
                {
                    { null, false },
                    { string.Empty, false },
                    { "你好世界", false },
                    { "1action", false },
                    { "action", true },
                    { "ACTION", true },
                    { "你好", true },
                    { "世界", true },
                };
            }
        }

        [Theory]
        [MemberData("ActionData")]
        public void MatchesAction_DetectsIndividualMatches(string action, bool expected)
        {
            // Act
            bool actual = _webHook.MatchesAction(action);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MatchesAction_DetectsOnlyWildcard()
        {
            // Arrange
            _webHook.Filters.Clear();
            _webHook.Filters.Add(WildcardWebHookFilterProvider.Name);

            // Act
            bool actual = _webHook.MatchesAction("something");

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void MatchesAction_DetectsWildcard()
        {
            // Arrange
            _webHook.Filters.Clear();
            _webHook.Filters.Add("other");
            _webHook.Filters.Add(WildcardWebHookFilterProvider.Name);

            // Act
            bool actual = _webHook.MatchesAction("something");

            // Assert
            Assert.True(actual);
        }
    }
}
