#if DEBUG
using Microsoft.Extensions.Logging;

namespace Aeonpulse
{
    /// <summary>
    /// <c>ILoggerProvider</c> that writes every log entry directly to
    /// <c>android.util.Log</c> so structured <c>AeonLog</c> output is visible
    /// in <c>adb logcat</c> without requiring an attached debugger.
    ///
    /// <para>
    /// <c>AddDebug()</c> on Android routes through <c>System.Diagnostics.Debug</c>
    /// which only emits to the Mono debugger channel. This provider bypasses that
    /// limitation by calling <c>Android.Util.Log.Debug/Info/Warn/Error</c> directly.
    /// </para>
    ///
    /// <para>All entries appear under logcat tag <c>"Aeonpulse"</c>:</para>
    /// <code>
    /// adb logcat -s Aeonpulse:V
    /// </code>
    ///
    /// <para>Compiled in only for <c>DEBUG</c> Android builds. Zero Release overhead.</para>
    /// </summary>
    internal sealed class AndroidLogcatLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
            => new AndroidLogcatLogger(categoryName);

        public void Dispose() { }

        private sealed class AndroidLogcatLogger : ILogger
        {
            // All messages share the same tag so a single adb filter covers everything.
            private const string Tag = "Aeonpulse";
            private readonly string _category;

            public AndroidLogcatLogger(string category) { _category = category; }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel level) => level >= LogLevel.Debug;

            public void Log<TState>(
                LogLevel level, EventId id, TState state,
                Exception? ex, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(level)) return;
                var msg = formatter(state, ex);
                switch (level)
                {
                    case LogLevel.Debug:
                        Android.Util.Log.Debug(Tag, msg);
                        break;
                    case LogLevel.Information:
                        Android.Util.Log.Info(Tag, msg);
                        break;
                    case LogLevel.Warning:
                        Android.Util.Log.Warn(Tag, msg);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        Android.Util.Log.Error(Tag, msg);
                        break;
                    default:
                        Android.Util.Log.Verbose(Tag, msg);
                        break;
                }
            }
        }
    }
}
#endif
