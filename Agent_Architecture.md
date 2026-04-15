# Agent_Architecture.md - The Structural Map for AI Agent Navigation

## 1. Complete File Overview

#### File Edit Protocol
- **Edit freely**: File is safe to modify when extending the app. All changes MUST follow architectural constraints and update all relevant documentation blocks.
- **Edit carefully**: File is critical or platform-specific. Changes MUST be minimal, justified, and follow all architectural constraints.
- **Do not edit**: File is auto-generated or infrastructure-only. MUST NOT be modified by agents or humans.
- **Tombstone**: File is intentionally empty. MUST NOT be deleted or repurposed.

---

### 1.1 Core Application Files

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `Aeonpulse.csproj` | Edit freely | - | SDK-style multi-targeted project file. Declares `<TargetFrameworks>`, min OS versions, NuGet packages, `<MauiXaml>` build actions, and `<EmbeddedResource>` entries for `.resx` files. Also declares `<WindowsPackageType>None</WindowsPackageType>` (Windows-only) so `MauiImage` assets are copied to the output directory next to the exe for unpackaged execution. Explicit `Microsoft.Graphics.Win2D 1.3.2` reference (Windows-only) for Win2D colour-matrix icon tinting. Add new platform targets, packages, or resource files here. |
| `Aeonpulse.sln` | Do not edit | - | Visual Studio solution file. Managed by IDE. |
| `App.xaml` | Edit freely | - | Application-level `ResourceDictionary` root. Merges `Colors.xaml` then `Styles.xaml` in load order. Merge order matters: Styles references Colors. |
| `App.xaml.cs` | Edit freely | `AppBootstrap` | Application entry point. Reads `Preferences` and calls `ThemeService`, `FontSizeService`, and `MainViewModel.ApplyLanguage()` **before** `InitializeComponent()` so the first rendered frame is already correct. On Windows sets `MainPage = new SplashPage()` (pre-warms Win2D tint cache then navigates to `MainPage`); on all other platforms sets `MainPage = new MainPage()` directly. Contains the `.NET 9` obsolete `MainPage` setter - do not add further usages. |
| `MauiProgram.cs` | Edit freely | `AppBootstrap` | MAUI host builder. Registers OpenSans fonts. Appends `ImageTint.ColorProperty` callbacks to `ImageHandler.Mapper` and `ImageButtonHandler.Mapper` globally. Declares `partial` stubs `ApplyImageTint` and `ApplyImageButtonTint` - implemented per-platform in `TintHelper.cs`. Add new handler mappers or DI registrations here. After `builder.Build()`, calls `AeonLog.Initialise(ILoggerFactory)` to wire the application logging gateway. |

---

### 1.2 Models - `Models/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `TickerData.cs` | Edit freely | `DataTransferObject` | Two-property DTO (`BriefText`, `FullText`) implementing `INotifyPropertyChanged`. All 13 typed result subclasses (see `TickerResults.cs`) inherit from this class. Live tickers mutate `BriefText`/`FullText` in-place every second via property setters so bindings update without replacing the object reference. |
| `TickerResults.cs` | Edit freely | `DataTransferObject` | Defines the 13 typed result subclasses (`TimeJubileesResult`, `CountdownResult`, `LifeOdometerResult`, `AlienAnniversariesResult`, `GalacticCommuteResult`, `PhotonPathResult`, `CosmicStretchResult`, `HumanBirthRankResult`, `BirthRuneResult`, `PersonalYearResult`, `GlobalExhaleResult`, `YourBreathResult`, `VibrantCosmosResult`) each extending `TickerData` with raw computed fields. Also defines the `PhotonPhase` enum, `CellularRefreshResult`, `GlobalCrowdResult`, `LifeLogResult`, `VibrantHumanityResult`, and `VibrantNatureResult`. Linked into `Aeonpulse.Tests` via `<Compile Link=...>`. `LifeOdometerResult` carries two extra init-only properties: `IllustrationSource` (filename of an optional inline image shown in the expanded view) and `HasIllustration` (computed bool, drives `IsVisible` on the `Image` element in `MainPage.xaml`). `TimeJubileesResult` carries `LastJubileeValue`, `LastJubileeUnit`, `LastJubileeDate`, `LastJubileeName`, `NextJubileeName`, `DaysSinceLast`, `DaysTillNext`, `ProgressFraction`, and computed `IsMoreRoomAtBottom` - all used to drive the dynamic timeline graphic in the expanded card view. `AlienAnniversariesResult` carries `MercuryYears`, `MercuryFraction`, `VenusYears`, `VenusFraction`, `EarthYears`, `EarthFraction`, `MarsYears`, `MarsFraction`, `JupiterYears`, `JupiterFraction` - all used to drive the orrery visualization in the expanded card view. `PhotonPathResult` carries `NextStarName`, `NextStarDistance`, `TotalDistancePassed`, `DistanceLeft`, `ProgressFraction`, and `NextStopText` - all used to drive the proportional track visualization in the Photon Path expanded card view. `HumanBirthRankResult` carries `ChartPoints` (raw Year/EverBorn data points for the birth-history curve) and `MarkerYear` (interpolated user birth year) - populated by `CalculationService.BirthRankChartPoints()` and used to drive `BirthRankChartDrawable` in the expanded card view. `LifeLogResult` carries `TotalDays`, `ActivityHours` (Dictionary), `ActivitySlices` (List<LifeLogSlice>: CategoryName, DailyHours, DailyProportion, ColorHex hex string, YearsToday, YearsForecast for each of the 7 ATUS activities). `LifeLogSlice` is a sealed DTO defined in the same file; `ColorHex` is a plain hex string (no MAUI dependency in model). `YourBreathResult` gains `AirVolumeCubicMeters` (AirLiters/1000) and `CubeEdgeMeters` (cbrt(AirVolumeCubicMeters)); used by VolumeCubeDrawable. `GlobalExhaleResult` gains `BaseDateCumCO2Gt`, `TodayCumCO2Gt`, `TotalBudgetGt` (~959 Gt IPCC 1.5-degree ceiling), `DepletionYear` (fractional year, binary-search), `ChartStartYear` (max(1900, baseYear-10)); only populated for post-1900 dates. `GlobalCrowdResult` carries `BasePopulation`, `CurrentPopulation`, `BaseYear`, `CurrentYear` (init-only) and mutable `HoverYear`/`HoverPopulation` (fire PropertyChanged via TickerData base). |
| `Models/FutharkRune.cs` | Edit freely | `DataTransferObject` | Immutable descriptor for one Elder Futhark rune. Carries `Symbol` (Unicode \uXXXX escape), `Name`, `Brief`, `Full` (localised strings from AppResources via FutharkCatalogue.Build()), and `Segments` ((int A, int B)[] of point-index pairs on the 15-point grid that trace the rune glyph shape). Used by `WyrdWebDrawable` and `ApplyWyrdWeb`. |
| `Models/FavoriteTickerItem.cs` | Edit freely | `DataTransferObject` | Live Bookmark tile model for the Favorites section. Implements `INotifyPropertyChanged`. Stores a `Func<string> _titleGetter` and `Func<TickerData> _dataGetter` delegate (not snapshots) so `Title` and `Data` always reflect the ViewModel's current language and recalculated object. `Refresh()` raises `PropertyChanged` for both, called by `MainViewModel.RefreshFavoriteTile()` whenever the matching ticker property changes. `JumpToTickerCommand` and `RemoveFromFavoritesCommand` are wired via constructor callbacks. |
| `Models/FutharkCatalogue.cs` | Edit freely | `DataTransferObject` | Static builder for the 24-rune Elder Futhark catalogue. `Build()` returns fresh `IReadOnlyList<FutharkRune>` with AppResources strings; called each card-open so locale changes reflect immediately. `IndexOf(catalogue, runeName)` finds rune by name. `_static` stores 24 entries as (Symbol \uXXXX, (int A,int B)[] Segs) on a 15-point grid (3 cols x 5 rows, aspect 1:2): glyphs U+16A0=Fehu..U+16DE=Dagaz (Jera=U+16C3, Eihwaz=U+16C7, Sowilo=U+16CB). AppResources lookups deferred to Build() body so language switches always reflect. |
| `TickerCardModel.cs` | Edit freely | `DataTransferObject` | Structural metadata for a ticker card: `Title`, `IconGlyph`, `IsLive`, `IsExpanded`, `HasRefresh`. Not yet wired to a `CollectionView` - reserved for a future refactor that replaces individually-templated XAML blocks. |
| `SubsectionState.cs` | Edit freely | - | Snapshot of a collapsible section: `Title` (used as key) and `IsExpanded`. Defined but not yet actively used for persistence - available for future state-save/restore logic. |

---

### 1.3 Services - `Services/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| ``AeonLog.cs`` | Edit freely | ``DiagnosticsGateway`` | Static logging gateway. Zero MAUI dependencies; also linked into ``Aeonpulse.Tests``. Wired by ``AeonLog.Initialise(ILoggerFactory)`` called from ``MauiProgram`` after ``builder.Build()``. Three ``[Conditional("DEBUG")]`` methods: ``Debug(cat, sub, msg, block?)``, ``Info(cat, sub, msg)``, ``Warn(cat, sub, msg)``. Falls back to ``NullLogger.Instance`` before initialisation. Fetch ``Agent_Ops.md`` and see the *Logging Infrastructure and AeonLog Gateway* section for message-format convention and ``[BLOCK]`` tag rules. |
| `CalculationService.cs` | Edit freely | `CoreCalculationEngine` | The single domain-logic class. Stateless - reads `DateTime.Now` internally so every call produces a fresh result. All 19 ticker methods return typed subclasses of `TickerData` (see `TickerResults.cs`). `FindNearestJubilee`, `FindPreviousJubilee`, `ReduceToSingleDigit`, and `GetRandomTeaseText` live here. All output strings are pulled from `AppResources` at call time, so output automatically reflects the active locale. Thread-safe; called from both the UI thread and the 1-second timer (via `MainThread.BeginInvokeOnMainThread`). All 19 public `Calculate*` methods accept an optional `DateTime? now = null` parameter for deterministic testing - production callers omit it and get `DateTime.Now`. `FindNearestJubilee` and `ReduceToSingleDigit` are `internal static` and accessible to `Aeonpulse.Tests` via `[assembly: InternalsVisibleTo]`. |
| `ThemeService.cs` | Edit freely | - | Singleton (`Instance`). Stores three `Dictionary<string, Color>` palettes: `_defaultColors` (DefaultDark), `_highContrastDarkColors`, `_highContrastLightColors`. `ApplyScheme(string)` iterates the chosen palette and writes each key directly into `Application.Current.Resources`, causing all `DynamicResource` bindings to repaint immediately. To add a new colour scheme: add a new palette dict and a new `const string` identifier, then add a case to the switch in `ApplyScheme`. |
| `FontSizeService.cs` | Edit freely | - | Singleton (`Instance`). Same pattern as `ThemeService` but for five font-size keys (`FontSizeSmall` through `FontSizeTitle`). `ApplyPreset(string)` mutates the resource dict. Three presets: `Small`, `Normal`, `Large`. |

---

### 1.4 ViewModels - `ViewModels/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `MainViewModel.cs` | Edit freely | - | Central state hub. Implements `INotifyPropertyChanged` manually. Owns all ticker result properties, section/card expanded states, settings, all `ICommand` instances, and the 1 Hz timer for live tickers. `SaveDate()` is the ONLY entry point for changing the base date. `UpdateStaticCalculations()` and `UpdateLiveCalculations()` MUST be used for ticker updates. No UI references allowed. |
| `LocalizedResources.cs` | Edit freely | - | Singleton (`Instance`). A thin passthrough wrapper: every property is `=> AppResources.SomeKey`. Bound in XAML as `{Binding Loc.PropertyName}`. `Invalidate()` fires `PropertyChanged(string.Empty)` which causes every bound property to re-read from `AppResources` with the newly-set culture. When adding a new localised string: add the `AppResources` key, then add the passthrough property here. |

---

### 1.5 Views - `Views/`

All XAML files must be saved as **UTF-8 with BOM**. All colour/font-size references must use `DynamicResource`. All user-visible text must bind via `{Binding Loc.Xxx}` or `{x:Static resources:AppResources.Xxx}`.

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `MainPage.xaml` | Edit freely | *(XAML AI comments)* | The application's only persistent page. 3-row root `Grid`: Row 0 = NavBar (`Border` + inner `Grid`, logo `Image` with `ImageTint`, app name `Label`, hamburger `Button`). Row 1 = TimelineHeading (`Border` + `HorizontalStackLayout` with `FormattedString`). Row 2 = `ScrollView` containing four `CardFrame`-styled `Border` elements (Lab, Cosmos, Mirror, Eco Echoes), each holding a header `Grid` and a collapsible `VerticalStackLayout` of ticker card `Border` elements. Uses `BoolToImageSource` converter for chevron icons. |
| `MainPage.xaml.cs` | Edit carefully | `NavigationCoordinator` | Code-behind for `MainPage`. **Contains no business logic.** Responsibilities: subscribe to `MainViewModel.RefreshRequested` in constructor; implement `OnMenuClicked`, `OnTimelineHeadingTapped`, `OnLogoTapped` (opens `TeasePopup` anchored below NavBar, left-aligned, with Copy-to-clipboard and Close buttons); implement 11 `OnXxxInfoClicked` handlers that push `DeepDivePopup`; implement `OnTickerRefreshRequested` that pushes `RefreshingPopup`. Guard flags (`_isXxxOpen`) on every push prevent double-open. `OpenDeepDiveAsync()` measures `NavBar.Height + TimelineHeading.Height` to pass as `topOffset` to `DeepDivePopup`. Holds 23 guard bools (14 deep-dive/popup guards + `_isTeasePopupOpen`). Overrides `OnAppearing`/`OnDisappearing` to start and abort the `"LiveBadgeBreathing"` `Animation` that pulses all 10 LIVE badge labels (opacity 1.0 -> 0.4 -> 1.0, 2500 ms, `Easing.SinInOut`, repeating). `OnAppearing` also starts the Ambient Sparks loop (`StartAmbientSparks`) if `VibrantCosmosExpanded` is true; `OnDisappearing` stops it. `OnViewModelPropertyChanged` starts or stops the loop on `VibrantCosmosExpanded` changes. `ApplyBirthRankChart` sets `BirthRankChart.Drawable` to a fresh `BirthRankChartDrawable` and calls `Invalidate()` when `HumanBirthRankExpanded` or `HumanBirthRank` changes; clears Drawable for pre-1900 dates. `ApplyWyrdWeb` rebuilds the Web of Wyrd Explorer: populates `WyrdRuneGrid` with 24 `Border`+`Label` tap-targets, sets `WyrdWebView.Drawable` to a new `WyrdWebDrawable`, updates `WyrdRuneName`/`WyrdRuneMeaning`; called on `BirthRuneExpanded`, `BirthRune`, `ColorScheme`, `DisplayLanguage` changes and from constructor. `OnWyrdRuneTapped` updates `_wyrdSelectedIndex`, re-highlights grid borders, updates labels, re-invalidates canvas; no ViewModel mutation. `_wyrdCatalogue`/`_wyrdSelectedIndex` are private UI-state fields. `ApplyEnneagram` sets `EnneagramView.Drawable` to a fresh `EnneagramDrawable(personalYearNumber)` and calls `Invalidate()`; called on `PersonalYearExpanded`, `PersonalYear`, `ColorScheme` changes and from constructor. `ApplyTaxonomyFlow`: sets TaxonomyDiscoveriesLabel/TaxonomyExtinctionsLabel text+colour, calls AssignTaxonomyFlowDrawable (creates TaxonomyFlowDrawable with 7 counts + localised InLabels/OutLabels from AppResources, calls Invalidate); triggered on VibrantNatureExpanded/VibrantNature/ColorScheme/DisplayLanguage. |
| `SettingsPopup.xaml` | Edit freely | *(XAML AI comments)* | Full-screen overlay modal (semi-transparent `BackgroundColor`). `Frame` (legacy, `.NET 9` obsolete - do not add more Frames) centred panel. 3-row inner `Grid`: title bar, scrollable settings, close button footer. Settings rendered as a 2-column 14-row `Grid` with custom `RadioButton` `ControlTemplate` (outer ring `Ellipse` + inner dot `Ellipse` driven by `{TemplateBinding IsChecked}`). Groups: Unit System (rows 0-1), Color Scheme (rows 3-5), Text Size (rows 7-9), Language (rows 11-13), with spacer rows between. |
| `SettingsPopup.xaml.cs` | Edit carefully | `ModalViewController` | Accepts `MainViewModel` via constructor; sets `BindingContext`. `_initialising = true` guard blocks `CheckedChanged` callbacks during radio-button seeding. Handlers `OnUnitSystemChanged`, `OnColorSchemeChanged`, `OnTextSizeChanged`, `OnDisplayLanguageChanged` each read the `RadioButton.Value` string and write to the ViewModel setter, which applies the change immediately and persists it. The About section uses a single HTML-formatted `Label` (`AboutTextLabel`) populated from `AppResources.Settings_AboutText`, which merges version, description, tagline, and attribution into one localised string. |
| `ChangeDatePopup.xaml` | Edit freely | *(XAML AI comments)* | Centred (no full-screen overlay) modal. No backdrop-dismiss tap by design - prevents accidental dismissal of an in-progress edit. Contains `Entry` (event name) and `DatePicker` (date) inside `Frame` wrappers (legacy, `.NET 9` obsolete). Cancel and OK `Button` in a 3-column `Grid`. Uses `{x:Static resources:AppResources.Xxx}` (acceptable: popup is freshly constructed each time). |
| `ChangeDatePopup.xaml.cs` | Edit carefully | `ModalViewController` | Pre-populates `EventNameEntry.Text` and `EventDatePicker.Date` from the ViewModel in constructor. `OnOkClicked` validates the name entry is non-empty, then enforces a minimum base date of 1900-01-01: if the selected date is earlier, the picker is reverted and a localized `DisplayAlert` is shown (keys: `Alert_Title_Aeonpulse`, `Alert_Message_Pre1900`, `Alert_Button_Close`); the save is aborted. Otherwise calls `MainViewModel.SaveDate(name, date)` atomically before `PopModalAsync()`. |
| `MainMenuPopup.xaml` | Edit freely | *(XAML AI comments)* | Full-screen overlay. `Frame` (legacy) panel positioned `HorizontalOptions=End` with top/right `Margin` injected in code-behind to sit below the NavBar hamburger button. Menu items are `Grid` + `TapGestureRecognizer` (not `Button`) to avoid nested hit-testing issues on Android. Items: Change Date, Settings, Exit. Close `Button` in footer. |
| `MainMenuPopup.xaml.cs` | Edit carefully | `ModalViewController` | Accepts `MainViewModel`, `topOffset`, `rightOffset`, `openChangeDateCallback`, `openSettingsCallback` via constructor. Each menu item first `await`s `PopModalAsync()` to finish its own dismiss animation, **then** invokes the callback. This ordering is mandatory on iOS to avoid `InvalidOperationException`. Exit calls `Application.Current.Quit()`. |
| `DeepDivePopup.xaml` | Edit freely | *(XAML AI comments)* | Generic info popup reused by all 19 ticker info buttons. Full-screen overlay. `Frame` (legacy) panel with top `Margin` overridden by code-behind. 3-row layout: non-scrollable title, `ScrollView` with two labelled content sections (methodology + sources), footer with close button. All text labels are set by code-behind via `x:Name`. |
| `DeepDivePopup.xaml.cs` | Edit freely | `ModalViewController` | Constructor accepts `title`, `section1Title`, `section1Text`, `section2Title`, `section2Text`, `topOffset`. Sets label text and overrides `PopupFrame.Margin` top component. To add more content sections, add new `Label` elements in the XAML and wire them here. |
| `RefreshingPopup.xaml` | Edit freely | *(XAML AI comments)* | Centred (no full-screen overlay) auto-dismissing overlay. `Frame` (legacy) containing `ActivityIndicator` + message `Label`. No user-dismiss gesture - dismisses automatically after 3 seconds. |
| `RefreshingPopup.xaml.cs` | Edit carefully | `ModalViewController` | Accepts `Action onDismissed` callback. `OnAppearing()` awaits `Task.Delay(3000)`, awaits `PopModalAsync()`, then invokes `onDismissed`. The callback updates a specific typed ticker result property on the ViewModel. The 3-second delay must remain to give the spinner time to animate. |
| `TeasePopup.xaml` | Edit freely | *(XAML AI comments)* | Left-aligned modal panel anchored below the NavBar via `Margin` injection. No fixed width - auto-sizes to content to avoid line-wrapping. Full-screen semi-transparent overlay with backdrop-dismiss tap. 3-row inner layout: title bar with divider, tease stat content label (`x:Name` set by code-behind), right-aligned footer with 2-button row (Copy + Close). Button `TextColor`/`BorderColor` use `{DynamicResource TextWhite}` to match content; `FontSize` uses `{DynamicResource FontSizeLarge}`; `MinimumWidthRequest=140` ensures equal size fitting `To Clipboard` at `FontSize=Large`. |
| `TeasePopup.xaml.cs` | Edit carefully | `ModalViewController` | Constructor accepts `string teaseText`, `double topOffset` (`NavBar.Height`), `double leftOffset` (NavBar left padding = 16), and `Func<string, Task> onCopiedCallback`. Sets `TeasePanel.Margin` top/left from offsets. `OnOkClicked` (also wired to backdrop tap) calls `PopModalAsync()`. `OnCopyClicked` calls `Clipboard.Default.SetTextAsync`, then `PopModalAsync()`, then invokes `onCopiedCallback` which shows a `DisplayAlert` on `MainPage`s navigation context (mandatory iOS pop-before-alert ordering). |
| `SplashPage.xaml` | Edit freely | *(XAML AI comments)* | Windows-only startup splash shown while the Win2D tint cache is pre-warmed. Full-screen `ContentPage` with `SpaceDarker` background. Centred `VerticalStackLayout`: tinted `aeonpulse.png` logo (`helpers:ImageTint.Color={DynamicResource CyberCyan}`) and a `SplashLabel` localised to `AppResources.App_Initializing`. Not instantiated on Android/iOS/macCatalyst (`App.xaml.cs` goes directly to `MainPage` there). |
| `SplashPage.xaml.cs` | Edit carefully | `AppBootstrap` | Code-behind for `SplashPage`. `OnAppearing` fires `RunStartupAsync` (fire-and-forget). On Windows: reads `CyberCyan` from `Application.Current.Resources` (populated by `ThemeService.ApplyScheme` before `InitializeComponent`; must NOT read from `SplashLogo.ImageTint` which is unresolved at `OnAppearing` time), calls `MauiProgram.WarmAllTintCachesAsync(tint)`, then sets `Application.Current.MainPage = new MainPage()`. On non-Windows platforms the `#if WINDOWS` block is absent so `RunStartupAsync` navigates immediately. |

---

#### Wyrd Web Drawable

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `Views/WyrdWebDrawable.cs` | Edit freely | `UIPresentation` | `IDrawable` for the Web of Wyrd visualization (GraphicsView WyrdWebView, 150x300, aspect 1:2). Grid: 15 points p[0]-p[14], 3 cols x 5 rows, stepX=gridW/2 stepY=gridH/4. Always draws 9 skeleton lines (dim thin): 3 verticals p[0]-p[12]/p[1]-p[13]/p[2]-p[14], 3 TL-BR diagonals p[0]-p[8]/p[3]-p[11]/p[6]-p[14], 3 BL-TR diagonals p[6]-p[2]/p[9]-p[5]/p[12]-p[8]. Then draws FutharkRune.Segments as thick accent lines and fills accent dots at segment endpoints. Colours: DefaultDark TextGray 25% skeleton/JubileeAccent accent; HC-Dark white; HC-Light black. Set on WyrdWebView.Drawable by ApplyWyrdWeb. |

#### Volume Cube Drawable

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `Views/TaxonomyFlowDrawable.cs` | Edit freely | `UIPresentation` | `IDrawable` for the Taxonomy Flow (Sankey-style) diagram in VibrantNature expanded card (GraphicsView TaxonomyFlowView, HeightRequest=160). Properties: TotalDiscovered, TotalExtinct, InsectsDiscovered, PlantsDiscovered, VertebratesDiscovered, InsectsExtinct, VertebratesExtinct (all double), InLabels (string[4]), OutLabels (string[3]). OthersDiscovered/OthersExtinct computed in Draw. Width scaling: proportional via MinValue normalisation; factor=100/max(sumIn,sumOut); clamp([4,40]). ComputeWidths() testable static. StackCentres() stacks with StreamGap=20px. Seven streams: fixed hex colours (Yellow/Green/Blue/Aqua inflows; Silver/Olive/Gray outflows) at 70% alpha. Labels drawn above each left/right anchor; scheme-aware label colour via SpaceDarker discriminator. Circle and icon drawn by XAML TaxonomyCircle Border + Image (not in drawable). |
| `Views/VolumeCubeDrawable.cs` | Edit freely | `UIPresentation` | `IDrawable` for the Volumetric Cube visualizer in the Your Breath expanded card (GraphicsView VolumeCubeView, HeightRequest=250). Property: `CubeEdgeMeters` (double). Outputs: `LastPpm` and `LastAnchorY` (written during Draw, read by ApplyVolumeCubeAsync to size the landmark Image). Dynamic scale: ppm = (h*0.80)/(edge*2), clamped [3,280]. Cube centred right-of-centre; left side reserved for landmark image. Draws three iso faces Right/Left/Top at 45/28/60% alpha. No human silhouette or rays. Colours via SpaceDarker discriminator. |

#### Carbon Budget Chart Drawable

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `Views/CarbonBudgetChartDrawable.cs` | Edit freely | `UIPresentation` | `IDrawable` for the 1.5-degree carbon budget chart in the Global Exhale expanded card (GraphicsView CarbonBudgetChartView, HeightRequest=220). Properties: `ChartStartYear`, `DepletionYear`, `TotalBudgetGt`, `BaseDateCumGt`, `TodayCumGt`, `BaseYear`, `TodayYear`. Draws: horizontal gridlines + Y-axis labels; dashed limit line at TotalBudgetGt; solid past curve (ChartStartYear..TodayYear) + dashed future projection (TodayYear..DepletionYear); base-date node (hollow circle, JubileeAccent); today node (filled CyberCyan); depletion node (filled CyberPink at limit intersection); X-axis year labels. Polynomial `CumCO2AtYear` mirrors CalculationService. Colours resolved at draw time via SpaceDarker discriminator (DefaultDark / HC-Dark / HC-Light). |

#### Population Chart Drawable

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `Views/PopulationChartDrawable.cs` | Edit freely | `UIPresentation` | `IDrawable` for the interactive population growth chart in the Global Crowd expanded card (GraphicsView PopulationChartView, HeightRequest=180, HorizontalOptions=FillAndExpand). Dataset: 10 points 1950-2050 (Year, PopBillions). PadLeft=36 for Y-axis labels; PadRight=10, PadTop=10, PadBottom=20. Draws: horizontal gridlines at 2B/4B/6B/8B/10B with labels; population curve as PathF; base-date dot (JubileeAccent filled); current-date dot (CardDark fill + JubileeAccent stroke); scrubber vertical line + interpolated hover dot at ScrubX (set by interaction handler, -1=hidden). `InterpolatePopulation(year)` is internal static for use by MainPage.xaml.cs. Scheme-aware via SpaceDarker. |

#### Enneagram Drawable

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `Views/EnneagramDrawable.cs` | Edit freely | `UIPresentation` | `IDrawable` for the Pythagorean Enneagram visualization (GraphicsView EnneagramView, 250x250). Draws: outer circle; {9/4} nonagram connecting each node to node+4 mod 9 (1-5-9-4-8-3-7-2-6-1); {9/3} equilateral triangle (nodes 3-6-9). Nine node circles with dim fill; number labels 1-9 outside nodes. Active node (PersonalYearNumber 1-9) highlighted with larger filled accent circle and contrasting number label. Node 1 at top (-90 deg), step 40 deg. Colours: DefaultDark TextGray 25% skeleton/TextDim labels/JubileeAccent accent; HC-Dark white; HC-Light black. Set on EnneagramView.Drawable by ApplyEnneagram in MainPage.xaml.cs. |

#### Birth Rank Chart Drawable

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `Views/BirthRankChartDrawable.cs` | Edit freely | `UIPresentation` | `IDrawable` implementation for the Human Birth Rank history curve `GraphicsView`. Linear X-axis [-5000, 2050], linear Y-axis [0, 125B]. Draws: Y-axis labels only (20B..120B, no grid lines); X-axis tick labels (5000 B.C.E., 2500 B.C.E., 1 C.E., 1200, 2022); PRB data polyline (points outside X domain skipped; MoveTo first in-range point so no baseline from x=-5000); left-side annotations with arrows for 1850/1900/1950/2000/2022 (1900 and 2000 +30 px extra offset); filled ellipse at user rank position. Scheme-aware colours: DefaultDark TextGray curve, TextDim labels, JubileeAccent=#FFD700 marker; HC-Dark all pure white; HC-Light all pure black. Scheme detected via SpaceDarker (Black=HC-Dark, White=HC-Light, other=DefaultDark). |

---

### 1.6 Scripts - `Scripts/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `Scripts/setup-android-avd.ps1` | Edit freely | - | One-time developer setup script. Patches every AVD `config.ini` under `%USERPROFILE%\.android\avd\` (or `\$ANDROID_AVD_HOME`) to set `hw.ramSize=4096` and `vm.heapSize=512`. Required because the default 1536 MB emulator RAM causes the Android Low Memory Killer to terminate the app during startup. Idempotent - re-running only changes values that differ from the required ones. Run once after cloning or creating a new AVD; restart the emulator afterwards. |

---

### 1.7 Converters - `Converters/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `ValueConverters.cs` | Edit freely | `UIConverter` | Three converters, all declared in one file. `BoolToVisibilityConverter`: `bool -> bool` passthrough (drives `IsVisible`; `ConvertBack` throws). `InverseBoolConverter`: `bool -> !bool` (drives collapsed-state chevron direction; `ConvertBack` implemented). `BoolToImageSourceConverter`: `bool -> string` filename selected from `ConverterParameter` which must be formatted as `"fileIfTrue.png|fileIfFalse.png"`. |

---

### 1.8 Helpers - `Helpers/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `ImageTint.cs` | Edit carefully | `PlatformAbstractionHelper` | Defines `ImageTint.Color` as a MAUI attached `BindableProperty`. `OnColorChanged` calls `view.Handler?.UpdateValue(nameof(ColorProperty))` which re-invokes the handler mapper registered in `MauiProgram.cs`. Setting `Color=null` clears the tint. Has no effect if `MauiProgram` mappers are not registered. Supports `DynamicResource` - live theme swaps re-invoke the mapper. |
| `TintBehavior.cs` | **Tombstone - do not edit or delete** | - | Empty file kept to prevent stale build artifact errors. The tinting functionality it formerly provided is now in `ImageTint.cs` + platform `TintHelper.cs`. |

---

### 1.9 Resources - `Resources/`

#### String Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/AppResources.resx` | Edit freely | Master English string resource file. Contains all user-visible strings: UI labels, ticker text templates (with `{placeholder}` tokens), star catalogue entries (57 stars), Elder Futhark rune data (24 runes), personal year interpretations (1-9), and tease text. **Every new user-visible string must be added here first.** Currently approximately 486 keys. |
| `Resources/AppResources.ru.resx` | Edit freely | Russian translations. Must contain a matching entry for every key in `AppResources.resx`. Missing keys fall back to English at runtime. |
| `Resources/AppResources.Designer.cs` | **Edit only to add new keys when Designer regeneration is unavailable** | Auto-generated strongly-typed accessor class (`namespace Aeonpulse.Resources`). Normally regenerated from `.resx` by `PublicResXFileCodeGenerator` on build. When a new `.resx` key cannot trigger an immediate Designer regeneration, manually add the corresponding `public static string` property following the existing pattern, then regenerate on the next full build. |

#### Style Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/Styles/Colors.xaml` | Edit carefully | Colour token `ResourceDictionary`. Defines 12 named `Color` keys (startup defaults for `DefaultDark` scheme), 5 `Opacity` doubles, 5 `FontSize` doubles, 3 `Spacing` doubles, and 3 `BorderRadius` doubles. All colour and font-size keys are **overwritten at runtime** by `ThemeService`/`FontSizeService` - never use `StaticResource` for these. The non-colour constants (`Opacity`, `Spacing`, `BorderRadius`) are safe for `StaticResource`. |
| `Resources/Styles/Styles.xaml` | Edit freely | Named `Style` `ResourceDictionary`. Styles: `BaseLabel`, `TitleLabel` (extends `BaseLabel`), `SubtitleLabel` (extends `BaseLabel`), `CyberButton`, `IconButton`, `CardFrame`, `ThemedIconButton`, `ThemedLogoImage`. All colour and font-size references use `DynamicResource`. New styles should extend `BaseLabel` where applicable. |

#### Image Resources

All images are in `Resources/Images/` and are declared as `<MauiImage>` in the `.csproj`. They are compiled into platform-native asset bundles (Android `res/drawable`, iOS asset catalog, etc.). Reference in XAML by filename only (e.g., `Source="info.png"`).

| File | Used by | Purpose |
|------|---------|---------|
| `aeonpulse.png` | NavBar `Image` | App logo, tinted via `ImageTint` |
| `chevron_up.png` / `chevron_down.png` | Section header `ImageButton` | Section expand/collapse toggle icon |
| `square_chevron_up.png` / `square_chevron_down.png` | Ticker card `ImageButton` | Card expand/collapse toggle icon |
| `info.png` | Ticker card `ImageButton` | Opens `DeepDivePopup` |
| `refresh.png` | Ticker card `ImageButton` | Triggers `RefreshingPopup` + recalculation |
| `settings.png` | `MainMenuPopup` menu item | Settings menu item icon |
| `profiles.png` | `MainMenuPopup` menu item | Change Date menu item icon |
| `exit.png` | `MainMenuPopup` menu item | Exit menu item icon |
| `anim_ecg_pulse.gif` | Life Odometer expanded card `Image` | Minimalist animated ECG heartbeat pulse trace shown in the expanded Life Odometer card. `IsAnimationPlaying=True` drives looping playback. Source is a XAML literal (not a binding) to prevent MAUI GIF decoder restart on each 1-second timer tick. Drawn below the `FullText` label as a sibling in the `VerticalStackLayout` (same flat pattern as `anim_countdown.gif`). **Replace placeholder file with a permissively-licensed GIF before release.** |
| `anim_countdown.gif` | Countdown expanded card `Image` | Animated split-flap (Solari) digit counter shown in the expanded Countdown card; `IsAnimationPlaying=True` drives animation. Source is a XAML literal (not a binding) to prevent MAUI GIF decoder restart on each 1-second timer tick. |
| `anim_mitosis.gif` | Cellular Refresh expanded card `Image` | Animated cell-division (mitosis) loop shown in the expanded Cellular Refresh card. `IsAnimationPlaying=True` drives looping playback. Source is a XAML literal (not a binding) to prevent MAUI GIF decoder restart on each timer tick. `IsVisible` bound to `CellularRefreshExpanded`. Drawn below the `FullText` label as a sibling in the `VerticalStackLayout` (same flat pattern as `anim_countdown.gif`). |
| `anim_spacewait.gif` | Space Wait expanded card `Image` | Animated split-flap digit counter shown in the expanded Space Wait card; same layout as `anim_countdown.gif` but uses the Predator TTF Bold 58px font for digit glyphs. `IsAnimationPlaying=True` drives animation. Source is a XAML literal (not a binding) to prevent GIF decoder restart on each 1-second timer tick. |
| `anim_sun_in_milky_way.gif` | Galactic Commute expanded card `Image` | Animated Milky Way galaxy scene shown in the expanded Galactic Commute card. `IsAnimationPlaying=True` drives looping playback. Source is a XAML literal (not a binding) to prevent MAUI GIF decoder restart on each 1-second timer tick. `IsVisible` bound to `GalacticCommuteExpanded`. Drawn below the `FullText` label as a sibling in the `VerticalStackLayout` (same flat pattern as `anim_countdown.gif`). |
| `anim_cosmic_stretch.gif` | Cosmic Stretch expanded card `Image` | Animated universe-expansion scene shown in the expanded Cosmic Stretch card. `IsAnimationPlaying=True` drives looping playback. Source is a XAML literal (not a binding) to prevent MAUI GIF decoder restart on each 1-second timer tick. `IsVisible` bound to `CosmicStretchExpanded`. Drawn below the `FullText` label as a sibling in the `VerticalStackLayout` (same flat pattern as `anim_countdown.gif`). |
| `in_favorites.png` | Favorites tile ImageButton, ticker card Add/Remove button (active state) | Star/bookmark icon shown when a ticker is already in Favorites. Tinted via `ImageTint.Color`. |
| `to_favorites.png` | Ticker card Add/Remove button (inactive state) | Star/bookmark icon shown when a ticker is not yet in Favorites. Tinted via `ImageTint.Color`. |
| `calendar.png`, `menu.png`, `picture.png`, `send.png`, `share.png`, `tease.png`, `text.png` | Unused / reserved | Available for future features |

#### Other Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/AppIcon/appicon.png` | Edit freely | App icon source image used by `<MauiIcon>` in `.csproj`. |
| `Resources/Splash/splash.svg` | Edit freely | Splash screen SVG used by `<MauiSplashScreen>` in `.csproj`. Background colour `#512BD4`. |

---

### 1.10 Attributes - `Attributes/`

| File | Edit? | Description |
|------|-------|-------------|
| `Attributes/AIContextAttribute.cs` | Edit carefully | Defines `[AIContext(string role)]`. `AllowMultiple = true`, `Inherited = false`. Used on classes and methods. Fetch ``Agents.md`` and see the *[PART D] AI MARKUP SCHEMA* section for the full role vocabulary. |

---

### 1.11 Platform-Specific Files - `Platforms/`

#### Android - `Platforms/Android/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `MainActivity.cs` | Edit carefully | `PlatformEntryPoint` | The single Android `Activity`. `MainLauncher = true`. Lists `ConfigurationChanges` to prevent Activity destruction on orientation, dark-mode, density, and screen-size changes. |
| `MainApplication.cs` | Edit carefully | `PlatformEntryPoint` | Android `Application` subclass. Bootstrapped by the Android runtime before `MainActivity`. Delegates `CreateMauiApp()` to `MauiProgram`. |
| `TintHelper.cs` | Edit carefully | `PlatformTintImplementation` | `partial` implementations of `MauiProgram.ApplyImageTint` and `ApplyImageButtonTint` for Android. Uses `PorterDuffColorFilter` with `SrcIn` mode on `ImageView`. For `ImageButton`, the platform view is `ShapeableImageView` (not `android.widget.ImageButton`) - this is a hidden dependency. |
| `AndroidManifest.xml` | Edit carefully | - | Android package manifest. Declares `targetSdkVersion` (currently 33, causes warning vs API-35 target - intentional for compatibility). Contains permissions and activity declarations. |
| `Resources/values/colors.xml` | Edit carefully | - | Android native colour resources required by the Material splash theme. Values: `colorPrimary=#512BD4`, `colorPrimaryDark=#2B0B98`, `colorAccent=#2B0B98`. |

#### iOS - `Platforms/iOS/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `AppDelegate.cs` | Edit carefully | `PlatformEntryPoint` | iOS `MauiUIApplicationDelegate`. Registered with Objective-C runtime via `[Register("AppDelegate")]`. Delegates `CreateMauiApp()` to `MauiProgram`. |
| `Program.cs` | Do not edit | `PlatformEntryPoint` | Static iOS entry point. Calls `UIApplication.Main` with `AppDelegate`. |
| `TintHelper.cs` | Edit carefully | `PlatformTintImplementation` | `partial` implementations for iOS. Sets `UIImageRenderingMode.AlwaysTemplate` + `TintColor` on `UIImageView` (Image) and `UIButton` (ImageButton). To clear: revert to `AlwaysOriginal` and set `TintColor = null`. |
| `Info.plist` | Edit carefully | - | iOS app configuration: bundle ID, display name, supported orientations, privacy descriptions. Edit when adding new OS permissions or changing app metadata. |
| `Resources/PrivacyInfo.xcprivacy` | Edit carefully | - | Apple Privacy Manifest (required by App Store). Declares API usage reasons. Add new entries when using additional Apple privacy-sensitive APIs. |

#### Mac Catalyst - `Platforms/MacCatalyst/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `AppDelegate.cs` | Edit carefully | `PlatformEntryPoint` | Structurally identical to iOS `AppDelegate`. |
| `Program.cs` | Do not edit | `PlatformEntryPoint` | Structurally identical to iOS `Program.cs`. |
| `TintHelper.cs` | Edit carefully | `PlatformTintImplementation` | Same as iOS but adds a `ToUIColor(Color)` helper with `Math.Clamp([0,1])` guards on all components to avoid native API argument exceptions from out-of-range float values generated by programmatic colour construction on the Mac layer. |
| `Info.plist` | Edit carefully | - | Mac Catalyst app configuration. |
| `Entitlements.plist` | Edit carefully | - | Mac App Store sandbox entitlements. `com.apple.security.app-sandbox = true` and `com.apple.security.network.client = true` are required for App Store distribution. |

#### Windows - `Platforms/Windows/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `App.xaml` | Do not edit | - | WinUI 3 application resource root. Mapped to `App.xaml.cs`. Add WinUI-specific resources here if needed. |
| `App.xaml.cs` | Edit carefully | `PlatformEntryPoint` | WinUI 3 `MauiWinUIApplication` subclass. Calls `InitializeComponent()` then delegates `CreateMauiApp()`. Runs **before** `Aeonpulse.App` in the startup sequence. |
| `TintHelper.cs` | Edit carefully | `PlatformTintImplementation` | Real per-pixel colour tinting using Win2D (`Microsoft.Graphics.Canvas`). Both `ApplyImageTint` and `ApplyImageButtonTint`: (1) obtain the source filename from `handler.VirtualView.Source` as a `FileImageSource`, appending `.scale-100` for the Windows resizetizer filename; (2) load the file via `System.IO.File.OpenRead()` + `AsRandomAccessStream()` (avoids WinRT `StorageFile` issues in unpackaged apps); (3) apply a `ColorMatrixEffect` that zeroes source RGB and injects the tint as a constant offset while `M44=1` preserves alpha; (4) render to a `CanvasRenderTarget`, copy to a `WriteableBitmap`, and set it as `Image.Source`. Results are cached by `(filename, colour)`. For `ImageButton`, the inner `Image` is found via `VisualTreeHelper` after `ApplyTemplate()`. If `FindDescendantImage` returns null (buttons in sections collapsed at startup before any layout pass), the tint params are stored in a `ConditionalWeakTable` and a `LayoutUpdated` handler retries on every layout pass until the inner `Image` is found, then calls `AttachAndTint` and unsubscribes. `AttachAndTint` subscribes to `ImageOpened` via `ConditionalWeakTable` so the tint survives every future MAUI source reset. **Hidden dependency:** requires `<WindowsPackageType>None</WindowsPackageType>` so scaled PNGs exist in `AppContext.BaseDirectory` at runtime. |
| `app.manifest` | Do not edit | - | Win32 application manifest. Sets `PerMonitorV2` DPI awareness. |
| `Package.appxmanifest` | Edit carefully | - | MSIX package manifest for Windows Store / sideload deployment. Edit for display name, publisher, capabilities, and version. |

#### Tizen - `Platforms/Tizen/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `Main.cs` | Edit carefully | `PlatformEntryPoint` | Tizen `MauiApplication` entry point. Currently **disabled** in `.csproj` (`TargetFrameworks` comment). Uncomment after installing the Tizen workload to enable. |
| `tizen-manifest.xml` | Edit carefully | - | Tizen package manifest with placeholder app ID and title. Update before enabling the Tizen target. |

---

### 1.12 Documentation Files

| File | Edit? | Description |
|------|-------|-------------|
| `Agents.md` | **Always update on change** | This file. AI Agent navigation guide. Must be kept in sync with all structural changes to the codebase. |

---

### 1.13 Test Project - `Aeonpulse.Tests/`

| File | Edit? | Description |
|------|-------|-------------|
| `Aeonpulse.Tests.csproj` | Edit freely | xUnit test project targeting `net9.0`. Links `CalculationService.cs`, `AeonLog.cs`, `TickerData.cs`, `TickerResults.cs`, `AIContextAttribute.cs`, and `AppResources.Designer.cs` directly from the main project via `<Compile Link=...>` items. Embeds `.resx` files so `ResourceManager` resolves strings at test runtime. No MAUI reference required. |
| `Helpers/TestFixture.cs` | Edit freely | Shared setup helper. `InitEnglish()` pins `AppResources.Culture` to `en` before each test class so string assertions are locale-stable on any CI machine. |
| `FindNearestJubileeTests.cs` | Edit freely | Tests for the `internal static FindNearestJubilee()` algorithm covering all four jubilee families and boundary values. |
| `ReduceToSingleDigitTests.cs` | Edit freely | Tests for the `internal static ReduceToSingleDigit()` digital-root algorithm. |
| `CalculateLifeOdometerTests.cs` | Edit freely | Tests for `CalculateLifeOdometer` with injected `now`. |
| `CalculateCountdownTests.cs` | Edit freely | Tests for all three countdown format branches (HH:MM:SS, days+hours, days-only). |
| `CalculateAlienAnniversariesTests.cs` | Edit freely | Tests for all five planet year calculations (Mercury, Venus, Earth, Mars, Jupiter) and fractional orbital progress values with fixed planetary constants. 17 tests total. |
| `CalculateHumanBirthRankTests.cs` | Edit freely | Tests for the piecewise birth-rank model across pre-1900, 1900-1950, 1950-2000, and post-2000 ranges. |
| `CalculatePersonalYearTests.cs` | Edit freely | Tests for numerology personal year with known input/output pairs. |
| `CalculateGlobalExhaleTests.cs` | Edit freely | Tests for the CO2 polynomial model including metric/imperial toggle and range comparison. |
| `CalculateTimeJubileesTests.cs` | Edit freely | Tests for jubilee selection across all seven time units including the overflow guard for very old dates. |
| `TickerDataTests.cs` | Edit freely | Tests for TickerData INotifyPropertyChanged behaviour (BriefText and FullText fire PropertyChanged; repeated sets; same-value re-sets; default empty strings) and correct round-trip population of the raw init fields on all 11 typed result subclasses. No MAUI dependency - runs in the plain net9.0 test project. |
| `CalculateYourBreathTests.cs` | Edit freely | Tests for `CalculateYourBreath` with injected `now`. Covers breath count formula (14 breaths/min), air volume formula (0.5 L/breath), CO2 formula (1.04 kg/day), metric/imperial CO2 toggle, air volume always in litres regardless of unit system, zero elapsed time, very old dates, proportional growth, and `UseMetric` field round-trip. 16 tests total. |
| `CalculateCellularRefreshTests.cs` | Edit freely | Tests for `CalculateCellularRefresh` with injected `now`. Covers skin cycle formula (27-day period, N0 format), RBC formula (2,000,000/s displayed in billions N2), unit string in BriefText, zero elapsed time, very old dates, proportional growth, and raw field round-trip. 16 tests total. |
| `CalculateVibrantCosmosTests.cs` | Edit freely | Tests for `CalculateVibrantCosmos` with injected `now`. Covers star-born formula (4,800/s), supernova formula (30/s), N0 formatting in BriefText, zero elapsed time, very old dates, proportional growth, and 160:1 star-to-supernova ratio. 11 tests total. |
| `CalculateGlobalCrowdTests.cs` | Edit freely | Tests for `CalculateGlobalCrowd` with injected `now`. Covers all three piecewise segments (pre-1900, 1900-1950, post-1950), epoch anchor values, live-update increment, zero elapsed time, very old dates, N0 formatting (no decimal), and typed result field round-trip. 15 tests total. |
| \CalculateCosmicStretchTests.cs\ | Edit freely | Tests for \CalculateCosmicStretch\ with injected \
ow\. Covers correct km expansion formula (elapsed seconds * 3,300,000), metric billion-km formatting, imperial billion-miles formatting, zero elapsed time, very old dates, and unit-system independence of `KmExpanded`. |
| `TypedResultFieldTests.cs` | Edit freely | Tests that each CalculationService method correctly populates the raw numeric fields of its typed result subclass - not covered by the existing string-assertion tests. Uses injected now for deterministic results. Covers CountdownResult decomposition, LifeOdometerResult formulas, AlienAnniversariesResult planet-year formulas, GalacticCommuteResult km calculation, PhotonPathResult phase, speed-of-light check, and proportional track fields (NextStarName, ProgressFraction, DistanceLeft, NextStopText including half-way-to-Proxima test), HumanBirthRankResult rank ordering, PersonalYearResult range, GlobalExhaleResult flags, TimeJubileesResult coherence. |
| `Aeonpulse.Tests/CalculateSpaceWaitTests.cs` | Edit freely | Tests for `CalculateSpaceWait` with injected `now`.
| `Aeonpulse.Tests/CalculateVibrantHumanityTests.cs` | Edit freely | Tests for `CalculateVibrantHumanity` with injected `now`. Covers births and deaths between base date and now, sub-statistic ratios (twins 2.4%, heart 27%, cancer 18%), zero elapsed time, very old base dates, N0 formatting, token replacement, and consistency with the shared `HumanBirthRankbyDate`/`HumanPopulationByDate` helpers. 16 tests total. | Covers happy path, countdown always less than Mercury period, Mercury birthday countdown formula, zero elapsed time, very old base dates, English ordinal suffix (1st/2nd/3rd/th/11th/21st), BriefText/FullText content, and typed result field round-trip. 14 tests total. |
| `Aeonpulse.Tests/CalculateVibrantNatureTests.cs` | Edit freely | Tests for `CalculateVibrantNature` with injected `now`. Covers species discovery and extinction counts, piecewise epoch boundaries (pre-1950, post-2000), zero elapsed time, very old base dates, N0 formatting, taxonomic sub-statistic proportions (insects 55%/60%, plants 15%, vertebrates 2%), proportional growth, and typed result field round-trip. 19 tests total. |
| `README.md` | Edit freely | High-level project README. Human-facing. Contains a project structure overview and migration notes from the original React implementation. |
| `IMPLEMENTATION_GUIDE.md` | Edit freely | Original React-to-MAUI migration guide. Contains early extension recipes and build commands. Some content is superseded by `Agents.md`. |

---

## 2. Architecture & Patterns


### 2.1 MVVM - Manual Implementation (Strict Protocol)

- MUST use manual MVVM. No CommunityToolkit.Mvvm, Prism, or any other framework. No source generators.
- View layer (`*.xaml`, `*.xaml.cs`) MUST declare UI structure, bind to ViewModel properties/commands, and handle navigation gestures ONLY. No business logic.
- ViewModel (`MainViewModel`) MUST own all application state, expose all `ICommand` instances, fire `PropertyChanged`, and coordinate the timer. MUST NOT reference UI elements or perform UI operations.
- Service layer (`CalculationService`, `ThemeService`, `FontSizeService`) MUST be stateless, contain all domain logic, and have NO UI references. No `INotifyPropertyChanged` in services.
- Model layer (`TickerData`, `TickerResults`, `TickerCardModel`, `SubsectionState`) MUST be pure data containers. `TickerData` is the INPC base; typed subclasses add raw computed fields per ticker.
- All settable properties in `MainViewModel` and `TickerData` MUST use the standard INPC pattern:
  - `public event PropertyChangedEventHandler? PropertyChanged;`
  - `protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));`
- All commands MUST be `System.Windows.Input.Command` instances created inline in the ViewModel constructor. No `RelayCommand`, no `AsyncCommand`.
- `MainViewModel` MUST be instantiated inline in XAML inside `MainPage.xaml`. For popups, the ViewModel MUST be passed as a constructor argument and assigned to `BindingContext` in code-behind.

---

### 2.2 Data Binding

#### Two binding modes in use

| Mode | XAML syntax | Used for |
|------|-------------|---------|
| Live (language-switchable) | `{Binding Loc.PropertyName}` | All labels that must update when the user changes language at runtime |
| Static (construction-time) | `{x:Static resources:AppResources.Key}` | Labels in popups that are always freshly constructed and do not need live language switching |

**Rule:** use `{Binding Loc.Xxx}` on `MainPage`. Use `{x:Static}` only in popup
pages that are created anew each time they open.

#### Converters registered in MainPage.xaml

```xml
<converters:BoolToVisibilityConverter  x:Key="BoolToVisibility" />
<converters:InverseBoolConverter       x:Key="InverseBool" />
<converters:BoolToImageSourceConverter x:Key="BoolToImageSource" />
```

`BoolToImageSourceConverter` requires a pipe-delimited `ConverterParameter`:

```xml
Source="{Binding LabExpanded,
         Converter={StaticResource BoolToImageSource},
         ConverterParameter='chevron_up.png|chevron_down.png'}"
```

Left of `|` = image when value is `true`. Right of `|` = image when value is `false`.

---


### 2.3 Data Flow (Strict Protocol)

- ALL user actions MUST be routed through ViewModel commands. No direct state mutation from Views.
- ALL ticker updates MUST be performed by calling `UpdateStaticCalculations()` (for static tickers) and `UpdateLiveCalculations()` (for live tickers) on the ViewModel.
- The main timer in `MainViewModel` MUST fire at 1 Hz (every second) and update ALL live tickers, including `VibrantCosmos`, at this interval. No ticker may update at a higher frequency unless explicitly documented.
- ALL property changes in the ViewModel MUST fire `OnPropertyChanged` to update XAML bindings.
- The timer thread MUST marshal all property updates to the main thread using `MainThread.BeginInvokeOnMainThread`. NEVER set a bound ViewModel property from a background thread.
- The Live Bookmark (Favorites) pattern MUST be implemented as a live reference to the ticker's BriefText. Tapping a Favorites tile MUST expand the parent section, expand the ticker card, and scroll to it.

---

### 2.4 Localisation Flow

```
USER selects a language in SettingsPopup
    |
    --> OnDisplayLanguageChanged (SettingsPopup.xaml.cs)
            --> _viewModel.DisplayLanguage = "Russian"
                    [MainViewModel.DisplayLanguage setter]
                    |
                    +-- MainViewModel.ApplyLanguage("Russian")  [static method]
                    |       --> CultureInfo.DefaultThreadCurrentCulture   = ru
                    |       --> CultureInfo.DefaultThreadCurrentUICulture = ru
                    |       --> AppResources.Culture = ru
                    |
                    +-- Preferences.Default.Set("DisplayLanguage", "Russian")
                    |
                    +-- Loc.Invalidate()
                    |       --> PropertyChanged("") on LocalizedResources singleton
                    |               --> every {Binding Loc.Xxx} re-reads its getter
                    |                       --> getter returns AppResources.Key
                    |                               (AppResources.Culture is now ru)
                    |                       --> all bound Labels repaint with Russian text
                    |
                    +-- UpdateAllCalculations()
                    |       --> all TickerData strings regenerated from AppResources
                    |               (CalculationService reads AppResources at call time)
                    |
                    +-- OnPropertyChanged(nameof(BaseDateDisplay))
                                --> date reformatted with new CultureInfo.CurrentUICulture
```

**Adding a new localised string requires exactly four steps (in order):**

1. Add key + English value to `Resources/AppResources.resx`
2. Add key + translated value to `Resources/AppResources.ru.resx`
3. Add passthrough property in `ViewModels/LocalizedResources.cs`:
   `public string MyKey => AppResources.MyKey;`
4. Bind in XAML: `{Binding Loc.MyKey}`

---

### 2.5 Theme and Font Size Flow

Both services write directly into `Application.Current.Resources`, causing all
`DynamicResource` bindings across the entire live UI to repaint without any page reload.

```
USER selects colour scheme in SettingsPopup
    |
    --> OnColorSchemeChanged (SettingsPopup.xaml.cs)
            --> _viewModel.ColorScheme = "HighContrastDark"
                    [MainViewModel.ColorScheme setter]
                    |
                    +-- ThemeService.Instance.ApplyScheme("HighContrastDark")
                    |       --> picks _highContrastDarkColors dictionary
                    |       --> foreach key in palette:
                    |               Application.Current.Resources[key] = color
                    |               (all 12 colour keys overwritten in one pass)
                    |               --> every {DynamicResource CyberCyan} etc. repaints
                    |
                    +-- Preferences.Default.Set("ColorScheme", "HighContrastDark")

USER selects text size in SettingsPopup
    |
    --> OnTextSizeChanged (SettingsPopup.xaml.cs)
            --> _viewModel.TextSize = "Large"
                    [MainViewModel.TextSize setter]
                    |
                    +-- FontSizeService.Instance.ApplyPreset("Large")
                    |       --> Application.Current.Resources["FontSizeSmall"]  = 14
                    |       --> Application.Current.Resources["FontSizeMedium"] = 16
                    |       --> Application.Current.Resources["FontSizeLarge"]  = 19
                    |       --> Application.Current.Resources["FontSizeXLarge"] = 23
                    |       --> Application.Current.Resources["FontSizeTitle"]  = 24
                    |               --> every {DynamicResource FontSizeXxx} binding reflows
                    |
                    +-- Preferences.Default.Set("TextSize", "Large")
```

**Startup bootstrap order** - must execute before `InitializeComponent()`:

```
App.xaml.cs constructor
    1. Preferences.Default.Get("ColorScheme")     --> ThemeService.ApplyScheme()
    2. Preferences.Default.Get("TextSize")        --> FontSizeService.ApplyPreset()
    3. Preferences.Default.Get("DisplayLanguage") --> MainViewModel.ApplyLanguage()
    4. InitializeComponent()   [merges Colors.xaml + Styles.xaml -- values already correct]
    5. MainPage = new MainPage()
```

If steps 1-3 ran after `InitializeComponent()`, the first rendered frame would show
wrong colours, font sizes, and language.

---

### 2.6 Cross-Platform Image Tinting

MAUI has no built-in tint API for `Image` or `ImageButton`. The project implements
it as a three-layer pipeline compiled per-platform via multi-targeting:

```
XAML attribute
    helpers:ImageTint.Color="{DynamicResource CyberCyan}"
        |
        v
ImageTint.OnColorChanged  (Helpers/ImageTint.cs)
    BindableProperty.propertyChanged callback
    --> view.Handler?.UpdateValue("ColorProperty")
        |
        v
MauiProgram handler mapper callback  (MauiProgram.cs)
    AppendToMapping on ImageHandler.Mapper / ImageButtonHandler.Mapper
    --> ImageTint.GetColor(bindable)
    --> ApplyImageTint(handler, tint)   [partial method -- resolved at compile time]
        |
        v
Platform TintHelper.cs  (selected at compile time by multi-targeting)
    Android      --> ImageView.SetColorFilter(PorterDuffColorFilter(color, SrcIn))
                     ImageButton PlatformView is ShapeableImageView, not ImageButton
    iOS          --> UIImageView: image.ImageWithRenderingMode(AlwaysTemplate)
                                  nativeImage.TintColor = UIColor
                     UIButton:    same pattern on CurrentImage
    MacCatalyst  --> identical to iOS + Math.Clamp([0,1]) guards on all components
    Windows      --> Image:       Win2D ColorMatrixEffect -> WriteableBitmap -> Image.Source
                     ImageButton: inner Image via VisualTreeHelper, same pipeline
                     Stream load from AppContext.BaseDirectory (not URI)
                     Results cached by (filename, tintColour)
```

When `DynamicResource` changes the tint value (e.g., theme swap), `BindableProperty
.propertyChanged` fires again and the full pipeline re-runs, repainting icon colours
instantly with no extra code required.

**XAML usage pattern:**

```xml
xmlns:helpers="clr-namespace:Aeonpulse.Helpers"

<ImageButton Source="info.png"
             Style="{StaticResource ThemedIconButton}"
             helpers:ImageTint.Color="{DynamicResource CyberCyan}" />
```

---

### 2.7 Modal Navigation Pattern

All secondary UI surfaces are modal pages. One consistent pattern governs all of them.

**Basic push/pop:**

```csharp
// Push - always from MainPage.xaml.cs
await Navigation.PushModalAsync(new SomePopup(viewModel));

// Pop - always from within the popup's own code-behind
await Navigation.PopModalAsync();
```

**Guard flag pattern** - prevents double-push on rapid taps, critical on iOS where
the animation takes longer:

```csharp
private bool _isMainMenuOpen;

private async void OnMenuClicked(object sender, EventArgs e)
{
    if (_isMainMenuOpen) return;
    _isMainMenuOpen = true;
    try   { await Navigation.PushModalAsync(new MainMenuPopup(...)); }
    finally { _isMainMenuOpen = false; }
}
```

Every popup entry point in `MainPage.xaml.cs` has its own named guard bool.

**iOS pop-then-push ordering** - pushing a new modal while a previous one is still
animating out throws `InvalidOperationException` on iOS. `MainMenuPopup` always
awaits `PopModalAsync()` fully before invoking the follow-up navigation callback:

```csharp
// MainMenuPopup.xaml.cs - mandatory ordering on iOS
private async void OnChangeDateClicked(object sender, EventArgs e)
{
    await Navigation.PopModalAsync();    // fully dismiss this popup first
    await _openChangeDateCallback();     // then push the next one
}
```

**topOffset positioning** - `DeepDivePopup` and `MainMenuPopup` appear anchored below
the NavBar. `MainPage` measures rendered element heights at open time and injects the
offset as a constructor argument. The popup overrides its `Frame.Margin` top:

```csharp
// MainPage.xaml.cs - measuring at open time
double topOffset = NavBar.Height + TimelineHeading.Height; // for DeepDivePopup
double topOffset = NavBar.Height;                          // for MainMenuPopup

// DeepDivePopup.xaml.cs constructor
PopupFrame.Margin = new Thickness(24, topOffset, 24, 24);
```

---

### 2.8 Settings Persistence

All user preferences use `Microsoft.Maui.Storage.Preferences` (maps to
`SharedPreferences` on Android, `NSUserDefaults` on iOS/Mac, Registry on Windows).

| Preference key | Type | Default | Read in | Written in |
|----------------|------|---------|---------|------------|
| `"ColorScheme"` | `string` | `"DefaultDark"` | `App.xaml.cs` ctor | `MainViewModel.ColorScheme` setter |
| `"TextSize"` | `string` | `"Normal"` | `App.xaml.cs` ctor | `MainViewModel.TextSize` setter |
| `"DisplayLanguage"` | `string` | `"Default"` | `App.xaml.cs` ctor | `MainViewModel.DisplayLanguage` setter |
| `"UseMetric"` | `bool` | `true` | `MainViewModel` ctor | `MainViewModel.UseMetric` setter |
| `"BaseDateName"` | `string` | `AppResources.Default_BaseDateName` | `MainViewModel` ctor | `MainViewModel.SaveDate()` |
| `"BaseDateValue"` | `string` | `"2000-01-01"` | `MainViewModel` ctor | `MainViewModel.SaveDate()` |
| `"ExpandedStates"` | `string` | 24-char `"0100..."  (Favorites=expanded, all tickers=collapsed) | `MainViewModel` ctor via `LoadExpandedStates()` | every `XxxExpanded` setter via `SaveExpandedStates()` |
| `"FavoriteTickerIds"` | `string` | comma-separated default 5 ticker IDs | `MainViewModel` ctor via `LoadFavorites()` | `AddToFavorites` / `RemoveFromFavorites` / `SaveFavorites()` |

---

## 3. Knowledge Graph Nodes

### 3.1 Node 1: `MauiProgram`
* **File:** `MauiProgram.cs`
* **AIContext:** `AppBootstrap`
* **Responsibilities:** Constructs `MauiApp` host. Registers fonts. Registers global `ImageTint.ColorProperty` handler mappers.
* **Owns:** `MauiApp` instance, global handler mappers.
* **Calls:** `ImageTint.GetColor()`, `ApplyImageTint()`, `ApplyImageButtonTint()`.
* **Called by:** Platform entry points (`MainApplication.cs`, `AppDelegate.cs`, `App.xaml.cs`, `Main.cs`).
* **Extend here when:** Adding NuGet packages requiring builder registration, adding cross-platform handler mappers.

### 3.2 Node 2: `App`
* **File:** `App.xaml`, `App.xaml.cs`
* **AIContext:** `AppBootstrap`
* **Responsibilities:** Applies persisted `Preferences` on first render. Merges global `ResourceDictionary` (`Colors.xaml` then `Styles.xaml`). Sets `MainPage`.
* **Owns:** Application-level `ResourceDictionary`, `MainPage` instance.
* **Calls:** `ThemeService.Instance.ApplyScheme()`, `FontSizeService.Instance.ApplyPreset()`, `MainViewModel.ApplyLanguage()`, `InitializeComponent()`.
* **Called by:** `MauiProgram.CreateMauiApp()`.
* **Extend here when:** Adding persisted preferences that must load before UI inflation.

### 3.3 Node 3: `MainPage`
* **File:** `Views/MainPage.xaml`, `Views/MainPage.xaml.cs`
* **AIContext:** `NavigationCoordinator`
* **Responsibilities:** Sole persistent page. Inflates UI layout. Constructs `MainViewModel` via `BindingContext`. Controls modal push/pop guard flags (`_isXxxOpen`).
* **Owns:** Modal navigation stack, 23 popup guard flags, `RefreshRequested` subscription.
* **Calls:** `Navigation.PushModalAsync()`, `OpenDeepDiveAsync()`, `ScrollToAsync()`.
* **Called by:** `App.xaml.cs`.
* **Extend here when:** Adding a new popup (add guard bool/push method), ticker card info handler, or main gesture event.

### 3.4 Node 4: `MainViewModel`
* **File:** `ViewModels/MainViewModel.cs`
* **AIContext:** *(State Orchestrator)*
* **Responsibilities:** Central state hub. Owns all ticker result properties, UI expansion bools, and settings state. Coordinates 1Hz `System.Timers.Timer`. Marshals to main thread.
* **Owns:** 19 `*Result` properties, 24 `bool` expansion states, `FavoritesCollection`, user settings, all `ICommand` instances, `RefreshRequested` event.
* **Calls:** `CalculationService.Calculate*()`, `ThemeService.Instance.ApplyScheme()`, `FontSizeService.Instance.ApplyPreset()`, `MainThread.BeginInvokeOnMainThread()`, `Preferences.Default.Set()`.
* **Called by:** `MainPage.xaml` (BindingContext), `SettingsPopup.xaml.cs`, `ChangeDatePopup.xaml.cs`.
* **Extend here when:** Adding a new ticker (add typed property, bool, toggle command), new section, or new setting.
* **Critical:** `SaveDate()` is the ONLY entry point for updating the base date.

### 3.5 Node 5: `CalculationService`
* **File:** `Services/CalculationService.cs`
* **AIContext:** `CoreCalculationEngine`
* **Responsibilities:** Sole domain logic provider. Stateless. Uses `DateTime.Now` internally. Populates result DTOs with raw math and localized `AppResources` strings.
* **Owns:** 19 `Calculate*()` methods, 57-star inline catalogue, PRB/Carbon dataset constants.
* **Calls:** `AppResources.*`, `AeonLog.Debug()`.
* **Called by:** `MainViewModel.UpdateStaticCalculations()`, `MainViewModel.UpdateLiveCalculations()`.
* **Extend here when:** Adding domain calculations or updating scientific dataset constants.

### 3.6 Node 6: `LocalizedResources`
* **File:** `ViewModels/LocalizedResources.cs`
* **AIContext:** *(Localization Hub)*
* **Responsibilities:** Singleton bridge between `AppResources` and XAML `{Binding Loc.Xxx}`. 
* **Owns:** `Invalidate()` method (fires `PropertyChanged(string.Empty)` for mass UI rebind).
* **Calls:** `AppResources.SomeKey`.
* **Called by:** XAML `{Binding Loc.Xxx}`, `MainViewModel.DisplayLanguage` setter.
* **Extend here when:** Adding any new localized string (add passthrough property).

### 3.7 Node 7: `AppResources.resx`
* **File:** `Resources/AppResources.resx`, `AppResources.ru.resx`, `AppResources.Designer.cs`
* **AIContext:** *(String Repository)*
* **Responsibilities:** Single source of truth for user-visible strings. Uses `{placeholder}` tokens for `string.Replace()`.
* **Key Prefixes:** `Star_`, `Rune_`, `Ticker_`, `Info_`, `Settings_`, `Unit_`, `PersonalYear_`, `ChangeDate_`, `Tease_`, `MainMenu_`, `Section_`, `Favorites_`, `Alert_`.
* **Called by:** `CalculationService`, `LocalizedResources`, XAML `{x:Static}`.
* **Extend here when:** Adding user-facing text. Must update both `en` and `ru` files.

### 3.8 Node 8: `ThemeService` & `FontSizeService`
* **File:** `Services/ThemeService.cs`, `Services/FontSizeService.cs`
* **AIContext:** *(Resource Mutators)*
* **Responsibilities:** Singletons that manage and inject color/font variables directly into `Application.Current.Resources`.
* **Owns:** Hardcoded palette dictionaries (`_defaultColors`, `_highContrastDarkColors`, etc.) and size presets (`_small`, `_normal`, etc.).
* **Calls:** `Application.Current.Resources[key] = value`.
* **Called by:** `App.xaml.cs`, `MainViewModel` settings setters.
* **Extend here when:** Adding a new color scheme, new font preset, or new globally-tracked UI token.

### 3.9 Node 9: `ImageTint` Pipeline
* **File:** `Helpers/ImageTint.cs`, `Platforms/{Platform}/TintHelper.cs`
* **AIContext:** `PlatformAbstractionHelper`, `PlatformTintImplementation`
* **Responsibilities:** Cross-platform icon tinting via `helpers:ImageTint.Color` attached property.
* **Platform Implementations:** * *Android:* `PorterDuffColorFilter`.
  * *iOS/Mac:* `AlwaysTemplate` + `TintColor`.
  * *Windows:* Win2D `ColorMatrixEffect` (requires `WindowsPackageType=None`).
* **Called by:** XAML `DynamicResource` bindings, `MauiProgram` handler mappers.
* **Extend here when:** Fixing native tint bugs or adding support for a new OS target.

### 3.10 Node 10: Modal Stack Popups
* **File:** `Views/*Popup.xaml`, `Views/*Popup.xaml.cs`
* **AIContext:** `ModalViewController`
* **Responsibilities:** Secondary UI interactions. Pushed via `PushModalAsync`. Popped internally via `PopModalAsync`. Contains no domain logic.
* **Owns:** Code-behind logic mapping gestures to ViewModel commands or callbacks.
* **Called by:** `MainPage.xaml.cs`.
* **Extend here when:** Adding settings options (`SettingsPopup`), menu items (`MainMenuPopup`), or building an entirely new modal surface.

---
