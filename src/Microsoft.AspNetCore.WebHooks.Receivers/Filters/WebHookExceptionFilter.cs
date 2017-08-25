// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IExceptionFilter"/> implementation that returns a structured response to WebHook requests when
    /// an <see cref="Exception"/> is thrown.
    /// </summary>
    public class WebHookExceptionFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new <see cref="WebHookExceptionFilter"/> instance.
        /// </summary>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebHookExceptionFilter(IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = loggerFactory.CreateLogger<WebHookExceptionFilter>();
        }

        /// <inheritdoc />
        public void OnException(ExceptionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Apply to all requests matching an action with the WebHook route template.
            if (context.RouteData.TryGetReceiverName(out var receiverName))
            {
                _logger.LogError(
                    0,
                    context.Exception,
                    "WebHook receiver '{ReceiverName}' could not process WebHook due to error.",
                    receiverName);

                var result = WebHookResultUtilities.CreateErrorResult(
                    context.Exception,
                    includeErrorDetail: _hostingEnvironment.IsDevelopment());
                context.Result = result;
            }
        }
    }
}
