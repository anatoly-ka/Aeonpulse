using Aeonpulse.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Aeonpulse.Helpers
{
    /// <summary>
    /// Attached property that applies a true pixel-level tint to an
    /// <see cref="Image"/> or <see cref="ImageButton"/> via platform
    /// handler mappers registered in <see cref="MauiProgram"/>.
    ///
    /// <para>
    /// <b>Why this exists:</b> MAUI's cross-platform <see cref="Image"/> has no
    /// built-in tint/colorize API. This attached property bridges the gap by
    /// delegating to native colour-filter APIs (<c>PorterDuff.SrcIn</c> on Android,
    /// <c>UIImageRenderingMode.AlwaysTemplate</c> on iOS/Mac Catalyst,
    /// <c>SolidColorBrush.Foreground</c> on Windows) without requiring per-image
    /// subclasses or behaviours.
    /// </para>
    /// <para>
    /// <b>Hidden dependency:</b> this property only takes effect if the corresponding
    /// handler mapper is registered in <see cref="MauiProgram.CreateMauiApp"/>.
    /// Setting the property without that registration is silently a no-op.
    /// </para>
    ///
    /// Usage in XAML:
    /// <code>
    ///   xmlns:helpers="clr-namespace:Aeonpulse.Helpers"
    ///   helpers:ImageTint.Color="{DynamicResource CyberCyan}"
    /// </code>
    /// </summary>
    [AIContext("PlatformAbstractionHelper")]
    public static class ImageTint
    {
        /// <summary>
        /// Gets or sets the tint colour applied to the attached <see cref="Image"/>
        /// or <see cref="ImageButton"/>. Setting to <c>null</c> clears the colour filter.
        /// Supports <c>DynamicResource</c> — changing the app theme will re-invoke the
        /// platform handler and update the filter in real time.
        /// </summary>
        public static readonly BindableProperty ColorProperty =
            BindableProperty.CreateAttached(
                "Color",
                typeof(Color),
                typeof(ImageTint),
                null,
                propertyChanged: OnColorChanged);

        /// <summary>Gets the current tint colour from the target <paramref name="view"/>.</summary>
        public static Color? GetColor(BindableObject view) =>
            (Color?)view.GetValue(ColorProperty);

        /// <summary>Sets the tint colour on the target <paramref name="view"/>.</summary>
        public static void SetColor(BindableObject view, Color value) =>
            view.SetValue(ColorProperty, value);

        /// <summary>
        /// Triggers the platform handler pipeline whenever the tint colour changes,
        /// by invalidating the handler's value mapping for <see cref="ColorProperty"/>.
        /// This causes MAUI to re-invoke the mapper registered in <see cref="MauiProgram"/>.
        /// </summary>
        private static void OnColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is IView view)
                view.Handler?.UpdateValue(nameof(ColorProperty));
        }
    }
}