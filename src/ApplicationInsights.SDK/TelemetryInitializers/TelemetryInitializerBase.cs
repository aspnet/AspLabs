using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace ApplicationInsights.AspNetCore.TelemetryInitializers
{
    public abstract class TelemetryInitializerBase : ITelemetryInitializer
    {
        private IHttpContextAccessor httpContextAccessor;

        public TelemetryInitializerBase(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException("httpContextAccessor");
            }

            this.httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            try
            {
                var context = httpContextAccessor.HttpContext;

                if (context == null)
                {
                    //AspNetCoreEventSource.Instance.LogTelemetryInitializerBaseInitializeContextNull();
                    return;
                }

                if (context.RequestServices == null)
                {
                    //AspNetCoreEventSource.Instance.LogTelemetryInitializerBaseInitializeRequestServicesNull();
                    return;
                }
                var request = context.RequestServices.GetService<RequestTelemetry>();

                if (request == null)
                {
                    //AspNetCoreEventSource.Instance.LogTelemetryInitializerBaseInitializeRequestNull();
                    return;
                }

                this.OnInitializeTelemetry(context, request, telemetry);
            }
            catch (Exception exp)
            {
                //AspNetCoreEventSource.Instance.LogTelemetryInitializerBaseInitializeException(exp.ToString());
                Debug.WriteLine(exp);
            }
        }

        protected abstract void OnInitializeTelemetry(
            HttpContext platformContext,
            RequestTelemetry requestTelemetry,
            ITelemetry telemetry);
    }
}
