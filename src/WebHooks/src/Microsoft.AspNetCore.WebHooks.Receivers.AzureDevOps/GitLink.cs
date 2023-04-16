// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// The link.
    /// </summary>
    public class GitLink
    {
        /// <summary>
        /// The url.
        /// </summary>
        [JsonProperty("href")]
        public Uri Href { get; set; }
    }
}