using System;
using AutoFixture;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal;

public class ServerVariablesNameValueCollectionTests
{
    private readonly Fixture _fixture;

    public ServerVariablesNameValueCollectionTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void PlatformNotSupportedExceptions()
    {
        var feature = new Mock<IServerVariablesFeature>();
        var collection = new ServerVariablesNameValueCollection(feature.Object);

        Assert.Throws<PlatformNotSupportedException>(() => collection.Keys);
        Assert.Throws<PlatformNotSupportedException>(() => collection.Get(0));
        Assert.Throws<PlatformNotSupportedException>(() => collection.GetValues(0));
        Assert.Throws<PlatformNotSupportedException>(() => collection.GetKey(0));
        Assert.Throws<PlatformNotSupportedException>(() => collection.AllKeys);
        Assert.Throws<PlatformNotSupportedException>(() => collection.Count);
        Assert.Throws<PlatformNotSupportedException>(() => collection.Clear());
        Assert.Throws<PlatformNotSupportedException>(() => collection.GetEnumerator());
    }

    [Fact]
    public void AddGetValue()
    {
        // Arrange
        var feature = new Mock<IServerVariablesFeature>();
        var collection = new ServerVariablesNameValueCollection(feature.Object);

        var newKey = _fixture.Create<string>();
        var newValue1 = _fixture.Create<string>();
        var newValue2 = _fixture.Create<string>();

        // Act
        collection.Add(newKey, newValue1);
        collection.Add(newKey, newValue2);

        // Assert
        feature.VerifySet(f => f[newKey] = newValue1, Times.Once);
        feature.VerifySet(f => f[newKey] = newValue2, Times.Once);
    }

    [Fact]
    public void SetValue()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = _fixture.Create<string>();

        var feature = new Mock<IServerVariablesFeature>();
        feature.Setup(f => f[key]).Returns(value);

        var collection = new ServerVariablesNameValueCollection(feature.Object);

        var newValue = _fixture.Create<string>();

        // Act
        collection.Set(key, newValue);

        // Assert
        feature.VerifySet(f => f[key] = newValue);
    }

    [Fact]
    public void GetKey()
    {
        // Arrange
        var feature = new Mock<IServerVariablesFeature>();
        var key = _fixture.Create<string>();
        var value = _fixture.Create<string>();
        feature.Setup(f => f[key]).Returns(value);

        var collection = new ServerVariablesNameValueCollection(feature.Object);

        // Act
        var result = collection.Get(key);
        var results = collection.GetValues(key);

        // Assert
        Assert.Equal(value, result);
        Assert.Collection(results, r => Assert.Equal(value, r));
    }

    [Fact]
    public void GetKeyNull()
    {
        // Arrange
        var feature = new Mock<IServerVariablesFeature>();
        var collection = new ServerVariablesNameValueCollection(feature.Object);

        // Act
        var result = collection.Get(null);
        var results = collection.GetValues(null);

        // Assert
        Assert.Null(result);
        Assert.Null(results);
    }

    [Fact]
    public void GetKeyNotPresent()
    {
        // Arrange
        var feature = new Mock<IServerVariablesFeature>();
        var collection = new ServerVariablesNameValueCollection(feature.Object);

        // Act
        var result = collection.Get(_fixture.Create<string>());
        var results = collection.GetValues(_fixture.Create<string>());

        // Assert
        Assert.Null(result);
        Assert.Null(results);
    }

    [Fact]
    public void HandleNullKey()
    {
        // Arrange
        var feature = new Mock<IServerVariablesFeature>();
        var collection = new ServerVariablesNameValueCollection(feature.Object);

        // Act
        collection.Add(null, _fixture.Create<string>());

        // Assert
        feature.VerifyNoOtherCalls();
    }

    [Fact]
    public void Remove()
    {
        // Arrange
        var key = _fixture.Create<string>();
        var value = _fixture.Create<string>();

        var feature = new Mock<IServerVariablesFeature>();
        feature.Setup(f => f[key]).Returns(value);

        var collection = new ServerVariablesNameValueCollection(feature.Object);

        // Act
        collection.Remove(key);

        // Assert
        feature.VerifySet(f => f[key] = null, Times.Once);
    }
}
