using Aeonpulse.Attributes;
using Aeonpulse.Resources;

namespace Aeonpulse.Views
{
    /// <summary>
    /// Code-behind for the tease popup shown when the user taps the app logo or app name.
    /// Displays a single live stat string captured from <see cref="ViewModels.MainViewModel.TeaseText"/>
    /// at the moment of opening. The panel is positioned flush below the NavBar, left-aligned,
    /// via a <paramref name="topOffset"/> injected as the top component of <c>TeasePanel.Margin</c>.
    ///
    /// <para>
    /// <b>Buttons:</b> "Close" dismisses the popup. "To Clipboard" copies the stat text to the
    /// OS clipboard, dismisses the popup, then shows a <c>DisplayAlert</c> confirmation.
    /// </para>
    /// <para>
    /// <b>Architecture note:</b> the tease text is captured at open time - no live binding needed.
    /// The confirmation <c>DisplayAlert</c> is shown from <c>MainPage</c> via an async callback
    /// so it runs on the correct navigation context after this popup has been fully popped.
    /// </para>
    /// </summary>
    [AIContext("ModalViewController")]
    public partial class TeasePopup : ContentPage
    {
        /// <summary>The stat text to display and optionally copy to the clipboard.</summary>
        private readonly string _teaseText;

        /// <summary>
        /// Async callback invoked by "To Clipboard" after this popup is dismissed,
        /// so the confirmation <c>DisplayAlert</c> runs on <c>MainPage</c>'s navigation
        /// context (mandatory ordering on iOS).
        /// </summary>
        private readonly Func<string, Task>? _onCopiedCallback;

        /// <summary>
        /// Creates the tease popup and positions it below the NavBar.
        /// </summary>
        /// <param name="teaseText">
        /// A pre-formatted live stat string from <see cref="ViewModels.MainViewModel.TeaseText"/>,
        /// captured by <c>MainPage.OnLogoTapped</c> at the moment of opening.
        /// </param>
        /// <param name="topOffset">
        /// Vertical offset in device-independent units equal to <c>NavBar.Height</c>,
        /// measured by <c>MainPage</c> at the moment of opening. Positions the panel
        /// flush below the navigation bar.
        /// </param>
        /// <param name="leftOffset">
        /// Horizontal offset equal to the NavBar left content padding so the panel
        /// left-aligns with the logo/app-name section.
        /// </param>
        /// <param name="onCopiedCallback">
        /// Async action invoked after the popup is dismissed when the user taps
        /// "To Clipboard". Receives the copied text and shows a confirmation alert
        /// on <c>MainPage</c>'s navigation context.
        /// </param>
        public TeasePopup(
            string teaseText,
            double topOffset,
            double leftOffset,
            Func<string, Task> onCopiedCallback)
        {
            InitializeComponent();

            _teaseText        = teaseText;
            _onCopiedCallback = onCopiedCallback;

            TeaseTextLabel.Text = teaseText;

            // Override only the top and left Margin; preserve right/bottom as zero.
            TeasePanel.Margin = new Thickness(leftOffset, topOffset, 0, 0);
        }

        /// <summary>
        /// Copies the stat text to the OS clipboard, dismisses the popup, then
        /// invokes the confirmation callback on <c>MainPage</c>'s navigation context.
        /// </summary>
        private async void OnCopyClicked(object sender, EventArgs e)
        {
            await Clipboard.Default.SetTextAsync(_teaseText);
            await Navigation.PopModalAsync();
            if (_onCopiedCallback is not null)
                await _onCopiedCallback(_teaseText);
        }

        /// <summary>Dismisses the popup when the user taps Close or the backdrop.</summary>
        private void OnOkClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}
