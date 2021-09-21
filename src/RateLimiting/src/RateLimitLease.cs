// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Threading.RateLimiting
{
    public abstract class RateLimitLease : IDisposable
    {
        // This represents whether lease acquisition was successful
        public abstract bool IsAcquired { get; }

        // Method to extract any general metadata. This is implemented by subclasses
        // to return the metadata they support.
        public abstract bool TryGetMetadata(string metadataName, out object? metadata);

        // This casts the metadata returned by the general method above to known types of values.
        public bool TryGetMetadata<T>(MetadataName<T> metadataName, [MaybeNullWhen(false)] out T? metadata)
        {
            if (metadataName.Name == null)
            {
                metadata = default;
                return false;
            }

            var successful = TryGetMetadata(metadataName.Name, out var rawMetadata);
            if (successful)
            {
                metadata = rawMetadata is null ? default : (T)rawMetadata;
                return true;
            }

            metadata = default;
            return false;
        }

        // Used to get a list of metadata that is available on the lease which can be dictionary keys or static list of strings.
        // Useful for debugging purposes but TryGetMetadata should be used instead in product code.
        public abstract IEnumerable<string> MetadataNames { get; }

        // Virtual method that extracts all the metadata using the list of metadata names and TryGetMetadata().
        public virtual IEnumerable<KeyValuePair<string, object?>> GetAllMetadata()
        {
            foreach (var name in MetadataNames)
            {
                if (TryGetMetadata(name, out var metadata))
                {
                    yield return new KeyValuePair<string, object?>(name, metadata);
                }
            }
        }

        // Follow the general .NET pattern for dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected abstract void Dispose(bool disposing);
    }
}
