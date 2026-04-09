using Aeonpulse.Attributes;
using System.Globalization;

namespace Aeonpulse.Converters
{
    /// <summary>
    /// Converts a <see cref="bool"/> to a MAUI <c>IsVisible</c>-compatible value.
    /// Used throughout the app to show/hide collapsible sections and ticker card bodies
    /// without requiring code-behind visibility logic.
    /// </summary>
    /// <remarks>
    /// <b>ConvertBack</b> is intentionally unimplemented: visibility is always driven
    /// one-way from the ViewModel (IsExpanded -> Visible).
    /// </remarks>
    [AIContext("UIConverter")]
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue;
            return false;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Inverts a <see cref="bool"/> value.
    /// Used to toggle collapse/expand indicators — e.g., to show a "collapsed" chevron
    /// when <c>IsExpanded</c> is <c>false</c>, without an extra ViewModel property.
    /// </summary>
    [AIContext("UIConverter")]
    public class InverseBoolConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return true;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    /// <summary>
    /// Selects one of two image source strings based on a <see cref="bool"/> value.
    /// Drives expand/collapse icon swaps on ticker card headers without code-behind.
    ///
    /// <para>
    /// <b>ConverterParameter format:</b> <c>"imageIfTrue.png|imageIfFalse.png"</c>
    /// </para>
    /// <para>
    /// Example XAML: <c>ConverterParameter="chevron_up.png|chevron_down.png"</c>
    /// </para>
    /// </summary>
    [AIContext("UIConverter")]
    public class BoolToImageSourceConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string param)
            {
                var parts = param.Split('|');
                if (parts.Length == 2)
                    return boolValue ? parts[0] : parts[1];
            }
            return string.Empty;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Returns a GIF filename when the bound <see cref="bool"/> is <c>true</c> (expanded),
    /// and <c>null</c> when <c>false</c> (collapsed).
    ///
    /// <para>
    /// <b>Why this is needed on Android:</b> when an <c>Image</c> has a GIF source set in
    /// XAML, Android decodes every animation frame into a <c>Bitmap</c> at load time
    /// regardless of <c>IsVisible</c>. For large animated GIFs
    /// (e.g. anim_sun_in_milky_way.gif at 5.7 MB, anim_mitosis.gif at 3.4 MB) this
    /// causes an <c>OutOfMemoryError</c> at startup because all six GIFs load simultaneously.
    /// Binding <c>Source</c> through this converter ensures the GIF is only decoded when
    /// the card is actually expanded, keeping startup memory near zero for all GIF cards.
    /// </para>
    /// <para>
    /// <b>ConverterParameter:</b> the GIF filename string, e.g. <c>"anim_countdown.gif"</c>.
    /// </para>
    /// <para>
    /// <b>Windows / iOS / Mac:</b> behaviour is identical - the image loads on expand and
    /// unloads on collapse - which is the correct UX in any case.
    /// </para>
    /// </summary>
    [AIContext("UIConverter")]
    public class BoolToGifSourceConverter : IValueConverter
    {
        /// <inheritdoc />
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool expanded && expanded && parameter is string filename)
                return filename;
            return null;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
