// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Config
{
    public class SettingsProviderTests
    {
        [Fact]
        public void GetSettings_CallsInitializeSettingsAndReturnsResult()
        {
            // Arrange
            SettingsDictionary settings = new SettingsDictionary();
            Mock<SettingsProvider> settingsProviderMock = new Mock<SettingsProvider>() { CallBase = true };
            settingsProviderMock.Protected()
                .Setup<SettingsDictionary>("InitializeSettings")
                .Returns(settings)
                .Verifiable();

            // Act
            SettingsDictionary actualSettings = settingsProviderMock.Object.GetSettings();

            // Assert
            Assert.Same(settings, actualSettings);
            settingsProviderMock.Verify();
        }

        [Fact]
        public void GetSettings_ReturnsSameInstance()
        {
            // Arrange
            SettingsProvider settingsProvider = new SettingsProvider();

            // Act
            SettingsDictionary settings1 = settingsProvider.GetSettings();
            SettingsDictionary settings2 = settingsProvider.GetSettings();

            // Assert
            Assert.Same(settings1, settings2);
        }

        [Fact]
        public void GetSettings_SetsCustomProperties()
        {
            // Arrange
            Mock<SettingsProvider> providerMock = GetProviderMock();

            // Act
            SettingsDictionary actual = providerMock.Object.GetSettings();

            // Assert
            Assert.Equal(actual["SampleKey"], "你好世界");
        }

        private static Mock<SettingsProvider> GetProviderMock()
        {
            Mock<SettingsProvider> settingsProviderMock = new Mock<SettingsProvider>() { CallBase = true };
            NameValueCollection appSettings = new NameValueCollection();
            appSettings["SampleKey"] = "你好世界";
            settingsProviderMock.Protected()
                .Setup<NameValueCollection>("GetAppSettings")
                .Returns(appSettings)
                .Verifiable();

            return settingsProviderMock;
        }
    }
}
