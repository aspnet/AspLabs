using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Diagnostics.Tools.Analyze.Commands
{
    public class HelpCommand : IAnalysisCommand
    {
        public string Name => "help";

        public async Task RunAsync(IConsole console, string[] args, AnalysisSession session)
        {
            await console.Error.WriteLineAsync("Not yet implemented");
        }
    }
}
