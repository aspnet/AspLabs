using System;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Tools.Collect
{
    public class EtwCollector: EventCollector
    {
        private readonly CollectionConfiguration _config;

        public EtwCollector(CollectionConfiguration config)
        {
            _config = config;
        }

        public override Task StartCollectingAsync()
        {
            throw new NotImplementedException();
        }

        public override Task StopCollectingAsync()
        {
            throw new NotImplementedException();
        }
    }
}
