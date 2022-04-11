// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;

namespace System.Web;

public class HttpServerUtility
{
    private readonly HttpContextCore _context;

    internal HttpServerUtility(HttpContextCore context)
    {
        _context = context;
    }

    public string MachineName => Environment.MachineName;

    [Obsolete("Not implemented for ASP.NET Core")]
    public string MapPath(string path) => throw new NotImplementedException();

    public Exception? GetLastError() => null;

    public void ClearError()
    {
    }

    public static byte[]? UrlTokenDecode(string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (input.Length < 1)
        {
            return Array.Empty<byte>();
        }

        // Calculate the number of padding chars to append to this string. The number of padding chars to append is stored in the last char of the string.
        var numPadChars = input[^1] - '0';
        if (numPadChars < 0 || numPadChars > 10)
        {
            return null;
        }

        var length = input.Length - 1 + numPadChars;
        var base64Chars = ArrayPool<char>.Shared.Rent(length);

        // Transform the "-" to "+", and "*" to "/"
        for (int iter = 0; iter < input.Length - 1; iter++)
        {
            base64Chars[iter] = input[iter] switch
            {
                '-' => '+',
                '_' => '/',
                var c => c,
            };
        }

        // Add padding chars
        for (int iter = input.Length - 1; iter < length; iter++)
        {
            base64Chars[iter] = '=';
        }

        try
        {
            return Convert.FromBase64CharArray(base64Chars, 0, length);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(base64Chars);
        }
    }

    // This method does a base64 encoding of the input, but replaces the `=` with a count of padding and transforms `+`->`-` and `/`->`_`
    public static string? UrlTokenEncode(byte[] input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (input.Length < 1)
        {
            return string.Empty;
        }

        var base64Str = Convert.ToBase64String(input);

        // Find how many padding chars are present in the end
        var endPos = 0;
        for (endPos = base64Str.Length; endPos > 0; endPos--)
        {
            if (base64Str[endPos - 1] != '=') // Found a non-padding char!
            {
                break;
            }
        }

        return string.Create(endPos + 1, base64Str, static (span, original) =>
        {
            var padding = original.Length - span.Length + 1;
            span[^1] = (char)('0' + padding);

            for (var iter = 0; iter < span.Length - 1; iter++)
            {
                span[iter] = original[iter] switch
                {
                    '+' => '-',
                    '/' => '_',
                    char c => c,
                };
            }
        });
    }
}
