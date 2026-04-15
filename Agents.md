# Agents.md - Master Agent Directive (Aeonpulse)

**Last updated:** 2026-04-13
**Maintained by:** AI Agents and human developers collaboratively.
**Rule:** Update this file and the Last Updated date upon each change.

**[CRITICAL] You must abide by the Prime Directives and Tool-Use
Guardrails below before reading any other file or executing any tool.**

---

## [PART A] PRIME DIRECTIVES (Absolute Constraints)

1.  **ARCHITECTURE & STATE (Manual MVVM Only):**

    - **No Code-Behind Logic:** \*.xaml.cs is restricted to modal
      navigation, geometry measurement, and event-to-command wiring.

    - **State:** ALL state lives in MainViewModel. ALL computations live
      in CalculationService.

    - **MVVM:** Use manual INotifyPropertyChanged and
      System.Windows.Input.Command. Do NOT use CommunityToolkit.Mvvm,
      source generators, or DI containers for view models.

2.  **UI & DATA BINDING:**

    - **Strict Resources:** MUST use DynamicResource for all colors and
      fonts (mutated at runtime). MUST use AppResources (via {Binding
      Loc.Xxx}) for all text. No hardcoded strings or hex colors.

    - **Container Rule:** MUST use Border for new containers. Frame is
      obsolete in .NET 9.

    - **Navigation:** Modal push/pop ONLY (Navigation.PushModalAsync).

3.  **ENCODING & MARKUP:**

    - **ASCII-Only Comments:** Non-ASCII characters (emoji, en-dashes)
      in //, ///, or \`\` comments are **FORBIDDEN** and will crash the
      MSBuild task. Non-ASCII is ONLY allowed in element attribute
      values (e.g., Text=\"🚀\").

    - **UTF-8 BOM:** All .xaml and .resx files MUST be saved as UTF-8
      with BOM.

4.  **COMMIT PROTOCOL:**

    - Append your model signature as the last line of the commit
      message: AI: GitHub Copilot (\<model\>). Add + manual changes if a
      human assisted.

    - Always run file-integrity checks (line counts, symbol census)
      before committing.

---

## [PART B] AGENT TOOL-USE GUARDRAILS

**1. Terminal Commands (Pre-Flight Gate)**

Evaluate these conditions before EVERY run_command_in_terminal call:

- **Multiline / Special Chars?** If the command contains &, {, }, (, ),
  newlines, or - at word-start, **STOP**. You MUST write a .ps1 script
  and execute it via -File. Never use -Command \"\...\" for complex
  logic.

- **Empty Output / Parse Error?** If a previous terminal command failed
  or returned empty output when Write-Host was present, the session is
  broken. **STOP**. Send Write-Host \"reset\" and confirm the output is
  reset before attempting real work.

**2. File Editing (Pre-Edit Gate & Routing Table)**

Evaluate before EVERY edit_file call. If in doubt, write a .ps1 or .csx
script to splice content using string.Replace().

| **Target File** | **Tool Rule (Non-Negotiable)** |
|-----------------|--------------------------------|
| Services/CalculationService.cs | **NEVER use edit_file.** Use a .ps1 script with a 3-line unique anchor. Run the Region Census before and after editing to verify no regions were deleted. |
| Views/MainPage.xaml | **NEVER use edit_file for insertions.** Use .ps1. edit_file is allowed ONLY for single-attribute changes anchored by a unique x:Name. |
| .resx and Designer.cs files | **NEVER use edit_file.** Use targeted ReadAllText/Replace/WriteAllText scripts. |
| All other files | edit_file is permitted ONLY if the anchor contains a unique signature/property. NEVER anchor on structural boilerplate (}, #endregion). |

**Post-Edit Verification:** After any edit_file, you MUST verify: (1)
The new symbol is present, (2) It appears the expected number of times,
(3) Pre-existing symbols surrounding it were not deleted. If any fail,
restore via git checkout HEAD \-- \<file\> immediately.

---

## [PART C] DIRECTORY ROUTING INDEX (Agentic Fetching)

**Do NOT read the files below unless your specific task requires it. If
required, use your file-reading tool to fetch them.**

- **IF** your task involves modifying data flow, modal popups,
  understanding ThemeService/FontSizeService, or tracing how
  MainViewModel interacts with XAML:

  - ➡️ **Fetch & Read:** Agent_Architecture.md

- **IF** your task is to add a completely new Ticker Card, Collapsible
  Section, Color Scheme, or Language:

  - ➡️ **Fetch & Read:** Agent_Recipes.md

- **IF** your task involves build errors, publishing, emulator RAM
  setup, or ADB logcat viewing:

  - ➡️ **Fetch & Read:** Agent_Ops.md

---

## [PART D] AI MARKUP SCHEMA

All code modifications MUST be annotated using the following three
systems to maintain architectural context for future agents:

**1. [AIContext] Attribute (C#)**

Place immediately above class or method declarations. Never apply to
private helpers unless they are distinct algorithms.

- *Roles:* AppBootstrap, CoreCalculationEngine, CoreCalculation,
  LiveTicker, NavigationCoordinator, ModalViewController,
  DataTransferObject, UIConverter, PlatformAbstractionHelper.

**2. XAML Comments (\`\`)**

Annotate layout structure, binding sources, and hidden dependencies.

- **Syntax:** \`\`

- **Rule:** Must be purely ASCII. Place immediately above the target
  element.

**3. XML Doc Comments (///)**

Required on ALL public classes and methods.

- **Rule:** Must include a \<para\>\<b\>Side effects / Hidden
  dependencies:\</b\> block to document what the method changes outside
  of its explicit returns.

---

## [PART E] Knowledge Graph (Node Map)

> **Visual Reference:** The dependency tree of the application. For detailed symbol ownership and extension rules, fetch and read `Agent_Architecture.md`.

```text
Platform entry points (Android/iOS/Mac/Windows/Tizen)
    |
    v
MauiProgram.CreateMauiApp()
    |
    v
App (App.xaml + App.xaml.cs)
    |
    +-- ThemeService.Instance
    +-- FontSizeService.Instance
    +-- MainViewModel.ApplyLanguage() [static]
    |
    v
MainPage (Views/MainPage.xaml + .xaml.cs)
    |
    +-- MainViewModel (ViewModels/MainViewModel.cs)
    |       |
    |       +-- CalculationService (Services/CalculationService.cs)
    |       +-- ThemeService.Instance
    |       +-- FontSizeService.Instance
    |       +-- LocalizedResources.Instance (ViewModels/LocalizedResources.cs)
    |               |
    |               +-- AppResources.resx (Resources/AppResources.resx)
    |
    +-- ImageTint (Helpers/ImageTint.cs)
    |       |
    |       +-- Platform TintHelper.cs (Platforms/{Platform}/TintHelper.cs)
    |
    +-- Modal stack popups (all pushed by MainPage.xaml.cs)
            +-- MainMenuPopup
            +-- ChangeDatePopup
            +-- SettingsPopup
            +-- DeepDivePopup
            +-- RefreshingPopup

---
