// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests
{
    public class GrpcHttpApiServiceExtensionsTests
    {
        [Fact]
        public void AddGrpcHttpApi_DefaultOptions_PopulatedProperties()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddGrpcHttpApi();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options1 = serviceProvider.GetRequiredService<IOptions<GrpcHttpApiOptions>>().Value;

            Assert.NotNull(options1.JsonFormatter);
            Assert.NotNull(options1.JsonParser);

            var options2 = serviceProvider.GetRequiredService<IOptions<GrpcHttpApiOptions>>().Value;

            Assert.Equal(options1, options2);
        }

        [Fact]
        public void AddGrpcHttpApi_OverrideOptions_OptionsApplied()
        {
            // Arrange
            var jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(formatDefaultValues: false));
            var jsonParser = new JsonParser(new JsonParser.Settings(recursionLimit: 1));

            var services = new ServiceCollection();

            // Act
            services.AddGrpcHttpApi(o =>
            {
                o.JsonFormatter = jsonFormatter;
                o.JsonParser = jsonParser;
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<GrpcHttpApiOptions>>().Value;

            Assert.Equal(jsonFormatter, options.JsonFormatter);
            Assert.Equal(jsonParser, options.JsonParser);
        }
    }
}
