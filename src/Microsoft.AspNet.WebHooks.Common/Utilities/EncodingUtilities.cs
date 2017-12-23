// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Utilities for converting to and from hex-encoded and base64-encoded strings.
    /// </summary>
    public static class EncodingUtilities
    {
        private static readonly char[] HexLookup = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private static readonly char[] Base64Padding = { '=' };

        /// <summary>
        /// Converts a <see cref="T:byte[]"/> to a hex-encoded string.
        /// </summary>
        public static string ToHex(byte[] data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            var content = new char[data.Length * 2];
            var output = 0;
            byte d;
            for (var input = 0; input < data.Length; input++)
            {
                d = data[input];
                content[output++] = HexLookup[d / 0x10];
                content[output++] = HexLookup[d % 0x10];
            }
            return new string(content);
        }

        /// <summary>
        /// Converts a hex-encoded string to a <see cref="T:byte[]"/>.
        /// </summary>
        public static byte[] FromHex(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return new byte[0];
            }

            byte[] data = null;
            try
            {
                data = new byte[content.Length / 2];
                var input = 0;
                for (var output = 0; output < data.Length; output++)
                {
                    data[output] = Convert.ToByte(new string(new char[2] { content[input++], content[input++] }), 16);
                }

                if (input != content.Length)
                {
                    data = null;
                }
            }
            catch
            {
                data = null;
            }

            if (data == null)
            {
                var message = string.Format(CultureInfo.CurrentCulture, CommonResources.EncodingUtils_InvalidHexValue, content);
                throw new InvalidOperationException(message);
            }

            return data;
        }

        /// <summary>
        /// Converts a <see cref="T:byte[]"/> to a base64-encoded string.
        /// </summary>
        /// <param name="data">The input to encode.</param>
        /// <param name="uriSafe">Substitute the Base64 characters '+' and '/' with the URI safe characters '-' and '_'. In addition, no padding is added.</param>
        public static string ToBase64(byte[] data, bool uriSafe)
        {
            if (data == null)
            {
                return string.Empty;
            }

            var content = Convert.ToBase64String(data);
            return uriSafe ? content.TrimEnd(Base64Padding).Replace('+', '-').Replace('/', '_') : content;
        }

        /// <summary>
        /// Converts a base64-encoded string to a <see cref="T:byte[]"/> (with or without URI safe mode encoding).
        /// </summary>
        /// <param name="content">The Base64 encoded content</param>
        public static byte[] FromBase64(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return new byte[0];
            }

            var base64 = content.Replace('_', '/').Replace('-', '+');
            switch (content.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;

                case 3:
                    base64 += "=";
                    break;
            }
            var data = Convert.FromBase64String(base64);
            return data;
        }
    }
}
