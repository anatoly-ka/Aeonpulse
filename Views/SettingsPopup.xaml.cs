using Aeonpulse.Attributes;
using Aeonpulse.Services;
using Aeonpulse.ViewModels;
using Aeonpulse.Resources;

namespace Aeonpulse.Views
{
    /// <summary>
    /// Code-behind for the Settings modal popup.
    /// Renders each setting option as a plain <see cref="HorizontalStackLayout"/>
    /// with two <see cref="Microsoft.Maui.Controls.Shapes.Ellipse"/> shapes (outer ring +
    /// inner dot) and a <see cref="Label"/>, wired by <see cref="TapGestureRecognizer"/>
    /// No <see cref="RadioButton"/> is used, avoiding WinUI RadioButton handler
    /// layout interference that caused dot-disappearing and spurious re-syncs on Windows.
    ///
    /// <para>
    /// Selection state is tracked by per-group string fields. The matching inner-dot
    /// <see cref="Microsoft.Maui.Controls.Shapes.Ellipse"/> is made visible by the
    /// <c>Set*Group</c> helpers, which are also called on construction to seed the
    /// initial state from the ViewModel.
    /// </para>
    /// <para>
    /// <b>Language labels</b> are updated manually in <see cref="OnLanguageTapped"/>
    /// after the culture switch, rather than relying on the binding engine's
    /// <c>PropertyChanged</c> propagation, which is unreliable for two-segment
    /// path bindings on Windows after a layout pass.
    /// </para>
    /// </summary>
    [AIContext("ModalViewController")]
    public partial class SettingsPopup : ContentPage
    {
        private readonly MainViewModel _viewModel;

        // Per-group selection tracking.
        private string _unitSelection     = string.Empty;
        private string _colorSelection    = string.Empty;
        private string _textSizeSelection = string.Empty;
        private string _langSelection     = string.Empty;

        /// <summary>
        /// Constructs the popup, sets BindingContext to <paramref name="viewModel"/>,

        /// and seeds all option groups to match the current persisted settings.
        /// </summary>
        public SettingsPopup(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            _unitSelection = _viewModel.UseMetric ? "Metric" : "Imperial";
            SetUnitGroup(_unitSelection);

            _colorSelection = _viewModel.ColorScheme;
            SetColorGroup(_colorSelection);

            _textSizeSelection = _viewModel.TextSize;
            SetTextSizeGroup(_textSizeSelection);

            _langSelection = _viewModel.DisplayLanguage;
            SetLangGroup(_langSelection);

            // Populate all localised label texts directly from AppResources.
            // No {Binding Loc.*} is used in the XAML; this is the sole source of
            // localised strings for this popup, and is also called after a language change.
            RefreshLocalisedLabels();
        }

        // --- Dot-visibility group helpers ------------------------------------

        private void SetUnitGroup(string value)
        {
            MetricDot.IsVisible   = value == "Metric";
            ImperialDot.IsVisible = value == "Imperial";
        }

        private void SetColorGroup(string value)
        {
            DefaultDarkDot.IsVisible      = value == ThemeService.DefaultDark;
            HighContrastDarkDot.IsVisible  = value == ThemeService.HighContrastDark;
            HighContrastLightDot.IsVisible = value == ThemeService.HighContrastLight;
        }

        private void SetTextSizeGroup(string value)
        {
            TextSizeSmallDot.IsVisible  = value == FontSizeService.Small;
            TextSizeNormalDot.IsVisible = value == FontSizeService.Normal;
            TextSizeLargeDot.IsVisible  = value == FontSizeService.Large;
        }

        private void SetLangGroup(string value)
        {
            LangDefaultDot.IsVisible = value == MainViewModel.LangDefault;
            LangEnglishDot.IsVisible = value == MainViewModel.LangEnglish;
            LangRussianDot.IsVisible = value == MainViewModel.LangRussian;
        }

        // --- Tapped handlers -------------------------------------------------

        /// <summary>
        /// Handles unit-system option taps.
        /// <para>
        /// <b>Side effect:</b> setting <c>_viewModel.UseMetric</c> triggers
        /// <c>UpdateAllCalculations()</c>.
        /// </para>
        /// </summary>
        private void OnUnitSystemTapped(object sender, TappedEventArgs e)
        {
            var value = e.Parameter?.ToString() ?? string.Empty;
            if (value == _unitSelection) return;
            _unitSelection = value;
            SetUnitGroup(value);
            _viewModel.UseMetric = value == "Metric";
        }

        /// <summary>
        /// Handles colour scheme option taps.
        /// <para>
        /// <b>Side effect:</b> setting <c>_viewModel.ColorScheme</c> calls
        /// <see cref="Services.ThemeService.ApplyScheme"/>.
        /// </para>
        /// </summary>
        private void OnColorSchemeTapped(object sender, TappedEventArgs e)
        {
            var value = e.Parameter?.ToString() ?? ThemeService.DefaultDark;
            if (value == _colorSelection) return;
            _colorSelection = value;
            SetColorGroup(value);
            _viewModel.ColorScheme = value;
        }

        /// <summary>
        /// Handles text size option taps.
        /// <para>
        /// <b>Side effect:</b> setting <c>_viewModel.TextSize</c> calls
        /// <see cref="Services.FontSizeService.ApplyPreset"/>.
        /// </para>
        /// </summary>
        private void OnTextSizeTapped(object sender, TappedEventArgs e)
        {
            var value = e.Parameter?.ToString() ?? FontSizeService.Normal;
            if (value == _textSizeSelection) return;
            _textSizeSelection = value;
            SetTextSizeGroup(value);
            _viewModel.TextSize = value;
        }

        /// <summary>
        /// Handles display language option taps.
        /// After the ViewModel applies the culture switch, manually refreshes every
        /// label in this popup that carries a localised string, because the MAUI
        /// Windows binding engine does not reliably re-evaluate two-segment path
        /// bindings (<c>{Binding Loc.Xxx}</c>) after a <c>PropertyChanged</c>
        /// notification on the intermediate object following a layout pass.
        /// <para>
        /// <b>Side effects (chained through <c>_viewModel.DisplayLanguage</c> setter):</b>
        /// <c>ApplyLanguage()</c>, <c>Loc.Invalidate()</c>,
        /// <c>UpdateAllCalculations()</c>, <c>Preferences.Set</c>.
        /// </para>
        /// </summary>
        private void OnLanguageTapped(object sender, TappedEventArgs e)
        {
            var value = e.Parameter?.ToString() ?? MainViewModel.LangDefault;
            if (value == _langSelection) return;
            _langSelection = value;
            SetLangGroup(value);
            _viewModel.DisplayLanguage = value;
            RefreshLocalisedLabels();
        }

        /// <summary>
        /// Resets all settings to factory defaults, re-seeds the UI groups to
        /// reflect the new values, and refreshes all localised labels (in case
        /// the language was reset to Default from a non-default language).
        /// </summary>
        private void OnResetSettingsTapped(object sender, EventArgs e)
        {
            _viewModel.ResetSettings();

#if WINDOWS
            // Apply the default window geometry immediately on Windows.
            // ResetSettings() has already cleared the persisted geometry keys,
            // so the next launch will also start with the default size/position.
            Aeonpulse.WinUI.App.ResetWindowGeometry();
#endif

            // Re-seed every group to reflect the new default values.
            _unitSelection = "Metric";
            SetUnitGroup(_unitSelection);

            _colorSelection = ThemeService.DefaultDark;
            SetColorGroup(_colorSelection);

            _textSizeSelection = FontSizeService.Normal;
            SetTextSizeGroup(_textSizeSelection);

            _langSelection = MainViewModel.LangDefault;
            SetLangGroup(_langSelection);

            // Refresh labels in case the language changed back to Default.
            RefreshLocalisedLabels();
        }

        /// <summary>
        /// Re-reads every localised string directly from <see cref="AppResources"/>
        /// (which already uses the new culture set by <see cref="MainViewModel.ApplyLanguage"/>)
        /// and assigns it to the matching label in this popup.
        /// </summary>
        private void RefreshLocalisedLabels()
        {
            SettingsTitleLabel.Text       = AppResources.Settings_Title;
            SettingsSectionHeader.Text    = AppResources.Settings_SettingsTitle;
            UnitsLabel.Text               = AppResources.Settings_UnitsLabel;
            MetricLabel.Text              = AppResources.Settings_UnitsMetric;
            ImperialLabel.Text            = AppResources.Settings_UnitsImperial;
            PaletteLabel.Text             = AppResources.Settings_PaletteLabel;
            DefaultDarkLabel.Text         = AppResources.Settings_PaletteDefault;
            HighContrastDarkLabel.Text    = AppResources.Settings_PaletteHighContrastDark;
            HighContrastLightLabel.Text   = AppResources.Settings_PaletteHighContrastLight;
            TextSizeLabel.Text            = AppResources.Settings_TextSizeLabel;
            TextSizeSmallLabel.Text       = AppResources.Settings_TextSizeSmall;
            TextSizeNormalLabel.Text      = AppResources.Settings_TextSizeNormal;
            TextSizeLargeLabel.Text       = AppResources.Settings_TextSizeLarge;
            LanguageLabel.Text            = AppResources.Settings_LanguageLabel;
            LangDefaultLabel.Text         = AppResources.Settings_LanguageDefault;
            LangEnglishLabel.Text         = AppResources.Settings_LanguageEnglish;
            LangRussianLabel.Text         = AppResources.Settings_LanguageRussian;
            AboutSectionHeader.Text = AppResources.Settings_AboutTitle;
            AboutTextLabel.Text     = AppResources.Settings_AboutText;
            CloseButton.Text              = AppResources.Settings_ButtonClose;
            ResetSettingsButton.Text      = AppResources.Settings_ButtonResetSettings;
        }

        /// <summary>Dismisses the settings popup.</summary>
        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}