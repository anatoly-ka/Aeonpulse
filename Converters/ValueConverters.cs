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
}
