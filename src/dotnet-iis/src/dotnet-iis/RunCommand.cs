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
    [Command("run", Description = "Runs an ASP.NET Core application in IIS Express.")]
    public class RunCommand
    {
        [Option("-p|--port <PORTNUMBER>", Description = "Specify the port number to bind to. Defaults to '59595'.")]
        public ushort Port { get; set; } = 59595;

        [Option("-h|--host <HOSTNAME>", Description = "Specify the host name to bind to. Defaults to 'localhost'.")]
        public string Host { get; set; } = "localhost";

        [Option("--ip-address <IPADDRESS>", Description = "Specify the local IP address to bind to. Defaults to '*'.")]
        public string LocalIp { get; set; } = "*";

        [Option("--ancm-path <ANCM_PATH>", Description = "Override the path to the ANCM module. VERY ADVANCED.")]
        public string AncmPath { get; set; }

        [Option("--ancm-version <ANCM_VERSION>", Description = "Specifies the version of the ANCM module to use. Defaults to 'v2'.")]
        public AspNetCoreModuleVersion? AncmVersion { get; set; }

        [Option("--iis-express <IIS_EXPRESS_PATH>", Description = @"Path to the root directory for IIS Express. Defaults to '%ProgramFiles%\IIS Express'")]
        public string IISExpressPath { get; set; }

#if DEBUG
        [Option("--preserve-temp", Description = "For Debugging. Preserves the temporary directory so it can be inspected.")]
#endif
        public bool PreserveTemporaryDirectory { get; set; }

        [Argument(0, "<APPROOT>", Description = "The path to the root of the application. Defaults to the current directory.")]
        public string ApplicationRoot { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            if (string.IsNullOrEmpty(ApplicationRoot))
            {
                ApplicationRoot = Directory.GetCurrentDirectory();
            }

            if (string.IsNullOrEmpty(AncmPath))
            {
                AncmPath = GetGlobalAncmPath(AncmVersion ?? IIS.AspNetCoreModuleVersion.V2);
            }
            else if (AncmVersion != null)
            {
                console.Error.WriteLine("The '--ancm-path' and '--ancm-version' options are mutually exclusive.");
                return 1;
            }

            if(string.IsNullOrEmpty(IISExpressPath))
            {
                IISExpressPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "IIS Express");
            }

            console.WriteLine($"Using ASP.NET Core Module from '{AncmPath}'.");

            ApplicationRoot = Path.GetFullPath(ApplicationRoot);

            // Build an applicationHost.config file from the template
            var resourceStream = typeof(RunCommand).Assembly.GetManifestResourceStream("Microsoft.DotNet.IIS.Resources.applicationHost.config");
            var appHostConfig = await XDocument.LoadAsync(resourceStream, LoadOptions.None, default);
            var configRoot = appHostConfig.Element("configuration");
            var sites = configRoot.Element("system.applicationHost").Element("sites");

            // Add the site
            var binding = $"{LocalIp}:{Port}:{Host}";
            var site = new XElement("site",
                new XAttribute("name", "DefaultSite"),
                new XAttribute("id", "1"),
                new XElement("application",
                    new XAttribute("path", "/"),
                    new XAttribute("applicationPool", "Clr4IntegratedAppPool"),
                    new XElement("virtualDirectory",
                        new XAttribute("path", "/"),
                        new XAttribute("physicalPath", ApplicationRoot))),
                new XElement("bindings",
                    new XElement("binding",
                        new XAttribute("protocol", "http"),
                        new XAttribute("bindingInformation", binding))));
            sites.AddFirst(site);

            // Add ANCM
            var addAncmModule = new XElement("add",
                new XAttribute("name", "AspNetCoreModuleV2"),
                new XAttribute("image", AncmPath));
            var globalModules = configRoot.Element("system.webServer").Element("globalModules");
            globalModules.Add(addAncmModule);

            // Generate the config file in a temp path
            var tempDir = Path.Combine(Path.GetTempPath(), $"dotnet-iis-run-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
            try
            {
                var appHostConfigPath = Path.Combine(tempDir, "applicationHost.config");
                using (var outputStream = new FileStream(appHostConfigPath, FileMode.Create))
                {
                    await appHostConfig.SaveAsync(outputStream, SaveOptions.None, default);
                }
                console.WriteLine($"Wrote applicationHost.config to: {appHostConfigPath}");

                var exitCode = await RunIISExpressAsync(console, appHostConfigPath, ApplicationRoot);

                if(exitCode != 0)
                {
                    console.Error.WriteLine($"IIS Express exited with exit code: {exitCode}");
                }
                return exitCode;
            }
            finally
            {
                if (!PreserveTemporaryDirectory)
                {
                    try
                    {
                        if (Directory.Exists(tempDir))
                        {
                            Directory.Delete(tempDir, recursive: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        console.Error.WriteLine("Warning: Failed to delete temporary directory: {tempDir}.");
                        console.Error.WriteLine(ex.ToString());
                    }
                }
            }
        }
        private Task<int> RunIISExpressAsync(IConsole console, string appHostConfig, string appBase)
        {
            var iisRunning = new TaskCompletionSource<int>();
            var iisExpressExe = Path.Combine(IISExpressPath, "iisexpress.exe");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(iisExpressExe, $"/config:\"{appHostConfig}\" /systray:false")
                {
                    WorkingDirectory = appBase
                },
                EnableRaisingEvents = true
            };

            process.Exited += (s, a) =>
            {
                iisRunning.TrySetResult(process.ExitCode);
            };

            process.Start();

            return iisRunning.Task;
        }

        private string GetGlobalAncmPath(AspNetCoreModuleVersion ancmVersion)
        {
            var iisPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "IIS Express");
            switch (ancmVersion)
            {
                case AspNetCoreModuleVersion.V1:
                    return Path.Combine(iisPath, "aspnetcore.dll");
                case AspNetCoreModuleVersion.V2:
                    return Path.Combine(iisPath, "Asp.Net Core Module", "V2", "aspnetcorev2.dll");
                default:
                    throw new CommandLineException($"Unknown ANCM Version: {ancmVersion}");
            }
        }
    }
}
