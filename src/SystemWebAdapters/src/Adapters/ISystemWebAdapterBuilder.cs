// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace System.Web.Adapters;

public interface ISystemWebAdapterBuilder
{
    IServiceCollection Services { get; }
}
