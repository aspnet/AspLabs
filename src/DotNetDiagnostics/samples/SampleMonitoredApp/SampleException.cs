// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace SampleMonitoredApp
{
    [Serializable]
    internal class SampleException : Exception
    {
        public SampleException()
        {
        }

        public SampleException(string message) : base(message)
        {
        }

        public SampleException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SampleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}