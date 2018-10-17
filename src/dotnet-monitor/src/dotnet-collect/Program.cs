using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Internal.Utilities;

namespace Microsoft.Diagnostics.Tools.Collect
{
    [Command(Name = "dotnet-collect", Description = "Collects Event Traces from .NET processes")]
    internal class Program
    {
        [Required(ErrorMessage = "You must provide the path of the EventPipe config file to write")]
        [Option("-c|--config-path <CONFIG_PATH>", Description = "The path of the EventPipe config file to write, must be named [AppName].eventpipeconfig and be in the base directory for a managed app.")]
        public string ConfigPath { get; set; }

        [Option("-p|--process-id <PROCESS_ID>", Description = "Filter to only the process with the specified process ID.")]
        public int? ProcessId { get; set; }

        [Option("-o|--output <OUTPUT_DIRECTORY>", Description = "The directory to write the trace to. Defaults to the current working directory.")]
        public string OutputDir { get; set; }

        [Option("--buffer <BUFFER_SIZE_IN_MB>", Description = "The size of the in-memory circular buffer in megabytes.")]
        public int? CircularMB { get; set; }

        [Option("--provider <PROVIDER_SPEC>", Description = "An EventPipe provider to enable. A string in the form '<provider name>:<keywords>:<level>'. Can be specified multiple times to enable multiple providers.")]
        public IList<string> Providers { get; set; }

        public async Task<int> OnExecute(IConsole console, CommandLineApplication app)
        {
            if (File.Exists(ConfigPath))
            {
                console.Error.WriteLine("Config file already exists, tracing is already underway by a different consumer.");
                return 1;
            }

            var appBase = Path.GetDirectoryName(ConfigPath);
            var appName = Path.GetFileNameWithoutExtension(ConfigPath);

            var config = new EventPipeConfiguration()
            {
                ProcessId = ProcessId,
                CircularMB = CircularMB,
                OutputPath = string.IsNullOrEmpty(OutputDir) ? Directory.GetCurrentDirectory() : OutputDir
            };

            if (Providers != null && Providers.Count > 0)
            {
                foreach (var provider in Providers)
                {
                    if (!EventSpec.TryParse(provider, out var spec))
                    {
                        console.Error.WriteLine($"Invalid provider specification: '{provider}'. A provider specification must be in one of the following formats:");
                        console.Error.WriteLine(" <providerName>                       - Enable all events at all levels for the provider.");
                        console.Error.WriteLine(" <providerName>:<keywords>            - Enable events matching the specified keywords for the specified provider.");
                        console.Error.WriteLine(" <providerName>:<level>               - Enable events at the specified level for the provider.");
                        console.Error.WriteLine(" <providerName>:<keywords>:<level>    - Enable events matching the specified keywords, at the specified level for the specified provider.");
                        console.Error.WriteLine("");
                        console.Error.WriteLine("'<provider>' must be the name of the EventSource.");
                        console.Error.WriteLine("'<level>' can be one of: Critical (1), Error (2), Warning (3), Informational (4), Verbose (5). Either the name or number can be specified.");
                        console.Error.WriteLine("'<keywords>' is a hexadecimal number, starting with '0x', defining the keywords to enable.");
                        return 1;
                    }
                    config.Providers.Add(spec);
                }
            }

            // Write the config file contents
            var configContent = config.ToConfigString();
            File.WriteAllText(ConfigPath, configContent);
            console.WriteLine("Tracing has started. Press Ctrl-C to stop.");

            await console.WaitForCtrlCAsync();

            File.Delete(ConfigPath);
            console.WriteLine($"Tracing stopped. Trace files written to {config.OutputPath}");

            return 0;
        }

        private static int Main(string[] args)
        {
            DebugUtil.WaitForDebuggerIfRequested(ref args);

            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (PlatformNotSupportedException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
            catch (OperationCanceledException)
            {
                return 0;
            }
        }
    }
}
