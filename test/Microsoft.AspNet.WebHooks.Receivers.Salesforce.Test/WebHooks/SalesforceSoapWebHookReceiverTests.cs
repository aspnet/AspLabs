// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class SalesforceSoapWebHookReceiverTests
    {
        private const string TestReceiver = "Test";
        private const string TestSecret = "123456789012345";

        private HttpConfiguration _config;
        private SettingsDictionary _settings;
        private HttpRequestContext _context;
        private Mock<SalesforceSoapWebHookReceiver> _receiverMock;
        private HttpRequestMessage _postRequest;

        public SalesforceSoapWebHookReceiverTests()
        {
            _settings = new SettingsDictionary();
            _settings["MS_WebHookReceiverSecret_SalesforceSoap"] = TestSecret;

            _config = HttpConfigurationMock.Create(new Dictionary<Type, object> { { typeof(SettingsDictionary), _settings } });
            _context = new HttpRequestContext { Configuration = _config };

            _receiverMock = new Mock<SalesforceSoapWebHookReceiver> { CallBase = true };

            _postRequest = new HttpRequestMessage(HttpMethod.Post, "https://some.ssl.host");
            _postRequest.SetRequestContext(_context);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotUsingHttps()
        {
            // Arrange
            _postRequest.RequestUri = new Uri("http://some.no.ssl.host");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'SalesforceSoapWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotXml()
        {
            // Arrange
            _postRequest.Content = new StringContent("{ }", Encoding.UTF8, "application/json");

            // Act
            HttpResponseException ex = await Assert.ThrowsAsync<HttpResponseException>(() => _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest));

            // Assert
            HttpError error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as XML.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasInvalidToken()
        {
            // Arrange
            _postRequest.Content = new StringContent("<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'><soapenv:Body><notifications xmlns='http://soap.sforce.com/2005/09/outbound'><OrganizationId>Invalid</OrganizationId></notifications></soapenv:Body></soapenv:Envelope>", Encoding.UTF8, "application/xml");

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The 'OrganizationId' parameter provided in the HTTP request did not match the expected value.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoAction()
        {
            // Arrange
            _postRequest.Content = new StringContent("<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'><soapenv:Body><notifications xmlns='http://soap.sforce.com/2005/09/outbound'><OrganizationId>123456789012345</OrganizationId></notifications></soapenv:Body></soapenv:Envelope>", Encoding.UTF8, "application/xml");

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            HttpError error = await actual.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The HTTP request body did not contain a required 'ActionId' property.", error.Message);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest()
        {
            // Arrange
            WebHooksConfig.Initialize(_config);
            List<string> actions = new List<string> { "abcde" };
            _postRequest.Content = new StringContent("<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'><soapenv:Body><notifications xmlns='http://soap.sforce.com/2005/09/outbound'><OrganizationId>123456789012345</OrganizationId><ActionId>abcde</ActionId></notifications></soapenv:Body></soapenv:Envelope>", Encoding.UTF8, "application/xml");
            _receiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", TestReceiver, _context, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, _postRequest);

            // Assert
            _receiverMock.Verify();
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("PATCH")]
        [InlineData("PUT")]
        [InlineData("OPTIONS")]
        public async Task ReceiveAsync_ReturnsError_IfInvalidMethod(string method)
        {
            // Arrange
            HttpRequestMessage req = new HttpRequestMessage { Method = new HttpMethod(method) };
            req.SetRequestContext(_context);

            // Act
            HttpResponseMessage actual = await _receiverMock.Object.ReceiveAsync(TestReceiver, _context, req);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, actual.StatusCode);
            _receiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestReceiver, _context, req, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("123456789012345", "123456789012345")]
        [InlineData("123456789012345678", "123456789012345")]
        [InlineData("你好世界你好世界你好世界你好世", "你好世界你好世界你好世界你好世")]
        [InlineData("你好世界你好世界你好世界你好世界你好", "你好世界你好世界你好世界你好世")]
        public void GetShortOrgId_HandlesIds(string id, string expected)
        {
            // Act
            string actual = SalesforceSoapWebHookReceiver.GetShortOrgId(id);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
