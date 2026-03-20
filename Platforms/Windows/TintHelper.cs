using Aeonpulse.Attributes;
using Microsoft.Maui.Graphics;
using WColor = Windows.UI.Color;
using WinUIButton = Microsoft.UI.Xaml.Controls.Button;
using WinUIImage = Microsoft.UI.Xaml.Controls.Image;
using WinUISolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Aeonpulse
{
    /// <summary>
    /// Windows (WinUI 3) implementation of the <see cref="MauiProgram"/> tint partials.
    ///
    /// <para>
    /// <b>Known limitation — <see cref="Image"/> tinting:</b> WinUI 3's
    /// <c>Microsoft.UI.Xaml.Controls.Image</c> has no <c>Foreground</c> property and
    /// does not support pixel-level colour filters natively. A full implementation
    /// would require a <c>WriteableBitmap</c> pixel-manipulation pass or a custom
    /// shader effect. This is currently a no-op; Windows icons will display in their
    /// original colours.
    /// </para>
    /// <para>
    /// <b><see cref="ImageButton"/> tinting</b> is supported via
    /// <c>Button.Foreground</c> -> <see cref="WinUISolidColorBrush"/>, which colours
    /// the button's glyph/icon foreground layer on WinUI.
    /// </para>
    /// </summary>
    [AIContext("PlatformTintImplementation")]
    public static partial class MauiProgram
    {
        /// <summary>
        /// No-op on Windows: WinUI <c>Image</c> does not support colour filters.
        /// A future implementation could use <c>WriteableBitmap</c> pixel manipulation.
        /// </summary>
        static partial void ApplyImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Color? tint)
        {
            if (handler.PlatformView is not WinUIImage nativeImage)
                return;

            // WinUI Image has no Foreground; tinting requires an ImageBrush + shader effect.
            // No-op for now.
        }

        /// <summary>
        /// Applies or clears a <see cref="WinUISolidColorBrush"/> on the native
        /// WinUI <c>Button.Foreground</c> to approximate icon tinting on Windows.
        /// </summary>
        static partial void ApplyImageButtonTint(
            Microsoft.Maui.Handlers.ImageButtonHandler handler, Color? tint)
        {
            if (handler.PlatformView is not WinUIButton nativeBtn)
                return;

            nativeBtn.Foreground = tint is null
                ? null
                : new WinUISolidColorBrush(WColor.FromArgb(
                    (byte)(tint.Alpha * 255),
                    (byte)(tint.Red   * 255),
                    (byte)(tint.Green * 255),
                    (byte)(tint.Blue  * 255)));
        }
    }
}