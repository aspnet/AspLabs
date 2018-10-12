using System.Collections.Generic;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Diagnostics.Tools.Analyze.Commands
{
    public class DumpStackCommand : IAnalysisCommand
    {
        public IEnumerable<string> Names { get; } = new List<string>() { "dumpstack" };

        public async Task RunAsync(IConsole console, string[] args, AnalysisSession session)
        {
            foreach(var frame in session.ActiveThread.EnumerateStackTrace())
            {
                await console.Out.WriteLineAsync($"{frame.StackPointer:x16}");
            }
        }
    }
}
