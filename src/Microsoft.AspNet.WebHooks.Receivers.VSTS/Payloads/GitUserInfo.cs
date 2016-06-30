// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Display information about a git user
    /// </summary>
    public class GitUserInfo
    {
        /// <summary>
        /// Name of the user
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Email of the user.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// A date associated with the user.
        /// </summary>
        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }
}