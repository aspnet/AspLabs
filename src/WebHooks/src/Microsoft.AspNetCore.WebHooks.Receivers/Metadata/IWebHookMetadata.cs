// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Marker interface for all WebHook metadata. Receivers must register a class that implements both
    /// <see cref="IWebHookMetadata"/> and <see cref="IWebHookReceiver"/> whether or not an implementation of an
    /// <see cref="IWebHookMetadata"/> sub-interface is needed.
    /// </summary>
    public interface IWebHookMetadata
    {
    }
}
