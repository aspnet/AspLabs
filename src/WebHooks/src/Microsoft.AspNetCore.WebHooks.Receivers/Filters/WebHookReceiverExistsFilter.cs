// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> that confirms the <see cref="Routing.WebHookReceiverNameConstraint"/> is
    /// configured and ran successfully for this request. Also confirms either <see cref="IWebHookVerifyCodeMetadata"/>
    /// is applicable or at least one <see cref="IWebHookReceiver"/> filter is configured to handle this request.
    /// The minimal configuration for a receiver without <see cref="IWebHookVerifyCodeMetadata"/> includes a
    /// <see cref="WebHookSecurityFilter"/> subclass to verify signatures or otherwise check secret keys.
    /// </summary>
    public class WebHookReceiverExistsFilter : IResourceFilter
    {
        private readonly ILogger _logger;
        private readonly WebHookMetadataProvider _metadataProvider;

        /// <summary>
        /// Instantiates a new <see cref="WebHookReceiverExistsFilter"/> instance.
        /// </summary>
        /// <param name="metadataProvider">
        /// The <see cref="WebHookMetadataProvider"/> service. Searched for applicable
        /// <see cref="IWebHookVerifyCodeMetadata"/> per-request.
        /// </param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebHookReceiverExistsFilter(
            WebHookMetadataProvider metadataProvider,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WebHookReceiverExistsFilter>();
            _metadataProvider = metadataProvider;
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all <see cref="WebHookReceiverExistsFilter"/>
        /// instances. The recommended filter sequence is
        /// <list type="number">
        /// <item>
        /// Confirm WebHooks configuration is set up correctly (in this filter).
        /// </item>
        /// <item>
        /// Confirm signature or <c>code</c> query parameter e.g. in <see cref="WebHookVerifyCodeFilter"/> or other
        /// <see cref="WebHookSecurityFilter"/> subclass.
        /// </item>
        /// <item>
        /// Confirm required headers, <see cref="RouteValueDictionary"/> entries and query parameters are provided (in
        /// <see cref="WebHookVerifyRequiredValueFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit GET or HEAD requests, if receiver supports either (in
        /// <see cref="WebHookGetHeadRequestFilter"/>).
        /// </item>
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in <see cref="WebHookVerifyBodyTypeFilter"/>).</item>
        /// <item>
        /// Map event name(s), if not done in <see cref="Routing.WebHookEventNameMapperConstraint"/> for this receiver
        /// (in <see cref="WebHookEventNameMapperFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetHeadRequestFilter"/> for this receiver (in
        /// <see cref="WebHookPingRequestFilter"/>).
        /// </item>
        /// </list>
        /// </summary>
        /// <value>Chosen to run WebHook filters early, prior to application-specific filters.</value>
        public static int Order => -500;

        /// <summary>
        /// <para>
        /// Confirms the <see cref="Routing.WebHookReceiverNameConstraint"/> is configured and ran successfully for
        /// this request. Also confirms at least one <see cref="IWebHookReceiver"/> filter is configured to handle this
        /// request.
        /// </para>
        /// <para>
        /// Logs an informational message when both confirmations succeed. If either confirmation fails, sets
        /// <see cref="ResourceExecutingContext.Result"/> to a <see cref="StatusCodeResult"/> with
        /// <see cref="StatusCodeResult.StatusCode"/> set to <see cref="StatusCodes.Status500InternalServerError"/>.
        /// </para>
        /// </summary>
        /// <param name="context">The <see cref="ResourceExecutingContext"/>.</param>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.RouteData.TryGetWebHookReceiverName(out var receiverName))
            {
                if (!context.RouteData.GetWebHookReceiverExists())
                {
                    _logger.LogError(
                        0,
                        "Unable to find WebHook routing constraints for the '{ReceiverName}' receiver. Add the " +
                        $"required configuration by calling a receiver-specific method that calls " +
                        $"'{typeof(IMvcBuilder)}.{nameof(WebHookMvcBuilderExtensions.AddWebHooks)}' or " +
                        $"'{nameof(IMvcCoreBuilder)}.{nameof(WebHookMvcCoreBuilderExtensions.AddWebHooks)}' in the " +
                        $"application startup code. For example, call '{nameof(IMvcCoreBuilder)}.AddGitHubWebHooks' " +
                        "to configure a minimal GitHub receiver.",
                        receiverName);

                    context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    return;
                }

                // Check for receiver-specific filters only for receivers that do _not_ use code verification.
                if (_metadataProvider.GetVerifyCodeMetadata(receiverName) == null)
                {
                    var found = false;
                    for (var i = 0; i < context.Filters.Count; i++)
                    {
                        var filter = context.Filters[i];
                        if (filter is IWebHookReceiver receiver && receiver.IsApplicable(receiverName))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        // This case is actually more likely a gap in the receiver-specific configuration method.
                        _logger.LogError(
                            1,
                            "Unable to find WebHook filters for the '{ReceiverName}' receiver. Add the required " +
                            "configuration by calling a receiver-specific method that calls " +
                            $"'{typeof(IMvcBuilder)}.{nameof(WebHookMvcBuilderExtensions.AddWebHooks)}' or " +
                            $"'{nameof(IMvcCoreBuilder)}.{nameof(WebHookMvcCoreBuilderExtensions.AddWebHooks)}' in " +
                            "the application startup code. For example, call " +
                            $"'{nameof(IMvcCoreBuilder)}.AddGitHubWebHooks' to configure a minimal GitHub receiver.",
                            receiverName);

                        context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                        return;
                    }
                }
            }
            else
            {
                // Routing not configured at all (wrong template) but the request reached this action.
                _logger.LogError(
                    2,
                    "Unable to find WebHook routing information in the request. Add the required " +
                    "configuration by calling a receiver-specific method that calls " +
                    $"'{typeof(IMvcBuilder)}.{nameof(WebHookMvcBuilderExtensions.AddWebHooks)}' or " +
                    $"'{nameof(IMvcCoreBuilder)}.{nameof(WebHookMvcCoreBuilderExtensions.AddWebHooks)}' in the " +
                    $"application startup code. For example, call '{nameof(IMvcCoreBuilder)}.AddGitHubWebHooks' to " +
                    "configure a minimal GitHub receiver.");

                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            context.RouteData.TryGetWebHookReceiverId(out var id);
            _logger.LogInformation(
                3,
                "Processing incoming WebHook request with receiver '{ReceiverName}' and id '{Id}'.",
                receiverName,
                id);
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }
    }
}
