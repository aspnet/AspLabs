// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;

namespace Microsoft.AspNet.WebHooks.Utilities
{
    /// <summary>
    /// Provides non-cryptographic hashing functions.
    /// </summary>
    public static class Hasher
    {
        private const uint FnvPrime32 = 0x01000193;
        private const uint FnvOffset32 = 0x811C9DC5;

        /// <summary>
        /// Gets a FNV-1a 32-bit hash of the provided <paramref name="content"/>. The FNV-1a algorithm
        /// is used in many context including DNS servers, database indexing hashes, non-cryptographic file
        /// fingerprints to name a few. For more information about FNV, please see the IETF document
        /// <c>The FNV Non-Cryptographic Hash Algorithm</c> as well as <c>http://isthe.com/chongo/tech/comp/fnv/</c>.
        /// </summary>
        /// <param name="content">The content to hash.</param>
        /// <returns>The computed hash.</returns>
        [CLSCompliant(false)]
        public static uint GetFnvHash32(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            byte[] data = Encoding.UTF8.GetBytes(content);

            uint hash = FnvOffset32;
            for (int cnt = 0; cnt < data.Length; cnt++)
            {
                hash ^= data[cnt];
                hash = hash * FnvPrime32;
            }
            return hash;
        }

        /// <summary>
        /// Gets a string representation of a FNV-1a 32-bit hash of the provided <paramref name="content"/>. The FNV-1a 
        /// algorithm is used in many context including DNS servers, database indexing hashes, non-cryptographic file
        /// fingerprints to name a few. For more information about FNV, please see the IETF document
        /// <c>The FNV Non-Cryptographic Hash Algorithm</c> as well as <c>http://isthe.com/chongo/tech/comp/fnv/</c>.
        /// </summary>
        /// <param name="content">The content to hash.</param>
        /// <returns>A string representation of the computed hash.</returns>
        public static string GetFnvHash32AsString(string content)
        {
            return GetFnvHash32(content).ToString("x8", CultureInfo.InvariantCulture);
        }
    }
}