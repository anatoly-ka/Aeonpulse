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
}
