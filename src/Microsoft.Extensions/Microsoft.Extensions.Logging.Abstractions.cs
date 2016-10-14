namespace Microsoft.Extensions.Logging
{
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct EventId
    {
        public EventId(int id, string name=null) { throw null;}
        public int Id { get { throw null; } }
        public string Name { get { throw null; } }
        public static implicit operator Microsoft.Extensions.Logging.EventId (int i) { throw null; }
    }
    public partial interface ILogger
    {
        System.IDisposable BeginScope<TState>(TState state);
        bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel);
        void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, System.Exception exception, System.Func<TState, System.Exception, string> formatter);
    }
    public partial interface ILogger<out TCategoryName> : Microsoft.Extensions.Logging.ILogger
    {
    }
    public partial interface ILoggerFactory : System.IDisposable
    {
        void AddProvider(Microsoft.Extensions.Logging.ILoggerProvider provider);
        Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName);
    }
    public partial interface ILoggerProvider : System.IDisposable
    {
        Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName);
    }
    public partial class Logger<T> : Microsoft.Extensions.Logging.ILogger, Microsoft.Extensions.Logging.ILogger<T>
    {
        public Logger(Microsoft.Extensions.Logging.ILoggerFactory factory) { }
        System.IDisposable Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state) { throw null; }
        bool Microsoft.Extensions.Logging.ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) { throw null; }
        void Microsoft.Extensions.Logging.ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, System.Exception exception, System.Func<TState, System.Exception, string> formatter) { }
    }
    public static partial class LoggerExtensions
    {
        public static System.IDisposable BeginScope(this Microsoft.Extensions.Logging.ILogger logger, string messageFormat, params object[] args) { throw null; }
        public static void LogCritical(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, System.Exception exception, string message, params object[] args) { }
        public static void LogCritical(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, string message, params object[] args) { }
        public static void LogCritical(this Microsoft.Extensions.Logging.ILogger logger, string message, params object[] args) { }
        public static void LogDebug(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, System.Exception exception, string message, params object[] args) { }
        public static void LogDebug(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, string message, params object[] args) { }
        public static void LogDebug(this Microsoft.Extensions.Logging.ILogger logger, string message, params object[] args) { }
        public static void LogError(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, System.Exception exception, string message, params object[] args) { }
        public static void LogError(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, string message, params object[] args) { }
        public static void LogError(this Microsoft.Extensions.Logging.ILogger logger, string message, params object[] args) { }
        public static void LogInformation(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, System.Exception exception, string message, params object[] args) { }
        public static void LogInformation(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, string message, params object[] args) { }
        public static void LogInformation(this Microsoft.Extensions.Logging.ILogger logger, string message, params object[] args) { }
        public static void LogTrace(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, System.Exception exception, string message, params object[] args) { }
        public static void LogTrace(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, string message, params object[] args) { }
        public static void LogTrace(this Microsoft.Extensions.Logging.ILogger logger, string message, params object[] args) { }
        public static void LogWarning(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, System.Exception exception, string message, params object[] args) { }
        public static void LogWarning(this Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.EventId eventId, string message, params object[] args) { }
        public static void LogWarning(this Microsoft.Extensions.Logging.ILogger logger, string message, params object[] args) { }
    }
    public static partial class LoggerFactoryExtensions
    {
        public static Microsoft.Extensions.Logging.ILogger CreateLogger(this Microsoft.Extensions.Logging.ILoggerFactory factory, System.Type type) { throw null; }
        public static Microsoft.Extensions.Logging.ILogger<T> CreateLogger<T>(this Microsoft.Extensions.Logging.ILoggerFactory factory) { throw null; }
    }
    public static partial class LoggerMessage
    {
        public static System.Action<Microsoft.Extensions.Logging.ILogger, System.Exception> Define(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, string formatString) { throw null; }
        public static System.Action<Microsoft.Extensions.Logging.ILogger, T1, System.Exception> Define<T1>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, string formatString) { throw null; }
        public static System.Action<Microsoft.Extensions.Logging.ILogger, T1, T2, System.Exception> Define<T1, T2>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, string formatString) { throw null; }
        public static System.Action<Microsoft.Extensions.Logging.ILogger, T1, T2, T3, System.Exception> Define<T1, T2, T3>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, string formatString) { throw null; }
        public static System.Action<Microsoft.Extensions.Logging.ILogger, T1, T2, T3, T4, System.Exception> Define<T1, T2, T3, T4>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, string formatString) { throw null; }
        public static System.Action<Microsoft.Extensions.Logging.ILogger, T1, T2, T3, T4, T5, System.Exception> Define<T1, T2, T3, T4, T5>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, string formatString) { throw null; }
        public static System.Action<Microsoft.Extensions.Logging.ILogger, T1, T2, T3, T4, T5, T6, System.Exception> Define<T1, T2, T3, T4, T5, T6>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, string formatString) { throw null; }
        public static System.Func<Microsoft.Extensions.Logging.ILogger, System.IDisposable> DefineScope(string formatString) { throw null; }
        public static System.Func<Microsoft.Extensions.Logging.ILogger, T1, System.IDisposable> DefineScope<T1>(string formatString) { throw null; }
        public static System.Func<Microsoft.Extensions.Logging.ILogger, T1, T2, System.IDisposable> DefineScope<T1, T2>(string formatString) { throw null; }
        public static System.Func<Microsoft.Extensions.Logging.ILogger, T1, T2, T3, System.IDisposable> DefineScope<T1, T2, T3>(string formatString) { throw null; }
    }
    public enum LogLevel
    {
        Critical = 5,
        Debug = 1,
        Error = 4,
        Information = 2,
        None = 6,
        Trace = 0,
        Warning = 3,
    }
}
namespace Microsoft.Extensions.Logging.Abstractions.Internal
{
    public partial class TypeNameHelper
    {
        public TypeNameHelper() { }
        public static string GetTypeDisplayName(System.Type type) { throw null; }
    }
}
namespace Microsoft.Extensions.Logging.Internal
{
    public partial class FormattedLogValues : System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>, System.Collections.Generic.IReadOnlyCollection<System.Collections.Generic.KeyValuePair<string, object>>, System.Collections.Generic.IReadOnlyList<System.Collections.Generic.KeyValuePair<string, object>>, System.Collections.IEnumerable
    {
        public FormattedLogValues(string format, params object[] values) { }
        public int Count { get { throw null; } }
        public System.Collections.Generic.KeyValuePair<string, object> this[int index] { get { throw null; } }
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, object>> GetEnumerator() { throw null; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw null; }
        public override string ToString() { throw null; }
    }
    public partial class LogValuesFormatter
    {
        public LogValuesFormatter(string format) { }
        public string OriginalFormat { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.List<string> ValueNames { get { throw null; } }
        public string Format(object[] values) { throw null; }
        public System.Collections.Generic.KeyValuePair<string, object> GetValue(object[] values, int index) { throw null; }
        public System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>> GetValues(object[] values) { throw null; }
    }
}
