// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Diagnostics.Client
{
    public class EventCounterState
    {
        public string ProviderName { get; }
        public string CounterName { get; }
        public double Mean { get; }
        public double StandardDeviation { get; }
        public double Count { get; }
        public double Min { get; }
        public double Max { get; }
        public TimeSpan Interval { get; }

        public EventCounterState(string providerName, string counterName, double mean, double standardDeviation, double count, double min, double max, TimeSpan interval)
        {
            ProviderName = providerName;
            CounterName = counterName;
            Mean = mean;
            StandardDeviation = standardDeviation;
            Count = count;
            Min = min;
            Max = max;
            Interval = interval;
        }
    }
}
