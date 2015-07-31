// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for getting the User ID from a given <see cref="IPrincipal"/>. The User ID 
    /// is used to identify which user a given <see cref="WebHook"/> belongs to.
    /// </summary>
    public interface IWebHookUser
    {
        /// <summary>
        /// Gets the user ID for a given <paramref name="user"/>. The user ID is used to uniquely 
        /// identify a user so that only events coming from actions of that user generates WebHooks
        /// registered for that user.
        /// </summary>
        /// <param name="user">The <see cref="IPrincipal"/> to get the user ID from.</param>
        /// <returns>The user ID.</returns>
        Task<string> GetUserIdAsync(IPrincipal user);
    }
}
