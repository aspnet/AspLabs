// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web
{
    public class HttpContextBase : IServiceProvider
    {
        protected HttpContextBase()
        {
        }

        public virtual HttpRequestBase Request => throw new NotImplementedException();

        public virtual HttpResponseBase Response => throw new NotImplementedException();

        public virtual object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
