using Aeonpulse.ViewModels;

namespace Aeonpulse.Views
{
    public partial class ChangeDatePopup : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public ChangeDatePopup(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;

            EventNameEntry.Text = viewModel.BaseDateName;

            if (DateTime.TryParse(viewModel.BaseDateValue, out var parsedDate))
            {
                EventDatePicker.Date = parsedDate;
            }
            else
            {
                EventDatePicker.Date = DateTime.Today;
            }
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void OnOkClicked(object sender, EventArgs e)
        {
            var newName = EventNameEntry.Text?.Trim();
            var newDate = EventDatePicker.Date.ToString("yyyy-MM-dd");

            // SaveDate atomically updates BaseDateName, BaseDateValue AND BaseDate,
            // then calls UpdateAllCalculations() once with all values consistent.
            // Previously, BaseDate was never updated here, so all calculations
            // used the old DateTime despite the new string values.
            _viewModel.SaveDate(
                string.IsNullOrWhiteSpace(newName) ? _viewModel.BaseDateName : newName,
                newDate
            );

            await Navigation.PopModalAsync();
        }
    }
}