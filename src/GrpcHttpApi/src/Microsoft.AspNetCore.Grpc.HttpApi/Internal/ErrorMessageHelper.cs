// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Grpc.Shared;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal
{
    internal static class ErrorMessageHelper
    {
        internal static string BuildErrorMessage(string message, Exception exception, bool? includeExceptionDetails)
        {
            if (includeExceptionDetails ?? false)
            {
                return message + " " + CommonGrpcProtocolHelpers.ConvertToRpcExceptionMessage(exception);
            }

            return message;
        }
    }
}
