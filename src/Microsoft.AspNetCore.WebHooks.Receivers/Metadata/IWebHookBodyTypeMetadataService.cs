// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Metadata describing the request body type an action expects. <see cref="IWebHookMetadata"/> service
    /// implementations that contain this information must implement this interface and not just
    /// <see cref="IWebHookBodyTypeMetadata"/>.
    /// </summary>
    public interface IWebHookBodyTypeMetadataService : IWebHookBodyTypeMetadata, IWebHookReceiver
    {
    }
}
