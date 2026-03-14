using Microsoft.Maui.Controls;

namespace Aeonpulse.Services
{
    /// <summary>
    /// Singleton service that applies one of several available colour schemes to the
    /// application's merged resource dictionary at runtime.
    /// </summary>
    public class ThemeService
    {
        // --- Singleton --------------------------------------------------------
        public static ThemeService Instance { get; } = new ThemeService();
        private ThemeService() { }

        // --- Scheme identifiers -----------------------------------------------
        public const string DefaultDark        = "DefaultDark";
        public const string HighContrastDark   = "HighContrastDark";
        public const string HighContrastLight  = "HighContrastLight";

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
        // Rule: taking R,G,B from the Default Dark and then:
        // avg(R,G,B) <= 128 -> Black, > 128 -> White
        private static readonly Dictionary<string, Color> _highContrastDarkColors = new()
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

        // --- High Contrast Light palette ----------------------------------------
        // Exact inverse of High Contrast Dark: Black -> White, White -> Black.
        private static readonly Dictionary<string, Color> _highContrastLightColors = new()
        {
            { "SpaceDark",       Colors.White   },
            { "SpaceDarker",     Colors.White   },
            { "CyberCyan",       Colors.Black   },
            { "CyberPurple",     Colors.Black   },
            { "CyberPink",       Colors.Black   },
            { "NeonGreen",       Colors.Black   },
            { "NeonGreenDark",   Colors.White   },
            { "TextWhite",       Colors.Black   },
            { "TextDim",         Colors.Black   },
            { "TextGray",        Colors.Black   },
            { "CardBackground",  Colors.White   },
            { "CardDark",        Colors.White   },
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

            var palette = scheme switch
            {
                HighContrastDark  => _highContrastDarkColors,
                HighContrastLight => _highContrastLightColors,
                _                 => _defaultColors,
            };

            var resources = Application.Current?.Resources;
            if (resources is null)
                return;

            foreach (var (key, color) in palette)
                resources[key] = color;
        }
    }
}