// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Security.Principal;

namespace System.Web
{
    public class HttpContextWrapper : HttpContextBase
    {
        private readonly HttpContext _context;

        private HttpRequestBase? _request;
        private HttpResponseBase? _response;

        public HttpContextWrapper(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            _context = httpContext;
        }

        public override IDictionary Items => _context.Items;

        public override HttpRequestBase Request
        {
            get
            {
                if (_request is null)
                {
                    _request = new HttpRequestWrapper(_context.Request);
                }

                return _request;
            }
        }

        public override HttpResponseBase Response
        {
            get
            {
                if (_response is null)
                {
                    _response = new HttpResponseWrapper(_context.Response);
                }

                return _response;
            }
        }

        public override IPrincipal User
        {
            get => _context.User;
            set => _context.User = value;
        }
    }
}
