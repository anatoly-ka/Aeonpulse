using Aeonpulse.Attributes;
using Aeonpulse.Services;
using Aeonpulse.Views;
using Aeonpulse.ViewModels;

namespace Aeonpulse
{
    /// <summary>
    /// Application entry point. Responsible for bootstrapping all singleton services
    /// (theme, font size, language) from <see cref="Preferences"/> <b>before</b>
    /// <see cref="Application.InitializeComponent"/> runs, ensuring that every
    /// <c>DynamicResource</c> and <c>AppResources</c> binding gets the correct
    /// persisted value on the very first rendered frame.
    ///
    /// <para>
    /// <b>Side effects / hidden dependencies:</b>
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ThemeService.ApplyScheme"/> mutates <c>Application.Current.Resources</c>
    ///     before <c>InitializeComponent</c> has merged the XAML resource dictionaries.
    ///     This works because MAUI merges dictionaries lazily during first UI inflate.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="MainViewModel.ApplyLanguage"/> sets
    ///     <c>CultureInfo.DefaultThreadCurrentUICulture</c> and <c>AppResources.Culture</c>
    ///     globally, affecting all subsequent string resource lookups on any thread.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="MainViewModel"/> is also constructed by XAML inside
    ///     <c>MainPage.xaml</c>'s <c>BindingContext</c>; that second construction re-reads
    ///     persisted preferences including the base date name and value
    ///     (keys <c>"BaseDateName"</c> / <c>"BaseDateValue"</c>, default <c>2000-01-01</c>).
    ///   </description></item>
    /// </list>
    /// </para>
    /// </summary>
    [AIContext("AppBootstrap")]
    public partial class App : Application
    {
        /// <summary>
        /// Initialises the application shell, applying all persisted user preferences
        /// before the XAML resource system inflates the first page.
        /// </summary>
        public App()
        {
            // Apply persisted colour scheme and text size before InitializeComponent()
            // so every DynamicResource binding gets the correct value from the first frame.
            var savedScheme   = Preferences.Default.Get("ColorScheme", ThemeService.DefaultDark);
            var savedTextSize = Preferences.Default.Get("TextSize",    FontSizeService.Normal);

            ThemeService.Instance.ApplyScheme(savedScheme);
            AeonLog.Info("BOOT", "ColorScheme", $"restored={savedScheme}");
            FontSizeService.Instance.ApplyPreset(savedTextSize);
            AeonLog.Info("BOOT", "TextSize",    $"restored={savedTextSize}");

            // Restore persisted display language before InitializeComponent()
            // so all {x:Static resources:AppResources.*} bindings use the right culture.
            var savedLanguage = Preferences.Default.Get("DisplayLanguage", MainViewModel.LangDefault);
            MainViewModel.ApplyLanguage(savedLanguage);
            AeonLog.Info("BOOT", "Language",    $"restored={savedLanguage}");

            InitializeComponent();
            MainPage = new Views.MainPage();
        }
    }
}
