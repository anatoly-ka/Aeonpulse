using Aeonpulse.Models;
using Aeonpulse.Resources;
using Aeonpulse.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Aeonpulse.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly CalculationService _calculationService;
        private IDispatcherTimer _updateTimer;
        private IDispatcherTimer _vibrantCosmosTimer;
        private const string LogCat = "VM";

        #region Language constants

        public const string LangDefault = "Default";
        public const string LangEnglish = "English";
        public const string LangRussian = "Russian";

        /// <summary>
        /// Applies a display-language choice by setting the thread cultures and
        /// AppResources.Culture. "Default" restores the original OS culture.
        /// Static so it can be called from App.xaml.cs before the VM is created.
        /// </summary>
        public static void ApplyLanguage(string language)
        {
            System.Globalization.CultureInfo culture = language switch
            {
                LangEnglish => new System.Globalization.CultureInfo("en"),
                LangRussian => new System.Globalization.CultureInfo("ru"),
                _           => System.Globalization.CultureInfo.InstalledUICulture
            };

            System.Globalization.CultureInfo.DefaultThreadCurrentCulture   = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
            AppResources.Culture = culture;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Live-bindable wrapper for all AppResources strings.
        /// Call <see cref="LocalizedResources.Invalidate"/> after a language change
        /// to push every new string to the UI at once.
        /// </summary>
        public LocalizedResources Loc { get; } = LocalizedResources.Instance;

        private string _baseDateName = AppResources.Default_BaseDateName;
        public string BaseDateName
        {
            get => _baseDateName;
            set { _baseDateName = value; OnPropertyChanged(); }
        }

        private string _baseDateValue = "1965-07-24";
        public string BaseDateValue
        {
            get => _baseDateValue;
            set { _baseDateValue = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Culture-aware short date string for display only.
        /// Formatted using the current UI culture so it respects the active locale.
        /// </summary>
        public string BaseDateDisplay =>
            _baseDate.ToString("d", System.Globalization.CultureInfo.CurrentUICulture);

        private DateTime _baseDate = new DateTime(1965, 7, 24);
        public DateTime BaseDate
        {
            get => _baseDate;
            set { _baseDate = value; OnPropertyChanged(); UpdateAllCalculations(); }
        }

        private bool _useMetric = true;
        public bool UseMetric
        {
            get => _useMetric;
            set
            {
                if (_useMetric == value)
                    return;
                _useMetric = value;
                OnPropertyChanged();
                Preferences.Default.Set("UseMetric", _useMetric);
                AeonLog.Info(LogCat, "UseMetric", $"value={value}");
                UpdateAllCalculations();
            }
        }

        private string _colorScheme = ThemeService.DefaultDark;
        public string ColorScheme
        {
            get => _colorScheme;
            set
            {
                if (_colorScheme == value)
                    return;
                _colorScheme = value;
                OnPropertyChanged();
                // Apply the scheme immediately so every DynamicResource binding updates
                ThemeService.Instance.ApplyScheme(_colorScheme);
                // Persist the user's choice across app restarts
                Preferences.Default.Set("ColorScheme", _colorScheme);
                AeonLog.Info(LogCat, "ColorScheme", $"value={value}");
            }
        }

        private string _textSize = FontSizeService.Normal;
        public string TextSize
        {
            get => _textSize;
            set
            {
                if (_textSize == value)
                    return;
                _textSize = value;
                OnPropertyChanged();
                // Apply the preset immediately so every DynamicResource FontSize binding updates
                FontSizeService.Instance.ApplyPreset(_textSize);
                // Persist the user's choice across app restarts
                Preferences.Default.Set("TextSize", _textSize);
            }
        }

        private string _displayLanguage = LangDefault;
        public string DisplayLanguage
        {
            get => _displayLanguage;
            set
            {
                if (_displayLanguage == value)
                    return;
                _displayLanguage = value;
                OnPropertyChanged();
                // Apply the language immediately
                ApplyLanguage(_displayLanguage);
                // Persist the user's choice across app restarts
                Preferences.Default.Set("DisplayLanguage", _displayLanguage);
                AeonLog.Info(LogCat, "Language", $"value={_displayLanguage} culture={System.Globalization.CultureInfo.CurrentUICulture.Name}");
                // Push all new AppResources strings to every bound Label
                Loc.Invalidate();
                // Also re-run all ticker calculations, as many strings are resource-driven
                UpdateAllCalculations();
                // BaseDateDisplay uses CurrentUICulture for date formatting
                OnPropertyChanged(nameof(BaseDateDisplay));
            }
        }

        // Subsection Expanded States
        private bool _labExpanded = true;
        public bool LabExpanded
        {
            get => _labExpanded;
            set { _labExpanded = value; OnPropertyChanged(); }
        }

        private bool _cosmosExpanded = false;
        public bool CosmosExpanded
        {
            get => _cosmosExpanded;
            set { _cosmosExpanded = value; OnPropertyChanged(); }
        }

        private bool _mirrorExpanded = false;
        public bool MirrorExpanded
        {
            get => _mirrorExpanded;
            set { _mirrorExpanded = value; OnPropertyChanged(); }
        }

        private bool _ecoExpanded = false;
        public bool EcoExpanded
        {
            get => _ecoExpanded;
            set { _ecoExpanded = value; OnPropertyChanged(); }
        }

        // Ticker Card Expanded States
        private bool _timeJubileesExpanded = true;
        public bool TimeJubileesExpanded
        {
            get => _timeJubileesExpanded;
            set { _timeJubileesExpanded = value; OnPropertyChanged(); }
        }

        private bool _countdownExpanded = true;
        public bool CountdownExpanded
        {
            get => _countdownExpanded;
            set { _countdownExpanded = value; OnPropertyChanged(); }
        }

        private bool _lifeOdometerExpanded = false;
        public bool LifeOdometerExpanded
        {
            get => _lifeOdometerExpanded;
            set { _lifeOdometerExpanded = value; OnPropertyChanged(); }
        }

        private bool _alienAnniversariesExpanded = true;
        public bool AlienAnniversariesExpanded
        {
            get => _alienAnniversariesExpanded;
            set { _alienAnniversariesExpanded = value; OnPropertyChanged(); }
        }

        private bool _galacticCommuteExpanded = true;
        public bool GalacticCommuteExpanded
        {
            get => _galacticCommuteExpanded;
            set { _galacticCommuteExpanded = value; OnPropertyChanged(); }
        }

        private bool _photonPathExpanded = true;
        public bool PhotonPathExpanded
        {
            get => _photonPathExpanded;
            set { _photonPathExpanded = value; OnPropertyChanged(); }
        }

        private bool _cosmicStretchExpanded = true;
        public bool CosmicStretchExpanded
        {
            get => _cosmicStretchExpanded;
            set { _cosmicStretchExpanded = value; OnPropertyChanged(); }
        }

        private bool _humanBirthRankExpanded = false;
        public bool HumanBirthRankExpanded
        {
            get => _humanBirthRankExpanded;
            set { _humanBirthRankExpanded = value; OnPropertyChanged(); }
        }

        private bool _birthRuneExpanded = false;
        public bool BirthRuneExpanded
        {
            get => _birthRuneExpanded;
            set { _birthRuneExpanded = value; OnPropertyChanged(); }
        }

        private bool _personalYearExpanded = false;
        public bool PersonalYearExpanded
        {
            get => _personalYearExpanded;
            set { _personalYearExpanded = value; OnPropertyChanged(); }
        }

        private bool _globalExhaleExpanded = false;
        public bool GlobalExhaleExpanded
        {
            get => _globalExhaleExpanded;
            set { _globalExhaleExpanded = value; OnPropertyChanged(); }
        }

        private bool _yourBreathExpanded = false;
        public bool YourBreathExpanded
        {
            get => _yourBreathExpanded;
            set { _yourBreathExpanded = value; OnPropertyChanged(); }
        }

        private bool _cellularRefreshExpanded = false;
        public bool CellularRefreshExpanded
        {
            get => _cellularRefreshExpanded;
            set { _cellularRefreshExpanded = value; OnPropertyChanged(); }
        }

        private bool _vibrantCosmosExpanded = false;
        public bool VibrantCosmosExpanded
        {
            get => _vibrantCosmosExpanded;
            set { _vibrantCosmosExpanded = value; OnPropertyChanged(); }
        }

        private bool _globalCrowdExpanded = false;
        public bool GlobalCrowdExpanded
        {
            get => _globalCrowdExpanded;
            set { _globalCrowdExpanded = value; OnPropertyChanged(); }
        }

        private bool _vibrantHumanityExpanded = false;
        public bool VibrantHumanityExpanded
        {
            get => _vibrantHumanityExpanded;
            set { _vibrantHumanityExpanded = value; OnPropertyChanged(); }
        }

        private bool _lifeLogExpanded = false;
        public bool LifeLogExpanded
        {
            get => _lifeLogExpanded;
            set { _lifeLogExpanded = value; OnPropertyChanged(); }
        }

        private bool _spaceWaitExpanded = false;
        public bool SpaceWaitExpanded
        {
            get => _spaceWaitExpanded;
            set { _spaceWaitExpanded = value; OnPropertyChanged(); }
        }

        private bool _vibrantNatureExpanded = false;
        public bool VibrantNatureExpanded
        {
            get => _vibrantNatureExpanded;
            set { _vibrantNatureExpanded = value; OnPropertyChanged(); }
        }

        // Ticker Data
        private TimeJubileesResult _timeJubilees = new TimeJubileesResult();
        public TimeJubileesResult TimeJubilees
        {
            get => _timeJubilees;
            set { _timeJubilees = value; OnPropertyChanged(); }
        }

        private CountdownResult _countdown = new CountdownResult();
        public CountdownResult Countdown
        {
            get => _countdown;
            set { _countdown = value; OnPropertyChanged(); }
        }

        private LifeOdometerResult _lifeOdometer = new LifeOdometerResult();
        public LifeOdometerResult LifeOdometer
        {
            get => _lifeOdometer;
            set { _lifeOdometer = value; OnPropertyChanged(); }
        }

        private AlienAnniversariesResult _alienAnniversaries = new AlienAnniversariesResult();
        public AlienAnniversariesResult AlienAnniversaries
        {
            get => _alienAnniversaries;
            set { _alienAnniversaries = value; OnPropertyChanged(); }
        }

        private GalacticCommuteResult _galacticCommute = new GalacticCommuteResult();
        public GalacticCommuteResult GalacticCommute
        {
            get => _galacticCommute;
            set { _galacticCommute = value; OnPropertyChanged(); }
        }

        private PhotonPathResult _photonPath = new PhotonPathResult();
        public PhotonPathResult PhotonPath
        {
            get => _photonPath;
            set { _photonPath = value; OnPropertyChanged(); }
        }

        private CosmicStretchResult _cosmicStretch = new CosmicStretchResult();
        public CosmicStretchResult CosmicStretch
        {
            get => _cosmicStretch;
            set { _cosmicStretch = value; OnPropertyChanged(); }
        }

        private HumanBirthRankResult _humanBirthRank = new HumanBirthRankResult();
        public HumanBirthRankResult HumanBirthRank
        {
            get => _humanBirthRank;
            set { _humanBirthRank = value; OnPropertyChanged(); }
        }

        private BirthRuneResult _birthRune = new BirthRuneResult();
        public BirthRuneResult BirthRune
        {
            get => _birthRune;
            set { _birthRune = value; OnPropertyChanged(); }
        }

        private PersonalYearResult _personalYear = new PersonalYearResult();
        public PersonalYearResult PersonalYear
        {
            get => _personalYear;
            set { _personalYear = value; OnPropertyChanged(); }
        }

        private GlobalExhaleResult _globalExhale = new GlobalExhaleResult();
        public GlobalExhaleResult GlobalExhale
        {
            get => _globalExhale;
            set { _globalExhale = value; OnPropertyChanged(); }
        }

        private YourBreathResult _yourBreath = new YourBreathResult();
        public YourBreathResult YourBreath
        {
            get => _yourBreath;
            set { _yourBreath = value; OnPropertyChanged(); }
        }

        private CellularRefreshResult _cellularRefresh = new CellularRefreshResult();
        public CellularRefreshResult CellularRefresh
        {
            get => _cellularRefresh;
            set { _cellularRefresh = value; OnPropertyChanged(); }
        }

        private VibrantCosmosResult _vibrantCosmos = new VibrantCosmosResult();
        public VibrantCosmosResult VibrantCosmos
        {
            get => _vibrantCosmos;
            set { _vibrantCosmos = value; OnPropertyChanged(); }
        }

        private GlobalCrowdResult _globalCrowd = new GlobalCrowdResult();
        public GlobalCrowdResult GlobalCrowd
        {
            get => _globalCrowd;
            set { _globalCrowd = value; OnPropertyChanged(); }
        }

        private VibrantHumanityResult _vibrantHumanity = new VibrantHumanityResult();
        public VibrantHumanityResult VibrantHumanity
        {
            get => _vibrantHumanity;
            set { _vibrantHumanity = value; OnPropertyChanged(); }
        }

        private LifeLogResult _lifeLog = new LifeLogResult();
        public LifeLogResult LifeLog
        {
            get => _lifeLog;
            set { _lifeLog = value; OnPropertyChanged(); }
        }

        private SpaceWaitResult _spaceWait = new SpaceWaitResult();
        public SpaceWaitResult SpaceWait
        {
            get => _spaceWait;
            set { _spaceWait = value; OnPropertyChanged(); }
        }

        private VibrantNatureResult _vibrantNature = new VibrantNatureResult();
        public VibrantNatureResult VibrantNature
        {
            get => _vibrantNature;
            set { _vibrantNature = value; OnPropertyChanged(); }
        }

        private string _teaseText = "";
        public string TeaseText
        {
            get => _teaseText;
            set { _teaseText = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand ToggleLabCommand { get; }
        public ICommand ToggleCosmosCommand { get; }
        public ICommand ToggleMirrorCommand { get; }
        public ICommand ToggleEcoCommand { get; }
        public ICommand RefreshStaticCommand { get; }
        public ICommand RefreshLiveCommand { get; }

        // Card-level toggle commands
        public ICommand ToggleTimeJubileesCommand { get; }
        public ICommand ToggleCountdownCommand { get; }
        public ICommand ToggleLifeOdometerCommand { get; }
        public ICommand ToggleAlienAnniversariesCommand { get; }
        public ICommand ToggleGalacticCommuteCommand { get; }
        public ICommand TogglePhotonPathCommand { get; }
        public ICommand ToggleCosmicStretchCommand { get; }
        public ICommand ToggleHumanBirthRankCommand { get; }
        public ICommand ToggleBirthRuneCommand { get; }
        public ICommand TogglePersonalYearCommand { get; }
        public ICommand ToggleGlobalExhaleCommand { get; }
        public ICommand ToggleYourBreathCommand { get; }
        public ICommand ToggleCellularRefreshCommand { get; }
        public ICommand ToggleVibrantCosmosCommand { get; }
        public ICommand ToggleGlobalCrowdCommand { get; }
        public ICommand ToggleSpaceWaitCommand { get; }
        public ICommand ToggleVibrantHumanityCommand { get; }
        public ICommand ToggleVibrantNatureCommand { get; }

        // Card-level refresh commands
        public ICommand RefreshTimeJubileesCommand { get; }
        public ICommand RefreshAlienAnniversariesCommand { get; }
        public ICommand RefreshGlobalExhaleCommand { get; }
        public ICommand RefreshCellularRefreshCommand { get; }
        public ICommand ToggleLifeLogCommand { get; }
        public ICommand RefreshLifeLogCommand { get; }
        public ICommand RefreshVibrantNatureCommand { get; }

        /// <summary>
        /// Raised when a live refresh is requested, so the View layer can show
        /// the RefreshingPopup before recalculation fires via the callback.
        /// </summary>
        public event Func<Action, Task>? RefreshRequested;

        #endregion

        public MainViewModel()
        {
            _calculationService = new CalculationService();

            // Restore persisted colour scheme (defaults to DefaultDark on first run)
            var savedScheme = Preferences.Default.Get("ColorScheme", ThemeService.DefaultDark);
            _colorScheme = savedScheme;
            ThemeService.Instance.ApplyScheme(_colorScheme);

            // Restore persisted text size (defaults to Normal on first run)
            var savedTextSize = Preferences.Default.Get("TextSize", FontSizeService.Normal);
            _textSize = savedTextSize;
            FontSizeService.Instance.ApplyPreset(_textSize);

            // Restore persisted display language (defaults to Default on first run)
            var savedLanguage = Preferences.Default.Get("DisplayLanguage", LangDefault);
            _displayLanguage = savedLanguage;
            // ApplyLanguage was already called in App.xaml.cs, no need to call again

            // Restore persisted unit system (defaults to true/Metric on first run)
            _useMetric = Preferences.Default.Get("UseMetric", true);

            // Initialize section commands
            ToggleLabCommand = new Command(() => LabExpanded = !LabExpanded);
            ToggleCosmosCommand = new Command(() => CosmosExpanded = !CosmosExpanded);
            ToggleMirrorCommand = new Command(() => MirrorExpanded = !MirrorExpanded);
            ToggleEcoCommand = new Command(() => EcoExpanded = !EcoExpanded);
            RefreshStaticCommand = new Command(UpdateStaticCalculations);
            RefreshLiveCommand = new Command(UpdateLiveCalculations);

            // Initialize card-level toggle commands
            ToggleTimeJubileesCommand = new Command(() => TimeJubileesExpanded = !TimeJubileesExpanded);
            ToggleCountdownCommand = new Command(() => CountdownExpanded = !CountdownExpanded);
            ToggleLifeOdometerCommand = new Command(() => LifeOdometerExpanded = !LifeOdometerExpanded);
            ToggleAlienAnniversariesCommand = new Command(() => AlienAnniversariesExpanded = !AlienAnniversariesExpanded);
            ToggleGalacticCommuteCommand = new Command(() => GalacticCommuteExpanded = !GalacticCommuteExpanded);
            TogglePhotonPathCommand = new Command(() => PhotonPathExpanded = !PhotonPathExpanded);
            ToggleCosmicStretchCommand = new Command(() => CosmicStretchExpanded = !CosmicStretchExpanded);
            ToggleHumanBirthRankCommand = new Command(() => HumanBirthRankExpanded = !HumanBirthRankExpanded);
            ToggleBirthRuneCommand = new Command(() => BirthRuneExpanded = !BirthRuneExpanded);
            TogglePersonalYearCommand = new Command(() => PersonalYearExpanded = !PersonalYearExpanded);
            ToggleGlobalExhaleCommand = new Command(() => GlobalExhaleExpanded = !GlobalExhaleExpanded);
            ToggleYourBreathCommand = new Command(() => YourBreathExpanded = !YourBreathExpanded);
            ToggleCellularRefreshCommand = new Command(() => CellularRefreshExpanded = !CellularRefreshExpanded);
            ToggleVibrantCosmosCommand = new Command(() => VibrantCosmosExpanded = !VibrantCosmosExpanded);
            ToggleGlobalCrowdCommand = new Command(() => GlobalCrowdExpanded = !GlobalCrowdExpanded);
            ToggleSpaceWaitCommand   = new Command(() => SpaceWaitExpanded = !SpaceWaitExpanded);
            ToggleVibrantHumanityCommand = new Command(() => VibrantHumanityExpanded = !VibrantHumanityExpanded);
            ToggleVibrantNatureCommand = new Command(() => VibrantNatureExpanded = !VibrantNatureExpanded);

            ToggleLifeLogCommand = new Command(() => LifeLogExpanded = !LifeLogExpanded);

            // Initialize card-level refresh commands
            RefreshTimeJubileesCommand = new Command(async () =>
            {
                if (RefreshRequested != null)
                    await RefreshRequested.Invoke(() =>
                        TimeJubilees = _calculationService.CalculateTimeJubilees(BaseDate, BaseDateName, BaseDateValue));
                else
                    TimeJubilees = _calculationService.CalculateTimeJubilees(BaseDate, BaseDateName, BaseDateValue);
            });
            RefreshAlienAnniversariesCommand = new Command(async () =>
            {
                if (RefreshRequested != null)
                    await RefreshRequested.Invoke(() =>
                        AlienAnniversaries = _calculationService.CalculateAlienAnniversaries(BaseDate, BaseDateName, BaseDateValue));
                else
                    AlienAnniversaries = _calculationService.CalculateAlienAnniversaries(BaseDate, BaseDateName, BaseDateValue);
            });
            RefreshGlobalExhaleCommand = new Command(async () =>
            {
                if (RefreshRequested != null)
                    await RefreshRequested.Invoke(() =>
                        GlobalExhale = _calculationService.CalculateGlobalExhale(BaseDate, BaseDateName, BaseDateValue, UseMetric));
                else
                    GlobalExhale = _calculationService.CalculateGlobalExhale(BaseDate, BaseDateName, BaseDateValue, UseMetric);
            });
            RefreshCellularRefreshCommand = new Command(async () =>
            {
                if (RefreshRequested != null)
                    await RefreshRequested.Invoke(() =>
                        CellularRefresh = _calculationService.CalculateCellularRefresh(BaseDate, BaseDateName, BaseDateValue));
                else
                    CellularRefresh = _calculationService.CalculateCellularRefresh(BaseDate, BaseDateName, BaseDateValue);
            });
            RefreshLifeLogCommand = new Command(async () =>
            {
                if (RefreshRequested != null)
                    await RefreshRequested.Invoke(() =>
                        LifeLog = _calculationService.CalculateLifeLog(BaseDate, BaseDateName, BaseDateValue));
                else
                    LifeLog = _calculationService.CalculateLifeLog(BaseDate, BaseDateName, BaseDateValue);
            });
            RefreshVibrantNatureCommand = new Command(async () =>
            {
                if (RefreshRequested != null)
                    await RefreshRequested.Invoke(() =>
                        VibrantNature = _calculationService.CalculateVibrantNature(BaseDate));
                else
                    VibrantNature = _calculationService.CalculateVibrantNature(BaseDate);
            });

            // Initial calculations
            UpdateAllCalculations();

            // Setup timer for live updates (every second)
            _updateTimer = Application.Current!.Dispatcher.CreateTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Tick += (s, e) => UpdateLiveCalculations();
            _updateTimer.Start();

            // Setup 200ms timer for Vibrant Cosmos - non-uniform rhythmic pulse
            _vibrantCosmosTimer = Application.Current!.Dispatcher.CreateTimer();
            _vibrantCosmosTimer.Interval = TimeSpan.FromMilliseconds(200);
            _vibrantCosmosTimer.Tick += (s, e) => UpdateVibrantCosmos();
            _vibrantCosmosTimer.Start();
        }

        public void UpdateAllCalculations()
        {
            UpdateStaticCalculations();
            UpdateLiveCalculations();
            UpdateVibrantCosmos();
        }

        public void UpdateStaticCalculations()
        {
            TimeJubilees = _calculationService.CalculateTimeJubilees(BaseDate, BaseDateName, BaseDateValue);
            AlienAnniversaries = _calculationService.CalculateAlienAnniversaries(BaseDate, BaseDateName, BaseDateValue);
            HumanBirthRank = _calculationService.CalculateHumanBirthRank(BaseDate, BaseDateName);
            BirthRune = _calculationService.CalculateBirthRune(BaseDate, BaseDateValue);
            PersonalYear = _calculationService.CalculatePersonalYear(BaseDate, BaseDateValue);
            GlobalExhale = _calculationService.CalculateGlobalExhale(BaseDate, BaseDateName, BaseDateValue, UseMetric);
            CellularRefresh = _calculationService.CalculateCellularRefresh(BaseDate, BaseDateName, BaseDateValue);
            LifeLog = _calculationService.CalculateLifeLog(BaseDate, BaseDateName, BaseDateValue);
            VibrantNature = _calculationService.CalculateVibrantNature(BaseDate);
        }

        public void UpdateLiveCalculations()
        {
            AeonLog.Debug(LogCat, "Timer", $"thread={Environment.CurrentManagedThreadId} isMainThread={MainThread.IsMainThread}");
            Countdown    = _calculationService.CalculateCountdown(BaseDate);
            LifeOdometer = _calculationService.CalculateLifeOdometer(BaseDate, BaseDateName, BaseDateValue);
            GalacticCommute = _calculationService.CalculateGalacticCommute(BaseDate, BaseDateValue, UseMetric);
            PhotonPath   = _calculationService.CalculatePhotonPath(BaseDate, BaseDateValue, UseMetric);
            CosmicStretch = _calculationService.CalculateCosmicStretch(BaseDate, BaseDateValue, UseMetric);
            YourBreath   = _calculationService.CalculateYourBreath(BaseDate, BaseDateValue, UseMetric);
            GlobalCrowd  = _calculationService.CalculateGlobalCrowd(BaseDate);
            SpaceWait    = _calculationService.CalculateSpaceWait(BaseDate);
            VibrantHumanity = _calculationService.CalculateVibrantHumanity(BaseDate, BaseDateName, BaseDateValue);

            TeaseText = _calculationService.GetRandomTeaseText(
                Countdown,
                LifeOdometer,
                GalacticCommute,
                GlobalExhale,
                BaseDateName,
                BaseDate
            );
        }

        public void UpdateVibrantCosmos()
        {
            VibrantCosmos = _calculationService.CalculateVibrantCosmos(BaseDate);
        }

        /// <summary>
        /// Updates all three base date fields atomically, then recalculates all tickers once.
        /// Replaces the old SaveDate that set properties sequentially, causing calculations
        /// to fire with stale BaseDateName/BaseDateValue before they were updated.
        /// </summary>
        public void SaveDate(string name, string date)
        {
            AeonLog.Info(LogCat, "SaveDate", $"in: name={name} date={date}");
            // Update the backing fields directly to avoid triggering
            // UpdateAllCalculations() prematurely via the BaseDate setter
            _baseDateName = name;
            _baseDateValue = date;
            _baseDate = DateTime.Parse(date);
            AeonLog.Debug(LogCat, "SaveDate", $"out: BaseDateName={_baseDateName} BaseDateValue={_baseDateValue} BaseDate={_baseDate:d}");

            // Notify UI of all changes
            OnPropertyChanged(nameof(BaseDateName));
            OnPropertyChanged(nameof(BaseDateValue));
            OnPropertyChanged(nameof(BaseDate));
            OnPropertyChanged(nameof(BaseDateDisplay));

            // Recalculate all tickers once, with all three values now consistent
            UpdateAllCalculations();
        }

        /// <summary>
        /// Resets all user-configurable settings to their factory defaults and
        /// persists the new values. Clears persisted window geometry so the next
        /// launch uses the default size and position.
        /// <para>
        /// Default values: <c>UseMetric=true</c>, <c>ColorScheme=DefaultDark</c>,
        /// <c>TextSize=Normal</c>, <c>DisplayLanguage=Default</c>.
        /// </para>
        /// </summary>
        public void ResetSettings()
        {
            UseMetric       = true;
            ColorScheme     = ThemeService.DefaultDark;
            TextSize        = FontSizeService.Normal;
            DisplayLanguage = LangDefault;

            // Clear persisted window geometry so the next launch recalculates
            // the default size (430 px wide, 2/3 of screen height, centred).
            Preferences.Default.Remove("WinX");
            Preferences.Default.Remove("WinY");
            Preferences.Default.Remove("WinWidth");
            Preferences.Default.Remove("WinHeight");

            AeonLog.Info(LogCat, "ResetSettings", "all settings restored to defaults");
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Stops both background timers immediately. Must be called before the
        /// UI visual tree is torn down (e.g. from <c>Window.Destroying</c>) to
        /// prevent in-flight <see cref="PropertyChanged"/> notifications from
        /// reaching already-disposed WinRT UI elements.
        /// </summary>
        public void StopTimers()
        {
            _updateTimer?.Stop();
            _vibrantCosmosTimer?.Stop();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
