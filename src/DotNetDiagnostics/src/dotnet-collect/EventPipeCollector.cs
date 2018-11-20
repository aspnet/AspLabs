using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;

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
                    // Try a few times
                    var tries = 0;
                    while (true)
                    {
                        try
                        {
                            // Read the current file
                            var events = await ReadEventsAsync(_currentFile);

                            // Update the file paths
                            _counter += 1;
                            _currentFile = _nextFile;
                            _nextFile = GetFilePath(_counter + 1);

                            return events;
                        }
                        catch (Exception) when (tries < 5)
                        {
                            tries += 1;
                        }
                    }
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

        private Task<List<TraceEvent>> ReadEventsAsync(string currentFile)
        {
            return Task.Run(() =>
            {
                var events = new List<TraceEvent>();
                var etlx = TraceLog.CreateFromEventPipeDataFile(currentFile);
                using (var trace = TraceLog.OpenOrConvert(etlx))
                {
                    try
                    {
                        foreach (var evt in trace.Events)
                        {
                            events.Add(evt.Clone());
                        }
                    }
                    catch (Exception)
                    {
                        // Just stop reading this file.
                    }
                }

                // Delete the file
                try
                {
                    File.Delete(currentFile);
                    File.Delete(etlx);
                }
                catch
                {
                    // Suppress the exception
                    // TODO: Logging
                }
                return events;
            });
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
