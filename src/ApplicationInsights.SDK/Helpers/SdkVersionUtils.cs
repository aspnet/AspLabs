using System.Linq;
using System.Reflection;

namespace ApplicationInsights.Helpers
{
    internal class SdkVersionUtils
    {
#if NET451
        public const string VersionPrefix = "aspnet5f:";
#else
        public const string VersionPrefix = "aspnet5c:";
#endif
        internal static string GetAssemblyVersion()
        {
            return typeof(SdkVersionUtils).GetTypeInfo().Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>()
                      .First()
                      .InformationalVersion;
        }
    }
}
