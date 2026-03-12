using Aeonpulse.ViewModels;

namespace Aeonpulse.Views
{
    public partial class MainMenuPopup : ContentPage
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Called by <c>MainPage</c> after this popup has been fully popped,
        /// to push the next modal on MainPage's own navigation stack.
        /// Null means no follow-up navigation is needed.
        /// </summary>
        private readonly Func<Task>? _openChangeDateCallback;

        /// <summary>
        /// Called by <c>MainPage</c> after this popup has been fully popped,
        /// to push the Settings modal on MainPage's own navigation stack.
        /// </summary>
        private readonly Func<Task>? _openSettingsCallback;

        /// <param name="viewModel">The shared ViewModel, forwarded to child popups if needed.</param>
        /// <param name="topOffset">
        /// Vertical offset (in device-independent units) equal to the NavBar height,
        /// so the panel appears flush below the navigation bar.
        /// </param>
        /// <param name="rightOffset">
        /// Horizontal offset matching the NavBar's right edge padding so the panel
        /// is right-aligned with the navigation bar content.
        /// </param>
        /// <param name="openChangeDateCallback">
        /// Async action invoked on MainPage's navigation context when the user taps
        /// "Change Date", after this popup has been popped.
        /// </param>
        /// <param name="openSettingsCallback">
        /// Async action invoked on MainPage's navigation context when the user taps
        /// "Settings", after this popup has been popped.
        /// </param>
        public MainMenuPopup(MainViewModel viewModel, double topOffset, double rightOffset, Func<Task> openChangeDateCallback, Func<Task> openSettingsCallback)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _openChangeDateCallback = openChangeDateCallback;
            _openSettingsCallback = openSettingsCallback;

            // Position the panel below the NavBar, aligned to the right edge.
            MenuFrame.Margin = new Thickness(0, topOffset, rightOffset, 0);
        }

        private async void OnChangeDateClicked(object sender, EventArgs e)
        {
            // Pop this menu first; once awaited the modal stack is clear,
            // then let MainPage push ChangeDatePopup on its own navigation context.
            await Navigation.PopModalAsync();
            if (_openChangeDateCallback is not null)
                await _openChangeDateCallback();
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            // Pop this menu first; once awaited the modal stack is clear,
            // then let MainPage push SettingsPopup on its own navigation context.
            await Navigation.PopModalAsync();
            if (_openSettingsCallback is not null)
                await _openSettingsCallback();
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            Application.Current?.Quit();
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}