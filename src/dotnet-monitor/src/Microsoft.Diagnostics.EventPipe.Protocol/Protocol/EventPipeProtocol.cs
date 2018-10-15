// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Diagnostics.Transport.Protocol
{
    public static class EventPipeProtocol
    {
        // I got lazy and just did JSON. We'll figure it out later.

        private const byte RecordSeparator = 0x1E;
        private static readonly ReadOnlyMemory<byte> RecordSeparatorMemory = new byte[] { RecordSeparator };
        private static readonly ReadOnlyMemory<byte> SerializedPing = PreserializePing();

        public static bool TryParseMessage(ref ReadOnlySequence<byte> input, out EventPipeMessage message)
        {
            if (input.PositionOf(RecordSeparator) is SequencePosition position)
            {
                var buffer = input.Slice(input.Start, position);
                input = input.Slice(input.GetPosition(1, position));

                message = ParseMessage(buffer);
                return true;
            }
            else
            {
                message = null;
                return false;
            }
        }

        public static void WriteMessage(EventPipeMessage message, PipeWriter writer)
        {
            if (ReferenceEquals(message, PingMessage.Instance))
            {
                writer.Write(SerializedPing.Span);
            }
            else
            {
                var json = new JObject(
                    new JProperty("type", message.Type),
                    new JProperty("payload", JObject.FromObject(message)));
                var str = json.ToString(Formatting.None);
                var bytes = Encoding.UTF8.GetBytes(str);
                writer.Write(bytes.AsSpan());
                writer.Write(RecordSeparatorMemory.Span);
            }
        }

        private static ReadOnlyMemory<byte> PreserializePing()
        {
            var json = new JObject(
                new JProperty("type", MessageType.Ping));
            var str = json.ToString(Formatting.None);
            var byteLen = Encoding.UTF8.GetByteCount(str);
            var bytes = new byte[byteLen + RecordSeparatorMemory.Length];
            Encoding.UTF8.GetBytes(str, 0, str.Length, bytes, 0);
            RecordSeparatorMemory.CopyTo(bytes.AsMemory(byteLen));
            return bytes;
        }

        private static EventPipeMessage ParseMessage(ReadOnlySequence<byte> input)
        {
            if (input.IsSingleSegment)
            {
                return ParseMessage(input.First.Span);
            }
            else
            {
                return ParseMessage(input.ToArray());
            }
        }

        private static EventPipeMessage ParseMessage(ReadOnlySpan<byte> input)
        {
            string str;
            unsafe
            {
                fixed (byte* i = input)
                {
                    str = Encoding.UTF8.GetString(i, input.Length);
                }
            }

            var json = JObject.Parse(str);
            var type = (MessageType)(int)json["type"];

            switch (type)
            {
                case MessageType.Ping: return PingMessage.Instance;
                case MessageType.EventSourceCreated: return json["payload"].ToObject<EventSourceCreatedMessage>();
                case MessageType.EnableEvents: return json["payload"].ToObject<EnableEventsMessage>();
                case MessageType.EventWritten: return json["payload"].ToObject<EventWrittenMessage>();
                default: throw new NotSupportedException($"Unknown message type: {type}");
            }
        }
    }
}
