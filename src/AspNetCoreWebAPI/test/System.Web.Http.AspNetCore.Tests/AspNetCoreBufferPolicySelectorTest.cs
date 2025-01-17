// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using Xunit;

namespace System.Web.Http.AspNetCore
{
    public class AspNetCoreBufferPolicySelectorTest
    {
        [Fact]
        public void UseBufferedOutputStream_ReturnsTrue_ForObjectContent()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new ObjectContent<string>("blue", new JsonMediaTypeFormatter());

            Assert.True(new AspNetCoreBufferPolicySelector(true).UseBufferedOutputStream(response));
        }

        [Fact]
        public void UseBufferedOutputStream_ReturnsFalse_ForSpecifiedContentLength()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new ObjectContent<string>("blue", new JsonMediaTypeFormatter());
            response.Content.Headers.ContentLength = 5;

            Assert.False(new AspNetCoreBufferPolicySelector(true).UseBufferedOutputStream(response));
        }

        [Fact]
        public void UseBufferedOutputStream_ReturnsFalse_ForChunkedTransferEncoding()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Headers.TransferEncodingChunked = true;
            response.Content = new ObjectContent<string>("blue", new JsonMediaTypeFormatter());

            Assert.False(new AspNetCoreBufferPolicySelector(true).UseBufferedOutputStream(response));
        }

        [Fact]
        public void UseBufferedOutputStream_ReturnsFalse_ForStreamContent()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StreamContent(new MemoryStream());

            Assert.False(new AspNetCoreBufferPolicySelector(true).UseBufferedOutputStream(response));
        }

        [Fact]
        public void UseBufferedOutputStream_ReturnsFalse_ForPushStreamContent()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new PushStreamContent((s, c, tc) => { return; });

            Assert.False(new AspNetCoreBufferPolicySelector(true).UseBufferedOutputStream(response));
        }
    }
}
