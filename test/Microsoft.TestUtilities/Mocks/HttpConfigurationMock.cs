// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Moq;

namespace Microsoft.TestUtilities.Mocks
{
    public static class HttpConfigurationMock
    {
        /// <summary>
        /// Returns a <see cref="HttpConfiguration"/> with the <see cref="IDependencyResolver"/> set to 
        /// serve the service instances provided by the <paramref name="getServiceInstance"/>.
        /// </summary>
        /// <param name="getServiceInstance">Delegate to lookup the required service instance.</param>
        /// <returns>A newly configured <see cref="HttpConfiguration"/> instance.</returns>
        public static HttpConfiguration Create(Func<Type, object> getServiceInstance = null, Func<Type, IEnumerable<object>> getServiceInstances = null)
        {
            HttpConfiguration config = new HttpConfiguration();
            if (getServiceInstance == null)
            {
                getServiceInstance = type => null;
            }
            if (getServiceInstances == null)
            {
                getServiceInstances = type => Enumerable.Empty<object>();
            }

            // Set dependency resolver
            Mock<IDependencyResolver> dependencyResolverMock = new Mock<IDependencyResolver>();
            dependencyResolverMock.Setup(d => d.GetService(It.IsAny<Type>()))
                .Returns<Type>(t => getServiceInstance(t));
            dependencyResolverMock.Setup(d => d.GetServices(It.IsAny<Type>()))
                .Returns<Type>(t => getServiceInstances(t));
            config.DependencyResolver = dependencyResolverMock.Object;

            return config;
        }

        /// <summary>
        /// Returns a <see cref="HttpConfiguration"/> with the <see cref="IDependencyResolver"/> set to 
        /// serve the service instances provided.
        /// </summary>
        /// <param name="serviceInstances">Set of service instances to return from dependency resolver.</param>
        /// <returns>A newly configured <see cref="HttpConfiguration"/> instance.</returns>
        public static HttpConfiguration Create(IEnumerable<KeyValuePair<Type, object>> serviceInstances)
        {
            HttpConfiguration config = new HttpConfiguration();

            // Set dependency resolver
            Mock<IDependencyResolver> dependencyResolverMock = new Mock<IDependencyResolver>();
            dependencyResolverMock.Setup(d => d.GetService(It.IsAny<Type>()))
                .Returns<Type>(t => serviceInstances.FirstOrDefault(kvp => kvp.Key == t).Value);
            dependencyResolverMock.Setup(d => d.GetServices(It.IsAny<Type>()))
                .Returns<Type>(t => serviceInstances.Where(kvp => kvp.Key == t).Select(kvp => kvp.Value));
            config.DependencyResolver = dependencyResolverMock.Object;

            return config;
        }
    }
}
