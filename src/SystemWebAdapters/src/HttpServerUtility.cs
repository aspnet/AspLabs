// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web
{
    public class HttpServerUtility
    {
        private readonly HttpContextCore _context;

        public HttpServerUtility(HttpContextCore context)
        {
            _context = context;
        }

        public string MachineName => throw new NotImplementedException();

        public string MapPath(string path) => throw new NotImplementedException();

        public Exception GetLastError() => throw new NotImplementedException();

        public void ClearError() => throw new NotImplementedException();

        public static byte[] UrlTokenDecode(string input) => throw new NotImplementedException();

        public static string UrlTokenEncode(byte[] input) => throw new NotImplementedException();
    }
}
