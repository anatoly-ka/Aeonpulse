# Agent_Recipes.md - How to Extend Guide for AI Agent

## 1. How to Extend

Each recipe below lists every file that must be changed, in the order they should
be changed, with exact code to add derived from the existing patterns in the
codebase. Follow every step. Skipping any step will cause either a build error
or a silent runtime regression (missing string, broken binding, or uncalculated ticker).

After completing any recipe, fetch ``Agent_Architecture.md`` and update the
*Complete File Overview* and *Knowledge Graph Nodes* sections. Then, fetch ``Agents.md``
and update the *[PART E] Knowledge Graph (Node Map)* to reflect the new symbols.

---

### 1.1 Adding a New Collapsible Section

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
  <value>ÐšÐ²Ð°Ð½Ñ‚Ð¾Ð²Ñ‹Ð¹</value>
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
             Add ticker card Border elements here (see the *Adding a New Ticker Card* section). -->
        <VerticalStackLayout Spacing="12"
                             IsVisible="{Binding QuantumExpanded}">

            <!-- ticker cards go here -->

        </VerticalStackLayout>

    </VerticalStackLayout>
</Border>
```

---

#### Recipe Verification Checklist

To successfully complete this extension, you MUST have modified exactly the following files. Do not commit until you verify this list:

* [ ] Resources/AppResources.resx
* [ ] Resources/AppResources.ru.resx
* [ ] ViewModels/LocalizedResources.cs
* [ ] ViewModels/MainViewModel.cs
* [ ] Views/MainPage.xaml

---

#### Verification

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

At runtime: the new section header appears, taps correctly toggle expansion.

---

### 1.2 Adding a New Ticker Card

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
Do **not** duplicate `BriefText`/`FullText` - those are inherited from `TickerData`.

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
                   Text="ðŸŒ™"
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

#### Recipe Verification Checklist

To successfully complete this extension, you MUST have modified exactly the following files. Do not commit until you verify this list:

* [ ] Models/TickerResults.cs _(Define the new subclass here)_
* [ ] Resources/AppResources.resx
* [ ] Resources/AppResources.ru.resx
* [ ] ViewModels/LocalizedResources.cs
* [ ] Services/CalculationService.cs
* [ ] ViewModels/MainViewModel.cs
* [ ] Views/MainPage.xaml
* [ ] Views/MainPage.xaml.cs
* [ ] Aeonpulse.Tests _(Must add at least 3 test cases)_

---

#### Verification

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

At runtime: ticker card appears in the section, BriefText is visible immediately,
FullText appears on expand, info button opens `DeepDivePopup`, refresh button
shows `RefreshingPopup` and recalculates.

---

### 1.3 Adding a New Colour Scheme

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

#### Recipe Verification Checklist

To successfully complete this extension, you MUST have modified exactly the following files. Do not commit until you verify this list:

* [ ] Resources/AppResources.resx
* [ ] Resources/AppResources.ru.resx
* [ ] ViewModels/LocalizedResources.cs
* [ ] Services/ThemeService.cs
* [ ] ViewModels/MainViewModel.cs
* [ ] Views/SettingsPopup.xaml
* [ ] Views/SettingsPopup.xaml.cs

---

#### Verification

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
```

At runtime: new radio button appears in Settings. Selecting it immediately repaints
the UI. The choice persists across restarts.

---

### 1.4 Adding a New Language

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

#### Recipe Verification Checklist

To successfully complete this extension, you MUST have modified exactly the following files. Do not commit until you verify this list:

* [ ] Resources/AppResources.resx
* [ ] Resources/AppResources.ru.resx
* [ ] Resources/AppResources._culture__.resx _(new file for added language)_
* [ ] ViewModels/LocalizedResources.cs
* [ ] ViewModels/MainViewModel.cs
* [ ] Views/SettingsPopup.xaml
* [ ] Views/SettingsPopup.xaml.cs
* [ ] Aeonpulse.csproj

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

### 1.5 Adding a New Font Size Preset

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

Follow the same pattern as the *Adding a New Colour Scheme* section steps 4-7:
add `LocalizedResources` passthrough, no ViewModel setter change needed,
add radio row to `SettingsPopup.xaml` in the Text Size group (expand
`RowDefinitions` and `TextSizeLabel Grid.RowSpan`), add seeding in
`SettingsPopup.xaml.cs`.

---

#### Recipe Verification Checklist

To successfully complete this extension, you MUST have modified exactly the following files. Do not commit until you verify this list:

* [ ] Resources/AppResources.resx
* [ ] Resources/AppResources.ru.resx
* [ ] ViewModels/LocalizedResources.cs
* [ ] Services/FontSizeService.cs
* [ ] Views/SettingsPopup.xaml
* [ ] Views/SettingsPopup.xaml.cs

---
