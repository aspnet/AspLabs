// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="Attribute"/> indicating the associated action is a Bitbucket WebHook endpoint.
    /// Specifies the optional <see cref="WebHookAttribute.Id"/>. Also adds a
    /// <see cref="Filters.WebHookReceiverExistsFilter"/> for the action.
    /// </para>
    /// <para>
    /// An example Bitbucket WebHook URI is
    /// '<c>https://&lt;host&gt;/api/webhooks/incoming/bitbucket/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
    /// See <see href="https://confluence.atlassian.com/bitbucket/manage-webhooks-735643732.html"/> for additional
    /// details about Bitbucket WebHook requests.
    /// </para>
    /// </summary>
    public class BitbucketWebHookAttribute : WebHookAttribute, IWebHookEventSelectorMetadata
    {
        private string _eventName;

        /// <summary>
        /// <para>
        /// Instantiates a new <see cref="BitbucketWebHookAttribute"/> indicating the associated action is a Bitbucket
        /// WebHook endpoint.
        /// </para>
        /// <para>The signature of the action should be:
        /// <code>
        /// Task{IActionResult} ActionName(string id, string @event, string webHookId, TData data)
        /// </code>
        /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
        /// <see cref="Newtonsoft.Json.Linq.JObject"/>.
        /// </para>
        /// <para>This constructor should usually be used at most once in a WebHook application.</para>
        /// </summary>
        public BitbucketWebHookAttribute()
            : base(BitbucketConstants.ReceiverName)
        {
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
