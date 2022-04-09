// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web.SessionState;

namespace System.Web.Adapters.SessionState;

internal sealed class RemoteAppSessionStateHandler : HttpTaskAsyncHandler, IRequiresSessionState, IReadOnlySessionState
{
    private readonly RemoteAppSessionStateOptions _options;
    private readonly SessionSerializer _serializer;

    public RemoteAppSessionStateHandler(RemoteAppSessionStateOptions options)
    {
        _options = options;
        _serializer = new SessionSerializer(options.KnownKeys);
    }

    public override bool IsReusable => true;

    public override async Task ProcessRequestAsync(HttpContext context)
    {
        if (_options.ApiKey is null || !string.Equals(_options.ApiKey, context.Request.Headers.Get(_options.ApiKeyHeader), StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
        }
        else
        {
            await _serializer.SerializeAsync(context.Session, context.Response.OutputStream, context.Request.TimedOutToken);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
        }

        context.ApplicationInstance.CompleteRequest();
    }
}
