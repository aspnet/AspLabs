// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class SalesforceSoapWebHookReceiverTests : WebHookReceiverTestsBase<SalesforceSoapWebHookReceiver>
    {
        private const string TestContent = "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'><soapenv:Body><notifications xmlns='http://soap.sforce.com/2005/09/outbound'><OrganizationId>123456789012345</OrganizationId><ActionId>abcde</ActionId></notifications></soapenv:Body></soapenv:Envelope>";
        private const string TestId = "";
        private const string TestSecret = "123456789012345";
        private const string ReceiverConfigName = "SalesforceSoap";

        private static readonly XNamespace Soap = "http://schemas.xmlsoap.org/soap/envelope/";

        private HttpRequestMessage _postRequest;

        [Fact]
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            IWebHookReceiver rec = new SalesforceSoapWebHookReceiver();
            var expected = "sfsoap";

            // Act
            var actual1 = rec.Name;
            var actual2 = SalesforceSoapWebHookReceiver.ReceiverName;

            // Assert
            Assert.Equal(expected, actual1);
            Assert.Equal(actual1, actual2);
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotUsingHttps()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.RequestUri = new Uri("http://some.no.ssl.host");

            // Act
            var ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook receiver 'SalesforceSoapWebHookReceiverProxy' requires HTTPS in order to be secure. Please register a WebHook URI of type 'https'.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_Throws_IfPostIsNotXml()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("{ }", Encoding.UTF8, "application/json");

            // Act
            var ex = await Assert.ThrowsAsync<HttpResponseException>(() => ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest));

            // Assert
            var error = await ex.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain an entity body formatted as XML.", error.Message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasInvalidToken()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'><soapenv:Body><notifications xmlns='http://soap.sforce.com/2005/09/outbound'><OrganizationId>Invalid</OrganizationId></notifications></soapenv:Body></soapenv:Envelope>", Encoding.UTF8, "application/xml");

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            var error = await actual.Content.ReadAsAsync<XElement>();
            var message = error
                .Element(Soap + "Body")
                .Element(Soap + "Fault")
                .Element("faultstring").Value;
            Assert.Equal("The 'OrganizationId' parameter provided in the HTTP request did not match the expected value.", message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Fact]
        public async Task ReceiveAsync_ReturnsError_IfPostHasNoAction()
        {
            // Arrange
            Initialize(TestSecret);
            _postRequest.Content = new StringContent("<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'><soapenv:Body><notifications xmlns='http://soap.sforce.com/2005/09/outbound'><OrganizationId>123456789012345</OrganizationId></notifications></soapenv:Body></soapenv:Envelope>", Encoding.UTF8, "application/xml");

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, _postRequest);

            // Assert
            var error = await actual.Content.ReadAsAsync<XElement>();
            var message = error
                .Element(Soap + "Body")
                .Element(Soap + "Fault")
                .Element("faultstring").Value;
            Assert.Equal("The HTTP request body did not contain a required 'ActionId' property.", message);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, _postRequest, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task ReceiveAsync_Succeeds_IfValidPostRequest(string id)
        {
            // Arrange
            Initialize(GetConfigValue(id, TestSecret));
            var actions = new List<string> { "abcde" };
            ReceiverMock.Protected()
                .Setup<Task<HttpResponseMessage>>("ExecuteWebHookAsync", id, RequestContext, _postRequest, actions, ItExpr.IsAny<object>())
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable();

            // Act
            await ReceiverMock.Object.ReceiveAsync(id, RequestContext, _postRequest);

            // Assert
            ReceiverMock.Verify();
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
            Initialize(TestSecret);
            var req = new HttpRequestMessage { Method = new HttpMethod(method) };
            req.SetRequestContext(RequestContext);

            // Act
            var actual = await ReceiverMock.Object.ReceiveAsync(TestId, RequestContext, req);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, actual.StatusCode);
            ReceiverMock.Protected()
                .Verify<Task<HttpResponseMessage>>("ExecuteWebHookAsync", Times.Never(), TestId, RequestContext, req, ItExpr.IsAny<IEnumerable<string>>(), ItExpr.IsAny<object>());
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
            var actual = SalesforceSoapWebHookReceiver.GetShortOrgId(id);

            // Assert
            Assert.Equal(expected, actual);
        }

        public override void Initialize(string config)
        {
            base.Initialize(ReceiverConfigName, config);

            _postRequest = new HttpRequestMessage(HttpMethod.Post, "https://some.ssl.host");
            _postRequest.SetRequestContext(RequestContext);
            _postRequest.Content = new StringContent(TestContent, Encoding.UTF8, "application/xml");
        }
    }
}
