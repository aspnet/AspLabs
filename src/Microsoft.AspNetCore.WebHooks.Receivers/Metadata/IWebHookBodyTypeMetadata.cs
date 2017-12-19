// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// <para>
    /// Metadata describing the request body type an action expects. Must be implemented in a protocol-specific
    /// <see cref="WebHookAttribute"/> subclass or a registered <see cref="IWebHookMetadata"/> service. For services,
    /// see <see cref="IWebHookBodyTypeMetadataService"/> in particular.
    /// </para>
    /// <para>
    /// <see cref="ApplicationModels.WebHookModelBindingProvider"/> sets <see cref="Mvc.ModelBinding.BindingInfo"/>
    /// properties based on this metadata and <see cref="IWebHookBindingMetadata"/>.
    /// <see cref="Filters.WebHookVerifyBodyTypeFilter"/> confirms the request body type based on this metadata.
    /// </para>
    /// </summary>
    public interface IWebHookBodyTypeMetadata : IWebHookMetadata
    {
        /// <summary>
        /// Gets the <see cref="WebHookBodyType"/> this receiver or specific action requires.
        /// </summary>
        WebHookBodyType BodyType { get; }
    }
}
