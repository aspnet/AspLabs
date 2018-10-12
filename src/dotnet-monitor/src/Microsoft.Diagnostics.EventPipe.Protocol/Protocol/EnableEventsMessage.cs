// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Diagnostics.Transport.Protocol
{
    public class EnableEventsMessage : EventPipeMessage
    {
        public IList<EnableEventsRequest> Requests { get; }
        public override MessageType Type => MessageType.EnableEvents;

        public EnableEventsMessage(): this(new List<EnableEventsRequest>())
        {
        }

        public EnableEventsMessage(IList<EnableEventsRequest> requests)
        {
            Requests = requests;
        }
    }
}
