// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Pending dotnet API review

namespace System.Threading.RateLimiting
{
    /// <summary>
    /// Options to control the behavior of a <see cref="ConcurrencyLimiter"/>.
    /// </summary>
    public sealed class ConcurrencyLimiterOptions
    {
        /// <summary>
        /// Initializes the <see cref="ConcurrencyLimiterOptions"/>.
        /// </summary>
        /// <param name="permitLimit"></param>
        /// <param name="queueProcessingOrder"></param>
        /// <param name="queueLimit"></param>
        /// <exception cref="ArgumentOutOfRangeException">When <paramref name="permitLimit"/> or <paramref name="queueLimit"/> are less than 0.</exception>
        public ConcurrencyLimiterOptions(int permitLimit, QueueProcessingOrder queueProcessingOrder, int queueLimit)
        {
            if (permitLimit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(permitLimit));
            }
            if (queueLimit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(queueLimit));
            }
            PermitLimit = permitLimit;
            QueueProcessingOrder = queueProcessingOrder;
            QueueLimit = queueLimit;
        }

        /// <summary>
        /// Maximum number of permits allowed to be leased.
        /// </summary>
        public int PermitLimit { get; }

        /// <summary>
        /// Determines the behaviour of <see cref="RateLimiter.WaitAsync"/> when not enough resources can be leased.
        /// </summary>
        /// <value>
        /// <see cref="QueueProcessingOrder.OldestFirst"/>
        /// </value>
        public QueueProcessingOrder QueueProcessingOrder { get; } = QueueProcessingOrder.OldestFirst;

        /// <summary>
        /// Maximum cumulative permit count of queued acquisition requests.
        /// </summary>
        public int QueueLimit { get; }
    }
}
