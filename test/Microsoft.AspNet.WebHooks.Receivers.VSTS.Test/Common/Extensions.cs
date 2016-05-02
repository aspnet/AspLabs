// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.AspNet.WebHooks
{
    internal static class Extensions
    {
        public static DateTime ToDateTime(this string self)
        {
            return DateTime.Parse(self, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        }
    }
}
