using Aeonpulse.Models;
using Aeonpulse.Resources;
using Aeonpulse.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        private string _baseDateValue = "2000-01-01";
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

        private DateTime _baseDate = new DateTime(2000, 1, 1);
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
        // Default (first-start): only Favorites section is open; all tickers are collapsed.
        private bool _labExpanded = false;
        public bool LabExpanded
        {
            get => _labExpanded;
            set { _labExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _favoritesExpanded = true;
        public bool FavoritesExpanded
        {
            get => _favoritesExpanded;
            set { _favoritesExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _cosmosExpanded = false;
        public bool CosmosExpanded
        {
            get => _cosmosExpanded;
            set { _cosmosExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _mirrorExpanded = false;
        public bool MirrorExpanded
        {
            get => _mirrorExpanded;
            set { _mirrorExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _ecoExpanded = false;
        public bool EcoExpanded
        {
            get => _ecoExpanded;
            set { _ecoExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        // Ticker Card Expanded States — all false (Brief view) on first start.
        private bool _timeJubileesExpanded = false;
        public bool TimeJubileesExpanded
        {
            get => _timeJubileesExpanded;
            set { _timeJubileesExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _countdownExpanded = false;
        public bool CountdownExpanded
        {
            get => _countdownExpanded;
            set { _countdownExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _lifeOdometerExpanded = false;
        public bool LifeOdometerExpanded
        {
            get => _lifeOdometerExpanded;
            set { _lifeOdometerExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _alienAnniversariesExpanded = false;
        public bool AlienAnniversariesExpanded
        {
            get => _alienAnniversariesExpanded;
            set { _alienAnniversariesExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _galacticCommuteExpanded = false;
        public bool GalacticCommuteExpanded
        {
            get => _galacticCommuteExpanded;
            set { _galacticCommuteExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _photonPathExpanded = false;
        public bool PhotonPathExpanded
        {
            get => _photonPathExpanded;
            set { _photonPathExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _cosmicStretchExpanded = false;
        public bool CosmicStretchExpanded
        {
            get => _cosmicStretchExpanded;
            set { _cosmicStretchExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _humanBirthRankExpanded = false;
        public bool HumanBirthRankExpanded
        {
            get => _humanBirthRankExpanded;
            set { _humanBirthRankExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _birthRuneExpanded = false;
        public bool BirthRuneExpanded
        {
            get => _birthRuneExpanded;
            set { _birthRuneExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _personalYearExpanded = false;
        public bool PersonalYearExpanded
        {
            get => _personalYearExpanded;
            set { _personalYearExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _globalExhaleExpanded = false;
        public bool GlobalExhaleExpanded
        {
            get => _globalExhaleExpanded;
            set { _globalExhaleExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _yourBreathExpanded = false;
        public bool YourBreathExpanded
        {
            get => _yourBreathExpanded;
            set { _yourBreathExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _cellularRefreshExpanded = false;
        public bool CellularRefreshExpanded
        {
            get => _cellularRefreshExpanded;
            set { _cellularRefreshExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _vibrantCosmosExpanded = false;
        public bool VibrantCosmosExpanded
        {
            get => _vibrantCosmosExpanded;
            set { _vibrantCosmosExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _globalCrowdExpanded = false;
        public bool GlobalCrowdExpanded
        {
            get => _globalCrowdExpanded;
            set { _globalCrowdExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _vibrantHumanityExpanded = false;
        public bool VibrantHumanityExpanded
        {
            get => _vibrantHumanityExpanded;
            set { _vibrantHumanityExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _lifeLogExpanded = false;
        public bool LifeLogExpanded
        {
            get => _lifeLogExpanded;
            set { _lifeLogExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _spaceWaitExpanded = false;
        public bool SpaceWaitExpanded
        {
            get => _spaceWaitExpanded;
            set { _spaceWaitExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
        }

        private bool _vibrantNatureExpanded = false;
        public bool VibrantNatureExpanded
        {
            get => _vibrantNatureExpanded;
            set { _vibrantNatureExpanded = value; OnPropertyChanged(); SaveExpandedStates(); }
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

        /// <summary>
        /// The ordered collection of tickers pinned to the Favorites section.
        /// Each item owns its own independent IsExpanded state so expanding a favorite
        /// does not affect the same ticker in its original section.
        /// </summary>
        public ObservableCollection<Aeonpulse.Models.FavoriteTickerItem> FavoritesCollection { get; }
            = new ObservableCollection<Aeonpulse.Models.FavoriteTickerItem>();

        /// <summary>Returns true when FavoritesCollection has at least one item.</summary>
        public bool HasFavorites => FavoritesCollection.Count > 0;

        /// <summary>Returns true when FavoritesCollection is empty (drives empty-state label).</summary>
        public bool HasNoFavorites => FavoritesCollection.Count == 0;

        // Per-ticker "is already in Favorites" flags - bound to the star/bookmark button Source
        // via BoolToImageSource("in_favorites.png|to_favorites.png").
        public bool TimeJubileesIsInFavorites       => FavoritesCollection.Any(f => f.TickerId == "TimeJubilees");
        public bool CountdownIsInFavorites          => FavoritesCollection.Any(f => f.TickerId == "Countdown");
        public bool LifeOdometerIsInFavorites       => FavoritesCollection.Any(f => f.TickerId == "LifeOdometer");
        public bool CellularRefreshIsInFavorites    => FavoritesCollection.Any(f => f.TickerId == "CellularRefresh");
        public bool AlienAnniversariesIsInFavorites => FavoritesCollection.Any(f => f.TickerId == "AlienAnniversaries");
        public bool SpaceWaitIsInFavorites          => FavoritesCollection.Any(f => f.TickerId == "SpaceWait");
        public bool GalacticCommuteIsInFavorites    => FavoritesCollection.Any(f => f.TickerId == "GalacticCommute");
        public bool PhotonPathIsInFavorites         => FavoritesCollection.Any(f => f.TickerId == "PhotonPath");
        public bool CosmicStretchIsInFavorites      => FavoritesCollection.Any(f => f.TickerId == "CosmicStretch");
        public bool VibrantCosmosIsInFavorites      => FavoritesCollection.Any(f => f.TickerId == "VibrantCosmos");
        public bool HumanBirthRankIsInFavorites     => FavoritesCollection.Any(f => f.TickerId == "HumanBirthRank");
        public bool BirthRuneIsInFavorites          => FavoritesCollection.Any(f => f.TickerId == "BirthRune");
        public bool PersonalYearIsInFavorites       => FavoritesCollection.Any(f => f.TickerId == "PersonalYear");
        public bool GlobalCrowdIsInFavorites        => FavoritesCollection.Any(f => f.TickerId == "GlobalCrowd");
        public bool LifeLogIsInFavorites            => FavoritesCollection.Any(f => f.TickerId == "LifeLog");
        public bool VibrantHumanityIsInFavorites    => FavoritesCollection.Any(f => f.TickerId == "VibrantHumanity");
        public bool GlobalExhaleIsInFavorites       => FavoritesCollection.Any(f => f.TickerId == "GlobalExhale");
        public bool YourBreathIsInFavorites         => FavoritesCollection.Any(f => f.TickerId == "YourBreath");
        public bool VibrantNatureIsInFavorites      => FavoritesCollection.Any(f => f.TickerId == "VibrantNature");

        // Used by AddToFavoritesCommand to build a FavoriteTickerItem from just the ticker ID.
        private (string Emoji, Func<string> TitleGetter, Func<Aeonpulse.Models.TickerData> DataGetter)
            GetTickerMeta(string tickerId) => tickerId switch
        {
            "TimeJubilees"       => ("📅", () => AppResources.Ticker_TimeJubileesTitle,       () => TimeJubilees),
            "Countdown"          => ("⏱️", () => AppResources.Ticker_CountdownTitle,           () => Countdown),
            "LifeOdometer"       => ("❤️", () => AppResources.Ticker_LifeOdometerTitle,        () => LifeOdometer),
            "AlienAnniversaries" => ("👽", () => AppResources.Ticker_AlienAnniversariesTitle,  () => AlienAnniversaries),
            "GalacticCommute"    => ("🚀", () => AppResources.Ticker_GalacticCommuteTitle,     () => GalacticCommute),
            "PhotonPath"         => ("💡", () => AppResources.Ticker_PhotonPathTitle,          () => PhotonPath),
            "CosmicStretch"      => ("🌌", () => AppResources.Ticker_CosmicStretchTitle,       () => CosmicStretch),
            "HumanBirthRank"     => ("🏅", () => AppResources.Ticker_HumanBirthRankTitle,      () => HumanBirthRank),
            "BirthRune"          => ("🔮", () => AppResources.Ticker_BirthRuneTitle,           () => BirthRune),
            "PersonalYear"       => ("🗓️", () => AppResources.Ticker_PersonalYearTitle,        () => PersonalYear),
            "GlobalExhale"       => ("🌿", () => AppResources.Ticker_GlobalExhaleTitle,        () => GlobalExhale),
            "YourBreath"         => ("🫁", () => AppResources.Ticker_YourBreathTitle,          () => YourBreath),
            "CellularRefresh"    => ("🧬", () => AppResources.Ticker_CellularRefreshTitle,     () => CellularRefresh),
            "VibrantCosmos"      => ("✨", () => AppResources.Ticker_VibrantCosmosTitle,       () => VibrantCosmos),
            "GlobalCrowd"        => ("👥", () => AppResources.Ticker_GlobalCrowdTitle,         () => GlobalCrowd),
            "LifeLog"            => ("🕰️", () => AppResources.Ticker_LifeLogTitle,             () => LifeLog),
            "SpaceWait"          => ("🪐", () => AppResources.Ticker_SpaceWaitTitle,           () => SpaceWait),
            "VibrantHumanity"    => ("🌍", () => AppResources.Ticker_VibrantHumanityTitle,     () => VibrantHumanity),
            "VibrantNature"      => ("🦋", () => AppResources.Ticker_VibrantNatureTitle,       () => VibrantNature),
            _                    => throw new ArgumentException($"Unknown ticker ID: {tickerId}")
        };

        #endregion

        #region Commands

        public ICommand ToggleLabCommand { get; }
        public ICommand ToggleCosmosCommand { get; }
        public ICommand ToggleMirrorCommand { get; }
        public ICommand ToggleEcoCommand { get; }
        public ICommand ToggleFavoritesCommand { get; }
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

        // Favorites commands
        public ICommand AddToFavoritesCommand { get; }

        /// <summary>
        /// Raised when a live refresh is requested, so the View layer can show
        /// the RefreshingPopup before recalculation fires via the callback.
        /// </summary>
        public event Func<Action, Task>? RefreshRequested;

        /// <summary>
        /// Raised when a Favorites tile is tapped and the View layer must scroll
        /// the main ScrollView to the specified ticker card.
        /// The string argument is the ticker ID (e.g., "TimeJubilees").
        /// </summary>
        public event Action<string>? ScrollToTickerRequested;

        #endregion

        public MainViewModel()
        {
            _calculationService = new CalculationService();

            // Wire FavoritesCollection changes → notify all computed IsInFavorites
            // properties and HasFavorites/HasNoFavorites so XAML bindings update.
            FavoritesCollection.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasFavorites));
                OnPropertyChanged(nameof(HasNoFavorites));
                OnPropertyChanged(nameof(TimeJubileesIsInFavorites));
                OnPropertyChanged(nameof(CountdownIsInFavorites));
                OnPropertyChanged(nameof(LifeOdometerIsInFavorites));
                OnPropertyChanged(nameof(CellularRefreshIsInFavorites));
                OnPropertyChanged(nameof(LifeLogIsInFavorites));
                OnPropertyChanged(nameof(AlienAnniversariesIsInFavorites));
                OnPropertyChanged(nameof(SpaceWaitIsInFavorites));
                OnPropertyChanged(nameof(GalacticCommuteIsInFavorites));
                OnPropertyChanged(nameof(PhotonPathIsInFavorites));
                OnPropertyChanged(nameof(CosmicStretchIsInFavorites));
                OnPropertyChanged(nameof(VibrantCosmosIsInFavorites));
                OnPropertyChanged(nameof(HumanBirthRankIsInFavorites));
                OnPropertyChanged(nameof(BirthRuneIsInFavorites));
                OnPropertyChanged(nameof(PersonalYearIsInFavorites));
                OnPropertyChanged(nameof(GlobalCrowdIsInFavorites));
                OnPropertyChanged(nameof(VibrantHumanityIsInFavorites));
                OnPropertyChanged(nameof(GlobalExhaleIsInFavorites));
                OnPropertyChanged(nameof(YourBreathIsInFavorites));
                OnPropertyChanged(nameof(VibrantNatureIsInFavorites));
            };

            // When any ticker property is replaced by a recalculation, refresh the
            // matching Favorites tile so its Title and Data reflect the new language/units.
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is null)
                    return;
                if (e.PropertyName.EndsWith("IsInFavorites"))
                    OnPropertyChanged(nameof(HasFavorites));
                RefreshFavoriteTile(e.PropertyName);
            };

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

            // Restore persisted base date name and value
            _baseDateName = Preferences.Default.Get("BaseDateName", AppResources.Default_BaseDateName);
            _baseDateValue = Preferences.Default.Get("BaseDateValue", "2000-01-01");
            _baseDate = DateTime.Parse(_baseDateValue);

            // Restore persisted section/ticker expanded states (falls back to defaults on first run)
            LoadExpandedStates();

            // Initialize section commands
            ToggleLabCommand = new Command(() => LabExpanded = !LabExpanded);
            ToggleCosmosCommand = new Command(() => CosmosExpanded = !CosmosExpanded);
            ToggleMirrorCommand = new Command(() => MirrorExpanded = !MirrorExpanded);
            ToggleEcoCommand = new Command(() => EcoExpanded = !EcoExpanded);
            ToggleFavoritesCommand = new Command(() => FavoritesExpanded = !FavoritesExpanded);
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
            ToggleSpaceWaitCommand = new Command(() => SpaceWaitExpanded = !SpaceWaitExpanded);
            ToggleVibrantHumanityCommand = new Command(() => VibrantHumanityExpanded = !VibrantHumanityExpanded);
            ToggleVibrantNatureCommand = new Command(() => VibrantNatureExpanded = !VibrantNatureExpanded);

            ToggleLifeLogCommand = new Command(() => LifeLogExpanded = !LifeLogExpanded);
            // Favorites - AddToFavoritesCommand accepts a tickerId string parameter.
            // Toggles: adds when not present, removes when already in Favorites.
            AddToFavoritesCommand = new Command<string>(tickerId =>
            {
                if (string.IsNullOrEmpty(tickerId)) return;
                var existing = FavoritesCollection.FirstOrDefault(f => f.TickerId == tickerId);
                if (existing is not null)
                {
                    RemoveFromFavorites(existing);
                }
                else
                {
                    AddToFavorites(tickerId);
                    SaveFavorites();
                }
            });

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

            // Load persisted favorites AFTER initial calculations so TickerData refs are populated
            LoadFavorites();

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

            // Persist the values
            Preferences.Default.Set("BaseDateName", _baseDateName);
            Preferences.Default.Set("BaseDateValue", _baseDateValue);

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

            // Reset all section/ticker expanded states to first-start defaults.
            ApplyDefaultExpandedStates();

            // Reset Favorites tiles to the default five.
            FavoritesCollection.Clear();
            Preferences.Default.Remove(FavoritesPrefsKey);
            LoadFavorites();

            AeonLog.Info(LogCat, "ResetSettings", "all settings restored to defaults");
        }

        #region ExpandedStates

        // Key for the single compact Preferences entry that stores all 24 expanded flags.
        // Format: a 24-character string of '0'/'1' in the fixed order defined by
        // _expandedSlots below.  A length mismatch (e.g. after adding a new ticker)
        // causes a graceful fallback to defaults.
        private const string ExpandedStatesKey = "ExpandedStates";

        // Ordered list of (property-name → backing-field getter/setter pairs).
        // The index in this array is the position in the compact string.
        // Adding a new ticker: append a new tuple at the END and increment the
        // expected length check in LoadExpandedStates.
        private (string Name, Func<bool> Get, Action<bool> Set)[] ExpandedSlots => new[]
        {
            // Sections (indices 0-4)
            (nameof(LabExpanded),      (Func<bool>)(() => _labExpanded),      (Action<bool>)(v => { _labExpanded      = v; OnPropertyChanged(nameof(LabExpanded));      })),
            (nameof(FavoritesExpanded),(Func<bool>)(() => _favoritesExpanded),(Action<bool>)(v => { _favoritesExpanded = v; OnPropertyChanged(nameof(FavoritesExpanded));})),
            (nameof(CosmosExpanded),   (Func<bool>)(() => _cosmosExpanded),   (Action<bool>)(v => { _cosmosExpanded    = v; OnPropertyChanged(nameof(CosmosExpanded));   })),
            (nameof(MirrorExpanded),   (Func<bool>)(() => _mirrorExpanded),   (Action<bool>)(v => { _mirrorExpanded    = v; OnPropertyChanged(nameof(MirrorExpanded));   })),
            (nameof(EcoExpanded),      (Func<bool>)(() => _ecoExpanded),      (Action<bool>)(v => { _ecoExpanded       = v; OnPropertyChanged(nameof(EcoExpanded));      })),
            // Lab ticker cards (indices 5-9): TimeJubilees, Countdown, LifeOdometer, CellularRefresh, LifeLog
            (nameof(TimeJubileesExpanded),       (Func<bool>)(() => _timeJubileesExpanded),       (Action<bool>)(v => { _timeJubileesExpanded       = v; OnPropertyChanged(nameof(TimeJubileesExpanded));       })),
            (nameof(CountdownExpanded),          (Func<bool>)(() => _countdownExpanded),          (Action<bool>)(v => { _countdownExpanded          = v; OnPropertyChanged(nameof(CountdownExpanded));          })),
            (nameof(LifeOdometerExpanded),       (Func<bool>)(() => _lifeOdometerExpanded),       (Action<bool>)(v => { _lifeOdometerExpanded       = v; OnPropertyChanged(nameof(LifeOdometerExpanded));       })),
            (nameof(CellularRefreshExpanded),    (Func<bool>)(() => _cellularRefreshExpanded),    (Action<bool>)(v => { _cellularRefreshExpanded    = v; OnPropertyChanged(nameof(CellularRefreshExpanded));    })),
            (nameof(LifeLogExpanded),            (Func<bool>)(() => _lifeLogExpanded),            (Action<bool>)(v => { _lifeLogExpanded            = v; OnPropertyChanged(nameof(LifeLogExpanded));            })),
            // Cosmos ticker cards (indices 10-14): AlienAnniversaries, SpaceWait, GalacticCommute, PhotonPath, CosmicStretch, VibrantCosmos
            (nameof(AlienAnniversariesExpanded), (Func<bool>)(() => _alienAnniversariesExpanded), (Action<bool>)(v => { _alienAnniversariesExpanded = v; OnPropertyChanged(nameof(AlienAnniversariesExpanded)); })),
            (nameof(SpaceWaitExpanded),          (Func<bool>)(() => _spaceWaitExpanded),          (Action<bool>)(v => { _spaceWaitExpanded          = v; OnPropertyChanged(nameof(SpaceWaitExpanded));          })),
            (nameof(GalacticCommuteExpanded),    (Func<bool>)(() => _galacticCommuteExpanded),    (Action<bool>)(v => { _galacticCommuteExpanded    = v; OnPropertyChanged(nameof(GalacticCommuteExpanded));    })),
            (nameof(PhotonPathExpanded),         (Func<bool>)(() => _photonPathExpanded),         (Action<bool>)(v => { _photonPathExpanded         = v; OnPropertyChanged(nameof(PhotonPathExpanded));         })),
            (nameof(CosmicStretchExpanded),      (Func<bool>)(() => _cosmicStretchExpanded),      (Action<bool>)(v => { _cosmicStretchExpanded      = v; OnPropertyChanged(nameof(CosmicStretchExpanded));      })),
            (nameof(VibrantCosmosExpanded),      (Func<bool>)(() => _vibrantCosmosExpanded),      (Action<bool>)(v => { _vibrantCosmosExpanded      = v; OnPropertyChanged(nameof(VibrantCosmosExpanded));      })),
            // Mirror ticker cards (indices 16-20): HumanBirthRank, BirthRune, PersonalYear, GlobalCrowd, VibrantHumanity
            (nameof(HumanBirthRankExpanded),     (Func<bool>)(() => _humanBirthRankExpanded),     (Action<bool>)(v => { _humanBirthRankExpanded     = v; OnPropertyChanged(nameof(HumanBirthRankExpanded));     })),
            (nameof(BirthRuneExpanded),          (Func<bool>)(() => _birthRuneExpanded),          (Action<bool>)(v => { _birthRuneExpanded          = v; OnPropertyChanged(nameof(BirthRuneExpanded));          })),
            (nameof(PersonalYearExpanded),       (Func<bool>)(() => _personalYearExpanded),       (Action<bool>)(v => { _personalYearExpanded       = v; OnPropertyChanged(nameof(PersonalYearExpanded));       })),
            (nameof(GlobalCrowdExpanded),        (Func<bool>)(() => _globalCrowdExpanded),        (Action<bool>)(v => { _globalCrowdExpanded        = v; OnPropertyChanged(nameof(GlobalCrowdExpanded));        })),
            (nameof(VibrantHumanityExpanded),    (Func<bool>)(() => _vibrantHumanityExpanded),    (Action<bool>)(v => { _vibrantHumanityExpanded    = v; OnPropertyChanged(nameof(VibrantHumanityExpanded));    })),
            // Eco ticker cards (indices 21-23): GlobalExhale, YourBreath, VibrantNature
            (nameof(GlobalExhaleExpanded),       (Func<bool>)(() => _globalExhaleExpanded),       (Action<bool>)(v => { _globalExhaleExpanded       = v; OnPropertyChanged(nameof(GlobalExhaleExpanded));       })),
            (nameof(YourBreathExpanded),         (Func<bool>)(() => _yourBreathExpanded),         (Action<bool>)(v => { _yourBreathExpanded         = v; OnPropertyChanged(nameof(YourBreathExpanded));         })),
            (nameof(VibrantNatureExpanded),      (Func<bool>)(() => _vibrantNatureExpanded),      (Action<bool>)(v => { _vibrantNatureExpanded      = v; OnPropertyChanged(nameof(VibrantNatureExpanded));      })),
        };

        // Default expanded state as a compact string (same order as ExpandedSlots).
        // 0=collapsed, 1=expanded.  Only FavoritesExpanded (index 1) is true on first start.
        // Length must equal the number of entries in ExpandedSlots (5 sections + 19 tickers = 24).
        private const string DefaultExpandedStates = "010000000000000000000000";

        /// <summary>
        /// Serialises all 24 expanded flags to a compact string and writes it to
        /// <c>Preferences</c>.  Called from every <c>Expanded</c> property setter so
        /// the state survives the next app launch.
        /// Suppressed during <see cref="LoadExpandedStates"/> (via <see cref="_suppressExpandedSave"/>)
        /// to avoid redundant writes while bulk-restoring.
        /// </summary>
        private bool _suppressExpandedSave = false;
        private void SaveExpandedStates()
        {
            if (_suppressExpandedSave) return;
            var slots = ExpandedSlots;
            var chars = new char[slots.Length];
            for (int i = 0; i < slots.Length; i++)
                chars[i] = slots[i].Get() ? '1' : '0';
            Preferences.Default.Set(ExpandedStatesKey, new string(chars));
        }

        /// <summary>
        /// Reads the persisted compact string from <c>Preferences</c> and restores
        /// all 24 expanded flags.  Falls back to <see cref="DefaultExpandedStates"/>
        /// when no entry exists (first launch) or the stored string length differs
        /// from the current slot count (schema upgrade).
        /// </summary>
        private void LoadExpandedStates()
        {
            var slots  = ExpandedSlots;
            var saved  = Preferences.Default.Get(ExpandedStatesKey, string.Empty);
            var source = (saved.Length == slots.Length) ? saved : DefaultExpandedStates;

            _suppressExpandedSave = true;
            try
            {
                for (int i = 0; i < slots.Length; i++)
                    slots[i].Set(source[i] == '1');
            }
            finally
            {
                _suppressExpandedSave = false;
            }

            // Persist defaults on first launch so subsequent saves are always diffs.
            if (saved.Length != slots.Length)
                SaveExpandedStates();
        }

        /// <summary>
        /// Applies the hard-coded default expanded states (all collapsed except
        /// Favorites) and persists them.  Called by <see cref="ResetSettings"/>.
        /// </summary>
        private void ApplyDefaultExpandedStates()
        {
            var slots = ExpandedSlots;
            _suppressExpandedSave = true;
            try
            {
                for (int i = 0; i < slots.Length; i++)
                    slots[i].Set(DefaultExpandedStates[i] == '1');
            }
            finally
            {
                _suppressExpandedSave = false;
            }
            SaveExpandedStates();
        }

        #endregion

        #region Favorites

        private const string FavoritesPrefsKey = "FavoriteTickerIds";
        private static readonly string[] DefaultFavoriteIds =
        {
            "TimeJubilees", "AlienAnniversaries", "VibrantHumanity", "VibrantNature", "VibrantCosmos"
        };

        // Maps ViewModel property names to their ticker IDs so that PropertyChanged
        // on this VM can efficiently locate and refresh the matching Favorites tile.
        private static readonly Dictionary<string, string> _propToTickerId = new()
        {
            { nameof(TimeJubilees),       "TimeJubilees"       },
            { nameof(Countdown),          "Countdown"          },
            { nameof(LifeOdometer),       "LifeOdometer"       },
            { nameof(AlienAnniversaries), "AlienAnniversaries" },
            { nameof(GalacticCommute),    "GalacticCommute"    },
            { nameof(PhotonPath),         "PhotonPath"         },
            { nameof(CosmicStretch),      "CosmicStretch"      },
            { nameof(HumanBirthRank),     "HumanBirthRank"     },
            { nameof(BirthRune),          "BirthRune"          },
            { nameof(PersonalYear),       "PersonalYear"       },
            { nameof(GlobalExhale),       "GlobalExhale"       },
            { nameof(YourBreath),         "YourBreath"         },
            { nameof(CellularRefresh),    "CellularRefresh"    },
            { nameof(VibrantCosmos),      "VibrantCosmos"      },
            { nameof(GlobalCrowd),        "GlobalCrowd"        },
            { nameof(LifeLog),            "LifeLog"            },
            { nameof(SpaceWait),          "SpaceWait"          },
            { nameof(VibrantHumanity),    "VibrantHumanity"    },
            { nameof(VibrantNature),      "VibrantNature"      },
        };

        /// <summary>
        /// Calls <see cref="FavoriteTickerItem.Refresh"/> on the tile whose
        /// <c>TickerId</c> matches <paramref name="propertyName"/>, if one exists.
        /// Invoked from the VM's own <c>PropertyChanged</c> event so every ticker
        /// recalculation automatically updates the matching Favorites tile's
        /// <c>Title</c> (language) and <c>Data</c> (new <see cref="TickerData"/> reference).
        /// </summary>
        private void RefreshFavoriteTile(string? propertyName)
        {
            if (propertyName is null) return;
            if (!_propToTickerId.TryGetValue(propertyName, out var tickerId)) return;
            var tile = FavoritesCollection.FirstOrDefault(f => f.TickerId == tickerId);
            tile?.Refresh();
        }

        /// <summary>
        /// Adds a ticker to the Favorites section as a Live Bookmark tile.
        /// The tile shows BriefText only; tapping it jumps to the main card.
        /// </summary>
        private void AddToFavorites(string tickerId)
        {
            var meta = GetTickerMeta(tickerId);
            var item = new Aeonpulse.Models.FavoriteTickerItem(
                tickerId,
                meta.Emoji,
                meta.TitleGetter,
                meta.DataGetter,
                JumpToTicker,
                RemoveFromFavorites);
            FavoritesCollection.Add(item);
        }

        /// <summary>
        /// Expands the parent section and the target ticker card, then raises
        /// <see cref="ScrollToTickerRequested"/> so the View layer can scroll to it.
        /// Called by <see cref="FavoriteTickerItem.JumpToTickerCommand"/>.
        /// </summary>
        private void JumpToTicker(Aeonpulse.Models.FavoriteTickerItem item)
        {
            // Expand the parent section for each ticker
            switch (item.TickerId)
            {
                case "TimeJubilees":
                case "Countdown":
                case "LifeOdometer":
                case "CellularRefresh":
                case "LifeLog":
                    LabExpanded = true;
                    break;
                case "AlienAnniversaries":
                case "GalacticCommute":
                case "PhotonPath":
                case "CosmicStretch":
                case "VibrantCosmos":
                case "SpaceWait":
                    CosmosExpanded = true;
                    break;
                case "HumanBirthRank":
                case "BirthRune":
                case "PersonalYear":
                case "GlobalCrowd":
                case "VibrantHumanity":
                    MirrorExpanded = true;
                    break;
                case "GlobalExhale":
                case "YourBreath":
                case "VibrantNature":
                    EcoExpanded = true;
                    break;
            }

            // Expand the target ticker card
            switch (item.TickerId)
            {
                case "TimeJubilees":       TimeJubileesExpanded       = true; break;
                case "Countdown":          CountdownExpanded          = true; break;
                case "LifeOdometer":       LifeOdometerExpanded       = true; break;
                case "CellularRefresh":    CellularRefreshExpanded    = true; break;
                case "LifeLog":            LifeLogExpanded            = true; break;
                case "AlienAnniversaries": AlienAnniversariesExpanded = true; break;
                case "GalacticCommute":    GalacticCommuteExpanded    = true; break;
                case "PhotonPath":         PhotonPathExpanded         = true; break;
                case "CosmicStretch":      CosmicStretchExpanded      = true; break;
                case "VibrantCosmos":      VibrantCosmosExpanded      = true; break;
                case "SpaceWait":          SpaceWaitExpanded          = true; break;
                case "HumanBirthRank":     HumanBirthRankExpanded     = true; break;
                case "BirthRune":          BirthRuneExpanded          = true; break;
                case "PersonalYear":       PersonalYearExpanded       = true; break;
                case "GlobalCrowd":        GlobalCrowdExpanded        = true; break;
                case "VibrantHumanity":    VibrantHumanityExpanded    = true; break;
                case "GlobalExhale":       GlobalExhaleExpanded       = true; break;
                case "YourBreath":         YourBreathExpanded         = true; break;
                case "VibrantNature":      VibrantNatureExpanded      = true; break;
            }

            // Request the View layer to scroll to the card
            ScrollToTickerRequested?.Invoke(item.TickerId);
        }

        /// <summary>
        /// Removes a ticker from the Favorites section and persists the change.
        /// </summary>
        private void RemoveFromFavorites(Aeonpulse.Models.FavoriteTickerItem item)
        {
            FavoritesCollection.Remove(item);
            SaveFavorites();
        }

        /// <summary>
        /// Persists the current Favorites list to Preferences.
        /// </summary>
        private void SaveFavorites()
        {
            var ids = string.Join(",", FavoritesCollection.Select(f => f.TickerId));
            Preferences.Default.Set(FavoritesPrefsKey, ids);
        }

        /// <summary>
        /// Loads the persisted Favorites list from Preferences. If no persisted list exists
        /// (first launch), populates with the two default tickers.
        /// </summary>
        private void LoadFavorites()
        {
            var saved = Preferences.Default.Get(FavoritesPrefsKey, string.Empty);
            string[] ids = string.IsNullOrWhiteSpace(saved)
                ? DefaultFavoriteIds
                : saved.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var tickerId in ids)
                AddToFavorites(tickerId);

            // Persist defaults if this was the first launch
            if (string.IsNullOrWhiteSpace(saved))
                SaveFavorites();
        }

        #endregion

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
