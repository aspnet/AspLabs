using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;
using ApplicationInsights.Extensions;
using ApplicationInsights.Helpers;
using System.Net.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using Microsoft.AspNetCore.Routing.Tree;

namespace ApplicationInsights.Listener
{
    public class AspNetCoreListener
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private readonly TelemetryClient _client;
        private readonly AsyncLocal<long> _beginRequestTimestamp = new AsyncLocal<long>();
        private readonly string _sdkVersion;

        public AspNetCoreListener(TelemetryClient client)
        {
            _client = client;
            _sdkVersion = SdkVersionUtils.VersionPrefix + SdkVersionUtils.GetAssemblyVersion();
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.BeginRequest")]
        public void OnBeginRequest(HttpContext httpContext)
        {
            _beginRequestTimestamp.Value = Stopwatch.GetTimestamp();
            _client.Context.Operation.Id = httpContext.TraceIdentifier;
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.EndRequest")]
        public void OnEndRequest(HttpContext httpContext)
        {
            var start = _beginRequestTimestamp.Value;
            var end = Stopwatch.GetTimestamp();

            var telemetry = new RequestTelemetry();
            telemetry.Duration = TimeSpan.FromTicks((long)((end - start) * TimestampToTicks));
            telemetry.StartTime = DateTime.Now - telemetry.Duration;
            telemetry.ResponseCode = httpContext.Response.StatusCode.ToString();
            telemetry.Success = (httpContext.Response.StatusCode < 400);
            telemetry.HttpMethod = httpContext.Request.Method;
            telemetry.Url = httpContext.Request.GetUri();
            telemetry.Context.GetInternalContext().SdkVersion = _sdkVersion;
            _client.TrackRequest(telemetry);
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void OnHostingException(HttpContext httpContext, Exception exception)
        {
            OnException(httpContext, exception);
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.HandledException")]
        public void OnDiagnosticsHandledException(HttpContext httpContext, Exception exception)
        {
            OnException(httpContext, exception);
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.UnhandledException")]
        public void OnDiagnosticsUnhandledException(HttpContext httpContext, Exception exception)
        {
            OnException(httpContext, exception);
        }

        private void OnException(HttpContext context, Exception exception)
        {
            _client.TrackException(exception);
        }

        private string GetNameFromRouteContext(RouteData routeData)
        {
            string name = null;

            if (routeData.Values.Count > 0)
            {
                var routeValues = routeData.Values;

                object controller;
                routeValues.TryGetValue("controller", out controller);
                string controllerString = (controller == null) ? string.Empty : controller.ToString();

                if (!string.IsNullOrEmpty(controllerString))
                {
                    name = controllerString;

                    object action;
                    routeValues.TryGetValue("action", out action);
                    string actionString = (action == null) ? string.Empty : action.ToString();

                    if (!string.IsNullOrEmpty(actionString))
                    {
                        name += "/" + actionString;
                    }

                    if (routeValues.Keys.Count > 2)
                    {
                        // Add parameters
                        var sortedKeys = routeValues.Keys
                            .Where(key =>
                                !string.Equals(key, "controller", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(key, "action", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(key, TreeRouter.RouteGroupKey, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
                            .ToArray();

                        if (sortedKeys.Length > 0)
                        {
                            string arguments = string.Join(@"/", sortedKeys);
                            name += " [" + arguments + "]";
                        }
                    }
                }
            }

            return name;
        }
    }
}
