// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.TestUtilities;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class PusherNotificationTests
    {
        private PusherNotification _notification;

        public static TheoryData<string, long> CreatedAtData
        {
            get
            {
                return new TheoryData<string, long>
                {
                    { "{ }", 0 },
                    { "{ \"time_ms\": 68719476736 }", 68719476736 },
                };
            }
        }

        public static TheoryData<string, IDictionary<string, int>> EventsData
        {
            get
            {
                return new TheoryData<string, IDictionary<string, int>>
                {
                    { "{ \"events\": [ { \"name\": \"你好世界\" } ] }", new Dictionary<string, int> { { "你好世界", 1 } } },
                    { "{ \"events\": [ { \"name\": \"e1\" }, { \"name\": \"e2\" }, { \"name\": \"e3\" } ] }", new Dictionary<string, int> { { "e1", 1 }, { "e2", 1 }, { "e3", 1 } } },
                    { "{ \"events\": [ { \"name\": \"e1\" }, { \"name\": \"e1\" }, { \"name\": \"e1\" } ] }", new Dictionary<string, int> { { "e1", 3 } } },
                    { "{ \"events\": [ { \"name\": \"e1\" }, { \"name\": \"e2\" }, { \"name\": \"e1\" } ] }", new Dictionary<string, int> { { "e1", 2 }, { "e2", 1 } } },
                };
            }
        }

        [Theory]
        [MemberData("CreatedAtData")]
        public void CreatedAt_Roundtrips(string input, long expected)
        {
            // Arrange
            JObject events = JObject.Parse(input);

            // Act
            _notification = new PusherNotification(events);

            // Assert
            PropertyAssert.Roundtrips(_notification, c => c.CreatedAt, defaultValue: expected, roundtripValue: long.MaxValue);
        }

        [Theory]
        [MemberData("EventsData")]
        public void Events_InitializesCorrectly(string input, IDictionary<string, int> expected)
        {
            // Arrange
            JObject events = JObject.Parse(input);

            // Act
            _notification = new PusherNotification(events);

            // Assert
            foreach (KeyValuePair<string, int> e in expected)
            {
                Collection<JObject> actual = _notification.Events[e.Key];
                Assert.Equal(e.Value, actual.Count);
            }
        }
    }
}
