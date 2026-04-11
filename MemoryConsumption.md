# Aeonpulse Memory Consumption Report

> **Date:** 2026-04-11
> **App version / commit:** `346b9f4` — `perf/fix: image resize to 96x96, dead asset removal, tint warm-up, file log sink`
> **Build configuration:** Debug (Release strips all `[Conditional("DEBUG")]` instrumentation — zero overhead)

---

## Table of Contents

1. [Instrumentation Overview](#1-instrumentation-overview)
2. [Windows Desktop Report](#2-windows-desktop-report)
3. [Android Emulator Report](#3-android-emulator-report)
4. [Windows vs Android Comparison](#4-windows-vs-android-comparison)
5. [Why the Android Emulator Needs 4 GB RAM](#5-why-the-android-emulator-needs-4-gb-ram)
6. [How to Repeat This Analysis](#6-how-to-repeat-this-analysis)

---

## 1. Instrumentation Overview

Memory snapshots are emitted by `MemSnapshot.Emit(label)` in `MauiProgram.cs`
(`#if DEBUG`). Each call emits a group of `[MEM]`-category `AeonLog.Info` lines,
one per block tag, so individual dimensions can be filtered independently.

### Snapshot points

| Label | Location | Trigger |
|---|---|---|
| `POST_WARM` | `SplashPage.RunStartupAsync` (Windows only) | Immediately after `WarmAllTintCachesAsync` completes |
| `MAIN_READY` | `MainPage.OnAppearing` | End of `OnAppearing`, after initial calculation pass |
| `T30` | Background `Task.Run` in `MainPage.OnAppearing` | 30 s after `OnAppearing` fires |
| `T120` | Same background chain | 120 s after `OnAppearing` (T30 + 90 s) |

### Block tags per snapshot

| Block | Platforms | What is measured |
|---|---|---|
| `HEAP` | All | `GC.GetTotalMemory(false)` — managed heap without forcing a collection |
| `GC` | All | `GC.CollectionCount(0/1/2)` — cumulative GC collections per generation |
| `PROCESS` | All | `Environment.WorkingSet` — OS private working set bytes |
| `TINT_CACHE` | Windows only | `MauiProgram.TintCacheCount` — Win2D `WriteableBitmap` pool entry count |
| `NATIVE_HEAP` | Android only | `Android.OS.Debug.NativeHeapAllocatedSize` / `NativeHeapSize` |
| `PSS` | Android only | `Android.OS.Debug.MemoryInfo` — total PSS, private dirty, private clean |

### Log format

```
HH:mm:ss.fff inf [Aeonpulse] [MEM] [LABEL] snapshot  wall=HH:mm:ss.fff
HH:mm:ss.fff inf [Aeonpulse] [MEM] [LABEL] [HEAP] managed_heap_MB=X  heap_bytes=X
HH:mm:ss.fff inf [Aeonpulse] [MEM] [LABEL] [GC] gen0=X  gen1=X  gen2=X
HH:mm:ss.fff inf [Aeonpulse] [MEM] [LABEL] [PROCESS] working_set_MB=X  working_set_bytes=X
HH:mm:ss.fff inf [Aeonpulse] [MEM] [LABEL] [TINT_CACHE] tint_cache_entries=X     (Windows)
HH:mm:ss.fff inf [Aeonpulse] [MEM] [LABEL] [NATIVE_HEAP] native_alloc_MB=X  native_size_MB=X  (Android)
HH:mm:ss.fff inf [Aeonpulse] [MEM] [LABEL] [PSS] pss_MB=X  private_dirty_MB=X  private_clean_MB=X  (Android)
```

---

## 2. Windows Desktop Report

### Environment

| Item | Value |
|---|---|
| Platform | Windows 10 x64 (unpackaged, `WindowsPackageType=None`) |
| Runtime | WinUI3 / .NET MAUI 9 |
| Target framework | `net9.0-windows10.0.19041.0` |
| Tint pipeline | Win2D `CanvasDevice` + `WriteableBitmap` pixel-buffer pool |
| Log sink | `AEONPULSE_LOG=1` ? `%TEMP%\aeonpulse_debug.log` |

### Raw snapshot data

| Field | POST_WARM (`21:48:41`) | MAIN_READY (`21:48:43`) | T30 (`21:49:13`) |
|---|---|---|---|
| **Managed heap** | **5.82 MB** | **13.29 MB** | **21.11 MB** |
| Heap bytes | 6 098 280 | 13 938 800 | 22 133 832 |
| Gen0 collections | 2 | 32 | 113 |
| Gen1 collections | 2 | 11 | 28 |
| Gen2 collections | 2 | 5 | 13 |
| **OS working set** | **237.95 MB** | **275.85 MB** | **376.15 MB** |
| Tint cache entries | 34 | 34 | 34 |

? **MAIN_READY ? POST_WARM:** heap +7.47 MB, working set +37.9 MB — cost of
constructing and rendering `MainPage` with all ticker cards plus the initial
calculation pass.

? **T30 ? MAIN_READY:** heap +7.82 MB, working set +100.3 MB — 30 seconds of
live-ticker operation (1 Hz timer, `VibrantCosmos` at ~5 Hz, GIF animation frame
decoding, ambient sparks canvas).

### Tint cache pixel-buffer footprint (computed from measured dimensions)

34 `WriteableBitmap` entries, each holding a raw BGRA32 pixel buffer:

| Group | Count | Dimensions | Per-entry | Subtotal |
|---|---|---|---|---|
| Icons (96×96) | 13 | 96×96 | 36 KB | 468 KB |
| Landmark: `07_Stonehenge.png` | 1 | 980×488 | 1 868 KB | 1 868 KB |
| Landmark: `15_parthenon.png` | 1 | 980×688 | 2 634 KB | 2 634 KB |
| Landmark: `14_hollywood-sign.png` | 1 | 765×115 | 344 KB | 344 KB |
| Landmarks (256×256) | 13 | 256×256 | 256 KB | 3 328 KB |
| Landmarks (256×107–256×244) | 8 | varies | ~130–244 KB | ~1 091 KB |
| **Total tint cache** | **34** | | | **? 10.1 MB** |

The two oversized landmark images (`07_Stonehenge` 980×488, `15_parthenon` 980×688)
account for 4.5 MB — 45% of the tint cache. Resizing them to 256×N would save ~4.5 MB.

### Working set attribution at T30 (~376 MB)

| Layer | Estimated share |
|---|---|
| .NET 9 CLR runtime + JIT compiled code | ~80 MB |
| WinUI3 compositor + XAML visual tree | ~100 MB |
| Win2D tint cache (`WriteableBitmap` BGRA32 buffers) | ~10 MB |
| GIF animation decoded frames (WinUI compositor) | ~50–80 MB (native, 6 GIFs) |
| `MainViewModel` + `TickerResult` objects + string pools | ~21 MB (managed heap) |
| `LocalizedResources` + `AppResources` (490 keys × 2 languages) | ~2 MB |
| MAUI handler peers, resource dictionaries | ~30 MB |
| OS-mapped shared libraries (WinRT, DLLs) | ~30 MB |

### GC behaviour at T30

- **Gen0: 113 collections in 30 s** — driven by the 1 Hz timer
  allocating new `TickerResult` objects every second
- **Gen2: 13 collections in 30 s** — driven by `VibrantCosmos` at ~5 Hz
  promoting short-lived objects before they can be collected in gen0
- Gen2 collections on Windows are notably higher than on Android at the same
  interval — the CLR's generational GC promotes objects more conservatively
  than ART's mark-compact collector

---

## 3. Android Emulator Report

### Environment

| Item | Value |
|---|---|
| Emulator model | `sdk_gphone64_x86_64` |
| Android version | 15 (API 35) |
| CPU ABI | `x86_64` |
| Emulator RAM | 4 014 388 KB (? 3 920 MB usable) |
| Emulator swap | 3 010 784 KB (? 2 941 MB) |
| Tint pipeline | `PorterDuff.SrcIn` colour filter on native `ImageView` — no bitmap cache |
| Log sink | `adb logcat -s Aeonpulse:V` (`AndroidLogcatLoggerProvider`, always on in Debug) |

> **Note:** Android skips the `POST_WARM` snapshot — `WarmAllTintCachesAsync` is
> a Windows-only code path. The first snapshot is `MAIN_READY`.

### Raw snapshot data

| Field | MAIN_READY (`19:27:39`, T+3 s) | T30 (`19:28:09`, T+33 s) | T120 (`19:29:39`, T+123 s) |
|---|---|---|---|
| **Managed heap** | **12.82 MB** | **21.87 MB** | **44.97 MB** |
| Heap bytes | 13 442 800 | 22 928 840 | 47 154 960 |
| Gen0 collections | 16 | 24 | 41 |
| Gen1 collections | 0 | 1 | 2 |
| Gen2 collections | 0 | 1 | 2 |
| **OS working set** | **206.29 MB** | **331.71 MB** | **376.80 MB** |
| **Native heap alloc** | **37.44 MB** | **103.45 MB** | **120.60 MB** |
| Native heap total | 43.94 MB | 116.39 MB | 135.03 MB |
| **PSS total** | **137.19 MB** | **249.32 MB** | **294.33 MB** |
| PSS private dirty | 78.57 MB | 158.83 MB | 203.81 MB |
| PSS private clean | 46.10 MB | 70.13 MB | 70.13 MB |

? **T30 ? MAIN_READY:** heap +9.05 MB, native heap +66 MB, PSS +112 MB —
GIF animation frames loaded into Glide's `BitmapPool` as each animated `Image`
becomes visible for the first time.

? **T120 ? T30:** heap +23.1 MB (ART heap target growing), native heap +17 MB
(stable — Glide pool already warm), PSS +45 MB (private dirty pages growing
as ART promotes survivors across gen boundaries).

### ART GC cadence (from system log `m.aeonpulse.app`)

- Runs every **~5 seconds** — matching the `VibrantCosmos` 5 Hz allocation rate
- Each cycle frees **~2.2–2.5 MB** of short-lived `TickerResult` objects
- **Pause times: 0.4–2.3 ms** (concurrent mark-compact — negligible UI jank)
- Heap target grows continuously: 18 MB ? 25 MB over 2 minutes (ART adaptive
  `HeapGrowthLimit`) — ART deliberately expands the target to reduce GC frequency

### Native heap attribution at T120 (~120 MB)

| Component | Estimated share |
|---|---|
| GIF animation decoded frames in Glide `BitmapPool` | ~40–60 MB |
| Skia `SkBitmap` pixel buffers (icon `ImageView` bitmaps) | ~0.5 MB |
| Android View hierarchy native peers (~2 500 MAUI elements) | ~25–35 MB |
| .NET for Android Mono runtime + JNI bridge | ~15–20 MB |
| ART class data + DEX cache | ~10–15 MB |

---

## 4. Windows vs Android Comparison

| Metric | Windows T30 | Android T30 | Android T120 |
|---|---|---|---|
| **Managed heap** | 21.11 MB | 21.87 MB | 44.97 MB |
| **OS working set** | 376.15 MB | 331.71 MB | 376.80 MB |
| **Native/unmanaged alloc** | ~10.1 MB (tint cache, computed) | 103.45 MB | 120.60 MB |
| **PSS** | n/a | 249.32 MB | 294.33 MB |
| Tint cache entries | 34 (Win2D BGRA32 pool) | **0** (no cache, PorterDuff direct) | 0 |
| GC gen0 count | 113 | 24 | 41 |
| GC gen2 count | **13** | 1 | 2 |
| GC pause | ~1–5 ms (CLR) | ~0.4–2.3 ms (ART CMC) | ~0.4–2.3 ms |

### Key differences

1. **Managed heap is nearly identical at T30** (21.11 MB vs 21.87 MB). The C#
   allocation pattern and TickerResult object churn are platform-agnostic.

2. **Windows gen2 count (13) >> Android (1) at T30.** The CLR promotes objects
   more conservatively than ART's mark-compact collector. ART reclaims short-lived
   objects before they reach gen2; the CLR tends to promote them, causing more
   frequent gen2 sweeps.

3. **Android native heap (103 MB at T30) >> Windows tint cache (~10 MB).**
   Android's Skia/Glide/ART JNI layer is far heavier than Win2D. Glide loads
   all GIF frames into native `BitmapPool` bitmaps eagerly; Win2D defers frame
   decoding to the WinUI3 compositor.

4. **Windows working set (376 MB) ? Android working set (376 MB at T120).**
   They converge at steady state, but the composition differs: Windows is
   dominated by WinUI3 compositor + Win2D; Android is dominated by ART heap
   + Skia/Glide bitmaps.

5. **No tint cache on Android** — by design. `PorterDuff.SrcIn` applies the
   colour filter directly on the native `ImageView` at render time. This
   eliminates the 10.1 MB Windows pre-warmed pixel-buffer pool entirely and
   removes the splash-screen warm-up delay on Android.

6. **Android PSS (294 MB) < Windows working set (376 MB)** — PSS counts shared
   library pages proportionally (shared with other processes), making it the
   fairer measure of Android's true RAM footprint. Windows RSS includes all
   mapped pages without sharing credit.

---

## 5. Why the Android Emulator Needs 4 GB RAM

The app's own memory use (PSS ~294 MB, RSS ~589 MB) is only a fraction of the
emulator's total RAM budget. The remaining RAM is consumed by the full Android
OS running inside the emulator. Measured live from a running 4 GB emulator:

### RAM budget breakdown (4 014 388 KB ? 3 920 MB usable)

| Consumer | RSS | Notes |
|---|---|---|
| **`com.aeonpulse.app`** | **589 MB** | Our app |
| `system_server` | 223 MB | Activity Manager, Package Manager, Window Manager, etc. |
| `com.google.android.gms.persistent` | 173 MB | Google Play Services — always present on Google APIs image |
| `com.google.android.googlequicksearchbox:search` | 171 MB | Google Search / Assistant |
| `com.android.systemui` | 151 MB | Status bar, notification shade, lock screen |
| `com.google.android.apps.messaging` | 132 MB | |
| `com.android.vending:background` | 120 MB | Play Store background |
| `com.google.android.apps.nexuslauncher` | 118 MB | Home screen / launcher |
| `com.google.android.gms` | 115 MB | Google Play Services (second process) |
| `com.google.android.as` + interactor | 186 MB | Android System Intelligence |
| `com.android.inputmethod.latin` | 82 MB | On-screen keyboard |
| 20+ other system processes | ~760 MB | Settings, Bluetooth, Network, WebView sandbox, etc. |
| **All non-app processes subtotal** | **~2 671 MB** | Measured live with `adb shell ps -A -o RSS` |
| Kernel structures (Slab + PageTables + KernelStack + Vmalloc) | ~329 MB | Non-reclaimable |
| Page cache (file-backed, reclaimable under pressure) | ~1 377 MB | DEX files, SO libraries, assets |
| **Total accounted** | **~3 967 MB** | Against 3 920 MB usable |

### What happens with less RAM

| Emulator RAM | Consequence |
|---|---|
| **2 GB** | OS baseline alone (~2.7 GB) exceeds available RAM. LMK kills the app at launch. |
| **3 GB** | Marginal. LMK evicts background apps constantly; app is killed after backgrounding. |
| **4 GB** | Stable. ~1 200 MB reported free, 573 MB swap used. LMK stays dormant during normal use. |
| **8 GB** | Ideal for development with Visual Studio debugger attached (~200–400 MB additional host overhead). |

### Why app RSS (589 MB) > PSS (294 MB)

RSS counts every mapped page — including shared library pages that are shared
with `system_server`, GMS, and other processes — at full size. PSS divides shared
pages proportionally across all processes that map them. PSS is the true RAM
cost; RSS overstates it by ~2×. The 294 MB PSS is the actual pressure the app
places on system RAM.

---

## 6. How to Repeat This Analysis

### Prerequisites

- Debug build on target platform (Release strips all instrumentation)
- Windows: `AEONPULSE_LOG=1` env var set before launch
- Android: emulator running, ADB on PATH (see Agents.md §8.4)

### Windows — capture snapshots

```powershell
# From the repo root
$env:AEONPULSE_LOG = "1"
Remove-Item "$env:TEMP\aeonpulse_debug.log" -ErrorAction SilentlyContinue

# Build (if needed)
dotnet build Aeonpulse.csproj -f net9.0-windows10.0.19041.0

# Launch and let run for at least 35 s (T30 fires at ~33 s from OnAppearing)
$proc = Start-Process "bin\Debug\net9.0-windows10.0.19041.0\win10-x64\Aeonpulse.exe" -PassThru
Start-Sleep 37
$proc | Stop-Process -Force

# Unset env var
$env:AEONPULSE_LOG = $null

# Read MEM snapshots
Get-Content "$env:TEMP\aeonpulse_debug.log" | Select-String "\[MEM\]"
```

Expected output — three snapshot groups:

```
HH:mm:ss.fff inf [Aeonpulse] [MEM] [POST_WARM] snapshot  wall=...
HH:mm:ss.fff inf [Aeonpulse] [MEM] [POST_WARM] [HEAP] managed_heap_MB=...
HH:mm:ss.fff inf [Aeonpulse] [MEM] [POST_WARM] [GC] gen0=...
HH:mm:ss.fff inf [Aeonpulse] [MEM] [POST_WARM] [PROCESS] working_set_MB=...
HH:mm:ss.fff inf [Aeonpulse] [MEM] [POST_WARM] [TINT_CACHE] tint_cache_entries=...
... (MAIN_READY group) ...
... (T30 group) ...
```

For the T120 snapshot extend the wait to 130 s:

```powershell
Start-Sleep 130
```

### Android — capture snapshots

```powershell
$adb = "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe"

# Build and deploy
dotnet build Aeonpulse.csproj -f net9.0-android -t:Install

# Enlarge logcat buffer, clear, launch
& $adb logcat -G 32M
& $adb logcat -c
& $adb shell am start -n "com.aeonpulse.app/crc647f95340f555b5d42.MainActivity"

# Stream Aeonpulse-tagged output to file for 140 s (captures MAIN_READY + T30 + T120)
$logFile = "$env:TEMP\android_aeonpulse.log"
$job = Start-Job -ScriptBlock {
    param($adb, $lf)
    & $adb logcat -s "Aeonpulse:V" -v threadtime 2>&1 | Out-File -FilePath $lf -Encoding utf8
} -ArgumentList $adb, $logFile
Start-Sleep 140
Stop-Job $job; Remove-Job $job

# Read MEM snapshots
Get-Content $logFile | Where-Object { $_ -match "\[MEM\]" }
```

Expected output — three snapshot groups (no `POST_WARM` on Android):

```
... I Aeonpulse: [MEM] [MAIN_READY] snapshot  wall=...
... I Aeonpulse: [MEM] [MAIN_READY] [HEAP] managed_heap_MB=...
... I Aeonpulse: [MEM] [MAIN_READY] [GC] gen0=...
... I Aeonpulse: [MEM] [MAIN_READY] [PROCESS] working_set_MB=...
... I Aeonpulse: [MEM] [MAIN_READY] [NATIVE_HEAP] native_alloc_MB=...
... I Aeonpulse: [MEM] [MAIN_READY] [PSS] pss_MB=...
... (T30 group) ...
... (T120 group) ...
```

### Android — full OS RAM budget

To reproduce the emulator RAM accounting:

```powershell
$adb = "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe"

# Emulator total RAM and free
& $adb shell "cat /proc/meminfo" | Select-String "MemTotal|MemFree|MemAvailable|SwapTotal|SwapFree"

# All process RSS, sorted descending (gives full system picture)
& $adb shell "ps -A -o PID,RSS,NAME --sort=-rss" | Select-Object -First 30

# Kernel non-process memory
& $adb shell "cat /proc/meminfo" |
    Select-String "Slab:|KernelStack|PageTables|VmallocUsed|Cached:|Buffers:"

# App-specific PSS (most accurate per-app cost)
& $adb shell dumpsys meminfo com.aeonpulse.app
```

### What to record for comparison

Copy the values from each snapshot group into the tables in §2 and §3 of this
document. The key metrics for trend analysis are:

| Metric | Why it matters |
|---|---|
| `managed_heap_MB` at T120 | Detects managed memory leaks — should plateau, not grow unboundedly |
| `native_alloc_MB` at T120 (Android) | Detects native bitmap/GIF decoder leaks |
| `pss_MB` at T120 (Android) | True RAM pressure on the system |
| `working_set_MB` at T30 (Windows) | OS-level cost including compositor and GIF frames |
| Gen2 count at T30 | High gen2 count indicates objects living too long — tuning opportunity |
| ART GC freed per cycle | Stable ~2–2.5 MB/cycle is healthy; a drop indicates allocation rate decrease |
