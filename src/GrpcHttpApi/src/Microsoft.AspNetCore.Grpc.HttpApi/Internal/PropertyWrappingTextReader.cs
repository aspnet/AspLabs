// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal
{
    internal sealed class PropertyWrappingTextReader : TextReader
    {
        private readonly HttpRequestStreamReader _inner;
        private readonly string _prefix;
        private int _index;
        private bool _finished;

        public PropertyWrappingTextReader(HttpRequestStreamReader inner, string propertyName)
        {
            _inner = inner;
            _prefix = @"{""" + propertyName + @""":";
        }

        public override int Read()
        {
            if (_index < _prefix.Length)
            {
                return _prefix[_index++];
            }

            var c = _inner.Read();
            if (c == -1 && !_finished)
            {
                _finished = true;
                return '}';
            }

            return c;
        }
    }
}
