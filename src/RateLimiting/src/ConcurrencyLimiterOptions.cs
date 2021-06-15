// Pending dotnet API review

namespace System.Threading.RateLimiting
{
    // Can be derived for config driven options
    public class ConcurrencyLimiterOptions : RateLimiterOptions
    {
        public ConcurrencyLimiterOptions(int permitLimit, QueueProcessingOrder queueProcessingOrder, int queueLimit)
            : base (permitLimit, queueProcessingOrder, queueLimit) { }
    }
}
