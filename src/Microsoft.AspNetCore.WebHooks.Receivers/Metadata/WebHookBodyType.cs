// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// General description of the required request body for a WebHook controller action.
    /// </summary>
    /// <remarks>Starts at <c>1</c> to ensure <see cref="WebHookBodyType"/> instances are initialized.</remarks>
    public enum WebHookBodyType
    {
        /// <summary>
        /// Request must have <c>content-type</c> <c>application/x-www-form-urlencoded</c>. A bound <c>data</c>
        /// parameter should have an associated <c>[FromForm]</c> attribute.
        /// </summary>
        Form = 1,

        /// <summary>
        /// Request must have <c>content-type</c> <c>application/json</c>, <c>text/json</c>, or a subset. A bound
        /// <c>data</c> parameter should have an associated <c>[FromBody]</c> attribute.
        /// </summary>
        Json,

        /// <summary>
        /// Request must have <c>content-type</c> <c>application/xml</c>, <c>text/xml</c>, or a subset. A bound
        /// <c>data</c> parameter should have an associated <c>[FromBody]</c> attribute.
        /// </summary>
        Xml,
    }
}
