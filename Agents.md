# Agents.md - AI Agent Navigation Guide for Aeonpulse

> **Last updated:** 2026-03-27
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
| **9.12** | Quick Violation Checklist | 16-item YES/NO checklist. Run before every commit. |
| **9.13** | Commit Signature | AI agent signature trailer format, ``+ manual changes`` rule. |

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

Each ticker card has a `BriefText` (always visible) and a `FullText` (shown when expanded).
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
| `TickerResults.cs` | Edit freely | `DataTransferObject` | Defines the 13 typed result subclasses (`TimeJubileesResult`, `CountdownResult`, `LifeOdometerResult`, `AlienAnniversariesResult`, `GalacticCommuteResult`, `PhotonPathResult`, `CosmicStretchResult`, `HumanBirthRankResult`, `BirthRuneResult`, `PersonalYearResult`, `GlobalExhaleResult`, `YourBreathResult`, `VibrantCosmosResult`) each extending `TickerData` with raw computed fields. Also defines the `PhotonPhase` enum, `CellularRefreshResult`, `GlobalCrowdResult`, and `LifeLogResult`. Linked into `Aeonpulse.Tests` via `<Compile Link=...>`. |
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
| `MainViewModel.cs` | Edit freely | - | The central state hub. Implements `INotifyPropertyChanged` manually (no toolkit). Owns: all 14 typed ticker result properties (`TimeJubileesResult`, `CountdownResult`, `CosmicStretchResult`, `YourBreathResult`, `VibrantCosmosResult`, etc. - see `TickerResults.cs`); 4 section `bool XxxExpanded` properties; 13 card `bool XxxExpanded` properties; settings properties (`UseMetric`, `ColorScheme`, `TextSize`, `DisplayLanguage`, `BaseDateName`, `BaseDateValue`, `BaseDate`); all `ICommand` instances (toggle + refresh); the 1-second `System.Timers.Timer`; and the `event Func<Action, Task>? RefreshRequested` event used to coordinate the `RefreshingPopup` lifecycle. `SaveDate()` is the only correct entry point for changing the base date. `UpdateStaticCalculations()` recalculates 8 tickers; `UpdateLiveCalculations()` recalculates 6 tickers + `TeaseText`; `UpdateVibrantCosmos()` is called every 200 ms by a dedicated `_vibrantCosmosTimer`. |
| `LocalizedResources.cs` | Edit freely | - | Singleton (`Instance`). A thin passthrough wrapper: every property is `=> AppResources.SomeKey`. Bound in XAML as `{Binding Loc.PropertyName}`. `Invalidate()` fires `PropertyChanged(string.Empty)` which causes every bound property to re-read from `AppResources` with the newly-set culture. When adding a new localised string: add the `AppResources` key, then add the passthrough property here. |

---

### Views - `Views/`

All XAML files must be saved as **UTF-8 with BOM**. All colour/font-size references must use `DynamicResource`. All user-visible text must bind via `{Binding Loc.Xxx}` or `{x:Static resources:AppResources.Xxx}`.

| File | Edit? | AIContext | Description |
|------|-------|-----------|-------------|
| `MainPage.xaml` | Edit freely | *(XAML AI comments)* | The application's only persistent page. 3-row root `Grid`: Row 0 = NavBar (`Border` + inner `Grid`, logo `Image` with `ImageTint`, app name `Label`, hamburger `Button`). Row 1 = TimelineHeading (`Border` + `HorizontalStackLayout` with `FormattedString`). Row 2 = `ScrollView` containing four `CardFrame`-styled `Border` elements (Lab, Cosmos, Mirror, Eco Echoes), each holding a header `Grid` and a collapsible `VerticalStackLayout` of ticker card `Border` elements. Uses `BoolToImageSource` converter for chevron icons. |
| `MainPage.xaml.cs` | Edit carefully | `NavigationCoordinator` | Code-behind for `MainPage`. **Contains no business logic.** Responsibilities: subscribe to `MainViewModel.RefreshRequested` in constructor; implement `OnMenuClicked`, `OnTimelineHeadingTapped`, `OnLogoTapped` (opens `TeasePopup` anchored below NavBar, left-aligned, with Copy-to-clipboard and Close buttons); implement 11 `OnXxxInfoClicked` handlers that push `DeepDivePopup`; implement `OnTickerRefreshRequested` that pushes `RefreshingPopup`. Guard flags (`_isXxxOpen`) on every push prevent double-open. `OpenDeepDiveAsync()` measures `NavBar.Height + TimelineHeading.Height` to pass as `topOffset` to `DeepDivePopup`. Holds 15 guard bools (14 deep-dive/popup guards + `_isTeasePopupOpen`). |
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
| `TintHelper.cs` | Edit carefully | `PlatformTintImplementation` | Real per-pixel colour tinting using Win2D (`Microsoft.Graphics.Canvas`). Both `ApplyImageTint` and `ApplyImageButtonTint`: (1) obtain the source filename from `handler.VirtualView.Source` as a `FileImageSource`, appending `.scale-100` for the Windows resizetizer filename; (2) load the file via `System.IO.File.OpenRead()` + `AsRandomAccessStream()` (avoids WinRT `StorageFile` issues in unpackaged apps); (3) apply a `ColorMatrixEffect` that zeroes source RGB and injects the tint as a constant offset while `M44=1` preserves alpha; (4) render to a `CanvasRenderTarget`, copy to a `WriteableBitmap`, and set it as `Image.Source`. Results are cached by `(filename, colour)`. For `ImageButton`, the inner `Image` is found via `VisualTreeHelper`, deferring to `Loaded` if the template is not yet applied. **Hidden dependency:** requires `<WindowsPackageType>None</WindowsPackageType>` so scaled PNGs exist in `AppContext.BaseDirectory` at runtime. |
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

### Test Project - `Aeonpulse.Tests/`

| File | Edit? | Description |
|------|-------|-------------|
| `Aeonpulse.Tests.csproj` | Edit freely | xUnit test project targeting `net9.0`. Links `CalculationService.cs`, `AeonLog.cs`, `TickerData.cs`, `TickerResults.cs`, `AIContextAttribute.cs`, and `AppResources.Designer.cs` directly from the main project via `<Compile Link=...>` items. Embeds `.resx` files so `ResourceManager` resolves strings at test runtime. No MAUI reference required. |
| `Helpers/TestFixture.cs` | Edit freely | Shared setup helper. `InitEnglish()` pins `AppResources.Culture` to `en` before each test class so string assertions are locale-stable on any CI machine. |
| `FindNearestJubileeTests.cs` | Edit freely | Tests for the `internal static FindNearestJubilee()` algorithm covering all four jubilee families and boundary values. |
| `ReduceToSingleDigitTests.cs` | Edit freely | Tests for the `internal static ReduceToSingleDigit()` digital-root algorithm. |
| `CalculateLifeOdometerTests.cs` | Edit freely | Tests for `CalculateLifeOdometer` with injected `now`. |
| `CalculateCountdownTests.cs` | Edit freely | Tests for all three countdown format branches (HH:MM:SS, days+hours, days-only). |
| `CalculateAlienAnniversariesTests.cs` | Edit freely | Tests for Mars/Venus year calculations with fixed planetary constants. |
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
| `TypedResultFieldTests.cs` | Edit freely | Tests that each CalculationService method correctly populates the raw numeric fields of its typed result subclass - not covered by the existing string-assertion tests. Uses injected now for deterministic results. Covers CountdownResult decomposition, LifeOdometerResult formulas, AlienAnniversariesResult planet-year formulas, GalacticCommuteResult km calculation, PhotonPathResult phase and speed-of-light check, HumanBirthRankResult rank ordering, PersonalYearResult range, GlobalExhaleResult flags, TimeJubileesResult coherence. |
| `README.md` | Edit freely | High-level project README. Human-facing. Contains a project structure overview and migration notes from the original React implementation. |
| `IMPLEMENTATION_GUIDE.md` | Edit freely | Original React-to-MAUI migration guide. Contains early extension recipes and build commands. Some content is superseded by `Agents.md`. |

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
                    --> GetRandomTeaseText()       --> TeaseText
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
| `Views/MainPage.xaml` | ~970 | Cellular Refresh ticker card header `Grid` - 3-column `[DNA emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.CellularRefresh` (live-updated every second). |
| `Views/MainPage.xaml` | ~710 | Vibrant Cosmos ticker card header `Grid` - 3-column `[sparkles emoji|title+LIVE badge|info+expand buttons]`, bound to `MainViewModel.VibrantCosmos` (live-updated every 200 ms). |
| `Views/MainPage.xaml` | 243 | Title + LIVE badge `HorizontalStackLayout` - side-by-side layout reason |
| `Views/SettingsPopup.xaml` | 9 | Full-screen overlay `Grid` - Layer 0/1 z-order explanation |
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

#### When Adding New XAML

Add an `<!-- AI: ... -->` comment in the following situations:

| Situation | What to document |
|-----------|-----------------|
| New page or popup (file level) | How it is pushed, what `BindingContext` it uses, any geometry injection |
| New multi-row/column `Grid` | Row and column slot assignments for every slot |
| New collapsible section | The `IsVisible` binding source and what it contains |
| New ticker card | The 3-column header layout, which typed result property it binds to (`XxxResult.BriefText`/`FullText`), whether it is live or static |
| Any non-obvious binding | The full ViewModel path and whether it is live-localised or static |
| Any layout trick or workaround | Why the approach was chosen and what alternative was rejected |

---

### 4.3 XML Documentation Comments (`///`) - C# Files

#### Purpose

XML doc comments provide IntelliSense-level documentation and serve as the primary
in-code explanation of **why** a method exists, what **side effects** it has, and
what **hidden dependencies** it carries. They are the main documentation channel
for agents reading `.cs` files.

#### Tags in Use

| Tag | Used for | Usage frequency |
|-----|----------|----------------|
| `<summary>` | One-paragraph description of the symbol's purpose | All public symbols (125 occurrences) |
| `<param>` | Each method parameter - name and semantic meaning | All non-obvious parameters (65 occurrences) |
| `<para>` | Additional paragraphs within `<summary>` for side effects, hidden deps, algorithm notes | Complex methods (53 occurrences) |
| `<returns>` | Return value description | All non-void public methods (13 occurrences) |
| `<list>` | Bullet or numbered lists inside `<summary>` or `<para>` | Algorithm step lists (7 occurrences) |
| `<see cref="...">` | Cross-reference to another type or member | Throughout |
| `<c>` | Inline code span within prose | Throughout |
| `<b>` | Bold emphasis for key terms inside `<para>` | Side effect headings |

#### Standard Summary Structure for Complex Methods

```csharp
/// <summary>
/// One-sentence description of what this method does.
///
/// <para>
/// <b>Algorithm / Design note:</b> explanation of the approach,
/// why it was chosen, and any significant trade-offs.
/// </para>
/// <para>
/// <b>Side effects / Hidden dependencies:</b> anything that this method
/// changes or depends on beyond its explicit parameters and return value.
/// <list type="bullet">
///   <item><description>Side effect 1</description></item>
///   <item><description>Side effect 2</description></item>
/// </list>
/// </para>
/// </summary>
/// <param name="paramName">Semantic meaning. Units if numeric. Source if injected.</param>
/// <returns>What the return value represents and when it varies.</returns>
```

#### Real Examples from the Codebase

**Class-level summary with hidden dependency list** (`App.xaml.cs`):
```csharp
/// <summary>
/// Application entry point. Responsible for bootstrapping all singleton services
/// (theme, font size, language) from <see cref="Preferences"/> <b>before</b>
/// <see cref="Application.InitializeComponent"/> runs, ensuring that every
/// <c>DynamicResource</c> and <c>AppResources</c> binding gets the correct
/// persisted value on the very first rendered frame.
///
/// <para>
/// <b>Side effects / hidden dependencies:</b>
/// <list type="bullet">
///   <item><description>
///     <see cref="ThemeService.ApplyScheme"/> mutates <c>Application.Current.Resources</c>
///     before <c>InitializeComponent</c> has merged the XAML resource dictionaries.
///     This works because MAUI merges dictionaries lazily during first UI inflate.
///   </description></item>
/// </list>
/// </para>
/// </summary>
```

**Method with algorithm note and param/returns** (`CalculationService.cs`):
```csharp
/// <summary>
/// Finds the nearest "jubilee" (a round, memorable milestone) that is
/// strictly greater than <paramref name="diff"/>.
///
/// <para>
/// The algorithm searches four jubilee families in order, then returns
/// the smallest candidate that beats <paramref name="diff"/>:
/// <list type="number">
///   <item><description>Major power-of-10 (10, 100, 1000 ...)</description></item>
///   <item><description>Minor leading-digit multiple (5, 20, 300 ...)</description></item>
///   <item><description>Quarter fractions (25, 250, 750 ...)</description></item>
///   <item><description>Repeating-digit "nice" numbers (111, 2222 ...)</description></item>
/// </list>
/// </para>
/// </summary>
/// <param name="diff">The elapsed count (days, weeks, months, etc.) since the base date.</param>
/// <returns>The smallest jubilee value greater than <paramref name="diff"/>.</returns>
```

#### Placement Rules

1. Every `public` class, method, and property must have a `<summary>`.
2. Every `public` method parameter must have a `<param>` tag if its purpose is not obvious from its name alone.
3. Side effects and hidden dependencies must be documented in `<para><b>Side effects / Hidden dependencies:</b>` blocks - this is the single most important convention for AI agents navigating the codebase.
4. `internal` and `private` members do not require XML doc comments unless they implement a named algorithm (in which case a `<summary>` is required).
5. Do **not** use XML doc comments for `//` line comments or `/* */` block comments - keep those as plain `//` comments.

---

### 4.4 Markup Consistency Rules

The following rules apply whenever any markup is added or modified:

#### The Four-Sync Rule

Any structural change to the codebase requires updating all four locations:

| Change | Must also update |
|--------|-----------------|
| New class or significant method | `[AIContext]` attribute + `///` summary + Section 4 role table (if new role) |
| New XAML element with non-obvious layout or binding | `<!-- AI: ... -->` comment above the element |
| New localised string | `<!-- Binding: ... -->` comment at the binding site in XAML |
| New role string | Row in Section 4.1 role vocabulary table |
| Any structural change | `Agents.md` Section 2 file description update |

#### ASCII-Only Constraint in Comment Blocks

Non-ASCII characters (emoji, Unicode dashes `--`, box-drawing `---`, ellipsis `...`)
are **forbidden** inside all comment blocks in both `.cs` and `.xaml` files:

```xml
<!-- WRONG: non-ASCII dash and ellipsis -->
<!-- AI: Menu panel -- HorizontalOptions=End... -->

<!-- CORRECT: ASCII only -->
<!-- AI: Menu panel - HorizontalOptions=End + right Margin (set in code-behind). -->
```

```csharp
// WRONG: em-dash in comment
// NavigationCoordinator -- translates gestures to modal push/pop

// CORRECT
// NavigationCoordinator - translates gestures to modal push/pop
```

Non-ASCII characters are safe **only** inside element attribute values in XAML:
```xml
<Label Text="??" />   <!-- safe: attribute value, not a comment -->
```

The reason: XAML files lacking a UTF-8 BOM cause `MSB4018 XamlCTask` crashes when
non-ASCII bytes appear in XML comment nodes, which the XML parser reads differently
from element content.

#### Do Not Use XML Doc Comments in XAML

XML doc `///` comments have no meaning in XAML and will cause parse errors.
Use `<!-- AI: ... -->` and `<!-- Binding: ... -->` in XAML exclusively.

#### Do Not Use `<!-- AI: -->` in C# Files

Use `///` XML doc comments and plain `//` comments in C# files.
The `<!-- AI: -->` convention is XAML-only.

---

## 5. Knowledge Graph Entry Points

This section defines the **root nodes** of the application's knowledge graph -
the symbols an AI agent should navigate to first when reasoning about any part of
the codebase. Each node entry states its file, its `[AIContext]` role, its primary
responsibilities, every outbound dependency it owns, and every inbound call site
that reaches it.

---

### How to Read This Section

```
Node title
  File:          the source file path
  AIContext:     the [AIContext] role(s) on the class or method
  Responsibility: what this node does
  Owns:          symbols / resources this node creates or controls
  Calls:         outbound dependencies this node invokes
  Called by:     inbound callers (who depends on this node)
  Extend here:   what to add to this node when extending the app
```

---

### Node Map (Dependency Order, Outermost First)

```
Platform entry points  (Android/iOS/Mac/Windows/Tizen)
    |
    v
MauiProgram.CreateMauiApp()
    |
    v
App  (App.xaml + App.xaml.cs)
    |
    +-- ThemeService.Instance
    +-- FontSizeService.Instance
    +-- MainViewModel.ApplyLanguage()  [static]
    |
    v
MainPage  (Views/MainPage.xaml + .xaml.cs)
    |
    +-- MainViewModel  (ViewModels/MainViewModel.cs)
    |       |
    |       +-- CalculationService  (Services/CalculationService.cs)
    |       +-- ThemeService.Instance
    |       +-- FontSizeService.Instance
    |       +-- LocalizedResources.Instance  (ViewModels/LocalizedResources.cs)
    |               |
    |               +-- AppResources.resx  (Resources/AppResources.resx)
    |
    +-- ImageTint  (Helpers/ImageTint.cs)
    |       |
    |       +-- Platform TintHelper.cs  (Platforms/{Platform}/TintHelper.cs)
    |
    +-- Modal stack popups (all pushed by MainPage.xaml.cs)
            +-- MainMenuPopup
            +-- ChangeDatePopup
            +-- SettingsPopup
            +-- DeepDivePopup
            +-- RefreshingPopup
```

---

### Root Node 1 - `MauiProgram`

```
File:        MauiProgram.cs
AIContext:   AppBootstrap
```

**Responsibilities:**
- Constructs the `MauiApp` host. Called exactly once, before `App` is instantiated, from every platform entry point.
- Registers `OpenSansRegular` and `OpenSansSemibold` fonts.
- Registers `ImageTint.ColorProperty` mapper callbacks on `ImageHandler.Mapper` and `ImageButtonHandler.Mapper` globally, making tinting available to every `Image` and `ImageButton` in the app.
- Declares `partial` stubs `ApplyImageTint` and `ApplyImageButtonTint` - the build system selects the correct platform implementation at compile time.
- Enables `AddDebug()` logging in `#if DEBUG` builds.

**Owns:**
- `MauiApp` instance (returned to platform entry point)
- Global handler mapper registrations (affect every `Image`/`ImageButton` app-wide)

**Calls:**
- `ImageTint.GetColor(bindable)` - reads the attached property value
- `ApplyImageTint(handler, tint)` / `ApplyImageButtonTint(handler, tint)` - platform `partial` methods in `Platforms/{Platform}/TintHelper.cs`

**Called by:**
- `Platforms/Android/MainApplication.cs` - `CreateMauiApp()`
- `Platforms/iOS/AppDelegate.cs` - `CreateMauiApp()`
- `Platforms/MacCatalyst/AppDelegate.cs` - `CreateMauiApp()`
- `Platforms/Windows/App.xaml.cs` - `CreateMauiApp()`
- `Platforms/Tizen/Main.cs` - `CreateMauiApp()`

**Extend here when:**
- Adding a new NuGet package that requires builder registration
- Adding a new cross-platform handler mapper (e.g., a new attached property)
- Registering services in the DI container (currently none are registered)

---

### Root Node 2 - `App`

```
File:        App.xaml  +  App.xaml.cs
AIContext:   AppBootstrap
```

**Responsibilities:**
- Executes **before `InitializeComponent()`** to apply all persisted user preferences on the first rendered frame:
  1. Reads `"ColorScheme"` from `Preferences` - calls `ThemeService.Instance.ApplyScheme()`
  2. Reads `"TextSize"` from `Preferences` - calls `FontSizeService.Instance.ApplyPreset()`
  3. Reads `"DisplayLanguage"` from `Preferences` - calls `MainViewModel.ApplyLanguage()` (static)
- Calls `InitializeComponent()` which merges `Colors.xaml` then `Styles.xaml` into `Application.Current.Resources`.
- Sets `MainPage = new MainPage()` (note: `.NET 9` obsolete setter - do not add new usages).
- `App.xaml` merges the two global resource dictionaries in the correct load order: `Colors.xaml` first (tokens), `Styles.xaml` second (references tokens).

**Owns:**
- Application-level `ResourceDictionary` (merged dictionaries)
- `MainPage` instance

**Calls:**
- `ThemeService.Instance.ApplyScheme(string)` - mutates `Application.Current.Resources`
- `FontSizeService.Instance.ApplyPreset(string)` - mutates `Application.Current.Resources`
- `MainViewModel.ApplyLanguage(string)` - static; sets `CultureInfo` globally and `AppResources.Culture`
- `InitializeComponent()` - merges XAML resource dictionaries

**Called by:**
- `MauiProgram.CreateMauiApp()` via `.UseMauiApp<App>()`

**Critical ordering constraint:**
`ThemeService`, `FontSizeService`, and `ApplyLanguage` must run **before** `InitializeComponent()`. If moved after, the first rendered frame will flash the wrong colours, font sizes, and language strings.

**Extend here when:**
- Adding a new persisted preference that must be applied before the first frame
- Adding application-level resources (add `<ResourceDictionary Source="..."/>` in `App.xaml`)

---

### Root Node 3 - `MainPage`

```
File:        Views/MainPage.xaml  +  Views/MainPage.xaml.cs
AIContext:   NavigationCoordinator
```

**Responsibilities:**
- The application's **only persistent page**. All other UI is pushed modally on top of it and eventually returns here.
- XAML declares the entire main UI: 3-row root grid (NavBar, TimelineHeading, ScrollView with 4 collapsible sections x 10 ticker cards).
- XAML constructs `MainViewModel` inline as `ContentPage.BindingContext` - the only place the ViewModel is constructed by XAML.
- Code-behind (`MainPage.xaml.cs`) acts as the **navigation coordinator** only:
  - Subscribes to `MainViewModel.RefreshRequested` in constructor to wire the `RefreshingPopup` lifecycle
  - Routes 3 gesture events (`OnLogoTapped`, `OnMenuClicked`, `OnTimelineHeadingTapped`) to modal pushes
  - Implements 10 `OnXxxInfoClicked` handlers, each pushing `DeepDivePopup` with ticker-specific content
  - Implements `OpenDeepDiveAsync` shared helper measuring `NavBar.Height + TimelineHeading.Height` for `topOffset`
  - Holds 13 `_isXxxOpen` guard bools preventing double-push on rapid taps

**Owns:**
- Modal navigation stack (pushes and controls all 5 popup types)
- All 13 popup guard flags
- `RefreshRequested` subscription

**Calls:**
- `Navigation.PushModalAsync(popup)` - pushes all popup types
- `MainViewModel` (via `BindingContext` cast) - reads `TeaseText`, `RefreshRequested`
- `NavBar.Height`, `TimelineHeading.Height` - measured at open time for `DeepDivePopup` and `MainMenuPopup` positioning

**Called by:**
- `App.xaml.cs` - instantiated as `MainPage = new MainPage()`

**Extend here when:**
- Adding a new popup: add guard bool, push method, and guard pattern
- Adding a new ticker card: add `OnXxxInfoClicked` handler calling `OpenDeepDiveAsync`
- Adding a new gesture on the main page (NavBar, TimelineHeading, or section header)

---

### Root Node 4 - `MainViewModel`

```
File:        ViewModels/MainViewModel.cs
AIContext:   (none - state orchestrator)
```

**Responsibilities:**
- The **central application state hub**. Every bound value in the UI originates here.
- Owns all 12 typed ticker result properties (`TimeJubileesResult`, `CountdownResult`, `CosmicStretchResult`, `YourBreathResult`, etc.), each a subclass of `TickerData` carrying both the display strings and raw computed values.
- Owns 4 section expansion bools (`LabExpanded` etc.) and 11 card expansion bools (`TimeJubileesExpanded` etc.).
- Owns user settings properties: `UseMetric`, `ColorScheme`, `TextSize`, `DisplayLanguage`, `BaseDateName`, `BaseDateValue`, `BaseDate`.
- Each settings setter immediately applies the change (`ThemeService`, `FontSizeService`, `ApplyLanguage`) **and** persists it via `Preferences`.
- Owns 17 `ICommand` instances (4 section toggles, 11 card toggles, 3 card refreshes, 2 bulk refresh commands).
- Owns the `event Func<Action, Task>? RefreshRequested` - the bridge between ViewModel refresh commands and `MainPage`'s `RefreshingPopup` lifecycle.
- Owns the 1-second `System.Timers.Timer` that calls `UpdateLiveCalculations()` on the UI thread via `MainThread.BeginInvokeOnMainThread`.
- `SaveDate(name, date)` is the **only correct entry point** for updating the base date - sets all three backing fields atomically before calling `UpdateAllCalculations()` once.
- `ApplyLanguage(string)` is `static` so `App.xaml.cs` can call it before the ViewModel is constructed.
- `Loc { get; } = LocalizedResources.Instance` - exposes the localisation singleton so XAML binds as `{Binding Loc.Xxx}`.

**Owns:**
- All 15 typed ticker result instances (`TimeJubileesResult`, `CountdownResult`, `CosmicStretchResult`, `YourBreathResult`, `VibrantCosmosResult`, `LifeLogResult`, etc. - all subclasses of `TickerData`)
- All section/card `bool` expanded states (18 total, including `VibrantCosmosExpanded`, `LifeLogExpanded`)
- All user settings state
- The 1-second live-update timer and the 200 ms `_vibrantCosmosTimer` (dedicated to Vibrant Cosmos)
- All `ICommand` instances
- `RefreshRequested` event

**Calls:**
- `CalculationService` - all 11 ticker calculation methods + `GetRandomTeaseText`
- `ThemeService.Instance.ApplyScheme(string)` - from `ColorScheme` setter
- `FontSizeService.Instance.ApplyPreset(string)` - from `TextSize` setter
- `MainViewModel.ApplyLanguage(string)` - from `DisplayLanguage` setter
- `Loc.Invalidate()` - from `DisplayLanguage` setter, forces UI rebind
- `Preferences.Default.Set(...)` - persists `ColorScheme`, `TextSize`, `DisplayLanguage`, `UseMetric`
- `MainThread.BeginInvokeOnMainThread(UpdateLiveCalculations)` - from timer callback

**Called by:**
- `MainPage.xaml` - constructed inline as `BindingContext`
- `MainPage.xaml.cs` - cast from `BindingContext` to subscribe to `RefreshRequested`
- `SettingsPopup.xaml.cs` - injected via constructor; reads and writes settings properties
- `ChangeDatePopup.xaml.cs` - injected via constructor; calls `SaveDate()`
- `MainMenuPopup.xaml.cs` - injected via constructor; reads `Loc` for menu labels
- `App.xaml.cs` - calls `ApplyLanguage()` (static) before ViewModel construction

**Extend here when:**
- Adding a new ticker: add a typed `XxxResult` property (subclass of `TickerData`), `bool XxxExpanded` property, toggle `ICommand`, refresh `ICommand` (if static), wire in `UpdateStaticCalculations()` or `UpdateLiveCalculations()`
- Adding a new section: add `bool XxxExpanded` property and `ToggleXxxCommand`
- Adding a new setting: add property with apply+persist setter pattern; read default in constructor

---

### Root Node 5 - `CalculationService`

```
File:        Services/CalculationService.cs
AIContext:   CoreCalculationEngine
```

**Responsibilities:**
- The **sole domain logic class**. Stateless. All 11 ticker calculations live here as separate `public` methods.
- Reads `DateTime.Now` internally - not a pure function by design; produces different output on each call (intentional for live tickers).
- Reads all output strings from `AppResources` at call time - strings automatically reflect whichever culture `MainViewModel.ApplyLanguage()` has set.
- Never writes global state. Thread-safe. Safe to call from background timer threads.
- Contains three private helpers: `FindNearestJubilee(long diff)`, `ReduceToSingleDigit(int num)`, and `GetPopulationByDate(DateTime date)` (piecewise population model used by `CalculateGlobalCrowd`).
- `CalculatePhotonPath` carries an inline 57-star distance catalogue (anonymous-type array defined inside the method body).
- `CalculateHumanBirthRank` and `CalculateGlobalExhale` embed scientific dataset constants sourced from PRB and Global Carbon Budget 2025 respectively.

**Ticker method registry:**

| Method | AIContext | Update type | Inputs beyond `baseDate` |
|--------|-----------|-------------|--------------------------|
| `CalculateTimeJubilees` | `CoreCalculation` | Static | `baseDateName`, `baseDateValue` |
| `CalculateCountdown` | `LiveTicker` | LIVE (1s) | *(none)* |
| `CalculateLifeOdometer` | `LiveTicker` | LIVE (1s) | `baseDateName`, `baseDateValue` |
| `CalculateAlienAnniversaries` | `CoreCalculation` | Static | `baseDateName`, `baseDateValue` |
| `CalculateGalacticCommute` | `LiveTicker` | LIVE (1s) | `baseDateValue`, `useMetric` |
| `CalculatePhotonPath` | `LiveTicker`, `StarCatalogueLookup` | LIVE (1s) | `baseDateValue`, `useMetric` |
| `CalculateCosmicStretch` | `LiveTicker` | LIVE (1s) | `baseDateValue`, `useMetric` |
| `CalculateHumanBirthRank` | `CoreCalculation`, `ExternalDataModel` | Static | `baseDateName` |
| `CalculateBirthRune` | `CoreCalculation` | Static | `baseDateValue` |
| `CalculatePersonalYear` | `CoreCalculation` | Static | `baseDateValue` |
| `CalculateGlobalExhale` | `CoreCalculation`, `ExternalDataModel` | Static | `baseDateName`, `baseDateValue`, `useMetric` |
| `CalculateYourBreath` | `LiveTicker` | LIVE (1s) | `baseDateValue`, `useMetric` |
| `CalculateCellularRefresh` | `CoreCalculation` | Static | `baseDateName`, `baseDateValue` |
| `CalculateGlobalCrowd` | `LiveTicker` | LIVE (1s) | *(none)* |
| `CalculateLifeLog` | `CoreCalculation` | Static | `baseDateName`, `baseDateValue`, optional `rand` for brief-text randomisation |
| `GetRandomTeaseText` | `UIPresentation` | LIVE (1s) | `CountdownResult`, `LifeOdometerResult`, `GalacticCommuteResult`, `GlobalExhaleResult`, `baseDateName`, `baseDate` (`DateTime`, formatted with `"d"` inside) - returns 1 of 5 random tease strings |

**Owns:**
- All domain computation logic
- 57-star distance catalogue (inline in `CalculatePhotonPath`)
- 24-rune calendar definitions (inline in `CalculateBirthRune`)
- 9 personal-year interpretations (via `AppResources`)
- PRB birth-rank piecewise model constants
- Global Carbon Budget polynomial regression constants

**Calls:**
- `AppResources.*` - all output string keys, read at call time
- `DateTime.Now` - internal, every method
- `AeonLog.Debug` - entry log on all `Calculate*` methods; `[BLOCK]`-tagged phase logs in `CalculatePhotonPath` and `CalculateTimeJubilees`

**Called by:**
- `MainViewModel.UpdateStaticCalculations()` - 6 methods
- `MainViewModel.UpdateLiveCalculations()` - 5 methods + `GetRandomTeaseText`
- `MainViewModel` refresh command lambdas - 5 specific methods (TimeJubilees, AlienAnniversaries, GlobalExhale, CellularRefresh, LifeLog)

**Extend here when:**
- Adding a new ticker: add a new `public XxxResult CalculateXxx(...)` method returning a typed subclass of `TickerData` defined in `TickerResults.cs`. Decorate with `[AIContext]`, add `///` docs, source all strings from `AppResources` only. No UI references. Add an `AeonLog.Debug` entry call; add `[BLOCK]`-tagged calls only if the method has named internal phases.
- Adding a new star to the catalogue: add entry to the inline `stars` array in `CalculatePhotonPath`
- Updating scientific dataset constants: update the relevant method's inline data and its `///` `ExternalDataModel` comment with the new source citation

---

### Root Node 6 - `LocalizedResources`

```
File:        ViewModels/LocalizedResources.cs
AIContext:   (none - localisation hub)
```

**Responsibilities:**
- The **live binding bridge** between `AppResources.resx` and the XAML UI.
- Singleton (`static readonly Instance`). Every XAML `{Binding Loc.Xxx}` expression resolves through this instance.
- Every property is a simple passthrough getter: `public string MyKey => AppResources.MyKey;`
- `Invalidate()` fires `PropertyChanged(string.Empty)` which causes MAUI's binding engine to re-read **every** property on this object simultaneously - the mechanism that makes language switching instant without page reload.
- Contains passthrough properties for all 365 string keys in `AppResources.resx`, grouped by: AppName/Badge, Timeline, Sections (4), Ticker titles (11), Settings, ChangeDate, MainMenu, DeepDive/Info (30), Units (metric + imperial), Stars (57), Runes (24), PersonalYear interpretations (9), Tease, Refreshing.

**Owns:**
- The `Invalidate()` mass-rebind mechanism
- The `INotifyPropertyChanged` implementation for the localisation layer

**Calls:**
- `AppResources.*` - every property getter reads its corresponding key
- `PropertyChanged.Invoke(this, new PropertyChangedEventArgs(string.Empty))` - in `Invalidate()`

**Called by:**
- `MainViewModel` - exposes as `public LocalizedResources Loc { get; } = LocalizedResources.Instance`
- Every XAML `{Binding Loc.Xxx}` binding on `MainPage` and all popup pages
- `MainViewModel.DisplayLanguage` setter - calls `Loc.Invalidate()` after `ApplyLanguage()`

**Extend here when:**
- Adding any new localised string: add one passthrough property per new `AppResources` key
- Changing a key name in `AppResources.resx`: update the property name here and every XAML binding that references it

---

### Root Node 7 - `AppResources.resx`

```
File:        Resources/AppResources.resx  (en)
             Resources/AppResources.ru.resx  (ru)
             Resources/AppResources.Designer.cs  (auto-generated - do not edit)
AIContext:   (none - string repository)
```

**Responsibilities:**
- The **single source of truth for all user-visible strings**. 365 string keys total.
- Organised into named groups by prefix (see key prefix table below).
- `AppResources.Culture` is set globally by `MainViewModel.ApplyLanguage()`. The .NET resource system automatically selects `AppResources.ru.resx` when the culture is `ru`.
- `AppResources.Designer.cs` provides the strongly-typed `AppResources.SomeKey` accessor used throughout `CalculationService` and `LocalizedResources`.

**Key prefix groups:**

| Prefix | Count | Content |
|--------|-------|---------|
| `Star_` | 108 | 57 stars: `_Name`, `_Info` pairs + shared constellation infos |
| `Rune_` | 72 | 24 Elder Futhark runes: `_Name`, `_Brief`, `_Full` triples |
| `Ticker_` | 46 | BriefText/FullText templates for all 11 tickers (multi-variant some tickers) |
| `Info_` | 36 | DeepDivePopup content: `_Title`, `_Method`, `_Source` per ticker |
| `Settings_` | 22 | All settings popup labels and values |
| `Unit_` / `UnitMetric_` / `UnitImperial_` | 20 | Distance, time, and mass unit strings (includes `UnitMetric_Kg`, `UnitImperial_Lbs`, `UnitMetric_BTonnes`, `UnitImperial_BTons`) |
| `PersonalYear1_` ... `PersonalYear9_` | 18 | Brief + Full interpretations for numerology years 1-9 |
| `ChangeDate_` | 7 | Change date popup labels |
| `Tease_` | 7 | Tease popup title, button, and 5 randomly-selected tease templates (`Tease_Countdown`, `Tease_Heartbeats`, `Tease_Breaths`, `Tease_GalacticCommute`, `Tease_GlobalExhale`) |
| `MainMenu_` | 5 | Main menu popup labels |
| `Section_` | 4 | Section header titles (Lab, Cosmos, Mirror, Eco Echoes) |
| Others | ~22 | `AppName`, `Badge_LIVE`, `Timeline_BaseDatePreposition`, `Default_BaseDateName`, `Refreshing_Message` |

**Template token format:**
Ticker text templates use `{placeholder}` tokens replaced by `string.Replace()` in `CalculationService`:
```
Ticker_TimeJubileesBrief = "Next jubilee: {nextJubilee} on {nearestJubileeDate:d}"
```
Tokens are replaced explicitly - no `string.Format`, no numbered `{0}` placeholders.

**Owns:**
- All user-visible strings for en and ru locales
- All scientific/cultural data strings (star names, rune interpretations, etc.)

**Called by:**
- `CalculationService` - reads all ticker output strings at calculation call time
- `LocalizedResources` - every passthrough property reads one key
- XAML `{x:Static resources:AppResources.Key}` - in freshly-constructed popups
- `MainViewModel.ApplyLanguage()` - sets `AppResources.Culture`

**Extend here when:**
- Adding any new user-visible string: add to this file first, then `AppResources.ru.resx`, then `LocalizedResources.cs`, then XAML binding
- Adding a new ticker: add `Ticker_XxxTitle`, `Ticker_XxxBrief`, `Ticker_XxxFull`, `Info_XxxTitle`, `Info_XxxMethod`, `Info_XxxSource`
- Adding a new language: create `AppResources.{culture}.resx` with all 359 keys translated; register in `.csproj`

---

### Root Node 8 - `ThemeService` and `FontSizeService`

```
Files:       Services/ThemeService.cs
             Services/FontSizeService.cs
AIContext:   (none)
```

These two nodes are listed together because they follow an identical architectural pattern.

**Responsibilities:**

`ThemeService`:
- Singleton. Owns 3 colour palettes as `Dictionary<string, Color>`: `_defaultColors` (DefaultDark), `_highContrastDarkColors`, `_highContrastLightColors`.
- `ApplyScheme(string)` overwrites all 12 colour keys in `Application.Current.Resources` in one pass. Every `DynamicResource` colour binding in the entire app repaints immediately.
- Colour keys managed: `SpaceDark`, `SpaceDarker`, `CyberCyan`, `CyberPurple`, `CyberPink`, `NeonGreen`, `NeonGreenDark`, `TextWhite`, `TextDim`, `TextGray`, `CardBackground`, `CardDark`.

`FontSizeService`:
- Singleton. Owns 3 size presets as `Dictionary<string, double>`: `_small`, `_normal`, `_large`.
- `ApplyPreset(string)` overwrites all 5 font-size keys in `Application.Current.Resources`. Every `DynamicResource` font-size binding reflows.
- Font-size keys managed: `FontSizeSmall`, `FontSizeMedium`, `FontSizeLarge`, `FontSizeXLarge`, `FontSizeTitle`.

**Owns:**
- Named palette/preset dictionaries
- `CurrentScheme` / `CurrentPreset` read-only properties

**Calls:**
- `Application.Current.Resources[key] = value` - direct resource dictionary mutation

**Called by:**
- `App.xaml.cs` constructor - on startup to restore persisted preferences
- `MainViewModel.ColorScheme` setter - on user change
- `MainViewModel.TextSize` setter - on user change
- `MainViewModel` constructor - re-applies persisted values after re-reading `Preferences`

**Extend here when:**
- Adding a new colour scheme: add a `const string`, add a new `Dictionary<string, Color>` with values for all 12 keys, add a case to `ApplyScheme`'s switch expression
- Adding a new font-size preset: add a `const string`, add a new `Dictionary<string, double>` with values for all 5 keys, add a case to `ApplyPreset`'s switch expression
- Adding a new colour token (new key): add the key to **all** three palette dictionaries in `ThemeService` and add the key/default to `Colors.xaml`

---

### Root Node 9 - `ImageTint` + Platform `TintHelper` Files

```
Files:       Helpers/ImageTint.cs
             Platforms/Android/TintHelper.cs
             Platforms/iOS/TintHelper.cs
             Platforms/MacCatalyst/TintHelper.cs
             Platforms/Windows/TintHelper.cs
AIContext:   PlatformAbstractionHelper (ImageTint.cs)
             PlatformTintImplementation (TintHelper.cs files)
```

**Responsibilities:**
- `ImageTint.cs` defines the cross-platform `helpers:ImageTint.Color` attached `BindableProperty`.
- `BindableProperty.propertyChanged` callback fires whenever the tint colour changes (including when `DynamicResource` theme swap updates the value).
- Callback calls `view.Handler?.UpdateValue("ColorProperty")` which re-triggers the mapper registered in `MauiProgram`.
- Each `TintHelper.cs` implements the `partial` methods `ApplyImageTint` and `ApplyImageButtonTint` for its platform using native colour filter APIs.

**Platform implementation notes** (hidden dependencies):
- **Android:** `ImageView.SetColorFilter(PorterDuffColorFilter)`. `ImageButton`'s platform view is `ShapeableImageView`, not `android.widget.ImageButton`.
- **iOS:** `UIImageRenderingMode.AlwaysTemplate` + `TintColor`. Applied to `UIImageView` (Image) and `UIButton.CurrentImage` (ImageButton).
- **MacCatalyst:** Same as iOS with `Math.Clamp([0,1])` guards on all color components.
- **Windows:** Both `ApplyImageTint` and `ApplyImageButtonTint` use Win2D `ColorMatrixEffect`. File loaded via `System.IO.File.OpenRead()` + `AsRandomAccessStream()` from `AppContext.BaseDirectory` (Stream overload required in unpackaged apps). Result is a `WriteableBitmap` replacing `Image.Source`. `ImageButton` uses `VisualTreeHelper` to find the inner `Image` child. Requires `WindowsPackageType=None` in `.csproj` and `Microsoft.Graphics.Win2D 1.3.2`.

**Owns:**
- The `ColorProperty` attached `BindableProperty` definition
- The `OnColorChanged` callback that triggers the mapper pipeline

**Calls:**
- `view.Handler?.UpdateValue(nameof(ColorProperty))` - triggers re-invocation of `MauiProgram` mapper

**Called by:**
- XAML `helpers:ImageTint.Color="{DynamicResource XxxColor}"` on any `Image` or `ImageButton`
- `MauiProgram` handler mapper callbacks - invoke `ApplyImageTint` / `ApplyImageButtonTint`
- `DynamicResource` binding system - calls `OnColorChanged` on every theme swap

**Extend here when:**
- Adding tinting to a new platform: add a new `TintHelper.cs` in `Platforms/{NewPlatform}/` implementing the two `partial` methods; register the target framework in `.csproj`
- Changing the tint strategy on an existing platform: edit only the relevant `TintHelper.cs`

---

### Root Node 10 - Modal Popup Classes

```
Files:       Views/MainMenuPopup.xaml(.cs)
             Views/ChangeDatePopup.xaml(.cs)
             Views/SettingsPopup.xaml(.cs)
             Views/DeepDivePopup.xaml(.cs)
             Views/RefreshingPopup.xaml(.cs)
AIContext:   ModalViewController (all)
```

All five popups share the same architectural contract. They are listed as one node because the pattern is uniform.

**Shared pattern:**
- Pushed by `MainPage.xaml.cs` via `Navigation.PushModalAsync(new XxxPopup(...))`.
- Dismissed by their own code-behind via `Navigation.PopModalAsync()`.
- Receive `MainViewModel` (or a callback delegate) as a constructor argument - not via `BindingContext` injection from XAML (except `SettingsPopup` which also sets `BindingContext`).
- No business logic in code-behind. Side effects happen through ViewModel setters.

**Individual responsibilities:**

| Popup | Constructor receives | Primary action | Side effect |
|-------|---------------------|----------------|-------------|
| `MainMenuPopup` | `MainViewModel`, `topOffset`, `rightOffset`, `openChangeDateCallback`, `openSettingsCallback` | Menu item tap: pop self, then invoke callback | Callback pushes next popup on `MainPage`'s stack after this one is fully dismissed (iOS constraint) |
| `ChangeDatePopup` | `MainViewModel` | OK tap: calls `MainViewModel.SaveDate(name, date)` | `SaveDate` atomically updates 3 backing fields and calls `UpdateAllCalculations()` |
| `SettingsPopup` | `MainViewModel` | Radio change: writes to ViewModel setter | Setter applies change immediately (theme/font/language) and persists via `Preferences`; `_initialising` guard prevents spurious writes during seeding |
| `DeepDivePopup` | `title`, 4 content strings, `topOffset` | Close tap: pop self | None - purely informational, no ViewModel interaction |
| `RefreshingPopup` | `Action onDismissed` | Auto: 3 s delay, pop self, invoke `onDismissed` | `onDismissed` recalculates one specific ticker on `MainViewModel` |

**Extend here when:**
- Adding a new popup: follow the constructor-injection pattern; use a guard bool in `MainPage.xaml.cs`; pop from within the popup's own code-behind; do not hold a reference to the popup after pushing
- Adding a new settings option to `SettingsPopup`: add `RadioButton` + `Label` rows in XAML; adjust `Grid.RowSpan`; seed in constructor; handle in `OnXxxChanged`; write to a new ViewModel setter
- Adding a new menu item to `MainMenuPopup`: add a `Grid` + `TapGestureRecognizer` row (not a `Button`); always await `PopModalAsync()` before invoking any follow-up navigation

---

## 6. Development Workflow

---

### 6.1 Prerequisites

| Requirement | Minimum version | Notes |
|-------------|----------------|-------|
| .NET SDK | 9.0.312 | `dotnet --version` to verify |
| Visual Studio | 2022 17.12+ | ".NET Multi-platform App UI development" workload required |
| Android SDK | API 35 | Build tools 35.0.0, OpenJDK 17 |
| Xcode | 16+ | macOS only; required for iOS and Mac Catalyst builds |
| Windows App SDK | 1.6 | Installed automatically via NuGet on Windows |

**Installed .NET workloads** (verified on this machine):

```
android         35.0.78 / 9.0.100
ios             26.0.9752 / 9.0.100
maccatalyst     26.0.9752 / 9.0.100
maui-windows    9.0.111 / 9.0.100
```

To install or update all MAUI workloads:

```
dotnet workload install maui
dotnet workload update
```

---

### 6.2 Restore

NuGet packages must be restored before the first build or after any `.csproj` change.

```
dotnet restore Aeonpulse.csproj
```

The project uses the following packages:
- `Microsoft.Maui.Controls 9.0.0`
- `Microsoft.Maui.Controls.Compatibility 9.0.0`
- `Microsoft.Extensions.Logging.Debug 9.0.0`
- `Microsoft.Graphics.Win2D 1.3.2` (Windows only - Win2D colour-matrix icon tinting)

---

### 6.3 Build Commands

#### Build all active target frameworks (Windows only - Android included automatically)

```
dotnet build Aeonpulse.csproj
```

On a Windows machine this builds `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`,
and `net9.0-windows10.0.19041.0`. iOS and Mac Catalyst compilation requires a Mac;
on Windows these targets compile the managed layer only and cannot produce runnable artifacts.

#### Build a single platform - Debug (fastest for iteration)

```
dotnet build Aeonpulse.csproj -f net9.0-android
dotnet build Aeonpulse.csproj -f net9.0-ios
dotnet build Aeonpulse.csproj -f net9.0-maccatalyst
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0
```

#### Build a single platform - Release

```
dotnet build Aeonpulse.csproj -f net9.0-android             -c Release
dotnet build Aeonpulse.csproj -f net9.0-ios                 -c Release
dotnet build Aeonpulse.csproj -f net9.0-maccatalyst         -c Release
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 -c Release
```

#### Force a full rebuild (clears incremental cache - use when XAML or resource changes are not picked up)

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

#### Build output locations

| Platform | Debug output | Release output |
|----------|-------------|----------------|
| Android | `bin\Debug\net9.0-android\` | `bin\Release\net9.0-android\` |
| iOS | `bin\Debug\net9.0-ios\` | `bin\Release\net9.0-ios\` |
| Mac Catalyst | `bin\Debug\net9.0-maccatalyst\` | `bin\Release\net9.0-maccatalyst\` |
| Windows | `bin\Debug\net9.0-windows10.0.19041.0\win10-x64\` | `bin\Release\net9.0-windows10.0.19041.0\win10-x64\` |

---

### 6.4 Known Warnings (Build-Clean - Expected, Do Not Fix)

The following warnings appear in every clean build and are known, accepted, and
not regressions. Do not treat them as failures.

| Code | Count (Windows Debug) | Cause | Status |
|------|-----------------------|-------|--------|
| `XC0022` | ~240 | `{Binding}` expressions in `MainPage.xaml` lack `x:DataType`; compiled binding not enabled | Accepted - runtime binding used intentionally |
| `CS0618` | ~60 | `Frame` (popup XAML), `Application.MainPage` setter (`App.xaml.cs`) deprecated in .NET 9 | Accepted - existing usage, do not add new occurrences |
| `CS8767` | ~48 | Nullability mismatch on `BoolToImageSourceConverter.ConvertBack` parameter | Accepted - minor nullability annotation difference |
| `CS0414` | 4 | `MainPage._isSettingsOpen` assigned but never read (guard flag only written, not checked) | Accepted - intentional guard pattern |

**Rule for agents:** if a build produces only the above warning codes, it is clean.
Any warning code not in this table is a new issue and must be investigated.

---

### 6.5 Testing

The test project `Aeonpulse.Tests` exists at `Aeonpulse.Tests\Aeonpulse.Tests.csproj`.
It targets `net9.0` (plain .NET, no MAUI) and links source files from the main project
directly so no TFM-incompatibility issues arise. Run all tests with:

```
dotnet test Aeonpulse.Tests\Aeonpulse.Tests.csproj
```

**What can be tested without a device (222 tests in 14 test classes):**
- `FindNearestJubilee()` / `ReduceToSingleDigit()` (pure algorithms, `internal` + `InternalsVisibleTo`)
- `CalculateTimeJubilees`, `CalculateCountdown`, `CalculateLifeOdometer`
- `CalculateAlienAnniversaries`, `CalculateHumanBirthRank`
- `CalculatePersonalYear`, `CalculateGlobalExhale`
- `CalculateCosmicStretch` (Hubble-Lemaitre expansion rate, injected `now`)
- `CalculateYourBreath` (breath/air/CO2 formulas, metric/imperial toggle, injected `now`)
- All `Calculate*` methods accept `DateTime? now = null` - pass a fixed value to make tests deterministic

**What requires a device or simulator:**
- Any MAUI UI (navigation, modal push/pop, `DynamicResource` binding)
- `ThemeService` / `FontSizeService` (require `Application.Current.Resources`)
- `ImageTint` (requires native handler pipeline)

---

### 6.6 Run and Deploy

#### Windows - Run directly (fastest)

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 -t:Run
```

Or press **F5** in Visual Studio with the `Windows Machine` target selected.

The Windows executable is at:
`bin\Debug\net9.0-windows10.0.19041.0\win10-x64\Aeonpulse.exe`

#### Android - Deploy to connected device or running emulator

List connected devices first:

```
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" devices -l
```

Deploy via dotnet CLI:

```
dotnet build Aeonpulse.csproj -f net9.0-android -t:Run
```

Or in Visual Studio: select the target device from the device picker dropdown
and press **F5**.

The signed debug APK is at:
`bin\Debug\net9.0-android\com.aeonpulse.app-Signed.apk`

Install manually to a connected device:

```
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" install -r "bin\Debug\net9.0-android\com.aeonpulse.app-Signed.apk"
```

#### iOS - Run on simulator (requires macOS + Xcode)

```
dotnet build Aeonpulse.csproj -f net9.0-ios -t:Run
```

List available simulators:

```
xcrun simctl list devices available
```

Target a specific simulator by UDID:

```
dotnet build Aeonpulse.csproj -f net9.0-ios -t:Run -p:_DeviceSpecificBuild=true -p:RuntimeIdentifier=iossimulator-arm64
```

Or in Visual Studio for Mac: select an iOS simulator from the device picker and press **F5**.

#### Mac Catalyst - Run on Mac (requires macOS)

```
dotnet build Aeonpulse.csproj -f net9.0-maccatalyst -t:Run
```

---

### 6.7 Publish (Release Packages)

#### Android - Signed AAB for Play Store

```
dotnet publish Aeonpulse.csproj -f net9.0-android -c Release
```

Output: `bin\Release\net9.0-android\com.aeonpulse.app-Signed.aab`

For Play Store submission, the AAB must be signed with a production keystore.
Replace the default debug keystore by passing:

```
dotnet publish Aeonpulse.csproj -f net9.0-android -c Release \
  -p:AndroidKeyStore=true \
  -p:AndroidSigningKeyStore=your.keystore \
  -p:AndroidSigningKeyAlias=your_alias \
  -p:AndroidSigningKeyPass=your_pass \
  -p:AndroidSigningStorePass=your_store_pass
```

#### Android - Signed APK for sideloading

```
dotnet publish Aeonpulse.csproj -f net9.0-android -c Release -p:AndroidPackageFormats=apk
```

Output: `bin\Release\net9.0-android\publish\com.aeonpulse.app-Signed.apk`

#### iOS - IPA for App Store (requires macOS + Apple Developer account)

```
dotnet publish Aeonpulse.csproj -f net9.0-ios -c Release \
  -p:ArchiveOnBuild=true \
  -p:RuntimeIdentifier=ios-arm64
```

The `.ipa` is produced in `bin\Release\net9.0-ios\ios-arm64\publish\`.

#### Mac Catalyst - PKG for Mac App Store

```
dotnet publish Aeonpulse.csproj -f net9.0-maccatalyst -c Release \
  -p:MtouchArch=x86_64 \
  -p:CreatePackage=true
```

#### Windows - MSIX for Store or sideload

```
dotnet publish Aeonpulse.csproj -f net9.0-windows10.0.19041.0 -c Release \
  -p:RuntimeIdentifier=win10-x64 \
  -p:WindowsPackageType=MSIX
```

---

### 6.8 Clean Build (XAML Encoding Issues)

If the build fails with `MSB4018`, `XamlCTask`, or `System.Xml.XmlException`,
the most common cause is a XAML file saved without a UTF-8 BOM.

**All `.xaml` files must be saved as UTF-8 with BOM (codepage 65001 with signature).**

#### Find XAML files missing the BOM

```powershell
Get-ChildItem -Path "C:\Dev\Aeonpulse" -Filter "*.xaml" -Recurse |
  Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' } |
  ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    if ($bytes.Length -lt 3 -or $bytes[0] -ne 0xEF -or $bytes[1] -ne 0xBB -or $bytes[2] -ne 0xBF) {
      $_.FullName
    }
  }
```

#### Re-save all XAML files with BOM

```powershell
Get-ChildItem -Path "C:\Dev\Aeonpulse" -Filter "*.xaml" -Recurse |
  Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' } |
  ForEach-Object {
    $content = [System.IO.File]::ReadAllText($_.FullName, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText($_.FullName, $content, [System.Text.Encoding]::UTF8)
  }
```

After re-saving, run a clean build:

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

#### Full clean (delete all build artefacts)

```
dotnet clean Aeonpulse.csproj
```

Or manually delete `bin\` and `obj\` directories:

```powershell
Remove-Item -Recurse -Force "C:\Dev\Aeonpulse\bin", "C:\Dev\Aeonpulse\obj"
```

---

### 6.9 AppResources Designer Regeneration

`Resources\AppResources.Designer.cs` is auto-generated from `AppResources.resx` by
the `PublicResXFileCodeGenerator` on every build. If the designer is out of sync
(e.g., a key is missing after editing the `.resx` manually), force regeneration by:

1. In Visual Studio: right-click `AppResources.resx` → **Run Custom Tool**
2. Or trigger a full rebuild:
   ```
   dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
   ```

Do not edit `AppResources.Designer.cs` manually - all changes will be overwritten.

---

### 6.10 Source Control

| Item | Value |
|------|-------|
| Repository | `https://github.com/anatoly-ka/Aeonpulse` |
| Default branch | `main` |
| Remote | `origin` |

Standard Git workflow:

```
git status
git add -A
git commit -m "Description of change"
git push origin main
```

`.gitignore` excludes `*.user`, `.vs/` (except Copilot chat history), `*.obj`,
`*.suo`, and similar IDE artefacts. The `bin\` and `obj\` build output directories
are not explicitly listed in `.gitignore` but are excluded by the catch-all patterns.
Verify before committing:

```
git status --short
```

---

### 6.11 Adding a New Language (Build Steps Only)

1. Create `Resources\AppResources.{culture}.resx` (copy from `AppResources.ru.resx` as template).

2. Add to `Aeonpulse.csproj` (follow the existing Russian pattern exactly):

```xml
<EmbeddedResource Update="Resources\AppResources.{culture}.resx">
  <Culture>{culture}</Culture>
  <DependentUpon>AppResources.resx</DependentUpon>
</EmbeddedResource>
```

3. Add the language constant and `ApplyLanguage` switch case to `MainViewModel.cs`.

4. Build to verify the `.resx` is embedded correctly:

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

5. At runtime, set temporarily `AppResources.Culture = new CultureInfo("{culture}")`, build, run,
   and verify strings load. Upon successful verification, remove/comment out this assignment.

---

### 6.12 Enabling the Tizen Target

Tizen is disabled in `Aeonpulse.csproj` by default (commented out). To enable:

1. Install the Tizen workload:
   ```
   dotnet workload install tizen
   ```

2. In `Aeonpulse.csproj`, uncomment the Tizen target framework line:
   ```xml
   <!-- <TargetFrameworks>$(TargetFrameworks);net8.0-tizen</TargetFrameworks> -->
   ```

3. Update `Platforms\Tizen\tizen-manifest.xml`: replace placeholder `appid` and `title`
   with real values matching `ApplicationId` and `ApplicationTitle` in the `.csproj`.

4. Build:
   ```
   dotnet build Aeonpulse.csproj -f net9.0-tizen
   ```

---

## 7. How to Extend

Each recipe below lists every file that must be changed, in the order they should
be changed, with exact code to add derived from the existing patterns in the
codebase. Follow every step. Skipping any step will cause either a build error
or a silent runtime regression (missing string, broken binding, or uncalculated ticker).

After completing any recipe, update `Agents.md` Section 2 (file descriptions) and
Section 5 (Knowledge Graph nodes) to reflect the new symbols.

---

### 7.1 Adding a New Collapsible Section

A "section" is a collapsible `CardFrame`-styled `Border` in the `ScrollView` of
`MainPage.xaml`, containing one or more ticker cards. The four existing sections
are `Lab`, `Cosmos`, `Mirror`, and `Eco Echoes`.

**Example:** adding a section called `Quantum`.

---

#### Step 1 - `Resources/AppResources.resx`

Add the section title string. Follow the `Section_` prefix pattern:

```xml
<data name="Section_QuantumTitle" xml:space="preserve">
  <value>Quantum</value>
</data>
```

---

#### Step 2 - `Resources/AppResources.ru.resx`

Add the Russian translation with the same key:

```xml
<data name="Section_QuantumTitle" xml:space="preserve">
  <value>Квантовый</value>
</data>
```

---

#### Step 3 - `ViewModels/LocalizedResources.cs`

Add one passthrough property in the `-- Sections --` group:

```csharp
// -- Sections -------------------------------------------------------
public string Section_LabTitle           => AppResources.Section_LabTitle;
public string Section_CosmosTitle        => AppResources.Section_CosmosTitle;
public string Section_MirrorTitle        => AppResources.Section_MirrorTitle;
public string Section_EcoEchoesTitle     => AppResources.Section_EcoEchoesTitle;
public string Section_QuantumTitle       => AppResources.Section_QuantumTitle;  // ADD
```

---

#### Step 4 - `ViewModels/MainViewModel.cs`

Add the expansion state property and toggle command. Follow the exact pattern of
`LabExpanded` / `ToggleLabCommand`:

```csharp
// --- in the Subsection Expanded States region ---
private bool _quantumExpanded = false;   // false = collapsed on first run
public bool QuantumExpanded
{
    get => _quantumExpanded;
    set { _quantumExpanded = value; OnPropertyChanged(); }
}
```

```csharp
// --- in the Commands region ---
public ICommand ToggleQuantumCommand { get; }
```

```csharp
// --- in the constructor, after the other section toggle commands ---
ToggleQuantumCommand = new Command(() => QuantumExpanded = !QuantumExpanded);
```

---

#### Step 5 - `Views/MainPage.xaml`

Add the section block inside the `<VerticalStackLayout Spacing="16">` in Row 2,
after the last existing section (`EcoExpanded`). Copy the exact structure of an
existing section block:

```xml
<!-- == QUANTUM SECTION ========================================
     Tickers: (add ticker names here as you add them)
     Binding: MainViewModel.QuantumExpanded -> ToggleQuantumCommand
================================================================ -->
<Border Style="{StaticResource CardFrame}">
    <VerticalStackLayout Spacing="12">

        <!-- AI: Section header row - [title label | expand/collapse icon button].
             Source binding uses BoolToImageSource with "chevron_up|chevron_down" parameter. -->
        <Grid ColumnDefinitions="*,Auto">
            <!-- Binding: MainViewModel.Loc.Section_QuantumTitle (live-localised) -->
            <Label Grid.Column="0"
                   Text="{Binding Loc.Section_QuantumTitle}"
                   Style="{StaticResource SubtitleLabel}"
                   TextColor="{DynamicResource CyberCyan}"
                   VerticalOptions="Center" />
            <!-- Binding: MainViewModel.QuantumExpanded -> BoolToImageSource -> chevron icon.
                 Command: MainViewModel.ToggleQuantumCommand toggles QuantumExpanded. -->
            <ImageButton Grid.Column="1"
                         Style="{StaticResource ThemedIconButton}"
                         helpers:ImageTint.Color="{DynamicResource CyberCyan}"
                         Source="{Binding QuantumExpanded, Converter={StaticResource BoolToImageSource}, ConverterParameter='chevron_up.png|chevron_down.png'}"
                         Command="{Binding ToggleQuantumCommand}" />
        </Grid>

        <!-- AI: Section body - visible only when QuantumExpanded=true.
             Add ticker card Border elements here (see Section 7.2). -->
        <VerticalStackLayout Spacing="12"
                             IsVisible="{Binding QuantumExpanded}">

            <!-- ticker cards go here -->

        </VerticalStackLayout>

    </VerticalStackLayout>
</Border>
```

---

#### Verification

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

Expected: build succeeds, no new warning codes beyond the known set in Section 6.4.
At runtime: the new section header appears, taps correctly toggle expansion.

---

### 7.2 Adding a New Ticker Card

A ticker card is a `Border` element inside a section body. It shows `BriefText`
always and `FullText` when expanded. It is backed by a typed result property (a subclass of `TickerData`, defined in `TickerResults.cs`) on
`MainViewModel` and a calculation method on `CalculationService`.

**Example:** adding a `LunarCycle` static ticker inside the `Quantum` section.
Replace `LunarCycle` / `lunarCycle` with your ticker name throughout.

---

#### Step 0 - `Models/TickerResults.cs`

Define the typed result class for the new ticker by adding a subclass of `TickerData`:

```csharp
[AIContext(`"DataTransferObject"")]
public class LunarCycleResult : TickerData
{
    public string Phase { get; init; } = string.Empty;
    public int DayInCycle { get; init; }
    public long CompleteCycles { get; init; }
}
```

Add only the raw computed fields that other code (tease text, cross-ticker logic) might need directly.
Do **not** duplicate `BriefText`/`FullText` - those are inherited from `TickerData`.'r

---

#### Step 1 - `Resources/AppResources.resx`

Add all six required string keys for the new ticker:

```xml
<!-- Ticker card display strings -->
<data name="Ticker_LunarCycleTitle" xml:space="preserve">
  <value>Lunar Cycle</value>
</data>
<data name="Ticker_LunarCycleBrief" xml:space="preserve">
  <value>Current phase: {phase} - Day {dayInCycle} of {cycleLength}</value>
</data>
<data name="Ticker_LunarCycleFull" xml:space="preserve">
  <value>Since {baseDateName} on {baseDate:d}, you have witnessed {completeCycles} complete lunar cycles. The current phase is {phase}, day {dayInCycle} of a {cycleLength}-day cycle.</value>
</data>

<!-- Deep Dive popup strings -->
<data name="Info_LunarCycleTitle" xml:space="preserve">
  <value>Lunar Cycle</value>
</data>
<data name="Info_LunarCycleMethod" xml:space="preserve">
  <value>The synodic lunar cycle (new moon to new moon) averages 29.53059 days. Phase is calculated as elapsed days modulo 29.53059 mapped to eight named phases.</value>
</data>
<data name="Info_LunarCycleSource" xml:space="preserve">
  <value>Jean Meeus, Astronomical Algorithms, 2nd ed. (1998). NASA lunar phase data.</value>
</data>
```

Use `{placeholder}` tokens (not `{0}`, `{1}`) - they are replaced by `string.Replace()`
calls in `CalculationService`, not `string.Format()`.

---

#### Step 2 - `Resources/AppResources.ru.resx`

Add Russian translations for all six keys:

```xml
<data name="Ticker_LunarCycleTitle" xml:space="preserve">
  <value>Лунный цикл</value>
</data>
<data name="Ticker_LunarCycleBrief" xml:space="preserve">
  <value>Текущая фаза: {phase} - День {dayInCycle} из {cycleLength}</value>
</data>
<data name="Ticker_LunarCycleFull" xml:space="preserve">
  <value>С {baseDateName} {baseDate:d} вы наблюдали {completeCycles} полных лунных цикла. Текущая фаза: {phase}, день {dayInCycle} из {cycleLength}.</value>
</data>
<data name="Info_LunarCycleTitle" xml:space="preserve">
  <value>Лунный цикл</value>
</data>
<data name="Info_LunarCycleMethod" xml:space="preserve">
  <value>Синодический лунный цикл (от новолуния до новолуния) составляет в среднем 29,53059 дней.</value>
</data>
<data name="Info_LunarCycleSource" xml:space="preserve">
  <value>Жан Мéус, "Астрономические алгоритмы", 2-е изд. (1998).</value>
</data>
```

---

#### Step 3 - `ViewModels/LocalizedResources.cs`

Add passthrough properties in the `-- Ticker titles --` and `-- Deep Dive --` groups:

```csharp
// -- Ticker titles ---------------------------------------------------
// ... existing entries ...
public string Ticker_LunarCycleTitle => AppResources.Ticker_LunarCycleTitle;

// -- Deep Dive / Info popup -------------------------------------------
// ... existing entries ...
public string Info_LunarCycleTitle   => AppResources.Info_LunarCycleTitle;
public string Info_LunarCycleMethod  => AppResources.Info_LunarCycleMethod;
public string Info_LunarCycleSource  => AppResources.Info_LunarCycleSource;
```

---

#### Step 4 - `Services/CalculationService.cs`

Add the calculation method. For a **static ticker** (recalculated on demand),
decorate with `[AIContext("CoreCalculation")]`. For a **live ticker** (called
every second), use `[AIContext("LiveTicker")]` and keep it fast (no I/O, no LINQ
over large collections):

```csharp
/// <summary>
/// Calculates the current lunar cycle phase relative to the user's base date.
/// Returns the current phase name, day within the current cycle, and number
/// of complete cycles since the base date.
///
/// <para>
/// <b>Algorithm:</b> synodic period = 29.53059 days. Phase is binned into
/// eight named phases based on fractional position in the current cycle.
/// </para>
/// <para>
/// <b>Side effect:</b> reads <see cref="AppResources"/> for phase name strings
/// so output language follows <c>AppResources.Culture</c>.
/// </para>
/// </summary>
/// <param name="baseDate">The user-selected origin date.</param>
/// <param name="baseDateName">Human-readable label for display in output strings.</param>
/// <param name="baseDateValue">ISO-8601 string of the base date for output formatting.</param>
/// <returns>
/// A <see cref="TickerData"/> with brief and full lunar cycle descriptions.
/// </returns>
[AIContext("CoreCalculation")]
public LunarCycleResult CalculateLunarCycle(DateTime baseDate, string baseDateName, string baseDateValue)
{
    const double synodicPeriod = 29.53059;
    DateTime now = DateTime.Now;
    double totalDays = (now - baseDate).TotalDays;
    long completeCycles = (long)(totalDays / synodicPeriod);
    double dayInCycleRaw = totalDays % synodicPeriod;
    int dayInCycle = (int)dayInCycleRaw + 1;
    int cycleLength = (int)synodicPeriod;

    // Map fractional position to phase name
    double fraction = dayInCycleRaw / synodicPeriod;
    string phase = fraction switch
    {
        < 0.025 => "New Moon",
        < 0.25  => "Waxing Crescent",
        < 0.275 => "First Quarter",
        < 0.475 => "Waxing Gibbous",
        < 0.525 => "Full Moon",
        < 0.725 => "Waning Gibbous",
        < 0.755 => "Last Quarter",
        < 0.975 => "Waning Crescent",
        _       => "New Moon",
    };

    string briefText = AppResources.Ticker_LunarCycleBrief
        .Replace("{phase}", phase)
        .Replace("{dayInCycle}", dayInCycle.ToString())
        .Replace("{cycleLength}", cycleLength.ToString());

    string fullText = AppResources.Ticker_LunarCycleFull
        .Replace("{baseDateName}", baseDateName)
        .Replace("{baseDate:d}", baseDate.ToString("d", System.Globalization.CultureInfo.CurrentUICulture))
        .Replace("{completeCycles}", completeCycles.ToString())
        .Replace("{phase}", phase)
        .Replace("{dayInCycle}", dayInCycle.ToString())
        .Replace("{cycleLength}", cycleLength.ToString());

    return new LunarCycleResult { BriefText = briefText, FullText = fullText };
}
```

Token replacement uses `string.Replace()` on `{tokenName}` placeholders - not
`string.Format()`. Match the exact placeholder names used in the `.resx` templates.

---

#### Step 5 - `ViewModels/MainViewModel.cs`

Add four items: the typed result property (`LunarCycleResult` - a new subclass of `TickerData` you define in `TickerResults.cs`), the `bool` expanded property, the
toggle command declaration, and the refresh command declaration (for static tickers
only). Wire into `UpdateStaticCalculations()` or `UpdateLiveCalculations()`.

```csharp
// --- Typed result property (in the "Ticker Data" region) ---
private LunarCycleResult _lunarCycle = new LunarCycleResult();
public LunarCycleResult LunarCycle
{
    get => _lunarCycle;
    set { _lunarCycle = value; OnPropertyChanged(); }
}

// --- Card expanded bool (in the "Ticker Card Expanded States" region) ---
private bool _lunarCycleExpanded = false;
public bool LunarCycleExpanded
{
    get => _lunarCycleExpanded;
    set { _lunarCycleExpanded = value; OnPropertyChanged(); }
}

// --- Command declarations (in the Commands region) ---
public ICommand ToggleLunarCycleCommand { get; }
public ICommand RefreshLunarCycleCommand { get; }   // omit for live tickers
```

```csharp
// --- In the constructor, after the other card toggle commands ---
ToggleLunarCycleCommand = new Command(() => LunarCycleExpanded = !LunarCycleExpanded);

// For a static ticker with a refresh button:
RefreshLunarCycleCommand = new Command(async () =>
{
    if (RefreshRequested != null)
        await RefreshRequested.Invoke(() =>
            LunarCycle = _calculationService.CalculateLunarCycle(BaseDate, BaseDateName, BaseDateValue));
    else
        LunarCycle = _calculationService.CalculateLunarCycle(BaseDate, BaseDateName, BaseDateValue);
});
```

```csharp
// --- In UpdateStaticCalculations() --- (use UpdateLiveCalculations for live tickers)
public void UpdateStaticCalculations()
{
    // ... existing calls ...
    LunarCycle = _calculationService.CalculateLunarCycle(BaseDate, BaseDateName, BaseDateValue);
}
```

---

#### Step 6 - `Views/MainPage.xaml`

Add the ticker card `Border` inside the target section's `<VerticalStackLayout IsVisible="{Binding QuantumExpanded}">`. Copy the static ticker template (with refresh button) or live ticker template (without refresh button) from an existing card.

**Static ticker with refresh button** (mirrors TimeJubilees):

```xml
<!-- == TICKER: LUNAR CYCLE ============================
     Static ticker - recalculated on base date change
     or via explicit RefreshLunarCycleCommand.
     Binding: MainViewModel.LunarCycle (TickerData)
     Commands: RefreshLunarCycleCommand, ToggleLunarCycleCommand
======================================================== -->
<Border BackgroundColor="{DynamicResource CardDark}"
        StrokeThickness="1"
        Stroke="{DynamicResource CyberCyan}"
        StrokeShape="RoundRectangle 8"
        Padding="12">
    <VerticalStackLayout Spacing="8">

        <!-- AI: Ticker card header - 3-column:
             [Col0: emoji icon | Col1: title | Col2: action buttons (info, refresh, expand)] -->
        <Grid ColumnDefinitions="Auto,*,Auto">
            <Label Grid.Column="0"
                   Text="🌙"
                   FontSize="{DynamicResource FontSizeTitle}"
                   VerticalOptions="Center"
                   Margin="0,0,8,0" />
            <!-- Binding: MainViewModel.Loc.Ticker_LunarCycleTitle (live-localised) -->
            <Label Grid.Column="1"
                   Text="{Binding Loc.Ticker_LunarCycleTitle}"
                   Style="{StaticResource BaseLabel}"
                   TextColor="{DynamicResource CyberPurple}"
                   FontAttributes="Bold"
                   VerticalOptions="Center" />
            <HorizontalStackLayout Grid.Column="2" Spacing="4">
                <!-- Info: opens DeepDivePopup with methodology + sources -->
                <ImageButton Style="{StaticResource ThemedIconButton}"
                             helpers:ImageTint.Color="{DynamicResource CyberCyan}"
                             Source="info.png"
                             Clicked="OnLunarCycleInfoClicked" />
                <!-- Refresh: shows RefreshingPopup then recalculates LunarCycle.
                     Omit this ImageButton for live tickers. -->
                <ImageButton Style="{StaticResource ThemedIconButton}"
                             helpers:ImageTint.Color="{DynamicResource CyberCyan}"
                             Source="refresh.png"
                             Command="{Binding RefreshLunarCycleCommand}" />
                <!-- Expand/collapse: toggles LunarCycleExpanded -->
                <ImageButton Style="{StaticResource ThemedIconButton}"
                             helpers:ImageTint.Color="{DynamicResource CyberCyan}"
                             Source="{Binding LunarCycleExpanded, Converter={StaticResource BoolToImageSource}, ConverterParameter='square_chevron_up.png|square_chevron_down.png'}"
                             Command="{Binding ToggleLunarCycleCommand}" />
            </HorizontalStackLayout>
        </Grid>

        <!-- Binding: MainViewModel.LunarCycle.BriefText - always visible -->
        <Label Text="{Binding LunarCycle.BriefText}"
               TextColor="{DynamicResource TextGray}"
               FontSize="{DynamicResource FontSizeMedium}" />
        <!-- Binding: MainViewModel.LunarCycle.FullText - visible when expanded -->
        <Label Text="{Binding LunarCycle.FullText}"
               TextColor="{DynamicResource TextGray}"
               FontSize="{DynamicResource FontSizeMedium}"
               LineBreakMode="WordWrap"
               IsVisible="{Binding LunarCycleExpanded}" />

    </VerticalStackLayout>
</Border>
```

---

#### Step 7 - `Views/MainPage.xaml.cs`

Add the guard bool and the `OnXxxInfoClicked` handler. Follow the exact pattern
of the existing 10 handlers:

```csharp
// --- In the guard flags block at the top of the class ---
private bool _isLunarCycleDeepDiveOpen;
```

```csharp
/// <summary>Opens the Lunar Cycle deep-dive info panel.</summary>
private async void OnLunarCycleInfoClicked(object sender, EventArgs e) =>
    await OpenDeepDiveAsync(
        () => _isLunarCycleDeepDiveOpen, v => _isLunarCycleDeepDiveOpen = v,
        AppResources.Info_LunarCycleTitle,
        AppResources.Info_MethodTitle, AppResources.Info_LunarCycleMethod,
        AppResources.Info_SourceTitle, AppResources.Info_LunarCycleSource);
```

---

#### Step 8 - `Aeonpulse.Tests/CalculationServiceTests.cs`

Add tests for the new `Calculate*` method in `Aeonpulse.Tests\`. A minimum of
three cases is required:

1. **Happy path** - inject a known `now`, assert `BriefText` contains the expected computed value.
2. **Zero elapsed time** - `baseDate == now`; assert the result is non-null and non-empty.
3. **Large elapsed time** - a `baseDate` 150+ years in the past; assert no exception is thrown.

```
dotnet test Aeonpulse.Tests\Aeonpulse.Tests.csproj
```

All tests must pass before committing.

---

#### Verification

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

At runtime: ticker card appears in the section, BriefText is visible immediately,
FullText appears on expand, info button opens `DeepDivePopup`, refresh button
shows `RefreshingPopup` and recalculates.

---

### 7.3 Adding a New Colour Scheme

A colour scheme is a palette of 12 named `Color` values stored as a
`Dictionary<string, Color>` in `ThemeService`. Adding one requires changes to
exactly three files.

**Example:** adding a `CyberGold` scheme.

---

#### Step 1 - `Services/ThemeService.cs`

Add the scheme identifier constant and the palette dictionary. Every palette
**must** contain all 12 colour keys - missing a key will leave the previous
theme's value for that key when switching:

```csharp
// --- Scheme identifier ---
public const string CyberGold = "CyberGold";

// --- CyberGold palette ---
// Design rule: warm dark background with gold/amber accents.
private static readonly Dictionary<string, Color> _cyberGoldColors = new()
{
    { "SpaceDark",       Color.FromArgb("#1A1200") },
    { "SpaceDarker",     Color.FromArgb("#0D0900") },
    { "CyberCyan",       Color.FromArgb("#FFB800") },   // gold replaces cyan
    { "CyberPurple",     Color.FromArgb("#FF8C00") },   // amber replaces purple
    { "CyberPink",       Color.FromArgb("#FFA040") },
    { "NeonGreen",       Color.FromArgb("#FFD700") },
    { "NeonGreenDark",   Color.FromArgb("#2A1A00") },
    { "TextWhite",       Color.FromArgb("#FFFFFF") },
    { "TextDim",         Color.FromArgb("#F5E0A0") },
    { "TextGray",        Color.FromArgb("#C8A060") },
    { "CardBackground",  Color.FromArgb("#1E1500") },
    { "CardDark",        Color.FromArgb("#120E00") },
};
```

Add the new case to `ApplyScheme`:

```csharp
public void ApplyScheme(string scheme)
{
    _currentScheme = scheme;

    var palette = scheme switch
    {
        HighContrastDark  => _highContrastDarkColors,
        HighContrastLight => _highContrastLightColors,
        CyberGold         => _cyberGoldColors,          // ADD
        _                 => _defaultColors,
    };

    var resources = Application.Current?.Resources;
    if (resources is null)
        return;

    foreach (var (key, color) in palette)
        resources[key] = color;
}
```

---

#### Step 2 - `Resources/AppResources.resx`

Add the display name string for the settings UI:

```xml
<data name="Settings_PaletteCyberGold" xml:space="preserve">
  <value>Cyber Gold</value>
</data>
```

---

#### Step 3 - `Resources/AppResources.ru.resx`

Add the Russian translation:

```xml
<data name="Settings_PaletteCyberGold" xml:space="preserve">
  <value>Кибер Золото</value>
</data>
```

---

#### Step 4 - `ViewModels/LocalizedResources.cs`

Add the passthrough property in the `-- Settings --` group:

```csharp
public string Settings_PaletteCyberGold => AppResources.Settings_PaletteCyberGold;
```

---

#### Step 5 - `ViewModels/MainViewModel.cs`

The `ColorScheme` setter already calls `ThemeService.Instance.ApplyScheme()` and
persists the value - no changes needed there. The new `const string` is now
accessible as `ThemeService.CyberGold`.

---

#### Step 6 - `Views/SettingsPopup.xaml`

The Color Scheme group currently occupies rows 3-5 (three options). Adding a
fourth option requires adding one row and updating `Grid.RowSpan` on the group label.

In `SettingsControlGrid`, change `RowDefinitions` to add one row to the Color Scheme
group. The grid currently has 14 rows (`0` through `13`). Insert a new row 6 and
shift subsequent row indices:

```xml
<!-- Change RowDefinitions from 14 rows to 15 rows -->
RowDefinitions="Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto"
```

Update the `PaletteLabel` `Grid.RowSpan` from 3 to 4:

```xml
<Label Grid.Row="3" Grid.Column="0"
       Grid.RowSpan="4"
       x:Name="PaletteLabel" ... />
```

Add the new radio row at `Grid.Row="6"` (between the existing row 5 and the
spacer that was at row 6, now shifted to row 7):

```xml
<!-- Row 6, Col 1: Cyber Gold radio. -->
<HorizontalStackLayout Grid.Row="6" Grid.Column="1"
                       x:Name="CyberGoldRadioRow"
                       Spacing="6"
                       HorizontalOptions="Start"
                       VerticalOptions="Center">
    <RadioButton x:Name="CyberGoldRadio"
                 Value="CyberGold"
                 GroupName="ColorScheme"
                 CheckedChanged="OnColorSchemeChanged">
        <RadioButton.ControlTemplate>
            <ControlTemplate>
                <Grid ColumnDefinitions="Auto,Auto" ColumnSpacing="6">
                    <Ellipse Grid.Column="0"
                             WidthRequest="16" HeightRequest="16"
                             Stroke="{DynamicResource CyberCyan}"
                             StrokeThickness="2"
                             Fill="Transparent" />
                    <Ellipse x:Name="InnerDot"
                             Grid.Column="0"
                             WidthRequest="8" HeightRequest="8"
                             HorizontalOptions="Center"
                             VerticalOptions="Center"
                             Fill="{DynamicResource CyberCyan}"
                             IsVisible="{TemplateBinding IsChecked}" />
                    <ContentPresenter Grid.Column="1" VerticalOptions="Center" />
                </Grid>
            </ControlTemplate>
        </RadioButton.ControlTemplate>
    </RadioButton>
    <!-- Binding: MainViewModel.Loc.Settings_PaletteCyberGold (live-localised) -->
    <Label Text="{Binding Loc.Settings_PaletteCyberGold}"
           FontSize="{DynamicResource FontSizeMedium}"
           TextColor="{DynamicResource CyberCyan}"
           VerticalOptions="Center" />
</HorizontalStackLayout>
```

Shift all subsequent `Grid.Row` values in the XAML by 1 (the spacer and all
Text Size and Language group rows that were at rows 6-13 become rows 7-14).

---

#### Step 7 - `Views/SettingsPopup.xaml.cs`

Add seeding in the constructor (inside the `_initialising = true` block):

```csharp
CyberGoldRadio.IsChecked = _viewModel.ColorScheme == ThemeService.CyberGold;
```

No change needed in `OnColorSchemeChanged` - it already reads `radio.Value` and
writes to `_viewModel.ColorScheme`, which calls `ThemeService.Instance.ApplyScheme()`.

---

#### Verification

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

At runtime: new radio button appears in Settings. Selecting it immediately repaints
the UI. The choice persists across restarts.

---

### 7.4 Adding a New Language

Adding a language requires no C# calculation changes - only string resources,
culture wiring, and one new radio button in `SettingsPopup`.

**Example:** adding German (`de`).

---

#### Step 1 - Create `Resources/AppResources.de.resx`

Copy `AppResources.ru.resx` as a starting template. Translate all 359 values.
The file must be named exactly `AppResources.{culture}.resx` where `{culture}`
is a valid BCP-47 tag (e.g., `de`, `de-DE`, `fr`, `zh-Hans`).

The file must use the same XML schema as the existing `.resx` files. Keep the
`<resheader>` block from the template unchanged.

---

#### Step 2 - `Aeonpulse.csproj`

Register the new resource file in the `<ItemGroup>` that contains the Russian
entry. Follow the identical pattern:

```xml
<EmbeddedResource Update="Resources\AppResources.de.resx">
  <Culture>de</Culture>
  <DependentUpon>AppResources.resx</DependentUpon>
</EmbeddedResource>
```

---

#### Step 3 - `ViewModels/MainViewModel.cs`

Add the language constant and the `ApplyLanguage` switch case:

```csharp
// --- In the Language constants region ---
public const string LangDefault = "Default";
public const string LangEnglish = "English";
public const string LangRussian = "Russian";
public const string LangGerman  = "German";    // ADD
```

```csharp
public static void ApplyLanguage(string language)
{
    System.Globalization.CultureInfo culture = language switch
    {
        LangEnglish => new System.Globalization.CultureInfo("en"),
        LangRussian => new System.Globalization.CultureInfo("ru"),
        LangGerman  => new System.Globalization.CultureInfo("de"),   // ADD
        _           => System.Globalization.CultureInfo.InstalledUICulture
    };

    System.Globalization.CultureInfo.DefaultThreadCurrentCulture   = culture;
    System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
    AppResources.Culture = culture;
}
```

---

#### Step 4 - `Resources/AppResources.resx`

Add the display name string for the new language option in Settings:

```xml
<data name="Settings_LanguageGerman" xml:space="preserve">
  <value>German</value>
</data>
```

---

#### Step 5 - `Resources/AppResources.ru.resx`

Add the Russian translation of the language name:

```xml
<data name="Settings_LanguageGerman" xml:space="preserve">
  <value>Немецкий</value>
</data>
```

---

#### Step 6 - `Resources/AppResources.de.resx`

Add the German self-translation:

```xml
<data name="Settings_LanguageGerman" xml:space="preserve">
  <value>Deutsch</value>
</data>
```

---

#### Step 7 - `ViewModels/LocalizedResources.cs`

Add the passthrough property in the `-- Settings --` group:

```csharp
public string Settings_LanguageGerman => AppResources.Settings_LanguageGerman;
```

---

#### Step 8 - `Views/SettingsPopup.xaml`

The Language group currently occupies rows 11-13 (three options in the 14-row
grid). Adding a fourth language option requires inserting one row and updating
the `LanguageLabel` span.

Update `RowDefinitions` to add one row (from 14 to 15 rows total, or 15 to 16
if a colour scheme was also added):

```xml
RowDefinitions="Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto"
```

Update `LanguageLabel` `Grid.RowSpan` from 3 to 4:

```xml
<Label Grid.Row="11" Grid.Column="0"
       Grid.RowSpan="4"
       x:Name="LanguageLabel" ... />
```

Add the new radio at the next row index (row 14 if no other rows were added,
row 15 if a colour scheme row was also inserted):

```xml
<!-- Row 14, Col 1: German radio.
     Value="German" -> CultureInfo("de") via MainViewModel.LangGerman. -->
<HorizontalStackLayout Grid.Row="14" Grid.Column="1"
                       x:Name="LangGermanRadioRow"
                       Spacing="6"
                       HorizontalOptions="Start"
                       VerticalOptions="Center">
    <RadioButton x:Name="LangGermanRadio"
                 Value="German"
                 GroupName="DisplayLanguage"
                 CheckedChanged="OnDisplayLanguageChanged">
        <RadioButton.ControlTemplate>
            <ControlTemplate>
                <Grid ColumnDefinitions="Auto,Auto" ColumnSpacing="6">
                    <Ellipse Grid.Column="0"
                             WidthRequest="16" HeightRequest="16"
                             Stroke="{DynamicResource CyberCyan}"
                             StrokeThickness="2"
                             Fill="Transparent" />
                    <Ellipse x:Name="InnerDot"
                             Grid.Column="0"
                             WidthRequest="8" HeightRequest="8"
                             HorizontalOptions="Center"
                             VerticalOptions="Center"
                             Fill="{DynamicResource CyberCyan}"
                             IsVisible="{TemplateBinding IsChecked}" />
                    <ContentPresenter Grid.Column="1" VerticalOptions="Center" />
                </Grid>
            </ControlTemplate>
        </RadioButton.ControlTemplate>
    </RadioButton>
    <!-- Binding: MainViewModel.Loc.Settings_LanguageGerman (live-localised) -->
    <Label Text="{Binding Loc.Settings_LanguageGerman}"
           FontSize="{DynamicResource FontSizeMedium}"
           TextColor="{DynamicResource CyberCyan}"
           VerticalOptions="Center" />
</HorizontalStackLayout>
```

---

#### Step 9 - `Views/SettingsPopup.xaml.cs`

Add seeding in the constructor:

```csharp
LangGermanRadio.IsChecked = _viewModel.DisplayLanguage == MainViewModel.LangGerman;
```

No change needed in `OnDisplayLanguageChanged` - it already reads `radio.Value`
and writes to `_viewModel.DisplayLanguage`, which calls `ApplyLanguage()`,
`Loc.Invalidate()`, and `UpdateAllCalculations()`.

---

#### Verification

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

Verify the resource is embedded:

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 -v:normal 2>&1 | Select-String "AppResources.de"
```

At runtime: new radio button appears in Settings. Selecting it switches all
`{Binding Loc.Xxx}` labels and regenerates all ticker strings using the German
`.resx`. The choice persists across restarts.

---

### 7.5 Adding a New Font Size Preset

`FontSizeService` follows the same pattern as `ThemeService`. Adding a preset
requires changes to two files.

**Example:** adding an `XLarge` preset for accessibility.

---

#### Step 1 - `Services/FontSizeService.cs`

Add the identifier constant, the preset dictionary (all 5 keys required), and
the switch case:

```csharp
public const string Small   = "Small";
public const string Normal  = "Normal";
public const string Large   = "Large";
public const string XLarge  = "XLarge";   // ADD

private static readonly Dictionary<string, double> _xLarge = new()
{
    { "FontSizeSmall",  16 },
    { "FontSizeMedium", 19 },
    { "FontSizeLarge",  22 },
    { "FontSizeXLarge", 26 },
    { "FontSizeTitle",  28 },
};

// In ApplyPreset:
var sizes = preset switch
{
    Small  => _small,
    Large  => _large,
    XLarge => _xLarge,   // ADD
    _      => _normal,
};
```

---

#### Step 2 - `Resources/AppResources.resx` and `.ru.resx`

Add the display name:

```xml
<!-- AppResources.resx -->
<data name="Settings_TextSizeXLarge" xml:space="preserve">
  <value>Extra Large</value>
</data>

<!-- AppResources.ru.resx -->
<data name="Settings_TextSizeXLarge" xml:space="preserve">
  <value>Очень крупный</value>
</data>
```

---

#### Steps 3-6

Follow the same pattern as Section 7.3 (Adding a New Colour Scheme) steps 4-7:
add `LocalizedResources` passthrough, no ViewModel setter change needed,
add radio row to `SettingsPopup.xaml` in the Text Size group (expand
`RowDefinitions` and `TextSizeLabel Grid.RowSpan`), add seeding in
`SettingsPopup.xaml.cs`.

---

### 7.6 Checklist: Files Changed Per Extension Type

| Extension | `.resx` (en) | `.resx` (ru) | `LocalizedResources` | `MainViewModel` | `CalculationService` | `ThemeService` / `FontSizeService` | `MainPage.xaml` | `MainPage.xaml.cs` | `SettingsPopup.xaml` | `SettingsPopup.xaml.cs` | `.csproj` |
|-----------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| New section | + | + | + | + | - | - | + | - | - | - | - |
| New static ticker | + | + | + | + | + | - | + | + | - | - | - |
| New live ticker | + | + | + | + | + | - | + | + | - | - | - |
| New colour scheme | + | + | + | - | - | + | - | - | + | + | - |
| New language | + | + | + | + | - | - | - | - | + | + | + |
| New font size preset | + | + | + | - | - | + | - | - | + | + | - |

`+` = must change. `-` = no change needed.

---

## 8. Debugging

---

### 8.1 Logging Infrastructure and `AeonLog` Gateway

#### Active logging registrations

`MauiProgram.cs` registers the debug provider and wires the application gateway:

```csharp
#if DEBUG
    builder.Logging.AddDebug();
#endif
var app = builder.Build();
AeonLog.Initialise(app.Services.GetRequiredService<ILoggerFactory>());
return app;
```

- **DEBUG builds:** MAUI framework events (binding errors, handler warnings) reach
  the platform debug channel. `AeonLog` calls reach the same channel.
- **RELEASE builds:** `AddDebug()` is compiled out. All `AeonLog` call sites are
  also erased by `[Conditional("DEBUG")]` - zero runtime overhead.

#### `AeonLog` gateway (`Services/AeonLog.cs`)

A static class with three methods, each carrying `[Conditional("DEBUG")]`:

| Method | `ILogger` level | Intended use |
|--------|-----------------|--------------|
| `Debug(cat, sub, msg, block?)` | `LogDebug` | Calculation inputs/outputs, method entry/exit, internal phase transitions |
| `Info(cat, sub, msg)` | `LogInformation` | User-driven actions: date change, settings change, language switch |
| `Warn(cat, sub, msg)` | `LogWarning` | Unexpected but recoverable states |

`AeonLog.Initialise(ILoggerFactory)` is called once from `MauiProgram` after
`builder.Build()`. Before initialisation (e.g. in unit tests) all calls resolve
to `NullLogger.Instance` and produce no output.

#### Message format convention

Every log message follows one of two patterns:

```
[CATEGORY] [SUBCATEGORY] message  key=value              (short methods)
[CATEGORY] [SUBCATEGORY] [BLOCK] message  key=value      (long multi-phase methods)
```

**Category tokens in use:**

| Token | Layer / concern |
|-------|----------------|
| `BOOT` | App startup, preferences restoration (`App.xaml.cs`) |
| `VM` | `MainViewModel` - user actions, timer, date save |
| `CALC` | `CalculationService` - all ticker calculations |
| `NAV` | Modal navigation (reserved for future use) |
| `THEME` | Theme and font-size changes (reserved) |
| `LOCALE` | Language switching (reserved) |
| `TINT` | Image tinting pipeline (reserved) |

#### `[BLOCK]` tag rule

Add a `[BLOCK]` fourth argument **only** when a method contains named internal
phases or a repeated scan loop where the same field names appear with different
semantic meanings across iterations. Use it for:

- `CalculateTimeJubilees` - 7-unit candidate scan: `UNIT_SCAN`, `WINNER`
- `CalculatePhotonPath` - phase dispatch and star catalogue walk:
  `INPUT`, `DISTANCE`, `PHASE_LOOKUP`, `STAR_MATCH`, `RESULT`

All other methods use only `[CATEGORY]` and `[SUBCATEGORY]`.

#### Instrumentation points in the codebase

| Location | Category | Level | What it logs |
|----------|----------|-------|--------------|
| `App()` ctor - after each `Preferences.Get` | `BOOT` | Info | `restored=value` for ColorScheme, TextSize, Language |
| `MainViewModel.SaveDate` - entry | `VM` | Info | `in: name=... date=...` |
| `MainViewModel.SaveDate` - after field assignment | `VM` | Debug | `out: BaseDateName=... BaseDateValue=... BaseDate=...` |
| `MainViewModel.UseMetric` setter | `VM` | Info | `value=...` |
| `MainViewModel.ColorScheme` setter | `VM` | Info | `value=...` |
| `MainViewModel.DisplayLanguage` setter | `VM` | Info | `value=... culture=...` |
| `MainViewModel.UpdateLiveCalculations` | `VM` | Debug | `thread=... isMainThread=...` |
| Entry of every `Calculate*` method | `CALC` | Debug | `baseDate=...` plus key input values |
| `CalculateTimeJubilees` - per unit | `CALC` | Debug | `[UNIT_SCAN]` `unit=... jubilee=... daysUntil=...` |
| `CalculateTimeJubilees` - winner | `CALC` | Debug | `[WINNER]` `unit=... jubilee=... daysUntil=...` |
| `CalculatePhotonPath` - after inputs | `CALC` | Debug | `[INPUT]` `baseDate=... seconds=...` |
| `CalculatePhotonPath` - after distance | `CALC` | Debug | `[DISTANCE]` `ly=... km=...` |
| `CalculatePhotonPath` - phase decision | `CALC` | Debug | `[PHASE_LOOKUP]` `phase=...` |
| `CalculatePhotonPath` - star catalogue walk | `CALC` | Debug | `[STAR_MATCH]` `star=... starLy=...` |
| `CalculatePhotonPath` - before return | `CALC` | Debug | `[RESULT]` `phase=... starName=... ly=...` |

---

### 8.2 Adding Log Calls to Application Code

Use `AeonLog` (see Section 8.1) for all application-level diagnostic output.
Never inject `ILogger<T>` directly into services or ViewModels - the gateway
already handles resolution and the `[Conditional("DEBUG")]` erasure.

#### Pattern for all application code

```csharp
private const string LogCat = "CALC";  // or VM, BOOT, etc.

// Simple entry log (short methods):
AeonLog.Debug(LogCat, nameof(CalculateCountdown), $"baseDate={baseDate:d}");

// With a [BLOCK] tag (long multi-phase methods only):
AeonLog.Debug(LogCat, nameof(CalculatePhotonPath), $"ly={lightYears:F4}", "DISTANCE");

// User action (Info level):
AeonLog.Info(LogCat, "SaveDate", $"in: name={name} date={date}");
```

#### Log level guidance

| Level | Use for |
|-------|---------|
| `AeonLog.Debug` | Calculation inputs/outputs, method entry/exit, phase transitions |
| `AeonLog.Info` | User-driven actions: date change, settings change, language switch |
| `AeonLog.Warn` | Unexpected but recoverable states (missing resource key, null result) |

#### Suppressing noisy framework output

To silence MAUI's verbose `Information`-level lifecycle logs while keeping
`Warning` and `Error` binding failures visible, add to `MauiProgram.cs`:

```csharp
#if DEBUG
    builder.Logging.AddDebug();
    builder.Logging.AddFilter("Microsoft.Maui", LogLevel.Warning);
#endif
```

---

### 8.3 Recommended Debug Instrumentation Points

All instrumentation below is already wired in the codebase. The table in
Section 8.1 lists every active call site. The examples below show the exact
`AeonLog` call at each location for reference.

#### Startup / preferences restoration (`App.xaml.cs`)

```csharp
AeonLog.Info("BOOT", "ColorScheme", $"restored={savedScheme}");
AeonLog.Info("BOOT", "TextSize",    $"restored={savedTextSize}");
AeonLog.Info("BOOT", "Language",    $"restored={savedLanguage}");
```

Useful when: the app starts with the wrong theme, font size, or language.
Confirms what was read from `Preferences` before `InitializeComponent()`.

#### Base date atomicity (`MainViewModel.SaveDate`)

```csharp
AeonLog.Info("VM",  "SaveDate", $"in: name={name} date={date}");
// ... after _baseDateName/_baseDateValue/_baseDate are set:
AeonLog.Debug("VM", "SaveDate", $"out: BaseDateName={_baseDateName} BaseDateValue={_baseDateValue} BaseDate={_baseDate:d}");
```

Useful when: ticker cards show stale data after a date change. Confirms that
`SaveDate` receives the new values and sets all three backing fields before
`UpdateAllCalculations` fires.

#### Timer thread marshalling (`MainViewModel.UpdateLiveCalculations`)

```csharp
AeonLog.Debug("VM", "Timer", $"thread={Environment.CurrentManagedThreadId} isMainThread={MainThread.IsMainThread}");
```

Useful when: live ticker cards stop updating. Confirms the timer is firing
and that `BeginInvokeOnMainThread` is delivering work to the UI thread.

#### Settings changes (`MainViewModel` setters)

```csharp
AeonLog.Info("VM", "UseMetric",   $"value={value}");
AeonLog.Info("VM", "ColorScheme", $"value={value}");
AeonLog.Info("VM", "Language",    $"value={_displayLanguage} culture={CultureInfo.CurrentUICulture.Name}");
```

Useful when: a settings change appears to apply partially or not at all.
The `Info` level means these lines are visible without enabling `Debug` verbosity.

#### Calculation entry (all `Calculate*` methods)

```csharp
AeonLog.Debug("CALC", nameof(CalculateCountdown),     $"baseDate={baseDate:d}");
AeonLog.Debug("CALC", nameof(CalculateGalacticCommute), $"baseDate={baseDate:d} seconds={seconds} useMetric={useMetric}");
```

Useful when: a specific ticker shows unexpected output. Compare the logged
`baseDate` with the expected value to confirm the correct input is reaching
the calculation.

#### `CalculatePhotonPath` phase walk (block-tagged)

```csharp
AeonLog.Debug("CALC", nameof(CalculatePhotonPath), $"baseDate={baseDate:d} seconds={seconds}", "INPUT");
AeonLog.Debug("CALC", nameof(CalculatePhotonPath), $"ly={lightYears:F4} km={kmTraveled:N0}",   "DISTANCE");
AeonLog.Debug("CALC", nameof(CalculatePhotonPath), $"phase=Interstellar ly={lightYears:F4}",   "PHASE_LOOKUP");
AeonLog.Debug("CALC", nameof(CalculatePhotonPath), $"star=Fomalhaut starLy=25.13",             "STAR_MATCH");
AeonLog.Debug("CALC", nameof(CalculatePhotonPath), $"phase=PastStar starName=Fomalhaut ly=25.13", "RESULT");
```

Useful when: the photon path ticker shows the wrong phase or wrong star.
The `[BLOCK]` tags let an AI agent filter each stage independently.

#### `CalculateTimeJubilees` unit scan (block-tagged)

```csharp
AeonLog.Debug("CALC", nameof(CalculateTimeJubilees), $"baseDate={baseDate:d} passedDays={passedDays}");
AeonLog.Debug("CALC", nameof(CalculateTimeJubilees), $"unit=Years jubilee=60 daysUntil=107",   "UNIT_SCAN");
AeonLog.Debug("CALC", nameof(CalculateTimeJubilees), $"unit=Days jubilee=25000 daysUntil=2934","UNIT_SCAN");
AeonLog.Debug("CALC", nameof(CalculateTimeJubilees), $"unit=Years jubilee=60 daysUntil=107",   "WINNER");
```

Useful when: the jubilee ticker shows an unexpected unit or date. The
`[UNIT_SCAN]` lines show every candidate; `[WINNER]` shows which one was chosen.

---

### 8.4 Viewing Logs by Platform

#### Windows

**Visual Studio Output window (Debug pane):**

1. Run with **F5** (Debug configuration).
2. Open **View → Output** → select **Debug** from the dropdown.
3. `AddDebug()` output and `System.Diagnostics.Debug.WriteLine()` calls appear here in real time.

**Filter by application output:**

In the Output window search bar, type `[Aeonpulse]` or any prefix used in your
log messages to isolate application lines from MAUI framework noise.

**DebugView (Sysinternals) - without Visual Studio:**

1. Download `DebugView` from https://learn.microsoft.com/sysinternals/downloads/debugview
2. Run as Administrator.
3. Enable **Capture → Capture Win32** and **Capture → Capture Global Win32**.
4. Run the app. All `Debug.WriteLine` output appears in the DebugView window.

#### Android

**Via `adb logcat`:**

ADB path on this machine: `C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe`

Add to `PATH` for convenience:

```powershell
$env:PATH += ";C:\Program Files (x86)\Android\android-sdk\platform-tools"
```

Stream all app output (package `com.aeonpulse.app`):

```
adb logcat --pid=$(adb shell pidof com.aeonpulse.app) -v time
```

Filter to MAUI/Mono output only:

```
adb logcat -s mono:D -v time
```

Filter to application `Debug.WriteLine` output (tag `mono-stdout`):

```
adb logcat -s mono-stdout:D -v time
```

**Via Visual Studio Android Device Log:**

1. Run the app from Visual Studio with **F5** on an Android target.
2. Open **View → Other Windows → Android Device Log**.
3. Filter by package name `com.aeonpulse.app`.

`AddDebug()` output appears under the `Microsoft-Maui` tag. `Debug.WriteLine()`
output appears under `mono-stdout`.

**Confirm the emulator is running:**

```
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" devices -l
```

#### iOS / Mac Catalyst

**Via Xcode Console (macOS only):**

1. Open Xcode → **Window → Devices and Simulators**.
2. Select the device or simulator running the app.
3. Click **Open Console** (bottom-left of the device panel).
4. Filter by process name `Aeonpulse`.

`AddDebug()` output and `Debug.WriteLine()` calls appear as `NSLog` entries under
the process.

**Via `Console.app` on macOS:**

1. Open `/Applications/Utilities/Console.app`.
2. Select the device or `My Mac` (for Mac Catalyst).
3. In the search bar, type `Aeonpulse` and press Enter.

**Via Visual Studio for Mac (if using VS Mac):**

The **Application Output** pad shows `Debug.WriteLine()` output inline during
an F5 debug session.

#### All Platforms - MAUI Binding Error Logging

MAUI emits binding error messages (e.g., `Binding: 'Xxx' property not found on 'MainViewModel'`)
through the `ILogger` pipeline automatically in Debug builds. These appear in the
Visual Studio Output window (Windows) or Xcode Console (iOS/Mac) without any
additional setup, because `AddDebug()` is already registered in `MauiProgram.cs`.

To make binding errors louder, add a minimum level filter in `MauiProgram.cs`:

```csharp
#if DEBUG
    builder.Logging.AddDebug();
    builder.Logging.AddFilter("Microsoft.Maui", LogLevel.Warning);
#endif
```

This suppresses MAUI's verbose `Information`-level lifecycle logs while keeping
all `Warning` and `Error` binding failures visible.

---

### 8.5 Diagnosing Common Failure Modes

#### Ticker cards show empty or stale text

**Likely causes and checks:**

1. `UpdateAllCalculations()` not called after `SaveDate()`.
   - Add `LogDebug` to `SaveDate()` and confirm it reaches `UpdateAllCalculations()`.
   - Verify `BaseDateName`, `BaseDateValue`, `BaseDate` are all set before the call.

2. `CalculationService` method returning empty `TickerData`.
   - Add `LogDebug` at the start and end of the suspect `CalculateXxx()` method,
     logging the `baseDate` argument and the `BriefText` return value.

3. `AppResources` string key missing from one locale.
   - Switch language in Settings. If the string appears in one language but not
     another, a key is missing from the non-working `.resx` file.
   - Check: `AppResources.SomeKey` returns an empty string (not `null`) when the
     key is missing from a satellite assembly - so a missing key shows as blank,
     not a crash.

#### Live tickers stop updating

**Likely causes and checks:**

1. Timer stopped or never started.
   - Add `Debug.WriteLine` to the `Timer.Elapsed` lambda and confirm it fires.
   - Confirm `_updateTimer.Start()` is called in `MainViewModel` constructor.

2. `MainThread.BeginInvokeOnMainThread` not reaching the UI thread.
   - Add `Debug.WriteLine($"IsMainThread: {MainThread.IsMainThread}")` inside
     `UpdateLiveCalculations()` to confirm thread context.

3. `PropertyChanged` not firing after setting `TickerData.BriefText`.
   - `TickerData` (base class) implements `INotifyPropertyChanged` directly. Confirm the typed result property
     setter calls `OnPropertyChanged()`. In-place mutation (setting `.BriefText`
     directly) is correct - replacing the whole `TickerData` object with `=` is
     also valid and triggers `PropertyChanged` on the ViewModel property.

#### Theme or font size does not apply

**Likely causes and checks:**

1. `StaticResource` used instead of `DynamicResource` in XAML.
   - Search `MainPage.xaml` and all popup XAML files for `StaticResource CyberCyan`
     (or any colour/font-size key). `StaticResource` reads once at construction time;
     only `DynamicResource` responds to `Application.Current.Resources` mutations.

2. `ThemeService.ApplyScheme()` / `FontSizeService.ApplyPreset()` not called.
   - `ColorScheme` and `TextSize` setters on `MainViewModel` both call the service.
     If a setting is changed outside these setters, the service is not invoked.

3. New colour key added to `ThemeService` palette but not to `Colors.xaml`.
   - `Colors.xaml` provides the startup default. If a new key is missing there,
     the first frame renders without it, and the resource key does not exist in
     `Application.Current.Resources` for `ThemeService` to overwrite.

#### Language change does not update all strings

**Likely causes and checks:**

1. `Loc.Invalidate()` not called after `ApplyLanguage()`.
   - The `DisplayLanguage` setter in `MainViewModel` calls both. If `DisplayLanguage`
     was not set via the property (e.g., the backing field was set directly), the
     side effects do not fire.

2. String bound via `{x:Static resources:AppResources.Key}` instead of `{Binding Loc.Key}`.
   - `{x:Static}` is evaluated once at XAML inflation and never re-evaluates.
   - Only `{Binding Loc.Xxx}` responds to `Loc.Invalidate()`.

3. `CalculationService` ticker output strings not regenerated.
   - `UpdateAllCalculations()` is called at the end of the `DisplayLanguage` setter.
     If the ticker output strings are still in the old language, confirm the setter
     reached `UpdateAllCalculations()` and that `AppResources.Culture` was already
     set before that call.

#### Tint not appearing on icons

**Likely causes and checks:**

1. `MauiProgram` mapper not registered.
   - `ImageTint` only works if `AppendToMapping` was called in `MauiProgram.CreateMauiApp()`.
     The mapper registration is the hidden dependency documented in `ImageTint.cs`.

2. `DynamicResource` colour key not yet in `Application.Current.Resources` when mapper fires.
   - On first layout pass, `ThemeService.ApplyScheme()` must have run before the
     mapper fires. This is guaranteed by `App.xaml.cs` constructor ordering
     (scheme applied before `InitializeComponent()`).

3. Windows platform: `Image` tinting is a known no-op.
   - `Platforms/Windows/TintHelper.cs` `ApplyImageTint` is intentionally empty.
     Only `ImageButton` tinting is approximated via `Button.Foreground`.

---

### 8.6 Debug Build vs Release Build Differences

| Behaviour | Debug | Release |
|-----------|-------|---------|
| `AddDebug()` logging provider | Active | Compiled out (`#if DEBUG`) |
| MAUI binding error log messages | Emitted to debug output | Suppressed |
| `System.Diagnostics.Debug.WriteLine()` | Writes to debug output | No-op (stripped by JIT) |
| `System.Diagnostics.Debugger.Break()` | Breaks into debugger | No-op |
| Android APK signing | Debug keystore (`debug.keystore`) | Production keystore required |
| Android APK size | Larger (no R8 shrinking by default) | Smaller (R8 shrink + obfuscate) |
| iOS bitcode | Not generated | Included for App Store |
| `[Conditional("DEBUG")]` methods | Called | Not emitted at call sites |

**Important:** `Debug.WriteLine()` calls are **not** compiled out in Release - they
become no-ops at the JIT level. To ensure they produce zero overhead in Release,
wrap them in `#if DEBUG` or use `[Conditional("DEBUG")]`:

```csharp
#if DEBUG
    System.Diagnostics.Debug.WriteLine($"[MyClass] value={someValue}");
#endif
```

---

### 8.7 XAML Hot Reload and Live Visual Tree

#### MAUI Hot Reload (Visual Studio 2022)

MAUI Hot Reload applies XAML changes to a running app without restarting it.

- Works on all platforms when running with **F5** (Debug).
- Save a `.xaml` file - changes apply automatically.
- **Does not work for:** C# code changes, new bindings added to a ViewModel,
  `App.xaml` resource dictionary changes (requires restart).
- **Known limitation:** Hot Reload does not re-run `InitializeComponent()` on
  existing page instances; popups already open will not reflect changes until
  they are closed and re-opened.

#### Live Visual Tree (Windows only)

1. Run with **F5** on Windows.
2. Open **Debug → Windows → Live Visual Tree**.
3. Inspect any element's `ActualWidth`, `ActualHeight`, `IsVisible`, `Opacity`,
   and bound property values in real time.
4. Use **Enable selection in running application** (cursor icon in the toolbar)
   to click a UI element and jump directly to it in the tree.

Useful for diagnosing `topOffset` geometry problems in `DeepDivePopup` and
`MainMenuPopup` - inspect `NavBar.Height` and `TimelineHeading.Height` values
live before the popup is pushed.

---

### 8.8 Attaching the Debugger to a Running Process

#### Android (already deployed)

1. In Visual Studio: **Debug → Attach to Android Process…**
2. Select the running `com.aeonpulse.app` process from the list.
3. Set breakpoints in any `.cs` file. The debugger maps managed frames correctly
   on both emulator and physical device.

#### Windows (already running)

1. In Visual Studio: **Debug → Attach to Process…** (Ctrl+Alt+P)
2. Filter for `Aeonpulse.exe`.
3. Select and attach with **Managed (.NET)** debugger type.

---

## 9. Guardrails & Style

This section is the **primary reference for AI agents before making any change**.
Rules are grouped by concern. Each rule states whether it is a hard constraint
(violation causes a build failure or a silent runtime regression) or a style
requirement (violation degrades maintainability or consistency).

The authority for every rule is the existing codebase. Rules are not aspirational -
they describe what the code already does and must continue to do.

---

### 9.1 Architecture

#### DO

- **Keep all business logic in `CalculationService`.** Every temporal, scientific,
  and numerological computation lives there. Code-behind files and `MainViewModel`
  must not contain domain calculations.

- **Keep all application state in `MainViewModel`.** Every typed ticker result property,
  every section/card expansion bool, every user setting, and every `ICommand`
  instance is owned there. No state lives in code-behind or in service classes.

- **Restrict `*.xaml.cs` code-behind to navigation and event-to-command bridging.**
  Code-behind may: push/pop modals, wire ViewModel events, measure element geometry
  for popup positioning, seed radio buttons from ViewModel state. Nothing else.

- **Use `Navigation.PushModalAsync` / `Navigation.PopModalAsync` exclusively.**
  All secondary surfaces are modal pages. `PushAsync` / `PopAsync` (non-modal) are
  not used anywhere in the app and must not be introduced.

- **Pop from within the popup itself.** Every popup calls
  `Navigation.PopModalAsync()` from its own code-behind. `MainPage` never pops a
  popup it has pushed.

- **Use `MainViewModel.SaveDate()` as the only entry point for base-date changes.**
  It sets all three backing fields atomically and calls `UpdateAllCalculations()`
  exactly once. Setting `BaseDateName`, `BaseDateValue`, or `BaseDate` individually
  triggers recalculations with partially-updated state.

- **Marshal all ViewModel property writes to the main thread.** The 1-second timer
  fires on a thread-pool thread. `MainViewModel` uses
  `MainThread.BeginInvokeOnMainThread(UpdateLiveCalculations)` for this. Any new
  background-thread callback that writes a bound property must do the same.

- **Use the existing singleton pattern for services.**
  `ThemeService.Instance`, `FontSizeService.Instance`. Do not register them in the
  DI container unless refactoring the entire service layer.

#### DO NOT

- **Do not add business logic to `*.xaml.cs` files.** Date parsing, string
  formatting, calculation, and data transformation belong in `CalculationService`.

- **Do not add state to code-behind.** Guard flags (`_isXxxOpen`) are the only
  instance fields permitted in code-behind. They exist solely to prevent
  double-push on rapid taps and have no domain meaning.

- **Do not use `Shell` navigation.** The project does not use `AppShell`. All
  navigation is `Navigation.PushModalAsync` from `MainPage.xaml.cs`.

- **Do not construct popups from outside `MainPage.xaml.cs`.** `MainMenuPopup`
  uses callback delegates specifically so that follow-up popups are always pushed
  on `MainPage`'s navigation stack, not from within an already-dismissed popup
  context.

- **Do not call `UpdateAllCalculations()` more than once per user action.** Each
  user-visible action (date change, settings change, language change) results in
  exactly one `UpdateAllCalculations()` call. Multiple calls produce visible
  flicker on live tickers.

- **Do not introduce a DI container for application classes** (`CalculationService`,
  `MainViewModel`, popup pages). The three existing NuGet packages cover everything
  the app needs. DI is used only by `MauiProgram` for framework plumbing.

---

### 9.2 MVVM Pattern

#### DO

- **Implement `INotifyPropertyChanged` manually.** The project uses the
  `[CallerMemberName]` pattern with no base class, no source generators, and no
  `CommunityToolkit.Mvvm`. Every new ViewModel property must follow:

  ```csharp
  private T _field;
  public T Property
  {
      get => _field;
      set { _field = value; OnPropertyChanged(); }
  }
  ```

- **Use `System.Windows.Input.Command` for all commands.** Commands are constructed
  as `new Command(...)` or `new Command(async () => ...)` inline in the
  `MainViewModel` constructor. No `RelayCommand`, no `AsyncCommand`, no
  `[RelayCommand]` attribute.

- **Declare commands as `public ICommand` properties** on `MainViewModel` with
  backing initialisation in the constructor.

- **Expose `LocalizedResources` as `Loc`** on `MainViewModel`. XAML binds
  localised strings as `{Binding Loc.Xxx}`. Do not add a second localisation
  accessor property.

#### DO NOT

- **Do not use CommunityToolkit.Mvvm, Prism, or any other MVVM framework.**
  The three existing NuGet packages are the complete dependency list. Do not add
  new packages to implement patterns the project already handles manually.

- **Do not use `[ObservableProperty]`, `[RelayCommand]`, or any source-generator
  attribute from an external MVVM toolkit.** These require packages that are not
  referenced.

- **Do not bind directly to `MainViewModel` properties from popup XAML using
  `{Binding}` without setting `BindingContext`.** Popups receive the ViewModel as a
  constructor argument. `SettingsPopup` sets `BindingContext = viewModel` explicitly.
  Other popups that use `{x:Static}` do not set a `BindingContext`.

- **Do not replace `TickerData` object references to update live text.** Set
  `.BriefText` and `.FullText` on the existing `TickerData` instance in-place when
  only the text content changes (live tickers). Replacing the entire property
  reference is only correct when recalculating from scratch. Both approaches work;
  in-place is preferred for live tickers because it avoids re-evaluating all
  bindings on the card.

---

### 9.3 Colours and Theming

#### DO

- **Use `{DynamicResource}` for all colour references in XAML.** `ThemeService`
  overwrites `Application.Current.Resources` at runtime. A `StaticResource`
  reference to any colour key reads the startup value once and never updates.

  ```xml
  <!-- CORRECT -->
  <Label TextColor="{DynamicResource CyberCyan}" />

  <!-- WRONG - will not repaint on theme switch -->
  <Label TextColor="{StaticResource CyberCyan}" />
  ```

- **Use `{StaticResource}` only for values that are never overridden at runtime.**
  Safe keys: `Opacity90`, `Opacity80`, `Opacity60`, `Opacity20`, `Opacity10`,
  `SpacingSmall`, `SpacingMedium`, `SpacingLarge`, `BorderRadiusSmall`,
  `BorderRadiusMedium`, `BorderRadiusLarge`. `ThemeService` does not touch these.

- **Reference all colours by their semantic key names.** The 12 theme-managed keys
  are: `SpaceDark`, `SpaceDarker`, `CyberCyan`, `CyberPurple`, `CyberPink`,
  `NeonGreen`, `NeonGreenDark`, `TextWhite`, `TextDim`, `TextGray`,
  `CardBackground`, `CardDark`. Use these everywhere.

- **When adding a new colour scheme, provide values for all 12 keys** in the
  palette dictionary. A missing key leaves the previous scheme's value for that key
  and produces an inconsistent mixed-theme appearance.

- **Tint icons using `helpers:ImageTint.Color="{DynamicResource XxxColor}"`.**
  This is the only tinting mechanism. It calls native colour-filter APIs per
  platform and automatically re-applies when `DynamicResource` updates the value.

#### DO NOT

- **Do not hardcode colour hex values in XAML files** other than the
  semi-transparent modal backdrop (`#80000000` on `BackgroundColor` of popup
  `ContentPage` elements). That value is intentionally literal because it is not
  theme-managed - it is a fixed 50% black overlay.

- **Do not use named colour constants** (`Red`, `Blue`, `Transparent` etc.) for
  any UI colour except `Transparent` for backgrounds that are intentionally
  transparent (e.g., `RadioButton` fill before checked). Palette colours must
  come from the resource dictionary.

- **Do not hardcode colours in C# code** (outside `ThemeService` palette
  dictionaries and platform `TintHelper` implementations). No
  `Color.FromArgb("#...")` calls in ViewModels, services, or code-behind.

- **Do not add new colour keys that are only used in one place.** Extend the
  existing 12-key palette only when the new colour participates in all three
  colour scheme variants (`DefaultDark`, `HighContrastDark`, `HighContrastLight`).

---

### 9.4 Font Sizes

#### DO

- **Use `{DynamicResource}` for all `FontSize` bindings.** `FontSizeService`
  overwrites the five font-size keys at runtime. A `StaticResource` reference
  reads the startup value and never responds to the user's text-size preference.

  ```xml
  <!-- CORRECT -->
  <Label FontSize="{DynamicResource FontSizeMedium}" />

  <!-- WRONG - ignores user text-size setting -->
  <Label FontSize="{StaticResource FontSizeMedium}" />
  ```

- **Use only the five defined font-size keys:** `FontSizeSmall`, `FontSizeMedium`,
  `FontSizeLarge`, `FontSizeXLarge`, `FontSizeTitle`.

#### DO NOT

- **Do not hardcode numeric font size values in XAML or C#.** Every font size must
  come from the resource dictionary so `FontSizeService` presets apply uniformly.

---

### 9.5 Localisation

#### DO

- **Use `{Binding Loc.Xxx}` for all user-visible strings on `MainPage`** and any
  page that must update strings without being reconstructed (i.e., pages that
  remain on the navigation stack while the language is changed).

- **Use `{x:Static resources:AppResources.Xxx}` only in freshly-constructed
  popups** (`ChangeDatePopup`, `MainMenuPopup`, `DeepDivePopup`) that are created
  anew every time they open. These pages never exist while a language change occurs.

- **Add every new user-visible string to both `.resx` files simultaneously:**
  `Resources/AppResources.resx` (English) and `Resources/AppResources.ru.resx`
  (Russian). A key present in `en` but absent in `ru` produces an empty string
  (not a crash) when the Russian locale is active.

- **Add a passthrough property to `LocalizedResources.cs`** for every new key
  that will be used via `{Binding Loc.Xxx}`:
  ```csharp
  public string MyKey => AppResources.MyKey;
  ```

- **Use `{placeholder}` tokens in `.resx` template strings** and replace them
  with `string.Replace("{placeholder}", value)` in `CalculationService`. The
  `Tease_*` group uses `string.Format` with `{0}` positional parameters - match
  the style of the template being edited.

- **Call `Loc.Invalidate()` after every `ApplyLanguage()` call** to push all new
  strings to bound labels simultaneously. The `MainViewModel.DisplayLanguage`
  setter already does this. Do not bypass it.

#### DO NOT

- **Do not hardcode user-visible strings in XAML or C#.** Every string that
  appears in the UI must come from `AppResources`. This includes section titles,
  ticker titles, button labels, placeholder text, and unit strings.

- **Do not add string keys to `AppResources.resx` without also adding them to
  `AppResources.ru.resx`.** An asymmetric key set produces invisible blank text
  in the Russian locale and is the most common localisation regression.

- **Do not read `AppResources` keys directly in ViewModel or code-behind** for
  UI display purposes. Read them through `LocalizedResources.Loc` so language
  switches propagate automatically.

- **Do not cache `AppResources` string values** in local variables or properties
  that are not re-read after a language change. `CalculationService` reads
  `AppResources` at call time on every invocation specifically to avoid this.

- **Do not edit `AppResources.resx` or `AppResources.ru.resx` or AppResources.Designer.cs
  in one pass using  the `edit_file` tool.** These files are too large for the
  tool (each exceeds 60 KB) and a single-pass edit will silently truncate
  content or corrupt file structure. Use targeted terminal writes instead: one
  `[System.IO.File]::ReadAllText` / `Replace` / `WriteAllText` call per logical
  change, saving after each replacement so that a mid-sequence failure leaves the
  file in a consistent partial state rather than destroying it.

---

### 9.6 XAML Structure and Encoding

#### DO

- **Save all `.xaml` files as UTF-8 with BOM** (byte order mark: `0xEF 0xBB 0xBF`
  as the first three bytes). This is verified for all 10 current XAML files. The
  build fails with `MSB4018 XamlCTask` if a XAML file is saved without a BOM.
  In Visual Studio: **File → Save As → Save with Encoding → UTF-8 with signature**.

- **Use `Border` for all new container elements.** `Frame` is obsolete in .NET 9.
  The existing popup XAML files still use `Frame` (accepted technical debt). Do
  not use `Frame` in any new XAML.

- **Follow the existing ticker card XAML structure exactly** when adding new
  ticker cards: outer `Border` with `CardDark` background, inner
  `VerticalStackLayout`, 3-column `Grid` header, `BriefText` label, `FullText`
  label with `IsVisible` binding.

- **Add `<!-- AI: ... -->` comments above every new structural XAML element** that
  has non-obvious layout, a hidden dependency, or multiple binding sources. See
  Section 4.2 for the exact comment syntax.

- **Add `<!-- Binding: ... -->` comments above every new binding** that references
  a non-obvious ViewModel path, especially multi-segment paths or converter-driven
  bindings.

- **Use `TapGestureRecognizer` on `Grid` elements for menu items** rather than
  `Button`. This avoids hit-testing conflicts on Android and provides finer layout
  control. `MainMenuPopup` demonstrates the established pattern.

#### DO NOT

- **Do not use `Shell.NavBarIsVisible="True"` on any page.** Every page suppresses
  the platform navigation bar. There is no Shell navigation in this app.

- **Do not use `x:DataType` on any page.** Compiled bindings are not enabled.
  All bindings are runtime-evaluated. `XC0022` warnings for this are in the known
  warning set (Section 6.4) and are accepted.

- **Do not add `Application.MainPage` setter usages.** It is obsolete in .NET 9.
  The single existing usage in `App.xaml.cs` is accepted technical debt.

- **Do not add new `MergedDictionaries` entries to `App.xaml`** without
  documenting the load order constraint: `Colors.xaml` must always be first
  because `Styles.xaml` references its keys.

---

### 9.7 Comment Style and Non-ASCII Characters

#### DO

- **Use `/// XML doc comments` on every `public` class and `public` method.**
  Follow the `<summary>` / `<para>` / `<list>` structure established in
  `CalculationService.cs`, `MainPage.xaml.cs`, and `App.xaml.cs`. Document
  side effects and hidden dependencies in a `<para><b>Side effects / Hidden
  dependencies:</b>` block.

- **Use plain `//` line comments** for inline code explanation within method
  bodies.

- **Use `<!-- AI: ... -->` comments in XAML** for structural layout documentation.
  See Section 4.2 for the full syntax contract.

- **Keep all comment text ASCII-only.** This applies to `//`, `/* */`, `///`, and
  `<!-- -->` comment blocks in all file types.

#### DO NOT

- **Do not put non-ASCII characters inside comment blocks of any kind.** This
  includes: Unicode em-dash (`—`), ellipsis (`...`), curly quotes, box-drawing
  characters, and emoji.

  ```csharp
  // WRONG - em-dash in comment
  // Minutes — guarded: large base dates can overflow DateTime.MaxValue

  // CORRECT
  // Minutes - guarded: large base dates can overflow DateTime.MaxValue
  ```

  ```xml
  <!-- WRONG - non-ASCII dash -->
  <!-- AI: Menu panel – anchored below NavBar -->

  <!-- CORRECT -->
  <!-- AI: Menu panel - anchored below NavBar -->
  ```

- **Do not use XML doc comments (`///`) in XAML files.** They are meaningless
  in XML and will cause parse errors.

- **Do not use `<!-- AI: ... -->` comments in C# files.** Use `///` XML doc
  comments or plain `//` comments there.

- **Do not omit `///` summaries on public symbols.** The project has 125 XML doc
  comment blocks. Every new public class and public method must add to this count.

---

### 9.8 New NuGet Packages and Dependencies

#### DO

- **Evaluate whether the existing three packages cover the need before adding any
  new package.** The three packages are:
  - `Microsoft.Maui.Controls 9.0.0`
  - `Microsoft.Maui.Controls.Compatibility 9.0.0`
  - `Microsoft.Extensions.Logging.Debug 9.0.0`

- **If a new package is genuinely required**, add it to `Aeonpulse.csproj` with
  an explicit version pinned to the same major version as the existing packages
  (`9.x.x`), and document the reason in the `Agents.md` Section 1 quick-reference
  table and Section 2 file description for `Aeonpulse.csproj`.

#### DO NOT

- **Do not add packages to implement patterns the project already handles
  manually.** Specifically: no `CommunityToolkit.Mvvm` (MVVM is manual), no
  `CommunityToolkit.Maui` (popups use `PushModalAsync`), no `Prism`, no
  `ReactiveUI`, no `Newtonsoft.Json` (no JSON in the app).

- **Do not copy code from public internet sources** (Stack Overflow, GitHub
  snippets, blog posts) without verifying it against the project's architectural
  constraints. Internet code frequently uses patterns incompatible with this
  project: `Shell` navigation, `CommunityToolkit` commands, `StaticResource`
  colour references, hardcoded strings.

- **Do not use `Microsoft.Maui.Controls.Compatibility` types** in new code. This
  package is present for backward compatibility only. New code must use the
  `.NET MAUI` equivalents (e.g., `Border` not `Frame`, `ImageButton` not
  `Button` with image content).

---

### 9.9 Platform-Specific Code

#### DO

- **Add platform-specific implementations only in the appropriate
  `Platforms/{Platform}/` directory.**

- **Use `partial` methods** declared in `MauiProgram.cs` to bridge platform
  implementations. `ApplyImageTint` and `ApplyImageButtonTint` demonstrate the
  exact pattern.

- **Implement platform-specific behaviour for all four active platforms**:
  Android, iOS, Mac Catalyst, Windows. A partial method with no implementation
  on a given platform is a silent no-op - document it explicitly if intentional
  (see `Platforms/Windows/TintHelper.cs` for the Windows `Image` tinting no-op
  example).

- **Guard against `null` platform views** before casting in `TintHelper`
  implementations. The `if (handler.PlatformView is not XxxType nativeView) return;`
  pattern is established in all four `TintHelper.cs` files.

#### DO NOT

- **Do not use `#if ANDROID`, `#if IOS`, `#if MACCATALYST`, or `#if WINDOWS`
  preprocessor directives** in shared project files (outside `Platforms/`).
  Platform divergence must live in the `Platforms/` directory via `partial` methods
  or multi-targeted files, not in `#if` blocks scattered through shared code.

- **Do not modify `Helpers/TintBehavior.cs`.** It is a tombstone file that exists
  only to prevent "class not found" errors in cached build artefacts. The comment
  at the top of the file explains this. The functional replacement is
  `Helpers/ImageTint.cs` + `Platforms/{Platform}/TintHelper.cs`.

---

### 9.10 Persistence

#### DO

- **Use `Microsoft.Maui.Storage.Preferences` exclusively** for persisting user
  settings. The currently-persisted keys are: `"ColorScheme"`, `"TextSize"`,
  `"DisplayLanguage"`, `"UseMetric"`.

- **Read `Preferences` in `App.xaml.cs` constructor before `InitializeComponent()`**
  for any setting that must be applied before the first rendered frame.

- **Write `Preferences` inside the `MainViewModel` property setter** for the
  corresponding setting, immediately after applying the change.

- **Use the service constant as the default value** in `Preferences.Default.Get()`:
  ```csharp
  Preferences.Default.Get("ColorScheme", ThemeService.DefaultDark)
  Preferences.Default.Get("TextSize",    FontSizeService.Normal)
  Preferences.Default.Get("DisplayLanguage", MainViewModel.LangDefault)
  Preferences.Default.Get("UseMetric",   true)
  ```

#### DO NOT

- **Do not persist data using files, SQLite, or any mechanism other than
  `Preferences`** for the current scope of user settings. The app has no
  structured data storage needs beyond key-value preferences.

- **Do not store base-date name or base-date value in `Preferences`** at this time.
  These are ViewModel fields reset to defaults on each app launch. If persistence
  is added for them, follow the same read-in-`App.xaml.cs` / write-in-setter
  pattern as the other three keys.

---

### 9.11 `Agents.md` Maintenance

#### DO

- **Update `Agents.md` on every structural change to the codebase.** The file
  header states: *"Update this file and all appropriate markup blocks upon each
  change."* This is a hard requirement, not a suggestion.

- **Update the specific sections that are affected by each change:**

  | Change type | Sections to update |
  |-------------|-------------------|
  | New file added | Section 2 (Complete File Overview) |
  | New ticker added | Section 1 (Ticker Cards table), Section 5 (Root Node 4 and 5) |
  | New section added | Section 1 (Application Structure), Section 5 (Root Node 4) |
  | New colour scheme | Section 1 (User-Configurable Settings), Section 5 (Root Node 8), Section 7.3 |
  | New language | Section 1 (Quick-Reference Facts), Section 5 (Root Node 7), Section 7.4 |
  | New `[AIContext]` role | Section 4.1 role vocabulary table |
  | New XAML `<!-- AI: -->` comment | Section 4.2 inventory table |
  | New persisted preference | Section 3.8 persistence table, Section 5 Root Node 4 |
  | New NuGet package | Section 1 quick-reference table |
  | Existing file substantially changed | Section 2 description for that file |
| New `AeonLog` instrumentation point added | Section 8.1 instrumentation table, Section 8.3 example |

- **Update the `> Last updated:` date** at the top of `Agents.md` when making
  any substantive change to the file. Always call `Get-Date -Format \"yyyy-MM-dd\"` first
  and copy the output verbatim - do not type the date from memory. Month and
  day values typed without checking are a recurring source of incorrect dates.

- **Keep section numbering and heading names stable.** Other sections in `Agents.md`
  and the `Agents.N.md` supplementary files cross-reference each other by section
  number. Renaming or renumbering a section breaks those references.

#### DO NOT

- **Do not make code changes without checking whether `Agents.md` needs updating.**
  An `Agents.md` that describes a different codebase than what exists is worse
  than no documentation at all.

- **Do not attempt to edit `Agents.md` in one pass using the `edit_file` tool.**
  The file exceeds 180 KB and the tool will truncate or corrupt it. Use targeted
  `[System.IO.File]::ReadAllText` / `Replace` / `WriteAllText` terminal writes
  instead: one `Replace` call per logical change, each saved immediately so that a
  failure mid-sequence leaves the file in a consistent partial state rather than
  destroying it entirely.

- **Prefer regenerating `AppResources.Designer.cs` over editing it manually.** It is auto-generated by
  `PublicResXFileCodeGenerator` from `AppResources.resx`. If a full regeneration is not immediately
  available, add the new `public static string` property following the existing pattern and regenerate
  on the next build. Always edit `AppResources.resx` and `AppResources.ru.resx` first.

---

### 9.12 Quick Violation Checklist

Use this checklist before committing any change. A `NO` answer on any line means
the change violates a guardrail and must be corrected first.

| # | Check | Expected |
|---|-------|----------|
| 1 | All new colour/font-size XAML bindings use `DynamicResource`? | YES |
| 2 | All new user-visible strings are in both `.resx` files and `LocalizedResources.cs`? | YES |
| 3 | All new `.xaml` files saved as UTF-8 with BOM? | YES |
| 4 | All comment blocks (XAML and C#) contain only ASCII characters? | YES |
| 5 | No business logic added to `*.xaml.cs` code-behind? | YES |
| 6 | No hardcoded hex colour values in XAML (except `#80000000` backdrop)? | YES |
| 7 | No hardcoded numeric font sizes in XAML or C#? | YES |
| 8 | No new `Frame` elements (use `Border` instead)? | YES |
| 9 | No new NuGet packages without architectural justification? | YES |
| 10 | No new `#if ANDROID / IOS / WINDOWS` blocks in shared files? | YES |
| 11 | All new `public` classes and methods have `///` XML doc summaries? | YES |
| 12 | All new `[AIContext]` roles documented in `Agents.md` Section 4.1? | YES |
| 13 | All new structural XAML elements have `<!-- AI: ... -->` comments? | YES |
| 14 | `Agents.md` updated for every structural change? | YES |
| 15 | Build produces only known warning codes (CS0618, CS8767, CS0414, XC0022)? | YES |
| 16 | `dotnet test Aeonpulse.Tests\Aeonpulse.Tests.csproj` passes (222+ tests, 0 failures)? | YES |
| 17 | All new `AeonLog` calls use `[Conditional("DEBUG")]` via the gateway (not raw `ILogger` or `Debug.WriteLine`)? `[BLOCK]` tag added only for methods with named internal phases? | YES |
| 18 | Commit message ends with `AI: GitHub Copilot (<model>)` trailer (§9.13)? If the commit includes **any human-authored edits**, does the trailer read `AI: GitHub Copilot (<model>) + manual changes`? | YES |


---

### 9.13 Commit Signature

#### DO

- **Add your AI model identifier as a trailer in every commit message** for
  commits where all changes were made entirely by the AI agent. Append the
  signature as the last line of the commit body, separated from the rest of
  the message by a blank line:

  ```
  feat: short description

  - detail line 1
  - detail line 2

  AI: GitHub Copilot (gpt-4o)
  ```

- **Append `+ manual changes` to the signature** when the commit contains a
  mix of AI-generated and human-authored edits:

  ```
  AI: GitHub Copilot (gpt-4o) + manual changes
  ```

- **Use `AI: GitHub Copilot (<model>)` as the exact format.** Replace
  `<model>` with the specific model identifier reported by the agent at the
  time of the commit (e.g. `gpt-4o`, `claude-sonnet-4-5`).

#### DO NOT

- **Do not omit the signature** on any commit that is driven by an AI agent,
  even for trivial one-line fixes.

- **Do not add the signature to commits made entirely by a human** without
  AI assistance.
