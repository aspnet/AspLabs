// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ElectronNET.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Components.Electron
{
    internal static class Launcher
    {
        public static IJSRuntime ElectronJSRuntime { get; private set; }
        public static string InitialUriAbsolute { get; private set; }
        public static string BaseUriAbsolute { get; private set; }

        public static void StartElectronProcess(Func<Task> callback)
        {
            var projectRoot = GetProjectRoot();
            Directory.SetCurrentDirectory(projectRoot);

            var applicationBaseDir = Path.GetDirectoryName(typeof(Launcher).Assembly.Location);
            var electronEntryPoint = Path.Combine(
                applicationBaseDir,
                "electron-js",
                "main.js");
            var electronPort = SelectPort();

            Log($"Launching Electron on port {electronPort}");

            var electronFilename = Path.Combine(projectRoot, "node_modules", "electron", "dist", GetElectronPath());
            var electronProcess = ElectronProcess.Start(new ProcessStartInfo
            {
                FileName = electronFilename,
                Arguments = $"\"{electronEntryPoint}\" {electronPort}"
            });

            // TODO - this is a gross hack because Electron.NET's bridge connector doesn't let us configure the port any other way.
            ElectronNET.API.WebHostBuilderExtensions.UseElectron(new FakeWebHostBuilder(), new[] { $"/electronport={electronPort}" });

            var current = SynchronizationContext.Current;
            var electronSynchronizationContext = new ElectronSynchronizationContext();

            SynchronizationContext.SetSynchronizationContext(electronSynchronizationContext);

            ElectronNET.API.Electron.IpcMain.On("BeginInvokeDotNetFromJS", args =>
            {
                electronSynchronizationContext.Send(state =>
                {
                    JSRuntime.SetCurrentJSRuntime(ElectronJSRuntime);
                    ElectronRenderer.ResetCurrentRendererRegistry();

                    var argsArray = (JArray)state;
                    JSInterop.DotNetDispatcher.BeginInvoke(
                        (string)argsArray[0],
                        (string)argsArray[1],
                        (string)argsArray[2],
                        (long)argsArray[3],
                        (string)argsArray[4]);
                }, args);
            });

            try
            {
                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await callback();
                    }
                    catch (Exception ex)
                    {
                        while (ex.InnerException != null)
                        {
                            ex = ex.InnerException;
                        }

                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        //Electron.App.Exit(1);
                    }
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

                electronProcess.Process.WaitForExit();
                electronSynchronizationContext.Stop();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(current);
            }
        }

        public static async Task<BrowserWindow> CreateWindowAsync(string relativePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), relativePath));
            var url = "file:///" + fullPath.Replace('\\', '/');

            var window = await ElectronNET.API.Electron.WindowManager.CreateWindowAsync(url);
            ElectronJSRuntime = new ElectronJSRuntime(window);

            ElectronSynchronizationContext.UnhandledException += (sender, ex) =>
            {
                ElectronNET.API.Electron.IpcMain.Send(window, "JS.Error", ex.ToString());
                throw ex;
            };

            // Do a two-way handshake with the browser, this ensures that the appropriate
            // interop handlers have been set up before control returns to the user.
            //
            // The handshake sequence looks like this:
            //
            // 1. dotnet starts listening for components:init
            // 2. dotnet sends components:init repeatedly
            // 3. electron starts listening for components:init
            // 4. electron sends a components:init once it has received one from dotnet - it's ready
            // 5. dotnet receives components:init - it's ready
            //
            // Because either side might take any amount of time to start listening,
            // step 3 can occur at any point prior to step 4. The whole process works
            // because steps 1, 2, 4, and 5 can only occur in that order

            var cts = new CancellationTokenSource();
            var incomingHandshakeCancellationToken = cts.Token;
            ElectronNET.API.Electron.IpcMain.Once("components:init", args =>
            {
                var argsArray = (JArray)args;
                InitialUriAbsolute = (string)argsArray[0];
                BaseUriAbsolute = (string)argsArray[1];
                cts.Cancel();
            });

            Log("Waiting for interop connection");
            while (!incomingHandshakeCancellationToken.IsCancellationRequested)
            {
                ElectronNET.API.Electron.IpcMain.Send(window, "components:init");

                try
                {
                    await Task.Delay(100, incomingHandshakeCancellationToken);
                }
                catch (TaskCanceledException)
                {
                }
            }

            Log("Interop connected");

            return window;
        }

        public static string GetProjectRoot()
        {
            // TODO: During build, write the project root to some kind of manifest/config file
            // That way it can be different after publishing and not dependent on the existence of a .csproj
            var startDir = Directory.GetCurrentDirectory();
            var dir = startDir;
            while (!string.IsNullOrEmpty(dir))
            {
                if (Directory.GetFiles(dir, "*.csproj").Length > 0)
                {
                    return dir;
                }

                dir = Path.GetDirectoryName(dir);
            }

            throw new InvalidOperationException($"Could not find any .csproj in or above '{startDir}'");
        }

        private static string GetElectronPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "electron.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "electron";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "Electron.app/Contents/MacOS/Electron";
            }
            else
            {
                throw new PlatformNotSupportedException("Unrecognized platform");
            }
        }

        // Finds a randomized, available port
        private static int SelectPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);

            try
            {
                listener.Start();
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }

        private static void Log(string message)
        {
            var process = Process.GetCurrentProcess();
            Console.WriteLine($"[{process.ProcessName}:{process.Id}] out: " + message);
        }

        class FakeWebHostBuilder : IWebHostBuilder
        {
            public IWebHost Build()
                => throw new NotImplementedException();

            public IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
                => throw new NotImplementedException();

            public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
                => throw new NotImplementedException();

            public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
                => throw new NotImplementedException();

            public string GetSetting(string key)
                => throw new NotImplementedException();

            public IWebHostBuilder UseSetting(string key, string value) => this;
        }
    }
}
