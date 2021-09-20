// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Pending dotnet API review

namespace System.Threading.RateLimiting
{
    sealed public class ConcurrencyLimiterOptions
    {
        public ConcurrencyLimiterOptions(int permitLimit, QueueProcessingOrder queueProcessingOrder, int queueLimit)
        {
            PermitLimit = permitLimit;
            QueueProcessingOrder = queueProcessingOrder;
            QueueLimit = queueLimit;
        }

        // Maximum number of permits allowed to be leased
        public int PermitLimit { get; }

        // Behaviour of WaitAsync when not enough resources can be leased
        public QueueProcessingOrder QueueProcessingOrder { get; }

        // Maximum cumulative permit count of queued acquisition requests
        public int QueueLimit { get; }
    }
}
