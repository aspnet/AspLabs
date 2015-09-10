// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public abstract class WebHookReceiverTestsBase<T>
        where T : class, IWebHookReceiver
    {
        internal const string SecretPrefix = "MS_WebHookReceiverSecret_";

        protected WebHookReceiverTestsBase()
        {
        }

        public static TheoryData<string> ValidIdData
        {
            get
            {
                return new TheoryData<string>
                {
                    { string.Empty },
                    { "id" },
                    { "你好" },
                    { "1" },
                    { "1234567890" },
                };
            }
        }

        protected ILogger Logger { get; set; }

        protected SettingsDictionary Settings { get; set; }

        protected IWebHookReceiverConfig ReceiverConfig { get; set; }

        protected HttpConfiguration HttpConfig { get; set; }

        protected HttpRequestContext RequestContext { get; set; }

        protected Mock<T> ReceiverMock { get; set; }

        protected T Receiver { get; set; }

        public static string GetConfigValue(string id, string config)
        {
            return id + " = " + config;
        }

        public virtual void Initialize(string config)
        {
            Initialize(null, config);
        }

        public virtual void Initialize(string name, string config)
        {
            if (ReceiverMock == null)
            {
                ReceiverMock = new Mock<T> { CallBase = true };
            }
            Receiver = ReceiverMock.Object;
            name = name ?? Receiver.Name; 

            Logger = new Mock<ILogger>().Object;
            Settings = new SettingsDictionary();
            Settings[SecretPrefix + name] = config;

            ReceiverConfig = new WebHookReceiverConfig(Settings, Logger);

            HttpConfig = HttpConfigurationMock.Create(new Dictionary<Type, object>
            {
                { typeof(IWebHookReceiverConfig), ReceiverConfig },
                { typeof(SettingsDictionary), Settings }
            });

            RequestContext = new HttpRequestContext { Configuration = HttpConfig };
        }
    }
}
