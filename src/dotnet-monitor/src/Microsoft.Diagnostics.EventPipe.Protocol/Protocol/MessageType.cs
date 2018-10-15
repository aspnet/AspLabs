// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Diagnostics.Transport.Protocol
{
    public enum MessageType
    {
        Ping = 1,
        EventSourceCreated = 2,
        EnableEvents = 3,
        EventWritten = 4,
    }
}
