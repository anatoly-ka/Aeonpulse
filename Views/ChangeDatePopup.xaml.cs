using Aeonpulse.Attributes;
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
        /// </summary>
        private async void OnOkClicked(object sender, EventArgs e)
        {
            var newName = EventNameEntry.Text?.Trim();
            var newDate = EventDatePicker.Date.ToString("yyyy-MM-dd");

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