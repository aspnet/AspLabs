namespace Microsoft.Extensions.Logging
{
    public static partial class ConsoleLoggerExtensions
    {
        public static Microsoft.Extensions.Logging.ILoggerFactory AddConsole(this Microsoft.Extensions.Logging.ILoggerFactory factory) { throw null; }
        public static Microsoft.Extensions.Logging.ILoggerFactory AddConsole(this Microsoft.Extensions.Logging.ILoggerFactory factory, Microsoft.Extensions.Configuration.IConfiguration configuration) { throw null; }
        public static Microsoft.Extensions.Logging.ILoggerFactory AddConsole(this Microsoft.Extensions.Logging.ILoggerFactory factory, Microsoft.Extensions.Logging.Console.IConsoleLoggerSettings settings) { throw null; }
        public static Microsoft.Extensions.Logging.ILoggerFactory AddConsole(this Microsoft.Extensions.Logging.ILoggerFactory factory, Microsoft.Extensions.Logging.LogLevel minLevel) { throw null; }
        public static Microsoft.Extensions.Logging.ILoggerFactory AddConsole(this Microsoft.Extensions.Logging.ILoggerFactory factory, Microsoft.Extensions.Logging.LogLevel minLevel, bool includeScopes) { throw null; }
        public static Microsoft.Extensions.Logging.ILoggerFactory AddConsole(this Microsoft.Extensions.Logging.ILoggerFactory factory, bool includeScopes) { throw null; }
        public static Microsoft.Extensions.Logging.ILoggerFactory AddConsole(this Microsoft.Extensions.Logging.ILoggerFactory factory, System.Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter) { throw null; }
        public static Microsoft.Extensions.Logging.ILoggerFactory AddConsole(this Microsoft.Extensions.Logging.ILoggerFactory factory, System.Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter, bool includeScopes) { throw null; }
    }
}
namespace Microsoft.Extensions.Logging.Console
{
    public partial class ConfigurationConsoleLoggerSettings : Microsoft.Extensions.Logging.Console.IConsoleLoggerSettings
    {
        public ConfigurationConsoleLoggerSettings(Microsoft.Extensions.Configuration.IConfiguration configuration) { }
        public Microsoft.Extensions.Primitives.IChangeToken ChangeToken { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool IncludeScopes { get { throw null; } }
        public Microsoft.Extensions.Logging.Console.IConsoleLoggerSettings Reload() { throw null; }
        public bool TryGetSwitch(string name, out Microsoft.Extensions.Logging.LogLevel level) { level = default(Microsoft.Extensions.Logging.LogLevel); throw null; }
    }
    public partial class ConsoleLogger : Microsoft.Extensions.Logging.ILogger
    {
        public ConsoleLogger(string name, System.Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter, bool includeScopes) { }
        public Microsoft.Extensions.Logging.Console.Internal.IConsole Console { get { throw null; } set { } }
        public System.Func<string, Microsoft.Extensions.Logging.LogLevel, bool> Filter { get { throw null; } set { } }
        public bool IncludeScopes { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.IDisposable BeginScope<TState>(TState state) { throw null; }
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) { throw null; }
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, System.Exception exception, System.Func<TState, System.Exception, string> formatter) { }
        public virtual void WriteMessage(Microsoft.Extensions.Logging.LogLevel logLevel, string logName, int eventId, string message, System.Exception exception) { }
    }
    public partial class ConsoleLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider, System.IDisposable
    {
        public ConsoleLoggerProvider(Microsoft.Extensions.Logging.Console.IConsoleLoggerSettings settings) { }
        public ConsoleLoggerProvider(System.Func<string, Microsoft.Extensions.Logging.LogLevel, bool> filter, bool includeScopes) { }
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string name) { throw null; }
        public void Dispose() { }
    }
    public partial class ConsoleLoggerSettings : Microsoft.Extensions.Logging.Console.IConsoleLoggerSettings
    {
        public ConsoleLoggerSettings() { }
        public Microsoft.Extensions.Primitives.IChangeToken ChangeToken { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool IncludeScopes { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Collections.Generic.IDictionary<string, Microsoft.Extensions.Logging.LogLevel> Switches { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.Extensions.Logging.Console.IConsoleLoggerSettings Reload() { throw null; }
        public bool TryGetSwitch(string name, out Microsoft.Extensions.Logging.LogLevel level) { level = default(Microsoft.Extensions.Logging.LogLevel); throw null; }
    }
    public partial class ConsoleLogScope
    {
        internal ConsoleLogScope() { }
        public static Microsoft.Extensions.Logging.Console.ConsoleLogScope Current { get { throw null; } set { } }
        public Microsoft.Extensions.Logging.Console.ConsoleLogScope Parent { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public static System.IDisposable Push(string name, object state) { throw null; }
        public override string ToString() { throw null; }
    }
    public partial interface IConsoleLoggerSettings
    {
        Microsoft.Extensions.Primitives.IChangeToken ChangeToken { get; }
        bool IncludeScopes { get; }
        Microsoft.Extensions.Logging.Console.IConsoleLoggerSettings Reload();
        bool TryGetSwitch(string name, out Microsoft.Extensions.Logging.LogLevel level);
    }
}
namespace Microsoft.Extensions.Logging.Console.Internal
{
    public partial class AnsiLogConsole : Microsoft.Extensions.Logging.Console.Internal.IConsole
    {
        public AnsiLogConsole(Microsoft.Extensions.Logging.Console.Internal.IAnsiSystemConsole systemConsole) { }
        public void Flush() { }
        public void Write(string message, System.Nullable<System.ConsoleColor> background, System.Nullable<System.ConsoleColor> foreground) { }
        public void WriteLine(string message, System.Nullable<System.ConsoleColor> background, System.Nullable<System.ConsoleColor> foreground) { }
    }
    public partial interface IAnsiSystemConsole
    {
        void Write(string message);
        void WriteLine(string message);
    }
    public partial interface IConsole
    {
        void Flush();
        void Write(string message, System.Nullable<System.ConsoleColor> background, System.Nullable<System.ConsoleColor> foreground);
        void WriteLine(string message, System.Nullable<System.ConsoleColor> background, System.Nullable<System.ConsoleColor> foreground);
    }
    public partial class WindowsLogConsole : Microsoft.Extensions.Logging.Console.Internal.IConsole
    {
        public WindowsLogConsole() { }
        public void Flush() { }
        public void Write(string message, System.Nullable<System.ConsoleColor> background, System.Nullable<System.ConsoleColor> foreground) { }
        public void WriteLine(string message, System.Nullable<System.ConsoleColor> background, System.Nullable<System.ConsoleColor> foreground) { }
    }
}
