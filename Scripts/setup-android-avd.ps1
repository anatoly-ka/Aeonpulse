<#
.SYNOPSIS
    One-time Android emulator setup for Aeonpulse development.

.DESCRIPTION
    Patches every AVD config.ini on this machine to ensure the emulator has
    enough RAM to run a .NET 9 MAUI app without being killed by the Android
    Low Memory Killer (LMK).

    Required values set by this script:
        hw.ramSize  = 4096   (MB)  -- minimum for a MAUI app on Android API 35
        vm.heapSize = 512    (MB)  -- Dalvik heap ceiling; default 256 is too low

    Run once after cloning the repo or creating a new AVD.
    Re-running is safe: values are only updated if they differ.

.EXAMPLE
    pwsh -ExecutionPolicy Bypass -File scripts/setup-android-avd.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---- Locate the .android/avd directory -----------------------------------------
$androidHome = $env:ANDROID_AVD_HOME
if (-not $androidHome) {
    $androidHome = Join-Path $env:USERPROFILE ".android\avd"
}
if (-not (Test-Path $androidHome)) {
    Write-Host "No AVD directory found at: $androidHome"
    Write-Host "Create at least one AVD in Android Studio or the AVD Manager first."
    exit 0
}

Write-Host "AVD directory: $androidHome"
Write-Host ""

# ---- Required settings ---------------------------------------------------------
$required = @{
    "hw.ramSize"  = "4096"
    "vm.heapSize" = "512"
}

# ---- Process each AVD ----------------------------------------------------------
$configs = @(Get-ChildItem $androidHome -Recurse -Filter "config.ini" -ErrorAction SilentlyContinue)
if ($configs.Count -eq 0) {
    Write-Host "No config.ini files found under $androidHome - nothing to patch."
    exit 0
}

foreach ($cfg in $configs) {
    $avdName = Split-Path (Split-Path $cfg.FullName) -Leaf
    Write-Host "AVD: $avdName"

    $content = Get-Content $cfg.FullName -Raw
    $changed = $false

    foreach ($key in $required.Keys) {
        $desired = $required[$key]
        $pattern = "(?m)^($([regex]::Escape($key))\s*=\s*)(.+)$"
        if ($content -match $pattern) {
            $current = $matches[2].Trim()
            if ($current -ne $desired) {
                $content = $content -replace $pattern, "`${1}$desired"
                Write-Host "  $key : $current -> $desired"
                $changed = $true
            } else {
                Write-Host "  $key : $current (already correct)"
            }
        } else {
            # Key not present - append it
            $content = $content.TrimEnd() + "`r`n$key = $desired`r`n"
            Write-Host "  $key : (added) $desired"
            $changed = $true
        }
    }

    if ($changed) {
        [System.IO.File]::WriteAllText($cfg.FullName, $content,
            [System.Text.UTF8Encoding]::new($false))
        Write-Host "  -> saved."
    }
    Write-Host ""
}

Write-Host "Done. Restart any running emulator for the new RAM settings to take effect."
