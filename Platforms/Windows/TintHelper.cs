using Microsoft.Maui.Graphics;
using WColor = Windows.UI.Color;
using WinUIButton = Microsoft.UI.Xaml.Controls.Button;
using WinUIImage = Microsoft.UI.Xaml.Controls.Image;
using WinUISolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Aeonpulse
{
    public static partial class MauiProgram
    {
        static partial void ApplyImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Color? tint)
        {
            if (handler.PlatformView is not WinUIImage nativeImage)
                return;

            // WinUI Image has no Foreground; tinting requires an ImageBrush + shader effect.
            // No-op for now.
        }

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