// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Microsoft.Diagnostics.Transport.Protocol
{
    public class EventWrittenMessage : EventPipeMessage
    {
        public override MessageType Type => MessageType.EventWritten;

        public string ProviderName { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
        public EventKeywords Keywords { get; set; }
        public EventLevel Level { get; set; }
        public string Message { get; set; }
        public EventOpcode Opcode { get; set; }
        public Guid RelatedActivityId { get; set; }
        public EventTags Tags { get; set; }
        public EventTask Task { get; set; }
        public byte Version { get; set; }
        public Guid ActivityId { get; set; }
        public EventChannel Channel { get; set; }
        public IList<string> PayloadNames { get; } = new List<string>();
        public IList<object> Payload { get; } = new List<object>();

        public static EventWrittenMessage Create(EventWrittenEventArgs args)
        {
            var message = new EventWrittenMessage()
            {
                ActivityId = args.ActivityId,
                Channel = args.Channel,
                EventId = args.EventId,
                EventName = args.EventName,
                ProviderName = args.EventSource.Name,
                Keywords = args.Keywords,
                Level = args.Level,
                Message = args.Message,
                Opcode = args.Opcode,
                RelatedActivityId = args.RelatedActivityId,
                Tags = args.Tags,
                Task = args.Task,
                Version = args.Version,
            };

            foreach(var name in args.PayloadNames)
            {
                message.PayloadNames.Add(name);
            }

            foreach(var value in args.Payload)
            {
                message.Payload.Add(value);
            }

            return message;
        }
    }
}
