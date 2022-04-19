// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.WebUtilities;

namespace System.Web;

public class HttpServerUtility
{
    private readonly HttpContextCore _context;

    internal HttpServerUtility(HttpContextCore context)
    {
        _context = context;
    }

    public string MachineName => Environment.MachineName;

    [Obsolete(Constants.NotImplemented)]
    public string MapPath(string path) => throw new NotImplementedException();

    public Exception? GetLastError() => null;

    public void ClearError()
    {
    }

    /// <summary>
    /// This method is similar to <see cref="WebEncoders.Base64UrlDecode(string)"/> but handles the trailing character that <see cref="UrlTokenEncode(byte[])"/>
    /// appends to the string.
    /// </summary>
    /// <param name="input">Value to decode</param>
    /// <returns>Decoded value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static byte[]? UrlTokenDecode(string input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (input.Length == 0)
        {
            return Array.Empty<byte>();
        }

        // The number of padding chars is expected to be the final character of the string
        if (!IsDigit(input[^1]))
        {
            return null;
        }

        return WebEncoders.Base64UrlDecode(input, 0, input.Length - 1);

        static bool IsDigit(char c) => (uint)(c - '0') <= (uint)('9' - '0');
    }

    /// <summary>
    /// This method is similar to <see cref="WebEncoders.Base64UrlEncode(byte[])"/> but the resulting string includes an extra character
    /// that is the count of how many padding characters where removed from the Base64 encoded value.
    /// </summary>
    /// <param name="input">Value to encode</param>
    /// <returns>URL encoded value.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string UrlTokenEncode(byte[] input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (input.Length == 0)
        {
            return string.Empty;
        }

        var encoded = WebEncoders.Base64UrlEncode(input);
        var padding = (encoded.Length % 4) switch
        {
            0 => 0,
            2 => 2,
            3 => 1,
            _ => throw new FormatException("Invalid input"),
        };

        return $"{encoded}{padding}";
    }
}
