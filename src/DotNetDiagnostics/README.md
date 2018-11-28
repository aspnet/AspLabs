# .NET Diagnostics Tools

This repository contains tools for collecting diagnostics from .NET applications.

Nightly builds of these tools are available on https://dotnet.myget.org/gallery/aspnetcore-dev. To install, use one of the following command lines:

* `dotnet tool install -g dotnet-dump --add-source https://dotnet.myget.org/F/aspnetcore-dev/api/v3/index.json --version [version]`
* `dotnet tool install -g dotnet-collect --add-source https://dotnet.myget.org/F/aspnetcore-dev/api/v3/index.json --version [version]`
* `dotnet tool install -g dotnet-analyze --add-source https://dotnet.myget.org/F/aspnetcore-dev/api/v3/index.json --version [version]`

You **must** specify an exact version to install. You can find the latest versions of these packages at https://dotnet.myget.org/gallery/aspnetcore-dev . They are also listed below:

* `dotnet-collect`: ![dotnet-collect](https://img.shields.io/dotnet.myget/aspnetcore-dev/v/dotnet-collect.svg)
* `dotnet-dump`: ![dotnet-dump](https://img.shields.io/dotnet.myget/aspnetcore-dev/v/dotnet-dump.svg)
* `dotnet-analyze`: ![dotnet-analyze](https://img.shields.io/dotnet.myget/aspnetcore-dev/v/dotnet-analyze.svg)

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

An SOS-like "REPL" for exploring .NET memory dumps (based on [CLRMD](https://github.com/Microsoft/clrmd)) as well as `.netperf` event traces collected by `dotnet-collect`.

```
Inspect a crash dump using interactive commands

Usage: dotnet-analyze [arguments] [options]

Arguments:
<DUMP>        The path to the dump file to analyze.

Options:
  -?|-h|--help  Show help information
```

When you launch this command, a REPL prompt is provided:

```
Loading crash dump C:\Users\anurse\Desktop\dotnet-18956-20181012-153829-088.dmp...
Ready to process analysis commands. Type 'help' to list available commands or 'help [command]' to get detailed help on a command.
Type 'quit' or 'exit' to exit the analysis session.
>
```

The following commands are available:
* `quit` (alias `exit`) - Exit the tool
* `help` - List commands and help information (not yet implemented ;))
* `threads` (alias `~`) - List thread
* `DumpStack` - Dump managed stack trace for the current thread. A little like SOS's `!DumpStack`
* `DumpHeap` - Dump information about objects on the heap, grouped by type. A little like SOS's `!DumpHeap -stat`