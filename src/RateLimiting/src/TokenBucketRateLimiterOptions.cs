// Pending dotnet API review

namespace System.Threading.RateLimiting
{
    public class TokenBucketRateLimiterOptions : RateLimiterOptions
    {
        public TokenBucketRateLimiterOptions(
            int permitLimit,
            QueueProcessingOrder queueProcessingOrder,
            int queueLimit,
            TimeSpan replenishmentPeriod,
            int tokensPerReplenishment)
            : base(permitLimit, queueProcessingOrder, queueLimit)
        {
            ReplenishmentPeriod = replenishmentPeriod;
            TokensPerReplenishment = tokensPerReplenishment;
        }

        // Specifies the period between replenishments
        public TimeSpan ReplenishmentPeriod { get; set; }

        // Specifies how many tokens to restore each replenishment
        public int TokensPerReplenishment { get; set; }

        // Trigger replenishment automatically
        // This parameter is optional
        public bool AutoReplenishment { get; set; } = true;
    }
}
