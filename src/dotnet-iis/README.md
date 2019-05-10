# dotnet-iis

An idea for a global tool to run and manage ASP.NET Core applications in IIS and IIS Express.

## `dotnet iis run`

Launches IIS Express to run an application.

```
〉 dotnet iis run --help
Runs an ASP.NET Core application in IIS Express.

Usage: dotnet-iis run [options] <<APPROOT>>

Arguments:
  <APPROOT>                         The path to the root of the application. Defaults to the current directory.

Options:
  -p|--port <PORTNUMBER>            Specify the port number to bind to. Defaults to '59595'.
  -h|--host <HOSTNAME>              Specify the host name to bind to. Defaults to 'localhost'.
  --ip-address <IPADDRESS>          Specify the local IP address to bind to. Defaults to '*'.
  --ancm-path <ANCM_PATH>           Override the path to the ANCM module. VERY ADVANCED.
  --ancm-version <ANCM_VERSION>     Specifies the version of the ANCM module to use. Defaults to 'v2'.
  --iis-express <IIS_EXPRESS_PATH>  Path to the root directory for IIS Express. Defaults to '%ProgramFiles%\IIS Express'
  -?|--help                         Show help information
```

## `dotnet iis logs`

Fetches Windows Event Log entries related to ASP.NET Core applications hosted in IIS and IIS Express

```
〉 dotnet iis logs --help
Fetches the most recent events in the Windows Event Log for ASP.NET Core Module

Usage: dotnet-iis logs [options]

Options:
  -n|--count <COUNT>        The number of events to fetch. Defaults to '10', specify '0' to fetch ALL events in the event log
  --machine <MACHINE_NAME>  A remote machine to retrieve logs for. Requires the ability to remotely access the Windows Event Log. Defaults to '.', the local machine.
  -?|-h|--help              Show help information
```
