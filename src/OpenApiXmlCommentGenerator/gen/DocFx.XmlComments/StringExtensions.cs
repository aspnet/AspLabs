using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DocFx.XmlComments;

public static class StringExtensions
{
    public static string TrimEachLine(this string text, string indent = "")
    {
        var minLeadingWhitespace = int.MaxValue;
        var lines = text.ReadLines().ToList();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var leadingWhitespace = 0;
            while (leadingWhitespace < line.Length && char.IsWhiteSpace(line[leadingWhitespace]))
            {
                leadingWhitespace++;
            }

            minLeadingWhitespace = Math.Min(minLeadingWhitespace, leadingWhitespace);
        }

        var builder = new StringBuilder();

        // Trim leading empty lines
        var trimStart = true;

        // Apply indentation to all lines except the first,
        // since the first new line in <pre></code> is significant
        var firstLine = true;

        foreach (var line in lines)
        {
            if (trimStart && string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (firstLine)
            {
                firstLine = false;
            }
            else
            {
                builder.Append(indent);
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                builder.AppendLine();
                continue;
            }

            trimStart = false;
            builder.AppendLine(line.Substring(minLeadingWhitespace));
        }

        return builder.ToString().TrimEnd();
    }

    public static IEnumerable<string> ReadLines(this string text)
    {
        string line;
        using var sr = new StringReader(text);
        while ((line = sr.ReadLine()) != null)
        {
            yield return line;
        }
    }
}
