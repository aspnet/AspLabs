// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Security.Principal;

namespace System.Web
{
    public class HttpContextBase : IServiceProvider
    {
        public virtual HttpRequestBase Request => throw new NotImplementedException();

        public virtual HttpResponseBase Response => throw new NotImplementedException();

        public virtual IDictionary Items => throw new NotImplementedException();

        public virtual IPrincipal User
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public HttpSessionStateBase Session => throw new NotImplementedException();

        public HttpServerUtilityBase Server => throw new NotImplementedException();

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
