﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components.Builder;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Components.Electron
{
    internal class ElectronApplicationBuilder : IComponentsApplicationBuilder
    {
        public ElectronApplicationBuilder(IServiceProvider services)
        {
            Services = services;
            Entries = new List<(Type componentType, string domElementSelector)>();
        }

        public List<(Type componentType, string domElementSelector)> Entries { get; }

        public IServiceProvider Services { get; }

        public void AddComponent(Type componentType, string domElementSelector)
        {
            if (componentType == null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }

            if (domElementSelector == null)
            {
                throw new ArgumentNullException(nameof(domElementSelector));
            }

            Entries.Add((componentType, domElementSelector));
        }
    }
}
