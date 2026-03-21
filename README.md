# Aeonpulse

A cross-platform mobile and desktop app that transforms a personal date — typically a birthday — into ten richly-contextualised **ticker cards**: live and on-demand views of that moment in time through scientific, astronomical, personal, and ecological lenses.

Built with **.NET 9 MAUI**. Runs on Android, iOS, Mac Catalyst, and Windows from a single shared codebase.

---

## What the App Does

The user provides a **base date** (a label such as "My Birthday" and an ISO date). The app continuously recalculates:

| # | Ticker | What it shows | Updates |
|---|--------|---------------|---------|
| 1 | Time Jubilees | Next round-number milestone in years, months, weeks, days, hours, minutes, or seconds | On demand |
| 2 | Countdown | Live HH:MM:SS (or days) until the next calendar anniversary | Every second |
| 3 | Life Odometer | Estimated heartbeats and breaths taken since the base date | Every second |
| 4 | Alien Anniversaries | Age in Mars years and Venus years | On demand |
| 5 | Galactic Commute | Distance the Solar System has carried you through the Milky Way | Every second |
| 6 | Photon Path | How far a photon of light has travelled since the base date, with named star milestones | Every second |
| 7 | Human Birth Rank | Estimated ordinal birth rank among all humans ever born | On demand |
| 8 | Birth Rune | Elder Futhark rune governing the birth date period | On demand |
| 9 | Personal Year | Numerological personal year number and its interpretation | On demand |
| 10 | Global Exhale | Estimated CO2 emitted globally since the base date | On demand |

Ticker cards are grouped into four collapsible sections: **Lab**, **Cosmos**, **Mirror**, and **Eco Echoes**. Each card shows a brief summary and expands to reveal the full methodology and sources.

---

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
  ├── DeepDivePopup       (info button on any ticker -> methodology and sources)
  └── RefreshingPopup     (3-second auto-dismissed overlay during manual refresh)
```

### Architecture

The project follows **manual MVVM** — no MVVM toolkit or code generators:

| Layer | Key Files | Role |
|-------|-----------|------|
| View | `Views/*.xaml` + `Views/*.xaml.cs` | UI structure, data binding, modal navigation only |
| ViewModel | `ViewModels/MainViewModel.cs` | All application state, commands, 1-second live-update timer |
| Service | `Services/CalculationService.cs` | All ten ticker calculations, stateless and thread-safe |
| Service | `Services/ThemeService.cs` | Applies colour palette to `Application.Current.Resources` at runtime |
| Service | `Services/FontSizeService.cs` | Applies font-size preset to `Application.Current.Resources` at runtime |
| Service | `Services/AeonLog.cs` | Structured debug logging gateway (`[Conditional("DEBUG")]`, zero production overhead) |
| Model | `Models/TickerResults.cs` | Ten typed result classes, one per ticker |
| Resources | `Resources/AppResources.resx` | All user-visible strings (English master) |
| Resources | `Resources/AppResources.ru.resx` | Russian translations |

### Key Conventions

- **All colours** in XAML use `DynamicResource` so theme changes take effect without restart.
- **All font sizes** in XAML use `DynamicResource` for the same reason.
- **All user-visible strings** come from `AppResources` — no hardcoded text anywhere.
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
├── Models/                     TickerData, TickerResults (10 typed), TickerCardModel
├── Platforms/
│   ├── Android/                Entry point, TintHelper (PorterDuff colour filter)
│   ├── iOS/                    Entry point, TintHelper (UIKit TintColor)
│   ├── MacCatalyst/            Entry point, TintHelper (UIKit + float-clamp guard)
│   ├── Windows/                Entry point, TintHelper (Win2D ColorMatrixEffect)
│   └── Tizen/                  Entry point (disabled by default)
├── Resources/
│   ├── AppResources.resx       English master strings (359+ keys)
│   ├── AppResources.ru.resx    Russian translations
│   ├── Images/                 PNG assets compiled into platform asset bundles
│   └── Styles/
│       ├── Colors.xaml         Colour token defaults (overwritten at runtime by ThemeService)
│       └── Styles.xaml         Named XAML styles (BaseLabel, CyberButton, CardFrame, etc.)
├── Services/
│   ├── AeonLog.cs              Structured logging gateway (DEBUG builds only)
│   ├── CalculationService.cs   All 10 ticker calculation methods
│   ├── FontSizeService.cs      Font-size preset applier (singleton)
│   └── ThemeService.cs         Colour scheme applier (singleton)
├── ViewModels/
│   ├── LocalizedResources.cs   Live binding bridge for AppResources strings
│   └── MainViewModel.cs        Central state hub, commands, 1-second timer
├── Views/
│   ├── ChangeDatePopup.xaml    Base date entry popup
│   ├── DeepDivePopup.xaml      Generic ticker info popup (reused for all 10 tickers)
│   ├── MainMenuPopup.xaml      Hamburger menu popup
│   ├── MainPage.xaml           The application's only persistent page
│   ├── RefreshingPopup.xaml    3-second refresh spinner overlay
│   └── SettingsPopup.xaml      Settings panel (unit system, theme, font, language)
├── App.xaml / App.xaml.cs      App bootstrap: applies persisted settings before first frame
├── MauiProgram.cs              Host builder, font registration, image tint mapper, AeonLog init
├── Aeonpulse.csproj            Multi-targeted project file
├── Agents.md                   AI agent navigation guide (authoritative development reference)
└── Aeonpulse.Tests/            xUnit test project (net9.0, no MAUI dependency)
```

---

## Development Reference

**`Agents.md`** in the repository root is the authoritative development reference. It documents every file, every architectural pattern, all extension recipes, build commands, debugging procedures, and a pre-commit checklist. Read it before making any structural change.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Maui.Controls` | 9.0.0 | Core MAUI framework |
| `Microsoft.Maui.Controls.Compatibility` | 9.0.0 | Legacy `Frame` compatibility (popup panels) |
| `Microsoft.Extensions.Logging.Debug` | 9.0.0 | Debug log provider wired in `MauiProgram` |
| `Microsoft.Graphics.Win2D` | 1.3.2 | Win2D colour-matrix icon tinting (Windows only) |

Test project additionally uses `xunit 2.9.2` and `coverlet.collector 6.0.2`.
