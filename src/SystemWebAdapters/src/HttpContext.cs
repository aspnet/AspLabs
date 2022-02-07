// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web
{
    public class HttpContext
    {
        public static HttpContext Current => throw new NotImplementedException();

        public HttpRequest Request => throw new NotImplementedException();

#if NETCOREAPP
        public static implicit operator HttpContext(HttpContextCore context) => throw new NotImplementedException();
#endif
    }
}
