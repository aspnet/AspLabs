// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Caching
{
    public enum CacheItemRemovedReason
    {
        Removed = 1,
        Expired,
        Underused,
        DependencyChanged
    }
}
