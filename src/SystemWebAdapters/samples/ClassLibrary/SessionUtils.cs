using Microsoft.AspNetCore.SystemWebAdapters.SessionState;

namespace ClassLibrary;

public class SessionUtils
{
    public static string ApiKey = "test-key";

    public static void RegisterSessionKeys(SessionOptions options)
    {
        options.RegisterKey<int>("test-value");
        options.RegisterKey<SessionDemoModel>("SampleSessionItem");
    }
}
