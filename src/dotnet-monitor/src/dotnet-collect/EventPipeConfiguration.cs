using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Diagnostics.Tools.Collect
{
    public class EventPipeConfiguration
    {
        public int? ProcessId { get; set; }
        public string OutputPath { get; set; }
        public int? CircularMB { get; set; }
        public IList<EventSpec> Providers { get; set; } = new List<EventSpec>();

        internal string ToConfigString()
        {
            var builder = new StringBuilder();
            if (ProcessId != null)
            {
                builder.AppendLine($"ProcessId={ProcessId.Value}");
            }
            if (!string.IsNullOrEmpty(OutputPath))
            {
                builder.AppendLine($"OutputPath={OutputPath}");
            }
            if (CircularMB != null)
            {
                builder.AppendLine($"CircularMB={CircularMB}");
            }
            if (Providers != null && Providers.Count > 0)
            {
                builder.AppendLine($"Providers={SerializeProviders(Providers)}");
            }
            return builder.ToString();
        }

        private string SerializeProviders(IList<EventSpec> providers) => string.Join(",", providers.Select(s => s.ToConfigString()));
    }
}
