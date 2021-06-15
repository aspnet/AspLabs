namespace System.Threading.RateLimiting
{
    public class RateLimiterOptions
    {
        public RateLimiterOptions(int permitLimit, QueueProcessingOrder queueProcessingOrder, int queueLimit)
        {
            PermitLimit = permitLimit;
            QueueProcessingOrder = queueProcessingOrder;
            QueueLimit = queueLimit;
        }

        // Maximum number of permits allowed to be leased
        public int PermitLimit { get; set; }

        // Behaviour of WaitAsync when not enough resources can be leased
        public QueueProcessingOrder QueueProcessingOrder { get; set; }

        // Maximum cumulative permit count of queued acquisition requests
        public int QueueLimit { get; set; }
    }
}
