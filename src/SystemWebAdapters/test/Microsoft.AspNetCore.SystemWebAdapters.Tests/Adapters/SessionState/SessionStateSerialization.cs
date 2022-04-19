using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;
using Xunit;

using KeyDictionary = System.Collections.Generic.Dictionary<string, System.Type>;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class SessionStateSerialization
{
    [Fact]
    public async Task NewSession()
    {
        // Arrange
        const string PayLoad = @"{
    ""IsNewSession"": true,
}";
        var serializer = new SessionSerializer(new KeyDictionary());

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(PayLoad));

        // Act
        var result = await serializer.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result!.Values);
        Assert.True(result.IsNewSession);
    }

    [Fact]
    public async Task SingleValueInt()
    {
        // Arrange
        const string PayLoad = @"{
    ""SessionID"": ""5"",
    ""Values"": {
        ""Key1"": 5
    }
}";

        var expected = new (string, object?)[] { ("Key1", 5) };
        var serializer = new SessionSerializer(new KeyDictionary
        {
            { "Key1", typeof(int) }
        });

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(PayLoad));

        // Act
        var result = await serializer.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(result);
        AssertValuesEqual(expected, result);
    }

    [Fact]
    public async Task Roundtrip()
    {
        // Arrange
        const string PayLoad = @"{
    ""SessionID"": ""5"",
    ""Values"": {
        ""Key1"": 5
    }
}";

        var serializer = new SessionSerializer(new KeyDictionary
        {
            { "Key1", typeof(int) }
        });

        serializer.Options.WriteIndented = true;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(PayLoad));
        var sessionState = await serializer.DeserializeAsync(stream);

        var result = new MemoryStream();

        // Act
        await serializer.SerializeAsync(sessionState!, result, default);
        var str = SessionStateSerialization.GetStream(result);

        // Assert
#if NETCOREAPP3_1
        const string Expected = @"{
  ""IsAbandoned"": false,
  ""SessionID"": ""5"",
  ""IsReadOnly"": false,
  ""Values"": {
    ""Key1"": 5
  },
  ""Timeout"": 0,
  ""IsNewSession"": false
}";

#else
        const string Expected = @"{
  ""SessionID"": ""5"",
  ""Values"": {
    ""Key1"": 5
  }
}";
#endif
        Assert.Equal(Expected, str);
    }

    private static string GetStream(MemoryStream stream)
        => Encoding.UTF8.GetString(stream.ToArray());

    [Fact]
    public async Task MultipleValuesPrimitive()
    {
        // Arrange
        const string PayLoad = @"{
    ""SessionID"": ""5"",
    ""Values"": {
        ""Key1"": 5,
        ""Key2"": ""hello""
    }
}";
        var expected = new (string, object?)[] { ("Key1", 5), ("Key2", "hello") };
        var serializer = new SessionSerializer(new KeyDictionary
        {
            { "Key1", typeof(int) },
            { "Key2", typeof(string) }
        });

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(PayLoad));

        // Act
        var result = await serializer.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.Values.Count);
        AssertValuesEqual(expected, result);
    }

    [Fact]
    public async Task ComplexObject()
    {
        // Arrange
        const string PayLoad = @"{
    ""SessionID"": ""5"",
    ""Values"": {
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

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(PayLoad));

        // Act
        var result = await serializer.DeserializeAsync(stream);

        // Assert
        var obj = Assert.IsType<SomeObject>(result!.Values["Key1"]);
        Assert.Equal(5, obj.IntKey);
        Assert.Equal("hello", obj.StringKey);
    }

    private static void AssertValuesEqual((string, object?)[] expected, RemoteSessionData? result) =>
    Assert.Collection(result!.Values.Keys, expected.Select<(string, object?), Action<string>>(expected =>
                 actual =>
                 {
                     Assert.Equal(expected.Item1, actual);
                     Assert.Equal(expected.Item2, result.Values[actual]);
                 }).ToArray());

    private class SomeObject
    {
        public int IntKey { get; set; }

        public string? StringKey { get; set; }
    }
}
