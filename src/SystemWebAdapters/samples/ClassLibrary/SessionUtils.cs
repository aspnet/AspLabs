using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Adapters.SessionState;

namespace ClassLibrary
{
    public class SessionUtils
    {
        public static void RegisterSessionKeys(RemoteAppSessionStateOptions options)
        {
            options.ApiKey = "test-key";
            options.RegisterKey<int>("test-value");
        }
    }
}
