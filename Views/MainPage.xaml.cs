using Aeonpulse.Attributes;
using Aeonpulse.Models;
using Aeonpulse.Services;
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

        // Stores the last photon track fraction so it can be re-applied on SizeChanged
        // when the Line element's rendered width becomes available after the first layout pass.
        private double _photonTrackFraction;

        // Ambient Sparks animation state.
        // _sparksCts is replaced on each Start call and cancelled on each Stop call.
        // _rng is shared across all spawn calls (not thread-local - always accessed on UI thread).
        // _sparksRunning prevents double-start when OnAppearing and PropertyChanged both fire.
        // _liveStars tracks all star-birth Labels currently visible on CosmosCanvas so that
        // a supernova can hijack one. Accessed under lock(_liveStars) from the UI thread.
        private CancellationTokenSource? _sparksCts;
        private readonly Random _rng = new Random();
        private bool _sparksRunning;
        private readonly List<Label> _liveStars = new List<Label>();

        // Web of Wyrd Explorer state (Birth Rune expanded view).
        // _wyrdCatalogue is rebuilt on each ApplyWyrdWeb call so locale changes
        // are reflected. _wyrdSelectedIndex tracks the current selection; it is
        // reset to the user's birth rune on first open or when the calculated rune changes.
        // _wyrdLastRuneName stores the RuneName from the previous ApplyWyrdWeb call
        // so that a base-date change that produces a different rune is detected correctly.
        private IReadOnlyList<Models.FutharkRune>? _wyrdCatalogue;
        private int _wyrdSelectedIndex;
        private string _wyrdLastRuneName = string.Empty;

        // Stored so TodayContextBlock.SizeChanged can re-fire ApplyTodayDotPosition
        // with the correct fraction after the block's height changes (e.g. when the
        // visible sub-label switches between 1 line and 3 lines).
        private double _todayFraction = 0.5;


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
                ApplyPhotonTrackPosition(vm.PhotonPath?.ProgressFraction ?? 0d);
                ApplyBirthRankChart(vm);
                ApplyWyrdWeb(vm);
                ApplyEnneagram(vm);
                ApplyPopulationChart(vm);
                ApplyLifeLogChart(vm);
                ApplyVibrantHumanityBars(vm);
                ApplyTaxonomyFlow(vm);
                ApplyCarbonBudgetChart(vm);
                _ = ApplyVolumeCubeAsync(vm);

                // Wire scroll-to-ticker: when a Favorites tile is tapped,
                // JumpToTicker in the VM raises this event so we can call ScrollToAsync.
                vm.ScrollToTickerRequested += tickerId => _ = ScrollToTickerAsync(tickerId);

            }

            // Re-apply the dotted fill Line endpoint after the first layout pass,
            // because PhotonTrackFill.Width is not available until the element is measured.
            PhotonTrackFill.SizeChanged += (_, _) =>
                ApplyPhotonTrackPosition(_photonTrackFraction);
            // Re-apply volume cube after first layout so LastPpm is populated.
            VolumeCubeView.SizeChanged += (_, _) =>
            {
                if (BindingContext is MainViewModel vmSc) _ = ApplyVolumeCubeAsync(vmSc);
            };
            // Re-apply Today dot+label position whenever any of the three label
            // elements change size - TodayContextBlock when IsMoreRoomAtBottom flips
            // (1-line vs 3-line), and JubileeLabelLast/Next when text or font changes.
            // All three handlers use the stored _todayFraction.
            TodayContextBlock.SizeChanged += (_, _) => ApplyTodayDotPosition(_todayFraction);
            JubileeLabelLast.SizeChanged  += (_, _) => ApplyTodayDotPosition(_todayFraction);
            JubileeLabelNext.SizeChanged  += (_, _) => ApplyTodayDotPosition(_todayFraction);
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

            if (e.PropertyName == nameof(MainViewModel.PhotonPath))
                ApplyPhotonTrackPosition(vm.PhotonPath?.ProgressFraction ?? 0d);

            if (e.PropertyName == nameof(MainViewModel.VibrantCosmosExpanded))
            {
                if (vm.VibrantCosmosExpanded)
                    StartAmbientSparks();
                else
                    StopAmbientSparks();
            }

            if (e.PropertyName == nameof(MainViewModel.HumanBirthRankExpanded) ||
                e.PropertyName == nameof(MainViewModel.HumanBirthRank)         ||
                e.PropertyName == nameof(MainViewModel.ColorScheme))
                ApplyBirthRankChart(vm);

            if (e.PropertyName == nameof(MainViewModel.BirthRuneExpanded) ||
                e.PropertyName == nameof(MainViewModel.BirthRune)         ||
                e.PropertyName == nameof(MainViewModel.ColorScheme)       ||
                e.PropertyName == nameof(MainViewModel.DisplayLanguage))
                ApplyWyrdWeb(vm);

            if (e.PropertyName == nameof(MainViewModel.PersonalYearExpanded) ||
                e.PropertyName == nameof(MainViewModel.PersonalYear)         ||
                e.PropertyName == nameof(MainViewModel.ColorScheme))
                ApplyEnneagram(vm);

            if (e.PropertyName == nameof(MainViewModel.GlobalCrowdExpanded) ||
                e.PropertyName == nameof(MainViewModel.GlobalCrowd)         ||
                e.PropertyName == nameof(MainViewModel.ColorScheme))
                ApplyPopulationChart(vm);

            if (e.PropertyName == nameof(MainViewModel.LifeLogExpanded) ||
                e.PropertyName == nameof(MainViewModel.LifeLog)         ||
                e.PropertyName == nameof(MainViewModel.ColorScheme))
                ApplyLifeLogChart(vm);

            if (e.PropertyName == nameof(MainViewModel.VibrantHumanityExpanded) ||
                e.PropertyName == nameof(MainViewModel.VibrantHumanity)         ||
                e.PropertyName == nameof(MainViewModel.DisplayLanguage))
                ApplyVibrantHumanityBars(vm);

            if (e.PropertyName == nameof(MainViewModel.VibrantNatureExpanded) ||
                e.PropertyName == nameof(MainViewModel.VibrantNature)         ||
                e.PropertyName == nameof(MainViewModel.ColorScheme)          ||
                e.PropertyName == nameof(MainViewModel.DisplayLanguage))
                ApplyTaxonomyFlow(vm);

            if (e.PropertyName == nameof(MainViewModel.GlobalExhaleExpanded) ||
                e.PropertyName == nameof(MainViewModel.GlobalExhale)         ||
                e.PropertyName == nameof(MainViewModel.ColorScheme)          ||
                e.PropertyName == nameof(MainViewModel.DisplayLanguage))
                ApplyCarbonBudgetChart(vm);

            if (e.PropertyName == nameof(MainViewModel.YourBreathExpanded) ||
                e.PropertyName == nameof(MainViewModel.YourBreath)         ||
                e.PropertyName == nameof(MainViewModel.ColorScheme)        ||
                e.PropertyName == nameof(MainViewModel.UseMetric)          ||
                e.PropertyName == nameof(MainViewModel.DisplayLanguage))
                _ = ApplyVolumeCubeAsync(vm);
        }

        /// <summary>
        /// Positions both <see cref="TodayDot"/> and <see cref="TodayContextBlock"/> to the
        /// same proportional Y coordinate along the timeline.
        ///
        /// <para>
        /// <b>Why imperative:</b> <c>AbsoluteLayout.LayoutBounds</c> is a four-component
        /// attached property whose individual components cannot be data-bound in XAML.
        /// Both the dot (Col 0) and the label block (Col 1) live inside
        /// <c>AbsoluteLayout</c> containers with <c>YProportional</c> flags, so setting
        /// <c>LayoutBounds.Y = visualY</c> on each is the only correct approach.
        /// </para>
        /// <para>
        /// <b>Geometry:</b> <c>visualY</c> is the fraction clamped to [0.05, 0.95] so
        /// neither the dot nor the label block ever touches the endpoint dots.
        /// The dot uses <c>visualY</c> directly. The label block adds a 2 px inset on each
        /// end (<c>labelGapFraction = 2 / 220.0</c>) so there is always at least 2 extra
        /// pixels of air between the Today text and the Last / Next milestone labels.
        /// </para>
        /// </summary>
        /// <param name="fraction">
        /// Clamped progress fraction [0.05, 0.95] from
        /// <see cref="Models.TimeJubileesResult.ProgressFraction"/>.
        /// </param>
        private void ApplyTodayDotPosition(double fraction)
        {
            _todayFraction = fraction;

            // -- Col 0: position the gold ring dot with full proportional range --
            const double dotTop    = 0.05;
            const double dotBottom = 0.95;
            double visualY = dotTop + fraction * (dotBottom - dotTop);

            AbsoluteLayout.SetLayoutBounds(TodayDot, new Rect(0.5, visualY, 14, 14));
            AbsoluteLayout.SetLayoutFlags(TodayDot,
                Microsoft.Maui.Layouts.AbsoluteLayoutFlags.PositionProportional);

            // -- Col 1: all three labels use AbsoluteLayoutFlags.None (absolute px) --
            // JubileeLabelsPanel.Height is the real rendered panel height.
            // Using absolute pixels eliminates the YProportional (containerH-childH)*f
            // formula that produced incorrect nextTopPx values in earlier attempts.
            double panelH = JubileeLabelsPanel.Height > 0 ? JubileeLabelsPanel.Height : 220.0;

            // Last label: top edge at dotTop fraction of the panel height.
            double lastTopPx   = dotTop * panelH;
            double lastLabelH  = JubileeLabelLast.Height > 0 ? JubileeLabelLast.Height : 18.0;
            AbsoluteLayout.SetLayoutBounds(JubileeLabelLast,
                new Rect(0, lastTopPx, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(JubileeLabelLast,
                Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);

            // Next label: BOTTOM edge at dotBottom fraction of the panel height,
            // so top = dotBottom*panelH - labelHeight.  This mirrors how the dot sits.
            double nextLabelH  = JubileeLabelNext.Height > 0 ? JubileeLabelNext.Height : 18.0;
            double nextBotPx   = dotBottom * panelH;
            double nextTopPx   = nextBotPx - nextLabelH;
            AbsoluteLayout.SetLayoutBounds(JubileeLabelNext,
                new Rect(0, nextTopPx, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(JubileeLabelNext,
                Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);

            // TodayContextBlock: clamp top-edge so it never overlaps Last or Next.
            const double gap   = 2.0;
            double lastBotPx   = lastTopPx + lastLabelH;
            double blockH      = TodayContextBlock.Height > 0 ? TodayContextBlock.Height : 18.0;
            double wantedTopPx = visualY * panelH;
            double minTopPx    = lastBotPx  + gap;
            double maxTopPx    = nextTopPx  - blockH - gap;

            if (minTopPx > maxTopPx)
                minTopPx = maxTopPx = (lastBotPx + nextTopPx - blockH) / 2.0;

            double clampedTopPx = Math.Clamp(wantedTopPx, minTopPx, maxTopPx);
            AbsoluteLayout.SetLayoutBounds(TodayContextBlock,
                new Rect(0, clampedTopPx, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(TodayContextBlock,
                Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);
        }

        /// <summary>
        /// Positions <see cref="PhotonShipMarker"/> and resizes <see cref="PhotonTrackFill"/>
        /// along the horizontal Photon Path track by setting <c>AbsoluteLayout.LayoutBounds</c>
        /// on both named elements.
        ///
        /// <para>
        /// <b>Why imperative:</b> <c>AbsoluteLayout.LayoutBounds</c> is a four-component string
        /// attached property whose individual components cannot be data-bound from XAML.
        /// The ship marker X and the fill track width must both reflect
        /// <see cref="Models.PhotonPathResult.ProgressFraction"/> at runtime.
        /// </para>
        /// <para>
        /// <b>Track geometry:</b> X=0.0 is the Sun; X=1.0 is the next star.
        /// The fill <see cref="Microsoft.Maui.Controls.Shapes.Line"/> spans the full track
        /// width via <c>WidthProportional</c> with height=2 so the layout engine allocates
        /// render space. <c>X2 = fraction * Width</c> sets the dotted stroke endpoint.
        /// The ship <see cref="Ellipse"/> uses <c>PositionProportional</c> so its X value
        /// maps directly to the fraction.
        /// </para>
        /// </summary>
        /// <param name="fraction">
        /// Progress fraction [0.0, 1.0] from <see cref="Models.PhotonPathResult.ProgressFraction"/>.
        /// </param>
        private void ApplyPhotonTrackPosition(double fraction)
        {
            double clamped = Math.Clamp(fraction, 0d, 1d);
            _photonTrackFraction = clamped;

            // Fill track: the Line fills the AbsoluteLayout (SizeProportional).
            // X1=0 is the Sun; X2 = fraction * rendered width gives the dotted fill endpoint.
            // If the element has not yet been measured (Width <= 0), the SizeChanged handler
            // will re-apply once the first layout pass completes.
            double trackWidth = PhotonTrackFill.Width;
            if (trackWidth > 0)
                PhotonTrackFill.X2 = clamped * trackWidth;

            // Ship marker: X = fraction (proportional), Y = 0.5 (centred), 16x16 absolute.
            AbsoluteLayout.SetLayoutBounds(PhotonShipMarker,
                new Rect(clamped, 0.5, 16, 16));
            AbsoluteLayout.SetLayoutFlags(PhotonShipMarker,
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

            const double cx      = 150;
            const double cy      = 150;
            const double symbolW = 18;
            const double symbolH = 18;
            const double labelW  = 100;
            const double labelH  = 20;
            const double gap     = 1; // px between symbol edge and nearest label edge
            const double toRad   = Math.PI / 180.0;

            // Orbit radii matching the XAML Ellipse sizes: 30, 55, 80, 110, 143.
            // isJupiter flag controls the inverted above/below rule for the outermost orbit.
            (Label sym, Label lbl, double r, double fraction, string name, double years, bool isJupiter)[] planets =
            {
                (OrreryMercurySymbol, OrreryMercuryLabel,  30,  result.MercuryFraction, "Mercury", result.MercuryYears, false),
                (OrreryVenusSymbol,   OrreryVenusLabel,    55,  result.VenusFraction,   "Venus",   result.VenusYears,   false),
                (OrreryEarthSymbol,   OrreryEarthLabel,    80,  result.EarthFraction,   "Earth",   result.EarthYears,   false),
                (OrreryMarsSymbol,    OrreryMarsLabel,     110, result.MarsFraction,    "Mars",    result.MarsYears,    false),
                (OrreryJupiterSymbol, OrreryJupiterLabel,  143, result.JupiterFraction, "Jupiter", result.JupiterYears, true),
            };

            foreach (var (sym, lbl, r, fraction, name, years, isJupiter) in planets)
            {
                // Map fraction [0,1) -> angle: 0.0 = 12 o'clock, clockwise.
                double angleDeg = fraction * 360.0 - 90.0;
                double angleRad = angleDeg * toRad;
                double px = cx + r * Math.Cos(angleRad);
                double py = cy + r * Math.Sin(angleRad);

                // Symbol: square box centred exactly on the orbit point (px, py).
                AbsoluteLayout.SetLayoutBounds(sym, new Rect(px - symbolW / 2, py - symbolH / 2, symbolW, symbolH));
                AbsoluteLayout.SetLayoutFlags(sym, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);

                // Label placement: purely vertical, no radial direction.
                // "Upper half" = symbol centre is above the canvas centre (py < cy).
                //
                // Mercury/Venus/Earth/Mars: upper half -> label ABOVE symbol
                //                           lower half -> label BELOW symbol
                // Jupiter (largest orbit):  upper half -> label BELOW symbol
                //                           lower half -> label ABOVE symbol
                bool inUpperHalf = py < cy;
                bool placeAbove  = isJupiter ? !inUpperHalf : inUpperHalf;

                double ly = placeAbove
                    ? py - symbolH / 2.0 - gap - labelH   // gap above symbol top
                    : py + symbolH / 2.0 + gap;            // gap below symbol bottom

                // Horizontally centre the label over the planet symbol.
                double lx = px - labelW / 2.0;

                // Clamp to canvas bounds so labels never render outside the 300x300 area.
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

        // --- Favorites tile portal: scroll to a ticker card in the main list --------

        /// <summary>
        /// Scrolls the main ScrollView so the ticker card with the given
        /// <paramref name="tickerId"/> is visible at the top of the viewport.
        /// Called when <see cref="MainViewModel.ScrollToTickerRequested"/> fires
        /// after a Favorites tile is tapped.
        /// A short delay is inserted so MAUI has time to measure the newly-expanded
        /// section and card before the scroll position is calculated.
        /// </summary>
        private async Task ScrollToTickerAsync(string tickerId)
        {
            // Brief yield so the layout engine can process the section/card expansion
            // that JumpToTicker triggered just before raising this event.
            await Task.Delay(120);
            View? target = tickerId switch
            {
                "TimeJubilees"       => TickerCardTimeJubilees,
                "Countdown"          => TickerCardCountdown,
                "LifeOdometer"       => TickerCardLifeOdometer,
                "AlienAnniversaries" => TickerCardAlienAnniversaries,
                "GalacticCommute"    => TickerCardGalacticCommute,
                "PhotonPath"         => TickerCardPhotonPath,
                "CosmicStretch"      => TickerCardCosmicStretch,
                "HumanBirthRank"     => TickerCardHumanBirthRank,
                "BirthRune"          => TickerCardBirthRune,
                "PersonalYear"       => TickerCardPersonalYear,
                "GlobalExhale"       => TickerCardGlobalExhale,
                "YourBreath"         => TickerCardYourBreath,
                "CellularRefresh"    => TickerCardCellularRefresh,
                "VibrantCosmos"      => TickerCardVibrantCosmos,
                "GlobalCrowd"        => TickerCardGlobalCrowd,
                "LifeLog"            => TickerCardLifeLog,
                "SpaceWait"          => TickerCardSpaceWait,
                "VibrantHumanity"    => TickerCardVibrantHumanity,
                "VibrantNature"      => TickerCardVibrantNature,
                _                    => null
            };
            if (target != null)
                await MainScrollView.ScrollToAsync(target, ScrollToPosition.Start, animated: true);
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
        /// <summary>
        /// Populates the Taxonomy Flow (Sankey-style) diagram inside
        /// <see cref="TaxonomyFlowContainer"/>. Sets localised header labels,
        /// creates a fresh <see cref="TaxonomyFlowDrawable"/> from the five
        /// discovery/extinction counts on <c>VibrantNatureResult</c>, assigns it
        /// to <see cref="TaxonomyFlowView"/>, and calls <c>Invalidate()</c>.
        ///
        /// <para>Called on <c>VibrantNatureExpanded</c>, <c>VibrantNature</c>,
        /// <c>ColorScheme</c>, and <c>DisplayLanguage</c> changes and from the
        /// constructor. Hidden when <c>VibrantNatureExpanded</c> is false.</para>
        /// </summary>
        private void ApplyTaxonomyFlow(MainViewModel vm)
        {
            var result = vm.VibrantNature;
            bool show = vm.VibrantNatureExpanded;
            TaxonomyFlowContainer.IsVisible = show;
            if (!show || result == null) return;

            TaxonomyDiscoveriesLabel.Text = AppResources.Chart_VibrantNature_Discoveries;
            TaxonomyDiscoveriesLabel.SetDynamicResource(Label.TextColorProperty, "NeonGreen");
            TaxonomyExtinctionsLabel.Text = AppResources.Chart_VibrantNature_Extinctions;
            TaxonomyExtinctionsLabel.SetDynamicResource(Label.TextColorProperty, "CyberPink");

            AssignTaxonomyFlowDrawable(result);
        }

        private void AssignTaxonomyFlowDrawable(VibrantNatureResult result)
        {
            // Circle and icon are drawn by XAML Border + Image (TaxonomyCircle / icon_taxonomy.png),
            // tinted via ImageTint.Color=TextWhite. Only the stream beziers live in the drawable.
            TaxonomyFlowView.Drawable = new TaxonomyFlowDrawable
            {
                TotalDiscovered       = result.DiscoveredSince,
                TotalExtinct          = result.ExtinctSince,
                InsectsDiscovered     = result.InsectsDiscovered,
                PlantsDiscovered      = result.PlantsDiscovered,
                VertebratesDiscovered = result.VertebratesDiscovered,
                InsectsExtinct        = result.InsectsExtinct,
                VertebratesExtinct    = result.VertebratesExtinct,
                InLabels  = new[]
                {
                    AppResources.Chart_VibrantNature_LabelInsectsIn,
                    AppResources.Chart_VibrantNature_LabelPlantsIn,
                    AppResources.Chart_VibrantNature_LabelVertsIn,
                    AppResources.Chart_VibrantNature_LabelOthersIn,
                },
                OutLabels = new[]
                {
                    AppResources.Chart_VibrantNature_LabelInsectsOut,
                    AppResources.Chart_VibrantNature_LabelVertsOut,
                    AppResources.Chart_VibrantNature_LabelOthersOut,
                },
            };
            TaxonomyFlowView.Invalidate();
        }

        /// <summary>
        /// Sets a fresh <see cref="BirthRankChartDrawable"/> on the <see cref="BirthRankChart"/>
        /// <c>GraphicsView</c> and triggers a redraw whenever the card expands or the result changes.
        ///
        /// <para>
        /// <b>Why imperative:</b> <c>GraphicsView.Drawable</c> is not data-bindable from XAML.
        /// The drawable is a strongly-typed object constructed here from the typed result.
        /// This method is called from <c>OnViewModelPropertyChanged</c> and the constructor.
        /// </para>
        /// </summary>
        private void ApplyBirthRankChart(MainViewModel vm)
        {
            var result = vm.HumanBirthRank;
            if (result == null || result.ChartPoints.Count == 0)
            {
                BirthRankChart.Drawable = null;
                return;
            }
            BirthRankChart.Drawable = new BirthRankChartDrawable(result);
            BirthRankChart.Invalidate();
        }

        /// <summary>
        /// Rebuilds the Web of Wyrd Explorer: populates <see cref="WyrdRuneGrid"/> with
        /// 24 rune tap-targets, sets <see cref="WyrdWebView"/> Drawable, and updates the
        /// description labels. Called when <c>BirthRuneExpanded</c>, <c>BirthRune</c>, or
        /// <c>ColorScheme</c> changes, and from the constructor for initial state.
        ///
        /// <para>
        /// <b>Why imperative:</b> <c>GraphicsView.Drawable</c> is not data-bindable.
        /// The rune grid is built from <c>FutharkCatalogue.Build()</c> so locale changes
        /// are reflected immediately when the card reopens.
        /// </para>
        /// <para>
        /// <b>Selection initialisation:</b> when the card first opens (previous catalogue
        /// is null or BirthRune changed) the selected rune is reset to the user's actual
        /// birth rune so the matrix defaults to the correct highlight.
        /// </para>
        /// </summary>
        private void ApplyWyrdWeb(MainViewModel vm)
        {
            var result = vm.BirthRune;
            if (result == null) return;

            // Rebuild the catalogue so localised strings are current.
            var catalogue = Models.FutharkCatalogue.Build();

            // Reset selection to the user's birth rune on first open or when the
            // calculated rune changes (e.g. after a base-date change).
            bool isNewOrChanged = _wyrdCatalogue == null
                               || result.RuneName != _wyrdLastRuneName;

            if (isNewOrChanged)
            {
                _wyrdSelectedIndex = Models.FutharkCatalogue.IndexOf(catalogue, result.RuneName);
                _wyrdLastRuneName  = result.RuneName;
            }

            _wyrdCatalogue = catalogue;

            // Rebuild the 24-button rune grid (always, to reflect locale + selection highlight).
            WyrdRuneGrid.Children.Clear();
            for (int i = 0; i < catalogue.Count; i++)
            {
                int capturedIndex = i;
                var rune = catalogue[i];
                bool isSelected = (i == _wyrdSelectedIndex);

                var label = new Label
                {
                    // TextType.Html forces the platform HTML renderer which has
                    // full Unicode font-fallback chains; Elder Futhark glyphs
                    // (U+16A0-U+16FF) are not covered by OpenSans or most system
                    // UI fonts but ARE available via HTML fallback on all platforms.
                    Text             = $"<span style='font-size:16px'>{rune.Symbol}</span>",
                    TextType         = TextType.Html,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment   = TextAlignment.Center,
                };

                var border = new Border
                {
                    WidthRequest      = 38,
                    HeightRequest     = 38,
                    StrokeShape       = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                    StrokeThickness   = isSelected ? 2 : 1,
                    Padding           = new Thickness(2),
                    Margin            = new Thickness(3),
                    Content           = label,
                };
                // Colours set via DynamicResource lookup at runtime so they theme-switch.
                border.SetDynamicResource(Border.BackgroundColorProperty, "CardDark");
                border.SetDynamicResource(Border.StrokeProperty,
                    isSelected ? "JubileeAccent" : "TextGray");
                label.SetDynamicResource(Label.TextColorProperty,
                    isSelected ? "JubileeAccent" : "TextDim");

                var tap = new TapGestureRecognizer();
                tap.Tapped += (_, _) => OnWyrdRuneTapped(capturedIndex);
                border.GestureRecognizers.Add(tap);

                WyrdRuneGrid.Add(border);
            }

            // Update description labels.
            var sel = catalogue[_wyrdSelectedIndex];
            WyrdRuneName.Text    = sel.Name;
            WyrdRuneMeaning.Text = sel.Brief;

            // Set GraphicsView drawable and trigger redraw.
            WyrdWebView.Drawable = new WyrdWebDrawable(catalogue, _wyrdSelectedIndex);
            WyrdWebView.Invalidate();
        }

        /// <summary>
        /// Handles a tap on one of the 24 rune buttons in the Web of Wyrd Explorer.
        /// Updates the selected rune index, refreshes button highlight states,
        /// updates the description labels, and triggers a canvas redraw.
        /// No ViewModel mutation - selection is pure UI state.
        /// </summary>
        /// <param name="runeIndex">0-based index into the current <c>_wyrdCatalogue</c>.</param>
        private void OnWyrdRuneTapped(int runeIndex)
        {
            if (_wyrdCatalogue == null || runeIndex < 0 || runeIndex >= _wyrdCatalogue.Count)
                return;

            _wyrdSelectedIndex = runeIndex;

            // Re-apply highlight states on all grid children.
            var children = WyrdRuneGrid.Children;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is not Border b) continue;
                bool sel = (i == _wyrdSelectedIndex);
                b.StrokeThickness = sel ? 2 : 1;
                b.SetDynamicResource(Border.StrokeProperty, sel ? "JubileeAccent" : "TextGray");
                if (b.Content is Label lbl)
                    lbl.SetDynamicResource(Label.TextColorProperty, sel ? "JubileeAccent" : "TextDim");
            }

            // Update description.
            var rune = _wyrdCatalogue[_wyrdSelectedIndex];
            WyrdRuneName.Text    = rune.Name;
            WyrdRuneMeaning.Text = rune.Brief;

            // Redraw canvas.
            WyrdWebView.Drawable = new WyrdWebDrawable(_wyrdCatalogue, _wyrdSelectedIndex);
            WyrdWebView.Invalidate();
        }

        /// <summary>
        /// Sets <see cref="EnneagramView"/>.Drawable to a fresh <see cref="EnneagramDrawable"/>
        /// and calls <c>Invalidate()</c>. Called when <c>PersonalYearExpanded</c>,
        /// <c>PersonalYear</c>, or <c>ColorScheme</c> changes, and from the constructor.
        ///
        /// <para>
        /// <b>Why imperative:</b> <c>GraphicsView.Drawable</c> is not data-bindable.
        /// The drawable is recreated on every relevant property change so it always
        /// reflects the current personal year number and active colour scheme.
        /// </para>
        /// </summary>
        private void ApplyEnneagram(MainViewModel vm)
        {
            EnneagramView.Drawable = new EnneagramDrawable(vm.PersonalYear?.PersonalYearNumber ?? 1);
            EnneagramView.Invalidate();
        }

        // Tracks the drawable instance so the interaction handlers can update ScrubX without
        // rebuilding the entire chart on every touch event.
        private PopulationChartDrawable? _populationChartDrawable;

        /// <summary>
        /// Creates a fresh <see cref="PopulationChartDrawable"/> from the current
        /// <see cref="GlobalCrowdResult"/>, assigns it to <see cref="PopulationChartView"/>
        /// and invalidates. Called on <c>GlobalCrowdExpanded</c>, <c>GlobalCrowd</c>,
        /// and <c>ColorScheme</c> changes and from the constructor.
        /// </summary>
        private void ApplyPopulationChart(MainViewModel vm)
        {
            var result = vm.GlobalCrowd;
            if (result == null) return;

            double basePopBillions    = result.BasePopulation    / 1_000_000_000.0;
            double currentPopBillions = result.CurrentPopulation / 1_000_000_000.0;

            _populationChartDrawable = new PopulationChartDrawable
            {
                BaseYear           = result.BaseYear,
                BasePopBillions    = basePopBillions,
                CurrentYear        = result.CurrentYear,
                CurrentPopBillions = currentPopBillions,
                ScrubX             = -1f,
            };

            // Initialise hover labels to current year/population.
            result.HoverYear       = result.CurrentYear;
            result.HoverPopulation = currentPopBillions;

            PopulationChartView.Drawable = _populationChartDrawable;
            PopulationChartView.Invalidate();
        }

        /// <summary>
        /// Handles <c>StartInteraction</c> and <c>DragInteraction</c> on
        /// <see cref="PopulationChartView"/>. Updates the scrubber X position,
        /// reverse-interpolates year and population from the touch X coordinate,
        /// and writes to <c>GlobalCrowd.HoverYear</c>/<c>HoverPopulation</c>
        /// so the bound labels update in real time.
        /// </summary>
        private void OnPopulationChartInteraction(object sender, TouchEventArgs e)
        {
            if (_populationChartDrawable == null) return;
            if (e.Touches == null || e.Touches.Length == 0) return;
            if (BindingContext is not MainViewModel vm || vm.GlobalCrowd == null) return;

            float touchX = e.Touches[0].X;
            float chartW = (float)PopulationChartView.Width;
            if (chartW <= 0) return;

            // Clamp to the drawable's chart area (matches PadLeft / PadRight in drawable).
            float chartLeft  = 36f;
            float chartRight = chartW - 10f;
            float clampedX   = Math.Clamp(touchX, chartLeft, chartRight);

            _populationChartDrawable.ScrubX = clampedX;

            // Reverse-map X pixel -> year -> population.
            double scrubYear = 1950.0 + (clampedX - chartLeft) / (chartRight - chartLeft) * (2050.0 - 1950.0);
            double scrubPop  = PopulationChartDrawable.InterpolatePopulation(scrubYear);

            vm.GlobalCrowd.HoverYear       = scrubYear;
            vm.GlobalCrowd.HoverPopulation = scrubPop;

            PopulationChartView.Invalidate();
        }

        /// <summary>
        /// Handles <c>EndInteraction</c> on <see cref="PopulationChartView"/>.
        /// Snaps the scrubber back to the current year so the display returns
        /// to the default state when the user lifts their finger.
        /// </summary>
        private void OnPopulationChartEndInteraction(object sender, TouchEventArgs e)
        {
            if (_populationChartDrawable == null) return;
            if (BindingContext is not MainViewModel vm || vm.GlobalCrowd == null) return;

            float chartW    = (float)PopulationChartView.Width;
            float chartLeft = 36f;
            float chartRight = chartW - 10f;

            double currentYear   = vm.GlobalCrowd.CurrentYear;
            double currentPopBil = vm.GlobalCrowd.CurrentPopulation / 1_000_000_000.0;

            float snappedX = chartLeft + (float)((currentYear - 1950.0) / (2050.0 - 1950.0)) * (chartRight - chartLeft);
            _populationChartDrawable.ScrubX = -1f;

            vm.GlobalCrowd.HoverYear       = currentYear;
            vm.GlobalCrowd.HoverPopulation = currentPopBil;

            PopulationChartView.Invalidate();
        }

        /// <summary>
        /// Creates a <see cref="LifeLogChartDrawable"/> from <c>LifeLog.ActivitySlices</c>,
        /// assigns it to <see cref="LifeLogChartView"/>, rebuilds the legend rows in
        /// <see cref="LifeLogLegend"/>, and calls <c>Invalidate()</c>.
        ///
        /// <para>
        /// Called on <c>LifeLogExpanded</c> and <c>LifeLog</c> property changes and
        /// from the constructor so the chart is always ready before the card opens.
        /// </para>
        /// <para>
        /// <b>Why imperative legend:</b> <see cref="LifeLogSlice"/> is a plain sealed
        /// class with no INPC; its <c>Color</c> property is a MAUI <see cref="Color"/>
        /// that cannot bind to <c>BoxView.Color</c> via <c>DynamicResource</c>.
        /// Building the rows in code gives full control over colours and formatting
        /// without requiring a custom converter or a ViewModel wrapper.
        /// </para>
        /// </summary>
        private void ApplyLifeLogChart(MainViewModel vm)
        {
            var result = vm.LifeLog;
            if (result?.ActivitySlices == null || result.ActivitySlices.Count == 0)
                return;

            LifeLogChartView.Drawable = new LifeLogChartDrawable(result.ActivitySlices);
            LifeLogChartView.Invalidate();

            // Rebuild legend rows - clear and repopulate.
            LifeLogLegend.Children.Clear();
            foreach (var slice in result.ActivitySlices)
            {
                var swatch = new BoxView
                {
                    Color         = Color.FromArgb(slice.ColorHex),
                    WidthRequest  = 12,
                    HeightRequest = 12,
                    CornerRadius  = 6,
                    VerticalOptions = LayoutOptions.Center,
                };

                var nameLabel = new Label
                {
                    Text           = slice.CategoryName,
                    FontAttributes = FontAttributes.Bold,
                    FontSize       = 12,
                    VerticalOptions = LayoutOptions.Center,
                };
                nameLabel.SetDynamicResource(Label.TextColorProperty, "TextDim");

                var todayLabel = new Label
                {
                    Text          = $"{slice.YearsToday:F1}y",
                    FontSize      = 11,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalOptions = LayoutOptions.Center,
                };
                todayLabel.SetDynamicResource(Label.TextColorProperty, "TextGray");

                var forecastLabel = new Label
                {
                    Text          = $"+10y: {slice.YearsForecast:F1}y",
                    FontSize      = 11,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalOptions = LayoutOptions.Center,
                };
                forecastLabel.SetDynamicResource(Label.TextColorProperty, "TextGray");

                var row = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = new GridLength(18) },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto },
                    },
                    ColumnSpacing = 8,
                };
                row.Add(swatch,        0, 0);
                row.Add(nameLabel,     1, 0);
                row.Add(todayLabel,    2, 0);
                row.Add(forecastLabel, 3, 0);

                LifeLogLegend.Children.Add(row);
            }
        }

        /// <summary>
        /// Populates the Volumetric Cube visualizer inside <see cref="VolumeCubeContainer"/>.
        /// Builds a three-part <see cref="FormattedString"/> description label (prefix / bold
        /// dimension / suffix), creates a fresh <see cref="VolumeCubeDrawable"/>, and calls
        /// <c>Invalidate()</c> so the isometric cube redraws at the current scale.
        ///
        /// <para>Called on <c>YourBreathExpanded</c>, <c>YourBreath</c>,
        /// <c>ColorScheme</c>, and <c>DisplayLanguage</c> changes and from the
        /// constructor. The container remains visible whenever <c>YourBreathExpanded</c> is true.</para>
        /// </summary>
        /// <summary>Landmark entry: size in metres, image filename, localised description accessor.</summary>
        private static readonly (double SizeM, string File, Func<string> Desc)[] _landmarks =
        {
            (1.7,  "01.7_human.png",              () => AppResources.Chart_YourBreath_LM_Human),
            (4.4,  "04.4_double-decker-bus.png",  () => AppResources.Chart_YourBreath_LM_Bus),
            (7.0,  "07_Stonehenge.png",            () => AppResources.Chart_YourBreath_LM_Stonehenge),
            (10.0, "10_moai-statues.png",          () => AppResources.Chart_YourBreath_LM_Moai),
            (14.0, "14_hollywood-sign.png",        () => AppResources.Chart_YourBreath_LM_Hollywood),
            (15.0, "15_parthenon.png",             () => AppResources.Chart_YourBreath_LM_Parthenon),
            (16.5, "16.5_itsukushima-shrine.png",  () => AppResources.Chart_YourBreath_LM_Itsukushima),
            (20.0, "20_great-sphinx-of-giza.png",  () => AppResources.Chart_YourBreath_LM_Sphinx),
            (21.0, "21_white-house.png",           () => AppResources.Chart_YourBreath_LM_WhiteHouse),
            (26.0, "26_brandenburg-gate.png",      () => AppResources.Chart_YourBreath_LM_Brandenburg),
            (30.0, "30_blue-whale.png",            () => AppResources.Chart_YourBreath_LM_Whale),
            (38.0, "38_christ-the-redeemer.png",   () => AppResources.Chart_YourBreath_LM_Christ),
            (46.0, "46_statue-of-liberty.png",     () => AppResources.Chart_YourBreath_LM_Liberty),
            (48.0, "48_colosseum.png",             () => AppResources.Chart_YourBreath_LM_Colosseum),
            (50.0, "50_arc-de-triomphe.png",       () => AppResources.Chart_YourBreath_LM_Arc),
            (53.0, "53_ruiguang-tower.png",        () => AppResources.Chart_YourBreath_LM_Ruiguang),
            (56.0, "56_tower-of-pisa.png",         () => AppResources.Chart_YourBreath_LM_Pisa),
            (61.0, "61_egyptian-pyramids-icon.png",() => AppResources.Chart_YourBreath_LM_Menkaure),
            (65.0, "65_tower-bridge.png",          () => AppResources.Chart_YourBreath_LM_TowerBridge),
            (69.0, "69_notre-dame.png",            () => AppResources.Chart_YourBreath_LM_NotreDame),
            (73.0, "73_taj-mahal.png",             () => AppResources.Chart_YourBreath_LM_TajMahal),
        };

        private async Task ApplyVolumeCubeAsync(MainViewModel vm)
        {
            var result = vm.YourBreath;
            bool show = vm.YourBreathExpanded;
            if (!show || result == null) return;

#if DEBUG
            AeonLog.Debug("VM", "ApplyVolumeCube",
                $"edge={result.CubeEdgeMeters:F3}  useMetric={vm.UseMetric}");
#endif

            double edge = Math.Max(result.CubeEdgeMeters, 0.001);
            bool useMetric = vm.UseMetric;
            const double MtoFt = 3.28084;

            // ---- Select landmark: largest whose SizeM < cube edge (fallback to Human). ----
            var lm = _landmarks[0];
            foreach (var entry in _landmarks)
            {
                if (entry.SizeM < edge) lm = entry;
                else break;
            }

            // ---- Cube drawable. ----
            var drawable = new VolumeCubeDrawable { CubeEdgeMeters = edge };
            VolumeCubeView.Drawable = drawable;
            VolumeCubeView.Invalidate();

            // ---- Compute ppm using the same deterministic formula as VolumeCubeDrawable.Draw
            // so we never depend on Draw() having been called (Invalidate is async on all platforms).
            float canvasH = (float)(VolumeCubeView.Height > 0 ? VolumeCubeView.Height : 250.0);
            float ppm = (float)Math.Clamp((canvasH * 0.80) / (edge * 2.0), 3.0, 280.0);

            // ---- Landmark image: height = LandmarkSizeM * ppm. ----
            double imageH = Math.Max(4.0, lm.SizeM * ppm);
            // Landmark PNGs are <MauiAsset> files. The correct ImageSource type differs
            // per platform: Windows needs FileImageSource (so WinUI uses BitmapImage which
            // fires ImageOpened, letting AttachAndTint retint after every decode); Android
            // needs StreamImageSource via OpenAppPackageFileAsync (AssetManager path).
            // MauiProgram.LandmarkImageSource encapsulates the per-platform choice.
            string fileName = lm.File;
            LandmarkImage.Source = MauiProgram.LandmarkImageSource(fileName);
            LandmarkImage.HeightRequest = imageH;
            LandmarkImage.Margin = new Thickness(8, 0, 0, 4);

            // ---- Cube description label (right): prefix + bold edge + suffix. ----
            double displayEdge = useMetric ? edge : edge * MtoFt;
            string unitFmt     = useMetric ? AppResources.Chart_YourBreath_CubeM
                                           : AppResources.Chart_YourBreath_CubeFt;
            var fs      = new FormattedString();
            var spanPre = new Span { Text = AppResources.Chart_YourBreath_CubePrefix };
            spanPre.SetDynamicResource(Span.TextColorProperty, "TextGray");
            var spanNum = new Span
            {
                Text           = string.Format(unitFmt, displayEdge),
                FontAttributes = FontAttributes.Bold,
            };
            spanNum.SetDynamicResource(Span.TextColorProperty, "CyberCyan");
            var spanSuf = new Span { Text = AppResources.Chart_YourBreath_CubeSuffix };
            spanSuf.SetDynamicResource(Span.TextColorProperty, "TextGray");
            fs.Spans.Add(spanPre);
            fs.Spans.Add(spanNum);
            fs.Spans.Add(spanSuf);
            VolumeCubeDescLabel.FormattedText = fs;

            // ---- Landmark label (left): "Description: size unit". ----
            double lmDisplay = useMetric ? lm.SizeM : lm.SizeM * MtoFt;
            string lmUnit    = useMetric ? AppResources.Chart_YourBreath_CubeM
                                         : AppResources.Chart_YourBreath_CubeFt;
            string lmSizeStr = string.Format(lmUnit, lmDisplay);
            var lmFs      = new FormattedString();
            var lmSpanDesc = new Span { Text = $"{lm.Desc()}: " };
            lmSpanDesc.SetDynamicResource(Span.TextColorProperty, "TextGray");
            var lmSpanSize = new Span { Text = lmSizeStr, FontAttributes = FontAttributes.Bold };
            lmSpanSize.SetDynamicResource(Span.TextColorProperty, "CyberCyan");
            lmFs.Spans.Add(lmSpanDesc);
            lmFs.Spans.Add(lmSpanSize);
            LandmarkLabel.FormattedText = lmFs;
        }

        /// <summary>
        /// Populates the carbon budget chart elements inside
        /// <see cref="CarbonBudgetChartContainer"/>: sets localized text on the title
        /// and depletion labels, assigns a fresh <see cref="CarbonBudgetChartDrawable"/>
        /// to <see cref="CarbonBudgetChartView"/>, and sets theme-aware colours on
        /// the axis labels via <c>SetDynamicResource</c>.
        ///
        /// <para>Called on <c>GlobalExhaleExpanded</c>, <c>GlobalExhale</c>, and
        /// <c>DisplayLanguage</c> changes and from the constructor.
        /// Hidden when the depletion year is unavailable.</para>
        /// </summary>
        private void ApplyCarbonBudgetChart(MainViewModel vm)
        {
            var result = vm.GlobalExhale;
            bool show = result != null && result.DepletionYear > 0;
            CarbonBudgetChartContainer.IsVisible = show && vm.GlobalExhaleExpanded;
            if (!show) return;

            // Title label.
            CarbonBudgetTitleLabel.Text = AppResources.Chart_GlobalExhale_BudgetTitle;
            CarbonBudgetTitleLabel.SetDynamicResource(Label.TextColorProperty, "TextDim");

            // Depletion label.
            int depYear = (int)Math.Round(result!.DepletionYear);
            CarbonBudgetDepletionLabel.Text = string.Format(
                AppResources.Chart_GlobalExhale_Depletion, depYear);
            CarbonBudgetDepletionLabel.SetDynamicResource(Label.TextColorProperty, "CyberPink");

            // Axis labels.
            CarbonChartLabelBase.Text = vm.BaseDateName;
            CarbonChartLabelBase.SetDynamicResource(Label.TextColorProperty, "TextGray");
            CarbonChartLabelToday.Text = AppResources.Chart_GlobalExhale_Today;
            CarbonChartLabelToday.SetDynamicResource(Label.TextColorProperty, "CyberCyan");
            CarbonChartLabelLimit.Text = AppResources.Chart_GlobalExhale_Limit;
            CarbonChartLabelLimit.SetDynamicResource(Label.TextColorProperty, "CyberPink");

            // Chart drawable.
            double todayYear = DateTime.Now.Year + DateTime.Now.DayOfYear / 365.25;
            CarbonBudgetChartView.Drawable = new CarbonBudgetChartDrawable
            {
                ChartStartYear = result.ChartStartYear,
                DepletionYear  = result.DepletionYear,
                TotalBudgetGt  = result.TotalBudgetGt,
                BaseDateCumGt  = result.BaseDateCumCO2Gt,
                TodayCumGt     = result.TodayCumCO2Gt,
                BaseYear       = vm.BaseDate.Year + vm.BaseDate.DayOfYear / 365.25,
                TodayYear      = todayYear,
            };
            CarbonBudgetChartView.Invalidate();
        }

        /// <summary>
        /// Builds the two proportional balance bars (Arrivals / Departures) inside
        /// <see cref="VibrantHumanityBarsContainer"/> from the current
        /// <see cref="VibrantHumanityResult"/> raw counts.
        ///
        /// <para>
        /// <b>Layout strategy:</b> the Arrivals bar always fills the full container width
        /// (<c>HorizontalOptions=Fill</c>). The Departures bar width is set in the
        /// <c>SizeChanged</c> handler on the container to
        /// <c>containerWidth * (deaths / births)</c>, making it physically shorter when
        /// population is growing. Each bar is a <c>Grid</c> whose column widths are
        /// proportional <c>GridLength.Star</c> values derived from the raw counts, so
        /// the coloured segments fill the bar precisely.
        /// </para>
        /// <para>
        /// <b>MinimumWidthRequest = 2</b> is applied to every segment column's
        /// <c>BoxView</c> to prevent the Twins sliver (~2.4% of births) from
        /// collapsing to zero and causing a layout exception.
        /// </para>
        /// </summary>
        private void ApplyVibrantHumanityBars(MainViewModel vm)
        {
            var result = vm.VibrantHumanity;
            VibrantHumanityBarsContainer.Children.Clear();

            if (result == null || result.BornBetweenDates <= 0) return;

            double births      = result.BornBetweenDates;
            double deaths      = result.DiedBetweenDates;
            double twins       = result.TwinsBorn * 2;           // TwinsBorn = pairs; each pair = 2 births
            double singletons  = births - twins;
            double heart       = result.HeartDeaths;
            double cancer      = result.CancerDeaths;
            double otherDeaths = Math.Max(0, deaths - heart - cancer);

            // Clamp twins sliver to at least a visible minimum proportion.
            double twinsStarVal      = Math.Max(twins / births, 0.001);
            double singletonsStarVal = 1.0 - twinsStarVal;

            double heartStarVal  = deaths > 0 ? heart       / deaths : 0.333;
            double cancerStarVal = deaths > 0 ? cancer      / deaths : 0.333;
            double otherStarVal  = deaths > 0 ? otherDeaths / deaths : 0.334;

            // Bar segment and swatch colours are pinned to DefaultDark palette values
            // so they remain visually consistent across all colour schemes.
            // Text colours are handled separately via SetDynamicResource.
            Color colSingletons = Color.FromArgb("#50FA7B"); // NeonGreen DefaultDark
            Color colTwins      = Color.FromArgb("#FFD700"); // JubileeAccent DefaultDark
            Color colHeart      = Color.FromArgb("#FF79C6"); // CyberPink DefaultDark
            Color colCancer     = Color.FromArgb("#BD93F9"); // CyberPurple DefaultDark
            Color colOther      = Color.FromArgb("#B0B0B0"); // TextGray DefaultDark

            // --- Arrivals bar ---
            var arrivalsLabel = new Label
            {
                Text           = AppResources.Chart_VibrantHumanity_Arrivals,
                FontAttributes = FontAttributes.Bold,
                FontSize       = 13,
            };
            arrivalsLabel.SetDynamicResource(Label.TextColorProperty, "TextDim");

            var arrivalsBar = BuildBar(new[]
            {
                (singletonsStarVal, colSingletons),
                (twinsStarVal,      colTwins),
            });
            arrivalsBar.HorizontalOptions = LayoutOptions.Fill;

            var arrivalsLegend = BuildLegend(new[]
            {
                (colSingletons, AppResources.Chart_VibrantHumanity_Singletons, $"{singletons:N0}"),
                (colTwins,      AppResources.Chart_VibrantHumanity_Twins,      $"{twins:N0}"),
            }, Colors.Transparent);

            var arrivalsStack = new VerticalStackLayout { Spacing = 5 };
            arrivalsStack.Children.Add(arrivalsLabel);
            arrivalsStack.Children.Add(arrivalsBar);
            arrivalsStack.Children.Add(arrivalsLegend);

            // --- Departures bar ---
            var departuresLabel = new Label
            {
                Text           = AppResources.Chart_VibrantHumanity_Departures,
                FontAttributes = FontAttributes.Bold,
                FontSize       = 13,
            };
            departuresLabel.SetDynamicResource(Label.TextColorProperty, "TextDim");

            var departuresBar = BuildBar(new[]
            {
                (heartStarVal,  colHeart),
                (cancerStarVal, colCancer),
                (otherStarVal,  colOther),
            });
            // Width is set proportionally in the SizeChanged handler below.
            departuresBar.HorizontalOptions = LayoutOptions.Start;
            departuresBar.MinimumWidthRequest = 4;

            var departuresLegend = BuildLegend(new[]
            {
                (colHeart,  AppResources.Chart_VibrantHumanity_Heart,  $"{heart:N0}"),
                (colCancer, AppResources.Chart_VibrantHumanity_Cancer, $"{cancer:N0}"),
                (colOther,  AppResources.Chart_VibrantHumanity_Other,  $"{otherDeaths:N0}"),
            }, Colors.Transparent);

            var departuresStack = new VerticalStackLayout { Spacing = 5 };
            departuresStack.Children.Add(departuresLabel);
            departuresStack.Children.Add(departuresBar);
            departuresStack.Children.Add(departuresLegend);

            VibrantHumanityBarsContainer.Children.Add(arrivalsStack);
            VibrantHumanityBarsContainer.Children.Add(departuresStack);

            // Scale Departures bar width proportionally once the container is measured.
            double ratio = births > 0 ? Math.Min(deaths / births, 1.0) : 1.0;
            void UpdateDeparturesWidth(object? s, EventArgs e)
            {
                double w = VibrantHumanityBarsContainer.Width;
                if (w > 0)
                    departuresBar.WidthRequest = w * ratio;
            }
            VibrantHumanityBarsContainer.SizeChanged += UpdateDeparturesWidth;
            // Also apply immediately if the container already has a measured width.
            if (VibrantHumanityBarsContainer.Width > 0)
                departuresBar.WidthRequest = VibrantHumanityBarsContainer.Width * ratio;
        }

        /// <summary>
        /// Builds a single horizontal stacked bar as a rounded <see cref="Border"/>
        /// containing a <see cref="Grid"/> whose columns are proportional star widths.
        /// </summary>
        private static Border BuildBar((double StarVal, Color Color)[] segments)
        {
            var grid = new Grid { HeightRequest =  22 };
            foreach (var seg in segments)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(Math.Max(seg.StarVal, 0.001), GridUnitType.Star),
                });
            }
            for ( int i = 0; i < segments.Length; i++)
            {
                var bv = new BoxView
                {
                    Color               = segments[i].Color,
                    MinimumWidthRequest = 2,
                };
                Grid.SetColumn(bv, i);
                grid.Children.Add(bv);
            }
            return new Border
            {
                StrokeThickness = 0,
                StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                Content         = grid,

            };
        }

        /// <summary>
        /// Builds a compact horizontal legend row from (colour, label, count) tuples.
        /// </summary>
        private static FlexLayout BuildLegend((Color Color, string Label, string Count)[] items, Color textColor)
        {
            var flex = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start };
            foreach (var item in items)
            {
                var swatch = new BoxView
                {
                    Color           = item.Color,
                    WidthRequest    = 10,
                    HeightRequest   = 10,
                    CornerRadius    = 5,
                    VerticalOptions = LayoutOptions.Center,
                };
                var lbl = new Label
                {
                    Text            = $"{item.Label}: {item.Count}",
                    FontSize        = 11,
                    VerticalOptions = LayoutOptions.Center,
                };
                lbl.SetDynamicResource(Label.TextColorProperty, "TextGray");
                var entry = new HorizontalStackLayout
                {
                    Spacing = 5,
                    Margin  = new Thickness(0, 0, 12, 2),
                };
                entry.Children.Add(swatch);
                entry.Children.Add(lbl);
                flex.Children.Add(entry);
            }
            return flex;
        }

        /// <summary>Reads a named colour from the application resource dictionary at call time.</summary>
        private static Color GetDynColor(String key, Color fallback)
        {
            if (Application.Current?.Resources.TryGetValue(key, out var raw) == true && raw is Color c)
                return c;
            return fallback;
        }

        /// <summary>
        /// Starts the Ambient Sparks loop on <see cref="CosmosCanvas"/> if not already running.
        /// Creates a fresh <see cref="CancellationTokenSource"/> and fires
        /// <see cref="SpawnAmbientSparks"/> as a detached <c>Task</c> on the UI thread.
        ///
        /// <para>
        /// <b>Design note:</b> presentational-only animation with no domain logic.
        /// Guard flag <c>_sparksRunning</c> prevents double-start when both
        /// <c>OnAppearing</c> and <c>PropertyChanged(VibrantCosmosExpanded)</c> fire.
        /// </para>
        /// </summary>
        private void StartAmbientSparks()
        {
            if (_sparksRunning) return;
            _sparksRunning = true;
            _sparksCts?.Cancel();
            _sparksCts = new CancellationTokenSource();
            var token = _sparksCts.Token;
            // Fire-and-forget: detached Task, cancelled via CTS. All UI ops are on
            // the main thread already (this method is only ever called from UI thread
            // event handlers and OnAppearing which run on the main thread).
            _ = SpawnAmbientSparks(token);
        }

        /// <summary>
        /// Cancels the running Ambient Sparks loop and resets the guard flag.
        /// Existing in-flight particle animations complete naturally but no new
        /// particles are spawned. Called from <c>OnDisappearing</c> and when
        /// <c>VibrantCosmosExpanded</c> becomes <c>false</c>.
        /// </summary>
        private void StopAmbientSparks()
        {
            _sparksRunning = false;
            _sparksCts?.Cancel();
            _sparksCts = null;
        }

        /// <summary>
        /// The inner spawner loop: fires a new particle every 200-600 ms until
        /// <paramref name="token"/> is cancelled. Each particle runs its own independent
        /// lifecycle <c>Task</c> so multiple stars are visible simultaneously.
        ///
        /// <para>
        /// <b>Spawn ratio:</b> 90% star-birth (U+2736), 10% supernova. When a supernova
        /// fires, it hijacks a randomly chosen live star <c>Label</c> from
        /// <c>_liveStars</c> (if any exist) instead of spawning at a new position.
        /// The hijacked star skips its dwell/fade-out and immediately plays the
        /// supernova sequence: U+2739 swell, U+1F4A5 flash, then dissipate.
        /// </para>
        /// <para>
        /// <b>Colour:</b> both U+2736 and U+2739 use the current value of the
        /// <c>JubileeAccent</c> resource key (same gold/white/black as the Photon Path
        /// Sun dot and the orrery Sun) read at spawn time, so the colour follows the
        /// active colour scheme.
        /// </para>
        /// <para>
        /// <b>Memory management:</b> star-birth labels are tracked in <c>_liveStars</c>
        /// while alive and removed from both the list and <c>CosmosCanvas.Children</c>
        /// when their lifecycle ends. Supernova labels are removed on dissipation.
        /// </para>
        /// </summary>
        /// <param name="token">Cancellation token; the loop exits cleanly when cancelled.</param>
        private async Task SpawnAmbientSparks(CancellationToken token)
        {
            // Glyph constants - not in comments per ASCII-only rule.
            const string StarBirth     = "\u2736";       // six-pointed black star - text-only glyph, TextColor applies correctly (U+2734 has Emoji_Presentation and ignores TextColor)
            const string SupernovaGrow = "\u2739";       // twelve-pointed black star
            const string SupernovaBoom = "\U0001F4A5";   // collision symbol emoji

            while (!token.IsCancellationRequested)
            {
                int delayMs = _rng.Next(200, 601);
                try { await Task.Delay(delayMs, token); }
                catch (TaskCanceledException) { return; }
                if (token.IsCancellationRequested) return;

                // Read JubileeAccent from the live resource dictionary so the colour
                // follows the active colour scheme (gold/white/black per ThemeService).
                var resources = Application.Current?.Resources;
                var accentColor = (resources != null && resources.TryGetValue("JubileeAccent", out var raw) && raw is Microsoft.Maui.Graphics.Color c)
                    ? c
                    : Microsoft.Maui.Graphics.Color.FromArgb("#FFD700");

                bool isSupernova = _rng.NextDouble() < 0.10;

                if (isSupernova)
                {
                    // Supernova: hijack a live star if one exists, otherwise skip this tick.
                    Label? victim = null;
                    lock (_liveStars)
                    {
                        if (_liveStars.Count > 0)
                        {
                            int idx = _rng.Next(_liveStars.Count);
                            victim = _liveStars[idx];
                            _liveStars.RemoveAt(idx);
                        }
                    }
                    if (victim != null)
                        _ = RunSupernovaOnLabel(victim, SupernovaGrow, SupernovaBoom, accentColor, token);
                    // If no live stars yet, silently skip this supernova tick.
                }
                else
                {
                    // Star birth: spawn at a random proportional position.
                    double xFrac = _rng.NextDouble();
                    double yFrac = _rng.NextDouble();
                    int dwellMs  = _rng.Next(1000, 10001);

                    var star = new Label
                    {
                        Text             = StarBirth,
                        FontSize         = 14,
                        TextColor        = accentColor,
                        Opacity          = 0,
                        InputTransparent = true,
                    };

                    AbsoluteLayout.SetLayoutBounds(star, new Rect(xFrac, yFrac, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                    AbsoluteLayout.SetLayoutFlags(star, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.PositionProportional);
                    CosmosCanvas.Children.Add(star);
                    lock (_liveStars) { _liveStars.Add(star); }

                    _ = RunStarLifecycle(star, dwellMs, token);
                }
            }
        }

        /// <summary>
        /// Runs the full lifecycle of a star-birth particle: fade in, dwell for
        /// <paramref name="dwellMs"/> milliseconds, then fade out and remove.
        /// If the label has already been removed from <c>_liveStars</c> by a supernova
        /// hijack, this task still holds a reference to the label; the supernova task
        /// owns the label at that point and will remove it from the canvas.
        /// The <c>_liveStars</c> lock ensures only one task acts on a given label.
        /// </summary>
        private async Task RunStarLifecycle(Label star, int dwellMs, CancellationToken token)
        {
            // Fade in.
            await star.FadeTo(0.7, 800, Easing.SinIn);

            // Dwell - random 1-10 s. Check whether we were hijacked before sleeping.
            bool stillAlive;
            lock (_liveStars) { stillAlive = _liveStars.Contains(star); }
            if (!stillAlive) return; // supernova hijack already took this label

            try { await Task.Delay(dwellMs, token); }
            catch (TaskCanceledException)
            {
                CosmosCanvas.Children.Remove(star);
                return;
            }

            // Re-check after dwell: supernova may have hijacked us during the dwell.
            lock (_liveStars)
            {
                if (!_liveStars.Remove(star))
                    return; // hijacked - supernova task owns the label now
            }

            // Fade out.
            if (!token.IsCancellationRequested)
                await star.FadeTo(0, 700, Easing.SinOut);
            CosmosCanvas.Children.Remove(star);
        }

        /// <summary>
        /// Runs the supernova sequence on a hijacked star <see cref="Label"/>:
        /// swaps glyph to U+2739, swells with scale-up + fade to near-full opacity,
        /// then swaps to U+1F4A5 for a brief flash, then dissipates (scale down + fade
        /// out). Removes the label from <c>CosmosCanvas.Children</c> when done.
        /// </summary>
        private async Task RunSupernovaOnLabel(Label victim, string supernovaGrow, string supernovaBoom,
                                               Microsoft.Maui.Graphics.Color accentColor, CancellationToken token)
        {
            // Swap to supernova glyph immediately - no fade-out of the star first.
            victim.Text      = supernovaGrow;
            victim.FontSize  = 20;
            victim.TextColor = accentColor;
            victim.Scale     = 0.6;

            // Phase 1: swell.
            var fadeIn  = victim.FadeTo(0.95, 600, Easing.CubicIn);
            var scaleUp = victim.ScaleTo(1.7, 800, Easing.CubicOut);
            await Task.WhenAll(fadeIn, scaleUp);

            if (token.IsCancellationRequested)
            {
                CosmosCanvas.Children.Remove(victim);
                return;
            }

            // Phase 2: collision flash - swap glyph, hold briefly.
            victim.Text     = supernovaBoom;
            victim.FontSize = 26;
            victim.Scale    = 1.0;
            try { await Task.Delay(200, token); }
            catch (TaskCanceledException)
            {
                CosmosCanvas.Children.Remove(victim);
                return;
            }

            // Phase 3: dissipate.
            var fadeOut   = victim.FadeTo(0, 500, Easing.CubicOut);
            var scaleDown = victim.ScaleTo(0.3, 500, Easing.CubicIn);
            await Task.WhenAll(fadeOut, scaleDown);

            CosmosCanvas.Children.Remove(victim);
        }

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
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            if (Window is not null)
                Window.Destroying += OnWindowDestroying;
        }

        private void OnWindowDestroying(object? sender, EventArgs e)
        {
            if (BindingContext is MainViewModel vm)
                vm.StopTimers();
        }

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartLiveBadgeAnimation();
            if (BindingContext is MainViewModel vm2 && vm2.VibrantCosmosExpanded)
                StartAmbientSparks();
#if DEBUG
            MemSnapshot.Emit("MAIN_READY");
            // Fire one-shot T+30 s and T+120 s snapshots on a background task.
            // Captured on the thread pool; AeonLog is thread-safe.
            _ = Task.Run(async () =>
            {
                await Task.Delay(30_000);
                MemSnapshot.Emit("T30");
                await Task.Delay(90_000);   // 90 s more = 120 s from MAIN_READY
                MemSnapshot.Emit("T120");
            });
#endif
        }



        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.AbortAnimation(LiveBadgeAnimationName);
            StopAmbientSparks();
        }
    }
}
