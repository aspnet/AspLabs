// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web.SessionState;

namespace System.Web.Adapters.SessionState;

public class RemoteAppSessionStateHandler : HttpTaskAsyncHandler, IRequiresSessionState, IReadOnlySessionState
{
    private SessionSerializer? _serializer;

    private static readonly RemoteAppSessionStateOptions _options = new();

    public override bool IsReusable => true;

    public static void Configure(Action<RemoteAppSessionStateOptions> configure) => configure(_options);

    private SessionSerializer Serializer
    {
        get
        {
            if (_serializer is null)
            {
                _serializer = new SessionSerializer(_options.KnownKeys);
            }

            return _serializer;
        }
    }

    public sealed override async Task ProcessRequestAsync(HttpContext context)
    {
        if (_options.ApiKey is null || !string.Equals(_options.ApiKey, context.Request.Headers.Get(_options.ApiKeyHeader), StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            return;
        }

        await Serializer.SerializeAsync(context.Session, context.Response.OutputStream, context.Request.TimedOutToken);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 200;

        context.Response.End();
    }

    public sealed override void ProcessRequest(HttpContext context) => base.ProcessRequest(context);
}
