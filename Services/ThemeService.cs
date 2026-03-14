using Microsoft.Maui.Controls;

namespace Aeonpulse.Services
{
    /// <summary>
    /// Singleton service that applies one of two colour schemes to the
    /// application's merged resource dictionary at runtime.
    /// </summary>
    public class ThemeService
    {
        // --- Singleton --------------------------------------------------------
        public static ThemeService Instance { get; } = new ThemeService();
        private ThemeService() { }

        // --- Scheme identifiers -----------------------------------------------
        public const string DefaultDark     = "DefaultDark";
        public const string HighContrastDark = "HighContrastDark";

        // --- Default Dark palette ---------------------------------------------
        private static readonly Dictionary<string, Color> _defaultColors = new()
        {
            { "SpaceDark",       Color.FromArgb("#0A0E27") },
            { "SpaceDarker",     Color.FromArgb("#060812") },
            { "CyberCyan",       Color.FromArgb("#00E5FF") },
            { "CyberPurple",     Color.FromArgb("#BD93F9") },
            { "CyberPink",       Color.FromArgb("#FF79C6") },
            { "NeonGreen",       Color.FromArgb("#50FA7B") },
            { "NeonGreenDark",   Color.FromArgb("#1A3A1A") },
            { "TextWhite",       Color.FromArgb("#FFFFFF") },
            { "TextDim",         Color.FromArgb("#E5E5E5") },
            { "TextGray",        Color.FromArgb("#B0B0B0") },
            { "CardBackground",  Color.FromArgb("#1A1F3A") },
            { "CardDark",        Color.FromArgb("#121628") },
        };

        // --- High Contrast Dark palette ----------------------------------------
        // Rule: avg(R,G,B) <= 128 -> Black, > 128 -> White
        private static readonly Dictionary<string, Color> _highContrastColors = new()
        {
            { "SpaceDark",       Colors.Black   },   // avg=17
            { "SpaceDarker",     Colors.Black   },   // avg=7
            { "CyberCyan",       Colors.White   },   // avg=156
            { "CyberPurple",     Colors.White   },   // avg=181
            { "CyberPink",       Colors.White   },   // avg=155
            { "NeonGreen",       Colors.White   },   // avg=143
            { "NeonGreenDark",   Colors.Black   },   // avg=29
            { "TextWhite",       Colors.White   },   // avg=255
            { "TextDim",         Colors.White   },   // avg=229
            { "TextGray",        Colors.White   },   // avg=176
            { "CardBackground",  Colors.Black   },   // avg=28
            { "CardDark",        Colors.Black   },   // avg=19
        };

        // --- Active scheme -------------------------------------------------------
        private string _currentScheme = DefaultDark;
        public string CurrentScheme => _currentScheme;

        /// <summary>
        /// Applies the requested colour scheme to the application resource dictionary.
        /// </summary>
        public void ApplyScheme(string scheme)
        {
            _currentScheme = scheme;

            var palette = scheme == HighContrastDark
                ? _highContrastColors
                : _defaultColors;

            var resources = Application.Current?.Resources;
            if (resources is null)
                return;

            foreach (var (key, color) in palette)
                resources[key] = color;
        }
    }
}