// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Microsoft.AspNetCore.WebHooks
{
    // ??? Need IApiResponseMetadataProvider? Old world controller had [ApiExplorerSettings(IgnoreApi = true)].
    /// <summary>
    /// <para>
    /// An <see cref="Attribute"/> indicating the associated action is a Slack WebHook endpoint. Specifies the optional
    /// <see cref="WebHookAttribute.Id"/>.  Also adds a <see cref="Filters.WebHookReceiverExistsFilter"/> for the
    /// action and delegates its <see cref="IResultFilter"/> implementation to a <see cref="ProducesAttribute"/>,
    /// indicating the action produces JSON-formatted responses.
    /// </para>
    /// <para>The signature of the action should be:
    /// <code>
    /// Task{TResult} ActionName(string id, string @event, TData data)
    /// </code>
    /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
    /// <see cref="Http.IFormCollection"/> or <see cref="System.Collections.Specialized.NameValueCollection"/>.
    /// <c>TResult</c> may be <see cref="SlackResponse"/>, <see cref="SlackSlashResponse"/>, or an
    /// <see cref="IActionResult"/> implementation.
    /// </para>
    /// <para>
    /// The '<c>MS_WebHookReceiverSecret_Slack</c>' configuration value contains Slack shared-private security tokens.
    /// </para>
    /// <para>
    /// An example Slack WebHook URI is '<c>https://&lt;host&gt;/api/webhooks/incoming/slack/{id}</c>'.
    /// See <see href="https://api.slack.com/custom-integrations/outgoing-webhooks"/> for additional details about
    /// Slack WebHook requests.
    /// </para>
    /// </summary>
    public class SlackWebHookAttribute : WebHookAttribute, IResultFilter, IApiResponseMetadataProvider
    {
        private static readonly ProducesAttribute Produces = new ProducesAttribute("application/json");

        /// <summary>
        /// <para>
        /// Instantiates a new <see cref="SlackWebHookAttribute"/> indicating the associated action is a
        /// Slack WebHook endpoint.
        /// </para>
        /// <para>This constructor should usually be used at most once in a WebHook application.</para>
        /// <para>
        /// The default route <see cref="Mvc.Routing.IRouteTemplateProvider.Name"/> is <see langword="null"/>.
        /// </para>
        /// </summary>
        public SlackWebHookAttribute()
            : base(SlackConstants.ReceiverName)
        {
        }

        /// <inheritdoc />
        Type IApiResponseMetadataProvider.Type => Produces.Type;

        /// <inheritdoc />
        int IApiResponseMetadataProvider.StatusCode => Produces.StatusCode;

        /// <inheritdoc />
        void IApiResponseMetadataProvider.SetContentTypes(MediaTypeCollection contentTypes)
            => Produces.SetContentTypes(contentTypes);

        /// <inheritdoc />
        void IResultFilter.OnResultExecuting(ResultExecutingContext context) => Produces.OnResultExecuting(context);

        /// <inheritdoc />
        void IResultFilter.OnResultExecuted(ResultExecutedContext context) => Produces.OnResultExecuted(context);
    }
}
