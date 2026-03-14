# .NET MAUI Implementation Guide for Aeonpulse

## Complete File Overview

This .NET MAUI implementation includes all the necessary components to run the Aeonpulse application on Android, iOS, and Windows platforms.

### Core Application Files

1. **App.xaml / App.xaml.cs** - Application entry point and resource dictionaries
2. **MauiProgram.cs** - Dependency injection and app configuration
3. **Aeonpulse.csproj** - Project configuration and dependencies

### Models (Data Structures)

All models are in `/Models/` directory:

- **TickerData.cs** - Contains BriefText and FullText for each ticker
- **SubsectionState.cs** - Tracks subsection expansion state
- **TickerCardModel.cs** - Complete ticker card data model

### Services (Business Logic)

Located in `/Services/` directory:

- **CalculationService.cs** - Complete port of all calculations from TypeScript
  - All 10 ticker calculations implemented
  - Helper methods for jubilee finding and numerology
  - Tease text generation

### ViewModels (MVVM Pattern)

Located in `/ViewModels/` directory:

- **MainViewModel.cs** - Main application state management
  - Implements INotifyPropertyChanged for data binding
  - Contains all ticker data properties
  - Manages subsection and card expansion states
  - Includes System.Timers.Timer for live updates (1 second interval)
  - Commands for UI interactions

### Views (UI)

Located in `/Views/` directory:

- **MainPage.xaml** - Main UI layout in XAML
  - Top navigation bar
  - Timeline heading
  - Scrollable ticker sections (Lab, Cosmos, Mirror, Eco-Echoes)
  - Collapsible subsections
  - Expandable ticker cards
  
- **MainPage.xaml.cs** - Code-behind for event handlers

### Converters (XAML Helpers)

Located in `/Converters/` directory:

- **ValueConverters.cs** - Contains:
  - BoolToVisibilityConverter
  - InverseBoolConverter

### Resources (Styling)

Located in `/Resources/Styles/` directory:

- **Colors.xaml** - Cyberpunk color palette:
  - Space Dark/Darker backgrounds
  - Cyber Cyan (#00E5FF)
  - Cyber Purple (#BD93F9)
  - Cyber Pink (#FF79C6)
  - Neon Green (#50FA7B)

- **Styles.xaml** - Reusable XAML styles:
  - BaseLabel, TitleLabel, SubtitleLabel
  - CyberButton, IconButton
  - CardFrame

### Platform-Specific Files

**Android** (`/Platforms/Android/`):
- AndroidManifest.xml
- MainActivity.cs

**iOS** (`/Platforms/iOS/`):
- Info.plist
- Program.cs
- AppDelegate.cs

## Key Implementation Details

### 1. Calculations Service

All calculations from the original TypeScript implementation have been ported to C#:

```csharp
// Example: Time Jubilees
public TickerData CalculateTimeJubilees(DateTime baseDate, string baseDateName, string baseDateValue)
{
    // Full implementation matching TypeScript logic
    // Returns TickerData with BriefText and FullText
}
```

### 2. Live Updates

The MainViewModel implements a timer-based live update system:

```csharp
_updateTimer = new System.Timers.Timer(1000); // 1 second
_updateTimer.Elapsed += (s, e) => UpdateLiveCalculations();
_updateTimer.Start();
```

This updates:
- Countdown
- Life Odometer
- Galactic Commute
- Photon Path

### 3. Data Binding

All UI elements are bound to ViewModel properties:

```xml
<Label Text="{Binding Countdown.BriefText}" />
<Label Text="{Binding Countdown.FullText}" IsVisible="{Binding CountdownExpanded}" />
```

### 4. MVVM Commands

Commands enable UI interaction without code-behind:

```csharp
public ICommand ToggleLabCommand { get; }
// In constructor:
ToggleLabCommand = new Command(() => LabExpanded = !LabExpanded);
```

### 5. Responsive Layout

The XAML uses Grid and StackLayout for responsive design:
- MinimumWidthRequest="393" (iPhone 16 width)
- MinimumHeightRequest="852" (iPhone 16 height)
- Scales to larger screens automatically

## How to Extend

### Adding a New Ticker

1. **Add calculation method to CalculationService.cs**:
```csharp
public TickerData CalculateMyNewTicker(DateTime baseDate)
{
    // Your calculation logic
    return new TickerData
    {
        BriefText = "Brief description",
        FullText = "Full detailed description"
    };
}
```

2. **Add properties to MainViewModel.cs**:
```csharp
private TickerData _myNewTicker = new TickerData();
public TickerData MyNewTicker
{
    get => _myNewTicker;
    set { _myNewTicker = value; OnPropertyChanged(); }
}

private bool _myNewTickerExpanded = false;
public bool MyNewTickerExpanded
{
    get => _myNewTickerExpanded;
    set { _myNewTickerExpanded = value; OnPropertyChanged(); }
}
```

3. **Add update call**:
```csharp
// In UpdateStaticCalculations() or UpdateLiveCalculations()
MyNewTicker = _calculationService.CalculateMyNewTicker(BaseDate);
```

4. **Add XAML UI** in MainPage.xaml:
```xml
<Border BackgroundColor="#121628" 
        StrokeThickness="1"
        Stroke="{StaticResource CyberCyan}"
        StrokeShape="RoundRectangle 8"
        Padding="12">
    <VerticalStackLayout Spacing="8">
        <Grid ColumnDefinitions="Auto,*,Auto">
            <Label Grid.Column="0" Text="🎯" FontSize="24" />
            <Label Grid.Column="1" Text="My New Ticker" />
            <!-- Add toggle button -->
        </Grid>
        <Label Text="{Binding MyNewTicker.BriefText}" />
        <Label Text="{Binding MyNewTicker.FullText}" 
               IsVisible="{Binding MyNewTickerExpanded}" />
    </VerticalStackLayout>
</Border>
```

### Adding a New Subsection

1. **Add state property to MainViewModel**:
```csharp
private bool _mySubsectionExpanded = false;
public bool MySubsectionExpanded
{
    get => _mySubsectionExpanded;
    set { _mySubsectionExpanded = value; OnPropertyChanged(); }
}
```

2. **Add command**:
```csharp
public ICommand ToggleMySubsectionCommand { get; }
// In constructor:
ToggleMySubsectionCommand = new Command(() => MySubsectionExpanded = !MySubsectionExpanded);
```

3. **Add XAML**:
```xml
<Border Style="{StaticResource CardFrame}">
    <VerticalStackLayout Spacing="12">
        <Grid ColumnDefinitions="*,Auto">
            <Label Grid.Column="0" Text="My Section" />
            <Button Grid.Column="1" Command="{Binding ToggleMySubsectionCommand}" />
        </Grid>
        <VerticalStackLayout IsVisible="{Binding MySubsectionExpanded}">
            <!-- Your ticker cards here -->
        </VerticalStackLayout>
    </VerticalStackLayout>
</Border>
```

### Customizing Colors

Edit `/Resources/Styles/Colors.xaml`:

```xml
<Color x:Key="CyberCyan">#YOUR_COLOR</Color>
```

All UI elements referencing `{StaticResource CyberCyan}` will update automatically.

### Adding Navigation

To add new pages:

1. **Create new page**:
```csharp
// Views/SettingsPage.xaml
<ContentPage ...>
    <!-- Your UI -->
</ContentPage>
```

2. **Navigate from MainPage**:
```csharp
await Navigation.PushAsync(new SettingsPage());
```

## Building for Production

### Android
```bash
dotnet publish -f net9.0-android -c Release
```
Output: `bin/Release/net9.0-android/publish/`

### iOS (requires Mac)
```bash
dotnet publish -f net9.0-ios -c Release
```

### Windows
```bash
dotnet publish -f net9.0-windows10.0.19041.0 -c Release
```

## Testing

### Unit Testing Calculations

Create a test project:
```bash
dotnet new xunit -n Aeonpulse.Tests
cd Aeonpulse.Tests
dotnet add reference ../Aeonpulse.csproj
```

Example test:
```csharp
[Fact]
public void CalculateCountdown_ReturnsCorrectFormat()
{
    var service = new CalculationService();
    var baseDate = new DateTime(2000, 1, 1);
    var result = service.CalculateCountdown(baseDate);
    
    Assert.NotNull(result);
    Assert.NotEmpty(result.BriefText);
    Assert.NotEmpty(result.FullText);
}
```

## Performance Tips

1. **Minimize timer frequency**: Current 1-second interval is appropriate
2. **Use data binding**: Avoid manual UI updates
3. **Lazy load**: Only calculate visible sections
4. **Cache results**: Store calculated values that don't change
5. **Use CollectionView**: For large lists (if adding more tickers)

## Debugging

### Enable debug logging:
```csharp
// In MauiProgram.cs
#if DEBUG
builder.Logging.AddDebug();
#endif
```

### View logs:
- **Android**: Android Device Monitor or `adb logcat`
- **iOS**: Xcode Output window
- **Windows**: Visual Studio Output window

## Common Issues

### Timer not firing
- Ensure timer is on UI thread or use `MainThread.BeginInvokeOnMainThread()`
- Check that timer is not garbage collected (keep reference in ViewModel)

### Data binding not working
- Verify property names match exactly (case-sensitive)
- Ensure `OnPropertyChanged()` is called
- Check that BindingContext is set

### Layout issues
- Use `MinimumWidthRequest` and `MinimumHeightRequest` for minimum sizes
- Test on multiple screen sizes
- Use `ScrollView` for content that may overflow

## Additional Resources

- [.NET MAUI Documentation](https://docs.microsoft.com/en-us/dotnet/maui/)
- [XAML Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/xaml/)
- [MVVM Pattern](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm)

## Migration Notes from React

| React Concept | MAUI Equivalent |
|---------------|-----------------|
| useState | INotifyPropertyChanged properties |
| useEffect | Constructor initialization + Timer |
| Components | UserControl or ContentView |
| CSS/Tailwind | XAML Styles + ResourceDictionary |
| onClick | Command binding or Clicked event |
| Conditional rendering | IsVisible binding |
| Props | Bindable Properties |
| Context | Dependency Injection |

## Summary

This implementation provides a complete, production-ready .NET MAUI application that:

✅ Implements all calculations from the original TypeScript code
✅ Uses MVVM pattern for clean architecture
✅ Supports Android, iOS, and Windows platforms
✅ Includes live-updating tickers
✅ Features cyberpunk styling matching the original design
✅ Is fully extensible and maintainable

The codebase is structured for easy maintenance and future enhancements while maintaining the original application's functionality and aesthetic.
