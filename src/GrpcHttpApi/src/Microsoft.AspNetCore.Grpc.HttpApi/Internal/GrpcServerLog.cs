// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal
{
    internal static class GrpcServerLog
    {
        private static readonly Action<ILogger, string?, Exception?> _unsupportedRequestContentType =
            LoggerMessage.Define<string?>(LogLevel.Information, new EventId(2, "UnsupportedRequestContentType"), "Request content-type of '{ContentType}' is not supported.");

        private static readonly Action<ILogger, string, Exception?> _errorExecutingServiceMethod =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(6, "ErrorExecutingServiceMethod"), "Error when executing service method '{ServiceMethod}'.");

        private static readonly Action<ILogger, StatusCode, string, Exception?> _rpcConnectionError =
            LoggerMessage.Define<StatusCode, string>(LogLevel.Information, new EventId(7, "RpcConnectionError"), "Error status code '{StatusCode}' with detail '{Detail}' raised.");

        private static readonly Action<ILogger, Exception?> _readingMessage =
            LoggerMessage.Define(LogLevel.Debug, new EventId(10, "ReadingMessage"), "Reading message.");

        private static readonly Action<ILogger, Type, Exception?> _deserializingMessage =
            LoggerMessage.Define<Type>(LogLevel.Trace, new EventId(12, "DeserializingMessage"), "Deserializing to '{MessageType}'.");

        private static readonly Action<ILogger, Exception?> _receivedMessage =
            LoggerMessage.Define(LogLevel.Trace, new EventId(13, "ReceivedMessage"), "Received message.");

        private static readonly Action<ILogger, Exception?> _errorReadingMessage =
            LoggerMessage.Define(LogLevel.Information, new EventId(14, "ErrorReadingMessage"), "Error reading message.");

        private static readonly Action<ILogger, Exception?> _sendingMessage =
            LoggerMessage.Define(LogLevel.Debug, new EventId(15, "SendingMessage"), "Sending message.");

        private static readonly Action<ILogger, Exception?> _messageSent =
            LoggerMessage.Define(LogLevel.Trace, new EventId(16, "MessageSent"), "Message sent.");

        private static readonly Action<ILogger, Exception?> _errorSendingMessage =
            LoggerMessage.Define(LogLevel.Information, new EventId(17, "ErrorSendingMessage"), "Error sending message.");

        private static readonly Action<ILogger, Type, Exception?> _serializedMessage =
            LoggerMessage.Define<Type>(LogLevel.Trace, new EventId(18, "SerializedMessage"), "Serialized '{MessageType}'.");

        public static void ErrorExecutingServiceMethod(ILogger logger, string serviceMethod, Exception ex)
        {
            _errorExecutingServiceMethod(logger, serviceMethod, ex);
        }

        public static void RpcConnectionError(ILogger logger, StatusCode statusCode, string detail)
        {
            _rpcConnectionError(logger, statusCode, detail, null);
        }

        public static void UnsupportedRequestContentType(ILogger logger, string? contentType)
        {
            _unsupportedRequestContentType(logger, contentType, null);
        }

        public static void ReadingMessage(ILogger logger)
        {
            _readingMessage(logger, null);
        }

        public static void DeserializingMessage(ILogger logger, Type messageType)
        {
            _deserializingMessage(logger, messageType, null);
        }

        public static void ReceivedMessage(ILogger logger)
        {
            _receivedMessage(logger, null);
        }

        public static void ErrorReadingMessage(ILogger logger, Exception ex)
        {
            _errorReadingMessage(logger, ex);
        }

        public static void SendingMessage(ILogger logger)
        {
            _sendingMessage(logger, null);
        }

        public static void MessageSent(ILogger logger)
        {
            _messageSent(logger, null);
        }

        public static void ErrorSendingMessage(ILogger logger, Exception ex)
        {
            _errorSendingMessage(logger, ex);
        }

        public static void SerializedMessage(ILogger logger, Type messageType)
        {
            _serializedMessage(logger, messageType, null);
        }
    }
}
