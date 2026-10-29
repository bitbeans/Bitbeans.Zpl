#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Fails when a NuGet package of the solution is vulnerable, deprecated or not permissively licensed.
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

function Get-LicenseFindings {
    # Permissive licenses only: no fees, no copyleft for the consumers of the packages
    $allowed = 'MIT', 'Apache-2.0', 'BSD-2-Clause', 'BSD-3-Clause', 'MS-PL'
    $packagesRoot = ((dotnet nuget locals global-packages --list) -replace '^global-packages:\s*', '').Trim()
    $json = dotnet list $Solution package --include-transitive --format json
    if ($LASTEXITCODE -ne 0) { throw "dotnet list package failed with exit code $LASTEXITCODE" }

    $packages = foreach ($project in @(($json | ConvertFrom-Json).projects)) {
        # test projects are not shipped
        if ($project.path -match '[\\/]tests[\\/]') { continue }
        foreach ($framework in @($project.frameworks)) {
            @($framework.topLevelPackages) + @($framework.transitivePackages) | Where-Object { $_ }
        }
    }

    foreach ($package in $packages | Sort-Object id, resolvedVersion -Unique) {
        $nuspec = Get-ChildItem (Join-Path $packagesRoot $package.id.ToLowerInvariant() $package.resolvedVersion) -Filter *.nuspec | Select-Object -First 1
        $license = ([xml](Get-Content $nuspec.FullName -Raw)).package.metadata.license
        $expression = if ($license -and $license.type -eq 'expression') { $license.'#text' } else { $null }
        $parts = if ($expression) { $expression -split '\s+(?:OR|AND|WITH)\s+' | ForEach-Object { $_.Trim('(', ')', ' ') } } else { @() }
        if (-not $expression -or ($parts | Where-Object { $_ -notin $allowed })) {
            [pscustomobject]@{
                Package = "$($package.id) $($package.resolvedVersion)"
                Details = if ($expression) { $expression } else { 'no SPDX license expression' }
            }
        }
    }
}

dotnet restore $Solution | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }

$failed = $false

$licenseFindings = @(Get-LicenseFindings)
if ($licenseFindings.Count -gt 0) {
    $failed = $true
    Write-Host "$($licenseFindings.Count) package(s) with a license that is not allowed:" -ForegroundColor Red
    $licenseFindings | Format-Table -AutoSize | Out-String | Write-Host
} else {
    Write-Host "All shipped dependencies use permissive licenses." -ForegroundColor Green
}

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
