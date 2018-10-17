using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Diagnostics.Runtime;

namespace Microsoft.Diagnostics.Tools.Analyze.Commands
{
    public class DumpStackCommand : IAnalysisCommand
    {
        public IEnumerable<string> Names { get; } = new List<string>() { "dumpstack" };

        public async Task RunAsync(IConsole console, string[] args, AnalysisSession session)
        {
            foreach(var frame in session.ActiveThread.EnumerateStackTrace())
            {
                var methodInfo = frame.Method == null ? "<Unknown Method>" : GenerateMethodInfo(frame.Method);
                await console.Out.WriteLineAsync($"{frame.StackPointer:x16} {frame.InstructionPointer:x16} {methodInfo}");
            }
        }

        private string GenerateMethodInfo(ClrMethod method)
        {
            return $"{method.Type.Name}.{method.Name}";
        }
    }
}
