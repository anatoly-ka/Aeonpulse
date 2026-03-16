namespace Aeonpulse.Services
{
    /// <summary>
    /// Singleton service that applies one of three text-size presets to the
    /// application's merged resource dictionary at runtime, mirroring the
    /// pattern used by <see cref="ThemeService"/>.
    /// </summary>
    public class FontSizeService
    {
        // --- Singleton --------------------------------------------------------
        public static FontSizeService Instance { get; } = new FontSizeService();
        private FontSizeService() { }

        // --- Preset identifiers -----------------------------------------------
        public const string Small  = "Small";
        public const string Normal = "Normal";
        public const string Large  = "Large";

        // --- Small preset -----------------------------------------------------
        private static readonly Dictionary<string, double> _small = new()
        {
            { "FontSizeSmall",  10 },
            { "FontSizeMedium", 12 },
            { "FontSizeLarge",  14 },
            { "FontSizeXLarge", 18 },
            { "FontSizeTitle",  24 },
        };

        // --- Normal preset (matches Colors.xaml startup values) ---------------
        private static readonly Dictionary<string, double> _normal = new()
        {
            { "FontSizeSmall",  12 },
            { "FontSizeMedium", 14 },
            { "FontSizeLarge",  16 },
            { "FontSizeXLarge", 20 },
            { "FontSizeTitle",  24 },
        };

        // --- Large preset -----------------------------------------------------
        private static readonly Dictionary<string, double> _large = new()
        {
            { "FontSizeSmall",  14 },
            { "FontSizeMedium", 16 },
            { "FontSizeLarge",  19 },
            { "FontSizeXLarge", 23 },
            { "FontSizeTitle",  24 },
        };

        // --- Active preset ----------------------------------------------------
        private string _currentPreset = Normal;
        public string CurrentPreset => _currentPreset;

        /// <summary>
        /// Applies the requested text-size preset to the application resource dictionary.
        /// </summary>
        public void ApplyPreset(string preset)
        {
            _currentPreset = preset;

            var sizes = preset switch
            {
                Small => _small,
                Large => _large,
                _     => _normal,
            };

            var resources = Application.Current?.Resources;
            if (resources is null)
                return;

            foreach (var (key, size) in sizes)
                resources[key] = size;
        }
    }
}