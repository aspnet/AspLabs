// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Metadata describing the request body type a receiver requires. All receivers must register an
    /// <see cref="IWebHookBodyTypeMetadataService"/> service.
    /// </summary>
    /// <remarks>
    /// <para>
    /// when the action has an associated receiver-specific <see cref="WebHookAttribute"/>,
    /// <see cref="ApplicationModels.WebHookModelBindingProvider"/> sets <see cref="Mvc.ModelBinding.BindingInfo"/>
    /// properties for a <c>data</c> parameter based on <see cref="IWebHookBindingMetadata"/> and this metadata.
    /// Otherwise i.e. when the action has an associated <see cref="GeneralWebHookAttribute"/>,
    /// <see cref="ApplicationModels.WebHookModelBindingProvider"/> uses <see cref="IWebHookBindingMetadata"/> and
    /// <see cref="IWebHookBodyTypeMetadata"/>.
    /// </para>
    /// <para>
    /// When processing each request, <see cref="Routing.WebHookReceiverExistsConstraint"/> checks for an applicable
    /// implementation of this metadata service to confirm the named receiver exists;
    /// <see cref="Filters.WebHookVerifyBodyTypeFilter"/> confirms the body type based on this metadata; and,
    /// <see cref="Filters.WebHookEventMapperFilter"/> uses this metadata to decide how to parse the request body and
    /// how to interpret <see cref="IWebHookEventFromBodyMetadata.BodyPropertyPath"/>.
    /// </para>
    /// </remarks>
    public interface IWebHookBodyTypeMetadataService : IWebHookMetadata, IWebHookReceiver
    {
        /// <summary>
        /// Gets the <see cref="WebHookBodyType"/> this receiver requires.
        /// </summary>
        WebHookBodyType BodyType { get; }
    }
}
