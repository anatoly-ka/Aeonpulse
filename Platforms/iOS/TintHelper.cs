using Aeonpulse.Attributes;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Aeonpulse
{
    /// <summary>
    /// iOS implementation of the <see cref="MauiProgram"/> tint partials.
    /// Uses UIKit's <c>UIImageRenderingMode.AlwaysTemplate</c> rendering mode
    /// combined with <c>TintColor</c> to recolour image assets at the native layer.
    ///
    /// <para>
    /// <b>Why AlwaysTemplate:</b> this mode ignores the source image's colour data
    /// and uses <c>TintColor</c> for all opaque pixels, producing an exact single-colour
    /// icon — identical in appearance to the Android PorterDuff.SrcIn approach.
    /// </para>
    /// <para>
    /// <b>Clearing tint:</b> reverting to <c>AlwaysOriginal</c> restores the asset's
    /// natural colours and sets <c>TintColor</c> to <c>null</c>.
    /// </para>
    /// </summary>
    [AIContext("PlatformTintImplementation")]
    public static partial class MauiProgram
    {
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
            nativeImage.TintColor = UIColor.FromRGBA(
                (byte)(tint.Red   * 255),
                (byte)(tint.Green * 255),
                (byte)(tint.Blue  * 255),
                (byte)(tint.Alpha * 255));
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
            nativeBtn.TintColor = UIColor.FromRGBA(
                (byte)(tint.Red   * 255),
                (byte)(tint.Green * 255),
                (byte)(tint.Blue  * 255),
                (byte)(tint.Alpha * 255));
        }

        // iOS uses AlwaysTemplate rendering mode; the tint is applied synchronously
        // when UIImage is rendered, so no deferred post-decode callback is needed.
        static partial void ApplyDeferredImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Microsoft.Maui.Graphics.Color? tint)
        {
        }

        internal static partial Task PrewarmTintCache(string fileName, Microsoft.Maui.Graphics.Color tint)
            => Task.CompletedTask;

        /// <summary>iOS: <c>MauiAsset</c> files are bundled in the app package; <c>FromFile</c> resolves correctly.</summary>
        internal static partial ImageSource LandmarkImageSource(string fileName)
            => ImageSource.FromFile(fileName);
    }
}