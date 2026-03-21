# Aeonpulse — Developer Guide

This guide is for humans continuing development of Aeonpulse. It covers how to work with AI effectively, how to debug, how to make manual-only changes, and how to sign and deploy the app on each platform.

> **Before every session:** direct the AI to read `Agents.md` first. That file is the single authoritative source of truth for the codebase. Everything in this guide references it.

---

## Table of Contents

1. [Starting a Session with AI](#1-starting-a-session-with-ai)
2. [Prompt Templates for Common Tasks](#2-prompt-templates-for-common-tasks)
3. [What AI Should Do vs What You Should Do Manually](#3-what-ai-should-do-vs-what-you-should-do-manually)
4. [Debugging with AI and the Logging System](#4-debugging-with-ai-and-the-logging-system)
5. [Viewing Logs Per Platform](#5-viewing-logs-per-platform)
6. [Signing and Deploying](#6-signing-and-deploying)
7. [Running Tests](#7-running-tests)
8. [Maintenance Habits](#8-maintenance-habits)

---

## 1. Starting a Session with AI

### The mandatory first prompt

Every session — no exceptions — begin with:

```
Please read Agents.md fully before making any changes.
It is the authoritative navigation guide for this codebase.
```

This one step prevents the AI from inventing file paths, missing architectural constraints, or duplicating work already documented.

### Why this matters

`Agents.md` contains:
- The complete file inventory with edit guidance (Section 2)
- Every hard architectural constraint (Sections 1.5 and 9)
- Step-by-step extension recipes (Section 7)
- The pre-commit checklist (Section 9.12)
- The commit signature convention (Section 9.13)

An AI that has not read `Agents.md` will produce structurally incorrect changes. One that has read it will implement changes correctly on the first attempt.

### Orientation prompts for a new AI session

If the AI needs context beyond just `Agents.md`, add one of these:

```
Please also read Services/CalculationService.cs — I want to add a new ticker.
```

```
Please also read ViewModels/MainViewModel.cs — I want to understand the settings flow.
```

```
Please also read Resources/AppResources.resx — I want to understand how strings are structured.
```

---

## 2. Prompt Templates for Common Tasks

Copy and adapt these prompts. The more specific you are about the desired outcome, the better the result.

---

### Fix a bug

```
Please read Agents.md first.

Bug: [describe what the user sees]
Expected: [describe what should happen instead]
Relevant file(s): [e.g., Services/CalculationService.cs, ViewModels/MainViewModel.cs]

Please diagnose and fix the issue. Run the build and tests before finishing.
```

**Example — ticker shows stale data after date change:**
```
Please read Agents.md first.

Bug: after changing the base date in ChangeDatePopup, the Time Jubilees ticker still
shows the old date's jubilee until the app is restarted.
Expected: all tickers should update immediately after SaveDate is called.

Please diagnose and fix. Run dotnet test after the fix.
```

---

### Add a new ticker card

```
Please read Agents.md first, specifically Section 7.2 (Adding a New Ticker Card).

Add a new ticker card called "[Name]" in the [Lab / Cosmos / Mirror / Eco Echoes] section.
It should show: [describe what the ticker calculates and displays]
Update type: [Static / Live (every second)]
Has refresh button: [Yes / No]

Please follow the Section 7.2 recipe exactly, including:
- Both .resx files (AppResources.resx and AppResources.ru.resx)
- LocalizedResources.cs
- CalculationService.cs (new Calculate* method)
- MainViewModel.cs
- MainPage.xaml and MainPage.xaml.cs
- A test in Aeonpulse.Tests
- Agents.md updates (Sections 1.3, 2, 5)
Run the build and all tests before finishing.
```

---

### Add a new colour scheme

```
Please read Agents.md first, specifically Section 7.3 (Adding a New Colour Scheme).

Add a new colour scheme called "[Name]".
Colours:
  Background: #...
  Primary accent: #...
  Secondary accent: #...
  [list all 12 colour keys from ThemeService]

Please follow Section 7.3 exactly. Run the build before finishing.
```

---

### Add a new language

```
Please read Agents.md first, specifically Sections 7.4 and 6.11 (Adding a New Language).

Add [language name] ([ISO code, e.g. "de"]) as a new display language.
I will provide the translated strings separately.

Please:
1. Add the .resx file and .csproj entry
2. Add the language constant and ApplyLanguage case in MainViewModel
3. Add the radio button in SettingsPopup.xaml and its handler in SettingsPopup.xaml.cs
4. Update LocalizedResources.cs
5. Update Agents.md (Sections 1.1, 5/Node 7, 7.4)
Leave the translated string values as placeholders — I will fill them in manually.
Run the build before finishing.
```

---

### Add a new collapsible section

```
Please read Agents.md first, specifically Section 7.1 (Adding a New Section).

Add a new collapsible section called "[Name]" after the [Lab / Cosmos / Mirror / Eco Echoes] section.
It will initially contain no ticker cards.

Please follow Section 7.1 exactly, including .resx updates, LocalizedResources,
MainViewModel, MainPage.xaml, and Agents.md. Run the build before finishing.
```

---

### Change a text size or colour scheme default

```
Please read Agents.md first, specifically Sections 9.3 and 9.4.

I want to change the default value of [colour key / font size key].
Current value: [...]
New value: [...]

Please update Colors.xaml and the relevant ThemeService or FontSizeService palette.
Run the build before finishing.
```

---

### Refactor or clean up a specific file

```
Please read Agents.md first, paying special attention to Sections 9 (Guardrails).

Please refactor [filename]. Goal: [describe what you want improved, e.g.
"reduce duplication in the three refresh command lambdas in MainViewModel"].
Constraints: do not change any public API signatures or observable behaviour.
Run the build and all tests before finishing.
```

---

### Update Agents.md after a manual change you made

```
Please read Agents.md first.

I manually made the following change: [describe what you changed and in which file].
Please update Agents.md to reflect this change, following the rules in Section 9.11.
Update the "Last updated" date by running Get-Date first.
```

---

## 3. What AI Should Do vs What You Should Do Manually

Most work in this project should be done by AI. Here is a clear split:

### Let AI handle entirely

- Adding new ticker cards (full 8-step recipe)
- Adding colour schemes, font size presets, languages
- Fixing bugs in `CalculationService.cs` or `MainViewModel.cs`
- Writing and updating unit tests
- All `Agents.md` updates after structural changes
- Build and commit after every change
- Diagnosing issues from log output (paste the log, ask AI to analyse)

### Do manually, then ask AI to update docs

| Task | Why manual | AI follow-up prompt |
|------|-----------|---------------------|
| Writing or editing `.resx` string values | You know the correct wording, tone, and target audience | "Please review the strings I added to AppResources.resx and AppResources.ru.resx for placeholder completeness and update LocalizedResources.cs if any new keys are missing." |
| Translating strings to Russian (or other languages) | Native speaker quality required | "Please check that every key in AppResources.resx has a corresponding key in AppResources.ru.resx. List any missing or empty keys." |
| Choosing colour values for a new scheme | Aesthetic judgment | "I have added the colour values for the new scheme. Please add it to ThemeService.cs, SettingsPopup, and both .resx files following Section 7.3." |
| Writing the DeepDive (methodology + sources) text for a new ticker | Domain knowledge and sourcing | "The deep-dive text for [ticker] is now in AppResources.resx. Please wire it to the info button in MainPage.xaml.cs and update Agents.md." |
| Changing app icon or splash screen images | Design work | "I have replaced Resources/AppIcon/appicon.png. Please confirm no build changes are needed and update README.md if the visual identity section changes." |
| Updating `Info.plist` or `Package.appxmanifest` for new permissions | Platform-specific policy knowledge | "Please check that Info.plist and PrivacyInfo.xcprivacy are consistent with the new [permission] I added." |

---

## 4. Debugging with AI and the Logging System

### How the logging system works

The app uses a structured logging gateway called `AeonLog` in `Services/AeonLog.cs`. It is active only in **Debug builds** — in Release, all log calls are compiled out entirely via `[Conditional("DEBUG")]`.

Every log line follows this format:
```
[CATEGORY] [SUBCATEGORY] message  key=value  key=value
```

For long multi-phase methods, a third `[BLOCK]` tag localises the line within the method:
```
[CALC] [CalculatePhotonPath] [PHASE_LOOKUP] phase=Interstellar ly=6.44
[CALC] [CalculateTimeJubilees] [UNIT_SCAN] unit=Years jubilee=60 daysUntil=107
```

Current category tokens:

| Token | Where it appears |
|-------|-----------------|
| `BOOT` | App startup, preferences restore (`App.xaml.cs`) |
| `VM` | `MainViewModel` — settings changes, date saves, timer health |
| `CALC` | `CalculationService` — all 10 ticker method entries and phase transitions |

### Diagnosing a bug with AI

1. **Run the app in Debug** on the target platform.
2. **Reproduce the issue** — trigger the specific user action.
3. **Capture the log output** (see Section 5 below for per-platform instructions).
4. **Paste the relevant log lines** into the AI chat with this prompt:

```
Please read Agents.md first, specifically Sections 8.1 and 8.5.

I am seeing [describe the symptom].
Here is the log output captured while reproducing it:

[paste log lines here]

Please analyse the log and identify the root cause.
```

### What the log tells you

| Filter | What you learn |
|--------|---------------|
| `grep "[BOOT]"` | What settings were read from Preferences at startup |
| `grep "[VM] [SaveDate]"` | Whether the new date reached the ViewModel and all three fields were set |
| `grep "[VM] [Timer]"` | Whether the 1-second timer is firing and reaching the main thread |
| `grep "[VM] [Language]"` | What culture was set when the language was changed |
| `grep "[CALC] [CalculatePhotonPath]"` | Full phase walk — input, distance, which phase was selected, which star matched |
| `grep "[CALC] [CalculateTimeJubilees] [UNIT_SCAN]"` | All 7 unit candidates and their days-until values |
| `grep "[CALC] [CalculateTimeJubilees] [WINNER]"` | Which unit won the jubilee tournament |

### Common failure modes and log signatures

| Symptom | What to look for in the log |
|---------|-----------------------------|
| Ticker shows stale data after date change | `[VM] [SaveDate]` — check that `out:` line shows the new date |
| Live tickers stop updating | `[VM] [Timer]` — if missing, the timer has stopped; if `isMainThread=False`, marshalling has broken |
| Language change does not update some strings | `[VM] [Language]` — check the `culture=` field; then look for `[CALC]` entries to confirm recalculation ran |
| Wrong jubilee unit shown | `[CALC] [CalculateTimeJubilees] [UNIT_SCAN]` — look at all 7 candidates and the `[WINNER]` line |
| Photon Path shows wrong phase | `[CALC] [CalculatePhotonPath] [PHASE_LOOKUP]` — check which threshold was hit |

---

## 5. Viewing Logs Per Platform

### Windows — Visual Studio Output window

1. Run with **F5** (Debug configuration).
2. Open **View → Output** → select **Debug** from the dropdown.
3. Filter by `[BOOT]`, `[VM]`, or `[CALC]` in the search bar to isolate app output from MAUI framework noise.

**Without Visual Studio** — use Sysinternals DebugView:
1. Download from `https://learn.microsoft.com/sysinternals/downloads/debugview`
2. Run as Administrator, enable **Capture Win32** and **Capture Global Win32**.
3. Run the app. All log output appears in real time.

### Android — adb logcat

Make sure `adb` is on your PATH:
```powershell
$env:PATH += ";C:\Program Files (x86)\Android\android-sdk\platform-tools"
```

Stream all app output by PID:
```
adb logcat --pid=$(adb shell pidof com.aeonpulse.app) -v time
```

Filter to `AeonLog` / `AddDebug()` output:
```
adb logcat -s Microsoft-Maui:D mono-stdout:D -v time
```

Confirm the device or emulator is visible:
```
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" devices -l
```

**Via Visual Studio:**
1. Run with **F5** on an Android target.
2. **View → Other Windows → Android Device Log**.
3. Filter by package name `com.aeonpulse.app`.

### iOS and Mac Catalyst — Xcode Console (macOS only)

1. Open Xcode → **Window → Devices and Simulators**.
2. Select the device or simulator.
3. Click **Open Console** (bottom-left).
4. In the search bar, type `Aeonpulse` and select **Process** in the filter dropdown.

`AeonLog` output and `AddDebug()` entries appear as `NSLog`-routed lines under the process name.

**Alternative — Console.app on macOS:**
1. Open `/Applications/Utilities/Console.app`.
2. Select the device or `My Mac` (for Mac Catalyst).
3. Search for `Aeonpulse`.

---

## 6. Signing and Deploying

### Android

#### Debug (sideload APK to a connected device)

```
dotnet build Aeonpulse.csproj -f net9.0-android -c Debug
adb install bin\Debug\net9.0-android\com.aeonpulse.app-Signed.apk
```

#### Release — Google Play (AAB)

```
dotnet publish Aeonpulse.csproj -f net9.0-android -c Release ^
  -p:AndroidKeyStore=true ^
  -p:AndroidSigningKeyStore=your.keystore ^
  -p:AndroidSigningKeyAlias=your_alias ^
  -p:AndroidSigningKeyPass=your_key_password ^
  -p:AndroidSigningStorePass=your_store_password
```

Output: `bin\Release\net9.0-android\com.aeonpulse.app-Signed.aab`

Upload the `.aab` to the Google Play Console.

#### Release — Sideload APK

```
dotnet publish Aeonpulse.csproj -f net9.0-android -c Release -p:AndroidPackageFormats=apk ^
  -p:AndroidKeyStore=true ^
  -p:AndroidSigningKeyStore=your.keystore ^
  -p:AndroidSigningKeyAlias=your_alias ^
  -p:AndroidSigningKeyPass=your_key_password ^
  -p:AndroidSigningStorePass=your_store_password
```

Output: `bin\Release\net9.0-android\publish\com.aeonpulse.app-Signed.apk`

---

### iOS (requires a Mac with Xcode)

#### Prerequisites

- Xcode installed and accepted license
- Apple Developer account with a valid provisioning profile and signing certificate
- `.NET MAUI iOS workload`: `dotnet workload install maui-ios`

#### Run on simulator (no signing required)

```
dotnet build Aeonpulse.csproj -f net9.0-ios -t:Run \
  -p:_DeviceSpecificBuild=true \
  -p:RuntimeIdentifier=iossimulator-arm64
```

#### Release — App Store IPA

```
dotnet publish Aeonpulse.csproj -f net9.0-ios -c Release \
  -p:ArchiveOnBuild=true \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:CodesignKey="iPhone Distribution: Your Name (TEAMID)" \
  -p:CodesignProvision="Your Provisioning Profile Name"
```

Output: `bin\Release\net9.0-ios\ios-arm64\publish\Aeonpulse.ipa`

Upload via Xcode Organizer or `xcrun altool`.

---

### Mac Catalyst (requires macOS)

#### Run locally

```
dotnet build Aeonpulse.csproj -f net9.0-maccatalyst -t:Run
```

#### Release — Mac App Store PKG

```
dotnet publish Aeonpulse.csproj -f net9.0-maccatalyst -c Release \
  -p:MtouchArch=x86_64 \
  -p:CreatePackage=true \
  -p:CodesignKey="Apple Distribution: Your Name (TEAMID)" \
  -p:CodesignEntitlements=Platforms/MacCatalyst/Entitlements.plist
```

---

### Windows

#### Debug run (unpackaged, no signing needed)

```
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 -c Debug -t:Run
```

Or press **F5** in Visual Studio with the Windows target selected.

#### Release — MSIX for sideload or Store

```
dotnet publish Aeonpulse.csproj -f net9.0-windows10.0.19041.0 -c Release ^
  -p:RuntimeIdentifier=win10-x64 ^
  -p:WindowsPackageType=MSIX
```

For Store submission, the MSIX must be signed with a trusted certificate. The publisher in `Platforms\Windows\Package.appxmanifest` must match your certificate's Subject field exactly. Use `signtool.exe` from the Windows SDK:

```
signtool sign /fd SHA256 /a /f your_cert.pfx /p your_password ^
  bin\Release\net9.0-windows10.0.19041.0\win10-x64\AppPackages\Aeonpulse_x.x.x.x_x64.msix
```

---

## 7. Running Tests

The test suite covers all 10 ticker calculation methods. Tests are in `Aeonpulse.Tests/` and target plain `net9.0` — no MAUI dependency, runs on any CI machine.

```
dotnet test Aeonpulse.Tests\Aeonpulse.Tests.csproj
```

Expected output: **66 tests, 0 failures**.

### When to run tests

- After **any** change to `Services/CalculationService.cs`
- After adding a new calculation method
- Before every commit (it is item 16 in the `Agents.md` Section 9.12 checklist)

### Asking AI to add tests

```
Please read Agents.md first.

I added a new ticker called [Name] with method CalculateXxx in CalculationService.cs.
Please add a test class CalculateXxxTests.cs in Aeonpulse.Tests following the pattern
of the existing test files (e.g. CalculateCountdownTests.cs).
Cover: normal input, boundary conditions, and the metric/imperial toggle if applicable.
Run dotnet test after adding the tests.
```

---

## 8. Maintenance Habits

### After every AI-assisted session

1. Review the diff with `git diff HEAD` before committing.
2. Check that `Agents.md` was updated if any structural change was made (AI should do this automatically, but verify).
3. Run `dotnet test Aeonpulse.Tests\Aeonpulse.Tests.csproj` — 66 tests, 0 failures.
4. Run `dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0 --no-incremental` and confirm only the four known warning codes appear (`CS0618`, `CS8767`, `CS0414`, `XC0022`).
5. Commit with a descriptive message. AI-only commits include the signature trailer: `AI: GitHub Copilot (gpt-4o)`.

### When you edit `.resx` files manually

Always check that:
- Every key added to `AppResources.resx` (English) also exists in `AppResources.ru.resx`.
- Every new key has a matching property in `ViewModels/LocalizedResources.cs` if it needs to be live-switchable via language change.

Then ask AI:
```
Please read Agents.md first.
I manually added [N] new string keys to AppResources.resx and AppResources.ru.resx.
Please check LocalizedResources.cs for any missing passthrough properties and add them.
Run the build after.
```

### When a XAML file loses its BOM (MSB4018 build error)

```powershell
# Find XAML files missing BOM
Get-ChildItem -Path "C:\Dev\Aeonpulse" -Filter "*.xaml" -Recurse |
  Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' } |
  ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    if ($bytes.Length -lt 3 -or $bytes[0] -ne 0xEF -or $bytes[1] -ne 0xBB -or $bytes[2] -ne 0xBF) {
      $_.FullName
    }
  }

# Re-save all with BOM
Get-ChildItem -Path "C:\Dev\Aeonpulse" -Filter "*.xaml" -Recurse |
  Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' } |
  ForEach-Object {
    $content = [System.IO.File]::ReadAllText($_.FullName, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText($_.FullName, $content, [System.Text.Encoding]::UTF8)
  }
```

### Keeping Agents.md current

`Agents.md` is only useful if it accurately reflects the codebase. Any structural change — new file, renamed method, new setting, new string key group — requires an update. AI agents handle this automatically when instructed. For manual changes you make, use:

```
Please read Agents.md first.
I manually made these changes: [describe]
Please update the relevant sections of Agents.md (run Get-Date first to get today's date).
