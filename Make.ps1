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

    # Do not build or run tests.  Produce merged code coverage report only.
    [Parameter(Mandatory, ParameterSetName="CoverageReportOnly")]
    [switch] $CoverageReportOnly,

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
    if (-not $CoverageReportOnly) {
        Invoke-Build
    }

    if ($Test -or $Coverage) {
        Invoke-Test
    }

    if ($Coverage -or $CoverageReportOnly) {
        Export-CoverageReport
    }
} 

function Invoke-Build {
    Write-Phase "Build"
    Invoke-DotNetExe build --configuration:$Configuration
}

function Invoke-Test {
    Write-Phase "Test$(if ($Coverage) {" + Coverage"})"
    Remove-Item test\* -Exclude ReportGenerator.cmd -Recurse
    Invoke-DotNetExe -Arguments @(
        "test"
        "--no-build"
        "--configuration:$Configuration"
        if ($Coverage) {
            "--settings:Coverlet.runsettings"
            "--results-directory:test"
        }
    )
}

function Export-CoverageReport {
    Write-Phase "Coverage Report"
    Invoke-ReportGenerator -Arguments @(
        "-reports:test\**\*.opencover.xml"
        "-targetdir:coverage"
        "-reporttypes:Cobertura;TeamCitySummary;Badges;HtmlInline_AzurePipelines_Dark"
        "-verbosity:Warning"
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

function Invoke-ReportGenerator {
    param (
        [Parameter(Mandatory, ValueFromRemainingArguments)]
        [string[]] $Arguments
    )
    & .\test\ReportGenerator.cmd $Arguments
    if ($LASTEXITCODE -ne 0) { throw "ReportGenerator exited with an error." }
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
