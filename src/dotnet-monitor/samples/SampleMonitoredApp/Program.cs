// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SampleMonitoredApp
{
    internal class Program
    {
        private const long KB = 1024;
        private const long MB = 1024 * 1024;
        private const long GB = 1024 * 1024 * 1024;
        private const int MaxSizeFactor = 8;

        private static readonly Random _rando = new Random();

        private static void Main(string[] args)
        {
            var logging = new LoggerFactory();
            logging.AddEventSourceLogger();

            var logger = logging.CreateLogger<Program>();

            DumpEventPipeInfo();

            Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");
            Console.WriteLine($"AppName: {Assembly.GetEntryAssembly().GetName().Name}");
            Console.WriteLine($"AppDomain Base: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"AppBase: {AppContext.BaseDirectory}");
            Console.WriteLine("Ready to start emitting events.");
            Console.WriteLine("Press X to quit.");
            Console.WriteLine("Press A to allocate 100 MB.");
            Console.WriteLine("Press G to force a GC.");
            Console.WriteLine("Press T to spawn parallel tasks.");
            Console.WriteLine("Press L to log a Microsoft.Extensions.Logging message to 'SampleMonitoredApp.Program'.");
            Console.WriteLine("Press E to write a random value to an EventSource/EventCounter.");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                switch (key.Key)
                {
                    case ConsoleKey.X:
                        return;
                    case ConsoleKey.A:
                        AllocateMemory();
                        break;
                    case ConsoleKey.G:
                        GC.Collect();
                        Console.WriteLine($"Total Memory after collection: {FormatSize(GC.GetTotalMemory(forceFullCollection: false))}");
                        break;
                    case ConsoleKey.T:
                        SpawnTasks();
                        break;
                    case ConsoleKey.L:
                        logger.Log(LogLevel.Information, new EventId(42, "SampleEvent"), "This is a sample event with an argument: {rando}", _rando.Next(0, 100));
                        break;
                    case ConsoleKey.E:
                        SampleEventSource.Log.MyEvent(_rando.Next(0, 100));
                        break;
                }
            }
        }

        private static void DumpEventPipeInfo()
        {
            var type = typeof(ValueType).Assembly.GetType("System.Diagnostics.Tracing.EventPipeController");
            if (type == null)
            {
                throw new InvalidOperationException("Could not find EventPipeController type!");
            }

            var instanceField = type.GetField("s_controllerInstance", BindingFlags.NonPublic | BindingFlags.Static);
            if (instanceField == null)
            {
                throw new InvalidOperationException("Could not find EventPipeController.s_controllerInstance field!");
            }

            var instance = instanceField.GetValue(null);
            if (instance == null)
            {
                throw new InvalidOperationException("EventPipeController.s_controllerInstance is null!");
            }

            var pathField = type.GetField("m_configFilePath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pathField == null)
            {
                throw new InvalidOperationException("Could not find EventPipeController.m_configFilePath field!");
            }

            Console.WriteLine($"EventPipe Config File Path: {pathField.GetValue(instance)}");
        }

        private static void SpawnTasks()
        {
            var tasks = new Task[100];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = RunTask(i);
            }

            async Task RunTask(int index)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * _rando.Next(0, 10)));
            }
        }

        private static void AllocateMemory()
        {
            var buffer = new byte[100 * 1024 * 1024];
            Console.WriteLine("Allocated 100 MB");
            Console.WriteLine($"Total Memory: {FormatSize(GC.GetTotalMemory(forceFullCollection: false))}");
        }

        private static void EmitEvent()
        {
            SampleEventSource.Log.MyEvent(_rando.Next());
        }

        private static string FormatSize(long v)
        {
            if ((v < (MaxSizeFactor * KB)))
            {
                return $"{v} bytes";
            }
            else if ((v < (MaxSizeFactor * MB)))
            {
                return $"{v / KB:0.00}KB";
            }
            else if ((v < (MaxSizeFactor * GB)))
            {
                return $"{v / MB:0.00}MB";
            }
            else
            {
                return $"{v / GB:0.00}GB";
            }
        }
    }

    [EventSource(Name = "Sample-EventSource")]
    public class SampleEventSource : EventSource
    {
        public static readonly SampleEventSource Log = new SampleEventSource();
        private readonly EventCounter _sampleCounter;

        private SampleEventSource()
        {
            _sampleCounter = new EventCounter("SampleCounter", this);
        }

        [Event(1, Message = "My event with payload {0}")]
        public void MyEvent(int rando)
        {
            WriteEvent(1, rando);
            _sampleCounter.WriteMetric(rando);
        }
    }
}
