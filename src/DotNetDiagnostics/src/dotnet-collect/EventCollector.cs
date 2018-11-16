using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;

namespace Microsoft.Diagnostics.Tools.Collect
{
    public abstract class EventCollector
    {
        public abstract Task StartCollectingAsync();
        public abstract Task StopCollectingAsync();

        public abstract Task<IEnumerable<TraceEvent>> ReadLatestEventsAsync(CancellationToken cancellationToken = default);
    }
}
