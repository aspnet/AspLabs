// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Base for <see cref="Attribute"/>s indicating the associated action is a WebHook endpoint. Specifies the
    /// required (in most cases) <see cref="ReceiverName"/> and optional <see cref="Id"/>. Also adds a
    /// <see cref="WebHookReceiverExistsFilter"/> and a <see cref="ModelStateInvalidFilter"/> (unless
    /// <see cref="ApiBehaviorOptions.SuppressModelStateInvalidFilter"/> is <see langword="true"/>) for the action.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the application enables CORS in general (see the <c>Microsoft.AspNetCore.Cors</c> package), apply
    /// <c>DisableCorsAttribute</c> to this action. If the application depends on the
    /// <c>Microsoft.AspNetCore.Mvc.ViewFeatures</c> package, apply <c>IgnoreAntiforgeryTokenAttribute</c> to this
    /// action.
    /// </para>
    /// <para>
    /// Subclasses of <see cref="WebHookAttribute"/> should be used at most once per <see cref="ReceiverName"/> and
    /// <see cref="Id"/> in a WebHook application. Some subclasses support additional uniqueness constraints.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class WebHookAttribute : Attribute, IAllowAnonymous, IFilterFactory
    {
        private string _id;

        internal WebHookAttribute()
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="WebHookAttribute"/> indicating the associated action is a WebHook endpoint
        /// for the given <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        protected WebHookAttribute(string receiverName)
        {
            if (string.IsNullOrEmpty(receiverName))
            {
                throw new ArgumentException(Resources.General_ArgumentCannotBeNullOrEmpty, nameof(receiverName));
            }

            ReceiverName = receiverName;
        }

        /// <summary>
        /// Gets the name of an available <see cref="IWebHookReceiver"/>.
        /// </summary>
        public string ReceiverName { get; }

        /// <summary>
        /// Gets or sets the id of the configuration this action accepts.
        /// </summary>
        /// <value>
        /// Default value is <see langword="null"/>, indicating this action accepts all requests for this
        /// <see cref="ReceiverName"/>.
        /// </value>
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(Resources.General_ArgumentCannotBeNullOrEmpty, nameof(value));
                }

                _id = value;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Allow the <see cref="WebHookReceiverExistsFilter"/> service's registration to determine its lifetime.
        /// </remarks>
        bool IFilterFactory.IsReusable => false;

        /// <inheritdoc />
        IFilterMetadata IFilterFactory.CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var filter = serviceProvider.GetRequiredService<WebHookReceiverExistsFilter>();
            return filter;
        }
    }
}
