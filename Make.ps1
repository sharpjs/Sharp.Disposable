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

    # Build, run tests, and produce code covarage report.
    [Parameter(Mandatory, ParameterSetName="Coverage")]
    [switch] $Coverage,

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

$MainAssemblyName = "Sharp.Disposable"

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
    Invoke-Build

    if ($Test -or $Coverage) {
        Set-Location -LiteralPath "$MainAssemblyName.Tests"
        Invoke-TestForTargetFramework net48
        Invoke-TestForTargetFramework netcoreapp3.1
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
            "dotcover"
                "--dcReportType=HTML"
                "--dcOutput=..\coverage\$TargetFramework.html"
                "--dcFilters=+:$MainAssemblyName`;+:$MainAssemblyName.*`;-:*.Tests"
                "--dcAttributeFilters=-:System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute"
        }
        "test"
            "--framework:$TargetFramework"
            "--configuration:$Configuration"
            "--no-build"
    )
}

function Invoke-DotNetExe {
    param (
        [Parameter(Mandatory, ValueFromRemainingArguments)]
        [string[]] $Arguments
    )
    & dotnet.exe $Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet.exe exited with an error." }
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
