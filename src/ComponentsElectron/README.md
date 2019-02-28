# Components.Electron

This directory contains samples and package sources that let you host Razor Components inside an Electron shell. This allows for modern, high-performance cross-platform desktop apps built with .NET and web technologies.

**Experimental and unsupported**. These packages are not intended for production use. At this stage there is no commitment to supporting Razor Components within Electron. This labs project is experimental.

## How to use

If you want to try building a Razor Components + Electron app:

 * Copy the `src/ComponentsElectron/sample/SampleApp` directory to some other location outside this repo's sources.
   * You do *not* need to use anything from `src/Components/src` (nor should you try to do so).
 * Then, build and run `SampleApp.csproj`.
   * You can do this either in Visual Studio (Windows) or via `dotnet run` (command line)

The contents of `src/ComponentsElectron/src` are only intended for people working on the `ComponentsElectron` experimental infrastructure itself.
