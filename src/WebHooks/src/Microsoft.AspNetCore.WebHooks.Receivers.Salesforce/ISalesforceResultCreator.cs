// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Provides an abstraction for creating the SOAP responses Salesforce expects.
    /// </summary>
    public interface ISalesforceResultCreator
    {
        /// <summary>
        /// Gets an <see cref="ContentResult"/> that when executed will produce a response with status code 400 "Bad
        /// Request" and an XML body containing <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The error message string to include in the XML-formatted response.</param>
        /// <returns>
        /// A <see cref="Task{ContentResult}"/> that on completion provides an <see cref="ContentResult"/> that when
        /// executed will produce the desired response.
        /// </returns>
        Task<ContentResult> GetFailedResultAsync(string message);

        /// <summary>
        /// Gets an <see cref="ContentResult"/> that when executed will produce a response with status code 200 "OK"
        /// and an XML body containing a Salesforce acknowledgment message.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{ContentResult}"/> that on completion provides an <see cref="ContentResult"/> that when
        /// executed will produce the desired response.
        /// </returns>
        Task<ContentResult> GetSuccessResultAsync();
    }
}
