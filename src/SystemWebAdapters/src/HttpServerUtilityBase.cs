// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public class HttpServerUtilityBase
{
    public virtual string MachineName => throw new NotImplementedException();

    [Obsolete(Constants.NotImplemented)]
    public virtual string MapPath(string path) => throw new NotImplementedException();

    public virtual Exception? GetLastError() => throw new NotImplementedException();

    public virtual byte[]? UrlTokenDecode(string input) => throw new NotImplementedException();

    public virtual void ClearError() => throw new NotImplementedException();

    public virtual string UrlTokenEncode(byte[] input) => throw new NotImplementedException();
}
