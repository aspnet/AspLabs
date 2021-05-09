// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.Web.Http.AspNetCore
{
    internal class AspNetCoreHttpRequestContext : HttpRequestContext
    {
        private readonly HttpContext _context;
        private readonly HttpRequestMessage _request;

        private X509Certificate2 _clientCertificate;
        private bool _clientCertificateSet;

        private bool _includeErrorDetail;
        private bool _includeErrorDetailSet;

        private bool _isLocal;
        private bool _isLocalSet;

        private UrlHelper _url;
        private bool _urlSet;

        private string _virtualPathRoot;
        private bool _virtualPathRootSet;

        public AspNetCoreHttpRequestContext(HttpContext context, HttpRequestMessage request)
        {
            Contract.Assert(context != null);
            Contract.Assert(context.Request != null);
            Contract.Assert(request != null);
            _context = context;
            _request = request;
        }

        public HttpContext Context
        {
            get { return _context; }
        }

        public HttpRequestMessage Request
        {
            get { return _request; }
        }

        //// Configuration and RouteData are not overridden; these values are provided by later points in the
        ////  pipeline (HttpServer and HttpRoutingDispatcher)

        public override X509Certificate2 ClientCertificate
        {
            get
            {
                if (!_clientCertificateSet)
                {
                    var feature = _context.Features.Get<ITlsConnectionFeature>();
                    _clientCertificate = feature?.ClientCertificate;
                    _clientCertificateSet = true;
                }

                return _clientCertificate;
            }
            set
            {
                _clientCertificate = value;
                _clientCertificateSet = true;
            }
        }

        public override bool IncludeErrorDetail
        {
            get
            {
                if (!_includeErrorDetailSet)
                {
                    HttpConfiguration configuration = Configuration;
                    IncludeErrorDetailPolicy includeErrorDetailPolicy;

                    if (configuration != null)
                    {
                        includeErrorDetailPolicy = configuration.IncludeErrorDetailPolicy;
                    }
                    else
                    {
                        includeErrorDetailPolicy = IncludeErrorDetailPolicy.Default;
                    }

                    bool value;

                    switch (includeErrorDetailPolicy)
                    {
                        case IncludeErrorDetailPolicy.Default:
                        case IncludeErrorDetailPolicy.LocalOnly:
                            value = IsLocal;
                            break;

                        case IncludeErrorDetailPolicy.Always:
                            value = true;
                            break;

                        case IncludeErrorDetailPolicy.Never:
                        default:
                            value = false;
                            break;
                    }

                    _includeErrorDetail = value;
                    _includeErrorDetailSet = true;
                }

                return _includeErrorDetail;
            }
            set
            {
                _includeErrorDetail = value;
                _includeErrorDetailSet = true;
            }
        }

        public override bool IsLocal
        {
            get
            {
                if (!_isLocalSet)
                {
                    var hostEnvironment = Context.RequestServices.GetRequiredService<IHostEnvironment>();
                    _isLocal = hostEnvironment.IsDevelopment();
                }

                _isLocalSet = true;
                return _isLocal;
            }
            set
            {
                _isLocal = value;
                _isLocalSet = true;
            }
        }

        public override IPrincipal Principal
        {
            get
            {
                return _context.User;
            }
            set
            {
                if (!(value is ClaimsPrincipal claimsPrincipal))
                {
                    throw new NotSupportedException("Prinicipal must be an instance of ClaimsPrincipal.");
                }

                _context.User = claimsPrincipal;
                Thread.CurrentPrincipal = value;
            }
        }

        public override UrlHelper Url
        {
            get
            {
                if (!_urlSet)
                {
                    _url = new UrlHelper(_request);
                    _urlSet = true;
                }

                return _url;
            }
            set
            {
                _url = value;
                _urlSet = true;
            }
        }

        public override string VirtualPathRoot
        {
            get
            {
                if (!_virtualPathRootSet)
                {
                    // Set the virtual path root for link resolution and link generation to work
                    // OWIN spec requires request path base to be either the empty string or start with "/"
                    string requestPathBase = _context.Request.PathBase.Value;
                    _virtualPathRoot = string.IsNullOrEmpty(requestPathBase) ? "/" : requestPathBase;
                    _virtualPathRootSet = true;
                }

                return _virtualPathRoot;
            }
            set
            {
                _virtualPathRoot = value;
                _virtualPathRootSet = true;
            }
        }
    }
}
