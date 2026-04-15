# Agent_Ops.md - Development and Debugging Guide for Aeonpulse

## 1. Development Workflow

---

### 1.1 Prerequisites

| Requirement | Minimum version | Notes |
|-------------|----------------|-------|
| .NET SDK | 9.0.312 | `dotnet --version` to verify |
| Visual Studio | 2022 17.12+ | ".NET Multi-platform App UI development" workload required |
| Android SDK | API 35 | Build tools 35.0.0, OpenJDK 17 |
| Android Emulator RAM | **4096 MB** | Default 1536 MB causes LMK kills; run `scripts/setup-android-avd.ps1` once |
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

#### Android Emulator Setup (run once per machine)

A .NET 9 MAUI app requires at least **4096 MB** of emulator RAM. The Android Low
Memory Killer terminates the app during startup on an emulator configured with the
default 1536 MB because image loading triggers native bitmap allocation that exhausts
the available swap.

Run the setup script after cloning the repo or creating a new AVD:

```
powershell -ExecutionPolicy Bypass -File scripts/setup-android-avd.ps1
```

The script sets the following values in every `config.ini` under
`%USERPROFILE%\.android\avd\` (or `\$ANDROID_AVD_HOME` if set):

| Key | Required value | Default |
|-----|---------------|---------|
| `hw.ramSize` | `4096` | `1536` |
| `vm.heapSize` | `512` | `256` |

Re-running is safe - values already at the required level are left unchanged.
**Restart the emulator after running the script.**

---

### 1.2 Restore

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

### 1.3 Build Commands

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

### 1.4 Known Warnings (Build-Clean - Expected, Do Not Fix)

The following warnings appear in every clean build and are known, accepted, and
not regressions. Do not treat them as failures.

| Code | Count (Windows Debug) | Cause | Status |
|------|-----------------------|-------|--------|
| `XC0022` | ~240 | `{Binding}` expressions in `MainPage.xaml` lack `x:DataType`; compiled binding not enabled | Accepted - runtime binding used intentionally |
| `CS0618` | ~60 | `Frame` (popup XAML), `Application.MainPage` setter (`App.xaml.cs`) deprecated in .NET 9 | Accepted - existing usage, do not add new occurrences |
| `CS8767` | ~48 | Nullability mismatch on `BoolToImageSourceConverter.ConvertBack` parameter | Accepted - minor nullability annotation difference |
| `CS0414` | 6 | `MainPage._isSettingsOpen` assigned but never read (guard flag only written, not checked) | Accepted - intentional guard pattern |

**Rule for agents:** if a build produces only the above warning codes, it is clean.
Any warning code not in this table is a new issue and must be investigated.

---

### 1.5 Testing

The test project `Aeonpulse.Tests` exists at `Aeonpulse.Tests\Aeonpulse.Tests.csproj`.
It targets `net9.0` (plain .NET, no MAUI) and links source files from the main project
directly so no TFM-incompatibility issues arise. Run all tests with:

```
dotnet test Aeonpulse.Tests\Aeonpulse.Tests.csproj
```

**What can be tested without a device (300 tests):**
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

### 1.6 Run and Deploy

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
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" uninstall com.aeonpulse.app
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" install "bin\Debug\net9.0-android\com.aeonpulse.app-Signed.apk"
```
> **Warning - Debug APK requires Fast Deployment:** `libmono-android.debug.so` checks
> for pre-pushed assemblies in `files/.__override__/` and aborts with SIGABRT if that
> folder exists but is empty (stale VS session artefact). Always uninstall cleanly first
> (as shown above).
>
> **Warning - Debug APK aborts if the VS debugger is not listening (timeout=30 s):**
> A VS-deployed Debug APK embeds a `--debugger-agent=...address=10.0.2.2:52503,timeout=30000`
> option in its launch intent. If the app is started via `adb shell am start` (or by
> tapping the icon) while VS is not actively listening on that port, the Mono debug agent
> waits 30 seconds then calls `abort()`. The emulator output looks like:
>
> ```
> [monodroid-debug] Trying to initialize the debugger with options:
>     --debugger-agent=transport=dt_socket,...,address=10.0.2.2:52503,timeout=30000
> [libc] Requested dump for pid <N>
> ```
>
> **Fix:** always launch a VS-deployed Debug APK via **F5 in Visual Studio** so the
> debugger host is already listening. For manual/standalone testing use the Release APK
> (see below) â€” it contains no debug agent and starts instantly from the icon or adb.
>
> **For standalone testing without VS** use a Release build targeting the emulator ABI:
>
> ```
> dotnet publish Aeonpulse.csproj -f net9.0-android -c Release -r android-x64
> "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" uninstall com.aeonpulse.app
> "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" install "bin\Release\net9.0-android\android-x64\com.aeonpulse.app-Signed.apk"
> ```
>
> Do **not** add `EmbedAssembliesIntoApk=true` to the csproj - it breaks VS Fast
> Deployment and makes every Debug build 15-30 s slower with no benefit during normal
> development. Release builds are already self-contained without it.


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

### 1.7 Publish (Release Packages)

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

### 1.8 Clean Build (XAML Encoding Issues)

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

### 1.9 AppResources Designer Regeneration

`Resources\AppResources.Designer.cs` is auto-generated from `AppResources.resx` by
the `PublicResXFileCodeGenerator` on every build. If the designer is out of sync
(e.g., a key is missing after editing the `.resx` manually), force regeneration by:

1. In Visual Studio: right-click `AppResources.resx` â†’ **Run Custom Tool**
2. Or trigger a full rebuild:
   ```
   dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental
   ```

Prefer not to edit `AppResources.Designer.cs` manually - changes will be overwritten on the next `Run Custom Tool` invocation. When Designer regeneration is unavailable (e.g., in a headless agent session), manually add only the missing `public static string` property following the existing pattern, then trigger regeneration on the next full build to reconcile.

---

### 1.10 Source Control

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

### 1.11 Adding a New Language (Build Steps Only)

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

### 1.12 Enabling the Tizen Target

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

## 2. Debugging

---

### 2.1 Logging Infrastructure and `AeonLog` Gateway

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
| `MEM` | Memory snapshots - managed heap, GC counts, OS working set, tint cache size |

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
| `SplashPage.RunStartupAsync` - after tint warm done | `MEM` | Info | `[POST_WARM]` managed heap, GC counts, OS working set, tint cache entry count |
| `MainPage.OnAppearing` - end of method | `MEM` | Info | `[MAIN_READY]` same fields as POST_WARM |
| Background task at T+30 s after `OnAppearing` | `MEM` | Info | `[T30]` same fields; captures steady-state after first 30 s of live-ticker operation |
| Background task at T+120 s after `OnAppearing` | `MEM` | Info | `[T120]` same fields; extended steady-state snapshot for Android (Android GC headroom grows over 2 min) |
| `MemSnapshot.Emit` (Android only) | `MEM` | Info | `[NATIVE_HEAP]` Dalvik/ART native heap allocated + total size via `Android.OS.Debug` |
| `MemSnapshot.Emit` (Android only) | `MEM` | Info | `[PSS]` process PSS, private dirty, private clean via `Android.OS.Debug.MemoryInfo` |

---

### 2.2 Adding Log Calls to Application Code

Use `AeonLog` (see the *Logging Infrastructure and AeonLog Gateway* section) for all application-level diagnostic output.
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

### 2.3 Recommended Debug Instrumentation Points

All instrumentation below is already wired in the codebase. The table in the
*Logging Infrastructure and AeonLog Gateway* section lists every active call
site. The examples below show the exact `AeonLog` call at each location for reference.

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

### 2.4 Viewing Logs by Platform

#### Windows

**Visual Studio Output window (Debug pane):**

1. Run with **F5** (Debug configuration).
2. Open **View â†’ Output** â†’ select **Debug** from the dropdown.
3. `AddDebug()` output and `System.Diagnostics.Debug.WriteLine()` calls appear here in real time.

**Filter by application output:**

In the Output window search bar, type `[Aeonpulse]` or any prefix used in your
log messages to isolate application lines from MAUI framework noise.

**DebugView (Sysinternals) - without Visual Studio:**

1. Download `DebugView` from https://learn.microsoft.com/sysinternals/downloads/debugview
2. Run as Administrator.
3. Enable **Capture â†’ Capture Win32** and **Capture â†’ Capture Global Win32**.
4. Run the app. All `Debug.WriteLine` output appears in the DebugView window.

**File log sink (`AEONPULSE_LOG`) - without Visual Studio or DebugView:**

Set `AEONPULSE_LOG=1` in the environment before launching to redirect all
`Debug`/`Info`/`Warn` output to `%TEMP%\aeonpulse_debug.log`:

```powershell
$env:AEONPULSE_LOG = "1"
& 'bin\Debug\net9.0-windows10.0.19041.0\win10-x64\Aeonpulse.exe'
```

Every line is timestamped to the millisecond. All `[BOOT]`, `[TINT]`,
`[CALC]`, and `[VM]` entries appear in order of emission. The variable is
read once at startup; restart the app to activate or deactivate. This sink is
active only in `DEBUG` builds (`#if DEBUG && WINDOWS` in `MauiProgram.cs`).

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

**Direct logcat sink (`Aeonpulse` tag) - no debugger required:**

On Android, `AddDebug()` only emits to the Mono debugger channel (requires an
attached debugger). A direct `AndroidLogcatLoggerProvider` (`Platforms/Android/`)
is registered in all `DEBUG` Android builds and writes every `AeonLog` entry
to `android.util.Log` under the tag `Aeonpulse`. Filter with:

```
adb logcat -s Aeonpulse:V
```

All `[BOOT]`, `[VM]`, `[CALC]`, `[MEM]` lines appear in real time
without Visual Studio attached.

**Via Visual Studio Android Device Log:**

1. Run the app from Visual Studio with **F5** on an Android target.
2. Open **View â†’ Other Windows â†’ Android Device Log**.
3. Filter by package name `com.aeonpulse.app`.

`AddDebug()` output appears under the `Microsoft-Maui` tag. `Debug.WriteLine()`
output appears under `mono-stdout`.

**Confirm the emulator is running:**

```
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" devices -l
```

#### iOS / Mac Catalyst

**Via Xcode Console (macOS only):**

1. Open Xcode â†’ **Window â†’ Devices and Simulators**.
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

### 2.5 Diagnosing Common Failure Modes

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

### 2.6 Debug Build vs Release Build Differences

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

### 2.7 XAML Hot Reload and Live Visual Tree

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
2. Open **Debug â†’ Windows â†’ Live Visual Tree**.
3. Inspect any element's `ActualWidth`, `ActualHeight`, `IsVisible`, `Opacity`,
   and bound property values in real time.
4. Use **Enable selection in running application** (cursor icon in the toolbar)
   to click a UI element and jump directly to it in the tree.

Useful for diagnosing `topOffset` geometry problems in `DeepDivePopup` and
`MainMenuPopup` - inspect `NavBar.Height` and `TimelineHeading.Height` values
live before the popup is pushed.

---

### 2.8 Attaching the Debugger to a Running Process

#### Android (already deployed)

1. In Visual Studio: **Debug â†’ Attach to Android Processâ€¦**
2. Select the running `com.aeonpulse.app` process from the list.
3. Set breakpoints in any `.cs` file. The debugger maps managed frames correctly
   on both emulator and physical device.

#### Windows (already running)

1. In Visual Studio: **Debug â†’ Attach to Processâ€¦** (Ctrl+Alt+P)
2. Filter for `Aeonpulse.exe`.
3. Select and attach with **Managed (.NET)** debugger type.

---
