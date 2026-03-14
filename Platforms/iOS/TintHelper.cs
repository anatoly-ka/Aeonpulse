using UIKit;
using Microsoft.Maui.Graphics;

namespace Aeonpulse
{
    public static partial class MauiProgram
    {
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
    }
}