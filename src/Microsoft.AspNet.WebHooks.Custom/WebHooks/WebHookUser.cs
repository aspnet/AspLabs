// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides a default implementation of <see cref="IWebHookUser"/> for getting information about a user.
    /// </summary>
    public class WebHookUser : IWebHookUser
    {
        private const string DefaultClaimsType = ClaimTypes.Name;

        private static string _claimsType = DefaultClaimsType;

        /// <summary>
        /// Gets or sets the claims type which is used to get the user ID from the <see cref="IPrincipal"/>.
        /// The default value is <see cref="ClaimTypes.Name"/> but can be set to any non-null string
        /// representing the user ID.
        /// </summary>
        public static string IdClaimsType
        {
            get
            {
                return _claimsType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _claimsType = value;
            }
        }

        /// <inheritdoc />
        public Task<string> GetUserIdAsync(IPrincipal user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            string id = null;
            ClaimsPrincipal principal = user as ClaimsPrincipal;
            if (principal != null)
            {
                Claim claim = principal.FindFirst(_claimsType);
                if (claim != null)
                {
                    id = claim.Value;
                }
            }

            // Fall back to name property
            if (id == null && user.Identity != null)
            {
                id = user.Identity.Name;
            }

            if (id == null)
            {
                string msg = CustomResources.Manager_NoUser;
                throw new InvalidOperationException(msg);
            }

            return Task.FromResult(id);
        }

        /// <summary>
        /// For testing purposes
        /// </summary>
        internal static void Reset()
        {
            _claimsType = DefaultClaimsType;
        }
    }
}
