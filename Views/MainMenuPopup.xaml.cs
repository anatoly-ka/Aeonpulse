using Aeonpulse.Attributes;
using Aeonpulse.ViewModels;

namespace Aeonpulse.Views
{
    /// <summary>
    /// Code-behind for the main hamburger menu popup.
    /// Positioned below the NavBar right edge via injected geometry offsets,
    /// and delegates navigation to callback lambdas supplied by <c>MainPage</c>
    /// so that child popups are always pushed on <c>MainPage</c>'s own modal stack
    /// (not from within this popup's already-popped context).
    ///
    /// <para>
    /// <b>Navigation contract:</b> each menu item first <c>await</c>s
    /// <c>Navigation.PopModalAsync()</c> to fully remove this popup, then invokes
    /// the callback. This ordering is critical — pushing a new modal while this one
    /// is still animating out causes a <c>InvalidOperationException</c> on iOS.
    /// </para>
    /// </summary>
    [AIContext("ModalViewController")]
    public partial class MainMenuPopup : ContentPage
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Callback invoked after this popup is dismissed to push <c>ChangeDatePopup</c>
        /// onto <c>MainPage</c>'s navigation stack. Null means no follow-up navigation.
        /// </summary>
        private readonly Func<Task>? _openChangeDateCallback;

        /// <summary>
        /// Callback invoked after this popup is dismissed to push <c>SettingsPopup</c>
        /// onto <c>MainPage</c>'s navigation stack.
        /// </summary>
        private readonly Func<Task>? _openSettingsCallback;

        /// <summary>
        /// Constructs the menu popup and positions the panel below the NavBar.
        /// </summary>
        /// <param name="viewModel">The shared ViewModel; forwarded to child popups if needed.</param>
        /// <param name="topOffset">
        /// Vertical offset equal to <c>NavBar.Height</c> — positions the panel
        /// flush below the navigation bar.
        /// </param>
        /// <param name="rightOffset">
        /// Horizontal offset equal to the NavBar's right content padding, so the panel
        /// right-aligns with the hamburger button.
        /// </param>
        /// <param name="openChangeDateCallback">
        /// Async action to push <c>ChangeDatePopup</c> after this menu closes.
        /// </param>
        /// <param name="openSettingsCallback">
        /// Async action to push <c>SettingsPopup</c> after this menu closes.
        /// </param>
        public MainMenuPopup(
            MainViewModel viewModel,
            double topOffset,
            double rightOffset,
            Func<Task> openChangeDateCallback,
            Func<Task> openSettingsCallback)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _openChangeDateCallback = openChangeDateCallback;
            _openSettingsCallback   = openSettingsCallback;

            // Position the panel below the NavBar, aligned to the right edge.
            MenuFrame.Margin = new Thickness(0, topOffset, rightOffset, 0);
        }

        /// <summary>
        /// Pops this menu, then invokes the Change Date navigation callback on
        /// <c>MainPage</c>'s modal stack.
        /// </summary>
        private async void OnChangeDateClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
            if (_openChangeDateCallback is not null)
                await _openChangeDateCallback();
        }

        /// <summary>
        /// Pops this menu, then invokes the Settings navigation callback on
        /// <c>MainPage</c>'s modal stack.
        /// </summary>
        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
            if (_openSettingsCallback is not null)
                await _openSettingsCallback();
        }

        /// <summary>
        /// Quits the application. On iOS this is a no-op per Apple's Human Interface Guidelines.
        /// </summary>
        private void OnExitClicked(object sender, EventArgs e)
        {
            Application.Current?.Quit();
        }

        /// <summary>Dismisses the menu without performing any navigation.</summary>
        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}