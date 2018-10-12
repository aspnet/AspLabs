// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Diagnostics.Transport.Protocol
{
    public abstract class EventPipeMessage
    {
        public abstract MessageType Type { get; }
    }
}
