// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.Components.Web.Extensions.Head
{
    internal class HeadManagementJSObjectReference
    {
        private const string ScriptPath = "./_content/Microsoft.AspNetCore.Components.Web.Extensions/headManager.js";
        private readonly IJSRuntime _jsRuntime;
        private Task<JSObjectReference>? _headManager;

        public HeadManagementJSObjectReference(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public Task<JSObjectReference> HeadManager =>
            _headManager ??= _jsRuntime.InvokeAsync<JSObjectReference>("import", ScriptPath).AsTask();

        public async ValueTask SetTitleAsync(string title)
        {
            var headManager = await HeadManager;
            await headManager.InvokeVoidAsync("setTitle", title);
        }

        public async ValueTask AddOrUpdateHeadTagAsync(TagElement tag, string id)
        {
            var headManager = await HeadManager;
            await headManager.InvokeVoidAsync("addOrUpdateHeadTag", tag, id);
        }

        public async ValueTask RemoveHeadTagAsync(string id)
        {
            var headManager = await HeadManager;
            await headManager.InvokeVoidAsync("removeHeadTag", id);
        }
    }
}
