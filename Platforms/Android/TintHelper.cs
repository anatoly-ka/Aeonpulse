using Android.Graphics;
using Android.Widget;
using Aeonpulse.Helpers;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;

namespace Aeonpulse
{
    public static partial class MauiProgram
    {
        // Change 'private static' to 'internal static' to allow access within the assembly
        static partial void ApplyImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Microsoft.Maui.Graphics.Color? tint)
        {
            if (handler is null) return; // guard against failed 'as' cast upstream

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
                        (int)(tint.Red * 255),
                        (int)(tint.Green * 255),
                        (int)(tint.Blue * 255)),
                    Android.Graphics.PorterDuff.Mode.SrcIn!));
        }

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
                        (int)(tint.Red * 255),
                        (int)(tint.Green * 255),
                        (int)(tint.Blue * 255)),
                    Android.Graphics.PorterDuff.Mode.SrcIn!));
        }
    }
}
