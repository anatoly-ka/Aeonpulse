using Aeonpulse.Services;
using Aeonpulse.ViewModels;

namespace Aeonpulse.Views
{
    public partial class SettingsPopup : ContentPage
    {
        private readonly MainViewModel _viewModel;

        // Guard to prevent CheckedChanged from firing during initialisation
        private bool _initialising = true;

        public SettingsPopup(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;

            // Initialise unit system radio buttons to match the persisted value
            MetricRadio.IsChecked   =  _viewModel.UseMetric;
            ImperialRadio.IsChecked = !_viewModel.UseMetric;

            // Initialise colour scheme radio buttons to match the persisted value
            DefaultDarkRadio.IsChecked       = _viewModel.ColorScheme == ThemeService.DefaultDark;
            HighContrastDarkRadio.IsChecked  = _viewModel.ColorScheme == ThemeService.HighContrastDark;
            HighContrastLightRadio.IsChecked = _viewModel.ColorScheme == ThemeService.HighContrastLight;

            // Initialise text size radio buttons to match the persisted value
            TextSizeSmallRadio.IsChecked  = _viewModel.TextSize == FontSizeService.Small;
            TextSizeNormalRadio.IsChecked = _viewModel.TextSize == FontSizeService.Normal;
            TextSizeLargeRadio.IsChecked  = _viewModel.TextSize == FontSizeService.Large;

            // Initialise display language radio buttons - default to "Default" for now
            LangDefaultRadio.IsChecked = true;
            LangEnglishRadio.IsChecked = false;
            LangRussianRadio.IsChecked = false;

            _initialising = false;
        }

        private void OnUnitSystemChanged(object sender, CheckedChangedEventArgs e)
        {
            // Only react to the radio that just became checked; ignore uncheck events
            // and events fired during InitializeComponent
            if (_initialising || !e.Value)
                return;

            var radio = (RadioButton)sender;
            _viewModel.UseMetric = radio.Value?.ToString() == "Metric";
        }

        private void OnColorSchemeChanged(object sender, CheckedChangedEventArgs e)
        {
            // Only react to the radio that just became checked; ignore uncheck events
            // and events fired during InitializeComponent
            if (_initialising || !e.Value)
                return;

            var radio = (RadioButton)sender;
            var scheme = radio.Value?.ToString() ?? ThemeService.DefaultDark;

            // Setting ColorScheme calls ThemeService.ApplyScheme() and persists the choice
            _viewModel.ColorScheme = scheme;
        }

        private void OnTextSizeChanged(object sender, CheckedChangedEventArgs e)
        {
            // Only react to the radio that just became checked; ignore uncheck events
            // and events fired during InitializeComponent
            if (_initialising || !e.Value)
                return;

            var radio = (RadioButton)sender;
            var preset = radio.Value?.ToString() ?? FontSizeService.Normal;

            // Setting TextSize calls FontSizeService.ApplyPreset() and persists the choice
            _viewModel.TextSize = preset;
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}