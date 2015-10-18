// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Describes a collection of Instagram WebHook event notifications as received from Instagram.
    /// For details about Instagram WebHooks, please see <c>https://instagram.com/developer/realtime/</c>.
    /// </summary>
    public class InstagramNotificationCollection : Collection<InstagramNotification>
    {
    }
}
