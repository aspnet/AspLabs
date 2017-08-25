// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="Attribute"/> indicating the associated action is a GitHub WebHook endpoint. Specifies whether
    /// the action <see cref="AcceptFormData"/>, optional <see cref="EventName"/>, and optional
    /// <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="Filters.WebHookReceiverExistsFilter"/> for
    /// the action.
    /// </para>
    /// <para>
    /// An example GitHub WebHook URI is
    /// '<c>https://&lt;host&gt;/api/webhooks/incoming/github/{id}</c>'. See
    /// <see href="https://developer.github.com/webhooks/"/> for additional details about GitHub WebHook requests.
    /// </para>
    /// </summary>
    public class GitHubWebHookAttribute : WebHookAttribute, IWebHookRequestMetadata, IWebHookEventSelectorMetadata
    {
        private string _eventName;

        /// <summary>
        /// <para>
        /// Instantiates a new <see cref="GitHubWebHookAttribute"/> indicating the associated action is a GitHub
        /// WebHook endpoint.
        /// </para>
        /// <para>The signature of the action should be:
        /// <code>
        /// Task{IActionResult} ActionName(string id, string @event, TData data)
        /// </code>
        /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests.
        /// </para>
        /// <para>This constructor should usually be used at most once in a WebHook application.</para>
        /// <para>
        /// The default route <see cref="Mvc.Routing.IRouteTemplateProvider.Name"/> is <see langword="null"/>.
        /// </para>
        /// </summary>
        public GitHubWebHookAttribute()
            : base(GitHubConstants.ReceiverName)
        {
        }

        /// <summary>
        /// Gets or sets an indication this action expects form data.
        /// </summary>
        /// <value>Defaults to <see langword="false"/>, indicating this action expects JSON data.</value>
        public bool AcceptFormData { get; set; }

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

        /// <inheritdoc />
        WebHookBodyType IWebHookRequestMetadata.BodyType => AcceptFormData ? WebHookBodyType.Form : WebHookBodyType.Json;

        /// <inheritdoc />
        public bool UseHttpContextModelBinder => false;
    }
}