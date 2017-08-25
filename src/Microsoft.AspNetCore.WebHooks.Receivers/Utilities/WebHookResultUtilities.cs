// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebHooks.Properties;

namespace Microsoft.AspNetCore.WebHooks.Utilities
{
    /// <summary>
    /// Utility methods returning <see cref="ObjectResult"/>s for error responses with consistent keys.
    /// </summary>
    /// <seealso cref="WebHookErrorKeys"/>
    public static class WebHookResultUtilities
    {
        /// <summary>
        /// Returns a new <see cref="ObjectResult"/> containing the given <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The error message to include in the response.</param>
        /// <returns>
        /// An <see cref="ObjectResult"/> that when executed will produce a response containing the
        /// <paramref name="message"/>. The response will by default have the Bad Request (400) status code.
        /// </returns>
        public static ObjectResult CreateErrorResult(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var error = new SerializableError
            {
                { WebHookErrorKeys.MessageKey, message },
            };

            return new BadRequestObjectResult(error);
        }

        /// <summary>
        /// Returns a new <see cref="ObjectResult"/> containing the error messages found i
        /// <paramref name="modelState"/>.
        /// </summary>
        /// <param name="modelState">The <see cref="ModelStateEntry"/> to search for error messages.</param>
        /// <returns>
        /// A new <see cref="ObjectResult"/> containing the error messages found in <paramref name="modelState"/>.
        /// </returns>
        /// <remarks>
        /// Should not be called if <see cref="ModelStateDictionary.IsValid"/> is <see langword="true"/>.
        /// </remarks>
        public static ObjectResult CreateErrorResult(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            return new BadRequestObjectResult(modelState);
        }

        /// <summary>
        /// Returns a new <see cref="ObjectResult"/> for the given <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to use for error information.</param>
        /// <returns>
        /// An <see cref="ObjectResult"/> that when executed will produce a response containing a generic error
        /// message. The response will by default have the Bad Request (400) status code.
        /// </returns>
        /// <remarks><paramref name="exception"/> is ignored but useful when debugging.</remarks>
        public static ObjectResult CreateErrorResult(Exception exception)
            => CreateErrorResult(exception, includeErrorDetail: false);

        /// <summary>
        /// Returns a new <see cref="ObjectResult"/> for the given <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to use for error information.</param>
        /// <param name="includeErrorDetail">
        /// <see langword="true"/> to include the <see cref="Exception"/> information in the error;
        /// <see langword="false"/> otherwise.
        /// </param>
        /// <returns>
        /// An <see cref="ObjectResult"/> that when executed will produce a response containing a generic error
        /// message and, if <paramref name="includeErrorDetail"/> is <see langword="true"/>, details about the
        /// <paramref name="exception"/>. The response will by default have the Bad Request (400) status code.
        /// </returns>
        /// <remarks>
        /// <paramref name="exception"/> is ignored when <paramref name="includeErrorDetail"/> is
        /// <see langword="false"/>, but useful when debugging even in that case.
        /// </remarks>
        public static ObjectResult CreateErrorResult(Exception exception, bool includeErrorDetail)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var error = new SerializableError
            {
                { WebHookErrorKeys.MessageKey, Resources.ResultUtilities_GenericError },
            };

            if (includeErrorDetail)
            {
                error.Add(WebHookErrorKeys.ExceptionMessageKey, exception.Message);
                error.Add(WebHookErrorKeys.ExceptionTypeKey, exception.GetType().FullName);
                error.Add(WebHookErrorKeys.StackTraceKey, exception.StackTrace);
                if (exception.InnerException != null)
                {
                    error.Add(
                        WebHookErrorKeys.InnerExceptionKey,
                        CreateErrorResult(exception.InnerException, includeErrorDetail));
                }
            }

            return new BadRequestObjectResult(error);
        }
    }
}