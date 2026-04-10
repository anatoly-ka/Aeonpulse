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

        /// <summary>
        /// Posts a deferred re-apply of the <c>PorterDuff.SrcIn</c> colour filter via
        /// <c>View.Post()</c> so it fires after the async <c>AssetManager</c> image decode
        /// completes and MAUI calls <c>setImageDrawable</c> on the native <c>ImageView</c>.
        ///
        /// <para>
        /// <b>Why deferred:</b> MAUI's Android <c>FileImageSourceService</c> decodes
        /// the bitmap off the main thread. The <c>"Source"</c> mapper callback fires
        /// synchronously before decoding finishes, so <c>ApplyImageTint</c> sets the filter
        /// on a view that has no drawable yet. <c>setImageDrawable</c> re-applies the
        /// stored <c>mColorFilter</c> internally, so the filter set before decode would
        /// survive -- however in practice MAUI may call <c>ClearColorFilter()</c> as part
        /// of its load lifecycle. <c>View.Post()</c> enqueues a runnable that executes
        /// after the current frame including the async decode callback, guaranteeing the
        /// filter is applied to the final decoded drawable.
        /// </para>
        /// </summary>
        static partial void ApplyDeferredImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Microsoft.Maui.Graphics.Color? tint)
        {
            if (handler is null) return;
            if (handler.PlatformView is not Android.Widget.ImageView nativeImage) return;
            if (tint is null) return;

            var filter = new Android.Graphics.PorterDuffColorFilter(
                AColor.Argb(
                    (int)(tint.Alpha * 255),
                    (int)(tint.Red   * 255),
                    (int)(tint.Green * 255),
                    (int)(tint.Blue  * 255)),
                Android.Graphics.PorterDuff.Mode.SrcIn!);

            // Post to the view's message queue: runs after the async decode callback
            // has called setImageDrawable on the native ImageView.
            nativeImage.Post(() => nativeImage.SetColorFilter(filter));
        }

        internal static partial Task PrewarmTintCache(string fileName, Microsoft.Maui.Graphics.Color tint)
            => Task.CompletedTask;

        /// <summary>
        /// Android: <c>MauiAsset</c> files live in the APK <c>assets/</c> directory.
        /// <c>ImageSource.FromFile</c> uses a bare filesystem path which Glide cannot
        /// resolve; <c>FromStream</c> via <c>OpenAppPackageFileAsync</c> calls
        /// <c>AssetManager.Open()</c> and is the correct approach on Android.
        /// </summary>
        internal static partial ImageSource LandmarkImageSource(string fileName)
            => ImageSource.FromStream(ct => FileSystem.OpenAppPackageFileAsync(fileName));
    }
}
