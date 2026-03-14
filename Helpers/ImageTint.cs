using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Aeonpulse.Helpers
{
    /// <summary>
    /// Attached property that applies a true pixel-level tint to an
    /// <see cref="Image"/> or <see cref="ImageButton"/> via platform
    /// handler mappers registered in <see cref="MauiProgram"/>.
    ///
    /// Usage in XAML:
    ///   xmlns:helpers="clr-namespace:Aeonpulse.Helpers"
    ///   helpers:ImageTint.Color="{DynamicResource CyberCyan}"
    /// </summary>
    public static class ImageTint
    {
        public static readonly BindableProperty ColorProperty =
            BindableProperty.CreateAttached(
                "Color",
                typeof(Color),
                typeof(ImageTint),
                null,
                propertyChanged: OnColorChanged);

        public static Color? GetColor(BindableObject view) =>
            (Color?)view.GetValue(ColorProperty);

        public static void SetColor(BindableObject view, Color value) =>
            view.SetValue(ColorProperty, value);

        private static void OnColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // Trigger the handler mapper by invalidating the handler.
            if (bindable is IView view)
                view.Handler?.UpdateValue(nameof(ColorProperty));
        }
    }
}