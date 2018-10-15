// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;

namespace Microsoft.Diagnostics.Transport
{
    public class TcpTransport : EventPipeTransport
    {
        private readonly IPEndPoint _endPoint;

        public TcpTransport(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        public override EventPipeClientTransport CreateClient()
        {
            throw new NotImplementedException();
        }

        public override EventPipeServerTransport CreateServer()
        {
            throw new NotImplementedException();
        }
    }
}
