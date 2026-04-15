
# Aeonpulse

**Aeonpulse** is a cross-platform .NET 9 MAUI app that transforms a personal date (like a birthday) into a dashboard of 19 live and on-demand "ticker cards". Each card gives a unique, real-time perspective on your journey through time—scientific, astronomical, personal, and ecological.

Runs on Android, iOS, Mac Catalyst, and Windows from a single shared codebase.

---


## Key Features

- Enter any base date (e.g., your birthday, an anniversary, or a historical event)
- Instantly see 19 contextualized "ticker cards"—from heartbeats taken to stars born since your date
- Live tickers update every second; static tickers recalculate on demand
- Four collapsible sections: Lab, Cosmos, Mirror, Eco Echoes
- All settings (units, theme, text size, language) persist across sessions
- Favorites: pin any ticker for instant access
- Modern, accessible UI with dynamic theming and font scaling

## Ticker Cards Overview

Each ticker card shows a brief summary and expands for full detail. Static cards have a refresh button; live cards update every second.

| # | Ticker | Section | What it shows | Updates |
|---|--------|---------|---------------|---------|
| 1 | Time Jubilees | Lab | Next round-number milestone in years, months, weeks, days, hours, minutes, or seconds | On demand |
| 2 | Countdown | Lab | Live HH:MM:SS (or days) until the next calendar anniversary | Every second |
| 3 | Life Odometer | Lab | Estimated heartbeats and breaths taken since the base date | Every second |
| 4 | Cellular Refresh | Lab | Estimated cell generations turned over since the base date, by tissue type | On demand |
| 5 | Alien Anniversaries | Cosmos | Age in Mars years and Venus years | On demand |
| 6 | Galactic Commute | Cosmos | Distance the Solar System has carried you through the Milky Way | Every second |
| 7 | Photon Path | Cosmos | How far a photon of light has travelled since the base date, with named star milestones | Every second |
| 8 | Cosmic Stretch | Cosmos | How much the observable universe has expanded since the base date | Every second |
| 9 | Vibrant Cosmos | Cosmos | Estimated new exoplanets confirmed, stars born, stars collapsed since the base date | Every second |
| 10 | Space Wait | Cosmos | Estimated cumulative wait time humanity has spent queuing for orbital flights | Every second |
| 11 | Human Birth Rank | Mirror | Estimated ordinal birth rank among all humans ever born | On demand |
| 12 | Birth Rune | Mirror | Elder Futhark rune governing the birth date period | On demand |
| 13 | Personal Year | Mirror | Numerological personal year number and its interpretation | On demand |
| 14 | Global Crowd | Mirror | Estimated number of humans born and who have died since the base date | Every second |
| 15 | Life Log | Lab | A structured personal log: time elapsed in every unit, age milestones, and next milestone | On demand |
| 16 | Vibrant Humanity | Mirror | Estimated births and deaths globally since the base date, with sub-statistics | Every second |
| 17 | Global Exhale | Eco Echoes | Estimated CO2 emitted globally since the base date | On demand |
| 18 | Your Breath | Eco Echoes | Estimated CO2 exhaled by the user since the base date | Every second |
| 19 | Vibrant Nature | Eco Echoes | Estimated new species described by science and species driven to extinction since the base date | On demand |

Ticker cards are grouped into four collapsible sections: **Lab**, **Cosmos**, **Mirror**, and **Eco Echoes**. Each card shows a brief summary and expands to reveal full detail. Static cards (marked "On demand") have a refresh button that shows a brief overlay while recalculating.

---


## Getting Started

**Clone the repository:**

```sh
git clone https://github.com/anatoly-ka/Aeonpulse.git
cd Aeonpulse
```

For build, run, and deployment instructions, see [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md).

To install and run the app, follow the platform-specific steps in the guide. The app runs on Windows, Android, iOS, and Mac Catalyst.
## Platforms

| Platform | Minimum OS |
|----------|-----------|
| Android | API 21 (Android 5.0) |
| iOS | 11.0 |
| Mac Catalyst | 13.1 |
| Windows | 10.0.17763.0 (version 1809) |

> Tizen support is prepared in the codebase but disabled by default.

---


## User Settings

All settings persist across sessions via the platform `Preferences` API:

| Setting | Options |
|---------|---------|
| Unit system | Metric / Imperial |
| Colour scheme | Default Dark, High Contrast Dark, High Contrast Light |
| Text size | Small, Normal, Large |
| Display language | System default, English, Russian |

---


## Application Structure

The app has a single persistent page (`MainPage`). All other UI surfaces are modal pages:

```
MainPage
  ├── MainMenuPopup       (hamburger button -> menu)
  ├── ChangeDatePopup     (tap timeline heading or menu -> Change Date)
  ├── SettingsPopup       (menu -> Settings)
  ├── TeasePopup          (tap app logo or app name -> single live stat with copy-to-clipboard)
  ├── DeepDivePopup       (info button on any ticker -> methodology and sources)
  └── RefreshingPopup     (3-second auto-dismissed overlay during manual refresh)
```

### Architecture

The project follows **manual MVVM** - no MVVM toolkit or code generators:

| Layer | Key Files | Role |
|-------|-----------|------|
| View | `Views/*.xaml` + `Views/*.xaml.cs` | UI structure, data binding, modal navigation only |
| ViewModel | `ViewModels/MainViewModel.cs` | All application state, commands, 1-second live-update timer |
| Service | `Services/CalculationService.cs` | All 19 ticker calculation methods, stateless and thread-safe |
| Service | `Services/ThemeService.cs` | Applies colour palette to `Application.Current.Resources` at runtime |
| Service | `Services/FontSizeService.cs` | Applies font-size preset to `Application.Current.Resources` at runtime |
| Service | `Services/AeonLog.cs` | Structured debug logging gateway (`[Conditional("DEBUG")]`, zero production overhead) |
| Model | `Models/TickerResults.cs` | 19 typed result classes, one per ticker, each extending `TickerData` |
| Resources | `Resources/AppResources.resx` | All user-visible strings (English master, ~486 keys) |
| Resources | `Resources/AppResources.ru.resx` | Russian translations |
| Drawable | `Views/*Drawable.cs` | 8 `IDrawable` implementations for in-card visualizations (WyrdWeb, BirthRankChart, Enneagram, LifeLogChart, PopulationChart, CarbonBudgetChart, VolumeCube, TaxonomyFlow) |

### Key Conventions

- **All colours** in XAML use `DynamicResource` so theme changes take effect without restart.
- **All font sizes** in XAML use `DynamicResource` for the same reason.
- **All user-visible strings** come from `AppResources` - no hardcoded text anywhere.
- **Platform-specific code** lives exclusively in `Platforms/{Platform}/TintHelper.cs` as `partial` method implementations. No `#if ANDROID` or similar in shared files.
- **No business logic** in code-behind (`.xaml.cs`) files.
- **XAML files** must be saved as UTF-8 with BOM or the build will fail with `MSB4018`.

---


## Project Layout

```
Aeonpulse/
├── Attributes/                 AIContextAttribute (codebase navigation marker)
├── Converters/                 ValueConverters.cs (bool / visibility / image-source)
├── Helpers/                    ImageTint.cs (cross-platform icon tinting via attached property)
├── Models/                     TickerData, TickerResults (19 typed), TickerCardModel, FavoriteTickerItem
├── Platforms/
│   ├── Android/                Entry point, TintHelper (PorterDuff colour filter)
│   ├── iOS/                    Entry point, TintHelper (UIKit TintColor)
│   ├── MacCatalyst/            Entry point, TintHelper (UIKit + float-clamp guard)
│   ├── Windows/                Entry point, TintHelper (Win2D ColorMatrixEffect)
│   └── Tizen/                  Entry point (disabled by default)
├── Resources/
│   ├── AppResources.resx       English master strings (~486 keys)
│   ├── AppResources.ru.resx    Russian translations
│   ├── Images/                 PNG assets compiled into platform asset bundles
│   └── Styles/
│       ├── Colors.xaml         Colour token defaults (overwritten at runtime by ThemeService)
│       └── Styles.xaml         Named XAML styles (BaseLabel, CyberButton, CardFrame, etc.)
├── Services/
│   ├── AeonLog.cs              Structured logging gateway (DEBUG builds only)
│   ├── CalculationService.cs   All 19 ticker calculation methods
│   ├── FontSizeService.cs      Font-size preset applier (singleton)
│   └── ThemeService.cs         Colour scheme applier (singleton)
├── ViewModels/
│   ├── LocalizedResources.cs   Live binding bridge for AppResources strings
│   └── MainViewModel.cs        Central state hub, commands, 1-second timer
├── Views/
│   ├── ChangeDatePopup.xaml    Base date entry popup
│   ├── DeepDivePopup.xaml      Generic ticker info popup (reused for all 19 tickers)
│   ├── MainMenuPopup.xaml      Hamburger menu popup
│   ├── MainPage.xaml           The application's only persistent page
│   ├── RefreshingPopup.xaml    3-second refresh spinner overlay
│   ├── SettingsPopup.xaml      Settings panel (unit system, theme, font, language)
│   ├── TeasePopup.xaml         Single live stat popup with copy-to-clipboard (tap logo/app name)
│   ├── BirthRankChartDrawable.cs   IDrawable - birth history curve for Human Birth Rank expanded view
│   ├── CarbonBudgetChartDrawable.cs IDrawable - 1.5-degree carbon budget chart for Global Exhale expanded view
│   ├── EnneagramDrawable.cs    IDrawable - Pythagorean Enneagram for Personal Year expanded view
│   ├── LifeLogChartDrawable.cs IDrawable - two-ring donut chart for Life Log expanded view
│   ├── PopulationChartDrawable.cs  IDrawable - interactive population chart for Global Crowd expanded view
│   ├── TaxonomyFlowDrawable.cs IDrawable - Sankey-style taxonomy flow for Vibrant Nature expanded view
│   ├── VolumeCubeDrawable.cs   IDrawable - isometric cube visualizer for Your Breath expanded view
│   └── WyrdWebDrawable.cs      IDrawable - Web of Wyrd rune grid for Birth Rune expanded view
├── App.xaml / App.xaml.cs      App bootstrap: applies persisted settings before first frame
├── MauiProgram.cs              Host builder, font registration, image tint mapper, AeonLog init
├── Aeonpulse.csproj            Multi-targeted project file
├── Agents.md                   AI agent navigation guide and router (authoritative development reference)
├── Agent_Architecture.md       The complete file inventory and architectural patterns
├── Agent_Recipes.md            Step-by-step extension recipes and verification checklists
├── Agent_Ops.md                Development, debugging, and ops workflows
└── Aeonpulse.Tests/            xUnit test project (net9.0, no MAUI dependency, 300 tests)
```

---


## Contributing & Technical Reference

For architecture, extension recipes, and all technical details, see the **Agent Documentation Suite** (anchored by `Agents.md`) contains:
- The Prime Directives, Commit Protocol, and Tool-Use Guardrails (in `Agents.md`)
- The complete file inventory and architectural patterns (in `Agent_Architecture.md`)
- Step-by-step extension recipes and verification checklists (in `Agent_Recipes.md`)
- Development, debugging, and ops workflows (in `Agent_Ops.md`).

These files are always up to date and are the authoritative reference for contributors and AI agents.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Maui.Controls` | 9.0.0 | Core MAUI framework |
| `Microsoft.Maui.Controls.Compatibility` | 9.0.0 | Legacy `Frame` compatibility (popup panels) |
| `Microsoft.Extensions.Logging.Debug` | 9.0.0 | Debug log provider wired in `MauiProgram` |
| `Microsoft.Graphics.Win2D` | 1.3.2 | Win2D colour-matrix icon tinting (Windows only) |

Test project additionally uses `xunit 2.9.2` and `coverlet.collector 6.0.2`.
