using Aeonpulse.Services;
using Aeonpulse.Views;

namespace Aeonpulse
{
    public partial class App : Application
    {
        public App()
        {
            // Apply the persisted colour scheme (or DefaultDark on first run) before
            // InitializeComponent() so every DynamicResource binding gets the right
            // value from the very first frame.
            var savedScheme = Preferences.Default.Get("ColorScheme", ThemeService.DefaultDark);
            ThemeService.Instance.ApplyScheme(savedScheme);

            InitializeComponent();
            MainPage = new Views.MainPage();
        }
    }
}
