using Aeonpulse.Attributes;
using Aeonpulse.ViewModels;
using Aeonpulse.Resources;

namespace Aeonpulse.Views
{
    /// <summary>
    /// Code-behind for the application's single main page.
    /// Owns the modal navigation lifecycle for all popups: settings, date change,
    /// main menu, deep-dive info panels, and the refreshing overlay.
    ///
    /// <para>
    /// <b>Architecture note:</b> all business logic lives in <see cref="MainViewModel"/>.
    /// This class is strictly a navigation coordinator - it translates user gesture
    /// events into modal push/pop operations and routes ViewModel events
    /// (<see cref="MainViewModel.RefreshRequested"/>) to the UI layer.
    /// </para>
    /// <para>
    /// <b>Guard flags</b> (<c>_isXxxOpen</c>) prevent double-opening any popup if the
    /// user taps rapidly before the push animation completes. Each flag is set before
    /// <c>PushModalAsync</c> and cleared in a <c>finally</c> block after the await.
    /// </para>
    /// <para>
    /// <b>Hidden dependency:</b> <see cref="MainViewModel.RefreshRequested"/> is an
    /// <c>event Func&lt;Action, Task&gt;</c> wired in the constructor. If the ViewModel
    /// is reconstructed (e.g., hot-reload), the subscription must be re-established.
    /// </para>
    /// </summary>
    [AIContext("NavigationCoordinator")]
    public partial class MainPage : ContentPage
    {
        // --- Popup-open guard flags -------------------------------------------
        // Prevent double-push if the user taps rapidly before an animation completes.
        private bool _isChangeDatePopupOpen;
        private bool _isMainMenuOpen;
        private bool _isSettingsOpen;
        private bool _isTeasePopupOpen;
        private bool _isTimeJubileesDeepDiveOpen;
        private bool _isCountdownDeepDiveOpen;
        private bool _isLifeOdometerDeepDiveOpen;
        private bool _isAlienAnniversariesDeepDiveOpen;
        private bool _isGalacticCommuteDeepDiveOpen;
        private bool _isPhotonPathDeepDiveOpen;
        private bool _isCosmicStretchDeepDiveOpen;
        private bool _isHumanBirthRankDeepDiveOpen;
        private bool _isBirthRuneDeepDiveOpen;
        private bool _isPersonalYearDeepDiveOpen;
        private bool _isGlobalExhaleDeepDiveOpen;
        private bool _isYourBreathDeepDiveOpen;
        private bool _isCellularRefreshDeepDiveOpen;
        private bool _isVibrantCosmosDeepDiveOpen;
        private bool _isGlobalCrowdDeepDiveOpen;
        private bool _isLifeLogDeepDiveOpen;
        private bool _isSpaceWaitDeepDiveOpen;
        private bool _isVibrantHumanityDeepDiveOpen;
        private bool _isVibrantNatureDeepDiveOpen;

        /// <summary>
        /// Constructs the page and subscribes to the ViewModel's
        /// <see cref="MainViewModel.RefreshRequested"/> event, wiring the
        /// <see cref="RefreshingPopup"/> lifecycle to each ticker's refresh command.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            if (BindingContext is MainViewModel vm)
            {
                vm.RefreshRequested += OnTickerRefreshRequested;

                // Reposition TodayDot whenever the TimeJubilees result is replaced
                // (base-date change or manual refresh). Apply initial position now.
                vm.PropertyChanged += OnViewModelPropertyChanged;
                ApplyTodayDotPosition(vm.TimeJubilees?.ProgressFraction ?? 0.5);
                ApplyOrreryPositions(vm.AlienAnniversaries);
                ApplyOrreryBaseDate(vm.BaseDateName, vm.BaseDateDisplay);
            }
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not MainViewModel vm)
                return;

            if (e.PropertyName == nameof(MainViewModel.TimeJubilees))
                ApplyTodayDotPosition(vm.TimeJubilees?.ProgressFraction ?? 0.5);

            if (e.PropertyName == nameof(MainViewModel.AlienAnniversaries))
                ApplyOrreryPositions(vm.AlienAnniversaries);

            if (e.PropertyName == nameof(MainViewModel.BaseDateName)
             || e.PropertyName == nameof(MainViewModel.BaseDateDisplay))
                ApplyOrreryBaseDate(vm.BaseDateName, vm.BaseDateDisplay);
        }

        /// <summary>
        /// Positions <see cref="TodayDot"/> along the timeline by setting
        /// <c>AbsoluteLayout.LayoutBounds</c> Y to <paramref name="fraction"/>.
        ///
        /// <para>
        /// <b>Why imperative:</b> <c>AbsoluteLayout.LayoutBounds</c> is a string-typed
        /// attached property whose four comma-separated components cannot be individually
        /// data-bound in XAML. This method is the only correct way to position a child
        /// element at a proportional Y coordinate derived from a ViewModel value at runtime.
        /// </para>
        /// </summary>
        /// <param name="fraction">
        /// Clamped progress fraction [0.05, 0.95] from
        /// <see cref="Models.TimeJubileesResult.ProgressFraction"/>.
        /// </param>
        private void ApplyTodayDotPosition(double fraction)
        {
            const double dotTop    = 0.05;
            const double dotBottom = 0.95;
            double visualY = dotTop + fraction * (dotBottom - dotTop);

            AbsoluteLayout.SetLayoutBounds(TodayDot, new Rect(0.5, visualY, 14, 14));
            AbsoluteLayout.SetLayoutFlags(TodayDot,
                Microsoft.Maui.Layouts.AbsoluteLayoutFlags.PositionProportional);
        }

        /// <summary>
        /// Positions all five planet symbols and their name/years labels on the orrery canvas.
        ///
        /// <para>
        /// <b>Why imperative:</b> <c>AbsoluteLayout.LayoutBounds</c> is a string-typed
        /// attached property whose four components cannot be individually data-bound in XAML.
        /// This method computes screen-space X/Y coordinates from each planet's orbital
        /// fraction using Sin/Cos, then pushes them to the named label elements directly.
        /// </para>
        /// <para>
        /// <b>Coordinate system:</b> the orrery canvas is 300x300 device units. Center is (150,150).
        /// Fraction 0.0 = 12 o'clock (angle = -90 deg = 270 deg), clockwise.
        /// angle_deg = fraction * 360 - 90;  X = cx + r * cos(angle_rad);  Y = cy + r * sin(angle_rad).
        /// </para>
        /// </summary>
        /// <param name="result">The latest <see cref="Models.AlienAnniversariesResult"/>; may be null on startup.</param>
        private void ApplyOrreryPositions(Models.AlienAnniversariesResult? result)
        {
            if (result is null)
                return;

            const double cx         = 150;
            const double cy         = 150;
            const double symbolW    = 18; // square bounding box - centres glyph exactly on orbit point
            const double symbolH    = 18;
            const double labelW     = 100; // "Mercury 252.00" at FontSize=13 needs ~98px; 80 caused truncation
            const double labelH     = 20; // font-13 needs ~20 px height to avoid clipping
            const double labelGap   = 6;  // pixels between symbol edge and label centre
            const double toRad      = Math.PI / 180.0;

            // Orbit radii matching the XAML Ellipse sizes: 30, 55, 80, 110, 143
            (Label sym, Label lbl, double r, double fraction, string name, double years)[] planets =
            {
                (OrreryMercurySymbol, OrreryMercuryLabel,  30,  result.MercuryFraction, "Mercury", result.MercuryYears),
                (OrreryVenusSymbol,   OrreryVenusLabel,    55,  result.VenusFraction,   "Venus",   result.VenusYears),
                (OrreryEarthSymbol,   OrreryEarthLabel,    80,  result.EarthFraction,   "Earth",   result.EarthYears),
                (OrreryMarsSymbol,    OrreryMarsLabel,     110, result.MarsFraction,    "Mars",    result.MarsYears),
                (OrreryJupiterSymbol, OrreryJupiterLabel,  143, result.JupiterFraction, "Jupiter", result.JupiterYears),
            };

            foreach (var (sym, lbl, r, fraction, name, years) in planets)
            {
                // Map fraction [0,1) -> angle: 0.0=top(12 o'clock), clockwise
                double angleDeg = fraction * 360.0 - 90.0;
                double angleRad = angleDeg * toRad;
                double px = cx + r * Math.Cos(angleRad);
                double py = cy + r * Math.Sin(angleRad);

                // Symbol: square box centred exactly on the orbit point (px, py).
                // Using equal W and H ensures the glyph anchor is at the geometric
                // centre of the box, which is placed precisely on the orbit circle.
                AbsoluteLayout.SetLayoutBounds(sym, new Rect(px - symbolW / 2, py - symbolH / 2, symbolW, symbolH));
                AbsoluteLayout.SetLayoutFlags(sym, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);

                // Label: placed outward along the radial direction from the symbol edge.
                double labelDist = r + symbolH / 2 + labelGap;
                double lx = cx + labelDist * Math.Cos(angleRad) - labelW / 2;
                double ly = cy + labelDist * Math.Sin(angleRad) - labelH / 2;

                // Clamp to canvas bounds so labels near the edges do not clip.
                lx = Math.Clamp(lx, 0, 300 - labelW);
                ly = Math.Clamp(ly, 0, 300 - labelH);

                lbl.Text = $"{name} {years:F2}";
                AbsoluteLayout.SetLayoutBounds(lbl, new Rect(lx, ly, labelW, labelH));
                AbsoluteLayout.SetLayoutFlags(lbl, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);
            }
        }

        /// <summary>
        /// Updates the base-date name and value labels flanking the 12-o'clock Today line
        /// on the orrery canvas. Called on construction and whenever
        /// <see cref="MainViewModel.BaseDateName"/> or <see cref="MainViewModel.BaseDateDisplay"/>
        /// changes (i.e. on every <c>SaveDate</c>).
        ///
        /// <para>
        /// <b>Why imperative:</b> the two label elements live inside the same
        /// <c>AbsoluteLayout</c> as the planet symbols; their text content cannot be
        /// data-bound because the labels are children of a canvas whose coordinate system
        /// is managed entirely from code-behind. Populating them here keeps all orrery
        /// state mutations in one place.
        /// </para>
        /// </summary>
        /// <param name="name">The user's base-date label (e.g. "My Birthday").</param>
        /// <param name="display">The culture-formatted base date string (e.g. "7/24/1965").</param>
        private void ApplyOrreryBaseDate(string name, string display)
        {
            OrreryBaseDateNameLabel.Text  = name;
            OrreryBaseDateValueLabel.Text = display;
        }

        /// <summary>
        /// Generic popup lifecycle handler for any ticker refresh.
        /// Shows <see cref="RefreshingPopup"/> modally, waits 3 seconds, then
        /// dismisses the overlay and fires the ticker-specific recalculation.
        ///
        /// <para>
        /// The ticker-specific recalculation is fully encapsulated in
        /// <paramref name="onDismissed"/>, supplied by whichever
        /// <c>RefreshXxxCommand</c> raised the event - keeping this handler
        /// agnostic of which ticker is being refreshed.
        /// </para>
        /// </summary>
        /// <param name="onDismissed">
        /// Delegate that updates the specific <see cref="Models.TickerData"/> on the ViewModel.
        /// Executed after the <see cref="RefreshingPopup"/> has been fully popped.
        /// </param>
        private async Task OnTickerRefreshRequested(Action onDismissed)
        {
            var popup = new RefreshingPopup(onDismissed);

            await Navigation.PushModalAsync(popup);

            // The RefreshingPopup auto-dismisses itself in OnAppearing after 3 s.
            // We also guard here in case the pop hasn't fired yet.
            await Task.Delay(3000);

            if (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();

            onDismissed();
        }

        /// <summary>
        /// Tapping the logo or app name shows the tease popup - a single attention-grabbing
        /// live stat pulled from <see cref="MainViewModel.TeaseText"/>.
        /// The popup is positioned flush below the NavBar, left-aligned.
        /// If the user taps "To Clipboard", the stat is copied and a confirmation
        /// <see cref="DisplayAlert"/> is shown after the popup has been fully dismissed.
        /// </summary>
        private async void OnLogoTapped(object sender, EventArgs e)
        {
            if (_isTeasePopupOpen) return;
            _isTeasePopupOpen = true;
            try
            {
                var viewModel  = (MainViewModel)BindingContext;
                double topOffset  = NavBar.Height;
                double leftOffset = 16; // matches NavBar Padding="16,12"

                var popup = new TeasePopup(
                    viewModel.TeaseText,
                    topOffset,
                    leftOffset,
                    onCopiedCallback: async _ =>
                    {
                        await DisplayAlert(
                            AppResources.Tease_CopiedTitle,
                            AppResources.Tease_CopiedText,
                            AppResources.Tease_CopiedButtonOK);
                    });

                await Navigation.PushModalAsync(popup);
            }
            finally { _isTeasePopupOpen = false; }
        }

        /// <summary>
        /// Opens the <see cref="MainMenuPopup"/> anchored below the NavBar.
        /// The popup receives callbacks for "Change Date" and "Settings" so that
        /// follow-up navigation fires on this page's modal stack after the menu
        /// has been fully dismissed.
        /// </summary>
        private async void OnMenuClicked(object sender, EventArgs e)
        {
            if (_isMainMenuOpen) return;
            _isMainMenuOpen = true;
            try
            {
                var viewModel  = (MainViewModel)BindingContext;
                double topOffset   = NavBar.Height;
                double rightOffset = 16; // matches NavBar Padding="16,12"

                var popup = new MainMenuPopup(viewModel, topOffset, rightOffset,
                    openChangeDateCallback: async () =>
                    {
                        _isChangeDatePopupOpen = true;
                        try   { await Navigation.PushModalAsync(new ChangeDatePopup(viewModel)); }
                        finally { _isChangeDatePopupOpen = false; }
                    },
                    openSettingsCallback: async () =>
                    {
                        _isSettingsOpen = true;
                        try   { await Navigation.PushModalAsync(new SettingsPopup(viewModel)); }
                        finally { _isSettingsOpen = false; }
                    });

                await Navigation.PushModalAsync(popup);
            }
            finally { _isMainMenuOpen = false; }
        }

        /// <summary>
        /// Tapping the Timeline Heading opens the Change Date popup directly,
        /// providing a shortcut to the most common editing action.
        /// </summary>
        private async void OnTimelineHeadingTapped(object sender, EventArgs e)
        {
            if (_isChangeDatePopupOpen) return;
            _isChangeDatePopupOpen = true;
            try
            {
                var viewModel = (MainViewModel)BindingContext;
                await Navigation.PushModalAsync(new ChangeDatePopup(viewModel));
            }
            finally { _isChangeDatePopupOpen = false; }
        }

        /// <summary>
        /// Shared helper that calculates the correct top offset from the rendered
        /// NavBar + TimelineHeading heights and opens a <see cref="DeepDivePopup"/>.
        ///
        /// <para>
        /// <b>Guard pattern:</b> <paramref name="getGuard"/> / <paramref name="setGuard"/>
        /// provide read-write access to the caller's guard field, because async methods
        /// cannot use <c>ref</c> parameters.
        /// </para>
        /// </summary>
        /// <param name="getGuard">Returns the current guard flag value.</param>
        /// <param name="setGuard">Sets the guard flag.</param>
        /// <param name="title">Popup heading text.</param>
        /// <param name="section1Title">First section header (methodology).</param>
        /// <param name="section1Text">First section body.</param>
        /// <param name="section2Title">Second section header (sources).</param>
        /// <param name="section2Text">Second section body.</param>
        [AIContext("NavigationCoordinator")]
        private async Task OpenDeepDiveAsync(
            Func<bool> getGuard, Action<bool> setGuard,
            string title,
            string section1Title, string section1Text,
            string section2Title, string section2Text)
        {
            if (getGuard()) return;
            setGuard(true);
            try
            {
                double topOffset = NavBar.Height + TimelineHeading.Height;
                var popup = new DeepDivePopup(
                    title, section1Title, section1Text,
                    section2Title, section2Text, topOffset);
                await Navigation.PushModalAsync(popup);
            }
            finally { setGuard(false); }
        }

        // --- Deep Dive handlers - one per ticker card --------------------------

        /// <summary>Opens the Time Jubilees deep-dive info panel.</summary>
        private async void OnTimeJubileesInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isTimeJubileesDeepDiveOpen, v => _isTimeJubileesDeepDiveOpen = v,
                AppResources.Info_TimeJubileesTitle,
                AppResources.Info_MethodTitle, AppResources.Info_TimeJubileesMethod,
                AppResources.Info_SourceTitle, AppResources.Info_TimeJubileesSource);

        /// <summary>Opens the Countdown deep-dive info panel.</summary>
        private async void OnCountdownInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isCountdownDeepDiveOpen, v => _isCountdownDeepDiveOpen = v,
                AppResources.Info_CountdownTitle,
                AppResources.Info_MethodTitle, AppResources.Info_CountdownMethod,
                AppResources.Info_SourceTitle, AppResources.Info_CountdownSource);

        /// <summary>Opens the Life Odometer deep-dive info panel.</summary>
        private async void OnLifeOdometerInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isLifeOdometerDeepDiveOpen, v => _isLifeOdometerDeepDiveOpen = v,
                AppResources.Info_LifeOdometerTitle,
                AppResources.Info_MethodTitle, AppResources.Info_LifeOdometerMethod,
                AppResources.Info_SourceTitle, AppResources.Info_LifeOdometerSource);

        /// <summary>Opens the Alien Anniversaries deep-dive info panel.</summary>
        private async void OnAlienAnniversariesInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isAlienAnniversariesDeepDiveOpen, v => _isAlienAnniversariesDeepDiveOpen = v,
                AppResources.Info_AlienAnniversariesTitle,
                AppResources.Info_MethodTitle, AppResources.Info_AlienAnniversariesMethod,
                AppResources.Info_SourceTitle, AppResources.Info_AlienAnniversariesSource);

        /// <summary>Opens the Galactic Commute deep-dive info panel.</summary>
        private async void OnGalacticCommuteInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isGalacticCommuteDeepDiveOpen, v => _isGalacticCommuteDeepDiveOpen = v,
                AppResources.Info_GalacticCommuteTitle,
                AppResources.Info_MethodTitle, AppResources.Info_GalacticCommuteMethod,
                AppResources.Info_SourceTitle, AppResources.Info_GalacticCommuteSource);

        /// <summary>Opens the Photon Path deep-dive info panel.</summary>
        private async void OnPhotonPathInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isPhotonPathDeepDiveOpen, v => _isPhotonPathDeepDiveOpen = v,
                AppResources.Info_PhotonPathTitle,
                AppResources.Info_MethodTitle, AppResources.Info_PhotonPathMethod,
                AppResources.Info_SourceTitle, AppResources.Info_PhotonPathSource);

        /// <summary>Opens the Cosmic Stretch deep-dive info panel.</summary>
        private async void OnCosmicStretchInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isCosmicStretchDeepDiveOpen, v => _isCosmicStretchDeepDiveOpen = v,
                AppResources.Info_CosmicStretchTitle,
                AppResources.Info_MethodTitle, AppResources.Info_CosmicStretchMethod,
                AppResources.Info_SourceTitle, AppResources.Info_CosmicStretchSource);

        /// <summary>Opens the Human Birth Rank deep-dive info panel.</summary>
        private async void OnHumanBirthRankInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isHumanBirthRankDeepDiveOpen, v => _isHumanBirthRankDeepDiveOpen = v,
                AppResources.Info_HumanBirthRankTitle,
                AppResources.Info_MethodTitle, AppResources.Info_HumanBirthRankMethod,
                AppResources.Info_SourceTitle, AppResources.Info_HumanBirthRankSource);

        /// <summary>Opens the Birth Rune deep-dive info panel.</summary>
        private async void OnBirthRuneInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isBirthRuneDeepDiveOpen, v => _isBirthRuneDeepDiveOpen = v,
                AppResources.Info_BirthRuneTitle,
                AppResources.Info_MethodTitle, AppResources.Info_BirthRuneMethod,
                AppResources.Info_SourceTitle, AppResources.Info_BirthRuneSource);

        /// <summary>Opens the Personal Year deep-dive info panel.</summary>
        private async void OnPersonalYearInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isPersonalYearDeepDiveOpen, v => _isPersonalYearDeepDiveOpen = v,
                AppResources.Info_PersonalYearTitle,
                AppResources.Info_MethodTitle, AppResources.Info_PersonalYearMethod,
                AppResources.Info_SourceTitle, AppResources.Info_PersonalYearSource);

        /// <summary>Opens the Global Exhale deep-dive info panel.</summary>
        private async void OnGlobalExhaleInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isGlobalExhaleDeepDiveOpen, v => _isGlobalExhaleDeepDiveOpen = v,
                AppResources.Info_GlobalExhaleTitle,
                AppResources.Info_MethodTitle, AppResources.Info_GlobalExhaleMethod,
                AppResources.Info_SourceTitle, AppResources.Info_GlobalExhaleSource);

        /// <summary>Opens the Your Breath deep-dive info panel.</summary>
        private async void OnYourBreathInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isYourBreathDeepDiveOpen, v => _isYourBreathDeepDiveOpen = v,
                AppResources.Info_YourBreathTitle,
                AppResources.Info_MethodTitle, AppResources.Info_YourBreathMethod,
                AppResources.Info_SourceTitle, AppResources.Info_YourBreathSource);

        /// <summary>Opens the Cellular Refresh deep-dive info panel.</summary>
        private async void OnCellularRefreshInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isCellularRefreshDeepDiveOpen, v => _isCellularRefreshDeepDiveOpen = v,
                AppResources.Info_CellularRefreshTitle,
                AppResources.Info_MethodTitle, AppResources.Info_CellularRefreshMethod,
                AppResources.Info_SourceTitle, AppResources.Info_CellularRefreshSource);

        /// <summary>Opens the Vibrant Cosmos deep-dive info panel.</summary>
        private async void OnVibrantCosmosInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isVibrantCosmosDeepDiveOpen, v => _isVibrantCosmosDeepDiveOpen = v,
                AppResources.Info_VibrantCosmosTitle,
                AppResources.Info_MethodTitle, AppResources.Info_VibrantCosmosMethod,
                AppResources.Info_SourceTitle, AppResources.Info_VibrantCosmosSource);

        /// <summary>Opens the Global Crowd deep-dive info panel.</summary>
        private async void OnGlobalCrowdInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isGlobalCrowdDeepDiveOpen, v => _isGlobalCrowdDeepDiveOpen = v,
                AppResources.Info_GlobalCrowdTitle,
                AppResources.Info_MethodTitle, AppResources.Info_GlobalCrowdMethod,
                AppResources.Info_SourceTitle, AppResources.Info_GlobalCrowdSource);

        /// <summary>Opens the Life Log deep-dive info panel.</summary>
        private async void OnLifeLogInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isLifeLogDeepDiveOpen, v => _isLifeLogDeepDiveOpen = v,
                AppResources.Info_LifeLogTitle,
                AppResources.Info_MethodTitle, AppResources.Info_LifeLogMethod,
                AppResources.Info_SourceTitle, AppResources.Info_LifeLogSource);


        /// <summary>Opens the Space Wait deep-dive info panel.</summary>
        private async void OnSpaceWaitInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isSpaceWaitDeepDiveOpen, v => _isSpaceWaitDeepDiveOpen = v,
                AppResources.Info_SpaceWaitTitle,
                AppResources.Info_MethodTitle, AppResources.Info_SpaceWaitMethod,
                AppResources.Info_SourceTitle, AppResources.Info_SpaceWaitSource);

        /// <summary>Opens the Vibrant Humanity deep-dive info panel.</summary>
        private async void OnVibrantHumanityInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isVibrantHumanityDeepDiveOpen, v => _isVibrantHumanityDeepDiveOpen = v,
                AppResources.Info_VibrantHumanityTitle,
                AppResources.Info_MethodTitle, AppResources.Info_VibrantHumanityMethod,
                AppResources.Info_SourceTitle, AppResources.Info_VibrantHumanitySource);

        /// <summary>Opens the Vibrant Nature deep-dive info panel.</summary>
        private async void OnVibrantNatureInfoClicked(object sender, EventArgs e) =>
            await OpenDeepDiveAsync(
                () => _isVibrantNatureDeepDiveOpen, v => _isVibrantNatureDeepDiveOpen = v,
                AppResources.Info_VibrantNatureTitle,
                AppResources.Info_MethodTitle, AppResources.Info_VibrantNatureMethod,
                AppResources.Info_SourceTitle, AppResources.Info_VibrantNatureSource);

        // --- LIVE badge breathing animation ----------------------------------

        private const string LiveBadgeAnimationName = "LiveBadgeBreathing";

        /// <summary>
        /// Starts (or restarts) the continuous breathing animation on all 10 LIVE badge
        /// labels. Uses the MAUI <see cref="Animation"/> class directly so the effect is
        /// driven by the UI compositor, not by the 1-second ticker timer.
        ///
        /// <para>
        /// <b>Design note:</b> all 10 badge labels are set in a single Animation callback
        /// so they stay perfectly in phase. The parent animation is committed against
        /// <c>this</c> page as the <c>IAnimatable</c> owner, with
        /// <c>repeat: () => true</c> for seamless looping. AbortAnimation is called
        /// first to prevent duplicate animations if <c>OnAppearing</c> fires more than once
        /// (e.g., after a modal is dismissed and this page re-surfaces).
        /// </para>
        /// <para>
        /// <b>Layout stability:</b> only <c>Opacity</c> is animated (1.0 to 0.4 and back).
        /// No size or position property is touched, so neighbouring elements never shift.
        /// </para>
        /// </summary>
        private void StartLiveBadgeAnimation()
        {
            this.AbortAnimation(LiveBadgeAnimationName);

            var allBadges = new[]
            {
                LiveBadgeCountdown,
                LiveBadgeLifeOdometer,
                LiveBadgeSpaceWait,
                LiveBadgeGalacticCommute,
                LiveBadgePhotonPath,
                LiveBadgeCosmicStretch,
                LiveBadgeVibrantCosmos,
                LiveBadgeGlobalCrowd,
                LiveBadgeVibrantHumanity,
                LiveBadgeYourBreath,
            };

            var parent = new Animation();

            // Fade out: 1.0 -> 0.4 (first half of the 2500 ms cycle)
            var fadeOut = new Animation(
                v => { foreach (var b in allBadges) b.Opacity = v; },
                start: 1.0, end: 0.4, easing: Easing.SinInOut);

            // Fade in: 0.4 -> 1.0 (second half of the 2500 ms cycle)
            var fadeIn = new Animation(
                v => { foreach (var b in allBadges) b.Opacity = v; },
                start: 0.4, end: 1.0, easing: Easing.SinInOut);

            parent.Add(0.0, 0.5, fadeOut);
            parent.Add(0.5, 1.0, fadeIn);

            parent.Commit(this, LiveBadgeAnimationName, length: 2500, repeat: () => true);
        }

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartLiveBadgeAnimation();
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.AbortAnimation(LiveBadgeAnimationName);
        }
    }
}
