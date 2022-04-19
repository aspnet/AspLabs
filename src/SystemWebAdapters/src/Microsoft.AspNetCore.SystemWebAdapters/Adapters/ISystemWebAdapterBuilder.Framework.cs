// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public interface ISystemWebAdapterBuilder
{
    ICollection<IHttpModule> Modules { get; }
}
