using Aeonpulse.Attributes;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Aeonpulse
{
    /// <summary>
    /// Mac Catalyst implementation of the <see cref="MauiProgram"/> tint partials.
    /// Functionally equivalent to the iOS implementation, but adds a
    /// <c>ToUIColor</c> helper with <see cref="Math.Clamp"/> guards to handle
    /// any out-of-range float components that can arise from programmatic colour
    /// construction on the Mac Catalyst layer.
    /// </summary>
    [AIContext("PlatformTintImplementation")]
    public static partial class MauiProgram
    {
        /// <summary>
        /// Converts a MAUI <see cref="Color"/> to a <c>UIColor</c>, clamping each
        /// component to [0, 1] before conversion to avoid native API argument exceptions.
        /// </summary>
        private static UIColor ToUIColor(Color color)
        {
            var r = (byte)(Math.Clamp(color.Red,   0, 1) * 255);
            var g = (byte)(Math.Clamp(color.Green, 0, 1) * 255);
            var b = (byte)(Math.Clamp(color.Blue,  0, 1) * 255);
            var a = (byte)(Math.Clamp(color.Alpha, 0, 1) * 255);
            return UIColor.FromRGBA(r, g, b, a);
        }

        /// <summary>
        /// Applies or clears the tint on the native <c>UIImageView</c> backing a MAUI <see cref="Image"/>.
        /// </summary>
        static partial void ApplyImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Color? tint)
        {
            if (handler.PlatformView is not UIImageView nativeImage)
                return;

            if (tint is null)
            {
                nativeImage.Image = nativeImage.Image?.ImageWithRenderingMode(
                    UIImageRenderingMode.AlwaysOriginal);
                nativeImage.TintColor = null;
                return;
            }

            nativeImage.Image = nativeImage.Image?.ImageWithRenderingMode(
                UIImageRenderingMode.AlwaysTemplate);
            nativeImage.TintColor = ToUIColor(tint);
        }

        /// <summary>
        /// Applies or clears the tint on the native <c>UIButton</c> backing a MAUI <see cref="ImageButton"/>.
        /// </summary>
        static partial void ApplyImageButtonTint(
            Microsoft.Maui.Handlers.ImageButtonHandler handler, Color? tint)
        {
            if (handler.PlatformView is not UIButton nativeBtn)
                return;

            if (tint is null)
            {
                var img = nativeBtn.CurrentImage?.ImageWithRenderingMode(
                    UIImageRenderingMode.AlwaysOriginal);
                nativeBtn.SetImage(img, UIControlState.Normal);
                nativeBtn.TintColor = null;
                return;
            }

            var tintedImg = nativeBtn.CurrentImage?.ImageWithRenderingMode(
                UIImageRenderingMode.AlwaysTemplate);
            nativeBtn.SetImage(tintedImg, UIControlState.Normal);
            nativeBtn.TintColor = ToUIColor(tint);
        }

        // Mac Catalyst uses AlwaysTemplate rendering mode; the tint is applied synchronously,
        // so no deferred post-decode callback is needed.
        static partial void ApplyDeferredImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Microsoft.Maui.Graphics.Color? tint)
        {
        }

        internal static partial Task PrewarmTintCache(string fileName, Microsoft.Maui.Graphics.Color tint)
            => Task.CompletedTask;

        /// <summary>macCatalyst: <c>MauiAsset</c> files are bundled in the app package; <c>FromFile</c> resolves correctly.</summary>
        internal static partial ImageSource LandmarkImageSource(string fileName)
            => ImageSource.FromFile(fileName);
    }
}