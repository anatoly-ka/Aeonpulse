using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Aeonpulse.Models;
using Aeonpulse.Services;

namespace Aeonpulse.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly CalculationService _calculationService;
        private System.Timers.Timer _updateTimer;

        #region Properties

        private string _baseDateName = "I was born";
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
            set { _useMetric = value; OnPropertyChanged(); UpdateAllCalculations(); }
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

        // Ticker Data
        private TickerData _timeJubilees = new TickerData();
        public TickerData TimeJubilees
        {
            get => _timeJubilees;
            set { _timeJubilees = value; OnPropertyChanged(); }
        }

        private TickerData _countdown = new TickerData();
        public TickerData Countdown
        {
            get => _countdown;
            set { _countdown = value; OnPropertyChanged(); }
        }

        private TickerData _lifeOdometer = new TickerData();
        public TickerData LifeOdometer
        {
            get => _lifeOdometer;
            set { _lifeOdometer = value; OnPropertyChanged(); }
        }

        private TickerData _alienAnniversaries = new TickerData();
        public TickerData AlienAnniversaries
        {
            get => _alienAnniversaries;
            set { _alienAnniversaries = value; OnPropertyChanged(); }
        }

        private TickerData _galacticCommute = new TickerData();
        public TickerData GalacticCommute
        {
            get => _galacticCommute;
            set { _galacticCommute = value; OnPropertyChanged(); }
        }

        private TickerData _photonPath = new TickerData();
        public TickerData PhotonPath
        {
            get => _photonPath;
            set { _photonPath = value; OnPropertyChanged(); }
        }

        private TickerData _humanBirthRank = new TickerData();
        public TickerData HumanBirthRank
        {
            get => _humanBirthRank;
            set { _humanBirthRank = value; OnPropertyChanged(); }
        }

        private TickerData _birthRune = new TickerData();
        public TickerData BirthRune
        {
            get => _birthRune;
            set { _birthRune = value; OnPropertyChanged(); }
        }

        private TickerData _personalYear = new TickerData();
        public TickerData PersonalYear
        {
            get => _personalYear;
            set { _personalYear = value; OnPropertyChanged(); }
        }

        private TickerData _globalExhale = new TickerData();
        public TickerData GlobalExhale
        {
            get => _globalExhale;
            set { _globalExhale = value; OnPropertyChanged(); }
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
        public ICommand ToggleHumanBirthRankCommand { get; }
        public ICommand ToggleBirthRuneCommand { get; }
        public ICommand TogglePersonalYearCommand { get; }
        public ICommand ToggleGlobalExhaleCommand { get; }

        // Card-level refresh commands
        public ICommand RefreshTimeJubileesCommand { get; }
        public ICommand RefreshAlienAnniversariesCommand { get; }
        public ICommand RefreshGlobalExhaleCommand { get; }

        /// <summary>
        /// Raised when a live refresh is requested, so the View layer can show
        /// the RefreshingPopup before recalculation fires via the callback.
        /// </summary>
        public event Func<Action, Task>? RefreshRequested;

        #endregion

        public MainViewModel()
        {
            _calculationService = new CalculationService();

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
            ToggleHumanBirthRankCommand = new Command(() => HumanBirthRankExpanded = !HumanBirthRankExpanded);
            ToggleBirthRuneCommand = new Command(() => BirthRuneExpanded = !BirthRuneExpanded);
            TogglePersonalYearCommand = new Command(() => PersonalYearExpanded = !PersonalYearExpanded);
            ToggleGlobalExhaleCommand = new Command(() => GlobalExhaleExpanded = !GlobalExhaleExpanded);

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

            // Initial calculations
            UpdateAllCalculations();

            // Setup timer for live updates (every second)
            _updateTimer = new System.Timers.Timer(1000);
            _updateTimer.Elapsed += (s, e) =>
                MainThread.BeginInvokeOnMainThread(UpdateLiveCalculations);
            _updateTimer.Start();
        }

        public void UpdateAllCalculations()
        {
            UpdateStaticCalculations();
            UpdateLiveCalculations();
        }

        public void UpdateStaticCalculations()
        {
            TimeJubilees = _calculationService.CalculateTimeJubilees(BaseDate, BaseDateName, BaseDateValue);
            AlienAnniversaries = _calculationService.CalculateAlienAnniversaries(BaseDate, BaseDateName, BaseDateValue);
            HumanBirthRank = _calculationService.CalculateHumanBirthRank(BaseDate, BaseDateName);
            BirthRune = _calculationService.CalculateBirthRune(BaseDate, BaseDateValue);
            PersonalYear = _calculationService.CalculatePersonalYear(BaseDate, BaseDateValue);
            GlobalExhale = _calculationService.CalculateGlobalExhale(BaseDate, BaseDateName, BaseDateValue, UseMetric);
        }

        public void UpdateLiveCalculations()
        {
            Countdown = _calculationService.CalculateCountdown(BaseDate);
            LifeOdometer = _calculationService.CalculateLifeOdometer(BaseDate, BaseDateName, BaseDateValue);
            GalacticCommute = _calculationService.CalculateGalacticCommute(BaseDate, BaseDateValue, UseMetric);
            PhotonPath = _calculationService.CalculatePhotonPath(BaseDate, BaseDateValue, UseMetric);

            TeaseText = _calculationService.GetRandomTeaseText(Countdown, LifeOdometer, GalacticCommute, GlobalExhale, BaseDateName, BaseDateValue);
        }

        /// <summary>
        /// Updates all three base date fields atomically, then recalculates all tickers once.
        /// Replaces the old SaveDate that set properties sequentially, causing calculations
        /// to fire with stale BaseDateName/BaseDateValue before they were updated.
        /// </summary>
        public void SaveDate(string name, string date)
        {
            // Update the backing fields directly to avoid triggering
            // UpdateAllCalculations() prematurely via the BaseDate setter
            _baseDateName = name;
            _baseDateValue = date;
            _baseDate = DateTime.Parse(date);

            // Notify UI of all three changes
            OnPropertyChanged(nameof(BaseDateName));
            OnPropertyChanged(nameof(BaseDateValue));
            OnPropertyChanged(nameof(BaseDate));

            // Recalculate all tickers once, with all three values now consistent
            UpdateAllCalculations();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
