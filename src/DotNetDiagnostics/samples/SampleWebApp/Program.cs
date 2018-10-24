// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SampleWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");
            Console.WriteLine($"AppBase: {AppContext.BaseDirectory}");
            DumpEventPipeInfo();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static void DumpEventPipeInfo()
        {
            var type = typeof(ValueType).Assembly.GetType("System.Diagnostics.Tracing.EventPipeController");
            if (type == null)
            {
                Console.Error.WriteLine("Could not find EventPipeController type!");
                return;
            }

            var instanceField = type.GetField("s_controllerInstance", BindingFlags.NonPublic | BindingFlags.Static);
            if (instanceField == null)
            {
                Console.Error.WriteLine("Could not find EventPipeController.s_controllerInstance field!");
                return;
            }

            var instance = instanceField.GetValue(null);
            if (instance == null)
            {
                Console.Error.WriteLine("EventPipeController.s_controllerInstance is null!");
                return;
            }

            var pathField = type.GetField("m_configFilePath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pathField == null)
            {
                Console.Error.WriteLine("Could not find EventPipeController.m_configFilePath field!");
                return;
            }

            Console.WriteLine($"EventPipe Config File Path: {pathField.GetValue(instance)}");
        }
    }
}
