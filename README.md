# Aeonpulse - .NET MAUI Implementation

A mobile dashboard application that tracks life metrics and milestones in a cyberpunk/space aesthetic.

## Project Structure

```
Aeonpulse/
├── Models/                  # Data models
│   ├── TickerData.cs       # Ticker data structure
│   ├── SubsectionState.cs  # Subsection state
│   └── TickerCardModel.cs  # Ticker card model
├── Services/               # Business logic and calculations
│   └── CalculationService.cs
├── ViewModels/             # MVVM ViewModels
│   └── MainViewModel.cs
├── Views/                  # XAML UI pages
│   ├── MainPage.xaml
│   └── MainPage.xaml.cs
├── Converters/             # Value converters for XAML bindings
│   └── ValueConverters.cs
├── Resources/              # App resources
│   └── Styles/
│       ├── Colors.xaml     # Color definitions
│       └── Styles.xaml     # Style definitions
├── App.xaml                # Application entry
├── App.xaml.cs
├── MauiProgram.cs          # App configuration
└── Aeonpulse.csproj        # Project file
```

## Features Implemented

### 1. **Calculation Service** (`Services/CalculationService.cs`)
Complete port of all calculation logic from TypeScript to C#:
- Time Jubilees (next milestone calculations)
- Countdown (live countdown to next anniversary)
- Life Odometer (heartbeats and breaths)
- Alien Anniversaries (Mars and Venus years)
- Galactic Commute (distance traveled through galaxy)
- Photon Path (light-speed travel distance)
- Human Birth Rank (estimated birth order)
- Birth Rune (Viking rune based on date)
- Personal Year (numerology calculations)
- Global Exhale (CO2 emissions since date)
- Tease Text generation

### 2. **Main ViewModel** (`ViewModels/MainViewModel.cs`)
Implements MVVM pattern with:
- Property change notifications (INotifyPropertyChanged)
- Automatic live updates every second using Timer
- Commands for UI interactions
- State management for all subsections and ticker cards
- Data binding for all ticker data

### 3. **UI Implementation** (`Views/MainPage.xaml`)
XAML-based UI featuring:
- Top navigation bar with logo and menu
- Timeline heading (clickable to change date)
- Scrollable ticker sections (Lab, Cosmos, Mirror, Eco-Echoes)
- Collapsible subsections
- Expandable ticker cards
- Live indicators for real-time tickers
- Cyberpunk color scheme (neon cyan, purple, pink)
- Dark theme optimized for mobile

### 4. **Styling** (`Resources/Styles/`)
- **Colors.xaml**: Cyberpunk color palette
  - Space Dark (#0A0E27)
  - Cyber Cyan (#00E5FF)
  - Cyber Purple (#BD93F9)
  - Cyber Pink (#FF79C6)
  - Neon Green (#50FA7B)
- **Styles.xaml**: Reusable XAML styles for consistency

### 5. **Value Converters** (`Converters/ValueConverters.cs`)
- BoolToVisibilityConverter: Show/hide UI elements
- InverseBoolConverter: Invert boolean values for bindings

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 with .NET MAUI workload, OR
- Visual Studio Code with C# Dev Kit extension
- Target platform SDKs:
  - Android: Android SDK (API 21+)
  - iOS: Xcode 14+ (macOS only)
  - Windows: Windows 10 SDK (10.0.19041.0+)

### Building the Project

1. **Clone and navigate to project**:
   ```bash
   cd MAUI
   ```

2. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

4. **Run on Android**:
   ```bash
   dotnet build -t:Run -f net8.0-android
   ```

5. **Run on iOS** (macOS only):
   ```bash
   dotnet build -t:Run -f net8.0-ios
   ```

6. **Run on Windows**:
   ```bash
   dotnet build -t:Run -f net8.0-windows10.0.19041.0
   ```

### Visual Studio

1. Open `Aeonpulse.csproj` in Visual Studio 2022
2. Select your target platform (Android, iOS, or Windows)
3. Press F5 to build and run

## Architecture

### MVVM Pattern
The application follows the Model-View-ViewModel (MVVM) architectural pattern:

- **Models**: Plain C# classes representing data structures
- **Views**: XAML files defining the UI
- **ViewModels**: Business logic and data binding intermediary

### Data Binding
All UI elements are bound to ViewModel properties using XAML data binding:
```xml
<Label Text="{Binding Countdown.BriefText}" />
```

### Dependency Injection
Services are registered in `MauiProgram.cs` and injected where needed:
```csharp
builder.Services.AddSingleton<CalculationService>();
builder.Services.AddTransient<MainViewModel>();
```

### Live Updates
A System.Timers.Timer in the ViewModel updates live calculations every second:
- Countdown
- Life Odometer
- Galactic Commute
- Photon Path

## Customization

### Changing Default Date
Edit in `MainViewModel.cs`:
```csharp
private DateTime _baseDate = new DateTime(1965, 7, 24);
private string _baseDateName = "I was born";
```

### Modifying Colors
Edit `Resources/Styles/Colors.xaml`:
```xml
<Color x:Key="CyberCyan">#00E5FF</Color>
<Color x:Key="CyberPurple">#BD93F9</Color>
```

### Adjusting Calculations
All calculation logic is in `Services/CalculationService.cs`. Each calculation method is independent and can be modified without affecting others.

## Key Differences from React Version

1. **State Management**: Uses INotifyPropertyChanged instead of React hooks
2. **Styling**: XAML styles and resources instead of CSS/Tailwind
3. **Timer**: System.Timers.Timer instead of JavaScript setInterval
4. **Navigation**: Native MAUI navigation instead of React Router
5. **Icons**: Emoji characters instead of lucide-react (can be replaced with icon fonts)

## Future Enhancements

- [ ] Add full popup implementations (Settings, Deep Dive, Share, etc.)
- [ ] Implement calendar sync functionality
- [ ] Add data persistence (save/load dates)
- [ ] Replace emoji icons with proper icon font (e.g., FontAwesome)
- [ ] Add animations and transitions
- [ ] Implement proper date picker dialog
- [ ] Add localization support
- [ ] Implement sharing functionality
- [ ] Add unit tests for calculations

## Performance Considerations

- The ViewModel uses a single timer for all live updates to minimize resource usage
- Static calculations are only updated when the base date changes
- UI updates use data binding to minimize manual UI manipulation
- Large lists use CollectionView with data virtualization (not needed yet with current ticker count)

## Troubleshooting

### Build Errors
- Ensure .NET 8.0 SDK is installed
- Verify MAUI workload is installed: `dotnet workload install maui`
- Clean and rebuild: `dotnet clean && dotnet build`

### Timer Not Updating
- Check that the timer is started in the ViewModel constructor
- Verify that PropertyChanged events are firing

### UI Not Responding to Data Changes
- Ensure INotifyPropertyChanged is implemented correctly
- Check that OnPropertyChanged() is called in property setters
- Verify data binding paths in XAML

## License

This is a sample implementation for the Aeonpulse application.

## Credits

- Original design and calculations from the React/TypeScript version
- CO2 data from Global Carbon Budget 2025
- Population data from UN and Population Reference Bureau
- Star data from astronomical databases
