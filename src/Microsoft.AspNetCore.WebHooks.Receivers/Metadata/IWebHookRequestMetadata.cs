// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Metadata describing the request body type an action expects. Must be implemented in a protocol-specific
    /// <see cref="WebHookAttribute"/> subclass or a registered <see cref="IWebHookMetadata"/> service. For services,
    /// see <see cref="IWebHookRequestMetadataService"/> in particular.
    /// </summary>
    public interface IWebHookRequestMetadata : IWebHookMetadata
    {
        /// <summary>
        /// Gets the <see cref="WebHookBodyType"/> this receiver or specific action requires.
        /// </summary>
        WebHookBodyType BodyType { get; }
    }
}
