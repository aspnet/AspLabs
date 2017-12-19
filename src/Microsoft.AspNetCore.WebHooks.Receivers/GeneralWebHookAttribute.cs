// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="Attribute"/> indicating the associated action is a WebHook endpoint for all enabled
    /// receivers. Specifies the expected <see cref="BodyType"/>, optional <see cref="EventName"/>, and optional
    /// <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="Filters.WebHookReceiverExistsFilter"/> for
    /// the action.
    /// </para>
    /// <para>The signature of the action should be:
    /// <code>
    /// Task{IActionResult} ActionName(string receiverName, string id, string[] events, TData data)
    /// </code>
    /// or the subset of parameters required. <c>TData</c> must be compatible with expected requests.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This attribute should usually be used at most once in a WebHook application.
    /// </remarks>
    public class GeneralWebHookAttribute : WebHookAttribute, IWebHookBodyTypeMetadata, IWebHookEventSelectorMetadata
    {
        private WebHookBodyType _bodyType = WebHookBodyType.Json;
        private string _eventName;

        /// <summary>
        /// Instantiates a new <see cref="GeneralWebHookAttribute"/> indicating the associated action is a WebHook
        /// endpoint for all enabled receivers.
        /// </summary>
        public GeneralWebHookAttribute()
            : base()
        {
        }

        /// <inheritdoc />
        /// <value>Default value is <see cref="WebHookBodyType.Json"/>.</value>
        public WebHookBodyType BodyType
        {
            get
            {
                return _bodyType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(WebHookBodyType), value))
                {
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.General_InvalidEnumValue,
                        nameof(WebHookBodyType),
                        value);
                    throw new ArgumentException(message, nameof(value));
                }

                _bodyType = value;
            }
        }

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
