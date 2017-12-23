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
                throw new ArgumentNullException(nameof(receivers));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _receiverLookup = receivers
               .GroupBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
               .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
            _logger = logger;

            var providerList = string.Join(", ", _receiverLookup.Keys);
            var message = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Manager_RegisteredNames, typeof(IWebHookReceiver).Name, providerList);
            _logger.Info(message);
        }

        /// <inheritdoc />
        public IWebHookReceiver GetReceiver(string receiverName)
        {
            if (receiverName == null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            if (!_receiverLookup.TryGetValue(receiverName, out var matches))
            {
                var message = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Manager_UnknownReceiver, receiverName);
                _logger.Info(message);
                return null;
            }
            else if (matches.Count > 1)
            {
                var providerList = string.Join(Environment.NewLine, matches.Select(p => p.GetType()));
                var message = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Manager_MultipleAmbiguousReceiversFound, receiverName, Environment.NewLine, providerList);
                _logger.Error(message);
                throw new InvalidOperationException(message);
            }
            return matches.First();
        }
    }
}
