using Aeonpulse.Services;
using Aeonpulse.Views;

namespace Aeonpulse
{
    public partial class App : Application
    {
        public App()
        {
            // Apply persisted colour scheme and text size before InitializeComponent()
            // so every DynamicResource binding gets the correct value from the first frame.
            var savedScheme   = Preferences.Default.Get("ColorScheme", ThemeService.DefaultDark);
            var savedTextSize = Preferences.Default.Get("TextSize",    FontSizeService.Normal);

            ThemeService.Instance.ApplyScheme(savedScheme);
            FontSizeService.Instance.ApplyPreset(savedTextSize);

            InitializeComponent();
            MainPage = new Views.MainPage();
        }
    }
}
