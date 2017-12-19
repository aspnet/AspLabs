// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// <para>
    /// Marker metadata interface for receivers that require a <c>code</c> query parameter. That query parameter must
    /// match the configured secret key. Implemented in a <see cref="IWebHookMetadata"/> service for receivers that do
    /// not include a specific <see cref="Filters.WebHookSecurityFilter"/> subclass.
    /// </para>
    /// <para>
    /// <see cref="Filters.WebHookVerifyCodeFilter"/> verifies the <c>code</c> query parameter based on the existence
    /// of this metadata for the receiver. <see cref="Filters.WebHookReceiverExistsFilter"/> verifies at least one
    /// receiver-specific filter exists unless this metadata exists for the receiver.
    /// </para>
    /// </summary>
    public interface IWebHookVerifyCodeMetadata : IWebHookMetadata, IWebHookReceiver
    {
    }
}
