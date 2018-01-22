// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Metadata describing the request body type an action expects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the action has an associated <see cref="GeneralWebHookAttribute"/>,
    /// <see cref="ApplicationModels.WebHookModelBindingProvider"/> sets <see cref="Mvc.ModelBinding.BindingInfo"/>
    /// properties for a <c>data</c> parameter based on <see cref="IWebHookBindingMetadata"/> and this metadata.
    /// Otherwise i.e. when the action has an associated receiver-specific <see cref="WebHookAttribute"/>,
    /// <see cref="ApplicationModels.WebHookModelBindingProvider"/> uses <see cref="IWebHookBindingMetadata"/> and
    /// <see cref="IWebHookBodyTypeMetadataService"/>.
    /// </para>
    /// <para>
    /// This metadata is not referenced per request. See <see cref="IWebHookBodyTypeMetadataService"/> for body type
    /// requirements enforced at runtime.
    /// </para>
    /// <para>
    /// This interface is implemented only in <see cref="GeneralWebHookAttribute"/>, where supported receivers may have
    /// conflicting requirements.
    /// </para>
    /// </remarks>
    public interface IWebHookBodyTypeMetadata : IWebHookMetadata
    {
        /// <summary>
        /// Gets the <see cref="WebHookBodyType"/> this action expects.
        /// </summary>
        /// <value>
        /// Default value is <see langword="null"/> which indicates a <c>data</c> parameter is not expected
        /// and, if such a parameter exists, it requires no additional <see cref="Mvc.ModelBinding.BindingInfo"/>.
        /// </value>
        WebHookBodyType? BodyType { get; }
    }
}
