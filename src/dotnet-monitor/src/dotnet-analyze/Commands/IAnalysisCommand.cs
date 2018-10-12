using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.Diagnostics.Tools.Analyze.Commands
{
    public interface IAnalysisCommand
    {
        string Name { get; }

        Task RunAsync(IConsole console, string[] args, AnalysisSession session);
    }
}
