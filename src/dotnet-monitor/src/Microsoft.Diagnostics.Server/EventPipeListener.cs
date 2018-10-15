// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading.Channels;
using Microsoft.Diagnostics.Transport.Protocol;

namespace Microsoft.Diagnostics.Server
{
    /// <summary>
    /// Listens to the creation of event sources and events and produces <see cref="EventPipeMessage"/> objects representing them.
    /// </summary>
    public class EventPipeListener : EventListener
    {
        // Things created in field initializer DO run before OnEventSourceCreated
        // This also means we can't make the Channel settings here configurable :(
        private readonly Channel<EventPipeMessage> _messages = Channel.CreateUnbounded<EventPipeMessage>();

        private readonly object _lock = new object();
        private readonly Dictionary<string, EventSource> _sources = new Dictionary<string, EventSource>();
        private readonly Dictionary<string, EnableEventsRequest> _queuedRequests = new Dictionary<string, EnableEventsRequest>();

        public ChannelReader<EventPipeMessage> Messages => _messages.Reader;

        public EventPipeListener()
        {
            // WARNING: Any code here is going to run AFTER the OnEventSourceCreated method is fired for
            // event sources that existed prior to the listener being constructed
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            var message = new EventSourceCreatedMessage(eventSource.Name, eventSource.Guid, eventSource.Settings);
            var successful = _messages.Writer.TryWrite(message);
            Debug.Assert(successful, "Channel should be unbounded!");

            lock (_lock)
            {
                _sources.Add(eventSource.Name, eventSource);

                // Process any pending enable requests
                if (_queuedRequests.TryGetValue(eventSource.Name, out var enableEventsRequest))
                {
                    EnableEvents(eventSource, enableEventsRequest);
                    _queuedRequests.Remove(eventSource.Name);
                }
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var message = EventWrittenMessage.Create(eventData);
            var successful = _messages.Writer.TryWrite(message);
            Debug.Assert(successful, "Channel should be unbounded!");
        }

        public void EnableEvents(EnableEventsRequest request)
        {
            lock (_lock)
            {
                if (_sources.TryGetValue(request.Provider, out var eventSource))
                {
                    // The source has already been created, just enable it
                    EnableEvents(eventSource, request);
                }
                else
                {
                    // The source has not yet been created, queue the request
                    // TODO: Can't handle multiple requests for the same provider!
                    _queuedRequests.Add(request.Provider, request);
                }
            }
        }

        private void EnableEvents(EventSource eventSource, EnableEventsRequest request)
        {
            EnableEvents(eventSource, request.Level, request.Keywords, request.Arguments);
        }
    }
}
