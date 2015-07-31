// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Indicates the result of inserting, updating, and deleting items in the <see cref="IWebHookStore"/>.
    /// </summary>
    public enum StoreResult
    {
        /// <summary>
        /// The operation succeeded.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The targeted entity did not exist.
        /// </summary>
        NotFound,

        /// <summary>
        /// The operation resulted in a conflict.
        /// </summary>
        Conflict,

        /// <summary>
        /// The operation was not formulated correctly.
        /// </summary>
        OperationError,

        /// <summary>
        /// The operation resulted in an internal error.
        /// </summary>
        InternalError
    }
}
