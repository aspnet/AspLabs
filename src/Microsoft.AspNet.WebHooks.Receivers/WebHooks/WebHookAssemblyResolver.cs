// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides a custom <see cref="IAssembliesResolver"/> implementation that ensures that the WebHooks receiver 
    /// assemblies are loaded. This is useful when running ASP.NET Web API using the OWIN Self Host as there 
    /// assemblies have to be loaded explicitly. For more information on running Web API in a self host, please see 
    /// '<c>http://www.asp.net/web-api/overview/hosting-aspnet-web-api/use-owin-to-self-host-web-api</c>'.
    /// </summary>
    public class WebHookAssemblyResolver : DefaultAssembliesResolver
    {
        /// <inheritdoc />
        public override ICollection<Assembly> GetAssemblies()
        {
            ICollection<Assembly> baseAssemblies = base.GetAssemblies();
            List<Assembly> assemblies = new List<Assembly>(baseAssemblies);

            // Add current if not already added
            Assembly current = Assembly.GetExecutingAssembly();
            if (!assemblies.Contains(current))
            {
                assemblies.Add(current);
            }
            return assemblies;
        }
    }
}
