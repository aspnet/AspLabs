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

        public static TheoryData<string[], bool> AnyActionData
        {
            get
            {
                return new TheoryData<string[], bool>
                {
                    { null, false },
                    { new[] { string.Empty, "a1" }, false },
                    { new[] { "你", "好", "世", "界" }, false },
                    { new[] { "你好世界" }, false },
                    { new[] { "1action" }, false },
                    { new[] { "a1", "action" }, true },
                    { new[] { "a1", "action", "a2" }, true },
                    { new[] { "a1", "ACTION", "a2" }, true },
                    { new[] { "你", "你好", "好" }, true },
                    { new[] { "你", "世界", "好" }, true },
                };
            }
        }

        [Theory]
        [MemberData(nameof(ActionData))]
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

        [Theory]
        [MemberData(nameof(AnyActionData))]
        public void MatchesAnyAction_DetectsIndividualMatches(string[] actions, bool expected)
        {
            // Act
            bool actual = _webHook.MatchesAnyAction(actions);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MatchesAnyAction_DetectsOnlyWildcard()
        {
            // Arrange
            _webHook.Filters.Clear();
            _webHook.Filters.Add(WildcardWebHookFilterProvider.Name);

            // Act
            bool actual = _webHook.MatchesAnyAction(new[] { "something" });

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void MatchesAnyAction_DetectsWildcard()
        {
            // Arrange
            _webHook.Filters.Clear();
            _webHook.Filters.Add("other");
            _webHook.Filters.Add(WildcardWebHookFilterProvider.Name);

            // Act
            bool actual = _webHook.MatchesAnyAction(new[] { "something" });

            // Assert
            Assert.True(actual);
        }
    }
}
