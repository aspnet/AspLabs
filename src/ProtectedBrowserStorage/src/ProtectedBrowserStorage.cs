// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.ProtectedBrowserStorage
{
    // Open design question: should the data protection purpose be scoped to the
    // user? Otherwise user A could give one of their values to use B (intentionally
    // or via malicious actions from user B). However it's not wise to just embed
    // User.Identity.Name into the purpose string, because there are cases where the
    // value has to be retained across a login flow, such as in the Blazing Pizzas
    // case where a logged-out user builds an order then logs in.
    //
    // Perhaps GetAsync/SetAsync should accept an optional "scope" or "purpose" string
    // that, if given, gets combined with the purpose we'd auto-generate anyway.

    public abstract class ProtectedBrowserStorage
    {
        private readonly string _storeName;
        private readonly IJSRuntime _jsRuntime;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ConcurrentDictionary<string, IDataProtector> _cachedDataProtectorsByPurpose
            = new ConcurrentDictionary<string, IDataProtector>();

        // Stylistically, it doesn't matter at all what options we choose, since the values
        // will be opaque after data protection. All that matters is that some fixed set of
        // options exists and remains constant forever. We should choose whatever options
        // maximize the ability to round-trip .NET objects reliably. 
        private readonly static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions();

        public ProtectedBrowserStorage(string storeName, IJSRuntime jsRuntime, IDataProtectionProvider dataProtectionProvider)
        {
            if (string.IsNullOrEmpty(storeName))
            {
                throw new ArgumentException("The value cannot be null or empty", nameof(storeName));
            }

            _storeName = storeName;
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            _dataProtectionProvider = dataProtectionProvider ?? throw new ArgumentNullException(nameof(dataProtectionProvider));
        }

        public Task SetAsync(string key, object value)
        {
            var json = JsonSerializer.Serialize(value, SerializerOptions);

            var protector = GetOrCreateCachedProtector(key);
            var protectedJson = protector.Protect(json);
            return _jsRuntime.InvokeAsync<object>(
                "blazorBrowserStorage.set",
                _storeName,
                key,
                protectedJson);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var protectedJson = await _jsRuntime.InvokeAsync<string>(
                "blazorBrowserStorage.get",
                _storeName,
                key);

            // We should consider having both TryGetAsync and GetValueOrDefaultAsync
            // It should be possible to distinguish between the value 'null' being stored
            // for a given key versus no value being stored for that key. However we should
            // still data-protect the 'null' value so it's indistinguishable to the end
            // user from non-null values.
            if (protectedJson == null)
            {
                return default;
            }

            var protector = GetOrCreateCachedProtector(key);
            var json = protector.Unprotect(protectedJson);
            return JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }

        // IDataProtect isn't disposable, so we're fine holding these indefinitely.
        // Only a bounded number of them will be created, as the 'key' values should
        // come from a bounded set known at compile-time. There's no use case for
        // letting runtime data determine the 'key' values.
        private IDataProtector GetOrCreateCachedProtector(string key)
            => _cachedDataProtectorsByPurpose.GetOrAdd(
                CreatePurpose(key),
                _dataProtectionProvider.CreateProtector);

        private string CreatePurpose(string key)
            => $"{GetType().FullName}:{_storeName}:{key}";
    }
}
