// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestUtilities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookQueueContextTests
    {
        private const string TestReceiver = "TestReceiver";
        private const string TestId = "12345";

        private string[] _actions = new string[] { "a1", "a2" };
        private object _data = new { Prop1 = "Hello", Prop2 = "World" };
        private JsonSerializerSettings _settings = new JsonSerializerSettings();
        private WebHookQueueContext _queueContext;

        public WebHookQueueContextTests()
        {
            WebHookHandlerContext context = new WebHookHandlerContext(_actions) { Id = TestId, Data = _data };
            _queueContext = new WebHookQueueContext(TestReceiver, context);
        }

        [Fact]
        public void Receiver_Roundtrips()
        {
            PropertyAssert.Roundtrips(_queueContext, c => c.Receiver, PropertySetter.NullRoundtrips, defaultValue: TestReceiver, roundtripValue: "你好世界");
        }

        [Fact]
        public void Id_Roundtrips()
        {
            PropertyAssert.Roundtrips(_queueContext, c => c.Id, PropertySetter.NullRoundtrips, defaultValue: TestId, roundtripValue: "你好世界");
        }

        [Fact]
        public void Data_Roundtrips()
        {
            PropertyAssert.Roundtrips(_queueContext, c => c.Data, PropertySetter.NullRoundtrips, defaultValue: _data, roundtripValue: new object());
        }

        [Fact]
        public void Actions_Roundtrips()
        {
            Assert.Equal((IEnumerable<string>)_actions, (IEnumerable<string>)_queueContext.Actions);
        }

        [Fact]
        public void Serializes_AsExpected()
        {
            SerializationAssert.SerializesAs(_queueContext, _settings, "{\"Receiver\":\"TestReceiver\",\"Id\":\"12345\",\"Actions\":[\"a1\",\"a2\"],\"Data\":{\"Prop1\":\"Hello\",\"Prop2\":\"World\"}}");
        }

        [Fact]
        public void Serialization_Roundtrips()
        {
            // Arrange
            _queueContext.Data = "data";

            // Act
            string ser = JsonConvert.SerializeObject(_queueContext, _settings);
            WebHookQueueContext actual = JsonConvert.DeserializeObject<WebHookQueueContext>(ser, _settings);

            // Assert
            Assert.Equal(_queueContext.Receiver, actual.Receiver);
            Assert.Equal(_queueContext.Actions, actual.Actions);
            Assert.Equal(_queueContext.Data, actual.Data);
        }
    }
}
