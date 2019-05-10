using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.DotNet.IIS
{
    [Command("logs", Description = "Fetches the most recent events in the Windows Event Log for ASP.NET Core Module")]
    public class LogsCommand
    {
        private static readonly ISet<string> EventSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "IIS Express AspNetCore Module V2",
            "IIS Express"
        };

        [Option("-n|--count <COUNT>", Description = "The number of events to fetch. Defaults to '10', specify '0' to fetch ALL events in the event log")]
        public int Count { get; set; } = 10;

        [Option("--machine <MACHINE_NAME>", Description = "A remote machine to retrieve logs for. Requires the ability to remotely access the Windows Event Log. Defaults to '.', the local machine.")]
        public string MachineName { get; set; } = ".";

        public int OnExecute(IConsole console)
        {
            var log = new EventLog("Application", MachineName);
            IEnumerable<EventLogEntry> entries = log.Entries
                .Cast<EventLogEntry>()
                .Where(e => EventSources.Contains(e.Source))
                .OrderByDescending(e => e.TimeGenerated);
            if (Count > 0)
            {
                entries = entries.Take(Count);
            }

            foreach (var entry in entries)
            {
                console.WriteLine($"[{entry.TimeGenerated:O}] {entry.Source}: {entry.Message}");
            }

            return 0;
        }
    }
}
