// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Greet;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Xmldoc;

namespace Microsoft.AspNetCore.Grpc.Swagger.Tests.Services
{
    public class XmlDocService : XmlDoc.XmlDocBase
    {
        private readonly ILogger _logger;

        public XmlDocService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<XmlDocServiceWithComments>();
        }

        public override Task<StringReply> BasicGet(StringRequest request, ServerCallContext context)
        {
            return base.BasicGet(request, context);
        }

        public override Task<StringReply> BodyRootPost(StringRequestWithDetail request, ServerCallContext context)
        {
            return base.BodyRootPost(request, context);
        }

        public override Task<StringReply> BodyPathPost(StringRequestWithDetail request, ServerCallContext context)
        {
            return base.BodyPathPost(request, context);
        }
    }
}
