<#
.SYNOPSIS
    Builds the Android APK for BubbleShot using Unity in batch mode.
.DESCRIPTION
    Executes BubbleShot.Editor.BuildScript.BuildAndroid headlessly.
#>
param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.38f1\Editor\Unity.exe",
    [string]$ProjectPath = (Get-Item -Path $PSScriptRoot\..).FullName
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  BubbleShot Android APK Build Pipeline   " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Project Path: $ProjectPath"
Write-Host "Unity Path:   $UnityPath"

if (-not (Test-Path $UnityPath)) {
    Write-Warning "Unity executable not found at '$UnityPath'. Locating via Unity Hub..."
    $hubEditors = Get-ChildItem "C:\Program Files\Unity\Hub\Editor" -Directory -ErrorAction SilentlyContinue
    if ($hubEditors) {
        $UnityPath = Join-Path $hubEditors[0].FullName "Editor\Unity.exe"
        Write-Host "Found Unity at: $UnityPath" -ForegroundColor Green
    } else {
        Write-Error "No Unity installation found. Please specify -UnityPath."
        exit 1
    }
}

$LogFile = Join-Path $ProjectPath "build-android.log"

$Arguments = @(
    "-quit",
    "-batchmode",
    "-projectPath", "`"$ProjectPath`"",
    "-executeMethod", "BubbleShot.Editor.BuildScript.BuildAndroid",
    "-logFile", "`"$LogFile`""
)

Write-Host "Executing Unity batch build..." -ForegroundColor Yellow
$Process = Start-Process -FilePath $UnityPath -ArgumentList $Arguments -PassThru -Wait

if ($Process.ExitCode -eq 0) {
    Write-Host "[SUCCESS] Android build finished with exit code 0." -ForegroundColor Green
    if (Test-Path "$ProjectPath\Builds\Android\BubbleShot.apk") {
        $apk = Get-Item "$ProjectPath\Builds\Android\BubbleShot.apk"
        Write-Host "APK Generated: $($apk.FullName) ($([math]::Round($apk.Length / 1MB, 2)) MB)" -ForegroundColor Green
    }
} else {
    Write-Error "[FAILURE] Android build failed with exit code $($Process.ExitCode). Inspect log at: $LogFile"
    exit $Process.ExitCode
}
