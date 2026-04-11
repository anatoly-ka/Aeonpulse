using Aeonpulse.Attributes;
using Aeonpulse.Helpers;
using Aeonpulse.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Aeonpulse
{
    /// <summary>
    /// The MAUI application host builder. Registers fonts, configures the
    /// <see cref="ImageTint"/> cross-platform tinting system, and wires the
    /// dependency-injection container.
    ///
    /// <para>
    /// <b>Hidden dependency — partial class tint pipeline:</b>
    /// <c>ApplyImageTint</c> and <c>ApplyImageButtonTint</c> are declared as
    /// <c>partial</c> stubs here; their platform-specific implementations live in
    /// <c>Platforms\{Platform}\TintHelper.cs</c>.  The build system selects the
    /// correct implementation at compile time via the MAUI multi-targeting mechanism.
    /// </para>
    /// <para>
    /// <b>Side effect:</b> handler mapper callbacks are registered globally into
    /// <c>Microsoft.Maui.Handlers.ImageHandler.Mapper</c> at app startup and affect
    /// every <see cref="Image"/> and <see cref="ImageButton"/> in the app that carries
    /// the <c>helpers:ImageTint.Color</c> attached property.
    /// </para>
    /// </summary>
    [AIContext("AppBootstrap")]
    public static partial class MauiProgram
    {
        /// <summary>
        /// Builds and returns the <see cref="MauiApp"/> instance used as the application root.
        /// Called once from each platform entry point (e.g., <c>AppDelegate</c>, <c>MainActivity</c>).
        /// </summary>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
                    // Append to the global ImageHandler mapper so every Image that has
                    // helpers:ImageTint.Color set will invoke the platform tint implementation.
                    Microsoft.Maui.Handlers.ImageHandler.Mapper.AppendToMapping(
                        nameof(ImageTint.ColorProperty),
                        (handler, view) =>
                        {
                            if (view is not BindableObject bindable) return;
                            var tint = ImageTint.GetColor(bindable);
                            if (handler is Microsoft.Maui.Handlers.ImageHandler imageHandler)
                                ApplyImageTint(imageHandler, tint);
                        });

                    // Also hook the Source mapper for ImageHandler so the tint is re-applied
                    // immediately after MAUI sets or resets the native image source.
                    // This covers Image controls whose Source is set programmatically at runtime
                    // (e.g. LandmarkImage in the Your Breath expanded card): without this hook
                    // the ColorProperty mapper fires once on the initial XAML DynamicResource
                    // binding but the new native image loaded on every source change is un-tinted.
                    Microsoft.Maui.Handlers.ImageHandler.Mapper.AppendToMapping(
                        "Source",
                        (handler, view) =>
                        {
                            if (view is not BindableObject bindable) return;
                            var tint = ImageTint.GetColor(bindable);
                            if (tint is null) return;
                            if (handler is Microsoft.Maui.Handlers.ImageHandler imageHandler)
                            {
                                ApplyImageTint(imageHandler, tint);
                                // Also schedule a deferred re-apply for platforms (Android) where
                                // the image decode is async and completes after this mapper fires.
                                ApplyDeferredImageTint(imageHandler, tint);
                            }
                        });

                    // Same pattern for ImageButton (e.g., toolbar icon buttons).
                    Microsoft.Maui.Handlers.ImageButtonHandler.Mapper.AppendToMapping(
                        nameof(ImageTint.ColorProperty),
                        (handler, view) =>
                        {
                            if (view is not BindableObject bindable) return;
                            var tint = ImageTint.GetColor(bindable);
                            if (handler is Microsoft.Maui.Handlers.ImageButtonHandler imageButtonHandler)
                                ApplyImageButtonTint(imageButtonHandler, tint);
                        });

                    // Also hook the Source mapper for ImageButton so the tint is re-applied
                    // immediately after MAUI sets or resets the native image source.
                    // This covers two cases that the ColorProperty mapper alone cannot handle:
                    //   1. Buttons inside a collapsed section: their WinUI Button ControlTemplate
                    //      is not applied until the section first becomes visible, so MAUI fires
                    //      the Source mapper at that point to paint the image for the first time.
                    //   2. Chevron buttons whose Source binding flips (BoolToImageSourceConverter):
                    //      MAUI fires the Source mapper with the new filename, which would
                    //      overwrite our previously-tinted WriteableBitmap with an un-tinted one.
                    // Appending here ensures tint always runs right after MAUI sets the source.
                    Microsoft.Maui.Handlers.ImageButtonHandler.Mapper.AppendToMapping(
                        "Source",
                        (handler, view) =>
                        {
                            if (view is not BindableObject bindable) return;
                            var tint = ImageTint.GetColor(bindable);
                            if (tint is null) return;
                            if (handler is Microsoft.Maui.Handlers.ImageButtonHandler imageButtonHandler)
                                ApplyImageButtonTint(imageButtonHandler, tint);
                        });

                    // Platform hook for any GIF-related mapper setup. Currently a no-op on
                    // all platforms: MAUI's built-in GIF decoder (ParseGIFBitmapHeaderAsync)
                    // handles animated GIF playback on Windows natively via frame-by-frame
                    // WriteableBitmap rendering. No platform intervention is needed or wanted.
                    ApplyGifToStaticPngMapper();
                });

#if DEBUG
            builder.Logging.AddDebug();
            // Opt-in file sink for diagnostics. Set env var AEONPULSE_LOG=1 before
            // launching the app to write all Debug/Info/Warn output to a log file.
            // Windows: %TEMP%\aeonpulse_debug.log
            // Useful for capturing timing data or diagnosing startup issues without
            // an attached debugger. See Agents.md §8.4 and IMPLEMENTATION_GUIDE.md §5.
#if WINDOWS
            if (Environment.GetEnvironmentVariable("AEONPULSE_LOG") == "1")
            {
                var logPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(), "aeonpulse_debug.log");
                builder.Logging.AddProvider(new FileLoggerProvider(logPath));
                builder.Logging.SetMinimumLevel(LogLevel.Debug);
                System.Diagnostics.Debug.WriteLine($"[AEONPULSE_LOG] file sink active  path={logPath}");
            }
#endif
#if ANDROID
            // AddDebug() on Android only emits to the Mono debugger channel, which
            // requires an attached debugger. Add a direct android.util.Log sink so
            // structured AeonLog output (MEM, BOOT, CALC, etc.) is always visible
            // in adb logcat under the tag "Aeonpulse" without requiring VS attached.
            builder.Logging.AddProvider(new AndroidLogcatLoggerProvider());
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif
#endif

            var app = builder.Build();
            AeonLog.Initialise(app.Services.GetRequiredService<ILoggerFactory>());
            return app;
        }

        /// <summary>
        /// Platform partial: applies a pixel-level colour filter to an <see cref="Image"/>
        /// native view. Implemented per-platform in <c>Platforms\{Platform}\TintHelper.cs</c>.
        /// </summary>
        /// <param name="handler">The resolved MAUI image handler.</param>
        /// <param name="tint">
        /// The desired tint colour; <c>null</c> clears any existing filter.
        /// </param>
        static partial void ApplyImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Microsoft.Maui.Graphics.Color? tint);

        /// <summary>
        /// Platform partial: applies a pixel-level colour filter to an <see cref="ImageButton"/>
        /// native view. Implemented per-platform in <c>Platforms\{Platform}\TintHelper.cs</c>.
        /// </summary>
        /// <param name="handler">The resolved MAUI image-button handler.</param>
        /// <param name="tint">
        /// The desired tint colour; <c>null</c> clears any existing filter.
        /// </param>
        static partial void ApplyImageButtonTint(
            Microsoft.Maui.Handlers.ImageButtonHandler handler, Microsoft.Maui.Graphics.Color? tint);

        /// <summary>
        /// Platform partial: hook point for any GIF-animation setup needed per platform.
        /// On Windows this is a no-op: MAUI's built-in <c>ParseGIFBitmapHeaderAsync</c>
        /// decoder in <c>Microsoft.Maui.Controls</c> animates GIFs natively on all
        /// platforms including Windows, so no platform-side intervention is required.
        /// </summary>
        static partial void ApplyGifToStaticPngMapper();

        /// <summary>
        /// Platform partial: re-applies the image tint after the native image decode
        /// completes asynchronously.
        ///
        /// <para>
        /// On Android, MAUI's <c>FileImageSource</c> loading is async (AssetManager decode
        /// runs off the main thread). The <c>"Source"</c> mapper fires synchronously before
        /// the decoded bitmap is set on the <c>ImageView</c>, so a second deferred call is
        /// needed. Android overrides this partial to post a <c>View.Post</c> callback that
        /// re-applies the <c>PorterDuff.SrcIn</c> filter after the decode completes.
        /// Other platforms handle this via <c>ImageOpened</c> subscription or synchronous
        /// loading and provide an empty implementation.
        /// </para>
        /// </summary>
        static partial void ApplyDeferredImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Microsoft.Maui.Graphics.Color? tint);

        /// <summary>
        /// Platform partial: pre-warms the Win2D tint cache for a named image file and
        /// tint colour so that the first <c>ScheduleTint</c> for that combination hits the
        /// cache and executes in microseconds rather than performing file I/O and Win2D
        /// rendering inline on the UI-thread dispatcher queue.
        ///
        /// <para>
        /// Windows implements this by running <see cref="GetTintedBitmapAsync"/> inline
        /// (awaited directly on the UI thread) so the cache is populated before the
        /// caller assigns <c>Image.Source</c> and triggers the tint pipeline.
        /// Other platforms return <see cref="Task.CompletedTask"/>.
        /// </para>
        /// </summary>
        internal static partial Task PrewarmTintCache(string fileName, Microsoft.Maui.Graphics.Color tint);

        /// <summary>
        /// Platform partial: creates the correct <see cref="ImageSource"/> for a landmark
        /// PNG so that MAUI's native image pipeline fires <c>ImageOpened</c> on the
        /// platform's native image control, enabling the tint pipeline to re-apply after
        /// every async decode.
        ///
        /// <para>
        /// <b>Windows:</b> returns <see cref="FileImageSource"/> (<c>ImageSource.FromFile</c>).
        /// Landmark PNGs are <c>MauiAsset</c> files copied verbatim to
        /// <c>AppContext.BaseDirectory</c>; <c>BitmapImage</c> fires <c>ImageOpened</c>
        /// when loading from a file URI, which lets <c>AttachAndTint</c>'s handler
        /// retint after every source change. <c>StreamImageSource</c> causes WinUI to use
        /// <c>WriteableBitmap</c> internally, which does <em>not</em> fire
        /// <c>ImageOpened</c>, leaving the raw untinted bitmap visible.
        /// </para>
        /// <para>
        /// <b>Android:</b> returns <see cref="StreamImageSource"/> via
        /// <c>FileSystem.OpenAppPackageFileAsync</c>. <c>ImageSource.FromFile</c> on
        /// Android uses a bare filesystem path which Glide/BitmapFactory cannot resolve
        /// for <c>MauiAsset</c> files; the stream API goes through <c>AssetManager</c>.
        /// </para>
        /// <para>
        /// <b>iOS / macCatalyst:</b> returns <see cref="FileImageSource"/>;
        /// <c>MauiAsset</c> files are bundled in the app package and accessible by name.
        /// </para>
        /// </summary>
        internal static partial ImageSource LandmarkImageSource(string fileName);
    }

#if DEBUG
    /// <summary>
    /// Captures a point-in-time memory snapshot and emits it via <see cref="AeonLog"/>
    /// under the <c>MEM</c> category. All fields use <c>[BLOCK]</c> tags so each
    /// dimension can be filtered independently.
    ///
    /// <para>Compiled in only for <c>DEBUG</c> builds.
    /// Zero Release overhead.</para>
    /// </summary>
    internal static class MemSnapshot
    {
        /// <summary>
        /// Emits one memory snapshot log group. Blocks are:
        /// HEAP (managed GC bytes), GC (collection counts per generation),
        /// PROCESS (OS working set), TINT_CACHE (Win2D WriteableBitmap pool, Windows only),
        /// NATIVE_HEAP (Dalvik/ART native heap, Android only).
        /// </summary>
        /// <param name="label">
        /// Free-form label for the snapshot point, e.g. <c>"POST_WARM"</c>,
        /// <c>"MAIN_READY"</c>, <c>"T30"</c>, <c>"T120"</c>.
        /// </param>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Emit(string label)
        {
            // Managed heap - do NOT force a GC; non-intrusive measurement.
            long heapBytes = GC.GetTotalMemory(forceFullCollection: false);
            int  gen0      = GC.CollectionCount(0);
            int  gen1      = GC.CollectionCount(1);
            int  gen2      = GC.CollectionCount(2);

            // OS working set via Environment (no System.Diagnostics.Process needed).
            long wsBytes   = Environment.WorkingSet;

            AeonLog.Info("MEM", label, $"snapshot  wall={DateTime.Now:HH:mm:ss.fff}");
            AeonLog.Info("MEM", label,
                $"managed_heap_MB={heapBytes / 1_048_576.0:F2}  heap_bytes={heapBytes:N0}",
                "HEAP");
            AeonLog.Info("MEM", label,
                $"gen0={gen0}  gen1={gen1}  gen2={gen2}",
                "GC");
            AeonLog.Info("MEM", label,
                $"working_set_MB={wsBytes / 1_048_576.0:F2}  working_set_bytes={wsBytes:N0}",
                "PROCESS");

#if WINDOWS
            // Win2D WriteableBitmap pixel-buffer pool (Windows only).
            int tintCount = MauiProgram.TintCacheCount;
            AeonLog.Info("MEM", label,
                $"tint_cache_entries={tintCount}",
                "TINT_CACHE");
#endif

#if ANDROID
            // Dalvik/ART heap and native heap sizes via Android.OS.Debug.
            // getNativeHeapAllocatedSize / getNativeHeapSize are always available
            // without permissions and reflect the current process's native allocations.
            long nativeAlloc = Android.OS.Debug.NativeHeapAllocatedSize;
            long nativeSize  = Android.OS.Debug.NativeHeapSize;
            var  mi          = new Android.OS.Debug.MemoryInfo();
            Android.OS.Debug.GetMemoryInfo(mi);
            AeonLog.Info("MEM", label,
                $"native_alloc_MB={nativeAlloc / 1_048_576.0:F2}  native_size_MB={nativeSize / 1_048_576.0:F2}",
                "NATIVE_HEAP");
            AeonLog.Info("MEM", label,
                $"pss_MB={mi.TotalPss / 1024.0:F2}  private_dirty_MB={mi.TotalPrivateDirty / 1024.0:F2}  private_clean_MB={mi.TotalPrivateClean / 1024.0:F2}",
                "PSS");
#endif
        }
    }
#endif

#if DEBUG && WINDOWS
    /// <summary>
    /// Minimal file-based <see cref="ILoggerProvider"/> used only when
    /// <c>AEONPULSE_LOG=1</c> is set in the environment. Writes every log entry
    /// as a single timestamped line; flushed after each write.
    /// Compiled out in Release builds via <c>#if DEBUG</c>.
    /// </summary>
    internal sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly System.IO.StreamWriter _writer;

        public FileLoggerProvider(string path)
        {
            _writer = new System.IO.StreamWriter(path, append: false) { AutoFlush = true };
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(_writer, categoryName);

        public void Dispose() => _writer.Dispose();

        private sealed class FileLogger : ILogger
        {
            private readonly System.IO.StreamWriter _w;
            private readonly string _cat;
            public FileLogger(System.IO.StreamWriter w, string cat) { _w = w; _cat = cat; }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel level) => level >= LogLevel.Debug;

            public void Log<TState>(LogLevel level, EventId id, TState state,
                Exception? ex, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(level)) return;
                var prefix = level switch
                {
                    LogLevel.Debug       => "dbg",
                    LogLevel.Information => "inf",
                    LogLevel.Warning     => "wrn",
                    LogLevel.Error       => "err",
                    _                    => "???"
                };
                _w.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {prefix} [{_cat}] {formatter(state, ex)}");
            }
        }
    }
#endif
}
