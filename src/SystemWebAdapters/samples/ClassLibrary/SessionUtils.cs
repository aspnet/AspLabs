using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

namespace ClassLibrary;

public class SessionUtils
{
    public static void RegisterSessionKeys(RemoteAppSessionStateOptions options)
    {
        options.ApiKey = "test-key";
        options.RegisterKey<int>("test-value");
        options.RegisterKey<SessionDemoModel>("SampleSessionItem");
    }
}
