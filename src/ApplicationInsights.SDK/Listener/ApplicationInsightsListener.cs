using System;
using System.Diagnostics;
using ApplicationInsights.Helpers;
using Microsoft.ApplicationInsights;

namespace ApplicationInsights.Listener
{
    public class ApplicationInsightsListener : IObserver<DiagnosticListener>
    {
        private readonly AspNetCoreHostingCallback _aspNetCoreHostingCallback;
        private readonly SystemNetHttpCallback _systemNetHttpCallback;    
        private readonly EntityFrameworkCallback _entityFrameworkCallback;
        private readonly SqlClientCallback _sqlClientCallback;

        public ApplicationInsightsListener(TelemetryClient client)
        {
            _aspNetCoreHostingCallback = new AspNetCoreHostingCallback(client);
            _systemNetHttpCallback = new SystemNetHttpCallback(client);
            _entityFrameworkCallback = new EntityFrameworkCallback(client);
            _sqlClientCallback = new SqlClientCallback(client);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == "Microsoft.AspNetCore")
            {
                value.SubscribeWithAdapter(_aspNetCoreHostingCallback);
            }
            if (value.Name == "HttpHandlerDiagnosticListener")
            {
                value.SubscribeWithAdapter(_systemNetHttpCallback);
            }
            if (value.Name == "Microsoft.EntityFrameworkCore")
            {
                value.SubscribeWithAdapter(_entityFrameworkCallback);
            }
            if (value.Name == "SqlClientDiagnosticListener")
            {
                //value.SubscribeWithAdapter(_sqlClientCallback);
            }
        }
    }
}
