// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="Attribute"/> indicating the associated action is a WebHook endpoint for all configured receivers.
    /// Specifies the expected <see cref="BodyType"/> (if any), optional <see cref="EventName"/>, and optional
    /// <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="WebHookReceiverExistsFilter"/> and a
    /// <see cref="ModelStateInvalidFilter"/> (unless <see cref="ApiBehaviorOptions.SuppressModelStateInvalidFilter"/>
    /// is <see langword="true"/>) for the action.
    /// </para>
    /// <para>
    /// The signature of the action should be:
    /// <code>
    /// Task{IActionResult} ActionName(string receiverName, string id, string[] events, TData data)
    /// </code>
    /// or the subset of parameters required. <c>TData</c> must be compatible with expected requests and
    /// <see cref="BodyType"/> (if that is non-<see langword="null"/>).
    /// </para>
    /// <para>
    /// An example WebHook URI is '<c>https://{host}/api/webhooks/incoming/{receiver name}/{id}</c>' or
    /// '<c>https://{host}/api/webhooks/incoming/{receiver name}/{id}?code=94c0c780e49a5c72972590571fd8</c>'.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Associating this <see cref="Attribute"/> and <see cref="Mvc.ConsumesAttribute"/> with the same action is not
    /// recommended, especially if a configured receiver handles ping requests i.e. the receiver metadata includes an
    /// <see cref="IWebHookPingRequestMetadata"/> implementation. Using the two attributes together may lead to routing
    /// issues.
    /// </para>
    /// <para>
    /// If the application enables CORS in general (see the <c>Microsoft.AspNetCore.Cors</c> package), apply
    /// <c>DisableCorsAttribute</c> to this action. If the application depends on the
    /// <c>Microsoft.AspNetCore.Mvc.ViewFeatures</c> package, apply <c>IgnoreAntiforgeryTokenAttribute</c> to this
    /// action.
    /// </para>
    /// <para>
    /// <see cref="GeneralWebHookAttribute"/> should be used at most once per <see cref="WebHookAttribute.Id"/> and
    /// <see cref="EventName"/> in a WebHook application.
    /// </para>
    /// </remarks>
    public class GeneralWebHookAttribute : WebHookAttribute, IWebHookBodyTypeMetadata, IWebHookEventSelectorMetadata
    {
        private string _eventName;

        /// <summary>
        /// Instantiates a new <see cref="GeneralWebHookAttribute"/> instance indicating the associated action is a
        /// WebHook endpoint for all configured receivers. Sets <see cref="BodyType"/> to <see langword="null"/>,
        /// indicating a <c>data</c> parameter is not expected and, if such a parameter exists, it requires no
        /// additional <see cref="Mvc.ModelBinding.BindingInfo"/>.
        /// </summary>
        public GeneralWebHookAttribute()
            : base()
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="GeneralWebHookAttribute"/> instance indicating the associated action is a
        /// WebHook endpoint for all configured receivers. Sets <see cref="BodyType"/> to <paramref name="bodyType"/>.
        /// </summary>
        /// <param name="bodyType">The <see cref="WebHookBodyType"/> this action expects</param>
        /// <remarks>
        /// Use <see cref="WebHookAttribute.Id"/> values to control routing to multiple actions with
        /// <see cref="GeneralWebHookAttribute"/> and distinct non-<see langword="null"/> <see cref="BodyType"/>
        /// settings. <c>data</c> parameters may otherwise not be correctly model bound.
        /// </remarks>
        public GeneralWebHookAttribute(WebHookBodyType bodyType)
            : base()
        {
            if (!Enum.IsDefined(typeof(WebHookBodyType), bodyType))
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.General_InvalidEnumValue,
                    typeof(WebHookBodyType),
                    bodyType);
                throw new ArgumentException(message, nameof(bodyType));
            }

            BodyType = bodyType;
        }

        /// <inheritdoc />
        public WebHookBodyType? BodyType { get; }

        /// <summary>
        /// Gets or sets the name of the event the associated controller action accepts.
        /// </summary>
        /// <value>Default value is <see langword="null"/>, indicating this action accepts all events.</value>
        public string EventName
        {
            get
            {
                return _eventName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(Resources.General_ArgumentCannotBeNullOrEmpty, nameof(value));
                }

                _eventName = value;
            }
        }
    }
}
