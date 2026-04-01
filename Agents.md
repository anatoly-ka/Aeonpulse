# Agents.md - AI Agent Navigation Guide for Aeonpulse

> **Last updated:** 2026-04-02
> **Maintained by:** AI Agents and human developers collaboratively.
> **Rule:** Update this file and all appropriate markup blocks upon each change.

---

## Table of Contents & Cross-Reference Index

> Quick-navigation for AI agents. Jump directly to the section for your task.
> Section numbers are stable - do not renumber sections without updating this table.

---

### By Section Number

| Section | Title | Primary use |
|---------|-------|-------------|
| **1** | Project Overview | First read. App purpose, quick-reference facts, architecture constraints, ticker table, settings table. |
| **1.1** | Quick-Reference Facts | Namespace, App ID, TFMs, min OS, MVVM approach, NuGet packages, languages, encoding. |
| **1.2** | Application Structure | Page/popup hierarchy, `MainPage` 3-row grid layout. |
| **1.3** | Ticker Cards | All 10 tickers: section, update type, refresh button. |
| **1.4** | User-Configurable Settings | All settings: type, options, `Preferences` persistence key. |
| **1.5** | Key Architectural Constraints | 8 hard rules every agent must read before making any change. |
| **2** | Complete File Overview | Every file in the project: edit guidance, `AIContext` role, description. |
| **2 / Core** | Core Files | `Aeonpulse.csproj`, `App.xaml(.cs)`, `MauiProgram.cs`. |
| **2 / Models** | Models | `TickerData`, `TickerCardModel`, `SubsectionState`. |
| **2 / Services** | Services | `CalculationService`, `ThemeService`, `FontSizeService`. |
| **2 / ViewModels** | ViewModels | `MainViewModel`, `LocalizedResources`. |
| **2 / Views** | Views | All 5 popup XAML/code-behind pairs + `MainPage`. |
| **2 / Converters** | Converters | `ValueConverters.cs` (3 converters). |
| **2 / Helpers** | Helpers | `ImageTint.cs`, `TintBehavior.cs` (tombstone). |
| **2 / Resources** | Resources | `.resx` files, `Colors.xaml`, `Styles.xaml`, images. |
| **2 / Platforms** | Platform Files | Android, iOS, MacCatalyst, Windows, Tizen per-platform files. |
| **3** | Architecture & Patterns | MVVM, data binding, all major data-flow diagrams. |
| **3.1** | MVVM - Manual Implementation | `INotifyPropertyChanged` pattern, `ICommand` pattern, `BindingContext` wiring. |
| **3.2** | Data Binding | Two binding modes (`{Binding Loc.Xxx}` vs `{x:Static}`), all three converter registrations. |
| **3.3** | Data Flow | Full ASCII diagram: user gestures -> ViewModel -> calculations -> UI repaint. |
| **3.4** | Localisation Flow | Full ASCII diagram: language switch -> `ApplyLanguage` -> `Invalidate` -> repaint. 4-step recipe. |
| **3.5** | Theme and Font Size Flow | Full ASCII diagram: settings change -> `ThemeService`/`FontSizeService` -> `DynamicResource` repaint. Startup bootstrap order. |
| **3.6** | Cross-Platform Image Tinting | Full ASCII diagram: `ImageTint.Color` -> mapper -> `TintHelper` -> native API. Per-platform notes. |
| **3.7** | Modal Navigation Pattern | Push/pop pattern, guard flag pattern, iOS pop-then-push ordering, `topOffset` positioning. |
| **3.8** | Settings Persistence | `Preferences` key table: read location, write location, default value. All 4 settings persisted. |
| **4** | AI Markup Schema | All three markup systems: `[AIContext]`, `<!-- AI: -->`, `///`. Syntax contracts. |
| **4.1** | `[AIContext]` Attribute | Complete role vocabulary table (17 roles). Placement rules. What to apply when adding new code. |
| **4.2** | XAML `<!-- AI: -->` Comments | Syntax contract, two styles, inventory of all current comments by file/line. When to add new ones. |
| **4.3** | XML Doc Comments (`///`) | Tags in use, standard summary structure, real examples. Placement rules. |
| **4.4** | Markup Consistency Rules | The Four-Sync Rule, ASCII-only constraint, cross-file consistency rules. |
| **5** | Knowledge Graph Entry Points | Root node map and 10 detailed node entries (responsibilities, owns, calls, called-by, extend-here). |
| **5 / Node 1** | `MauiProgram` | App host builder, font registration, handler mapper registration. |
| **5 / Node 2** | `App` | Startup bootstrap order, preferences restoration, resource dictionary merge. |
| **5 / Node 3** | `MainPage` | Navigation coordinator, guard flags, `OpenDeepDiveAsync`, `topOffset` measurement. |
| **5 / Node 4** | `MainViewModel` | Central state hub, all owned symbols, all callers. Extend here for new tickers/sections/settings. |
| **5 / Node 5** | `CalculationService` | All 11 ticker methods with AIContext and update type. Extend here for new tickers. |
| **5 / Node 6** | `LocalizedResources` | Live binding bridge, `Invalidate()` mechanism. Extend here for new strings. |
| **5 / Node 7** | `AppResources.resx` | 365 string keys, prefix group table, template token format. Extend here for new strings/languages. |
| **5 / Node 8** | `ThemeService` + `FontSizeService` | Palette/preset dictionaries, `ApplyScheme`/`ApplyPreset`. Extend here for new schemes/presets. |
| **5 / Node 9** | `ImageTint` + `TintHelper` | Tint pipeline, per-platform notes. Extend here for new platform tint support. |
| **5 / Node 10** | Modal Popup Classes | All 5 popups: constructor args, primary action, side effects. Extend here for new popups/settings. |
| **6** | Development Workflow | All build, test, run, publish, and maintenance commands. |
| **6.1** | Prerequisites | SDK version, workloads, Xcode, Android SDK requirements. |
| **6.2** | Restore | `dotnet restore` command. Package list. |
| **6.3** | Build Commands | All platforms, Debug/Release, `--no-incremental`. Output path table. |
| **6.4** | Known Warnings | 4 accepted warning codes (XC0022, CS0618, CS8767, CS0414). Clean-build rule for agents. |
| **6.5** | Testing | No test project yet. What can/cannot be tested without a device. Create-test-project recipe. |
| **6.6** | Run and Deploy | Per-platform run commands, ADB path, APK install command, iOS simulator targeting. |
| **6.7** | Publish | Release package commands for all 4 platforms including keystore signing. |
| **6.8** | Clean Build | BOM detection and bulk-fix PowerShell scripts. Full clean commands. |
| **6.9** | AppResources Designer Regeneration | When and how to regenerate `AppResources.Designer.cs`. |
| **6.10** | Source Control | Repo URL, branch, standard Git workflow, `.gitignore` notes. |
| **6.11** | Adding a New Language (Build Only) | `.csproj` `EmbeddedResource` entry, `ApplyLanguage` wiring, build verification. |
| **6.12** | Enabling the Tizen Target | Workload install, `.csproj` uncomment, manifest update, build command. |
| **7** | How to Extend | Step-by-step recipes for all extension types. |
| **7.1** | Adding a New Section | 5-step recipe: `.resx` x2, `LocalizedResources`, `MainViewModel`, `MainPage.xaml`. |
| **7.2** | Adding a New Ticker Card | 8-step recipe: `.resx` x2, `LocalizedResources`, `CalculationService`, `MainViewModel`, `MainPage.xaml`, `MainPage.xaml.cs`, `Aeonpulse.Tests`. |
| **7.3** | Adding a New Colour Scheme | 7-step recipe: `ThemeService`, `.resx` x2, `LocalizedResources`, `SettingsPopup.xaml`, `SettingsPopup.xaml.cs`. |
| **7.4** | Adding a New Language | 9-step recipe: `.resx` x3, `.csproj`, `MainViewModel`, `LocalizedResources`, `SettingsPopup.xaml`, `SettingsPopup.xaml.cs`. |
| **7.5** | Adding a New Font Size Preset | 2-step recipe + reference to 7.3 pattern for settings UI wiring. |
| **7.6** | Files-Changed Checklist | Matrix of extension type vs file - which files to touch for each recipe. |
| **8** | Debugging | Logging infrastructure, instrumentation points, per-platform log viewing. |
| **8.1** | Logging Infrastructure and `AeonLog` Gateway | `AeonLog` static gateway, message-format convention, `[BLOCK]` tag rules. `AddDebug()` wiring in `MauiProgram`. |
| **8.2** | Enabling Debug Logging | Option A (resolve from `IPlatformApplication`), Option B (DI injection). Log level table. |
| **8.3** | Recommended Instrumentation Points | 5 specific locations: startup, `SaveDate`, timer, localisation, tint pipeline. |
| **8.4** | Viewing Logs by Platform | Windows Output pane, Android `adb logcat`, iOS Xcode Console, binding error logging. |
| **8.5** | Diagnosing Common Failure Modes | 4 symptoms: empty tickers, stopped live tickers, theme not applying, language not switching. |
| **8.6** | Debug vs Release Differences | Table of 8 behavioural differences. `Debug.WriteLine` no-op note. |
| **8.7** | Hot Reload and Live Visual Tree | Hot Reload limitations, Live Visual Tree usage for `topOffset` debugging. |
| **8.8** | Attaching the Debugger | Android attach-to-process, Windows attach-to-process. |
| **9** | Guardrails & Style | **Read before every change.** DO/DO NOT rules by concern. |
| **9.1** | Architecture | 8 DOs + 7 DO NOTs for navigation, state, threading, DI. |
| **9.2** | MVVM Pattern | Manual INPC, `Command` construction, `BindingContext` rules. |
| **9.3** | Colours and Theming | `DynamicResource` mandate, 12 key names, tinting, no hardcoding. |
| **9.4** | Font Sizes | `DynamicResource` mandate, 5 key names, no hardcoded values. |
| **9.5** | Localisation | Binding mode rules, `.resx` symmetry, `Loc.Invalidate()` contract. |
| **9.6** | XAML Structure and Encoding | BOM requirement, `Border` not `Frame`, comment rules, `TapGestureRecognizer` pattern. |
| **9.7** | Comment Style and Non-ASCII | `///` on all public symbols, ASCII-only in comments, no `///` in XAML. |
| **9.8** | New NuGet Packages | 3 existing packages, no-internet-copy rule, no Compatibility types. |
| **9.9** | Platform-Specific Code | `partial` methods only, all 4 platforms, no `#if` in shared files, tombstone rule. |
| **9.10** | Persistence | `Preferences` only, 4 persisted keys, read-before-`InitializeComponent` rule. |
| **9.11** | `Agents.md` Maintenance | When and what to update, change-type table, date update rule. |
| **9.12** | Quick Violation Checklist | 20-item YES/NO checklist. Run before every commit. |
| **9.13** | Commit Signature | AI agent signature trailer format, ``+ manual changes`` rule, mandatory date verification before commit. |

---

### By Topic (Task-Oriented Lookup)

| Task / Question | Go to |
|-----------------|-------|
| What files exist and what do they do? | **§2** |
| What are the hard rules I must not break? | **§1.5** then **§9** |
| How does the app start up? | **§3.5** (bootstrap order), **§5/Node 2** |
| How does data flow from user tap to screen repaint? | **§3.3** |
| How does language switching work end-to-end? | **§3.4** |
| How does theme switching work end-to-end? | **§3.5** |
| How does image tinting work? | **§3.6**, **§5/Node 9** |
| How are modals pushed and dismissed? | **§3.7**, **§5/Node 10** |
| Where is user data persisted? | **§3.8**, **§9.10** |
| Which `[AIContext]` role should I use? | **§4.1** |
| Where should I add an `<!-- AI: -->` comment? | **§4.2** |
| How do I write a `///` doc comment? | **§4.3** |
| How do I build for a specific platform? | **§6.3** |
| Why is the build producing warnings? | **§6.4** |
| How do I run on Android / Windows / iOS? | **§6.6** |
| How do I publish a release build? | **§6.7** |
| XAML build fails with MSB4018 / XamlCTask | **§6.8** |
| How do I add a new collapsible section? | **§7.1** |
| How do I add a new ticker card? | **§7.2** |
| How do I add a new colour scheme? | **§7.3** |
| How do I add a new language? | **§7.4** (full recipe), **§6.11** (build steps only) |
| How do I add a new font size preset? | **§7.5** |
| Which files change for a given extension? | **§7.6** |
| How do I see log output on Android? | **§8.4** |
| Ticker cards show empty or stale text | **§8.5** |
| Live tickers have stopped updating | **§8.5** |
| Theme or font size change has no effect | **§8.5** |
| Language change does not update all strings | **§8.5** |
| Icons are not tinted | **§8.5** |
| Can I use `StaticResource` for this colour? | **§9.3** |
| Can I use `StaticResource` for this font size? | **§9.4** |
| Where do I put a new user-visible string? | **§9.5**, **§3.4** |
| Should I use `Frame` or `Border`? | **§9.6** (always `Border`) |
| Can I put an emoji in a XAML comment? | **§9.7** (no) |
| Can I add a NuGet package? | **§9.8** |
| Where does platform-specific code go? | **§9.9** |
| What is `TintBehavior.cs` and why is it empty? | **§2/Helpers**, **§9.9** |
| What must I update in `Agents.md` after my change? | **§9.11** |
| Pre-commit checklist | **§9.12** |
| Commit signature format for AI agents | **§9.13** |

---

## 1. Project Overview

### Purpose

**Aeonpulse** is a cross-platform .NET 9 MAUI application. It presents a single-page
temporal dashboard: the user supplies a **base date** (label + date, typically a birthday)
and the app continuously transforms it into eleven richly-contextualised "ticker cards" grouped
into four collapsible sections. Perspectives range from scientific (photon travel distance,
galactic commute, CO2 emitted) to personal (numerology, birth rune, life odometer) and
astronomical (alien-planet ages, countdown to next anniversary).

### Quick-Reference Facts for AI Agents

| Property | Value |
|----------|-------|
| Root namespace | `Aeonpulse` |
| App ID | `com.aeonpulse.app` |
| Target frameworks | `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net9.0-windows10.0.19041.0` |
| Min OS versions | Android API 21, iOS 11.0, macCatalyst 13.1, Windows 10.0.17763.0 |
| MVVM approach | **Manual** `INotifyPropertyChanged` + `System.Windows.Input.ICommand`. No CommunityToolkit.Mvvm. |
| DI container | Not used for services. `MauiProgram` registers fonts and `ImageTint` handler mappers only. |
| Services pattern | Direct singleton instantiation: `new CalculationService()`, `ThemeService.Instance`, `FontSizeService.Instance` |
| NuGet packages | `Microsoft.Maui.Controls 9.0.0`, `Microsoft.Maui.Controls.Compatibility 9.0.0`, `Microsoft.Extensions.Logging.Debug 9.0.0`, `Microsoft.Graphics.Win2D 1.3.2` (Windows only) |
| Supported languages | English (`en`, default), Russian (`ru`) |
| Neutral language | `en` |
| String storage | `Resources/AppResources.resx` (en) + `Resources/AppResources.ru.resx` (ru) |
| XAML file encoding | **UTF-8 with BOM required** (MSB4018 / XamlCTask crashes without it) |
| Test project | `Aeonpulse.Tests` (`net9.0` xUnit, plain .NET - no MAUI) |

### Application Structure

The app has **one page** (`MainPage`). All other UI surfaces are modal pages pushed onto
the navigation stack:

```
MainPage (always present)
  |-- SettingsPopup      (hamburger menu -> Settings)
  |-- ChangeDatePopup    (TimelineHeading tap, or hamburger menu -> Change Date)
  |-- MainMenuPopup      (hamburger button)
  |-- DeepDivePopup      (info button on any ticker card - generic, reused 10 times)
  |-- RefreshingPopup    (auto-dismissed 3s overlay shown during manual ticker refresh)
  |-- TeasePopup         (logo or app-name tap - shows a live stat from MainViewModel.TeaseText; Copy button copies text to OS clipboard)
```

`MainPage` layout is a 3-row grid:
- **Row 0** - NavBar (logo, app name, hamburger button)
- **Row 1** - TimelineHeading (base date label, tappable to open `ChangeDatePopup`)
- **Row 2** - `ScrollView` containing four collapsible section cards, each containing ticker cards

### Ticker Cards (11 total)

Each ticker card has a `BriefText` (shown when collapsed) and a `FullText` (shown when expanded). The two are mutually exclusive: `BriefText` binds `IsVisible` to `{Binding XxxExpanded, Converter={StaticResource InverseBool}}`; `FullText` binds `IsVisible` to `{Binding XxxExpanded}`.
LIVE tickers update every second via a `System.Timers.Timer` in `MainViewModel`.

| # | Ticker | Section | Update | Has Refresh Button |
|---|--------|---------|--------|--------------------|
| 1 | Time Jubilees | Lab | Static | Yes |
| 2 | Countdown | Lab | **LIVE** | No |
| 3 | Life Odometer | Lab | **LIVE** | No |
| 4 | Alien Anniversaries | Cosmos | Static | Yes |
| 5 | Galactic Commute | Cosmos | **LIVE** | No |
| 6 | Photon Path | Cosmos | **LIVE** | No |
| 7 | Cosmic Stretch | Cosmos | **LIVE** | No |
| 8 | Human Birth Rank | Mirror | Static | No |
| 9 | Birth Rune | Mirror | Static | No |
| 10 | Personal Year | Mirror | Static | No |
| 11 | Global Exhale | Eco Echoes | Static | Yes |
| 12 | Your Breath | Eco Echoes | **LIVE** | No |
| 13 | Cellular Refresh | Lab | Static | Yes |
| 14 | Vibrant Cosmos | Cosmos | **LIVE** (200 ms) | No |
| 15 | Global Crowd | Mirror | **LIVE** | No |
| 16 | Life Log | Mirror | Static | Yes |
| 17 | Space Wait | Cosmos | **LIVE** | No |
| 18 | Vibrant Humanity | Mirror | **LIVE** | No |
| 19 | Vibrant Nature | Eco Echoes | Static | Yes |

### User-Configurable Settings (persisted via `Preferences`)

| Setting | Type | Options | Persistence Key |
|---------|------|---------|----------------|
| Base date name | `string` | Free text | *(ViewModel field, not Preferences)* |
| Base date value | `string` (ISO-8601) | Date picker | *(ViewModel field, not Preferences)* |
| Unit system | `bool UseMetric` | Metric / Imperial | `"UseMetric"` |
| Colour scheme | `string ColorScheme` | `DefaultDark`, `HighContrastDark`, `HighContrastLight` | `"ColorScheme"` |
| Text size | `string TextSize` | `Small`, `Normal`, `Large` | `"TextSize"` |
| Display language | `string DisplayLanguage` | `Default`, `English`, `Russian` | `"DisplayLanguage"` |

### Key Architectural Constraints for AI Agents

1. **No business logic in code-behind.** `*.xaml.cs` files contain only modal navigation
   and event-to-command bridging. All computation lives in `CalculationService`; all state
   lives in `MainViewModel`.

2. **All colours via `DynamicResource`.** `ThemeService` mutates `Application.Current.Resources`
   at runtime. Any `StaticResource` reference to a colour key will **not** respond to theme changes.

3. **All font sizes via `DynamicResource`.** `FontSizeService` mutates font-size resource keys.
   Same rule applies.

4. **All user-visible strings via `AppResources`.** Hardcoded strings break localisation.
   XAML binds via `{Binding Loc.Xxx}` (live-switching) or `{x:Static resources:AppResources.Xxx}`
   (static, acceptable only in popups that are always freshly constructed).

5. **ASCII-only in comment blocks.** Non-ASCII characters (emoji, Unicode dashes, box-drawing)
   inside `<!-- -->` (XAML) or `//` / `/* */` (C#) comment blocks cause `MSB4018 XamlCTask`
   encoding failures if the file lacks a BOM, and are banned regardless. Non-ASCII is safe only
   in element attribute values (e.g., `Text="emoji"`).

6. **`Frame` is obsolete in .NET 9.** Use `Border` for all new container elements.

7. **`Application.MainPage` setter is obsolete in .NET 9.** Do not add new usages.

8. **`SaveDate()` is the only correct way to update the base date.** It sets all three
   backing fields (`_baseDateName`, `_baseDateValue`, `_baseDate`) atomically before
   calling `UpdateAllCalculations()` once, avoiding stale intermediate recalculations.

---

## 2. Complete File Overview

> Legend: **Edit freely** = safe to modify when extending the app. **Do not edit** = auto-generated
> or infrastructure-only. **Tombstone** = intentionally empty, do not delete.

---

### Core Application Files

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `Aeonpulse.csproj` | Edit freely | - | SDK-style multi-targeted project file. Declares `<TargetFrameworks>`, min OS versions, NuGet packages, `<MauiXaml>` build actions, and `<EmbeddedResource>` entries for `.resx` files. Also declares `<WindowsPackageType>None</WindowsPackageType>` (Windows-only) so `MauiImage` assets are copied to the output directory next to the exe for unpackaged execution. Explicit `Microsoft.Graphics.Win2D 1.3.2` reference (Windows-only) for Win2D colour-matrix icon tinting. Add new platform targets, packages, or resource files here. |
| `Aeonpulse.sln` | Do not edit | - | Visual Studio solution file. Managed by IDE. |
| `App.xaml` | Edit freely | - | Application-level `ResourceDictionary` root. Merges `Colors.xaml` then `Styles.xaml` in load order. Merge order matters: Styles references Colors. |
| `App.xaml.cs` | Edit freely | `AppBootstrap` | Application entry point. Reads `Preferences` and calls `ThemeService`, `FontSizeService`, and `MainViewModel.ApplyLanguage()` **before** `InitializeComponent()` so the first rendered frame is already correct. Sets `MainPage = new MainPage()`. Contains the `.NET 9` obsolete `MainPage` setter - do not add further usages. |
| `MauiProgram.cs` | Edit freely | `AppBootstrap` | MAUI host builder. Registers OpenSans fonts. Appends `ImageTint.ColorProperty` callbacks to `ImageHandler.Mapper` and `ImageButtonHandler.Mapper` globally. Declares `partial` stubs `ApplyImageTint` and `ApplyImageButtonTint` - implemented per-platform in `TintHelper.cs`. Add new handler mappers or DI registrations here. After `builder.Build()`, calls `AeonLog.Initialise(ILoggerFactory)` to wire the application logging gateway. |

---

### Models - `Models/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `TickerData.cs` | Edit freely | `DataTransferObject` | Two-property DTO (`BriefText`, `FullText`) implementing `INotifyPropertyChanged`. All 13 typed result subclasses (see `TickerResults.cs`) inherit from this class. Live tickers mutate `BriefText`/`FullText` in-place every second via property setters so bindings update without replacing the object reference. |
| `TickerResults.cs` | Edit freely | `DataTransferObject` | Defines the 13 typed result subclasses (`TimeJubileesResult`, `CountdownResult`, `LifeOdometerResult`, `AlienAnniversariesResult`, `GalacticCommuteResult`, `PhotonPathResult`, `CosmicStretchResult`, `HumanBirthRankResult`, `BirthRuneResult`, `PersonalYearResult`, `GlobalExhaleResult`, `YourBreathResult`, `VibrantCosmosResult`) each extending `TickerData` with raw computed fields. Also defines the `PhotonPhase` enum, `CellularRefreshResult`, `GlobalCrowdResult`, `LifeLogResult`, `VibrantHumanityResult`, and `VibrantNatureResult`. Linked into `Aeonpulse.Tests` via `<Compile Link=...>`. `LifeOdometerResult`, `TimeJubileesResult`, and `CountdownResult` each carry two extra init-only properties: `IllustrationSource` (filename of an optional inline image shown in the expanded view) and `HasIllustration` (computed bool, drives `IsVisible` on the `Image` element in `MainPage.xaml`). |
| `TickerCardModel.cs` | Edit freely | `DataTransferObject` | Structural metadata for a ticker card: `Title`, `IconGlyph`, `IsLive`, `IsExpanded`, `HasRefresh`. Not yet wired to a `CollectionView` - reserved for a future refactor that replaces individually-templated XAML blocks. |
| `SubsectionState.cs` | Edit freely | - | Snapshot of a collapsible section: `Title` (used as key) and `IsExpanded`. Defined but not yet actively used for persistence - available for future state-save/restore logic. |

---

### Services - `Services/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| ``AeonLog.cs`` | Edit freely | ``DiagnosticsGateway`` | Static logging gateway. Zero MAUI dependencies; also linked into ``Aeonpulse.Tests``. Wired by ``AeonLog.Initialise(ILoggerFactory)`` called from ``MauiProgram`` after ``builder.Build()``. Three ``[Conditional("DEBUG")]`` methods: ``Debug(cat, sub, msg, block?)``, ``Info(cat, sub, msg)``, ``Warn(cat, sub, msg)``. Falls back to ``NullLogger.Instance`` before initialisation. See Section 8.1 for message-format convention and ``[BLOCK]`` tag rules. |
| `CalculationService.cs` | Edit freely | `CoreCalculationEngine` | The single domain-logic class. Stateless - reads `DateTime.Now` internally so every call produces a fresh result. All 11 ticker methods return typed subclasses of `TickerData` (see `TickerResults.cs`). `FindNearestJubilee`, `ReduceToSingleDigit`, and `GetRandomTeaseText` live here. All output strings are pulled from `AppResources` at call time, so output automatically reflects the active locale. Thread-safe; called from both the UI thread and the 1-second timer (via `MainThread.BeginInvokeOnMainThread`). All 11 public `Calculate*` methods accept an optional `DateTime? now = null` parameter for deterministic testing - production callers omit it and get `DateTime.Now`. `FindNearestJubilee` and `ReduceToSingleDigit` are `internal static` and accessible to `Aeonpulse.Tests` via `InternalsVisibleTo`. |
| `ThemeService.cs` | Edit freely | - | Singleton (`Instance`). Stores three `Dictionary<string, Color>` palettes: `_defaultColors` (DefaultDark), `_highContrastDarkColors`, `_highContrastLightColors`. `ApplyScheme(string)` iterates the chosen palette and writes each key directly into `Application.Current.Resources`, causing all `DynamicResource` bindings to repaint immediately. To add a new colour scheme: add a new palette dict and a new `const string` identifier, then add a case to the switch in `ApplyScheme`. |
| `FontSizeService.cs` | Edit freely | - | Singleton (`Instance`). Same pattern as `ThemeService` but for five font-size keys (`FontSizeSmall` through `FontSizeTitle`). `ApplyPreset(string)` mutates the resource dict. Three presets: `Small`, `Normal`, `Large`. |

---

### ViewModels - `ViewModels/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `MainViewModel.cs` | Edit freely | - | The central state hub. Implements `INotifyPropertyChanged` manually (no toolkit). Owns: all 19 typed ticker result properties (`TimeJubileesResult`, `CountdownResult`, `CosmicStretchResult`, `YourBreathResult`, etc. - see `TickerResults.cs`); 4 section `bool XxxExpanded` properties; 13 card `bool XxxExpanded` properties; settings properties (`UseMetric`, `ColorScheme`, `TextSize`, `DisplayLanguage`, `BaseDateName`, `BaseDateValue`, `BaseDate`); all `ICommand` instances (toggle + refresh); the 1-second `System.Timers.Timer`; and the `event Func<Action, Task>? RefreshRequested` event used to coordinate the `RefreshingPopup` lifecycle. `SaveDate()` is the only correct entry point for changing the base date. `UpdateStaticCalculations()` recalculates 8 tickers; `UpdateLiveCalculations()` recalculates 6 tickers + `TeaseText`; `UpdateVibrantCosmos()` is called every 200 ms by a dedicated `_vibrantCosmosTimer`. |
| `LocalizedResources.cs` | Edit freely | - | Singleton (`Instance`). A thin passthrough wrapper: every property is `=> AppResources.SomeKey`. Bound in XAML as `{Binding Loc.PropertyName}`. `Invalidate()` fires `PropertyChanged(string.Empty)` which causes every bound property to re-read from `AppResources` with the newly-set culture. When adding a new localised string: add the `AppResources` key, then add the passthrough property here. |

---

### Views - `Views/`

All XAML files must be saved as **UTF-8 with BOM**. All colour/font-size references must use `DynamicResource`. All user-visible text must bind via `{Binding Loc.Xxx}` or `{x:Static resources:AppResources.Xxx}`.

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `MainPage.xaml` | Edit freely | *(XAML AI comments)* | The application's only persistent page. 3-row root `Grid`: Row 0 = NavBar (`Border` + inner `Grid`, logo `Image` with `ImageTint`, app name `Label`, hamburger `Button`). Row 1 = TimelineHeading (`Border` + `HorizontalStackLayout` with `FormattedString`). Row 2 = `ScrollView` containing four `CardFrame`-styled `Border` elements (Lab, Cosmos, Mirror, Eco Echoes), each holding a header `Grid` and a collapsible `VerticalStackLayout` of ticker card `Border` elements. Uses `BoolToImageSource` converter for chevron icons. |
| `MainPage.xaml.cs` | Edit carefully | `NavigationCoordinator` | Code-behind for `MainPage`. **Contains no business logic.** Responsibilities: subscribe to `MainViewModel.RefreshRequested` in constructor; implement `OnMenuClicked`, `OnTimelineHeadingTapped`, `OnLogoTapped` (opens `TeasePopup` anchored below NavBar, left-aligned, with Copy-to-clipboard and Close buttons); implement 11 `OnXxxInfoClicked` handlers that push `DeepDivePopup`; implement `OnTickerRefreshRequested` that pushes `RefreshingPopup`. Guard flags (`_isXxxOpen`) on every push prevent double-open. `OpenDeepDiveAsync()` measures `NavBar.Height + TimelineHeading.Height` to pass as `topOffset` to `DeepDivePopup`. Holds 15 guard bools (14 deep-dive/popup guards + `_isTeasePopupOpen`). Overrides `OnAppearing`/`OnDisappearing` to start and abort the `"LiveBadgeBreathing"` `Animation` that pulses all 10 LIVE badge labels (opacity 1.0 -> 0.4 -> 1.0, 2500 ms, `Easing.SinInOut`, repeating). |
| `SettingsPopup.xaml` | Edit freely | *(XAML AI comments)* | Full-screen overlay modal (semi-transparent `BackgroundColor`). `Frame` (legacy, `.NET 9` obsolete - do not add more Frames) centred panel. 3-row inner `Grid`: title bar, scrollable settings, close button footer. Settings rendered as a 2-column 14-row `Grid` with custom `RadioButton` `ControlTemplate` (outer ring `Ellipse` + inner dot `Ellipse` driven by `{TemplateBinding IsChecked}`). Groups: Unit System (rows 0-1), Color Scheme (rows 3-5), Text Size (rows 7-9), Language (rows 11-13), with spacer rows between. |
| `SettingsPopup.xaml.cs` | Edit carefully | `ModalViewController` | Accepts `MainViewModel` via constructor; sets `BindingContext`. `_initialising = true` guard blocks `CheckedChanged` callbacks during radio-button seeding. Handlers `OnUnitSystemChanged`, `OnColorSchemeChanged`, `OnTextSizeChanged`, `OnDisplayLanguageChanged` each read the `RadioButton.Value` string and write to the ViewModel setter, which applies the change immediately and persists it. |
| `ChangeDatePopup.xaml` | Edit freely | *(XAML AI comments)* | Centred (no full-screen overlay) modal. No backdrop-dismiss tap by design - prevents accidental dismissal of an in-progress edit. Contains `Entry` (event name) and `DatePicker` (date) inside `Frame` wrappers (legacy, `.NET 9` obsolete). Cancel and OK `Button` in a 3-column `Grid`. Uses `{x:Static resources:AppResources.Xxx}` (acceptable: popup is freshly constructed each time). |
| `ChangeDatePopup.xaml.cs` | Edit carefully | `ModalViewController` | Pre-populates `EventNameEntry.Text` and `EventDatePicker.Date` from the ViewModel in constructor. `OnOkClicked` validates the name entry is non-empty, then calls `MainViewModel.SaveDate(name, date)` atomically before `PopModalAsync()`. |
| `MainMenuPopup.xaml` | Edit freely | *(XAML AI comments)* | Full-screen overlay. `Frame` (legacy) panel positioned `HorizontalOptions=End` with top/right `Margin` injected in code-behind to sit below the NavBar hamburger button. Menu items are `Grid` + `TapGestureRecognizer` (not `Button`) to avoid nested hit-testing issues on Android. Items: Change Date, Settings, Exit. Close `Button` in footer. |
| `MainMenuPopup.xaml.cs` | Edit carefully | `ModalViewController` | Accepts `MainViewModel`, `topOffset`, `rightOffset`, `openChangeDateCallback`, `openSettingsCallback` via constructor. Each menu item first `await`s `PopModalAsync()` to finish its own dismiss animation, **then** invokes the callback. This ordering is mandatory on iOS to avoid `InvalidOperationException`. Exit calls `Application.Current.Quit()`. |
| `DeepDivePopup.xaml` | Edit freely | *(XAML AI comments)* | Generic info popup reused by all 11 ticker info buttons. Full-screen overlay. `Frame` (legacy) panel with top `Margin` overridden by code-behind. 3-row layout: non-scrollable title, `ScrollView` with two labelled content sections (methodology + sources), footer with close button. All text labels are set by code-behind via `x:Name`. |
| `DeepDivePopup.xaml.cs` | Edit freely | `ModalViewController` | Constructor accepts `title`, `section1Title`, `section1Text`, `section2Title`, `section2Text`, `topOffset`. Sets label text and overrides `PopupFrame.Margin` top component. To add more content sections, add new `Label` elements in the XAML and wire them here. |
| `RefreshingPopup.xaml` | Edit freely | *(XAML AI comments)* | Centred (no full-screen overlay) auto-dismissing overlay. `Frame` (legacy) containing `ActivityIndicator` + message `Label`. No user-dismiss gesture - dismisses automatically after 3 seconds. |
| `RefreshingPopup.xaml.cs` | Edit carefully | `ModalViewController` | Accepts `Action onDismissed` callback. `OnAppearing()` awaits `Task.Delay(3000)`, awaits `PopModalAsync()`, then invokes `onDismissed`. The callback updates a specific typed ticker result property on the ViewModel. The 3-second delay must remain to give the spinner time to animate. |
| `TeasePopup.xaml` | Edit freely | *(XAML AI comments)* | Left-aligned modal panel anchored below the NavBar via `Margin` injection. No fixed width - auto-sizes to content to avoid line-wrapping. Full-screen semi-transparent overlay with backdrop-dismiss tap. 3-row inner layout: title bar with divider, tease stat content label (`x:Name` set by code-behind), right-aligned footer with 2-button row (Copy + Close). Button `TextColor`/`BorderColor` use `{DynamicResource TextWhite}` to match content; `FontSize` uses `{DynamicResource FontSizeLarge}`; `MinimumWidthRequest=140` ensures equal size fitting `To Clipboard` at `FontSize=Large`. |
| `TeasePopup.xaml.cs` | Edit carefully | `ModalViewController` | Constructor accepts `string teaseText`, `double topOffset` (`NavBar.Height`), `double leftOffset` (NavBar left padding = 16), and `Func<string, Task> onCopiedCallback`. Sets `TeasePanel.Margin` top/left from offsets. `OnOkClicked` (also wired to backdrop tap) calls `PopModalAsync()`. `OnCopyClicked` calls `Clipboard.Default.SetTextAsync`, then `PopModalAsync()`, then invokes `onCopiedCallback` which shows a `DisplayAlert` on `MainPage`s navigation context (mandatory iOS pop-before-alert ordering). |

---

### Converters - `Converters/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `ValueConverters.cs` | Edit freely | `UIConverter` | Three converters, all declared in one file. `BoolToVisibilityConverter`: `bool -> bool` passthrough (drives `IsVisible`; `ConvertBack` throws). `InverseBoolConverter`: `bool -> !bool` (drives collapsed-state chevron direction; `ConvertBack` implemented). `BoolToImageSourceConverter`: `bool -> string` filename selected from `ConverterParameter` which must be formatted as `"fileIfTrue.png|fileIfFalse.png"`. |

---

### Helpers - `Helpers/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `ImageTint.cs` | Edit carefully | `PlatformAbstractionHelper` | Defines `ImageTint.Color` as a MAUI attached `BindableProperty`. `OnColorChanged` calls `view.Handler?.UpdateValue(nameof(ColorProperty))` which re-invokes the handler mapper registered in `MauiProgram.cs`. Setting `Color=null` clears the tint. Has no effect if `MauiProgram` mappers are not registered. Supports `DynamicResource` - live theme swaps re-invoke the mapper. |
| `TintBehavior.cs` | **Tombstone - do not edit or delete** | - | Empty file kept to prevent stale build artifact errors. The tinting functionality it formerly provided is now in `ImageTint.cs` + platform `TintHelper.cs`. |

---

### Resources - `Resources/`

#### String Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/AppResources.resx` | Edit freely | Master English string resource file. Contains all user-visible strings: UI labels, ticker text templates (with `{placeholder}` tokens), star catalogue entries (57 stars), Elder Futhark rune data (24 runes), personal year interpretations (1-9), and tease text. **Every new user-visible string must be added here first.** |
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
| `heartbeat.png` | Life Odometer expanded card `Image` | ECG waveform illustration shown in the expanded Life Odometer card. |
| `img_timejubilees.png` | Time Jubilees expanded card `Image` | Golden mechanical watch-gears illustration shown in the expanded Time Jubilees card. |
| `anim_countdown.gif` | Countdown expanded card `Image` | Animated split-flap (Solari) digit counter shown in the expanded Countdown card; `IsAnimationPlaying=True` drives animation. |
| `anim_spacewait.gif` | Space Wait expanded card `Image` | Animated split-flap (Solari) digit counter shown in the expanded Space Wait card; same layout as `anim_countdown.gif` but uses the Predator TTF for digit glyphs. `IsAnimationPlaying=True` drives animation. Source is a XAML literal (not a binding) to prevent GIF restart on each 1-second timer tick. |
| `img_spacewait_static.png` | Space Wait Windows static fallback | First-frame static PNG exported alongside `anim_spacewait.gif`. Reserved for future Windows-specific fallback use. |
| `calendar.png`, `menu.png`, `picture.png`, `send.png`, `share.png`, `tease.png`, `text.png` | Unused / reserved | Available for future features |

#### Other Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/AppIcon/appicon.png` | Edit freely | App icon source image used by `<MauiIcon>` in `.csproj`. |
| `Resources/Splash/splash.svg` | Edit freely | Splash screen SVG used by `<MauiSplashScreen>` in `.csproj`. Background colour `#512BD4`. |

---

### Attributes - `Attributes/`

| File | Edit? | Description |
|------|-------|-------------|
| `Attributes/AIContextAttribute.cs` | Edit carefully | Defines `[AIContext(string role)]`. `AllowMultiple = true`, `Inherited = false`. Used on classes and methods. See Section 4 for the full role vocabulary. |

---

### Platform-Specific Files - `Platforms/`

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

### Documentation Files

| File | Edit? | Description |
|------|-------|-------------|
| `Agents.md` | **Always update on change** | This file. AI Agent navigation guide. Must be kept in sync with all structural changes to the codebase. |

---

## 3. Architecture & Patterns

### 3.1 MVVM - Manual Implementation

The project uses MVVM **without** CommunityToolkit.Mvvm, Prism, or any other framework.
Every pattern is implemented by hand.

| Layer | Class | Responsibilities |
|-------|-------|-----------------|
| **View** | `*.xaml` + `*.xaml.cs` | Declare UI structure; bind to ViewModel properties and commands; handle navigation gestures only |
| **ViewModel** | `MainViewModel` | Own all application state; expose `ICommand` instances; fire `PropertyChanged`; coordinate the timer |
| **Service** | `CalculationService`, `ThemeService`, `FontSizeService` | Stateless domain logic; no UI references; no `INotifyPropertyChanged` |
| **Model** | `TickerData` (base), `TickerResults` (12 typed subclasses), `TickerCardModel`, `SubsectionState` | `TickerData` is the INPC base; typed subclasses add raw computed fields per ticker |

#### INotifyPropertyChanged pattern used throughout

Every settable property in `MainViewModel` and `TickerData` follows the same
boilerplate - no source generators, no base class helper beyond:

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
```

Setters call `OnPropertyChanged()` with no argument - `[CallerMemberName]` fills the
property name automatically at the call site.

#### ICommand pattern used throughout

All commands are `System.Windows.Input.Command` instances created inline in the
`MainViewModel` constructor. No `RelayCommand`, no `AsyncCommand`:

```csharp
// Pattern: Simple toggle command
ToggleLabCommand = new Command(() => LabExpanded = !LabExpanded);

// Pattern: Refresh command with popup lifecycle coordination
// Note: the 'else' branch runs during startup before MainPage subscribes to RefreshRequested.
RefreshTimeJubileesCommand = new Command(async () =>
{
    if (RefreshRequested != null)
        await RefreshRequested.Invoke(() =>
            TimeJubilees = _calculationService.CalculateTimeJubilees(...));
    else
        TimeJubilees = _calculationService.CalculateTimeJubilees(...);
});
```

#### BindingContext wiring

`MainViewModel` is instantiated **inline in XAML** inside `MainPage.xaml`:

```xml
<ContentPage.BindingContext>
    <viewmodels:MainViewModel />
</ContentPage.BindingContext>
```

`MainPage.xaml.cs` accesses the XAML-constructed instance by casting `BindingContext`:

```csharp
if (BindingContext is MainViewModel vm)
    vm.RefreshRequested += OnTickerRefreshRequested;
```

For popup pages (`SettingsPopup`, `ChangeDatePopup`, `MainMenuPopup`), the ViewModel
is passed as a **constructor argument** and assigned to `BindingContext` in
code-behind - not constructed by XAML.

---

### 3.2 Data Binding

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

### 3.3 Data Flow

```
USER ACTION
    |
    +-- Taps TimelineHeading / menu "Change Date"
    |       --> MainPage.OnTimelineHeadingTapped
    |               --> Navigation.PushModalAsync(new ChangeDatePopup(vm))
    |                       --> ChangeDatePopup.OnOkClicked
    |                               --> MainViewModel.SaveDate(name, date)
    |                                       sets _baseDateName, _baseDateValue, _baseDate
    |                                       --> UpdateAllCalculations()
    |                                               --> UpdateStaticCalculations() [6 tickers]
    |                                               --> UpdateLiveCalculations()   [4 tickers]
    |
    +-- Taps refresh icon on a static ticker card
    |       --> RefreshXxxCommand.Execute()
    |               --> RefreshRequested.Invoke(callback)
    |                       --> MainPage.OnTickerRefreshRequested(callback)
    |                               --> Navigation.PushModalAsync(new RefreshingPopup(callback))
    |                                       [3-second auto-dismiss in RefreshingPopup.OnAppearing]
    |                                       --> callback()
    |                                               --> Typed result property set on ViewModel
    |
    +-- Taps section chevron
    |       --> ToggleXxxCommand.Execute()
    |               --> XxxExpanded = !XxxExpanded --> OnPropertyChanged()
    |                       --> IsVisible on collapsible VerticalStackLayout updates
    |
    +-- Taps ticker card chevron
            --> ToggleXxxCommand.Execute() [card level]
                    --> XxxExpanded = !XxxExpanded --> OnPropertyChanged()
                            --> IsVisible on FullText Label updates
                            --> BoolToImageSource on ImageButton source updates

TIMER (every 1 second, thread-pool thread)
    |
    --> System.Timers.Timer.Elapsed
            --> MainThread.BeginInvokeOnMainThread(UpdateLiveCalculations)
                    --> CalculateCountdown()       --> Countdown.BriefText/FullText
                    --> CalculateLifeOdometer()    --> LifeOdometer.BriefText/FullText
                    --> CalculateGalacticCommute() --> GalacticCommute.BriefText/FullText
                    --> CalculatePhotonPath()      --> PhotonPath.BriefText/FullText
                    --> CalculateCosmicStretch()    --> CosmicStretch.BriefText/FullText
                    --> CalculateYourBreath()       --> YourBreath.BriefText/FullText
                    --> CalculateGlobalCrowd()      --> GlobalCrowd.BriefText/FullText
                    --> CalculateSpaceWait()       --> SpaceWait.BriefText/FullText
                    --> CalculateVibrantHumanity() --> VibrantHumanity.BriefText/FullText
                    --> GetRandomTeaseText()       --> TeaseText
                    --> (LIVE badge breathing animation runs independently via Animation.Commit in MainPage.OnAppearing)
                            each setter fires PropertyChanged
                                --> {Binding Xxx.BriefText} Labels repaint
```

**Critical constraint:** the timer fires on a thread-pool thread. `UpdateLiveCalculations`
is always marshalled back via `MainThread.BeginInvokeOnMainThread` before any bound
property is set. Never set a bound ViewModel property from a background thread.

---

### 3.4 Localisation Flow

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

### 3.5 Theme and Font Size Flow

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

### 3.6 Cross-Platform Image Tinting

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

### 3.7 Modal Navigation Pattern

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

### 3.8 Settings Persistence

All user preferences use `Microsoft.Maui.Storage.Preferences` (maps to
`SharedPreferences` on Android, `NSUserDefaults` on iOS/Mac, Registry on Windows).

| Preference key | Type | Default | Read in | Written in |
|----------------|------|---------|---------|------------|
| `"ColorScheme"` | `string` | `"DefaultDark"` | `App.xaml.cs` ctor | `MainViewModel.ColorScheme` setter |
| `"TextSize"` | `string` | `"Normal"` | `App.xaml.cs` ctor | `MainViewModel.TextSize` setter |
| `"DisplayLanguage"` | `string` | `"Default"` | `App.xaml.cs` ctor | `MainViewModel.DisplayLanguage` setter |
| `"UseMetric"` | `bool` | `true` | `MainViewModel` ctor | `MainViewModel.UseMetric` setter |

---

## 4. AI Markup Schema

This section defines every markup convention used in the codebase to communicate
architectural intent to AI agents. Three distinct systems are in use: the
`[AIContext]` C# attribute, XAML `<!-- AI: ... -->` comments, and XML documentation
comments (`///`). Each has a precise syntax contract and a defined scope of use.
All three must be kept in sync with structural code changes.

---

### 4.1 `[AIContext]` Attribute - C# Files

#### Definition

```csharp
// Attributes/AIContextAttribute.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property,
                AllowMultiple = true,
                Inherited = false)]
public sealed class AIContextAttribute : Attribute
{
    public string Role { get; }
    public AIContextAttribute(string role) => Role = role;
}
```

Key properties:
- `AllowMultiple = true` - a single symbol may carry more than one role (e.g., a method that is both a `LiveTicker` and a `StarCatalogueLookup`)
- `Inherited = false` - subclasses do not inherit the attribute; each must be decorated explicitly
- Target: `Class`, `Method`, or `Property`

#### Syntax

```csharp
// Single role - most common
[AIContext("CoreCalculation")]
public BirthRuneResult CalculateBirthRune(DateTime baseDate, string baseDateValue) { ... }

// Multiple roles on one symbol - used when a method crosses concern boundaries
[AIContext("LiveTicker")]
[AIContext("StarCatalogueLookup")]
public PhotonPathResult CalculatePhotonPath(DateTime baseDate, string baseDateValue, bool useMetric) { ... }

// On a class
[AIContext("AppBootstrap")]
public partial class App : Application { ... }
```

#### Complete Role Vocabulary

Every role string currently in use is listed below. Use an existing role before
introducing a new one. Introduce a new role **only** when the semantic function is
genuinely novel and not covered by any existing role.

| Role | Semantic Meaning | Currently Applied To |
|------|-----------------|----------------------|
| `AppBootstrap` | Application startup and configuration. Runs before the UI is inflated. | `App` (class + ctor), `MauiProgram` (class + `CreateMauiApp`) |
| `CoreCalculationEngine` | The top-level stateless calculation service class. | `CalculationService` (class) |
| `CoreCalculation` | A specific ticker calculation that runs once on demand (not live). | `CalculateTimeJubilees`, `CalculateAlienAnniversaries`, `CalculateHumanBirthRank`, `CalculateBirthRune`, `CalculatePersonalYear`, `CalculateGlobalExhale` |
| `LiveTicker` | A calculation called every second by the 1-second timer. | `CalculateCountdown`, `CalculateLifeOdometer`, `CalculateGalacticCommute`, `CalculatePhotonPath`, `CalculateCosmicStretch` |
| `JubileeSelectionAlgorithm` | The private helper that finds the next round-number milestone. | `FindNearestJubilee` |
| `StarCatalogueLookup` | Method that contains and queries the inline 57-star distance catalogue. | `CalculatePhotonPath` (second attribute) |
| `ExternalDataModel` | Method whose constants derive from a named external scientific dataset. | `CalculateHumanBirthRank` (PRB data), `CalculateGlobalExhale` (Global Carbon Budget 2025) |
| `UIPresentation` | User-facing text assembly with no domain logic. | `GetRandomTeaseText` |
| `NavigationCoordinator` | Code-behind whose sole role is modal push/pop and event-to-command wiring. | `MainPage` (class), `OpenDeepDiveAsync` (method) |
| `ModalViewController` | Code-behind for a popup/modal page. | `SettingsPopup`, `ChangeDatePopup`, `MainMenuPopup`, `DeepDivePopup`, `RefreshingPopup` (all classes) |
| `DataTransferObject` | A data-carrying model with no domain behaviour. | `TickerData`, all 10 `*Result` subclasses, `TickerCardModel` (classes) |
| `DiagnosticsGateway` | Static logging gateway with zero domain logic; active in DEBUG builds only via `[Conditional("DEBUG")]`. | `AeonLog` (class) |
| `UIConverter` | An `IValueConverter` implementation used in XAML bindings. | `BoolToVisibilityConverter`, `InverseBoolConverter`, `BoolToImageSourceConverter` (classes) |
| `PlatformAbstractionHelper` | A cross-platform helper that bridges a missing MAUI API to native layers. | `ImageTint` (class) |
| `PlatformTintImplementation` | Platform-specific `partial` method implementation for `ImageTint`. | All four `TintHelper.cs` classes |
| `PlatformEntryPoint` | The platform-specific bootstrapper class (Activity, AppDelegate, Application subclass). | `MainActivity`, `MainApplication`, `AppDelegate` (iOS + Mac), `Program` (iOS + Mac), `App` (Windows), `Main` (Tizen) |

#### Placement Rules

1. Place the attribute **immediately above** the `class` or `method` declaration, before access modifiers.
2. For a class with a `[AIContext]` on both the class and a method, the class-level attribute describes the overall role; the method-level attribute describes a more specific sub-role.
3. Do **not** apply `[AIContext]` to private helper methods unless they implement a named algorithm worth tracking (e.g., `FindNearestJubilee`).
4. Do **not** apply `[AIContext]` to properties, constructors, or event handlers unless they represent a distinct architectural concern.

#### When Adding New Code

- New service class: apply `[AIContext("CoreCalculationEngine")]` or introduce a new role with a documented entry in this table.
- New static ticker method: apply `[AIContext("CoreCalculation")]`.
- New live ticker method: apply `[AIContext("LiveTicker")]`.
- New popup code-behind: apply `[AIContext("ModalViewController")]`.
- New platform entry point: apply `[AIContext("PlatformEntryPoint")]`.
- Update this table when a new role is introduced.

---

### 4.2 XAML `<!-- AI: ... -->` Comments

#### Purpose

XAML AI comments serve a different purpose from `[AIContext]`: they annotate
**layout structure**, **binding sources**, **hidden dependencies**, and **design
decisions** that are not visible from the element attributes alone. They are the
primary documentation channel for agents reading or modifying XAML files.

#### Syntax Contract

```xml
<!-- AI: {One-line role/purpose summary}.
     {Binding sources with full ViewModel path}.
     {Layout geometry or grid slot assignments}.
     {Side effects, hidden dependencies, or constraints}. -->
```

Rules:
- The prefix `AI:` is mandatory and must be the first token after `<!--`
- Multi-line comments indent continuation lines to align with the first character after `AI:`
- **ASCII only** - no emoji, no Unicode dashes (`-` only), no box-drawing characters
- End with ` -->` on its own line for multi-line comments, or inline for single-line
- Placed **immediately before** the element it describes, at the same indentation level

#### Two Styles in Use

**Block comment** (before a significant element):
```xml
<!-- AI: Row 1 - Timeline Heading.
     Displays "{BaseDateName} from {BaseDateDisplay}" as a tappable row
     that opens the ChangeDatePopup for quick base-date editing.
     Binding sources: MainViewModel.BaseDateName, Loc.Timeline_BaseDatePreposition,
     MainViewModel.BaseDateDisplay. -->
<Border Grid.Row="1" x:Name="TimelineHeading" ...>
```

**Inline comment** (on a single descriptive line):
```xml
<!-- AI: Centred card layout - no dismiss gesture; auto-dismissed after 3 s -->
<Grid x:Name="RefreshingRoot" ...>
```

#### Non-AI XAML Comments (also in use)

Not all XAML comments use the `AI:` prefix. Two other comment patterns exist and
must be preserved:

**Binding annotation** (documents a single binding source):
```xml
<!-- Binding: MainViewModel.Loc.Ticker_TimeJubileesTitle (live-localised) -->
<Label Text="{Binding Loc.Ticker_TimeJubileesTitle}" ... />
```

**Inline resource annotation** (documents a resource value and its theme-swap equivalents):
```xml
<Color x:Key="CyberCyan">#00E5FF</Color>  <!-- HC-Dark: #FFFFFF avg=156 | HC-Light: #000000 -->
```

These non-`AI:` comments are also authoritative documentation - do not remove them.

#### Current `<!-- AI: -->` Comment Inventory

The table below lists every existing `AI:` comment by file and element, giving agents
a map to navigate without reading every XAML file.

| File | Line | Element described |
|------|------|-------------------|
| `Resources/Styles/Colors.xaml` | 1 | Entire file - startup-defaults-only warning, `DynamicResource` mandate |
| `Resources/Styles/Styles.xaml` | 1 | Entire file - `BasedOn` inheritance guidance |
| `Views/MainPage.xaml` | 22 | `ContentPage.BindingContext` - XAML-constructed VM, code-behind cast pattern |
| `Views/MainPage.xaml` | 29 | Root `Grid` - 3-row layout slots `[NavBar|TimelineHeading|ScrollContent]` |
| `Views/MainPage.xaml` | 35 | `Border` Row 0 - NavBar 3-column layout |
| `Views/MainPage.xaml` | 83 | `Border` Row 1 - TimelineHeading, binding sources, tap opens `ChangeDatePopup` |
| `Views/MainPage.xaml` | 97 | `Label.FormattedText` - 5-span composition reason (avoids spacing/wrapping) |
| `Views/MainPage.xaml` | 126 | `ScrollView` Row 2 - section/card structure overview |
| `Views/MainPage.xaml` | 143 | Section header `Grid` - 2-column `[title|chevron]`, `BoolToImageSource` binding |
| `Views/MainPage.xaml` | 161 | Section body `VerticalStackLayout` - `IsVisible` binding, contained tickers |
| `Views/MainPage.xaml` | 179 | Ticker card header `Grid` - 3-column `[emoji|title|action buttons]` |
| `Views/MainPage.xaml` | 910 | Your Breath ticker card header `Grid` - 3-column `[lung emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.YourBreath` (live-updated every second). |
| `Views/MainPage.xaml` | ~1020 | Global Crowd ticker card header `Grid` - 3-column `[people emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.GlobalCrowd` (live-updated every second). |
| `Views/MainPage.xaml` | ~1022 | Life Log ticker card header `Grid` - 3-column `[clock emoji|title|info+refresh+expand buttons]`, bound to `MainViewModel.LifeLog` (static, re-randomises on refresh). |
| `Views/MainPage.xaml` | ~970 | Cellular Refresh ticker card header
| `Views/MainPage.xaml` | ~1070 | Space Wait ticker card header `Grid` - 3-column `[planet emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.SpaceWait` (live-updated every second). | `Grid` - 3-column `[DNA emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.CellularRefresh` (live-updated every second). |
| `Views/MainPage.xaml` | ~1170 | Vibrant Humanity ticker card header `Grid` - 3-column `[globe emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.VibrantHumanity` (live-updated every second). |
| `Views/MainPage.xaml` | ~1400 | Vibrant Nature ticker card header `Grid` - 3-column `[butterfly emoji|title|info+refresh+expand buttons]`, bound to `MainViewModel.VibrantNature` (static, re-calculates on refresh). |
| `Views/MainPage.xaml` | ~710 | Vibrant Cosmos ticker card header `Grid` - 3-column `[sparkles emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.VibrantCosmos` (live-updated every 200 ms). |
| `Views/SettingsPopup.xaml` | 9 | Full-screen overlay `Grid` - Layer 0/1 dismiss/panel pattern |
| `Views/SettingsPopup.xaml` | 25 | `Frame` panel - floats over backdrop, `DynamicResource` theme note |
| `Views/SettingsPopup.xaml` | 38 | 3-row inner `Grid` - `[Title+Divider|Settings|CloseButton]` row assignments |
| `Views/SettingsPopup.xaml` | 76 | Settings control `Grid` - 2-column 14-row layout, all row group assignments |
| `Views/SettingsPopup.xaml` | 121 | `RadioButton.ControlTemplate` - outer ring + inner dot pattern |
| `Views/SettingsPopup.xaml` | 588 | About section - read-only, all strings live-localised via `Loc` bindings |
| `Views/DeepDivePopup.xaml` | 16 | Full-screen overlay `Grid` - Layer 0/1 dismiss/panel pattern |
| `Views/DeepDivePopup.xaml` | 32 | `Frame` panel - `topOffset` margin injection from code-behind |
| `Views/DeepDivePopup.xaml` | 44 | 3-row inner `Grid` - `[Title+Divider|ScrollableContent|Footer]` |
| `Views/MainMenuPopup.xaml` | 1 | Entire file (page-level) - anchoring below NavBar, callback navigation pattern |
| `Views/MainMenuPopup.xaml` | 16 | Full-screen overlay `Grid` - Layer 0/1 |
| `Views/MainMenuPopup.xaml` | 30 | `Frame` panel - `HorizontalOptions=End`, right `Margin` injected in code-behind |
| `Views/MainMenuPopup.xaml` | 59 | Menu item `Grid` - 2-column `[icon|label]`, `TapGestureRecognizer` not `Button` reason |
| `Views/RefreshingPopup.xaml` | 2 | Entire file (page-level) - transient overlay, auto-dismiss, no `BindingContext` |
| `Views/RefreshingPopup.xaml` | 15 | Centred card `Grid` - no dismiss gesture, auto-dismissed after 3 s |
| `Views/TeasePopup.xaml` | Edit freely | *(XAML AI comments)* | Left-aligned modal panel anchored below the NavBar via `Margin` injection. No fixed width - auto-sizes to content to avoid line-wrapping. Full-screen semi-transparent overlay with backdrop-dismiss tap. 3-row inner layout: title bar with divider, tease stat content label (`x:Name` set by code-behind), right-aligned footer with 2-button row (Copy + Close). Button `TextColor`/`BorderColor` use `{DynamicResource TextWhite}` to match content; `FontSize` uses `{DynamicResource FontSizeLarge}`; `MinimumWidthRequest=140` ensures equal size fitting `To Clipboard` at `FontSize=Large`. |
| `TeasePopup.xaml.cs` | Edit carefully | `ModalViewController` | Constructor accepts `string teaseText`, `double topOffset` (`NavBar.Height`), `double leftOffset` (NavBar left padding = 16), and `Func<string, Task> onCopiedCallback`. Sets `TeasePanel.Margin` top/left from offsets. `OnOkClicked` (also wired to backdrop tap) calls `PopModalAsync()`. `OnCopyClicked` calls `Clipboard.Default.SetTextAsync`, then `PopModalAsync()`, then invokes `onCopiedCallback` which shows a `DisplayAlert` on `MainPage`s navigation context (mandatory iOS pop-before-alert ordering). |

---

### Converters - `Converters/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `ValueConverters.cs` | Edit freely | `UIConverter` | Three converters, all declared in one file. `BoolToVisibilityConverter`: `bool -> bool` passthrough (drives `IsVisible`; `ConvertBack` throws). `InverseBoolConverter`: `bool -> !bool` (drives collapsed-state chevron direction; `ConvertBack` implemented). `BoolToImageSourceConverter`: `bool -> string` filename selected from `ConverterParameter` which must be formatted as `"fileIfTrue.png|fileIfFalse.png"`. |

---

### Helpers - `Helpers/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `ImageTint.cs` | Edit carefully | `PlatformAbstractionHelper` | Defines `ImageTint.Color` as a MAUI attached `BindableProperty`. `OnColorChanged` calls `view.Handler?.UpdateValue(nameof(ColorProperty))` which re-invokes the handler mapper registered in `MauiProgram.cs`. Setting `Color=null` clears the tint. Has no effect if `MauiProgram` mappers are not registered. Supports `DynamicResource` - live theme swaps re-invoke the mapper. |
| `TintBehavior.cs` | **Tombstone - do not edit or delete** | - | Empty file kept to prevent stale build artifact errors. The tinting functionality it formerly provided is now in `ImageTint.cs` + platform `TintHelper.cs`. |

---

### Resources - `Resources/`

#### String Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/AppResources.resx` | Edit freely | Master English string resource file. Contains all user-visible strings: UI labels, ticker text templates (with `{placeholder}` tokens), star catalogue entries (57 stars), Elder Futhark rune data (24 runes), personal year interpretations (1-9), and tease text. **Every new user-visible string must be added here first.** |
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
| `heartbeat.png` | Life Odometer expanded card `Image` | ECG waveform illustration shown in the expanded Life Odometer card. |
| `img_timejubilees.png` | Time Jubilees expanded card `Image` | Golden mechanical watch-gears illustration shown in the expanded Time Jubilees card. |
| `anim_countdown.gif` | Countdown expanded card `Image` | Animated split-flap (Solari) digit counter shown in the expanded Countdown card; `IsAnimationPlaying=True` drives animation. |
| `anim_spacewait.gif` | Space Wait expanded card `Image` | Animated split-flap (Solari) digit counter shown in the expanded Space Wait card; same layout as `anim_countdown.gif` but uses the Predator TTF for digit glyphs. `IsAnimationPlaying=True` drives animation. Source is a XAML literal (not a binding) to prevent GIF restart on each 1-second timer tick. |
| `img_spacewait_static.png` | Space Wait Windows static fallback | First-frame static PNG exported alongside `anim_spacewait.gif`. Reserved for future Windows-specific fallback use. |
| `calendar.png`, `menu.png`, `picture.png`, `send.png`, `share.png`, `tease.png`, `text.png` | Unused / reserved | Available for future features |

#### Other Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/AppIcon/appicon.png` | Edit freely | App icon source image used by `<MauiIcon>` in `.csproj`. |
| `Resources/Splash/splash.svg` | Edit freely | Splash screen SVG used by `<MauiSplashScreen>` in `.csproj`. Background colour `#512BD4`. |

---

### Attributes - `Attributes/`

| File | Edit? | Description |
|------|-------|-------------|
| `Attributes/AIContextAttribute.cs` | Edit carefully | Defines `[AIContext(string role)]`. `AllowMultiple = true`, `Inherited = false`. Used on classes and methods. See Section 4 for the full role vocabulary. |

---

### Platform-Specific Files - `Platforms/`

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

### Documentation Files

| File | Edit? | Description |
|------|-------|-------------|
| `Agents.md` | **Always update on change** | This file. AI Agent navigation guide. Must be kept in sync with all structural changes to the codebase. |

---

## 3. Architecture & Patterns

### 3.1 MVVM - Manual Implementation

The project uses MVVM **without** CommunityToolkit.Mvvm, Prism, or any other framework.
Every pattern is implemented by hand.

| Layer | Class | Responsibilities |
|-------|-------|-----------------|
| **View** | `*.xaml` + `*.xaml.cs` | Declare UI structure; bind to ViewModel properties and commands; handle navigation gestures only |
| **ViewModel** | `MainViewModel` | Own all application state; expose `ICommand` instances; fire `PropertyChanged`; coordinate the timer |
| **Service** | `CalculationService`, `ThemeService`, `FontSizeService` | Stateless domain logic; no UI references; no `INotifyPropertyChanged` |
| **Model** | `TickerData` (base), `TickerResults` (12 typed subclasses), `TickerCardModel`, `SubsectionState` | `TickerData` is the INPC base; typed subclasses add raw computed fields per ticker |

#### INotifyPropertyChanged pattern used throughout

Every settable property in `MainViewModel` and `TickerData` follows the same
boilerplate - no source generators, no base class helper beyond:

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
```

Setters call `OnPropertyChanged()` with no argument - `[CallerMemberName]` fills the
property name automatically at the call site.

#### ICommand pattern used throughout

All commands are `System.Windows.Input.Command` instances created inline in the
`MainViewModel` constructor. No `RelayCommand`, no `AsyncCommand`:

```csharp
// Pattern: Simple toggle command
ToggleLabCommand = new Command(() => LabExpanded = !LabExpanded);

// Pattern: Refresh command with popup lifecycle coordination
// Note: the 'else' branch runs during startup before MainPage subscribes to RefreshRequested.
RefreshTimeJubileesCommand = new Command(async () =>
{
    if (RefreshRequested != null)
        await RefreshRequested.Invoke(() =>
            TimeJubilees = _calculationService.CalculateTimeJubilees(...));
    else
        TimeJubilees = _calculationService.CalculateTimeJubilees(...);
});
```

#### BindingContext wiring

`MainViewModel` is instantiated **inline in XAML** inside `MainPage.xaml`:

```xml
<ContentPage.BindingContext>
    <viewmodels:MainViewModel />
</ContentPage.BindingContext>
```

`MainPage.xaml.cs` accesses the XAML-constructed instance by casting `BindingContext`:

```csharp
if (BindingContext is MainViewModel vm)
    vm.RefreshRequested += OnTickerRefreshRequested;
```

For popup pages (`SettingsPopup`, `ChangeDatePopup`, `MainMenuPopup`), the ViewModel
is passed as a **constructor argument** and assigned to `BindingContext` in
code-behind - not constructed by XAML.

---

### 3.2 Data Binding

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

### 3.3 Data Flow

```
USER ACTION
    |
    +-- Taps TimelineHeading / menu "Change Date"
    |       --> MainPage.OnTimelineHeadingTapped
    |               --> Navigation.PushModalAsync(new ChangeDatePopup(vm))
    |                       --> ChangeDatePopup.OnOkClicked
    |                               --> MainViewModel.SaveDate(name, date)
    |                                       sets _baseDateName, _baseDateValue, _baseDate
    |                                       --> UpdateAllCalculations()
    |                                               --> UpdateStaticCalculations() [6 tickers]
    |                                               --> UpdateLiveCalculations()   [4 tickers]
    |
    +-- Taps refresh icon on a static ticker card
    |       --> RefreshXxxCommand.Execute()
    |               --> RefreshRequested.Invoke(callback)
    |                       --> MainPage.OnTickerRefreshRequested(callback)
    |                               --> Navigation.PushModalAsync(new RefreshingPopup(callback))
    |                                       [3-second auto-dismiss in RefreshingPopup.OnAppearing]
    |                                       --> callback()
    |                                               --> Typed result property set on ViewModel
    |
    +-- Taps section chevron
    |       --> ToggleXxxCommand.Execute()
    |               --> XxxExpanded = !XxxExpanded --> OnPropertyChanged()
    |                       --> IsVisible on collapsible VerticalStackLayout updates
    |
    +-- Taps ticker card chevron
            --> ToggleXxxCommand.Execute() [card level]
                    --> XxxExpanded = !XxxExpanded --> OnPropertyChanged()
                            --> IsVisible on FullText Label updates
                            --> BoolToImageSource on ImageButton source updates

TIMER (every 1 second, thread-pool thread)
    |
    --> System.Timers.Timer.Elapsed
            --> MainThread.BeginInvokeOnMainThread(UpdateLiveCalculations)
                    --> CalculateCountdown()       --> Countdown.BriefText/FullText
                    --> CalculateLifeOdometer()    --> LifeOdometer.BriefText/FullText
                    --> CalculateGalacticCommute() --> GalacticCommute.BriefText/FullText
                    --> CalculatePhotonPath()      --> PhotonPath.BriefText/FullText
                    --> CalculateCosmicStretch()    --> CosmicStretch.BriefText/FullText
                    --> CalculateYourBreath()       --> YourBreath.BriefText/FullText
                    --> CalculateGlobalCrowd()      --> GlobalCrowd.BriefText/FullText
                    --> CalculateSpaceWait()       --> SpaceWait.BriefText/FullText
                    --> CalculateVibrantHumanity() --> VibrantHumanity.BriefText/FullText
                    --> GetRandomTeaseText()       --> TeaseText
                    --> (LIVE badge breathing animation runs independently via Animation.Commit in MainPage.OnAppearing)
                            each setter fires PropertyChanged
                                --> {Binding Xxx.BriefText} Labels repaint
```

**Critical constraint:** the timer fires on a thread-pool thread. `UpdateLiveCalculations`
is always marshalled back via `MainThread.BeginInvokeOnMainThread` before any bound
property is set. Never set a bound ViewModel property from a background thread.

---

### 3.4 Localisation Flow

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

### 3.5 Theme and Font Size Flow

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

### 3.6 Cross-Platform Image Tinting

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

### 3.7 Modal Navigation Pattern

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

### 3.8 Settings Persistence

All user preferences use `Microsoft.Maui.Storage.Preferences` (maps to
`SharedPreferences` on Android, `NSUserDefaults` on iOS/Mac, Registry on Windows).

| Preference key | Type | Default | Read in | Written in |
|----------------|------|---------|---------|------------|
| `"ColorScheme"` | `string` | `"DefaultDark"` | `App.xaml.cs` ctor | `MainViewModel.ColorScheme` setter |
| `"TextSize"` | `string` | `"Normal"` | `App.xaml.cs` ctor | `MainViewModel.TextSize` setter |
| `"DisplayLanguage"` | `string` | `"Default"` | `App.xaml.cs` ctor | `MainViewModel.DisplayLanguage` setter |
| `"UseMetric"` | `bool` | `true` | `MainViewModel` ctor | `MainViewModel.UseMetric` setter |

---

## 4. AI Markup Schema

This section defines every markup convention used in the codebase to communicate
architectural intent to AI agents. Three distinct systems are in use: the
`[AIContext]` C# attribute, XAML `<!-- AI: ... -->` comments, and XML documentation
comments (`///`). Each has a precise syntax contract and a defined scope of use.
All three must be kept in sync with structural code changes.

---

### 4.1 `[AIContext]` Attribute - C# Files

#### Definition

```csharp
// Attributes/AIContextAttribute.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property,
                AllowMultiple = true,
                Inherited = false)]
public sealed class AIContextAttribute : Attribute
{
    public string Role { get; }
    public AIContextAttribute(string role) => Role = role;
}
```

Key properties:
- `AllowMultiple = true` - a single symbol may carry more than one role (e.g., a method that is both a `LiveTicker` and a `StarCatalogueLookup`)
- `Inherited = false` - subclasses do not inherit the attribute; each must be decorated explicitly
- Target: `Class`, `Method`, or `Property`

#### Syntax

```csharp
// Single role - most common
[AIContext("CoreCalculation")]
public BirthRuneResult CalculateBirthRune(DateTime baseDate, string baseDateValue) { ... }

// Multiple roles on one symbol - used when a method crosses concern boundaries
[AIContext("LiveTicker")]
[AIContext("StarCatalogueLookup")]
public PhotonPathResult CalculatePhotonPath(DateTime baseDate, string baseDateValue, bool useMetric) { ... }

// On a class
[AIContext("AppBootstrap")]
public partial class App : Application { ... }
```

#### Complete Role Vocabulary

Every role string currently in use is listed below. Use an existing role before
introducing a new one. Introduce a new role **only** when the semantic function is
genuinely novel and not covered by any existing role.

| Role | Semantic Meaning | Currently Applied To |
|------|-----------------|----------------------|
| `AppBootstrap` | Application startup and configuration. Runs before the UI is inflated. | `App` (class + ctor), `MauiProgram` (class + `CreateMauiApp`) |
| `CoreCalculationEngine` | The top-level stateless calculation service class. | `CalculationService` (class) |
| `CoreCalculation` | A specific ticker calculation that runs once on demand (not live). | `CalculateTimeJubilees`, `CalculateAlienAnniversaries`, `CalculateHumanBirthRank`, `CalculateBirthRune`, `CalculatePersonalYear`, `CalculateGlobalExhale` |
| `LiveTicker` | A calculation called every second by the 1-second timer. | `CalculateCountdown`, `CalculateLifeOdometer`, `CalculateGalacticCommute`, `CalculatePhotonPath`, `CalculateCosmicStretch` |
| `JubileeSelectionAlgorithm` | The private helper that finds the next round-number milestone. | `FindNearestJubilee` |
| `StarCatalogueLookup` | Method that contains and queries the inline 57-star distance catalogue. | `CalculatePhotonPath` (second attribute) |
| `ExternalDataModel` | Method whose constants derive from a named external scientific dataset. | `CalculateHumanBirthRank` (PRB data), `CalculateGlobalExhale` (Global Carbon Budget 2025) |
| `UIPresentation` | User-facing text assembly with no domain logic. | `GetRandomTeaseText` |
| `NavigationCoordinator` | Code-behind whose sole role is modal push/pop and event-to-command wiring. | `MainPage` (class), `OpenDeepDiveAsync` (method) |
| `ModalViewController` | Code-behind for a popup/modal page. | `SettingsPopup`, `ChangeDatePopup`, `MainMenuPopup`, `DeepDivePopup`, `RefreshingPopup` (all classes) |
| `DataTransferObject` | A data-carrying model with no domain behaviour. | `TickerData`, all 10 `*Result` subclasses, `TickerCardModel` (classes) |
| `DiagnosticsGateway` | Static logging gateway with zero domain logic; active in DEBUG builds only via `[Conditional("DEBUG")]`. | `AeonLog` (class) |
| `UIConverter` | An `IValueConverter` implementation used in XAML bindings. | `BoolToVisibilityConverter`, `InverseBoolConverter`, `BoolToImageSourceConverter` (classes) |
| `PlatformAbstractionHelper` | A cross-platform helper that bridges a missing MAUI API to native layers. | `ImageTint` (class) |
| `PlatformTintImplementation` | Platform-specific `partial` method implementation for `ImageTint`. | All four `TintHelper.cs` classes |
| `PlatformEntryPoint` | The platform-specific bootstrapper class (Activity, AppDelegate, Application subclass). | `MainActivity`, `MainApplication`, `AppDelegate` (iOS + Mac), `Program` (iOS + Mac), `App` (Windows), `Main` (Tizen) |

#### Placement Rules

1. Place the attribute **immediately above** the `class` or `method` declaration, before access modifiers.
2. For a class with a `[AIContext]` on both the class and a method, the class-level attribute describes the overall role; the method-level attribute describes a more specific sub-role.
3. Do **not** apply `[AIContext]` to private helper methods unless they implement a named algorithm worth tracking (e.g., `FindNearestJubilee`).
4. Do **not** apply `[AIContext]` to properties, constructors, or event handlers unless they represent a distinct architectural concern.

#### When Adding New Code

- New service class: apply `[AIContext("CoreCalculationEngine")]` or introduce a new role with a documented entry in this table.
- New static ticker method: apply `[AIContext("CoreCalculation")]`.
- New live ticker method: apply `[AIContext("LiveTicker")]`.
- New popup code-behind: apply `[AIContext("ModalViewController")]`.
- New platform entry point: apply `[AIContext("PlatformEntryPoint")]`.
- Update this table when a new role is introduced.

---

### 4.2 XAML `<!-- AI: ... -->` Comments

#### Purpose

XAML AI comments serve a different purpose from `[AIContext]`: they annotate
**layout structure**, **binding sources**, **hidden dependencies**, and **design
decisions** that are not visible from the element attributes alone. They are the
primary documentation channel for agents reading or modifying XAML files.

#### Syntax Contract

```xml
<!-- AI: {One-line role/purpose summary}.
     {Binding sources with full ViewModel path}.
     {Layout geometry or grid slot assignments}.
     {Side effects, hidden dependencies, or constraints}. -->
```

Rules:
- The prefix `AI:` is mandatory and must be the first token after `<!--`
- Multi-line comments indent continuation lines to align with the first character after `AI:`
- **ASCII only** - no emoji, no Unicode dashes (`-` only), no box-drawing characters
- End with ` -->` on its own line for multi-line comments, or inline for single-line
- Placed **immediately before** the element it describes, at the same indentation level

#### Two Styles in Use

**Block comment** (before a significant element):
```xml
<!-- AI: Row 1 - Timeline Heading.
     Displays "{BaseDateName} from {BaseDateDisplay}" as a tappable row
     that opens the ChangeDatePopup for quick base-date editing.
     Binding sources: MainViewModel.BaseDateName, Loc.Timeline_BaseDatePreposition,
     MainViewModel.BaseDateDisplay. -->
<Border Grid.Row="1" x:Name="TimelineHeading" ...>
```

**Inline comment** (on a single descriptive line):
```xml
<!-- AI: Centred card layout - no dismiss gesture; auto-dismissed after 3 s -->
<Grid x:Name="RefreshingRoot" ...>
```

#### Non-AI XAML Comments (also in use)

Not all XAML comments use the `AI:` prefix. Two other comment patterns exist and
must be preserved:

**Binding annotation** (documents a single binding source):
```xml
<!-- Binding: MainViewModel.Loc.Ticker_TimeJubileesTitle (live-localised) -->
<Label Text="{Binding Loc.Ticker_TimeJubileesTitle}" ... />
```

**Inline resource annotation** (documents a resource value and its theme-swap equivalents):
```xml
<Color x:Key="CyberCyan">#00E5FF</Color>  <!-- HC-Dark: #FFFFFF avg=156 | HC-Light: #000000 -->
```

These non-`AI:` comments are also authoritative documentation - do not remove them.

#### Current `<!-- AI: -->` Comment Inventory

The table below lists every existing `AI:` comment by file and element, giving agents
a map to navigate without reading every XAML file.

| File | Line | Element described |
|------|------|-------------------|
| `Resources/Styles/Colors.xaml` | 1 | Entire file - startup-defaults-only warning, `DynamicResource` mandate |
| `Resources/Styles/Styles.xaml` | 1 | Entire file - `BasedOn` inheritance guidance |
| `Views/MainPage.xaml` | 22 | `ContentPage.BindingContext` - XAML-constructed VM, code-behind cast pattern |
| `Views/MainPage.xaml` | 29 | Root `Grid` - 3-row layout slots `[NavBar|TimelineHeading|ScrollContent]` |
| `Views/MainPage.xaml` | 35 | `Border` Row 0 - NavBar 3-column layout |
| `Views/MainPage.xaml` | 83 | `Border` Row 1 - TimelineHeading, binding sources, tap opens `ChangeDatePopup` |
| `Views/MainPage.xaml` | 97 | `Label.FormattedText` - 5-span composition reason (avoids spacing/wrapping) |
| `Views/MainPage.xaml` | 126 | `ScrollView` Row 2 - section/card structure overview |
| `Views/MainPage.xaml` | 143 | Section header `Grid` - 2-column `[title|chevron]`, `BoolToImageSource` binding |
| `Views/MainPage.xaml` | 161 | Section body `VerticalStackLayout` - `IsVisible` binding, contained tickers |
| `Views/MainPage.xaml` | 179 | Ticker card header `Grid` - 3-column `[emoji|title|action buttons]` |
| `Views/MainPage.xaml` | 910 | Your Breath ticker card header `Grid` - 3-column `[lung emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.YourBreath` (live-updated every second). |
| `Views/MainPage.xaml` | ~1020 | Global Crowd ticker card header `Grid` - 3-column `[people emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.GlobalCrowd` (live-updated every second). |
| `Views/MainPage.xaml` | ~1022 | Life Log ticker card header `Grid` - 3-column `[clock emoji|title|info+refresh+expand buttons]`, bound to `MainViewModel.LifeLog` (static, re-randomises on refresh). |
| `Views/MainPage.xaml` | ~970 | Cellular Refresh ticker card header
| `Views/MainPage.xaml` | ~1070 | Space Wait ticker card header `Grid` - 3-column `[planet emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.SpaceWait` (live-updated every second). | `Grid` - 3-column `[DNA emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.CellularRefresh` (live-updated every second). |
| `Views/MainPage.xaml` | ~1170 | Vibrant Humanity ticker card header `Grid` - 3-column `[globe emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.VibrantHumanity` (live-updated every second). |
| `Views/MainPage.xaml` | ~1400 | Vibrant Nature ticker card header `Grid` - 3-column `[butterfly emoji|title|info+refresh+expand buttons]`, bound to `MainViewModel.VibrantNature` (static, re-calculates on refresh). |
| `Views/MainPage.xaml` | ~710 | Vibrant Cosmos ticker card header `Grid` - 3-column `[sparkles emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.VibrantCosmos` (live-updated every 200 ms). |
| `Views/SettingsPopup.xaml` | 9 | Full-screen overlay `Grid` - Layer 0/1 dismiss/panel pattern |
| `Views/SettingsPopup.xaml` | 25 | `Frame` panel - floats over backdrop, `DynamicResource` theme note |
| `Views/SettingsPopup.xaml` | 38 | 3-row inner `Grid` - `[Title+Divider|Settings|CloseButton]` row assignments |
| `Views/SettingsPopup.xaml` | 76 | Settings control `Grid` - 2-column 14-row layout, all row group assignments |
| `Views/SettingsPopup.xaml` | 121 | `RadioButton` `ControlTemplate` - outer ring + inner dot pattern |
| `Views/SettingsPopup.xaml` | 588 | About section - read-only, all strings live-localised via `Loc` bindings |
| `Views/DeepDivePopup.xaml` | 16 | Full-screen overlay `Grid` - Layer 0/1 dismiss/panel pattern |
| `Views/DeepDivePopup.xaml` | 32 | `Frame` panel - `topOffset` margin injection from code-behind |
| `Views/DeepDivePopup.xaml` | 44 | 3-row inner `Grid` - `[Title+Divider|ScrollableContent|Footer]` |
| `Views/MainMenuPopup.xaml` | 1 | Entire file (page-level) - anchoring below NavBar, callback navigation pattern |
| `Views/MainMenuPopup.xaml` | 16 | Full-screen overlay `Grid` - Layer 0/1 |
| `Views/MainMenuPopup.xaml` | 30 | `Frame` panel - `HorizontalOptions=End`, right `Margin` injected in code-behind |
| `Views/MainMenuPopup.xaml` | 59 | Menu item `Grid` - 2-column `[icon|label]`, `TapGestureRecognizer` not `Button` reason |
| `Views/RefreshingPopup.xaml` | 2 | Entire file (page-level) - transient overlay, auto-dismiss, no `BindingContext` |
| `Views/RefreshingPopup.xaml` | 15 | Centred card `Grid` - no dismiss gesture, auto-dismissed after 3 s |
| `Views/TeasePopup.xaml` | Edit freely | *(XAML AI comments)* | Left-aligned modal panel anchored below the NavBar via `Margin` injection. No fixed width - auto-sizes to content to avoid line-wrapping. Full-screen semi-transparent overlay with backdrop-dismiss tap. 3-row inner layout: title bar with divider, tease stat content label (`x:Name` set by code-behind), right-aligned footer with 2-button row (Copy + Close). Button `TextColor`/`BorderColor` use `{DynamicResource TextWhite}` to match content; `FontSize` uses `{DynamicResource FontSizeLarge}`; `MinimumWidthRequest=140` ensures equal size fitting `To Clipboard` at `FontSize=Large`. |
| `TeasePopup.xaml.cs` | Edit carefully | `ModalViewController` | Constructor accepts `string teaseText`, `double topOffset` (`NavBar.Height`), `double leftOffset` (NavBar left padding = 16), and `Func<string, Task> onCopiedCallback`. Sets `TeasePanel.Margin` top/left from offsets. `OnOkClicked` (also wired to backdrop tap) calls `PopModalAsync()`. `OnCopyClicked` calls `Clipboard.Default.SetTextAsync`, then `PopModalAsync()`, then invokes `onCopiedCallback` which shows a `DisplayAlert` on `MainPage`s navigation context (mandatory iOS pop-before-alert ordering). |

---

### Converters - `Converters/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `ValueConverters.cs` | Edit freely | `UIConverter` | Three converters, all declared in one file. `BoolToVisibilityConverter`: `bool -> bool` passthrough (drives `IsVisible`; `ConvertBack` throws). `InverseBoolConverter`: `bool -> !bool` (drives collapsed-state chevron direction; `ConvertBack` implemented). `BoolToImageSourceConverter`: `bool -> string` filename selected from `ConverterParameter` which must be formatted as `"fileIfTrue.png|fileIfFalse.png"`. |

---

### Helpers - `Helpers/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `ImageTint.cs` | Edit carefully | `PlatformAbstractionHelper` | Defines `ImageTint.Color` as a MAUI attached `BindableProperty`. `OnColorChanged` calls `view.Handler?.UpdateValue(nameof(ColorProperty))` which re-invokes the handler mapper registered in `MauiProgram.cs`. Setting `Color=null` clears the tint. Has no effect if `MauiProgram` mappers are not registered. Supports `DynamicResource` - live theme swaps re-invoke the mapper. |
| `TintBehavior.cs` | **Tombstone - do not edit or delete** | - | Empty file kept to prevent stale build artifact errors. The tinting functionality it formerly provided is now in `ImageTint.cs` + platform `TintHelper.cs`. |

---

### Resources - `Resources/`

#### String Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/AppResources.resx` | Edit freely | Master English string resource file. Contains all user-visible strings: UI labels, ticker text templates (with `{placeholder}` tokens), star catalogue entries (57 stars), Elder Futhark rune data (24 runes), personal year interpretations (1-9), and tease text. **Every new user-visible string must be added here first.** |
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
| `heartbeat.png` | Life Odometer expanded card `Image` | ECG waveform illustration shown in the expanded Life Odometer card. |
| `img_timejubilees.png` | Time Jubilees expanded card `Image` | Golden mechanical watch-gears illustration shown in the expanded Time Jubilees card. |
| `anim_countdown.gif` | Countdown expanded card `Image` | Animated split-flap (Solari) digit counter shown in the expanded Countdown card; `IsAnimationPlaying=True` drives animation. |
| `anim_spacewait.gif` | Space Wait expanded card `Image` | Animated split-flap (Solari) digit counter shown in the expanded Space Wait card; same layout as `anim_countdown.gif` but uses the Predator TTF for digit glyphs. `IsAnimationPlaying=True` drives animation. Source is a XAML literal (not a binding) to prevent GIF restart on each 1-second timer tick. |
| `img_spacewait_static.png` | Space Wait Windows static fallback | First-frame static PNG exported alongside `anim_spacewait.gif`. Reserved for future Windows-specific fallback use. |
| `calendar.png`, `menu.png`, `picture.png`, `send.png`, `share.png`, `tease.png`, `text.png` | Unused / reserved | Available for future features |

#### Other Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/AppIcon/appicon.png` | Edit freely | App icon source image used by `<MauiIcon>` in `.csproj`. |
| `Resources/Splash/splash.svg` | Edit freely | Splash screen SVG used by `<MauiSplashScreen>` in `.csproj`. Background colour `#512BD4`. |

---

### Attributes - `Attributes/`

| File | Edit? | Description |
|------|-------|-------------|
| `Attributes/AIContextAttribute.cs` | Edit carefully | Defines `[AIContext(string role)]`. `AllowMultiple = true`, `Inherited = false`. Used on classes and methods. See Section 4 for the full role vocabulary. |

---

### Platform-Specific Files - `Platforms/`

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

### Documentation Files

| File | Edit? | Description |
|------|-------|-------------|
| `Agents.md` | **Always update on change** | This file. AI Agent navigation guide. Must be kept in sync with all structural changes to the codebase. |

---

## 3. Architecture & Patterns

### 3.1 MVVM - Manual Implementation

The project uses MVVM **without** CommunityToolkit.Mvvm, Prism, or any other framework.
Every pattern is implemented by hand.

| Layer | Class | Responsibilities |
|-------|-------|-----------------|
| **View** | `*.xaml` + `*.xaml.cs` | Declare UI structure; bind to ViewModel properties and commands; handle navigation gestures only |
| **ViewModel** | `MainViewModel` | Own all application state; expose `ICommand` instances; fire `PropertyChanged`; coordinate the timer |
| **Service** | `CalculationService`, `ThemeService`, `FontSizeService` | Stateless domain logic; no UI references; no `INotifyPropertyChanged` |
| **Model** | `TickerData` (base), `TickerResults` (12 typed subclasses), `TickerCardModel`, `SubsectionState` | `TickerData` is the INPC base; typed subclasses add raw computed fields per ticker |

#### INotifyPropertyChanged pattern used throughout

Every settable property in `MainViewModel` and `TickerData` follows the same
boilerplate - no source generators, no base class helper beyond:

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
```

Setters call `OnPropertyChanged()` with no argument - `[CallerMemberName]` fills the
property name automatically at the call site.

#### ICommand pattern used throughout

All commands are `System.Windows.Input.Command` instances created inline in the
`MainViewModel` constructor. No `RelayCommand`, no `AsyncCommand`:

```csharp
// Pattern: Simple toggle command
ToggleLabCommand = new Command(() => LabExpanded = !LabExpanded);

// Pattern: Refresh command with popup lifecycle coordination
// Note: the 'else' branch runs during startup before MainPage subscribes to RefreshRequested.
RefreshTimeJubileesCommand = new Command(async () =>
{
    if (RefreshRequested != null)
        await RefreshRequested.Invoke(() =>
            TimeJubilees = _calculationService.CalculateTimeJubilees(...));
    else
        TimeJubilees = _calculationService.CalculateTimeJubilees(...);
});
```

#### BindingContext wiring

`MainViewModel` is instantiated **inline in XAML** inside `MainPage.xaml`:

```xml
<ContentPage.BindingContext>
    <viewmodels:MainViewModel />
</ContentPage.BindingContext>
```

`MainPage.xaml.cs` accesses the XAML-constructed instance by casting `BindingContext`:

```csharp
if (BindingContext is MainViewModel vm)
    vm.RefreshRequested += OnTickerRefreshRequested;
```

For popup pages (`SettingsPopup`, `ChangeDatePopup`, `MainMenuPopup`), the ViewModel
is passed as a **constructor argument** and assigned to `BindingContext` in
code-behind - not constructed by XAML.

---

### 3.2 Data Binding

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

### 3.3 Data Flow

```
USER ACTION
    |
    +-- Taps TimelineHeading / menu "Change Date"
    |       --> MainPage.OnTimelineHeadingTapped
    |               --> Navigation.PushModalAsync(new ChangeDatePopup(vm))
    |                       --> ChangeDatePopup.OnOkClicked
    |                               --> MainViewModel.SaveDate(name, date)
    |                                       sets _baseDateName, _baseDateValue, _baseDate
    |                                       --> UpdateAllCalculations()
    |                                               --> UpdateStaticCalculations() [6 tickers]
    |                                               --> UpdateLiveCalculations()   [4 tickers]
    |
    +-- Taps refresh icon on a static ticker card
    |       --> RefreshXxxCommand.Execute()
    |               --> RefreshRequested.Invoke(callback)
    |                       --> MainPage.OnTickerRefreshRequested(callback)
    |                               --> Navigation.PushModalAsync(new RefreshingPopup(callback))
    |                                       [3-second auto-dismiss in RefreshingPopup.OnAppearing]
    |                                       --> callback()
    |                                               --> Typed result property set on ViewModel
    |
    +-- Taps section chevron
    |       --> ToggleXxxCommand.Execute()
    |               --> XxxExpanded = !XxxExpanded --> OnPropertyChanged()
    |                       --> IsVisible on collapsible VerticalStackLayout updates
    |
    +-- Taps ticker card chevron
            --> ToggleXxxCommand.Execute() [card level]
                    --> XxxExpanded = !XxxExpanded --> OnPropertyChanged()
                            --> IsVisible on FullText Label updates
                            --> BoolToImageSource on ImageButton source updates

TIMER (every 1 second, thread-pool thread)
    |
    --> System.Timers.Timer.Elapsed
            --> MainThread.BeginInvokeOnMainThread(UpdateLiveCalculations)
                    --> CalculateCountdown()       --> Countdown.BriefText/FullText
                    --> CalculateLifeOdometer()    --> LifeOdometer.BriefText/FullText
                    --> CalculateGalacticCommute() --> GalacticCommute.BriefText/FullText
                    --> CalculatePhotonPath()      --> PhotonPath.BriefText/FullText
                    --> CalculateCosmicStretch()    --> CosmicStretch.BriefText/FullText
                    --> CalculateYourBreath()       --> YourBreath.BriefText/FullText
                    --> CalculateGlobalCrowd()      --> GlobalCrowd.BriefText/FullText
                    --> CalculateSpaceWait()       --> SpaceWait.BriefText/FullText
                    --> CalculateVibrantHumanity() --> VibrantHumanity.BriefText/FullText
                    --> GetRandomTeaseText()       --> TeaseText
                    --> (LIVE badge breathing animation runs independently via Animation.Commit in MainPage.OnAppearing)
                            each setter fires PropertyChanged
                                --> {Binding Xxx.BriefText} Labels repaint
```

**Critical constraint:** the timer fires on a thread-pool thread. `UpdateLiveCalculations`
is always marshalled back via `MainThread.BeginInvokeOnMainThread` before any bound
property is set. Never set a bound ViewModel property from a background thread.

---

### 3.4 Localisation Flow

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

### 3.5 Theme and Font Size Flow

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

### 3.6 Cross-Platform Image Tinting

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

### 3.7 Modal Navigation Pattern

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

### 3.8 Settings Persistence

All user preferences use `Microsoft.Maui.Storage.Preferences` (maps to
`SharedPreferences` on Android, `NSUserDefaults` on iOS/Mac, Registry on Windows).

| Preference key | Type | Default | Read in | Written in |
|----------------|------|---------|---------|------------|
| `"ColorScheme"` | `string` | `"DefaultDark"` | `App.xaml.cs` ctor | `MainViewModel.ColorScheme` setter |
| `"TextSize"` | `string` | `"Normal"` | `App.xaml.cs` ctor | `MainViewModel.TextSize` setter |
| `"DisplayLanguage"` | `string` | `"Default"` | `App.xaml.cs` ctor | `MainViewModel.DisplayLanguage` setter |
| `"UseMetric"` | `bool` | `true` | `MainViewModel` ctor | `MainViewModel.UseMetric` setter |

---

## 4. AI Markup Schema

This section defines every markup convention used in the codebase to communicate
architectural intent to AI agents. Three distinct systems are in use: the
`[AIContext]` C# attribute, XAML `<!-- AI: ... -->` comments, and XML documentation
comments (`///`). Each has a precise syntax contract and a defined scope of use.
All three must be kept in sync with structural code changes.

---

### 4.1 `[AIContext]` Attribute - C# Files

#### Definition

```csharp
// Attributes/AIContextAttribute.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property,
                AllowMultiple = true,
                Inherited = false)]
public sealed class AIContextAttribute : Attribute
{
    public string Role { get; }
    public AIContextAttribute(string role) => Role = role;
}
```

Key properties:
- `AllowMultiple = true` - a single symbol may carry more than one role (e.g., a method that is both a `LiveTicker` and a `StarCatalogueLookup`)
- `Inherited = false` - subclasses do not inherit the attribute; each must be decorated explicitly
- Target: `Class`, `Method`, or `Property`

#### Syntax

```csharp
// Single role - most common
[AIContext("CoreCalculation")]
public BirthRuneResult CalculateBirthRune(DateTime baseDate, string baseDateValue) { ... }

// Multiple roles on one symbol - used when a method crosses concern boundaries
[AIContext("LiveTicker")]
[AIContext("StarCatalogueLookup")]
public PhotonPathResult CalculatePhotonPath(DateTime baseDate, string baseDateValue, bool useMetric) { ... }

// On a class
[AIContext("AppBootstrap")]
public partial class App : Application { ... }
```

#### Complete Role Vocabulary

Every role string currently in use is listed below. Use an existing role before
introducing a new one. Introduce a new role **only** when the semantic function is
genuinely novel and not covered by any existing role.

| Role | Semantic Meaning | Currently Applied To |
|------|-----------------|----------------------|
| `AppBootstrap` | Application startup and configuration. Runs before the UI is inflated. | `App` (class + ctor), `MauiProgram` (class + `CreateMauiApp`) |
| `CoreCalculationEngine` | The top-level stateless calculation service class. | `CalculationService` (class) |
| `CoreCalculation` | A specific ticker calculation that runs once on demand (not live). | `CalculateTimeJubilees`, `CalculateAlienAnniversaries`, `CalculateHumanBirthRank`, `CalculateBirthRune`, `CalculatePersonalYear`, `CalculateGlobalExhale` |
| `LiveTicker` | A calculation called every second by the 1-second timer. | `CalculateCountdown`, `CalculateLifeOdometer`, `CalculateGalacticCommute`, `CalculatePhotonPath`, `CalculateCosmicStretch` |
| `JubileeSelectionAlgorithm` | The private helper that finds the next round-number milestone. | `FindNearestJubilee` |
| `StarCatalogueLookup` | Method that contains and queries the inline 57-star distance catalogue. | `CalculatePhotonPath` (second attribute) |
| `ExternalDataModel` | Method whose constants derive from a named external scientific dataset. | `CalculateHumanBirthRank` (PRB data), `CalculateGlobalExhale` (Global Carbon Budget 2025) |
| `UIPresentation` | User-facing text assembly with no domain logic. | `GetRandomTeaseText` |
| `NavigationCoordinator` | Code-behind whose sole role is modal push/pop and event-to-command wiring. | `MainPage` (class), `OpenDeepDiveAsync` (method) |
| `ModalViewController` | Code-behind for a popup/modal page. | `SettingsPopup`, `ChangeDatePopup`, `MainMenuPopup`, `DeepDivePopup`, `RefreshingPopup` (all classes) |
| `DataTransferObject` | A data-carrying model with no domain behaviour. | `TickerData`, all 10 `*Result` subclasses, `TickerCardModel` (classes) |
| `DiagnosticsGateway` | Static logging gateway with zero domain logic; active in DEBUG builds only via `[Conditional("DEBUG")]`. | `AeonLog` (class) |
| `UIConverter` | An `IValueConverter` implementation used in XAML bindings. | `BoolToVisibilityConverter`, `InverseBoolConverter`, `BoolToImageSourceConverter` (classes) |
| `PlatformAbstractionHelper` | A cross-platform helper that bridges a missing MAUI API to native layers. | `ImageTint` (class) |
| `PlatformTintImplementation` | Platform-specific `partial` method implementation for `ImageTint`. | All four `TintHelper.cs` classes |
| `PlatformEntryPoint` | The platform-specific bootstrapper class (Activity, AppDelegate, Application subclass). | `MainActivity`, `MainApplication`, `AppDelegate` (iOS + Mac), `Program` (iOS + Mac), `App` (Windows), `Main` (Tizen) |

#### Placement Rules

1. Place the attribute **immediately above** the `class` or `method` declaration, before access modifiers.
2. For a class with a `[AIContext]` on both the class and a method, the class-level attribute describes the overall role; the method-level attribute describes a more specific sub-role.
3. Do **not** apply `[AIContext]` to private helper methods unless they implement a named algorithm worth tracking (e.g., `FindNearestJubilee`).
4. Do **not** apply `[AIContext]` to properties, constructors, or event handlers unless they represent a distinct architectural concern.

#### When Adding New Code

- New service class: apply `[AIContext("CoreCalculationEngine")]` or introduce a new role with a documented entry in this table.
- New static ticker method: apply `[AIContext("CoreCalculation")]`.
- New live ticker method: apply `[AIContext("LiveTicker")]`.
- New popup code-behind: apply `[AIContext("ModalViewController")]`.
- New platform entry point: apply `[AIContext("PlatformEntryPoint")]`.
- Update this table when a new role is introduced.

---

### 4.2 XAML `<!-- AI: ... -->` Comments

#### Purpose

XAML AI comments serve a different purpose from `[AIContext]`: they annotate
**layout structure**, **binding sources**, **hidden dependencies**, and **design
decisions** that are not visible from the element attributes alone. They are the
primary documentation channel for agents reading or modifying XAML files.

#### Syntax Contract

```xml
<!-- AI: {One-line role/purpose summary}.
     {Binding sources with full ViewModel path}.
     {Layout geometry or grid slot assignments}.
     {Side effects, hidden dependencies, or constraints}. -->
```

Rules:
- The prefix `AI:` is mandatory and must be the first token after `<!--`
- Multi-line comments indent continuation lines to align with the first character after `AI:`
- **ASCII only** - no emoji, no Unicode dashes (`-` only), no box-drawing characters
- End with ` -->` on its own line for multi-line comments, or inline for single-line
- Placed **immediately before** the element it describes, at the same indentation level

#### Two Styles in Use

**Block comment** (before a significant element):
```xml
<!-- AI: Row 1 - Timeline Heading.
     Displays "{BaseDateName} from {BaseDateDisplay}" as a tappable row
     that opens the ChangeDatePopup for quick base-date editing.
     Binding sources: MainViewModel.BaseDateName, Loc.Timeline_BaseDatePreposition,
     MainViewModel.BaseDateDisplay. -->
<Border Grid.Row="1" x:Name="TimelineHeading" ...>
```

**Inline comment** (on a single descriptive line):
```xml
<!-- AI: Centred card layout - no dismiss gesture; auto-dismissed after 3 s -->
<Grid x:Name="RefreshingRoot" ...>
```

#### Non-AI XAML Comments (also in use)

Not all XAML comments use the `AI:` prefix. Two other comment patterns exist and
must be preserved:

**Binding annotation** (documents a single binding source):
```xml
<!-- Binding: MainViewModel.Loc.Ticker_TimeJubileesTitle (live-localised) -->
<Label Text="{Binding Loc.Ticker_TimeJubileesTitle}" ... />
```

**Inline resource annotation** (documents a resource value and its theme-swap equivalents):
```xml
<Color x:Key="CyberCyan">#00E5FF</Color>  <!-- HC-Dark: #FFFFFF avg=156 | HC-Light: #000000 -->
```

These non-`AI:` comments are also authoritative documentation - do not remove them.

#### Current `<!-- AI: -->` Comment Inventory

The table below lists every existing `AI:` comment by file and element, giving agents
a map to navigate without reading every XAML file.

| File | Line | Element described |
|------|------|-------------------|
| `Resources/Styles/Colors.xaml` | 1 | Entire file - startup-defaults-only warning, `DynamicResource` mandate |
| `Resources/Styles/Styles.xaml` | 1 | Entire file - `BasedOn` inheritance guidance |
| `Views/MainPage.xaml` | 22 | `ContentPage.BindingContext` - XAML-constructed VM, code-behind cast pattern |
| `Views/MainPage.xaml` | 29 | Root `Grid` - 3-row layout slots `[NavBar|TimelineHeading|ScrollContent]` |
| `Views/MainPage.xaml` | 35 | `Border` Row 0 - NavBar 3-column layout |
| `Views/MainPage.xaml` | 83 | `Border` Row 1 - TimelineHeading, binding sources, tap opens `ChangeDatePopup` |
| `Views/MainPage.xaml` | 97 | `Label.FormattedText` - 5-span composition reason (avoids spacing/wrapping) |
| `Views/MainPage.xaml` | 126 | `ScrollView` Row 2 - section/card structure overview |
| `Views/MainPage.xaml` | 143 | Section header `Grid` - 2-column `[title|chevron]`, `BoolToImageSource` binding |
| `Views/MainPage.xaml` | 161 | Section body `VerticalStackLayout` - `IsVisible` binding, contained tickers |
| `Views/MainPage.xaml` | 179 | Ticker card header `Grid` - 3-column `[emoji|title|action buttons]` |
| `Views/MainPage.xaml` | 910 | Your Breath ticker card header `Grid` - 3-column `[lung emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.YourBreath` (live-updated every second). |
| `Views/MainPage.xaml` | ~1020 | Global Crowd ticker card header `Grid` - 3-column `[people emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.GlobalCrowd` (live-updated every second). |
| `Views/MainPage.xaml` | ~1022 | Life Log ticker card header `Grid` - 3-column `[clock emoji|title|info+refresh+expand buttons]`, bound to `MainViewModel.LifeLog` (static, re-randomises on refresh). |
| `Views/MainPage.xaml` | ~970 | Cellular Refresh ticker card header
| `Views/MainPage.xaml` | ~1070 | Space Wait ticker card header `Grid` - 3-column `[planet emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.SpaceWait` (live-updated every second). | `Grid` - 3-column `[DNA emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.CellularRefresh` (live-updated every second). |
| `Views/MainPage.xaml` | ~1170 | Vibrant Humanity ticker card header `Grid` - 3-column `[globe emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.VibrantHumanity` (live-updated every second). |
| `Views/MainPage.xaml` | ~1400 | Vibrant Nature ticker card header `Grid` - 3-column `[butterfly emoji|title|info+refresh+expand buttons]`, bound to `MainViewModel.VibrantNature` (static, re-calculates on refresh). |
| `Views/MainPage.xaml` | ~710 | Vibrant Cosmos ticker card header `Grid` - 3-column `[sparkles emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.VibrantCosmos` (live-updated every 200 ms). |
| `Views/SettingsPopup.xaml` | 9 | Full-screen overlay `Grid` - Layer 0/1 dismiss/panel pattern |
| `Views/SettingsPopup.xaml` | 25 | `Frame` panel - floats over backdrop, `DynamicResource` theme note |
| `Views/SettingsPopup.xaml` | 38 | 3-row inner `Grid` - `[Title+Divider|Settings|CloseButton]` row assignments |
| `Views/SettingsPopup.xaml` | 76 | Settings control `Grid` - 2-column 14-row layout, all row group assignments |
| `Views/SettingsPopup.xaml` | 121 | `RadioButton` `ControlTemplate` - outer ring + inner dot pattern |
| `Views/SettingsPopup.xaml` | 588 | About section - read-only, all strings live-localised via `Loc` bindings |
| `Views/DeepDivePopup.xaml` | 16 | Full-screen overlay `Grid` - Layer 0/1 dismiss/panel pattern |
| `Views/DeepDivePopup.xaml` | 32 | `Frame` panel - `topOffset` margin injection from code-behind |
| `Views/DeepDivePopup.xaml` | 44 | 3-row inner `Grid` - `[Title+Divider|ScrollableContent|Footer]` |
| `Views/MainMenuPopup.xaml` | 1 | Entire file (page-level) - anchoring below NavBar, callback navigation pattern |
| `Views/MainMenuPopup.xaml` | 16 | Full-screen overlay `Grid` - Layer 0/1 |
| `Views/MainMenuPopup.xaml` | 30 | `Frame` panel - `HorizontalOptions=End`, right `Margin` injected in code-behind |
| `Views/MainMenuPopup.xaml` | 59 | Menu item `Grid` - 2-column `[icon|label]`, `TapGestureRecognizer` not `Button` reason |
| `Views/RefreshingPopup.xaml` | 2 | Entire file (page-level) - transient overlay, auto-dismiss, no `BindingContext` |
| `Views/RefreshingPopup.xaml` | 15 | Centred card `Grid` - no dismiss gesture, auto-dismissed after 3 s |
| `Views/TeasePopup.xaml` | Edit freely | *(XAML AI comments)* | Left-aligned modal panel anchored below the NavBar via `Margin` injection. No fixed width - auto-sizes to content to avoid line-wrapping. Full-screen semi-transparent overlay with backdrop-dismiss tap. 3-row inner layout: title bar with divider, tease stat content label (`x:Name` set by code-behind), right-aligned footer with 2-button row (Copy + Close). Button `TextColor`/`BorderColor` use `{DynamicResource TextWhite}` to match content; `FontSize` uses `{DynamicResource FontSizeLarge}`; `MinimumWidthRequest=140` ensures equal size fitting `To Clipboard` at `FontSize=Large`. |
| `TeasePopup.xaml.cs` | Edit carefully | `ModalViewController` | Constructor accepts `string teaseText`, `double topOffset` (`NavBar.Height`), `double leftOffset` (NavBar left padding = 16), and `Func<string, Task> onCopiedCallback`. Sets `TeasePanel.Margin` top/left from offsets. `OnOkClicked` (also wired to backdrop tap) calls `PopModalAsync()`. `OnCopyClicked` calls `Clipboard.Default.SetTextAsync`, then `PopModalAsync()`, then invokes `onCopiedCallback` which shows a `DisplayAlert` on `MainPage`s navigation context (mandatory iOS pop-before-alert ordering). |

---

### Converters - `Converters/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `ValueConverters.cs` | Edit freely | `UIConverter` | Three converters, all declared in one file. `BoolToVisibilityConverter`: `bool -> bool` passthrough (drives `IsVisible`; `ConvertBack` throws). `InverseBoolConverter`: `bool -> !bool` (drives collapsed-state chevron direction; `ConvertBack` implemented). `BoolToImageSourceConverter`: `bool -> string` filename selected from `ConverterParameter` which must be formatted as `"fileIfTrue.png|fileIfFalse.png"`. |

---

### Helpers - `Helpers/`

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `ImageTint.cs` | Edit carefully | `PlatformAbstractionHelper` | Defines `ImageTint.Color` as a MAUI attached `BindableProperty`. `OnColorChanged` calls `view.Handler?.UpdateValue(nameof(ColorProperty))` which re-invokes the handler mapper registered in `MauiProgram.cs`. Setting `Color=null` clears the tint. Has no effect if `MauiProgram` mappers are not registered. Supports `DynamicResource` - live theme swaps re-invoke the mapper. |
| `TintBehavior.cs` | **Tombstone - do not edit or delete** | - | Empty file kept to prevent stale build artifact errors. The tinting functionality it formerly provided is now in `ImageTint.cs` + platform `TintHelper.cs`. |

---

### Resources - `Resources/`

#### String Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/AppResources.resx` | Edit freely | Master English string resource file. Contains all user-visible strings: UI labels, ticker text templates (with `{placeholder}` tokens), star catalogue entries (57 stars), Elder Futhark rune data (24 runes), personal year interpretations (1-9), and tease text. **Every new user-visible string must be added here first.** |
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
| `heartbeat.png` | Life Odometer expanded card `Image` | ECG waveform illustration shown in the expanded Life Odometer card. |
| `img_timejubilees.png` | Time Jubilees expanded card `Image` | Golden mechanical watch-gears illustration shown in the expanded Time Jubilees card. |
| `anim_countdown.gif` | Countdown expanded card `Image` | Animated split-flap (Solari) digit counter shown in the expanded Countdown card; `IsAnimationPlaying=True` drives animation. |
| `anim_spacewait.gif` | Space Wait expanded card `Image` | Animated split-flap (Solari) digit counter shown in the expanded Space Wait card; same layout as `anim_countdown.gif` but uses the Predator TTF for digit glyphs. `IsAnimationPlaying=True` drives animation. Source is a XAML literal (not a binding) to prevent GIF restart on each 1-second timer tick. |
| `img_spacewait_static.png` | Space Wait Windows static fallback | First-frame static PNG exported alongside `anim_spacewait.gif`. Reserved for future Windows-specific fallback use. |
| `calendar.png`, `menu.png`, `picture.png`, `send.png`, `share.png`, `tease.png`, `text.png` | Unused / reserved | Available for future features |

#### Other Resources

| File | Edit? | Description |
|------|-------|-------------|
| `Resources/AppIcon/appicon.png` | Edit freely | App icon source image used by `<MauiIcon>` in `.csproj`. |
| `Resources/Splash/splash.svg` | Edit freely | Splash screen SVG used by `<MauiSplashScreen>` in `.csproj`. Background colour `#512BD4`. |

---

### Attributes - `Attributes/`

| File | Edit? | Description |
|------|-------|-------------|
| `Attributes/AIContextAttribute.cs` | Edit carefully | Defines `[AIContext(string role)]`. `AllowMultiple = true`, `Inherited = false`. Used on classes and methods. See Section 4 for the full role vocabulary. |

---

### Platform-Specific Files - `Platforms/`

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

### Documentation Files

| File | Edit? | Description |
|------|-------|-------------|
| `Agents.md` | **Always update on change** | This file. AI Agent navigation guide. Must be kept in sync with all structural changes to the codebase. |

---

## 3. Architecture & Patterns

### 3.1 MVVM - Manual Implementation

The project uses MVVM **without** CommunityToolkit.Mvvm, Prism, or any other framework.
Every pattern is implemented by hand.

| Layer | Class | Responsibilities |
|-------|-------|-----------------|
| **View** | `*.xaml` + `*.xaml.cs` | Declare UI structure; bind to ViewModel properties and commands; handle navigation gestures only |
| **ViewModel** | `MainViewModel` | Own all application state; expose `ICommand` instances; fire `PropertyChanged`; coordinate the timer |
| **Service** | `CalculationService`, `ThemeService`, `FontSizeService` | Stateless domain logic; no UI references; no `INotifyPropertyChanged` |
| **Model** | `TickerData` (base), `TickerResults` (12 typed subclasses), `TickerCardModel`, `SubsectionState` | `TickerData` is the INPC base; typed subclasses add raw computed fields per ticker |

#### INotifyPropertyChanged pattern used throughout

Every settable property in `MainViewModel` and `TickerData` follows the same
boilerplate - no source generators, no base class helper beyond:

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
```

Setters call `OnPropertyChanged()` with no argument - `[CallerMemberName]` fills the
property name automatically at the call site.

#### ICommand pattern used throughout

All commands are `System.Windows.Input.Command` instances created inline in the
`MainViewModel` constructor. No `RelayCommand`, no `AsyncCommand`:

```csharp
// Pattern: Simple toggle command
ToggleLabCommand = new Command(() => LabExpanded = !LabExpanded);

// Pattern: Refresh command with popup lifecycle coordination
// Note: the 'else' branch runs during startup before MainPage subscribes to RefreshRequested.
RefreshTimeJubileesCommand = new Command(async () =>
{
    if (RefreshRequested != null)
        await RefreshRequested.Invoke(() =>
            TimeJubilees = _calculationService.CalculateTimeJubilees(...));
    else
        TimeJubilees = _calculationService.CalculateTimeJubilees(...);
});
```

#### BindingContext wiring

`MainViewModel` is instantiated **inline in XAML** inside `MainPage.xaml`:

```xml
<ContentPage.BindingContext>
    <viewmodels:MainViewModel />
</ContentPage.BindingContext>
```

`MainPage.xaml.cs` accesses the XAML-constructed instance by casting `BindingContext`:

```csharp
if (BindingContext is MainViewModel vm)
    vm.RefreshRequested += OnTickerRefreshRequested;
```

For popup pages (`SettingsPopup`, `ChangeDatePopup`, `MainMenuPopup`), the ViewModel
is passed as a **constructor argument** and assigned to `BindingContext` in
code-behind - not constructed by XAML.

---

### 3.2 Data Binding

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

### 3.3 Data Flow

```
USER ACTION
    |
    +-- Taps TimelineHeading / menu "Change Date"
    |       --> MainPage.OnTimelineHeadingTapped
    |               --> Navigation.PushModalAsync(new ChangeDatePopup(vm))
    |                       --> ChangeDatePopup.OnOkClicked
    |                               --> MainViewModel.SaveDate(name, date)
    |                                       sets _baseDateName, _baseDateValue, _baseDate
    |                                       --> UpdateAllCalculations()
    |                                               --> UpdateStaticCalculations() [6 tickers]
    |                                               --> UpdateLiveCalculations()   [4 tickers]
    |
    +-- Taps refresh icon on a static ticker card
    |       --> RefreshXxxCommand.Execute()
    |               --> RefreshRequested.Invoke(callback)
    |                       --> MainPage.OnTickerRefreshRequested(callback)
    |                               --> Navigation.PushModalAsync(new RefreshingPopup(callback))
    |                                       [3-second auto-dismiss in RefreshingPopup.OnAppearing]
    |                                       --> callback()
    |                                               --> Typed result property set on ViewModel
    |
    +-- Taps section chevron
    |       --> ToggleXxxCommand.Execute()
    |               --> XxxExpanded = !XxxExpanded --> OnPropertyChanged()
    |                       --> IsVisible on collapsible VerticalStackLayout updates
    |
    +-- Taps ticker card chevron
            --> ToggleXxxCommand.Execute() [card level]
                    --> XxxExpanded = !XxxExpanded --> OnPropertyChanged()
                            --> IsVisible on FullText Label updates
                            --> BoolToImageSource on ImageButton source updates

TIMER (every 1 second, thread-pool thread)
    |
    --> System.Timers.Timer.Elapsed
            --> MainThread.BeginInvokeOnMainThread(UpdateLiveCalculations)
                    --> CalculateCountdown()       --> Countdown.BriefText/FullText
                    --> CalculateLifeOdometer()    --> LifeOdometer.BriefText/FullText
                    --> CalculateGalacticCommute() --> GalacticCommute.BriefText/FullText
                    --> CalculatePhotonPath()      --> PhotonPath.BriefText/FullText
                    --> CalculateCosmicStretch()    --> CosmicStretch.BriefText/FullText
                    --> CalculateYourBreath()       --> YourBreath.BriefText/FullText
                    --> CalculateGlobalCrowd()      --> GlobalCrowd.BriefText/FullText
                    --> CalculateSpaceWait()       --> SpaceWait.BriefText/FullText
                    --> CalculateVibrantHumanity() --> VibrantHumanity.BriefText/FullText
                    --> GetRandomTeaseText()       --> TeaseText
                    --> (LIVE badge breathing animation runs independently via Animation.Commit in MainPage.OnAppearing)
                            each setter fires PropertyChanged
                                --> {Binding Xxx.BriefText} Labels repaint
```

**Critical constraint:** the timer fires on a thread-pool thread. `UpdateLiveCalculations`
is always marshalled back via `MainThread.BeginInvokeOnMainThread` before any bound
property is set. Never set a bound ViewModel property from a background thread.

---

### 3.4 Localisation Flow

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

### 3.5 Theme and Font Size Flow

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

### 3.6 Cross-Platform Image Tinting

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

### 3.7 Modal Navigation Pattern

All secondary UI surfaces are modal pages. One consistent pattern governs all of them.

**Basic push/pop:**:

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

### 3.8 Settings Persistence

All user preferences use `Microsoft.Maui.Storage.Preferences` (maps to
`SharedPreferences` on Android, `NSUserDefaults` on iOS/Mac, Registry on Windows).

| Preference key | Type | Default | Read in | Written in |
|----------------|------|---------|---------|------------|
| `"ColorScheme"` | `string` | `"DefaultDark"` | `App.xaml.cs` ctor | `MainViewModel.ColorScheme` setter |
| `"TextSize"` | `string` | `"Normal"` | `App.xaml.cs` ctor | `MainViewModel.TextSize` setter |
| `"DisplayLanguage"` | `string` | `"Default"` | `App.xaml.cs` ctor | `MainViewModel.DisplayLanguage` setter |
| `"UseMetric"` | `bool` | `true` | `MainViewModel` ctor | `MainViewModel.UseMetric` setter |

---
