# .NET Diagnostics Tools

This repository contains tools for collecting diagnostics from .NET applications.

## [dotnet-dump](src/dotnet-dump)

A cross-platform tool to collect memory dumps of .NET processes:

```
Captures memory dumps of .NET processes

Usage: dotnet-dump [options]

Options:
  -p|--process-id <PROCESS_ID>    The ID of the process to collect a memory dump for
  -o|--output <OUTPUT_DIRECTORY>  The directory to write the dump to. Defaults to the current working directory.
  -?|-h|--help                    Show help information
```

This tool collects dumps of .NET processes. Currently supports Windows and Linux (using the `createdump` command). The tool currently supports capturing a dump immediately (when invoked). We plan to add support for "daemonizing" (running the tool the in the background) and having it capture a dump on certain conditions:

* When the CPU usage goes over a certain amount
* When memory usage goes over a certain amount
* When a certain EventPipe event is emitted (this will inherently "lag" a bit since the events are buffered)

## [dotnet-collect](src/dotnet-collect)

A cross-platform tool for collecting data from Managed EventSources and .NET Runtime events using EventPipe

```
Collects Event Traces from .NET processes

Usage: dotnet-collect [options]

Options:
  -p|--process-id <PROCESS_ID>    Filter to only the process with the specified process ID.
  -c|--config-path <CONFIG_PATH>  The path of the EventPipe config file to write, must be named [AppName].eventpipeconfig and be in the base directory for a managed app.
  -o|--output <OUTPUT_DIRECTORY>  The directory to write the trace to. Defaults to the current working directory.
  --buffer <BUFFER_SIZE_IN_MB>    The size of the in-memory circular buffer in megabytes.
  --provider <PROVIDER_SPEC>      An EventPipe provider to enable. A string in the form '<provider name>:<keywords>:<level>'.
  -?|-h|--help                    Show help information
 ```

 This tool collects EventPipe traces. Currently it is limited to using the file-based configuration added in .NET Core 2.2. To use it, you must manually provide the destination path for the `eventpipeconfig` file. For example:
 
 ```
 dotnet-collect -c ./path/to/my/app/MyApp.eventpipeconfig --provider Microsoft-Windows-DotNETRuntime
 ```

 The default behavior is to put traces in the directory from which you launched `dotnet-collect`. Traces are in files of the form `[appname].[processId].netperf` and can be viewed with [PerfView](https://github.com/Microsoft/PerfView).

 ## [dotnet-analyze](src/dotnet-analyze)

 A tool to analyze crash dumps, event traces, etc. for useful information. Right now it just has an analyzer that looks for incomplete async state machines (like DumpAsync in SOS). We are exploring providing more SOS-like functionality here (perhaps a managed SOS-style repl?)