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
    }
}
