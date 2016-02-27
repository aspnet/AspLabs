// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookSenderTests : IDisposable
    {
        private const string TestUser = "TestUser";
        private const string SerializedWebHook = "{\r\n  \"Id\": \"1234567890\",\r\n  \"Attempt\": 1,\r\n  \"Properties\": {\r\n    \"p1\": \"pv1\"\r\n  },\r\n  \"Notifications\": [\r\n    {\r\n      \"Action\": \"a1\",\r\n      \"d1\": \"dv1\"\r\n    },\r\n    {\r\n      \"Action\": \"a1\",\r\n      \"d2\": \"http://localhost/\"\r\n    }\r\n  ]\r\n}";
        private const string WebHookSignature = "sha256=69941A3C522CE0B52B5F08BD23309D4356422FEFF99A3398062A7C015B9FD48D";

        private readonly Mock<ILogger> _loggerMock;

        private WebHookSenderMock _sender;

        public WebHookSenderTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void CreateWebHookRequest_CreatesExpectedRequest()
        {
            // Arrange
            WebHookWorkItem workItem = CreateWorkItem();
            workItem.WebHook.Headers.Add("Content-Language", "da");
            _sender = new WebHookSenderMock(_loggerMock.Object);

            // Act
            HttpRequestMessage actual = _sender.CreateWebHookRequest(workItem);

            // Assert
            Assert.Equal(HttpMethod.Post, actual.Method);
            Assert.Equal(workItem.WebHook.WebHookUri, actual.RequestUri);

            IEnumerable<string> headers;
            actual.Headers.TryGetValues("h1", out headers);
            Assert.Equal("hv1", headers.Single());

            actual.Headers.TryGetValues("ms-signature", out headers);
            Assert.Equal(WebHookSignature, headers.Single());

            Assert.Equal("da", actual.Content.Headers.ContentLanguage.Single());
            Assert.Equal("application/json; charset=utf-8", actual.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public void CreateWebHookRequestBody_CreatesExpectedBody()
        {
            // Arrange
            WebHookWorkItem workItem = CreateWorkItem();
            _sender = new WebHookSenderMock(_loggerMock.Object);

            // Act
            JObject actual = _sender.CreateWebHookRequestBody(workItem);

            // Assert
            Assert.Equal(SerializedWebHook, actual.ToString());
        }

        [Fact]
        public void SignWebHookRequest_HandlesNullWebHook()
        {
            WebHookWorkItem workItem = CreateWorkItem();
            HttpRequestMessage request = new HttpRequestMessage();
            _sender = new WebHookSenderMock(_loggerMock.Object);
            JObject body = _sender.CreateWebHookRequestBody(workItem);
            workItem.WebHook = null;

            // Act
            ArgumentException ex = Assert.Throws<ArgumentException>(() => _sender.SignWebHookRequest(workItem, request, body));

            // Assert
            Assert.StartsWith("Invalid 'WebHookSenderMock' instance: 'WebHook' cannot be null.", ex.Message);
        }

        [Fact]
        public async Task SignWebHookRequest_SignsBodyCorrectly()
        {
            // Arrange
            WebHookWorkItem workItem = CreateWorkItem();
            HttpRequestMessage request = new HttpRequestMessage();
            _sender = new WebHookSenderMock(_loggerMock.Object);
            JObject body = _sender.CreateWebHookRequestBody(workItem);

            // Act
            _sender.SignWebHookRequest(workItem, request, body);

            // Assert
            IEnumerable<string> signature;
            request.Headers.TryGetValues("ms-signature", out signature);
            Assert.Equal(WebHookSignature, signature.Single());

            string requestBody = await request.Content.ReadAsStringAsync();
            Assert.Equal(SerializedWebHook, requestBody);

            Assert.Equal("application/json; charset=utf-8", request.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public void Dispose_Succeeds()
        {
            // Arrange
            WebHookSenderMock s = new WebHookSenderMock(_loggerMock.Object);

            // Act
            s.Dispose();
            s.Dispose();
        }

        public void Dispose()
        {
            if (_sender != null)
            {
                _sender.Dispose();
            }
        }

        private static WebHookWorkItem CreateWorkItem()
        {
            WebHook webHook = WebHookManagerTests.CreateWebHook();
            NotificationDictionary notification1 = new NotificationDictionary("a1", new { d1 = "dv1" });
            NotificationDictionary notification2 = new NotificationDictionary("a1", new Dictionary<string, object> { { "d2", new Uri("http://localhost") } });
            WebHookWorkItem workItem = new WebHookWorkItem(webHook, new[] { notification1, notification2 })
            {
                Id = "1234567890",
            };
            return workItem;
        }

        private class WebHookSenderMock : WebHookSender
        {
            public WebHookSenderMock(ILogger logger)
                : base(logger)
            {
            }

            public new void SignWebHookRequest(WebHookWorkItem workItem, HttpRequestMessage request, JObject body)
            {
                base.SignWebHookRequest(workItem, request, body);
            }

            public new HttpRequestMessage CreateWebHookRequest(WebHookWorkItem workItem)
            {
                return base.CreateWebHookRequest(workItem);
            }

            public new JObject CreateWebHookRequestBody(WebHookWorkItem workItem)
            {
                return base.CreateWebHookRequestBody(workItem);
            }

            public override Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems)
            {
                throw new NotImplementedException();
            }
        }
    }
}
