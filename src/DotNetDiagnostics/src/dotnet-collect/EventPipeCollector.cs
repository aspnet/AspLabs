using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;

namespace Microsoft.Diagnostics.Tools.Collect
{
    public class EventPipeCollector : EventCollector
    {
        private const string FileSuffix = "netperf";

        private readonly CollectionConfiguration _config;
        private readonly string _configPath;
        private readonly string _filePrefix;
        private int _counter;
        private string _currentFile;
        private string _nextFile;

        public EventPipeCollector(CollectionConfiguration config, string configPath)
        {
            _config = config;
            _configPath = configPath;

            _counter = 1;
            _filePrefix = $"{Path.GetFileNameWithoutExtension(_configPath)}.{_config.ProcessId}";
            _currentFile = GetFilePath(_counter);
            _nextFile = GetFilePath(_counter + 1);
        }

        public override async Task<IEnumerable<TraceEvent>> ReadLatestEventsAsync(CancellationToken cancellationToken = default)
        {
            if (_config.FlushInterval == null)
            {
                return Enumerable.Empty<TraceEvent>();
            }

            // Check if the file after the current counter value exists
            while (!cancellationToken.IsCancellationRequested)
            {
                if (File.Exists(_nextFile))
                {
                    // Read the current file
                    var events = await ReadEventsAsync(_currentFile);

                    // Update the file paths
                    _counter += 1;
                    _currentFile = _nextFile;
                    _nextFile = GetFilePath(_counter + 1);

                    return events;
                }
                else
                {
                    // Wait for the file to exist
                    // REVIEW: This is hacky ;)
                    await Task.Delay(_config.FlushInterval.Value, cancellationToken);
                }
            }

            return Enumerable.Empty<TraceEvent>();
        }

        private Task<IEnumerable<TraceEvent>> ReadEventsAsync(string currentFile)
        {
            Console.WriteLine($"Reading file: {currentFile}");
            return Task.FromResult(Enumerable.Empty<TraceEvent>());
        }

        public override Task StartCollectingAsync()
        {
            var configContent = _config.ToConfigString();
            return File.WriteAllTextAsync(_configPath, configContent);
        }

        public override Task StopCollectingAsync()
        {
            File.Delete(_configPath);
            return Task.CompletedTask;
        }

        private string GetFilePath(int counter)
        {
            return Path.Combine(_config.OutputPath, $"{_filePrefix}.{counter}.netperf");
        }
    }
}
