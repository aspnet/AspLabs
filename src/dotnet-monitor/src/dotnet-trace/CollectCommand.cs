// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Diagnostics.Transport.Protocol;

namespace Microsoft.Diagnostics.Tools.Trace
{
    [Command(Name = Name, Description = "Collects events from the target process")]
    public class CollectCommand : TargetCommandBase
    {
        public const string Name = "collect";

        [Option("-p|--provider <PROVIDER>", "An EventSource provider to enable.", CommandOptionType.MultipleValue)]
        public IList<string> Providers { get; }

        [Option("-l|--logger <LOGGER>", "A Microsoft.Extensions.Logging logger prefix to enable.", CommandOptionType.MultipleValue)]
        public IList<string> Loggers { get; }

        [Option("-c|--counter <LOGGER>", "An EventSource to enable counters for.", CommandOptionType.MultipleValue)]
        public IList<string> Counters { get; }

        public async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            var disconnectCts = new CancellationTokenSource();
            var cancellationToken = console.GetCtrlCToken();

            if (!TryCreateClient(console, out var client))
            {
                return 1;
            }

            console.WriteLine("Connecting to application...");

            client.OnEventWritten += (evt) =>
            {
                // TODO: Format both kinds of messages ("Foo {0}" and "Foo {foo}")
                console.WriteLine($"{evt.ProviderName}/{evt.EventName}({evt.EventId}): {evt.Message}");
                for (var i = 0; i < evt.Payload.Count; i++)
                {
                    console.WriteLine($"  {evt.PayloadNames[i]}: {evt.Payload[i]}");
                }
            };

            client.OnEventCounterUpdated += (state) =>
            {
                console.WriteLine($"Counter: {state.ProviderName}/{state.CounterName} (Avg: {state.Mean}, StdDev: {state.StandardDeviation}, Count: {state.Count}, Min: {state.Min}, Max: {state.Max})");
            };

            client.Disconnected += (ex) =>
            {
                console.WriteLine("Disconnected");
                if (ex != null)
                {
                    console.Error.WriteLine(ex.ToString());
                }
                disconnectCts.Cancel();
            };

            await client.ConnectAsync();

            var enabledSomething = false;
            if (Loggers != null && Loggers.Any())
            {
                await client.EnableLoggersAsync(Loggers);
                enabledSomething = true;
            }

            if (Counters != null && Counters.Any())
            {
                await client.EnableCountersAsync(Counters);
                enabledSomething = true;
            }

            if (Providers != null && Providers.Any())
            {
                var requests = new List<EnableEventsRequest>(Providers.Count);
                foreach (var p in Providers)
                {
                    if (!TryCreateEventRequest(console, p, out var request))
                    {
                        return 1;
                    }
                    requests.Add(request);
                }
                await client.EnableEventsAsync(requests);
                enabledSomething = true;
            }

            if (!enabledSomething)
            {
                console.Error.WriteLine("At least one of '--provider', '--logger', or '--counter' must be provided.");
                return 1;
            }

            console.WriteLine("Connected, press Ctrl-C to terminate...");
            await CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, disconnectCts.Token).WaitForCancellationAsync();

            return 0;
        }

        private bool TryCreateEventRequest(IConsole console, string providerString, out EnableEventsRequest request)
        {
            // Format:
            //  provider[:level[:keywords]]

            var splat = providerString.Split(':');
            var level = EventLevel.Informational;
            var keywords = EventKeywords.All;
            var provider = splat[0].Trim();
            if (splat.Length > 1)
            {
                if (!Enum.TryParse(splat[1].Trim(), out level))
                {
                    console.Error.WriteLine($"Invalid event level '{splat[1]}'. Expected one of: {string.Join(", ", Enum.GetValues(typeof(EventLevel)))}");
                    request = null;
                    return false;
                }

                if (splat.Length > 2)
                {
                    if (!TryParseKeywords(splat[2].Trim(), provider, out keywords))
                    {
                        request = null;
                        return false;
                    }
                }
            }

            request = new EnableEventsRequest(provider, level, keywords);
            return true;
        }

        private bool TryParseKeywords(string input, string provider, out EventKeywords keywords)
        {
            var segments = input.Split("|");
            keywords = 0;
            foreach (var segment in segments)
            {
                if (!TryParseKeywordSegment(segment, provider, out var keyword))
                {
                    keywords = EventKeywords.All;
                    return false;
                }
                keywords |= keyword;
            }

            return true;
        }

        private Dictionary<string, Dictionary<string, EventKeywords>> _namedKeywords = new Dictionary<string, Dictionary<string, EventKeywords>>(StringComparer.OrdinalIgnoreCase)
        {
            // Known keywords indexed by Provider name and keyword
            {
                // Source: https://docs.microsoft.com/en-us/dotnet/framework/performance/clr-etw-keywords-and-levels
                "Microsoft-Windows-DotNETRuntime",
                new Dictionary<string, EventKeywords>(StringComparer.OrdinalIgnoreCase)
                {
                    { "GC", (EventKeywords)0x00000001 },
                    { "Loader", (EventKeywords)0x00000008 },
                    { "JIT", (EventKeywords)0x00000010 },
                    { "NGen", (EventKeywords)0x00000020 },
                    { "StartEnumeration", (EventKeywords)0x00000040 },
                    { "EndEnumeration", (EventKeywords)0x00000080 },
                    { "Security", (EventKeywords)0x00000400 },
                    { "AppDomainResourceManagement", (EventKeywords)0x00000800 },
                    { "JITTracing", (EventKeywords)0x00001000 },
                    { "Interop", (EventKeywords)0x00002000 },
                    { "Contention", (EventKeywords)0x00004000 },
                    { "Exception", (EventKeywords)0x00008000 },
                    { "Threading", (EventKeywords)0x00010000 },
                    { "OverrideAndSuppressNGenEvents", (EventKeywords)0x00040000 },
                    { "PerfTrack", (EventKeywords)0x02000000 },
                    { "Stack", (EventKeywords)0x40000000 },
                }
            }
        };
        private bool TryParseKeywordSegment(string segment, string provider, out EventKeywords keyword)
        {
            if (segment.StartsWith("0x"))
            {
                var chars = segment.AsSpan(2);
                var val = 0;
                for(var i = 0; i < chars.Length; i++)
                {
                    val = (val << 4) + FromHexChar(chars[i]);
                }
            }
            else if(int.TryParse(segment, out var intVal))
            {
                keyword = (EventKeywords)intVal;
                return true;
            }
            else if(_namedKeywords.TryGetValue(provider, out var providerKeywords) &&
                (providerKeywords.TryGetValue(segment, out keyword) ||
                 providerKeywords.TryGetValue($"{segment}Keyword", out keyword)))
            {
                return true;
            }

            keyword = 0;
            return false;
        }

        private int FromHexChar(char v)
        {
            if(v >= '0' && v <= '9')
            {
                return v - '0';
            }
            else if(v >= 'a' && v <= 'f')
            {
                return 0x0A + (v - 'a');
            }
            else if(v >= 'A' && v <= 'F')
            {
                return 0x0A + (v - 'A');
            }
            else
            {
                throw new InvalidOperationException($"Invalid hex character: {v}");
            }
        }
    }
}
