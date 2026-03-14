using UIKit;
using Microsoft.Maui.Graphics;

namespace Aeonpulse
{
    public static partial class MauiProgram
    {
        private static UIColor ToUIColor(Color color)
        {
            var r = (byte)(Math.Clamp(color.Red,   0, 1) * 255);
            var g = (byte)(Math.Clamp(color.Green, 0, 1) * 255);
            var b = (byte)(Math.Clamp(color.Blue,  0, 1) * 255);
            var a = (byte)(Math.Clamp(color.Alpha, 0, 1) * 255);
            return UIColor.FromRGBA(r, g, b, a);
        }

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
    }
}