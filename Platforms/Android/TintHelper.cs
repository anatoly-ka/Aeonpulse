using Android.Graphics;
using Android.Widget;
using Aeonpulse.Attributes;
using Aeonpulse.Helpers;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;

namespace Aeonpulse
{
    /// <summary>
    /// Android implementation of the <see cref="MauiProgram"/> tint partials.
    /// Applies a <c>PorterDuff.SrcIn</c> colour filter directly to the native
    /// <c>ImageView</c> / <c>ShapeableImageView</c> for true pixel-level tinting.
    ///
    /// <para>
    /// <b>Why PorterDuff.SrcIn:</b> this mode replaces every non-transparent pixel's
    /// colour with the tint colour while preserving the source alpha mask, giving a
    /// clean single-colour icon effect regardless of the original asset's palette.
    /// </para>
    /// <para>
    /// <b>Hidden dependency:</b> <c>ImageButtonHandler.PlatformView</c> on Android
    /// resolves to <c>Google.Android.Material.ImageView.ShapeableImageView</c>,
    /// not the standard <c>Android.Widget.ImageButton</c> — hence the explicit type check.
    /// </para>
    /// </summary>
    [AIContext("PlatformTintImplementation")]
    public static partial class MauiProgram
    {
        /// <summary>
        /// Applies or clears the tint colour filter on the native Android <c>ImageView</c>.
        /// </summary>
        static partial void ApplyImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Microsoft.Maui.Graphics.Color? tint)
        {
            if (handler is null) return;

            if (handler.PlatformView is not Android.Widget.ImageView nativeImage)
                return;

            if (tint is null)
            {
                nativeImage.ClearColorFilter();
                return;
            }

            nativeImage.SetColorFilter(
                new Android.Graphics.PorterDuffColorFilter(
                    AColor.Argb(
                        (int)(tint.Alpha * 255),
                        (int)(tint.Red   * 255),
                        (int)(tint.Green * 255),
                        (int)(tint.Blue  * 255)),
                    Android.Graphics.PorterDuff.Mode.SrcIn!));
        }

        /// <summary>
        /// Applies or clears the tint colour filter on the native Android
        /// <c>ShapeableImageView</c> that backs a MAUI <c>ImageButton</c>.
        /// </summary>
        static partial void ApplyImageButtonTint(
            Microsoft.Maui.Handlers.ImageButtonHandler handler, Microsoft.Maui.Graphics.Color? tint)
        {
            // ImageButtonHandler.PlatformView is a ShapeableImageView, not Android.Widget.ImageButton.
            if (handler.PlatformView is not Google.Android.Material.ImageView.ShapeableImageView nativeBtn)
                return;

            if (tint is null)
            {
                nativeBtn.ClearColorFilter();
                return;
            }

            nativeBtn.SetColorFilter(
                new Android.Graphics.PorterDuffColorFilter(
                    AColor.Argb(
                        (int)(tint.Alpha * 255),
                        (int)(tint.Red   * 255),
                        (int)(tint.Green * 255),
                        (int)(tint.Blue  * 255)),
                    Android.Graphics.PorterDuff.Mode.SrcIn!));
        }
    }
}
