using System.Threading.Tasks;
using Microsoft.Diagnostics.Runtime;

namespace DotNet.Analyzer.Services
{
    // This is full of races and just a simple way to get prototyping something
    // The idea is that this web app is "single-user" anyway, so it's not so bad :).
    public class DumpService
    {
        public DataTarget Dump { get; set; }

        public Task LoadDumpAsync(string path)
        {
            Dump = DataTarget.LoadCrashDump(path);
            return Task.CompletedTask;
        }

        public Task UnloadDumpAsync()
        {
            Dump = null;
            return Task.CompletedTask;
        }
    }
}
