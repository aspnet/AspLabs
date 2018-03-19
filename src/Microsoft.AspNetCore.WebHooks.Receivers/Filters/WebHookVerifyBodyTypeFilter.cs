// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> to allow only WebHook requests with a <c>Content-Type</c> matching the
    /// receiver's <see cref="IWebHookBodyTypeMetadataService.BodyType"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Done as an <see cref="IResourceFilter"/> implementation and not an
    /// <see cref="Mvc.ActionConstraints.IActionConstraintMetadata"/> because receivers do not dynamically vary their
    /// <see cref="IWebHookBodyTypeMetadataService.BodyType"/>s.
    /// </para>
    /// <para>
    /// Use <see cref="WebHookAttribute.Id"/> values to control routing to multiple actions with
    /// <see cref="GeneralWebHookAttribute"/> and distinct non-<see langword="null"/>
    /// <see cref="IWebHookBodyTypeMetadata.BodyType"/> settings.
    /// </para>
    /// </remarks>
    public class WebHookVerifyBodyTypeFilter : IResourceFilter, IOrderedFilter
    {
        private static readonly MediaTypeHeaderValue ApplicationAnyJsonMediaType
            = new MediaTypeHeaderValue("application/*+json").CopyAsReadOnly();
        private static readonly MediaTypeHeaderValue ApplicationAnyXmlMediaType
            = new MediaTypeHeaderValue("application/*+xml").CopyAsReadOnly();
        private static readonly MediaTypeHeaderValue ApplicationJsonMediaType
            = new MediaTypeHeaderValue("application/json").CopyAsReadOnly();
        private static readonly MediaTypeHeaderValue ApplicationXmlMediaType
            = new MediaTypeHeaderValue("application/xml").CopyAsReadOnly();
        private static readonly MediaTypeHeaderValue TextJsonMediaType
            = new MediaTypeHeaderValue("text/json").CopyAsReadOnly();
        private static readonly MediaTypeHeaderValue TextXmlMediaType
            = new MediaTypeHeaderValue("text/xml").CopyAsReadOnly();

        private readonly IReadOnlyList<IWebHookBodyTypeMetadataService> _allBodyTypeMetadata;
        private readonly IWebHookBodyTypeMetadataService _receiverBodyTypeMetadata;
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifyBodyTypeFilter"/> instance to verify the given
        /// <paramref name="receiverBodyTypeMetadata"/>.
        /// </summary>
        /// <param name="receiverBodyTypeMetadata">
        /// The receiver's <see cref="IWebHookBodyTypeMetadataService"/>.
        /// </param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebHookVerifyBodyTypeFilter(
            IWebHookBodyTypeMetadataService receiverBodyTypeMetadata,
            ILoggerFactory loggerFactory)
        {
            if (receiverBodyTypeMetadata == null)
            {
                throw new ArgumentNullException(nameof(receiverBodyTypeMetadata));
            }
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _receiverBodyTypeMetadata = receiverBodyTypeMetadata;
            _logger = loggerFactory.CreateLogger<WebHookVerifyBodyTypeFilter>();
        }

        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifyBodyTypeFilter"/> instance to verify the receiver's
        /// <see cref="IWebHookBodyTypeMetadataService.BodyType"/>. That <see cref="WebHookBodyType"/> value is found
        /// in <paramref name="allBodyTypeMetadata"/>).
        /// </summary>
        /// <param name="allBodyTypeMetadata">
        /// The collection of <see cref="IWebHookBodyTypeMetadataService"/> services. Searched for applicable metadata
        /// per-request.
        /// </param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <remarks>
        /// This overload is intended for use with <see cref="GeneralWebHookAttribute"/>.
        /// </remarks>
        public WebHookVerifyBodyTypeFilter(
            IReadOnlyList<IWebHookBodyTypeMetadataService> allBodyTypeMetadata,
            ILoggerFactory loggerFactory)
        {
            if (allBodyTypeMetadata == null)
            {
                throw new ArgumentNullException(nameof(allBodyTypeMetadata));
            }
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _allBodyTypeMetadata = allBodyTypeMetadata;
            _logger = loggerFactory.CreateLogger<WebHookVerifyBodyTypeFilter>();
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> used in all <see cref="WebHookVerifyBodyTypeFilter"/>
        /// instances. The recommended filter sequence is
        /// <list type="number">
        /// <item>
        /// Confirm signature or <c>code</c> query parameter e.g. in <see cref="WebHookVerifyCodeFilter"/> or other
        /// <see cref="WebHookSecurityFilter"/> subclass.
        /// </item>
        /// <item>
        /// Confirm required headers, <see cref="RouteValueDictionary"/> entries and query parameters are provided
        /// (in <see cref="WebHookVerifyRequiredValueFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit GET or HEAD requests, if receiver supports either (in
        /// <see cref="WebHookGetHeadRequestFilter"/>).
        /// </item>
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in this filter).</item>
        /// <item>
        /// Map event name(s), if not done in <see cref="Routing.WebHookEventMapperConstraint"/> for this receiver (in
        /// <see cref="WebHookEventMapperFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetHeadRequestFilter"/> for this receiver (in
        /// <see cref="WebHookPingRequestFilter"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookVerifyMethodFilter.Order + 10;

        /// <inheritdoc />
        int IOrderedFilter.Order => Order;

        /// <inheritdoc />
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeData = context.RouteData;
            if (!routeData.TryGetWebHookReceiverName(out var receiverName))
            {
                return;
            }

            var receiverBodyTypeMetadata = _receiverBodyTypeMetadata ??
                // WebHookReceiverExistsConstraint confirms the IWebHookBodyTypeMetadataService implementation exists.
                _allBodyTypeMetadata.First(metadata => metadata.IsApplicable(receiverName));

            var request = context.HttpContext.Request;
            var contentType = request.GetTypedHeaders().ContentType;
            switch (receiverBodyTypeMetadata.BodyType)
            {
                case WebHookBodyType.Form:
                    if (!request.HasFormContentType)
                    {
                        _logger.LogWarning(
                            0,
                            "The '{ReceiverName}' WebHook receiver does not support content type '{ContentType}'. " +
                            "The WebHook request must contain an entity body formatted as HTML form URL-encoded data.",
                            receiverName,
                            contentType);
                        var message = string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.VerifyBody_NoFormData,
                            receiverName,
                            contentType);
                        context.Result = new BadRequestObjectResult(message)
                        {
                            StatusCode = StatusCodes.Status415UnsupportedMediaType
                        };
                    }
                    break;

                case WebHookBodyType.Json:
                    if (!IsJson(contentType))
                    {
                        _logger.LogWarning(
                            1,
                            "The '{ReceiverName}' WebHook receiver does not support content type '{ContentType}'. " +
                            "The WebHook request must contain an entity body formatted as JSON.",
                            receiverName,
                            contentType);
                        var message = string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.VerifyBody_NoJson,
                            receiverName,
                            contentType);
                        context.Result = new BadRequestObjectResult(message)
                        {
                            StatusCode = StatusCodes.Status415UnsupportedMediaType
                        };
                    }
                    break;

                case WebHookBodyType.Xml:
                    if (!IsXml(contentType))
                    {
                        _logger.LogWarning(
                            2,
                            "The '{ReceiverName}' WebHook receiver does not support content type '{ContentType}'. " +
                            "The WebHook request must contain an entity body formatted as XML.",
                            receiverName,
                            contentType);
                        var message = string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.VerifyBody_NoXml,
                            receiverName,
                            contentType);
                        context.Result = new BadRequestObjectResult(message)
                        {
                            StatusCode = StatusCodes.Status415UnsupportedMediaType
                        };
                    }
                    break;

                default:
                    {
                        var message = string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.General_InvalidEnumValue,
                            typeof(WebHookBodyType),
                            receiverBodyTypeMetadata.BodyType);
                        throw new InvalidOperationException(message);
                    }
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }

        /// <summary>
        /// Determines whether the specified <paramref name="contentType"/> is <c>application/json</c>,
        /// <c>text/json</c> or <c>application/xyz+json</c>. The term <c>xyz</c> can for example be <c>hal</c> or some
        /// other JSON-derived media type.
        /// </summary>
        /// <param name="contentType">The request's <see cref="MediaTypeHeaderValue"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="contentType"/> indicates the request has JSON content;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        protected static bool IsJson(MediaTypeHeaderValue contentType)
        {
            if (contentType == null)
            {
                return false;
            }

            return contentType.IsSubsetOf(ApplicationJsonMediaType) ||
                contentType.IsSubsetOf(ApplicationAnyJsonMediaType) ||
                contentType.IsSubsetOf(TextJsonMediaType);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="contentType"/> is <c>application/xml</c>, <c>text/xml</c>
        /// or <c>application/xyz+xml</c>. The term <c>xyz</c> can for example be <c>rdf</c> or some other XML-derived
        /// media type.
        /// </summary>
        /// <param name="contentType">The request's <see cref="MediaTypeHeaderValue"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="contentType"/> indicates the request has XML content;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        protected static bool IsXml(MediaTypeHeaderValue contentType)
        {
            if (contentType == null)
            {
                return false;
            }

            return contentType.IsSubsetOf(ApplicationXmlMediaType) ||
                contentType.IsSubsetOf(ApplicationAnyXmlMediaType) ||
                contentType.IsSubsetOf(TextXmlMediaType);
        }
    }
}
