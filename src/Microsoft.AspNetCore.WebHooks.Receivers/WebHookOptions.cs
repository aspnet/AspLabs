// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Provides programmatic configuration for the WebHook infrastructure.
    /// </summary>
    public class WebHookOptions
    {
        /// <summary>
        /// Gets the list of action parameter <see cref="Type"/>s the
        /// <see cref="ModelBinding.WebHookHttpContextModelBinder"/> can be associated with. Parameters assignable to
        /// the given types may be model bound from <see cref="HttpContext.Items"/> entries.
        /// </summary>
        /// <value>
        /// Default collection includes <see cref="IFormCollection"/>, <see cref="JArray"/>, <see cref="JObject"/>, and
        /// <see cref="XElement"/>.
        /// </value>
        public IList<Type> HttpContextItemsTypes { get; } = new List<Type>
        {
            typeof(IFormCollection),
            typeof(JArray),
            typeof(JObject),
            typeof(XElement),
        };
    }
}
