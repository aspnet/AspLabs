// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Pending dotnet API review

namespace System.Threading.RateLimiting
{
#pragma warning disable 1591
    sealed public class TokenBucketRateLimiterOptions
    {
        public TokenBucketRateLimiterOptions(
            int tokenLimit,
            QueueProcessingOrder queueProcessingOrder,
            int queueLimit,
            TimeSpan replenishmentPeriod,
            int tokensPerPeriod,
            bool autoReplenishment = true)
        {
            TokenLimit = tokenLimit;
            QueueProcessingOrder = queueProcessingOrder;
            QueueLimit = queueLimit;
            ReplenishmentPeriod = replenishmentPeriod;
            TokensPerPeriod = tokensPerPeriod;
            AutoReplenishment = autoReplenishment;
        }

        // Specifies the period between replenishments
        public TimeSpan ReplenishmentPeriod { get; }

        // Specifies how many tokens to restore each replenishment
        public int TokensPerPeriod { get; }

        // Whether to create a timer to trigger replenishment automatically
        // This parameter is optional
        public bool AutoReplenishment { get; }

        // Maximum number of permits allowed to be leased
        public int TokenLimit { get; }

        // Behaviour of WaitAsync when not enough resources can be leased
        public QueueProcessingOrder QueueProcessingOrder { get; }

        // Maximum cumulative permit count of queued acquisition requests
        public int QueueLimit { get; }
    }
#pragma warning disable
}
