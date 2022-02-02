// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;

namespace Microsoft.AspNetCore.Grpc.Microbenchmarks
{
    internal class DefaultCoreConfig : ManualConfig
    {
        public DefaultCoreConfig()
        {
            AddLogger(ConsoleLogger.Default);
            AddExporter(MarkdownExporter.GitHub);

            AddDiagnoser(MemoryDiagnoser.Default);
            AddColumn(StatisticColumn.OperationsPerSecond);
            AddColumnProvider(DefaultColumnProviders.Instance);

            AddValidator(JitOptimizationsValidator.FailOnError);

            AddJob(Job.Default
                .WithToolchain(CsProjCoreToolchain.From(new NetCoreAppSettings("net6.0", null, ".NET 6")))
                .WithGcMode(new GcMode { Server = true })
                .WithStrategy(RunStrategy.Throughput));
        }
    }
}
