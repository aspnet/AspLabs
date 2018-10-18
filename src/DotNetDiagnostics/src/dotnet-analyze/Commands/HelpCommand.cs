using System.Collections.Generic;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Diagnostics.Tools.Analyze.Commands
{
    public class HelpCommand : IAnalysisCommand
    {
        public IEnumerable<string> Names { get; } = new List<string>() { "help" };

        public async Task RunAsync(IConsole console, string[] args, AnalysisSession session)
        {
            await console.Error.WriteLineAsync("Not yet implemented");
        }
    }
}
