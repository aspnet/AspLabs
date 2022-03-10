﻿using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http.Features;

namespace System.Web.Internal
{
    internal class ServerVariablesNameValueCollection : NameValueCollection
    {
        private const string EnumerationErrorMessage = "ASP.NET Core doesn't support enumerating server variables.";
        private const string IndexErrorMessage = "ASP.NET Core doesn't support accessing server variables by index.";
        private const string SerializationErrorMessage = "ASP.NET Core doesn't suppor serialization of server variables.";

        private readonly IServerVariablesFeature _serverVariables;

        public ServerVariablesNameValueCollection(IServerVariablesFeature serverVariables)
        {
            _serverVariables = serverVariables;
            IsReadOnly = false;
        }

        public override string?[] AllKeys => throw new PlatformNotSupportedException(EnumerationErrorMessage);

        public override int Count => throw new PlatformNotSupportedException(EnumerationErrorMessage);

        public override void Add(string? name, string? value)
        {
            if (name is null)
            {
                return;
            }

            _serverVariables[name] = value;
        }

        public override KeysCollection Keys => throw new PlatformNotSupportedException("KeysCollection is not supported as Get(int) is not available.");

        public override string? Get(int index) => throw new PlatformNotSupportedException(IndexErrorMessage);

        public override string? GetKey(int index) => throw new PlatformNotSupportedException(IndexErrorMessage);

        public override string[]? GetValues(int index) => throw new PlatformNotSupportedException(IndexErrorMessage);

        public override string[]? GetValues(string? name)
        {
            if (name is not null && _serverVariables[name] is string result)
            {
                return new[] { result };
            }

            return null;
        }

        public override void Remove(string? name)
        {
            if (name is null)
            {
                return;
            }

            _serverVariables[name] = null;
        }

        public override void Set(string? name, string? value)
        {
            if (name is null)
            {
                return;
            }

            _serverVariables[name] = value;
        }

        public override string? Get(string? name)
        {
            if (name is null)
            {
                return null;
            }

            return _serverVariables[name];
        }

        public override void Clear() => throw new PlatformNotSupportedException(EnumerationErrorMessage);

        public override IEnumerator GetEnumerator() => throw new PlatformNotSupportedException(EnumerationErrorMessage);

        public override void GetObjectData(SerializationInfo info, StreamingContext context) => throw new PlatformNotSupportedException(SerializationErrorMessage);

        public override void OnDeserialization(object? sender)=> throw new PlatformNotSupportedException(SerializationErrorMessage);
    }
}
