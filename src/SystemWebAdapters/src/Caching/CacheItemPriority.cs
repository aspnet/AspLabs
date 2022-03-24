// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Caching;

public enum CacheItemPriority
{
    Low = 1,
    BelowNormal,
    Normal,
    AboveNormal,
    High,
    NotRemovable,
    Default = Normal
}
