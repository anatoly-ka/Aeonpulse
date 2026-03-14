using Aeonpulse.Helpers;
using Aeonpulse.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Aeonpulse
{
    public static partial class MauiProgram
    {
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
                    Microsoft.Maui.Handlers.ImageHandler.Mapper.AppendToMapping(
                        nameof(ImageTint.ColorProperty),
                        (handler, view) =>
                        {
                            if (view is not BindableObject bindable) return;
                            var tint = ImageTint.GetColor(bindable);
                            if (handler is Microsoft.Maui.Handlers.ImageHandler imageHandler)
                                ApplyImageTint(imageHandler, tint);
                        });

                    Microsoft.Maui.Handlers.ImageButtonHandler.Mapper.AppendToMapping(
                        nameof(ImageTint.ColorProperty),
                        (handler, view) =>
                        {
                            if (view is not BindableObject bindable) return;
                            var tint = ImageTint.GetColor(bindable);
                            if (handler is Microsoft.Maui.Handlers.ImageButtonHandler imageButtonHandler)
                                ApplyImageButtonTint(imageButtonHandler, tint);
                        });
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        // Partial declarations only - implementations live in Platforms\<Platform>\TintHelper.cs
        static partial void ApplyImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Microsoft.Maui.Graphics.Color? tint);

        static partial void ApplyImageButtonTint(
            Microsoft.Maui.Handlers.ImageButtonHandler handler, Microsoft.Maui.Graphics.Color? tint);
    }
}
