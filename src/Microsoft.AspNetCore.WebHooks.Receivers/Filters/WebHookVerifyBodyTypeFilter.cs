// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> to allow only WebHook requests with a <c>Content-Type</c> matching
    /// <see cref="IWebHookBodyTypeMetadata.BodyType"/>.
    /// </summary>
    /// <remarks>
    /// Done as an <see cref="IResourceFilter"/> implementation and not an
    /// <see cref="Mvc.ActionConstraints.IActionConstraintMetadata"/> because receivers do not dynamically vary their
    /// <see cref="IWebHookBodyTypeMetadata"/>. Use distinct <see cref="WebHookAttribute.Id"/> values if different
    /// configurations are needed for one receiver and the receiver's <see cref="WebHookAttribute"/> implements
    /// <see cref="IWebHookBodyTypeMetadata"/>.
    /// </remarks>
    public class WebHookVerifyBodyTypeFilter : IResourceFilter, IOrderedFilter
    {
        private static readonly MediaTypeHeaderValue ApplicationJsonMediaType
            = new MediaTypeHeaderValue("application/json");
        private static readonly MediaTypeHeaderValue ApplicationXmlMediaType
            = new MediaTypeHeaderValue("application/xml");
        private static readonly MediaTypeHeaderValue TextJsonMediaType = new MediaTypeHeaderValue("text/json");
        private static readonly MediaTypeHeaderValue TextXmlMediaType = new MediaTypeHeaderValue("text/xml");

        private readonly IWebHookBodyTypeMetadata _bodyTypeMetadata;
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifyMethodFilter"/> instance.
        /// </summary>
        /// <param name="bodyTypeMetadata">The <see cref="IWebHookBodyTypeMetadata"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebHookVerifyBodyTypeFilter(IWebHookBodyTypeMetadata bodyTypeMetadata, ILoggerFactory loggerFactory)
        {
            if (bodyTypeMetadata == null)
            {
                throw new ArgumentNullException(nameof(bodyTypeMetadata));
            }
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _bodyTypeMetadata = bodyTypeMetadata;
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
        /// Confirm required headers, <see cref="AspNetCore.Routing.RouteValueDictionary"/> entries and query
        /// parameters are provided (in <see cref="WebHookVerifyRequiredValueFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit GET or HEAD requests, if receiver supports either (in <see cref="WebHookGetRequestFilter"/>).
        /// </item>
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in this filter).</item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetRequestFilter"/> for this receiver (in
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

            var request = context.HttpContext.Request;
            switch (_bodyTypeMetadata.BodyType)
            {
                case WebHookBodyType.Form:
                    if (!request.HasFormContentType)
                    {
                        context.Result = CreateUnsupportedMediaTypeResult(Resources.VerifyBody_NoFormData);
                    }
                    break;

                case WebHookBodyType.Json:
                    if (!IsJson(request))
                    {
                        context.Result = CreateUnsupportedMediaTypeResult(Resources.VerifyBody_NoJson);
                    }
                    break;

                case WebHookBodyType.Xml:
                    if (!IsXml(request))
                    {
                        context.Result = CreateUnsupportedMediaTypeResult(Resources.VerifyBody_NoXml);
                    }
                    break;

                default:
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.General_InvalidEnumValue,
                        nameof(WebHookBodyType),
                        _bodyTypeMetadata.BodyType);
                    throw new InvalidOperationException(message);
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }

        /// <summary>
        /// Determines whether the specified request contains JSON as indicated by a content type of
        /// <c>application/json</c>, <c>text/json</c> or <c>application/xyz+json</c>. The term <c>xyz</c> can for
        /// example be <c>hal</c> or some other JSON-derived media type.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if the specified request contains JSON content; otherwise, <see langword="false"/>.
        /// </returns>
        protected static bool IsJson(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var contentType = request.GetTypedHeaders().ContentType;
            if (contentType == null)
            {
                return false;
            }

            if (contentType.IsSubsetOf(ApplicationJsonMediaType) || contentType.IsSubsetOf(TextJsonMediaType))
            {
                return true;
            }

            // MVC's JsonInputFormatter does not support text/*+json by default. RFC 3023 and 6839 allow */*+json but
            // https://www.iana.org/assignments/media-types/media-types.xhtml shows all +json registrations except
            // model/gltf+json match application/*+json.
            return contentType.Type.Equals("application", StringComparison.OrdinalIgnoreCase) &&
                contentType.SubType.EndsWith("+json", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified request contains XML as indicated by a content type of
        /// <c>application/xml</c>, <c>text/xml</c> or <c>application/xyz+xml</c>. The term <c>xyz</c> can for example
        /// be <c>rdf</c> or some other XML-derived media type.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if the specified request contains XML content; otherwise, <see langword="false"/>.
        /// </returns>
        protected static bool IsXml(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var contentType = request.GetTypedHeaders().ContentType;
            if (contentType == null)
            {
                return false;
            }

            if (contentType.IsSubsetOf(ApplicationXmlMediaType) || contentType.IsSubsetOf(TextXmlMediaType))
            {
                return true;
            }

            // MVC's XML input formatters do not support text/*+xml by default. RFC 3023 and 6839 allow */*+xml but
            // https://www.iana.org/assignments/media-types/media-types.xhtml shows almost all +xml registrations
            // match application/*+xml and none match text/*+xml.
            return contentType.Type.Equals("application", StringComparison.OrdinalIgnoreCase) &&
                contentType.SubType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase);
        }

        private IActionResult CreateUnsupportedMediaTypeResult(string message)
        {
            _logger.LogInformation(0, message);

            var badMethod = new BadRequestObjectResult(message)
            {
                StatusCode = StatusCodes.Status415UnsupportedMediaType
            };

            return badMethod;
        }
    }
}
