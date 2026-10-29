#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Fails when a NuGet package of the solution is vulnerable or deprecated.
.DESCRIPTION
    Checks direct and transitive packages against the advisory and deprecation data of the configured
    package sources (nuget.org). Used by CI and the weekly scheduled run.
#>
[CmdletBinding()]
param(
    [string] $Solution = (Join-Path $PSScriptRoot '..' 'Bitbeans.Zpl.slnx')
)

$ErrorActionPreference = 'Stop'

function Get-Findings([string] $Kind) {
    $json = dotnet list $Solution package "--$Kind" --include-transitive --format json
    if ($LASTEXITCODE -ne 0) { throw "dotnet list package --$Kind failed with exit code $LASTEXITCODE" }

    $report = $json | ConvertFrom-Json
    foreach ($project in @($report.projects)) {
        foreach ($framework in @($project.frameworks)) {
            foreach ($package in @($framework.topLevelPackages) + @($framework.transitivePackages)) {
                if ($null -eq $package) { continue }
                $details = if ($Kind -eq 'vulnerable') {
                    ($package.vulnerabilities | ForEach-Object { "$($_.severity) $($_.advisoryurl)" }) -join ', '
                } else {
                    (@($package.deprecationReasons) -join ', ') + $(if ($package.alternativePackage) { " -> $($package.alternativePackage.id)" })
                }
                [pscustomobject]@{
                    Project = Split-Path $project.path -Leaf
                    Package = "$($package.id) $($package.resolvedVersion)"
                    Details = $details
                }
            }
        }
    }
}

dotnet restore $Solution | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }

$failed = $false
foreach ($kind in 'vulnerable', 'deprecated') {
    $findings = @(Get-Findings $kind)
    if ($findings.Count -gt 0) {
        $failed = $true
        Write-Host "$($findings.Count) $kind package(s):" -ForegroundColor Red
        $findings | Format-Table -AutoSize | Out-String | Write-Host
    } else {
        Write-Host "No $kind packages." -ForegroundColor Green
    }
}

if ($failed) { exit 1 }
