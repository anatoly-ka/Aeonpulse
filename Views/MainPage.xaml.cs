using Aeonpulse.ViewModels;
using Aeonpulse.Resources;

namespace Aeonpulse.Views
{
    public partial class MainPage : ContentPage
    {
        private bool _isChangeDatePopupOpen;
        private bool _isMainMenuOpen;
        private bool _isSettingsOpen;
        private bool _isTimeJubileesDeepDiveOpen;
        private bool _isCountdownDeepDiveOpen;
        private bool _isLifeOdometerDeepDiveOpen;
        private bool _isAlienAnniversariesDeepDiveOpen;
        private bool _isGalacticCommuteDeepDiveOpen;
        private bool _isPhotonPathDeepDiveOpen;
        private bool _isHumanBirthRankDeepDiveOpen;
        private bool _isBirthRuneDeepDiveOpen;
        private bool _isPersonalYearDeepDiveOpen;
        private bool _isGlobalExhaleDeepDiveOpen;

        public MainPage()
        {
            InitializeComponent();

            if (BindingContext is MainViewModel vm)
            {
                // Each ticker's RefreshRequested event is routed through the same
                // popup lifecycle handler; the onDismissed callback carries the
                // ticker-specific recalculation supplied by the ViewModel.
                vm.RefreshRequested += OnTickerRefreshRequested;
            }
        }

        /// <summary>
        /// Generic popup lifecycle handler for any ticker refresh.
        /// The ticker-specific recalculation is fully encapsulated in <paramref name="onDismissed"/>,
        /// supplied by whichever RefreshXxxCommand raised the event.
        /// </summary>
        private async Task OnTickerRefreshRequested(Action onDismissed)
        {
            var popup = new RefreshingPopup(onDismissed);

            // Show the loading spinner
            await Navigation.PushModalAsync(popup);

            // Wait for 3 seconds to simulate refreshing
            await Task.Delay(3000);

            // Dismiss the popup
            if (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();

            // Execute the ticker-specific recalculation callback
            onDismissed();
        }

        private void OnLogoTapped(object sender, EventArgs e)
        {
            // Show tease popup
            DisplayAlert(AppResources.Tease_Title, ((MainViewModel)BindingContext).TeaseText, AppResources.Tease_ButtonOK);
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            // Ignore taps while the popup is already open or being pushed
            if (_isMainMenuOpen)
                return;

            _isMainMenuOpen = true;
            try
            {
                var viewModel = (MainViewModel)BindingContext;
                double topOffset = NavBar.Height;
                // Right-align with the NavBar's content padding (matches Padding="16,12" in XAML)
                double rightOffset = 16;

                // Each callback runs on MainPage's navigation stack after MainMenuPopup
                // has been fully popped, so PushModalAsync has a live context to push onto.
                var popup = new MainMenuPopup(viewModel, topOffset, rightOffset,
                    openChangeDateCallback: async () =>
                    {
                        _isChangeDatePopupOpen = true;
                        try
                        {
                            await Navigation.PushModalAsync(new ChangeDatePopup(viewModel));
                        }
                        finally
                        {
                            _isChangeDatePopupOpen = false;
                        }
                    },
                    openSettingsCallback: async () =>
                    {
                        _isSettingsOpen = true;
                        try
                        {
                            await Navigation.PushModalAsync(new SettingsPopup(viewModel));
                        }
                        finally
                        {
                            _isSettingsOpen = false;
                        }
                    });
                await Navigation.PushModalAsync(popup);
            }
            finally
            {
                _isMainMenuOpen = false;
            }
        }

        private async void OnTimelineHeadingTapped(object sender, EventArgs e)
        {
            // Ignore taps while the popup is already open or being pushed
            if (_isChangeDatePopupOpen)
                return;

            _isChangeDatePopupOpen = true;
            try
            {
                var viewModel = (MainViewModel)BindingContext;
                var popup = new ChangeDatePopup(viewModel);
                await Navigation.PushModalAsync(popup);
            }
            finally
            {
                _isChangeDatePopupOpen = false;
            }
        }

        /// <summary>
        /// Builds the top offset from the rendered nav bar and timeline heading heights,
        /// then opens a <see cref="DeepDivePopup"/> with the supplied content.
        /// The <paramref name="getGuard"/> and <paramref name="setGuard"/> callbacks
        /// provide read/write access to the caller's guard field, since async methods
        /// cannot take ref parameters.
        /// </summary>
        private async Task OpenDeepDiveAsync(Func<bool> getGuard, Action<bool> setGuard, string title, string section1Title, string section1Text, string section2Title, string section2Text)
        {
            if (getGuard())
                return;

            setGuard(true);
            try
            {
                double topOffset = NavBar.Height + TimelineHeading.Height;
                var popup = new DeepDivePopup(title, section1Title, section1Text, section2Title, section2Text, topOffset);
                await Navigation.PushModalAsync(popup);
            }
            finally
            {
                setGuard(false);
            }
        }

        private async void OnTimeJubileesInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isTimeJubileesDeepDiveOpen,
                v => _isTimeJubileesDeepDiveOpen = v,
                AppResources.Info_TimeJubileesTitle,
                AppResources.Info_MethodTitle,
                AppResources.Info_TimeJubileesMethod,
                AppResources.Info_SourceTitle,
                AppResources.Info_TimeJubileesSource
            );
        }

        private async void OnCountdownInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isCountdownDeepDiveOpen,
                v => _isCountdownDeepDiveOpen = v,
                AppResources.Info_CountdownTitle,
                AppResources.Info_MethodTitle,
                AppResources.Info_CountdownMethod,
                AppResources.Info_SourceTitle,
                AppResources.Info_CountdownSource
            );
        }

        private async void OnLifeOdometerInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isLifeOdometerDeepDiveOpen,
                v => _isLifeOdometerDeepDiveOpen = v,
                AppResources.Info_LifeOdometerTitle,
                AppResources.Info_MethodTitle,
                AppResources.Info_LifeOdometerMethod,
                AppResources.Info_SourceTitle,
                AppResources.Info_LifeOdometerSource
            );
        }

        private async void OnAlienAnniversariesInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isAlienAnniversariesDeepDiveOpen,
                v => _isAlienAnniversariesDeepDiveOpen = v,
                AppResources.Info_AlienAnniversariesTitle,
                AppResources.Info_MethodTitle,
                AppResources.Info_AlienAnniversariesMethod,
                AppResources.Info_SourceTitle,
                AppResources.Info_AlienAnniversariesSource
            );
        }

        private async void OnGalacticCommuteInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isGalacticCommuteDeepDiveOpen,
                v => _isGalacticCommuteDeepDiveOpen = v,
                AppResources.Info_GalacticCommuteTitle,
                AppResources.Info_MethodTitle,
                AppResources.Info_GalacticCommuteMethod,
                AppResources.Info_SourceTitle,
                AppResources.Info_GalacticCommuteSource
            );
        }

        private async void OnPhotonPathInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isPhotonPathDeepDiveOpen,
                v => _isPhotonPathDeepDiveOpen = v,
                AppResources.Info_PhotonPathTitle,
                AppResources.Info_MethodTitle,
                AppResources.Info_PhotonPathMethod,
                AppResources.Info_SourceTitle,
                AppResources.Info_PhotonPathSource
            );
        }

        private async void OnHumanBirthRankInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isHumanBirthRankDeepDiveOpen,
                v => _isHumanBirthRankDeepDiveOpen = v,
                AppResources.Info_HumanBirthRankTitle,
                AppResources.Info_MethodTitle,
                AppResources.Info_HumanBirthRankMethod,
                AppResources.Info_SourceTitle,
                AppResources.Info_HumanBirthRankSource
            );
        }

        private async void OnBirthRuneInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isBirthRuneDeepDiveOpen,
                v => _isBirthRuneDeepDiveOpen = v,
                AppResources.Info_BirthRuneTitle,
                AppResources.Info_MethodTitle,
                AppResources.Info_BirthRuneMethod,
                AppResources.Info_SourceTitle,
                AppResources.Info_BirthRuneSource
            );
        }

        private async void OnPersonalYearInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isPersonalYearDeepDiveOpen,
                v => _isPersonalYearDeepDiveOpen = v,
                AppResources.Info_PersonalYearTitle,
                AppResources.Info_MethodTitle,
                AppResources.Info_PersonalYearMethod,
                AppResources.Info_SourceTitle,
                AppResources.Info_PersonalYearSource
            );
        }

        private async void OnGlobalExhaleInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isGlobalExhaleDeepDiveOpen,
                v => _isGlobalExhaleDeepDiveOpen = v,
                AppResources.Info_GlobalExhaleTitle,
                AppResources.Info_MethodTitle,
                AppResources.Info_GlobalExhaleMethod,
                AppResources.Info_SourceTitle,
                AppResources.Info_GlobalExhaleSource
            );
        }
    }
}
