// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Tracing;

namespace Microsoft.Diagnostics.Transport.Protocol
{
    public class EventSourceCreatedMessage : EventPipeMessage
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public EventSourceSettings Settings { get; set; }

        public override MessageType Type => MessageType.EventSourceCreated;

        public EventSourceCreatedMessage()
        {
        }

        public EventSourceCreatedMessage(string name, Guid guid, EventSourceSettings settings) : this()
        {
            Name = name;
            Guid = guid;
            Settings = settings;
        }
    }
}
