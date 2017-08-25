// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Services
{
    /// <summary>
    /// The default <see cref="ISalesforceResultCreator"/> implementation.
    /// </summary>
    public class SalesforceResultCreator : ISalesforceResultCreator
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new <see cref="SalesforceResultCreator"/> instance.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public SalesforceResultCreator(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SalesforceResultCreator>();
        }

        /// <inheritdoc />
        public async Task<ContentResult> GetFailedResultAsync(string message)
        {
            var resource = await GetResourceAsync("Microsoft.AspNetCore.WebHooks.Messages.FaultResponse.xml");
            var formattedResource = string.Format(CultureInfo.CurrentCulture, resource, message);

            return GetXmlResult(formattedResource, StatusCodes.Status400BadRequest);
        }

        /// <inheritdoc />
        public async Task<ContentResult> GetSuccessResultAsync()
        {
            var resource = await GetResourceAsync("Microsoft.AspNetCore.WebHooks.Messages.NotificationResponse.xml");

            return GetXmlResult(resource, StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Gets an <see cref="ContentResult"/> that when executed will produce a response with given
        /// <paramref name="content"/> and <paramref name="statusCode"/>.
        /// </summary>
        /// <param name="content">The requested XML-formatted content of the response.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <returns>
        /// An <see cref="ContentResult"/> that when executed will produce a response with given
        /// <paramref name="content"/> and <paramref name="statusCode"/>.
        /// </returns>
        protected virtual ContentResult GetXmlResult(string content, int statusCode)
        {
            return new ContentResult
            {
                Content = content,
                ContentType = "application/xml",
                StatusCode = statusCode,
            };
        }

        /// <summary>
        /// Gets the string content of the <paramref name="resourceName"/> resource.
        /// </summary>
        /// <param name="resourceName">
        /// Name of a manifest resource in the assembly containing <see cref="SalesforceResultCreator"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task{String}"/> that on completion provides the content of the <paramref name="resourceName"/>
        /// resource.
        /// </returns>
        protected virtual async Task<string> GetResourceAsync(string resourceName)
        {
            var assembly = typeof(SalesforceResultCreator).Assembly;
            var content = assembly.GetManifestResourceStream(resourceName);
            if (content == null)
            {
                var assemblyName = assembly.GetName().Name;
                _logger.LogCritical(
                    3,
                    "No '{0}' embedded resource found in the '{1}' assembly.",
                    resourceName,
                    assemblyName);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.ResultCreator_MissingResource,
                    resourceName,
                    assemblyName);
                throw new InvalidOperationException(message);
            }

            using (var reader = new StreamReader(content))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
