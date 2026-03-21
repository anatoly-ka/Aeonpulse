using Aeonpulse.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aeonpulse.Services
{
    /// <summary>
    /// Lightweight static logging gateway for application-level diagnostics.
    ///
    /// <para>
    /// The logger factory is wired from <c>MauiProgram.CreateMauiApp()</c> via
    /// <see cref="Initialise"/>. Before initialisation (e.g. during unit tests),
    /// all log calls silently resolve to <see cref="NullLogger.Instance"/> and
    /// produce no output.
    /// </para>
    ///
    /// <para>
    /// <b>Message format convention:</b>
    /// Every message begins with bracketed tags that AI agents and developers can
    /// filter reliably in the debug output stream:
    /// <list type="bullet">
    ///   <item><description>
    ///     Short methods: <c>[CATEGORY] [SUBCATEGORY] message  key=value</c>
    ///   </description></item>
    ///   <item><description>
    ///     Long multi-phase methods: <c>[CATEGORY] [SUBCATEGORY] [BLOCK] message  key=value</c>
    ///   </description></item>
    /// </list>
    /// Use <c>[BLOCK]</c> only when a method contains named internal phases or a
    /// repeated scan loop where the same fields appear with different meanings
    /// (e.g. <c>CalculatePhotonPath</c>, <c>CalculateTimeJubilees</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Category tokens in use:</b>
    /// <c>BOOT</c>, <c>VM</c>, <c>CALC</c>, <c>NAV</c>, <c>THEME</c>,
    /// <c>LOCALE</c>, <c>TIMER</c>, <c>TINT</c>.
    /// </para>
    ///
    /// <para>
    /// <b>Active in DEBUG builds only.</b> All public methods carry
    /// <c>[Conditional("DEBUG")]</c> so their call sites are erased entirely from
    /// Release binaries - zero runtime overhead in production.
    /// </para>
    /// </summary>
    [AIContext("DiagnosticsGateway")]
    internal static class AeonLog
    {
        private static ILogger? _logger;

        /// <summary>
        /// Wires the logger factory. Call once from <c>MauiProgram.CreateMauiApp()</c>
        /// after <c>builder.Build()</c> returns, passing the built app's
        /// <see cref="ILoggerFactory"/>.
        /// </summary>
        internal static void Initialise(ILoggerFactory factory)
            => _logger = factory.CreateLogger("Aeonpulse");

        private static ILogger Logger => _logger ?? NullLogger.Instance;

        /// <summary>
        /// Emits a <c>LogDebug</c> entry: calculation inputs/outputs, method
        /// entry/exit, internal phase transitions.
        /// </summary>
        /// <param name="category">Top-level category token, e.g. <c>"CALC"</c>.</param>
        /// <param name="sub">Method or event name, e.g. <c>"CalculateCountdown"</c>.</param>
        /// <param name="message">Structured message with <c>key=value</c> pairs.</param>
        /// <param name="block">
        /// Optional internal phase tag for long multi-phase methods,
        /// e.g. <c>"UNIT_SCAN"</c>. Pass <see langword="null"/> for short methods.
        /// </param>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Debug(string category, string sub, string message, string? block = null)
        {
            if (block is null)
                Logger.LogDebug("[{Cat}] [{Sub}] {Msg}", category, sub, message);
            else
                Logger.LogDebug("[{Cat}] [{Sub}] [{Block}] {Msg}", category, sub, block, message);
        }

        /// <summary>
        /// Emits a <c>LogInformation</c> entry: user-driven actions such as date
        /// changes, settings changes, and language switches.
        /// </summary>
        /// <param name="category">Top-level category token.</param>
        /// <param name="sub">Method or event name.</param>
        /// <param name="message">Structured message with <c>key=value</c> pairs.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Info(string category, string sub, string message)
            => Logger.LogInformation("[{Cat}] [{Sub}] {Msg}", category, sub, message);

        /// <summary>
        /// Emits a <c>LogWarning</c> entry: unexpected but recoverable states.
        /// </summary>
        /// <param name="category">Top-level category token.</param>
        /// <param name="sub">Method or event name.</param>
        /// <param name="message">Structured message with <c>key=value</c> pairs.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Warn(string category, string sub, string message)
            => Logger.LogWarning("[{Cat}] [{Sub}] {Msg}", category, sub, message);
    }
}
