using System.Collections.Generic;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Diagnostics.Tools.Analyze.Commands
{
    public class ThreadsCommand : IAnalysisCommand
    {
        public IEnumerable<string> Names { get; } = new List<string>() { "~", "threads" };

        public async Task RunAsync(IConsole console, string[] args, AnalysisSession session)
        {
            foreach(var thread in session.Runtime.Threads)
            {
                var isActive = session.ActiveThreadId == thread.ManagedThreadId ? "." : " ";
                await console.Out.WriteLineAsync($"{isActive}{thread.ManagedThreadId.ToString().PadLeft(2)} Id: {Utils.FormatAddress(thread.OSThreadId)} Teb: {Utils.FormatAddress(thread.Teb)}");
            }
        }
    }
}
