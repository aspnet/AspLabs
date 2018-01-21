// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.ModelBinding
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for <see cref="SalesforceNotifications"/> instances.
    /// </summary>
    public class SalesforceNotificationsModelBinderProvider : IModelBinderProvider
    {
        private readonly IModelBinder _bodyModelBinder;

        /// <summary>
        /// Instantiates a new <see cref="SalesforceNotificationsModelBinderProvider"/> instance.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="options">The <see cref="MvcOptions"/>.</param>
        /// <param name="readerFactory">The <see cref="IHttpRequestStreamReaderFactory"/>.</param>
        public SalesforceNotificationsModelBinderProvider(
            ILoggerFactory loggerFactory,
            MvcOptions options,
            IHttpRequestStreamReaderFactory readerFactory)
        {
            _bodyModelBinder = new BodyModelBinder(options.InputFormatters, readerFactory, loggerFactory, options);
        }

        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(SalesforceNotifications))
            {
                return new SalesforceNotificationsModelBinder(_bodyModelBinder);
            }

            return null;
        }
    }
}
