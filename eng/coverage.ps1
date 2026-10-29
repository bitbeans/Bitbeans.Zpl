#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs all tests with code coverage, merges the results and enforces coverage thresholds.

.PARAMETER Configuration
    Build configuration, default Release.

.PARAMETER LineThreshold
    Minimum merged line coverage in percent.

.PARAMETER BranchThreshold
    Minimum merged branch coverage in percent.

.PARAMETER NoBuild
    Skip the build (use after `dotnet build`).
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Release',
    [double] $LineThreshold = 100,
    [double] $BranchThreshold = 100,
    [switch] $NoBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $root 'artifacts'
$results = Join-Path $artifacts 'test-results'
$report = Join-Path $artifacts 'coverage'

Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $results, $report

$testArgs = @(
    'test', (Join-Path $root 'Bitbeans.Zpl.slnx'),
    '--configuration', $Configuration,
    '--collect', 'XPlat Code Coverage',
    '--results-directory', $results,
    '--logger', 'trx'
)
if ($NoBuild) { $testArgs += '--no-build' }
$testArgs += @('--', 'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura')

& dotnet @testArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet test failed with exit code $LASTEXITCODE" }

& dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed' }

& dotnet reportgenerator `
    "-reports:$results/**/coverage.cobertura.xml" `
    "-targetdir:$report" `
    '-reporttypes:Html;Cobertura;JsonSummary;MarkdownSummaryGithub;TextSummary' `
    '-assemblyfilters:+Bitbeans.Zpl*;-*.Tests'
if ($LASTEXITCODE -ne 0) { throw 'reportgenerator failed' }

$summary = Get-Content (Join-Path $report 'Summary.json') -Raw | ConvertFrom-Json
$line = [double] $summary.summary.linecoverage
$branch = [double] $summary.summary.branchcoverage

Get-Content (Join-Path $report 'Summary.txt') | Select-Object -First 20 | Write-Host
Write-Host ''
Write-Host ("Line coverage:   {0,6:N2} % (threshold {1} %)" -f $line, $LineThreshold)
Write-Host ("Branch coverage: {0,6:N2} % (threshold {1} %)" -f $branch, $BranchThreshold)

$failed = $false
if ($line -lt $LineThreshold) { Write-Error "Line coverage $line % is below $LineThreshold %" -ErrorAction Continue; $failed = $true }
if ($branch -lt $BranchThreshold) { Write-Error "Branch coverage $branch % is below $BranchThreshold %" -ErrorAction Continue; $failed = $true }
if ($failed) { exit 1 }
