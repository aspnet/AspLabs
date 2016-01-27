// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.WebHooks.Serialization
{
    /// <summary>
    /// Converts the Instagram string representation of a Unix time stamp to and from a <see cref="DateTime"/>.
    /// </summary>
    internal class InstagramUnixTimeConverter : UnixTimeConverter
    {
        /// <inheritdoc />
        public InstagramUnixTimeConverter() : base(true)
        {
        }
    }
}
