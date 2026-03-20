using Aeonpulse.Attributes;

namespace Aeonpulse.Views
{
    /// <summary>
    /// Code-behind for the transient "Refreshing…" loading overlay.
    /// The popup is self-dismissing: <see cref="OnAppearing"/> starts a 3-second
    /// delay, then pops the modal and fires the ticker-specific recalculation
    /// callback supplied by the ViewModel's <c>RefreshXxxCommand</c>.
    ///
    /// <para>
    /// <b>Hidden dependency / side effect:</b>
    /// <see cref="OnAppearing"/> is called on the UI thread by the MAUI page lifecycle.
    /// The <c>await Task.Delay</c> yields back to the UI thread, so the spinner
    /// animation continues to run during the wait. After the delay,
    /// <c>Navigation.PopModalAsync()</c> is awaited before <c>_onDismissed()</c>
    /// is invoked — ensuring the recalculation fires only after the overlay is gone.
    /// </para>
    /// </summary>
    [AIContext("ModalViewController")]
    public partial class RefreshingPopup : ContentPage
    {
        /// <summary>
        /// Ticker-specific recalculation delegate, supplied by the ViewModel command
        /// that triggered this popup. Invoked after the popup auto-dismisses.
        /// </summary>
        private readonly Action _onDismissed;

        /// <summary>
        /// Constructs the popup with the post-dismiss recalculation callback.
        /// </summary>
        /// <param name="onDismissed">
        /// Action executed after the 3-second delay and modal pop complete.
        /// Typically updates a single <see cref="Models.TickerData"/> property on
        /// <see cref="ViewModels.MainViewModel"/>.
        /// </param>
        public RefreshingPopup(Action onDismissed)
        {
            InitializeComponent();
            _onDismissed = onDismissed;
        }

        /// <summary>
        /// Starts the auto-dismiss timer when the popup appears on screen.
        /// Waits 3 seconds to let the spinner animate, then pops the modal
        /// and fires <see cref="_onDismissed"/>.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Task.Delay(3000);
            await Navigation.PopModalAsync();
            _onDismissed();
        }
    }
}