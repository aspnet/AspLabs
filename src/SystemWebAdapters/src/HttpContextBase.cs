// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Security.Principal;
using System.Diagnostics.CodeAnalysis;
using System.Web.Adapters;

namespace System.Web
{
    public class HttpContextBase : IServiceProvider
    {
        protected HttpContextBase()
        {
        }

        public virtual HttpRequestBase Request => throw new NotImplementedException();

        public virtual HttpResponseBase Response => throw new NotImplementedException();

        public virtual IDictionary Items => throw new NotImplementedException();

        public virtual IPrincipal User
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public HttpServerUtilityBase Server => throw new NotImplementedException();

        public virtual object? GetService(Type serviceType) => throw new NotImplementedException();

        [return: NotNullIfNotNull("context")]
        public static implicit operator HttpContextBase?(HttpContextCore? context) => context?.GetAdapterBase();
    }
}
