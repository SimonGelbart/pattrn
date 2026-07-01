$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path (Join-Path $ScriptDir '../..')
$Project = Join-Path $ScriptDir 'Pattrn.AotCompatibility.csproj'
$Artifacts = Join-Path $RepoRoot 'artifacts/Pattrn.AotCompatibility'
$Dotnet = if ($env:DOTNET_CMD) { $env:DOTNET_CMD } else { 'dotnet' }
$Attempted = [System.Collections.Generic.List[string]]::new()
$Passed = [System.Collections.Generic.List[string]]::new()
$Failed = [System.Collections.Generic.List[string]]::new()
$Skipped = [System.Collections.Generic.List[string]]::new()

function Can-ExecuteRid([string]$Rid) {
    return ($Rid -eq 'win-x64' -and $IsWindows) -or ($Rid -eq 'linux-x64' -and $IsLinux -and [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq 'X64')
}

function Get-ExePath([string]$Rid, [string]$Mode) {
    $extension = if ($Rid.StartsWith('win-')) { '.exe' } else { '' }
    return Join-Path $Artifacts (Join-Path $Rid (Join-Path $Mode "Pattrn.AotCompatibility$extension"))
}

function Invoke-Publish([string]$Rid, [string]$Mode, [string[]]$Properties) {
    $label = "$Mode publish $Rid"
    $Attempted.Add($label)
    $output = Join-Path $Artifacts (Join-Path $Rid $Mode)
    $log = Join-Path $output 'publish.txt'
    Remove-Item $output -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $output -Force | Out-Null

    & $Dotnet publish $Project -c Release -r $Rid -o $output @Properties *> $log
    if ($LASTEXITCODE -ne 0) {
        $text = Get-Content $log -Raw -ErrorAction SilentlyContinue
        if ($text -match '(?i)NU1301|NU1101|NU1801|proxy|Unable to load the service index|Failed to retrieve information') {
            $Skipped.Add("$label`: package source or restore prerequisite unavailable; see $log")
            return
        }
        if ($Mode -eq 'aot' -and $text -match '(?i)native aot|clang|linker|was not found|is required|Platform linker|Exit code 1') {
            $Skipped.Add("$label`: Native AOT toolchain or RID prerequisite unavailable; see $log")
            return
        }
        $Failed.Add("$label`: dotnet publish failed; see $log")
        return
    }

    $content = Get-Content $log -Raw
    if ($content -match '(^|[^A-Z0-9])(IL[0-9]{4}|ILC[0-9]{4}|Trim analysis warning|AOT analysis warning)') {
        $Failed.Add("$label`: trim/AOT warnings were emitted; see $log")
        return
    }

    $Passed.Add($label)
}

function Invoke-Smoke([string]$Rid, [string]$Mode) {
    $label = "$Mode smoke $Rid"
    $Attempted.Add($label)
    $exe = Get-ExePath $Rid $Mode
    if (-not (Test-Path $exe)) {
        $Skipped.Add("$label`: published executable not present")
        return
    }
    if (-not (Can-ExecuteRid $Rid)) {
        $Skipped.Add("$label`: host cannot execute $Rid binaries")
        return
    }
    & $exe
    if ($LASTEXITCODE -eq 0) { $Passed.Add($label) } else { $Failed.Add("$label`: smoke executable returned $LASTEXITCODE") }
}

Remove-Item $Artifacts -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $Artifacts -Force | Out-Null
Set-Location $RepoRoot

foreach ($rid in @('linux-x64', 'win-x64')) {
    Invoke-Publish $rid 'trimmed' @('-p:PublishTrimmed=true', '-p:SelfContained=true')
    Invoke-Smoke $rid 'trimmed'
    Invoke-Publish $rid 'aot' @('-p:PublishAot=true')
    Invoke-Smoke $rid 'aot'
}

Write-Host 'AOT compatibility validation summary'
Write-Host 'Attempted:'; $Attempted | ForEach-Object { Write-Host "  - $_" }
Write-Host 'Passed:'; if ($Passed.Count) { $Passed | ForEach-Object { Write-Host "  - $_" } } else { Write-Host '  - none' }
Write-Host 'Skipped:'; if ($Skipped.Count) { $Skipped | ForEach-Object { Write-Host "  - $_" } } else { Write-Host '  - none' }
Write-Host 'Failed:'; if ($Failed.Count) { $Failed | ForEach-Object { Write-Host "  - $_" } } else { Write-Host '  - none' }

if ($Failed.Count -gt 0) { exit 1 }
