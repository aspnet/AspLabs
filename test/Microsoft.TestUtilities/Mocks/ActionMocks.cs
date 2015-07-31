// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.TestUtilities.Mocks
{
    /// <summary>
    /// Various mockable <see cref="Action{T}"/> which can be used when mocking <see cref="Action{T}"/> and 
    /// <see cref="Func{T}"/> passed as arguments.
    /// </summary>
    public class ActionMocks
    {
        public virtual void Action<T1>(T1 value1)
        {
            throw new NotImplementedException();
        }

        public virtual void Action<T1, T2>(T1 value1, T2 value2)
        {
            throw new NotImplementedException();
        }
    }
}
