// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// <para>
    /// Metadata describing additional action parameters supported on a per-receiver basis. Implemented in a
    /// <see cref="IWebHookMetadata"/> service for receivers that support additional action parameters.
    /// </para>
    /// <para>
    /// Metadata is used for model binding and, if <see cref="WebHookParameter.IsRequired"/> is <see langword="true"/>,
    /// incoming request validation.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="IWebHookEventMetadata"/> because these bindings do not support constant values or
    /// fall backs. Separate from <see cref="IWebHookRequestMetadata"/> because the information cannot be changed on a
    /// per-action basis; a service must provide this metadata.
    /// </remarks>
    public interface IWebHookBindingMetadata : IWebHookMetadata, IWebHookReceiver
    {
        /// <summary>
        /// Gets the collection of <see cref="WebHookParameter"/>s.
        /// </summary>
        IReadOnlyList<WebHookParameter> Parameters { get; }
    }
}
