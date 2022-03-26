// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Configuration;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace System.Web.Adapters.SessionState;

public class RemoteAppSessionStateHandler : HttpTaskAsyncHandler, IRequiresSessionState, IReadOnlySessionState
{
    private const string AppSettingsApiKey = "RemoteAppSessionStateApiKey";

    private SessionSerializer? _serializer;
    private RemoteAppSessionStateOptions? _options;

    public override bool IsReusable => true;

    protected RemoteAppSessionStateOptions Options
    {
        get
        {
            if (_options is null)
            {
                var options = new RemoteAppSessionStateOptions();
                RegisterOptions(options);
                _options = options;
            }

            return _options;
        }
    }

    private SessionSerializer Serializer
    {
        get
        {
            if (_serializer is null)
            {
                _serializer = new SessionSerializer(Options.KnownKeys);
            }

            return _serializer;
        }
    }

    public sealed override async Task ProcessRequestAsync(HttpContext context)
    {
        if (!ValidateRequest(context))
        {
            context.Response.StatusCode = 401;
            return;
        }

        await Serializer.SerializeAsync(context.Session, context.Response.OutputStream, context.Request.TimedOutToken);
    }

    protected virtual bool ValidateRequest(HttpContext context)
    {
        if (Options.ApiKey is { } apiKey)
        {
            return !string.Equals(apiKey, context.Request.Headers.Get(Options.ApiKeyHeader), StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    protected virtual void RegisterOptions(RemoteAppSessionStateOptions options)
    {
        options.ApiKey = ConfigurationManager.AppSettings[AppSettingsApiKey];
    }

    public sealed override void ProcessRequest(HttpContext context) => base.ProcessRequest(context);
}
