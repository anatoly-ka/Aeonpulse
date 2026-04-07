using Aeonpulse.Attributes;
using Aeonpulse.Resources;
using Aeonpulse.ViewModels;

namespace Aeonpulse.Views
{
    /// <summary>
    /// Code-behind for the Change Date modal popup.
    /// Initialises the form controls from the current ViewModel state and
    /// writes validated changes back atomically via <see cref="MainViewModel.SaveDate"/>.
    ///
    /// <para>
    /// <b>Hidden dependency:</b> <see cref="MainViewModel.SaveDate"/> not only updates
    /// <c>BaseDateName</c>, <c>BaseDateValue</c>, and <c>BaseDate</c> atomically, but also
    /// calls <c>UpdateAllCalculations()</c> — triggering a full recalculation of all ten
    /// tickers immediately after the popup closes.
    /// </para>
    /// </summary>
    [AIContext("ModalViewController")]
    public partial class ChangeDatePopup : ContentPage
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Constructs the popup and pre-populates the form with the current
        /// <paramref name="viewModel"/> base date name and value.
        /// </summary>
        /// <param name="viewModel">
        /// The shared <see cref="MainViewModel"/>; used to read current values
        /// and to write validated changes back via <see cref="MainViewModel.SaveDate"/>.
        /// </param>
        public ChangeDatePopup(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;

            // Pre-populate from the ViewModel's current persisted values
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

        /// <summary>
        /// Cancels the edit without saving; dismisses the modal.
        /// </summary>
        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        /// <summary>
        /// Validates the form, then atomically saves name + date via
        /// <see cref="MainViewModel.SaveDate"/> before dismissing.
        ///
        /// <para>
        /// <b>Side effect:</b> <see cref="MainViewModel.SaveDate"/> calls
        /// <c>UpdateAllCalculations()</c>, so all ticker cards will re-render
        /// with the new base date as soon as this popup is popped.
        /// </para>
        /// <para>
        /// <b>Pre-1900 guard:</b> dates before January 1 1900 are rejected with a
        /// localized <c>DisplayAlert</c>. The picker value is reverted to the
        /// current <see cref="MainViewModel.BaseDate"/> so no state change occurs.
        /// </para>
        /// </summary>
        private async void OnOkClicked(object sender, EventArgs e)
        {
            var selectedDate = EventDatePicker.Date;

            if (selectedDate < new DateTime(1900, 1, 1))
            {
                // Revert the picker to the current saved date so no partial state leaks
                if (DateTime.TryParse(_viewModel.BaseDateValue, out var current))
                    EventDatePicker.Date = current;

                await Application.Current!.MainPage!.DisplayAlert(
                    AppResources.Alert_Title_Aeonpulse,
                    AppResources.Alert_Message_Pre1900,
                    AppResources.Alert_Button_Close);
                return;
            }

            var newName = EventNameEntry.Text?.Trim();
            var newDate = selectedDate.ToString("yyyy-MM-dd");

            // SaveDate atomically updates BaseDateName, BaseDateValue AND BaseDate,
            // then calls UpdateAllCalculations() once with all values consistent.
            _viewModel.SaveDate(
                string.IsNullOrWhiteSpace(newName) ? _viewModel.BaseDateName : newName,
                newDate
            );

            await Navigation.PopModalAsync();
        }
    }
}