using Aeonpulse.ViewModels;

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
            DisplayAlert("AeonPulse", ((MainViewModel)BindingContext).TeaseText, "OK");
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
                "Time Jubilees",
                "The Methodology",
                "The word \"Jubilee\" itself comes from \"yobel\" (Hebrew), the ram's horn, which was sounded on the Day of Atonement (the 10th day of the 7th month) to proclaim the year. Jubilees are celebrated in round numbers because they symbolize a complete, intentional \"reset\" of life, land, and faith, stemming from various traditions. For example, 10th Year (Decennalia) is rooted in Roman tradition (e.g., Constantine) to mark a decade of rule. Numbers that are \"round\" (ending in 0, 5, 10, 100, etc.) or containing same digits (111, 33, etc.) appear \"nice\" or satisfying to us primarily because of processing fluency—they are much easier for our brains to remember, and understand compared to specific, precise numbers. This preference is rooted in both cognitive psychology and evolutionary history.",
                "Data Sources & Attribution",
                "Detailed breakdown of the exact time units remaining until your next major life milestone based on proprietary base-10 mathematics."
            );
        }

        private async void OnCountdownInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isCountdownDeepDiveOpen,
                v => _isCountdownDeepDiveOpen = v,
                "Countdown",
                "The Methodology",
                "Calculating the time units remaining until your closest next major milestone from the Time Jubilees.",
                "Data Sources & Attribution",
                "Detailed breakdown of the exact time units remaining until your next major life milestone based on proprietary base-10 mathematics."
            );
        }

        private async void OnLifeOdometerInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isLifeOdometerDeepDiveOpen,
                v => _isLifeOdometerDeepDiveOpen = v,
                "Life Odometer",
                "The Methodology",
                "A medical breakdown of average heart rates (70 bpm) and respiratory cycles (16 breaths/min) used to calculate your biological mileage.",
                "Data Sources & Attribution",
                "Calculation based on the data provided by the National Center for Biotechnology Information (NCBI) of the National Library of Medicine (NIH)."
            );
        }

        private async void OnAlienAnniversariesInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isAlienAnniversariesDeepDiveOpen,
                v => _isAlienAnniversariesDeepDiveOpen = v,
                "Alien Anniversaries",
                "The Methodology",
                "Planetary orbital mechanics is the study of how objects in space move under the influence of gravity, primarily dictated by Kepler's laws and Newton's law of universal gravitation. Planets move in elliptical orbits around the Sun, with orbital speeds increasing as they get closer to the Sun. Timekeeping on other planets requires translating Earth days into local days (sols) and years based on their rotation and orbital periods. A \"sol\" is the name for a 24-hour, 39-minute, 35-second solar day on Mars, just about 40 minutes longer than an Earth day. A year on Mars lasts 669.6 sols, which is equivalent to 687 Earth days (almost two Earth years). Venus is a strange planet with a \"retrograde\" rotation (spins backward) and a year that is shorter than its day. Venus completes one orbit in 225 Earth days, but takes 243 Earth days to rotate once. Therefore, a day on Venus is longer than a year on Venus.",
                "Data Sources & Attribution",
                "Data courtesy of the NASA Jet Propulsion Laboratory (JPL) Horizons On-Line Ephemeris System."
            );
        }

        private async void OnGalacticCommuteInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isGalacticCommuteDeepDiveOpen,
                v => _isGalacticCommuteDeepDiveOpen = v,
                "Galactic Commute",
                "The Methodology",
                "Our Solar System orbits the Milky Way's center at a speed of approximately 220-230 km/s (514,000 mph), completing one revolution every 225-230 million years. This journey, known as a Galactic Year or Cosmic Year, moves the system through the galactic disk at about 1/1300 of the speed of light.",
                "Data Sources & Attribution",
                "Data courtesy of the NASA Jet Propulsion Laboratory (JPL) Horizons On-Line Ephemeris System."
            );
        }

        private async void OnPhotonPathInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isPhotonPathDeepDiveOpen,
                v => _isPhotonPathDeepDiveOpen = v,
                "Photon Path",
                "The Methodology",
                "Astronomical data showing which specific stars are currently being passed through by light emitted from Earth on a base date, traveling at the speed of light in a vacuum of 299,792,458 m/s. The HYG Star Database (v3.0) is a comprehensive catalog of stars, including their positions, distances, and other relevant data, which allows us to determine which stars the light from Earth has reached over time.",
                "Data Sources & Attribution",
                "Star Data from the HYG Star Database (v3.0), distributed under the Creative Commons Attribution-ShareAlike 2.5 License."
            );
        }

        private async void OnHumanBirthRankInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isHumanBirthRankDeepDiveOpen,
                v => _isHumanBirthRankDeepDiveOpen = v,
                "Human Birth Rank",
                "The Methodology",
                "Demographic estimations based on historical global population models and UN data.",
                "Data Sources & Attribution",
                "World population statistics and projections courtesy of the United Nations, Department of Economic and Social Affairs, Population Division (2024 Revision). Historical estimates by Human Mortality Database (2025 revision) and the Population Reference Bureau (2024)."
            );
        }

        private async void OnBirthRuneInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isBirthRuneDeepDiveOpen,
                v => _isBirthRuneDeepDiveOpen = v,
                "Birth Rune",
                "The Methodology",
                "The Elder Futhark is the oldest runic alphabet, used by Germanic tribes from approximately AD 150 to 800 for inscriptions on artifacts. It consists of 24 characters, divided into three families - groups of eight (aettir), named after its first six letters (F-U-Þ-A-R-K). The Elder Futhark birth rune system assigns specific runes to birth dates, often based on a 24-rune cycle linked to the Sun's position throughout the year. In the Norse mysticism, each rune is more than just a symbol; it is a richly meaningful sign embodying various aspects of life, nature, and the divine powers revered by the Norse and other Germanic peoples.",
                "Data Sources & Attribution",
                "Runic interpretations based on the Elder Futhark system of the historical Viking Age."
            );
        }

        private async void OnPersonalYearInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isPersonalYearDeepDiveOpen,
                v => _isPersonalYearDeepDiveOpen = v,
                "Personal Year",
                "The Methodology",
                "Numerology (known before the 20th century as arithmancy) is the belief in an occult, divine or mystical relationship between a number and metaphysical phenomena or ideas. It is also the study of the numerical value, via an alphanumeric system, of the letters in words and names (e.g. gematria). When numerology is applied to a person's name, it is a form of onomancy. It is often associated with astrology and other divinatory arts. Number symbolism is an ancient and pervasive aspect of human thought, deeply intertwined with religion, philosophy, mysticism, and mathematics. Different cultures and traditions have assigned specific meanings to numbers, often linking them to divine principles, cosmic forces, or natural patterns.",
                "Data Sources & Attribution",
                "Personal Year calculations based on the Pythagorean system of Numerology."
            );
        }

        private async void OnGlobalExhaleInfoClicked(object sender, EventArgs e)
        {
            await OpenDeepDiveAsync(
                () => _isGlobalExhaleDeepDiveOpen,
                v => _isGlobalExhaleDeepDiveOpen = v,
                "Global Exhale",
                "The Methodology",
                "Ecological statistics tracking global environmental shifts, forest coverage, and carbon cycles during your specific timeframe.",
                "Data Sources & Attribution",
                "Emissions data courtesy of the Global Carbon Project (GCP) and the Global Carbon Budget (GCB) of University of Exeter's Global Systems Institute."
            );
        }
    }
}
