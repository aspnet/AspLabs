using System.Collections.Generic;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Diagnostics.Tools.Analyze.Commands
{
    public interface IAnalysisCommand
    {
        IEnumerable<string> Names { get; }

        Task RunAsync(IConsole console, string[] args, AnalysisSession session);
    }
}
