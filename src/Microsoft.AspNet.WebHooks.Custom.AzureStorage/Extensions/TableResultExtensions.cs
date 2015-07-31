// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Extension methods for <see cref="TableResult"/>.
    /// </summary>
    [CLSCompliant(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TableResultExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="TableResult"/> was successful or not.
        /// </summary>
        /// <param name="result">The <see cref="TableResult"/> to inspect.</param>
        /// <returns><c>true</c> if the result was successful, <c>false</c> otherwise.</returns>
        public static bool IsSuccess(this TableResult result)
        {
            return result != null && result.HttpStatusCode >= 200 && result.HttpStatusCode <= 299;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TableResult"/> was referencing an non-existing entity.
        /// </summary>
        /// <param name="result">The <see cref="TableResult"/> to inspect.</param>
        /// <returns><c>true</c> if the entity was not found, <c>false</c> otherwise.</returns>
        public static bool IsNotFound(this TableResult result)
        {
            return result != null && result.HttpStatusCode == 404;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TableResult"/> represents a conflict
        /// causing the operation to fail.
        /// </summary>
        /// <param name="result">The <see cref="TableResult"/> to inspect.</param>
        /// <returns><c>true</c> if the result was conflicting, <c>false</c> otherwise.</returns>
        public static bool IsConflict(this TableResult result)
        {
            return result != null && (result.HttpStatusCode == 409 || result.HttpStatusCode == 412);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TableResult"/> represents an internal
        /// server error causing the operation to fail.
        /// </summary>
        /// <param name="result">The <see cref="TableResult"/> to inspect.</param>
        /// <returns><c>true</c> if the result was an internal server error, <c>false</c> otherwise.</returns>
        public static bool IsServerError(this TableResult result)
        {
            return result != null && result.HttpStatusCode >= 500 && result.HttpStatusCode <= 599;
        }
    }
}
