using Aeonpulse.Attributes;
using Aeonpulse.Services;
using Aeonpulse.ViewModels;
using Aeonpulse.Resources;

namespace Aeonpulse.Views
{
    /// <summary>
    /// Code-behind for the Settings modal popup.
    /// Synchronises radio button groups to the ViewModel's persisted preferences
    /// on construction, then propagates user changes back to the ViewModel
    /// (which in turn applies them immediately and persists them via <c>Preferences</c>).
    ///
    /// <para>
    /// <b>Initialisation guard:</b> <c>_initialising</c> suppresses
    /// <c>CheckedChanged</c> events that fire during <c>InitializeComponent()</c>
    /// and the subsequent radio-button seeding — preventing spurious ViewModel writes
    /// on popup open.
    /// </para>
    /// <para>
    /// <b>Hidden dependencies / side effects triggered through the ViewModel setters:</b>
    /// <list type="bullet">
    ///   <item><description>
    ///     Setting <c>ColorScheme</c> -> <see cref="Services.ThemeService.ApplyScheme"/>
    ///     mutates <c>Application.Current.Resources</c> immediately, repainting all
    ///     <c>DynamicResource</c> bindings across the entire live UI.
    ///   </description></item>
    ///   <item><description>
    ///     Setting <c>TextSize</c> -> <see cref="Services.FontSizeService.ApplyPreset"/>
    ///     mutates font-size resource keys, reflowing all bound text instantly.
    ///   </description></item>
    ///   <item><description>
    ///     Setting <c>DisplayLanguage</c> -> <c>ApplyLanguage()</c> changes
    ///     <c>CultureInfo.DefaultThreadCurrentUICulture</c> globally,
    ///     then calls <c>Loc.Invalidate()</c> (fires <c>PropertyChanged("")</c> on
    ///     <see cref="ViewModels.LocalizedResources"/>) and <c>UpdateAllCalculations()</c>.
    ///   </description></item>
    ///   <item><description>
    ///     All three settings are persisted via <c>Preferences.Default.Set</c> inside
    ///     the ViewModel setters, surviving app restarts.
    ///   </description></item>
    /// </list>
    /// </para>
    /// </summary>
    [AIContext("ModalViewController")]
    public partial class SettingsPopup : ContentPage
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Suppresses <c>CheckedChanged</c> callbacks during initialisation to prevent
        /// spurious ViewModel writes before the UI reflects the current persisted state.
        /// </summary>
        private bool _initialising = true;

        /// <summary>
        /// Constructs the popup, sets BindingContext to <paramref name="viewModel"/>,
        /// and seeds all radio button groups to match the current persisted settings.
        /// </summary>
        /// <param name="viewModel">
        /// The shared <see cref="MainViewModel"/>; set as BindingContext so
        /// <c>{Binding Loc.Xxx}</c> expressions in XAML resolve correctly.
        /// </param>
        public SettingsPopup(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;

            // Expose the VM as BindingContext so {Binding Loc.Xxx} works in XAML
            BindingContext = viewModel;

            // Seed radio buttons to reflect persisted user preferences.
            // _initialising = true prevents CheckedChanged from writing back to the VM.
            MetricRadio.IsChecked   =  _viewModel.UseMetric;
            ImperialRadio.IsChecked = !_viewModel.UseMetric;

            DefaultDarkRadio.IsChecked       = _viewModel.ColorScheme == ThemeService.DefaultDark;
            HighContrastDarkRadio.IsChecked  = _viewModel.ColorScheme == ThemeService.HighContrastDark;
            HighContrastLightRadio.IsChecked = _viewModel.ColorScheme == ThemeService.HighContrastLight;

            TextSizeSmallRadio.IsChecked  = _viewModel.TextSize == FontSizeService.Small;
            TextSizeNormalRadio.IsChecked = _viewModel.TextSize == FontSizeService.Normal;
            TextSizeLargeRadio.IsChecked  = _viewModel.TextSize == FontSizeService.Large;

            LangDefaultRadio.IsChecked = _viewModel.DisplayLanguage == MainViewModel.LangDefault;
            LangEnglishRadio.IsChecked = _viewModel.DisplayLanguage == MainViewModel.LangEnglish;
            LangRussianRadio.IsChecked = _viewModel.DisplayLanguage == MainViewModel.LangRussian;

            _initialising = false;
        }

        /// <summary>
        /// Handles unit-system radio changes.
        /// Compares against the culture-neutral <c>Value</c> ("Metric"), not the
        /// localised display string, to remain correct after a language change.
        ///
        /// <para>
        /// <b>Side effect:</b> setting <c>_viewModel.UseMetric</c> triggers
        /// <c>UpdateAllCalculations()</c>, which immediately refreshes all distance-unit
        /// dependent tickers (GalacticCommute, PhotonPath, GlobalExhale).
        /// </para>
        /// </summary>
        private void OnUnitSystemChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_initialising || !e.Value) return;
            var radio = (RadioButton)sender;
            _viewModel.UseMetric = radio.Value?.ToString() == "Metric";
        }

        /// <summary>
        /// Handles colour scheme radio changes.
        ///
        /// <para>
        /// <b>Side effect:</b> setting <c>_viewModel.ColorScheme</c> calls
        /// <see cref="Services.ThemeService.ApplyScheme"/>, instantly repainting
        /// all <c>DynamicResource</c> colour bindings across the entire UI.
        /// </para>
        /// </summary>
        private void OnColorSchemeChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_initialising || !e.Value) return;
            var radio = (RadioButton)sender;
            _viewModel.ColorScheme = radio.Value?.ToString() ?? ThemeService.DefaultDark;
        }

        /// <summary>
        /// Handles text size radio changes.
        ///
        /// <para>
        /// <b>Side effect:</b> setting <c>_viewModel.TextSize</c> calls
        /// <see cref="Services.FontSizeService.ApplyPreset"/>, instantly reflowing
        /// all <c>DynamicResource</c> font-size bindings across the entire UI.
        /// </para>
        /// </summary>
        private void OnTextSizeChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_initialising || !e.Value) return;
            var radio = (RadioButton)sender;
            _viewModel.TextSize = radio.Value?.ToString() ?? FontSizeService.Normal;
        }

        /// <summary>
        /// Handles display language radio changes.
        ///
        /// <para>
        /// <b>Side effects (chained through <c>_viewModel.DisplayLanguage</c> setter):</b>
        /// <list type="number">
        ///   <item><description>
        ///     <c>ApplyLanguage()</c> sets <c>CultureInfo.DefaultThreadCurrentUICulture</c> globally.
        ///   </description></item>
        ///   <item><description>
        ///     <c>Loc.Invalidate()</c> fires <c>PropertyChanged("")</c> on
        ///     <see cref="ViewModels.LocalizedResources"/>, refreshing all bound labels.
        ///   </description></item>
        ///   <item><description>
        ///     <c>UpdateAllCalculations()</c> regenerates all ticker strings in the new language.
        ///   </description></item>
        ///   <item><description>
        ///     The choice is persisted via <c>Preferences.Default.Set</c>.
        ///   </description></item>
        /// </list>
        /// </para>
        /// </summary>
        private void OnDisplayLanguageChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_initialising || !e.Value) return;
            var radio = (RadioButton)sender;
            _viewModel.DisplayLanguage = radio.Value?.ToString() ?? MainViewModel.LangDefault;
        }

        /// <summary>Dismisses the settings popup.</summary>
        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}