// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Manages registered <see cref="IWebHookReceiver"/> instances.
    /// </summary>
    public class WebHookReceiverManager : IWebHookReceiverManager
    {
        private readonly ILogger _logger;
        private readonly IDictionary<string, List<IWebHookReceiver>> _receiverLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookReceiverManager"/> class with the given <paramref name="receivers"/>
        /// and <paramref name="logger"/>.
        /// </summary>
        public WebHookReceiverManager(IEnumerable<IWebHookReceiver> receivers, ILogger logger)
        {
            if (receivers == null)
            {
                throw new ArgumentNullException("receivers");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _receiverLookup = receivers
               .GroupBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
               .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
            _logger = logger;
        }

        /// <inheritdoc />
        public IWebHookReceiver GetReceiver(string receiverName)
        {
            if (receiverName == null)
            {
                throw new ArgumentNullException("receiverName");
            }

            List<IWebHookReceiver> matches;
            if (!_receiverLookup.TryGetValue(receiverName, out matches))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Manager_UnknownReceiver, receiverName);
                _logger.Info(msg);
                return null;
            }
            else if (matches.Count > 1)
            {
                string providerList = string.Join(Environment.NewLine, matches.Select(p => p.GetType()));
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Manager_MultipleAmbiguousReceiversFound, receiverName, Environment.NewLine, providerList);
                _logger.Error(msg);
                throw new InvalidOperationException(msg);
            }
            return matches.First();
        }
    }
}
