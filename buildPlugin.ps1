Param(
    $Version = "1.0.2"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. ".\settings.ps1"

Invoke-Exe $MSBuildPath "/t:Restore;Rebuild;Pack" "$SolutionPath" "/v:minimal" "/p:PackageVersion=$Version" "/p:PackageOutputPath=`"$OutputDirectory`""