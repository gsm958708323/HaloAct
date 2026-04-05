$ErrorActionPreference = "Stop"

function Get-BashExecutable {
    $candidates = @(
        "C:\Program Files\Git\bin\bash.exe",
        "C:\Program Files (x86)\Git\bin\bash.exe",
        (Join-Path $env:LOCALAPPDATA "Programs\Git\bin\bash.exe")
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    $command = Get-Command bash -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    throw "No bash executable was found. Install Git Bash or add bash to PATH."
}

function Get-BashScriptPath {
    $windowsPath = "C:\Users\Halo\.codex\skills\self-improvement\scripts\activator.sh"
    $msysPath = "/c/Users/Halo/.codex/skills/self-improvement/scripts/activator.sh"
    $wslPath = "/mnt/c/Users/Halo/.codex/skills/self-improvement/scripts/activator.sh"

    return [pscustomobject]@{
        Windows = $windowsPath
        Msys    = $msysPath
        Wsl     = $wslPath
    }
}

$bash = Get-BashExecutable
$scriptPaths = Get-BashScriptPath

if (-not (Test-Path $scriptPaths.Windows)) {
    throw "Missing activator script: $($scriptPaths.Windows)"
}

if ($bash -match "Git\\bin\\bash\.exe$") {
    & $bash -lc "`"$($scriptPaths.Msys)`""
    exit $LASTEXITCODE
}

$command = @"
if [ -f "$($scriptPaths.Msys)" ]; then
  "$($scriptPaths.Msys)"
elif [ -f "$($scriptPaths.Wsl)" ]; then
  "$($scriptPaths.Wsl)"
else
  echo "Missing activator script" >&2
  exit 1
fi
"@

& $bash -lc $command
exit $LASTEXITCODE
