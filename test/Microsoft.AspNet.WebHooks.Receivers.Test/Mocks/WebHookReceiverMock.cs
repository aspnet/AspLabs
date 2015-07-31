// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks.Mocks
{
    public class WebHookReceiverMock : WebHookReceiver
    {
        public static readonly string ReceiverName = "MockReceiver";

        private readonly HttpResponseMessage _response;
        private readonly Exception _exception;

        public WebHookReceiverMock()
        {
        }

        public WebHookReceiverMock(HttpResponseMessage response)
        {
            _response = response;
        }

        public WebHookReceiverMock(Exception exception)
        {
            _exception = exception;
        }

        public override IEnumerable<string> Names
        {
            get { return new string[] { ReceiverName }; }
        }

        public override Task<HttpResponseMessage> ReceiveAsync(string receiver, HttpRequestContext context, HttpRequestMessage request)
        {
            if (_response != null)
            {
                return Task.FromResult(_response);
            }
            if (_exception != null)
            {
                throw _exception;
            }
            return Task.FromResult(new HttpResponseMessage());
        }

        public new void EnsureSecureConnection(HttpRequestMessage request)
        {
            base.EnsureSecureConnection(request);
        }

        public new string GetWebHookSecret(HttpRequestMessage request, string setting, int minLength, int maxLength)
        {
            return base.GetWebHookSecret(request, setting, minLength, maxLength);
        }

        public new Task<JObject> ReadAsJsonAsync(HttpRequestMessage request)
        {
            return base.ReadAsJsonAsync(request);
        }

        public new Task<NameValueCollection> ReadAsFormDataAsync(HttpRequestMessage request)
        {
            return base.ReadAsFormDataAsync(request);
        }

        public new string GetRequestHeader(HttpRequestMessage request, string headerName)
        {
            return base.GetRequestHeader(request, headerName);
        }

        public new HttpResponseMessage CreateBadMethodResponse(HttpRequestMessage request)
        {
            return base.CreateBadMethodResponse(request);
        }

        public new HttpResponseMessage CreateBadSignatureResponse(HttpRequestMessage request, string signatureHeaderName)
        {
            return base.CreateBadSignatureResponse(request, signatureHeaderName);
        }

        public new Task<HttpResponseMessage> ExecuteWebHookAsync(string receiver, HttpRequestContext context, HttpRequestMessage request, IEnumerable<string> actions, object data)
        {
            return base.ExecuteWebHookAsync(receiver, context, request, actions, data);
        }
    }
}
