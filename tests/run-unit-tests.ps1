<#
.SYNOPSIS
    Run the fast, cross-platform Mission Planner regression unit tests.

.DESCRIPTION
    These tests reference only the netstandard2.0 logic libraries (no WinForms),
    so they build and run anywhere the .NET 8 SDK is installed. They are offline:
    the "Network" category (live downloads in the legacy test project) is excluded.

    On Windows the project also multi-targets net472 for Visual Studio Test
    Explorer; this script pins net8.0 for a consistent, fast command-line run.

.EXAMPLE
    pwsh tests/run-unit-tests.ps1
#>
param([Parameter(ValueFromRemainingArguments = $true)] $ExtraArgs)

$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

$project = "MissionPlannerTests.Unit/MissionPlannerTests.Unit.csproj"

dotnet test $project `
  -f net8.0 `
  -c Debug `
  --filter "TestCategory=Unit" `
  --logger "trx;LogFileName=unit.trx" `
  --results-directory "TestResults" `
  @ExtraArgs
