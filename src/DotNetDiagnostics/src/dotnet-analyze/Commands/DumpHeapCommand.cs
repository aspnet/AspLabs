using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Diagnostics.Tools.Analyze.Commands
{
    public class DumpHeapCommand : IAnalysisCommand
    {
        public IEnumerable<string> Names { get; } = new[] { "DumpHeap" };

        public Task RunAsync(IConsole console, string[] args, AnalysisSession session)
        {
            var stats = session.ComputeHeapStatistics().OrderBy(s => s.TotalSize);
            console.WriteLine("              MT    Count    TotalSize Class Name");
            foreach (var heapStats in stats)
            {
                console.WriteLine($"{heapStats.Type.MethodTable:X16} {heapStats.Count.ToString().PadLeft(8)} {heapStats.TotalSize.ToString().PadLeft(12)} {heapStats.Type.Name}");
            }

            return Task.CompletedTask;
        }
    }
}
