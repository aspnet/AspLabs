// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace System.Web.Adapters;

public class MigrationWebApplicationOptions
{
#if NET6_0_OR_GREATER
    public static WebApplicationOptions CreateWithRelativeContentRoute(string[] args)
    {
        if (args.Length > 0)
        {
            var config = new ConfigurationBuilder()
                           .AddCommandLine(args)
                           .Build();

            if (config[WebHostDefaults.ContentRootKey] is { } contentRoot)
            {
                contentRoot = Path.GetFullPath(contentRoot);

                var newArgs = config.AsEnumerable()
                    .Where(t => t.Key != WebHostDefaults.ContentRootKey)
                    .SelectMany(t => new[] { $"--{t.Key}", t.Value })
                    .ToArray();

                return new()
                {
                    Args = newArgs,
                    ContentRootPath = contentRoot,
                };
            }
        }

        return new() { Args = args, };
    }
#endif
}
