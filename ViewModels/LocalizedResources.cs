using System.ComponentModel;
using System.Runtime.CompilerServices;
using Aeonpulse.Resources;

namespace Aeonpulse.ViewModels
{
    /// <summary>
    /// A live, bindable wrapper around AppResources.
    /// Raise <see cref="Invalidate"/> after changing AppResources.Culture to push all
    /// new strings to every bound UI element simultaneously.
    /// </summary>
    public class LocalizedResources : INotifyPropertyChanged
    {
        public static readonly LocalizedResources Instance = new();

        // -- AppName / Badge ------------------------------------------------
        public string AppName                    => AppResources.AppName;
        public string Badge_LIVE                 => AppResources.Badge_LIVE;

        // -- Timeline -------------------------------------------------------
        public string Timeline_BaseDatePreposition => AppResources.Timeline_BaseDatePreposition;

        // -- Sections -------------------------------------------------------
        public string Section_LabTitle           => AppResources.Section_LabTitle;
        public string Section_CosmosTitle        => AppResources.Section_CosmosTitle;
        public string Section_MirrorTitle        => AppResources.Section_MirrorTitle;
        public string Section_EcoEchoesTitle     => AppResources.Section_EcoEchoesTitle;

        // -- Ticker titles ---------------------------------------------------
        public string Ticker_TimeJubileesTitle         => AppResources.Ticker_TimeJubileesTitle;
        public string Ticker_TimeJubilees_LastJubilee  => AppResources.Ticker_TimeJubilees_LastJubilee;
        public string Ticker_TimeJubilees_NextJubilee  => AppResources.Ticker_TimeJubilees_NextJubilee;
        public string Ticker_TimeJubilees_DaysPassed   => AppResources.Ticker_TimeJubilees_DaysPassed;
        public string Ticker_TimeJubilees_DaysLeft     => AppResources.Ticker_TimeJubilees_DaysLeft;
        public string Ticker_TimeJubilees_Today        => AppResources.Ticker_TimeJubilees_Today;
        public string Ticker_CountdownTitle            => AppResources.Ticker_CountdownTitle;
        public string Ticker_LifeOdometerTitle         => AppResources.Ticker_LifeOdometerTitle;
        public string Ticker_AlienAnniversariesTitle   => AppResources.Ticker_AlienAnniversariesTitle;
        public string Ticker_GalacticCommuteTitle      => AppResources.Ticker_GalacticCommuteTitle;
        public string Ticker_PhotonPathTitle           => AppResources.Ticker_PhotonPathTitle;
        public string Ticker_CosmicStretchTitle        => AppResources.Ticker_CosmicStretchTitle;
        public string Ticker_HumanBirthRankTitle       => AppResources.Ticker_HumanBirthRankTitle;
        public string Ticker_BirthRuneTitle            => AppResources.Ticker_BirthRuneTitle;
        public string Ticker_PersonalYearTitle         => AppResources.Ticker_PersonalYearTitle;
        public string Ticker_GlobalExhaleTitle         => AppResources.Ticker_GlobalExhaleTitle;
        public string Ticker_YourBreathTitle           => AppResources.Ticker_YourBreathTitle;
        public string Ticker_CellularRefreshTitle      => AppResources.Ticker_CellularRefreshTitle;
        public string Ticker_VibrantCosmosTitle        => AppResources.Ticker_VibrantCosmosTitle;
        public string Ticker_GlobalCrowdTitle          => AppResources.Ticker_GlobalCrowdTitle;
        public string Ticker_LifeLogTitle              => AppResources.Ticker_LifeLogTitle;
        public string Ticker_SpaceWaitTitle            => AppResources.Ticker_SpaceWaitTitle;
        public string Ticker_VibrantHumanityTitle      => AppResources.Ticker_VibrantHumanityTitle;
        public string Ticker_VibrantNatureTitle        => AppResources.Ticker_VibrantNatureTitle;

        // -- Settings --------------------------------------------------------
        public string Settings_Title                   => AppResources.Settings_Title;
        public string Settings_SettingsTitle           => AppResources.Settings_SettingsTitle;
        public string Settings_UnitsLabel              => AppResources.Settings_UnitsLabel;
        public string Settings_UnitsMetric             => AppResources.Settings_UnitsMetric;
        public string Settings_UnitsImperial           => AppResources.Settings_UnitsImperial;
        public string Settings_PaletteLabel            => AppResources.Settings_PaletteLabel;
        public string Settings_PaletteDefault          => AppResources.Settings_PaletteDefault;
        public string Settings_PaletteHighContrastDark => AppResources.Settings_PaletteHighContrastDark;
        public string Settings_PaletteHighContrastLight=> AppResources.Settings_PaletteHighContrastLight;
        public string Settings_TextSizeLabel           => AppResources.Settings_TextSizeLabel;
        public string Settings_TextSizeSmall           => AppResources.Settings_TextSizeSmall;
        public string Settings_TextSizeNormal          => AppResources.Settings_TextSizeNormal;
        public string Settings_TextSizeLarge           => AppResources.Settings_TextSizeLarge;
        public string Settings_LanguageLabel           => AppResources.Settings_LanguageLabel;
        public string Settings_LanguageDefault         => AppResources.Settings_LanguageDefault;
        public string Settings_LanguageEnglish         => AppResources.Settings_LanguageEnglish;
        public string Settings_LanguageRussian         => AppResources.Settings_LanguageRussian;
        public string Settings_AboutTitle              => AppResources.Settings_AboutTitle;
        public string Settings_AboutVersion            => AppResources.Settings_AboutVersion;
        public string Settings_AboutDescription        => AppResources.Settings_AboutDescription;
        public string Settings_AboutTagline            => AppResources.Settings_AboutTagline;
        public string Settings_ButtonClose             => AppResources.Settings_ButtonClose;
        public string Settings_ButtonResetSettings     => AppResources.Settings_ButtonResetSettings;

        // -- Change Date popup ------------------------------------------------
        public string ChangeDate_Title                    => AppResources.ChangeDate_Title;
        public string ChangeDate_Description              => AppResources.ChangeDate_Description;
        public string ChangeDate_BaseDateNameLabel        => AppResources.ChangeDate_BaseDateNameLabel;
        public string ChangeDate_BaseDateNamePlaceholder  => AppResources.ChangeDate_BaseDateNamePlaceholder;
        public string ChangeDate_BaseDateLabel            => AppResources.ChangeDate_BaseDateLabel;
        public string ChangeDate_ButtonOK                 => AppResources.ChangeDate_ButtonOK;
        public string ChangeDate_ButtonClose              => AppResources.ChangeDate_ButtonClose;

        // -- Main Menu popup --------------------------------------------------
        public string MainMenu_Title        => AppResources.MainMenu_Title;
        public string MainMenu_ChangeDate   => AppResources.MainMenu_ChangeDate;
        public string MainMenu_Settings     => AppResources.MainMenu_Settings;
        public string MainMenu_Exit         => AppResources.MainMenu_Exit;
        public string MainMenu_ButtonClose  => AppResources.MainMenu_ButtonClose;

        // -- Deep Dive / Info popup -------------------------------------------
        public string Info_MethodTitle                 => AppResources.Info_MethodTitle;
        public string Info_SourceTitle                 => AppResources.Info_SourceTitle;
        public string Info_ButtonClose                 => AppResources.Info_ButtonClose;
        public string Info_TimeJubileesTitle           => AppResources.Info_TimeJubileesTitle;
        public string Info_TimeJubileesMethod          => AppResources.Info_TimeJubileesMethod;
        public string Info_TimeJubileesSource          => AppResources.Info_TimeJubileesSource;
        public string Info_CountdownTitle              => AppResources.Info_CountdownTitle;
        public string Info_CountdownMethod             => AppResources.Info_CountdownMethod;
        public string Info_CountdownSource             => AppResources.Info_CountdownSource;
        public string Info_LifeOdometerTitle           => AppResources.Info_LifeOdometerTitle;
        public string Info_LifeOdometerMethod          => AppResources.Info_LifeOdometerMethod;
        public string Info_LifeOdometerSource          => AppResources.Info_LifeOdometerSource;
        public string Info_AlienAnniversariesTitle     => AppResources.Info_AlienAnniversariesTitle;
        public string Info_AlienAnniversariesMethod    => AppResources.Info_AlienAnniversariesMethod;
        public string Info_AlienAnniversariesSource    => AppResources.Info_AlienAnniversariesSource;
        public string Info_GalacticCommuteTitle        => AppResources.Info_GalacticCommuteTitle;
        public string Info_GalacticCommuteMethod       => AppResources.Info_GalacticCommuteMethod;
        public string Info_GalacticCommuteSource       => AppResources.Info_GalacticCommuteSource;
        public string Info_PhotonPathTitle             => AppResources.Info_PhotonPathTitle;
        public string Info_PhotonPathMethod            => AppResources.Info_PhotonPathMethod;
        public string Info_PhotonPathSource            => AppResources.Info_PhotonPathSource;
        public string Info_CosmicStretchTitle          => AppResources.Info_CosmicStretchTitle;
        public string Info_CosmicStretchMethod         => AppResources.Info_CosmicStretchMethod;
        public string Info_CosmicStretchSource         => AppResources.Info_CosmicStretchSource;
        public string Info_HumanBirthRankTitle         => AppResources.Info_HumanBirthRankTitle;
        public string Info_HumanBirthRankMethod        => AppResources.Info_HumanBirthRankMethod;
        public string Info_HumanBirthRankSource        => AppResources.Info_HumanBirthRankSource;
        public string Info_BirthRuneTitle              => AppResources.Info_BirthRuneTitle;
        public string Info_BirthRuneMethod             => AppResources.Info_BirthRuneMethod;
        public string Info_BirthRuneSource             => AppResources.Info_BirthRuneSource;
        public string Info_PersonalYearTitle           => AppResources.Info_PersonalYearTitle;
        public string Info_PersonalYearMethod          => AppResources.Info_PersonalYearMethod;
        public string Info_PersonalYearSource          => AppResources.Info_PersonalYearSource;
        public string Info_GlobalExhaleTitle           => AppResources.Info_GlobalExhaleTitle;
        public string Info_GlobalExhaleMethod          => AppResources.Info_GlobalExhaleMethod;
        public string Info_GlobalExhaleSource          => AppResources.Info_GlobalExhaleSource;
        public string Info_YourBreathTitle             => AppResources.Info_YourBreathTitle;
        public string Info_YourBreathMethod            => AppResources.Info_YourBreathMethod;
        public string Info_YourBreathSource            => AppResources.Info_YourBreathSource;
        public string Info_CellularRefreshTitle        => AppResources.Info_CellularRefreshTitle;
        public string Info_CellularRefreshMethod       => AppResources.Info_CellularRefreshMethod;
        public string Info_CellularRefreshSource       => AppResources.Info_CellularRefreshSource;
        public string Info_VibrantCosmosTitle          => AppResources.Info_VibrantCosmosTitle;
        public string Info_VibrantCosmosMethod         => AppResources.Info_VibrantCosmosMethod;
        public string Info_VibrantCosmosSource         => AppResources.Info_VibrantCosmosSource;
        public string Info_GlobalCrowdTitle            => AppResources.Info_GlobalCrowdTitle;
        public string Info_GlobalCrowdMethod           => AppResources.Info_GlobalCrowdMethod;
        public string Info_GlobalCrowdSource           => AppResources.Info_GlobalCrowdSource;
        public string Chart_GlobalCrowd_Year_Prefix    => AppResources.Chart_GlobalCrowd_Year_Prefix;
        public string Chart_GlobalCrowd_Pop_Prefix     => AppResources.Chart_GlobalCrowd_Pop_Prefix;
        public string Chart_GlobalExhale_BudgetTitle     => AppResources.Chart_GlobalExhale_BudgetTitle;
        public string Chart_GlobalExhale_Depletion       => AppResources.Chart_GlobalExhale_Depletion;
        public string Chart_GlobalExhale_BaseDate        => AppResources.Chart_GlobalExhale_BaseDate;
        public string Chart_GlobalExhale_Today           => AppResources.Chart_GlobalExhale_Today;
        public string Chart_GlobalExhale_Limit           => AppResources.Chart_GlobalExhale_Limit;

        // -- Tease ------------------------------------------------------------
        public string Tease_Title          => AppResources.Tease_Title;
        public string Tease_ButtonOK       => AppResources.Tease_ButtonOK;
        public string Tease_ButtonCopy     => AppResources.Tease_ButtonCopy;
        public string Tease_CopiedTitle    => AppResources.Tease_CopiedTitle;
        public string Tease_CopiedText     => AppResources.Tease_CopiedText;
        public string Tease_CopiedButtonOK => AppResources.Tease_CopiedButtonOK;

        // -- Refreshing -------------------------------------------------------
        public string Refreshing_Message => AppResources.Refreshing_Message;

        /// <summary>
        /// Fires PropertyChanged("") which causes every bound property on this
        /// instance to re-read its value from AppResources (which now uses the
        /// new culture).
        /// </summary>
        public void Invalidate() => OnPropertyChanged(string.Empty);

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}