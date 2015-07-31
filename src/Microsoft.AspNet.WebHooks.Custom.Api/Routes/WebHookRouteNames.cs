// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.WebHooks.Routes
{
    /// <summary>
    /// Provides a set of common route names used by the custom WebHooks Web API controllers.
    /// </summary>
    internal static class WebHookRouteNames
    {
        /// <summary>
        /// Provides the name of the <see cref="Controllers.WebHookFiltersController"/> GET action.
        /// </summary>
        public const string FiltersGetAction = "FiltersGetAction";

        /// <summary>
        /// Provides the name of the <see cref="Controllers.WebHookRegistrationsController"/> lookup action.
        /// </summary>
        public const string RegistrationLookupAction = "RegistrationLookupAction";
    }
}
