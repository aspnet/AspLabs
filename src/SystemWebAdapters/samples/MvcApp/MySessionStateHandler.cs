using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Adapters.SessionState;

namespace MvcApp
{
    public class MySessionStateHandler : RemoteAppSessionStateHandler
    {
        protected override void RegisterOptions(RemoteAppSessionStateOptions options)
        {
            ClassLibrary.SessionUtils.RegisterSessionKeys(options);
        }
    }
}
