// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// General description of the required request body for a WebHook controller action.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The value <c>0</c> is not valid; a bit must be set in any use of this type.
    /// </para>
    /// <para>
    /// <see cref="Filters.WebHookVerifyBodyTypeFilter"/> enforces the "must" requirements described below.
    /// </para>
    /// </remarks>
    /// <seealso cref="IWebHookBodyTypeMetadata.BodyType"/>
    /// <seealso cref="IWebHookBodyTypeMetadataService.BodyType"/>
    public enum WebHookBodyType
    {
        /// <summary>
        /// Request must have <c>content-type</c> <c>application/x-www-form-urlencoded</c>.
        /// </summary>
        /// <remarks>
        /// The <see cref="ApplicationModels.WebHookModelBindingProvider"/> gives a bound <c>data</c> parameter the
        /// same <see cref="Mvc.ModelBinding.BindingInfo"/> as an associated <c>[FromForm]</c> attribute when the
        /// action or receiver has this <see cref="WebHookBodyType"/>.
        /// </remarks>
        Form = 1,

        /// <summary>
        /// Request must have <c>content-type</c> <c>application/json</c>, <c>application/*+json</c>, <c>text/json</c>,
        /// or a subset.
        /// </summary>
        /// <remarks>
        /// The <see cref="ApplicationModels.WebHookModelBindingProvider"/> gives a bound <c>data</c> parameter the
        /// same <see cref="Mvc.ModelBinding.BindingInfo"/> as an associated <c>[FromBody]</c> attribute when the
        /// action or receiver has this <see cref="WebHookBodyType"/>.
        /// </remarks>
        Json,

        /// <summary>
        /// Request must have <c>content-type</c> <c>application/xml</c>, <c>application/*+xml</c>, <c>text/xml</c>,
        /// or a subset.
        /// </summary>
        /// <remarks>
        /// The <see cref="ApplicationModels.WebHookModelBindingProvider"/> gives a bound <c>data</c> parameter the
        /// same <see cref="Mvc.ModelBinding.BindingInfo"/> as an associated <c>[FromBody]</c> attribute when the
        /// action or receiver has this <see cref="WebHookBodyType"/>.
        /// </remarks>
        Xml,
    }
}
