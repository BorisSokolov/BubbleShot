<#
.SYNOPSIS
    Executes the 10,000-shot headless fuzz simulation across deterministic seeds.
.DESCRIPTION
    Runs the automated headless fuzzing suite targeting BubbleShot.Core.
#>
param(
    [string]$SolutionPath = (Get-Item -Path $PSScriptRoot\..\BubbleShot.sln).FullName
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  BubbleShot 10,000-Shot Fuzz Simulation " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

dotnet test $SolutionPath --filter "FullyQualifiedName~HeadlessFuzzSimulationTests" --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[PASS] All 10,000 fuzz simulation shots executed with 0 invariant breaches!" -ForegroundColor Green
} else {
    Write-Error "`n[FAIL] Fuzz simulation encountered invariant failures."
    exit $LASTEXITCODE
}
