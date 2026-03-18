using Aeonpulse.Services;
using Aeonpulse.Views;
using Aeonpulse.ViewModels;

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

            // Restore persisted display language before InitializeComponent()
            // so all {x:Static resources:AppResources.*} bindings use the right culture.
            var savedLanguage = Preferences.Default.Get("DisplayLanguage", MainViewModel.LangDefault);
            MainViewModel.ApplyLanguage(savedLanguage);

            InitializeComponent();
            MainPage = new Views.MainPage();
        }
    }
}
