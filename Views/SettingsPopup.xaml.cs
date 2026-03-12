using Aeonpulse.ViewModels;

namespace Aeonpulse.Views
{
    public partial class SettingsPopup : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public SettingsPopup(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;

            // IsToggled = true (right) ? Metric, false (left) ? Imperial
            MetricSwitch.IsToggled = _viewModel.UseMetric;
        }

        private void OnMetricSwitchToggled(object sender, ToggledEventArgs e)
        {
            // No dynamic label update needed — labels are now static in XAML
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            // Apply the toggle value to the ViewModel on close.
            // UseMetric setter already fires UpdateAllCalculations() when the value changes.
            _viewModel.UseMetric = MetricSwitch.IsToggled;

            await Navigation.PopModalAsync();
        }
    }
}