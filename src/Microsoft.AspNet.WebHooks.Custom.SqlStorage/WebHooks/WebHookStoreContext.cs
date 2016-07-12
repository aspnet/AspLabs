// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information

using System;
using System.Data.Entity;
using Microsoft.AspNet.WebHooks.Storage;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Defines a <see cref="DbContext"/> which contains the set of WebHook <see cref="Registration"/> instances.
    /// </summary>
    public class WebHookStoreContext : DbContext
    {
        internal const string ConnectionStringName = "MS_SqlStoreConnectionString";
        private const string ConnectionStringNameParameter = "name=" + ConnectionStringName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookStoreContext"/> class.
        /// </summary>
        public WebHookStoreContext() : base(ConnectionStringNameParameter)
        {
        }

        /// <summary>
        /// Gets or sets the current collection of <see cref="Registration"/> instances.
        /// </summary>
        public virtual DbSet<Registration> Registrations { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.HasDefaultSchema("WebHooks");
        }
    }
}
