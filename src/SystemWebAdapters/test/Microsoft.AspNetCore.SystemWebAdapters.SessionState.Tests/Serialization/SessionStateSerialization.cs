// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Text;
using Xunit;

using KeyDictionary = System.Collections.Generic.Dictionary<string, System.Type>;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

public class SessionStateSerialization
{
    [Fact]
    public void NewSession()
    {
        // Arrange
        const string PayLoad = @"{
    ""n"": true,
}";
        var serializer = new SessionSerializer(new KeyDictionary());

        // Act
        var result = serializer.Deserialize(PayLoad);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result!.Count);
        Assert.True(result.IsNewSession);
    }

    [Fact]
    public void SingleValueInt()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": 5
    }
}";

        var serializer = new SessionSerializer(new KeyDictionary
        {
            { "Key1", typeof(int) }
        });

        // Act
        var result = serializer.Deserialize(PayLoad);

        // Assert
        Assert.NotNull(result);
        AssertKey(result!, "Key1", 5);
    }

    [Fact]
    public void Roundtrip()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": 5
    }
}";

        var serializer = new SessionSerializer(new KeyDictionary
        {
            { "Key1", typeof(int) }
        }, writeIndented: true);

        var sessionState = serializer.Deserialize(PayLoad);

        var result = new MemoryStream();

        // Act
        var byteResult = serializer.Serialize(sessionState!);
        var str = Encoding.UTF8.GetString(byteResult);

        // Assert
#if NETCOREAPP3_1
        const string Expected = @"{
  ""id"": ""5"",
  ""r"": false,
  ""v"": {
    ""Key1"": 5
  },
  ""t"": 0,
  ""n"": false,
  ""a"": false
}";

#else
        const string Expected = @"{
  ""id"": ""5"",
  ""v"": {
    ""Key1"": 5
  }
}";
#endif
        Assert.Equal(Expected, str);
    }

    [Fact]
    public void MultipleValuesPrimitive()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": 5,
        ""Key2"": ""hello""
    }
}";
        var serializer = new SessionSerializer(new KeyDictionary
        {
            { "Key1", typeof(int) },
            { "Key2", typeof(string) }
        });

        // Act
        var result = serializer.Deserialize(PayLoad);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);

        AssertKey(result, "Key1", 5);
        AssertKey(result, "Key2", "hello");
    }

    [Fact]
    public void ComplexObject()
    {
        // Arrange
        const string PayLoad = @"{
    ""id"": ""5"",
    ""v"": {
        ""Key1"": {
            ""IntKey"": 5,
            ""StringKey"": ""hello""
        }
    }
}";
        var serializer = new SessionSerializer(new KeyDictionary
        {
            { "Key1", typeof(SomeObject) }
        });

        // Act
        var result = serializer.Deserialize(PayLoad);

        // Assert
        var obj = Assert.IsType<SomeObject>(result!["Key1"]);
        Assert.Equal(5, obj.IntKey);
        Assert.Equal("hello", obj.StringKey);
    }

    private static void AssertKey<T>(ISessionState state, string key, T expected)
    {
        var result = Assert.IsType<T>(state[key]);
        Assert.Equal(expected, result);
    }

    private class SomeObject
    {
        public int IntKey { get; set; }

        public string StringKey { get; set; } = null!;
    }
}
