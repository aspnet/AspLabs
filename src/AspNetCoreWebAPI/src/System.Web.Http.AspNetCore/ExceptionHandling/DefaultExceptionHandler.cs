// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace System.Web.Http.AspNetCore.ExceptionHandling
{
    public sealed class DefaultExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            return Task.CompletedTask;
        }

        private static void Handle(ExceptionHandlerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ExceptionContext exceptionContext = context.ExceptionContext;
            Contract.Assert(exceptionContext != null);

            HttpRequestMessage request = exceptionContext.Request;

            context.Result = new ResponseMessageResult(request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                exceptionContext.Exception));
        }
    }
}
