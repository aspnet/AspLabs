// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;
using Autofac.Extras.Moq;
using Moq;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;

namespace System.Web.Adapters;

public class PreBufferRequestStreamMiddlewareTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task RequestBuffering(bool isEnabled)
    {
        // Arrange
        var next = new Mock<RequestDelegate>();
        using var mock = AutoMock.GetLoose(c => c.RegisterMock(next));

        var logger = new Mock<ILogger<PreBufferRequestStreamMiddleware>>();

        var metadata = new Mock<IPreBufferRequestStreamMetadata>();
        metadata.Setup(m => m.IsEnabled).Returns(isEnabled);

        var metadataCollection = new EndpointMetadataCollection(metadata.Object);

        var endpointFeature = new Mock<IEndpointFeature>();
        endpointFeature.Setup(e => e.Endpoint).Returns(new Endpoint(null, metadataCollection, null));

        var stream = new Mock<Stream>();

        var requestFeature = new Mock<IHttpRequestFeature>();
        requestFeature.SetupProperty(r => r.Body);
        requestFeature.Object.Body = stream.Object;

        var responseFeature = new Mock<IHttpResponseFeature>();

        var features = new Mock<IFeatureCollection>();
        features.Setup(f => f.Get<IEndpointFeature>()).Returns(endpointFeature.Object);
        features.Setup(f => f.Get<IHttpRequestFeature>()).Returns(requestFeature.Object);
        features.Setup(f => f.Get<IHttpResponseFeature>()).Returns(responseFeature.Object);

        var context = new DefaultHttpContext(features.Object);

        // Act
        await mock.Create<PreBufferRequestStreamMiddleware>().InvokeAsync(context);

        // Assert
        Assert.Equal(isEnabled, context.Request.Body.CanSeek);
    }
}
