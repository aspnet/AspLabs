// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.SessionState;


namespace System.Web
{
    public class HttpContext : IServiceProvider
    {
        public static HttpContext Current => throw new NotImplementedException();

        public HttpRequest Request => throw new NotImplementedException();

        public HttpResponse Response => throw new NotImplementedException();

        public IDictionary Items => throw new NotImplementedException();

        public HttpServerUtility Server => throw new NotImplementedException();

        public HttpSessionState Session => throw new NotImplementedException();

        public Cache Cache => throw new NotImplementedException();

        public IPrincipal User
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
