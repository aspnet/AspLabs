// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using AutoFixture;
using Xunit;

namespace System.Web.Internal;

public class HttpValueCollectionTests
{
    private readonly Fixture _fixture;

    public HttpValueCollectionTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void EmptyValue()
    {
        var str = string.Empty;
        var collection = new HttpValueCollection(str, Encoding.UTF8);

        Assert.Empty(collection);
    }

    [Fact]
    public void RoundtripCookieString()
    {
        // Arrange
        var key1 = _fixture.Create<string>();
        var value1 = _fixture.Create<string>();
        var key2 = _fixture.Create<string>();
        var value2 = _fixture.Create<string>();
        var cookieString = $"{key1}={value1}&{key2}={value2}";

        var collection = new HttpValueCollection(cookieString, Encoding.UTF8);

        // Act
        var retrieved1 = collection[key1];
        var retrieved2 = collection[key2];

        // Assert
        Assert.Equal(value1, retrieved1);
        Assert.Equal(value2, retrieved2);
    }

    [Fact]
    public void AddItems()
    {
        // Arrange
        var key1 = _fixture.Create<string>();
        var value1 = _fixture.Create<string>();
        var key2 = _fixture.Create<string>();
        var value2 = _fixture.Create<string>();

        var collection = new HttpValueCollection(string.Empty, Encoding.UTF8)
        {
            { key1, value1 },
            { key2, value2 },
        };

        // Act
        var str = collection.ToString();

        // Assert
        Assert.Equal($"{key1}={value1}&{key2}={value2}", str);
    }

    [Fact]
    public void ToStringEmpty()
    {
        // Arrange
        var collection = new HttpValueCollection();

        // Act
        var str = collection.ToString();

        // Assert
        Assert.Same(string.Empty, str);
    }

    [Fact]
    public void SingleValueNoKey()
    {
        // Arrange
        var value1 = _fixture.Create<string>();
        var value2 = _fixture.Create<string>();
        var collection = new HttpValueCollection
        {
            { null,  value1 },
            { null,  value2 },
        };

        // Act
        var str = collection.ToString();

        // Assert
        Assert.Equal($"{value1}&{value2}", str);
    }

    [Fact]
    public void TrailingAmpersand()
    {
        // Arrange
        var key1 = _fixture.Create<string>();
        var value1 = _fixture.Create<string>();
        var key2 = _fixture.Create<string>();
        var value2 = _fixture.Create<string>();
        var collection = new HttpValueCollection($"{key1}={value1}&{key2}={value2}&");

        // Act
        var str = collection.ToString();

        // Assert
        Assert.Equal(3, collection.Count);
        Assert.Equal(value1, collection[key1]);
        Assert.Equal(value2, collection[key2]);
        Assert.Same(string.Empty, collection[null]);
    }
}
