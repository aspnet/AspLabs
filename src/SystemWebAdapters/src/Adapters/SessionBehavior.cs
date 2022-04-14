// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Adapters;

public enum SessionBehavior
{
    None,
    Eager,

    [Obsolete("This will enable session on the endpoint but will resort to async over sync behavior")]
    OnDemand,
}
