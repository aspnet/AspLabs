// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    internal sealed class FieldMaskConverter<TMessage> : JsonConverter<TMessage> where TMessage : IMessage, new()
    {
        private static readonly char[] FieldMaskPathSeparators = new[] { ',' };

        public FieldMaskConverter(JsonSettings settings)
        {
        }

        public override TMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var message = new TMessage();

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new InvalidOperationException("Expected string value for FieldMask");
            }
            // TODO: Do we *want* to remove empty entries? Probably okay to treat "" as "no paths", but "foo,,bar"?
            string[] jsonPaths = reader.GetString()!.Split(FieldMaskPathSeparators, StringSplitOptions.RemoveEmptyEntries);
            IList messagePaths = (IList)message.Descriptor.Fields[FieldMask.PathsFieldNumber].Accessor.GetValue(message);
            foreach (var path in jsonPaths)
            {
                messagePaths.Add(ToSnakeCase(path));
            }

            return message;
        }

        public override void Write(Utf8JsonWriter writer, TMessage value, JsonSerializerOptions options)
        {
            var paths = (IList<string>)value.Descriptor.Fields[FieldMask.PathsFieldNumber].Accessor.GetValue(value);
            var firstInvalid = paths.FirstOrDefault(p => !IsPathValid(p));
            if (firstInvalid == null)
            {
                writer.WriteStringValue(string.Join(",", paths.Select(ToJsonName)));
            }
            else
            {
                throw new InvalidOperationException($"Invalid field mask to be converted to JSON: {firstInvalid}");
            }
        }

        internal static string ToJsonName(string name)
        {
            StringBuilder result = new StringBuilder(name.Length);
            bool isNextUpperCase = false;
            foreach (char ch in name)
            {
                if (ch == '_')
                {
                    isNextUpperCase = true;
                }
                else if (isNextUpperCase)
                {
                    result.Append(char.ToUpperInvariant(ch));
                    isNextUpperCase = false;
                }
                else
                {
                    result.Append(ch);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Checks whether the given path is valid for a field mask.
        /// </summary>
        /// <returns>true if the path is valid; false otherwise</returns>
        private static bool IsPathValid(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c >= 'A' && c <= 'Z')
                {
                    return false;
                }
                if (c == '_' && i < input.Length - 1)
                {
                    char next = input[i + 1];
                    if (next < 'a' || next > 'z')
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Ported from src/google/protobuf/util/internal/utility.cc
        private static string ToSnakeCase(string text)
        {
            var builder = new StringBuilder(text.Length * 2);
            // Note: this is probably unnecessary now, but currently retained to be as close as possible to the
            // C++, whilst still throwing an exception on underscores.
            bool wasNotUnderscore = false;  // Initialize to false for case 1 (below)
            bool wasNotCap = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= 'A' && c <= 'Z') // ascii_isupper
                {
                    // Consider when the current character B is capitalized:
                    // 1) At beginning of input:   "B..." => "b..."
                    //    (e.g. "Biscuit" => "biscuit")
                    // 2) Following a lowercase:   "...aB..." => "...a_b..."
                    //    (e.g. "gBike" => "g_bike")
                    // 3) At the end of input:     "...AB" => "...ab"
                    //    (e.g. "GoogleLAB" => "google_lab")
                    // 4) Followed by a lowercase: "...ABc..." => "...a_bc..."
                    //    (e.g. "GBike" => "g_bike")
                    if (wasNotUnderscore &&               //            case 1 out
                        (wasNotCap ||                     // case 2 in, case 3 out
                         (i + 1 < text.Length &&         //            case 3 out
                          (text[i + 1] >= 'a' && text[i + 1] <= 'z')))) // ascii_islower(text[i + 1])
                    {  // case 4 in
                       // We add an underscore for case 2 and case 4.
                        builder.Append('_');
                    }
                    // ascii_tolower, but we already know that c *is* an upper case ASCII character...
                    builder.Append((char)(c + 'a' - 'A'));
                    wasNotUnderscore = true;
                    wasNotCap = false;
                }
                else
                {
                    builder.Append(c);
                    if (c == '_')
                    {
                        throw new InvalidOperationException($"Invalid field mask: {text}");
                    }
                    wasNotUnderscore = true;
                    wasNotCap = true;
                }
            }
            return builder.ToString();
        }
    }
}
