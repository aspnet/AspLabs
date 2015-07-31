// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Config
{
    public class ConnectionSettingsTests
    {
        private const string ConnectionName = "ConName";
        private const string ConnectionString = "ConString";

        private ConnectionSettings _settings = new ConnectionSettings(ConnectionName, ConnectionString);

        [Fact]
        public void Name_Roundtrips()
        {
            PropertyAssert.Roundtrips(_settings, s => s.Name, PropertySetter.NullThrows, defaultValue: ConnectionName, roundtripValue: "Value");
        }

        [Fact]
        public void ConnectionString_Roundtrips()
        {
            PropertyAssert.Roundtrips(_settings, s => s.ConnectionString, PropertySetter.NullThrows, defaultValue: ConnectionString, roundtripValue: "Value");
        }

        [Fact]
        public void Provider_Roundtrips()
        {
            PropertyAssert.Roundtrips(_settings, s => s.Provider, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }
    }
}
