<#
.SYNOPSIS
    Invokes various build commands.

.DESCRIPTION
    This script is similar to a makefile.
#>
[CmdletBinding(DefaultParameterSetName="Test")]
param (
    # Build.
    [Parameter(Mandatory, ParameterSetName="Build")]
    [switch] $Build,

    # Build, run tests.
    [Parameter(ParameterSetName="Test")]
    [switch] $Test,

    # Build, run tests, and produce merged code coverage report.
    [Parameter(Mandatory, ParameterSetName="Coverage")]
    [switch] $Coverage,

    # Produce merged code coverage report.
    [Parameter(Mandatory, ParameterSetName="MergeCoverage")]
    [switch] $MergeCoverage,

    # The configuration to build: Debug or Release.  The default is Debug.
    [Parameter(ParameterSetName="Build")]
    [Parameter(ParameterSetName="Test")]
    [Parameter(ParameterSetName="Coverage")]
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Debug"
)

#Requires -Version 5
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$AssemblyNameRoot = "Sharp.Disposable"

$Command = $PSCmdlet.ParameterSetName
if ($Command -eq "Test") { $Test = $true }

# http://patorjk.com/software/taag/#p=display&f=Slant
Write-Host -ForegroundColor Cyan @' 

       _____ __                       ____  _                             __    __   
      / ___// /_  ____ __________    / __ \(_)________  ____  _________ _/ /_  / /__ 
      \__ \/ __ \/ __ `/ ___/ __ \  / / / / / ___/ __ \/ __ \/ ___/ __ `/ __ \/ / _ \
     ___/ / / / / /_/ / /  / /_/ / / /_/ / (__  ) /_/ / /_/ (__  ) /_/ / /_/ / /  __/
    /____/_/ /_/\__,_/_/  / .___(_)_____/_/____/ .___/\____/____/\__,_/_.___/_/\___/ 
                         /_/                  /_/                                    
'@

function Main {
    if (-not $MergeCoverage) {
        Invoke-Build
    }

    if ($Test -or $Coverage) {
        Set-Location -LiteralPath "$AssemblyNameRoot.Tests"
        Invoke-TestForTargetFramework net48
        Invoke-TestForTargetFramework netcoreapp3.1
        Set-Location ..
    }

    if ($Coverage -or $MergeCoverage) {
        Set-Location -LiteralPath coverage
        Export-CoverageReport
        Set-Location ..
    }
} 

function Invoke-Build {
    Write-Phase "Build"
    Invoke-DotNetExe build --configuration $Configuration
}

function Invoke-TestForTargetFramework {
    param (
        [Parameter(Mandatory)]
        [string] $TargetFramework
    )

    Write-Phase "Test: $TargetFramework$(if ($Coverage) {" + Coverage"})"
    Invoke-DotNetExe -Arguments @(
        if ($Coverage) {
            $Directory = Get-Location | Split-Path -Leaf
            "dotcover"
                "--dcOutput=..\coverage\$Directory.$TargetFramework.dcvr"
                "--dcFilters=+:$AssemblyNameRoot`;+:$AssemblyNameRoot.*`;-:*.Tests"
                "--dcAttributeFilters=System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute"
        }
        "test"
            "--framework:$TargetFramework"
            "--configuration:$Configuration"
            "--no-build"
    )
}

function Export-CoverageReport {
    Write-Phase "Coverage Report"
    $Snapshots = (Get-Item *.dcvr) -join ';'
    Invoke-DotCoverExe merge  /Source=$Snapshots   /Output=Coverage.mrg
    Invoke-DotCoverExe report /Source=Coverage.mrg /Output=Coverage.html /ReportType=HTML /HideAutoProperties
    Invoke-DotCoverExe report /Source=Coverage.mrg /Output=Coverage.xml  /ReportType=XML  /HideAutoProperties
    $Report   = [xml] (Get-Content Coverage.xml -Raw -Encoding UTF8)
    $Coverage = [int] $Report.Root.CoveragePercent
    $Color    = $(if ($Coverage -eq 100) {"Green"} else {"Red"})
    Write-Host "`nCode Coverage: $Coverage%`n" -ForegroundColor $Color
}

function Invoke-DotNetExe {
    param (
        [Parameter(Mandatory, ValueFromRemainingArguments)]
        [string[]] $Arguments
    )
    & dotnet.exe $Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet.exe exited with an error." }
}

function Invoke-DotCoverExe {
    param (
        [Parameter(Mandatory, ValueFromRemainingArguments)]
        [string[]] $Arguments
    )
    $DotCoverExe = Resolve-NuGetPackageCache `
        | Join-Path -Resolve -ChildPath jetbrains.dotcover.dotnetclitool\2019.3.0 `
        | Join-Path -Resolve -ChildPath tools\dotCover.exe
    & $DotCoverExe $Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotcover.exe exited with an error." }
}

function Resolve-NuGetPackageCache {
    $Path = Invoke-DotNetExe nuget locals global-packages --list
    # dotnet.exe returns package path in a format that must be split up:
    #     info : global-packages: C:\Users\Foo\.nuget\packages\
    #                             |-------desired--part-------|
    ($Path -split ': ')[2]
}

function Write-Phase {
    param (
        [Parameter(Mandatory)]
        [string] $Name
    )
    Write-Host "`n===== $Name =====`n" -ForegroundColor Cyan
}

# Invoke Main
try {
    Push-Location $PSScriptRoot
    Main
}
finally {
    Pop-Location
}
