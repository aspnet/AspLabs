// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Transport;
using Microsoft.Diagnostics.Transport.Protocol;
using Newtonsoft.Json.Linq;

namespace Microsoft.Diagnostics.Client
{
    public class DiagnosticsClient
    {
        private static readonly EventLevel[] _mappingArray = new EventLevel[]
        {
            EventLevel.Verbose,
            EventLevel.Verbose,
            EventLevel.Informational,
            EventLevel.Warning,
            EventLevel.Error,
            EventLevel.Critical,
        };

        private IDuplexPipe _pipe;
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
        private readonly EventPipeClientTransport _transport;

        public event Action<EventSourceCreatedMessage> OnEventSourceCreated;
        public event Action<EventCounterState> OnEventCounterUpdated;
        public event Action<EventWrittenMessage> OnEventWritten;
        public event Action<Exception> Disconnected;

        public DiagnosticsClient(string url) : this(new Uri(url))
        {
        }

        public DiagnosticsClient(Uri uri)
        {
            _transport = EventPipeTransport.Create(uri).CreateClient();
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            // Connect transport
            _pipe = await _transport.ConnectAsync(cancellationToken);

            // Start receive loop.
            _ = ReceiveLoop(_pipe.Input);
        }

        public async Task EnableEventsAsync(IEnumerable<EnableEventsRequest> requests)
        {
            await _writeLock.WaitAsync();
            try
            {
                EventPipeProtocol.WriteMessage(new EnableEventsMessage(requests.ToList()), _pipe.Output);
                await _pipe.Output.FlushAsync();
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public Task EnableCountersAsync(IEnumerable<string> counters)
        {
            var requests = counters.Select(CreateCounterRequest);
            return EnableEventsAsync(requests);
        }

        public Task EnableLoggersAsync(IEnumerable<string> loggers)
        {
            var request = new EnableEventsRequest(
                provider: "Microsoft-Extensions-Logging",
                level: EventLevel.Verbose,
                keywords: (EventKeywords)2,
                arguments: new Dictionary<string, string>() {
                    {"FilterSpecs", GenerateFilterSpec(loggers)}
                });
            return EnableEventsAsync(new EnableEventsRequest[] { request });
        }

        private async Task ReceiveLoop(PipeReader reader)
        {
            Exception shutdownEx = null;
            try
            {
                while (true)
                {
                    var result = await reader.ReadAsync();
                    var buffer = result.Buffer;

                    try
                    {
                        if (result.IsCanceled)
                        {
                            return;
                        }

                        while (EventPipeProtocol.TryParseMessage(ref buffer, out var message))
                        {
                            switch (message)
                            {
                                case EventSourceCreatedMessage eventSourceCreatedMessage:
                                    _ = Task.Run(() => OnEventSourceCreated?.Invoke(eventSourceCreatedMessage));
                                    break;
                                case EventWrittenMessage eventWrittenMessage:
                                    if (eventWrittenMessage.ProviderName.Equals("Microsoft-Extensions-Logging") && eventWrittenMessage.EventId == 2)
                                    {
                                        HandleLoggerMessage(eventWrittenMessage);
                                    }
                                    else if (eventWrittenMessage.EventId == -1 && eventWrittenMessage.EventName.Equals("EventCounters"))
                                    {
                                        HandleEventCounter(eventWrittenMessage);
                                    }
                                    else
                                    {
                                        if (eventWrittenMessage.Message != null && eventWrittenMessage.Payload.Count > 0)
                                        {
                                            // TODO: This is sketchy.
                                            eventWrittenMessage.Message = string.Format(eventWrittenMessage.Message, eventWrittenMessage.Payload.ToArray());
                                        }
                                        _ = Task.Run(() => OnEventWritten?.Invoke(eventWrittenMessage));
                                    }
                                    break;
                                default:
                                    throw new NotSupportedException($"Unsupported message type: {message.GetType().FullName}");
                            }
                        }

                        if (result.IsCompleted)
                        {
                            return;
                        }
                    }
                    finally
                    {
                        reader.AdvanceTo(buffer.Start);
                    }
                }
            }
            catch (Exception ex)
            {
                reader.Complete(ex);
                shutdownEx = ex;
            }
            finally
            {
                _ = Task.Run(() => Disconnected?.Invoke(shutdownEx));
                reader.Complete();
            }
        }

        private void HandleEventCounter(EventWrittenMessage eventWrittenMessage)
        {
            var payloadIndex = eventWrittenMessage.PayloadNames.IndexOf("Payload");
            if (payloadIndex == -1)
            {
                // No-op, something is wrong.
                return;
            }

            var payload = (JObject)eventWrittenMessage.Payload[payloadIndex];
            var eventCounterState = new EventCounterState(
                eventWrittenMessage.ProviderName,
                payload.Value<string>("Name"),
                payload.Value<double>("Mean"),
                payload.Value<double>("StandardDeviation"),
                payload.Value<double>("Count"),
                payload.Value<double>("Min"),
                payload.Value<double>("Max"),
                TimeSpan.FromSeconds(payload.Value<double>("IntervalSec")));
            _ = Task.Run(() => OnEventCounterUpdated?.Invoke(eventCounterState));
        }

        private void HandleLoggerMessage(EventWrittenMessage inputMessage)
        {
            var payloadDict = Enumerable.Range(0, inputMessage.Payload.Count).ToDictionary(
                i => inputMessage.PayloadNames[i],
                i => inputMessage.Payload[i]);

            var args = (JArray)payloadDict["Arguments"];

            var outputMessage = new EventWrittenMessage()
            {
                EventName = (string)payloadDict["EventId"],
                Level = MapLogLevel((long)payloadDict["Level"]),
                ProviderName = (string)payloadDict["LoggerName"],
                ActivityId = inputMessage.ActivityId,
                Channel = inputMessage.Channel,
                Version = inputMessage.Version,
                EventId = inputMessage.EventId,
                Keywords = inputMessage.Keywords,
                Opcode = inputMessage.Opcode,
                RelatedActivityId = inputMessage.RelatedActivityId,
                Tags = inputMessage.Tags,
                Task = inputMessage.Task,
            };

            string messageFormat = null;
            var messageArgs = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                var obj = (JObject)arg;
                var key = obj.Value<string>("Key");
                var value = obj.Value<string>("Value");
                if (key.Equals("{OriginalFormat}"))
                {
                    messageFormat = value;
                }
                else
                {
                    outputMessage.PayloadNames.Add(key);
                    outputMessage.Payload.Add(value);
                    messageArgs.Add(key, value);
                }
            }

            if (messageFormat != null)
            {
                outputMessage.Message = LogValuesFormatter.Format(messageFormat, messageArgs);
            }

            _ = Task.Run(() => OnEventWritten?.Invoke(outputMessage));
        }

        private EventLevel MapLogLevel(long inputLogLevel)
        {
            if (inputLogLevel < 0 || inputLogLevel > _mappingArray.Length)
            {
                return EventLevel.LogAlways;
            }
            return _mappingArray[inputLogLevel];
        }

        private EnableEventsRequest CreateCounterRequest(string providerName)
        {
            return new EnableEventsRequest(
                providerName, EventLevel.Informational, EventKeywords.All, new Dictionary<string, string>()
                {
                    { "EventCounterIntervalSec", "1" }
                });
        }

        private string GenerateFilterSpec(IEnumerable<string> loggers)
        {
            return string.Join(";", loggers.Select(l => $"{l}"));
        }
    }
}
