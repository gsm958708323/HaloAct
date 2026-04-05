[CmdletBinding()]
param(
    [string]$UnityPath,
    [string]$ProjectPath,
    [string]$TestFilter,
    [string]$TestClass,
    [string]$TestMethod,
    [string]$TestNamespace,
    [ValidateSet('EditMode', 'PlayMode')]
    [string]$TestPlatform = 'EditMode',
    [string]$ResultsXmlPath,
    [string]$SummaryJsonPath,
    [string]$LogPath,
    [switch]$NoRunSynchronously,
    [string[]]$AdditionalUnityArgs
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-ProjectRoot {
    if ($ProjectPath) {
        return (Resolve-Path -LiteralPath $ProjectPath).Path
    }

    return (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
}

function Get-UnityVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResolvedProjectPath
    )

    $versionFile = Join-Path $ResolvedProjectPath 'ProjectSettings/ProjectVersion.txt'
    $versionLine = Get-Content -LiteralPath $versionFile | Where-Object { $_ -like 'm_EditorVersion:*' } | Select-Object -First 1
    if (-not $versionLine) {
        throw "Unable to find Unity editor version in $versionFile"
    }

    return ($versionLine -split ':', 2)[1].Trim()
}

function Resolve-UnityPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResolvedProjectPath,
        [string]$RequestedUnityPath
    )

    if ($RequestedUnityPath) {
        $resolved = (Resolve-Path -LiteralPath $RequestedUnityPath).Path
        if (-not (Test-Path -LiteralPath $resolved)) {
            throw "Unity executable not found: $RequestedUnityPath"
        }

        return $resolved
    }

    $version = Get-UnityVersion -ResolvedProjectPath $ResolvedProjectPath
    $candidates = @(@(
        "D:\Work\UnityInstall\$version\Editor\Unity.exe",
        (Join-Path ${env:ProgramFiles} "Unity\Hub\Editor\$version\Editor\Unity.exe"),
        (Join-Path ${env:ProgramFiles} "Unity\Editor\$version\Editor\Unity.exe")
    ) | Where-Object { $_ -and (Test-Path -LiteralPath $_) })

    if ($candidates.Count -eq 0) {
        throw "Unable to locate Unity.exe for version $version. Pass -UnityPath explicitly."
    }

    return $candidates[0]
}

function New-TestSummary {
    param(
        [Parameter(Mandatory = $true)]
        [xml]$XmlDocument,
        [Parameter(Mandatory = $true)]
        [string]$ResolvedResultsXmlPath,
        [Parameter(Mandatory = $true)]
        [string]$ResolvedLogPath,
        [string]$ResolvedFilter
    )

    $run = $XmlDocument.'test-run'
    if (-not $run) {
        throw "Test results XML is missing the test-run root node."
    }

    $failedCases = @()
    $failedNodes = @(Select-Xml -Xml $XmlDocument -XPath "//test-case[@result='Failed']" | ForEach-Object { $_.Node })
    foreach ($node in $failedNodes) {
        $failedCases += [ordered]@{
            name = $node.fullname
            message = $node.failure.message.'#text'
            stackTrace = $node.failure.'stack-trace'.'#text'
            duration = [double]$node.duration
        }
    }

    return [ordered]@{
        result = $run.result
        total = [int]$run.total
        passed = [int]$run.passed
        failed = [int]$run.failed
        inconclusive = [int]$run.inconclusive
        skipped = [int]$run.skipped
        duration = [double]$run.duration
        testFilter = $ResolvedFilter
        resultsXmlPath = $ResolvedResultsXmlPath
        logPath = $ResolvedLogPath
        failedTests = $failedCases
    }
}

function Resolve-TestFilter {
    param(
        [string]$RawTestFilter,
        [string]$RequestedTestClass,
        [string]$RequestedTestMethod,
        [string]$RequestedTestNamespace
    )

    $hasRawFilter = -not [string]::IsNullOrWhiteSpace($RawTestFilter)
    $hasTestClass = -not [string]::IsNullOrWhiteSpace($RequestedTestClass)
    $hasTestMethod = -not [string]::IsNullOrWhiteSpace($RequestedTestMethod)
    $hasTestNamespace = -not [string]::IsNullOrWhiteSpace($RequestedTestNamespace)
    $convenienceSelectorCount = @($hasTestClass, $hasTestMethod, $hasTestNamespace).Where({ $_ }).Count

    if ($hasRawFilter -and $convenienceSelectorCount -gt 0) {
        throw 'Use either -TestFilter or one convenience selector (-TestClass, -TestMethod, -TestNamespace), not both.'
    }

    if ($hasTestNamespace -and ($hasTestClass -or $hasTestMethod)) {
        throw '-TestNamespace cannot be combined with -TestClass or -TestMethod.'
    }

    if ($hasTestClass -and $hasTestMethod) {
        if ($RequestedTestMethod.Contains('.')) {
            throw '-TestMethod must be an unqualified method name when -TestClass is also provided.'
        }

        return "$RequestedTestClass.$RequestedTestMethod"
    }

    if ($hasTestMethod) {
        return $RequestedTestMethod
    }

    if ($hasTestClass) {
        return $RequestedTestClass
    }

    if ($hasTestNamespace) {
        return "^{0}\." -f [Regex]::Escape($RequestedTestNamespace)
    }

    return $RawTestFilter
}

function Test-IsPathWithin {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BasePath,
        [Parameter(Mandatory = $true)]
        [string]$CandidatePath
    )

    $resolvedBasePath = [System.IO.Path]::GetFullPath($BasePath)
    $resolvedCandidatePath = [System.IO.Path]::GetFullPath($CandidatePath)
    if (-not $resolvedBasePath.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
        $resolvedBasePath += [System.IO.Path]::DirectorySeparatorChar
    }

    return $resolvedCandidatePath.StartsWith($resolvedBasePath, [System.StringComparison]::OrdinalIgnoreCase)
}

function Assert-PersistentArtifactPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResolvedProjectPath,
        [Parameter(Mandatory = $true)]
        [string]$ResolvedArtifactPath,
        [Parameter(Mandatory = $true)]
        [string]$ArtifactLabel
    )

    $projectTempPath = Join-Path $ResolvedProjectPath 'Temp'
    if (Test-IsPathWithin -BasePath $projectTempPath -CandidatePath $ResolvedArtifactPath) {
        throw "$ArtifactLabel must not be written under $projectTempPath because Unity can delete Temp artifacts during batchmode shutdown: $ResolvedArtifactPath"
    }
}

function Invoke-UnityTestRun {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResolvedUnityPath,
        [Parameter(Mandatory = $true)]
        [string[]]$UnityArguments
    )

    $process = Start-Process -FilePath $ResolvedUnityPath -ArgumentList $UnityArguments -PassThru -Wait
    return $process.ExitCode
}

$resolvedProjectPath = Get-ProjectRoot
$resolvedUnityPath = Resolve-UnityPath -ResolvedProjectPath $resolvedProjectPath -RequestedUnityPath $UnityPath
$resultsDirectory = Join-Path $resolvedProjectPath 'TestArtifacts\TestRunner'
$resolvedTestFilter = Resolve-TestFilter `
    -RawTestFilter $TestFilter `
    -RequestedTestClass $TestClass `
    -RequestedTestMethod $TestMethod `
    -RequestedTestNamespace $TestNamespace

if (-not $ResultsXmlPath) {
    $ResultsXmlPath = Join-Path $resultsDirectory 'editmode-results.xml'
}

if (-not $SummaryJsonPath) {
    $SummaryJsonPath = Join-Path $resultsDirectory 'editmode-summary.json'
}

if (-not $LogPath) {
    $LogPath = Join-Path $resultsDirectory 'editmode.log'
}

$resolvedResultsXmlPath = [System.IO.Path]::GetFullPath($ResultsXmlPath)
$resolvedSummaryJsonPath = [System.IO.Path]::GetFullPath($SummaryJsonPath)
$resolvedLogPath = [System.IO.Path]::GetFullPath($LogPath)

Assert-PersistentArtifactPath -ResolvedProjectPath $resolvedProjectPath -ResolvedArtifactPath $resolvedResultsXmlPath -ArtifactLabel 'ResultsXmlPath'
Assert-PersistentArtifactPath -ResolvedProjectPath $resolvedProjectPath -ResolvedArtifactPath $resolvedSummaryJsonPath -ArtifactLabel 'SummaryJsonPath'
Assert-PersistentArtifactPath -ResolvedProjectPath $resolvedProjectPath -ResolvedArtifactPath $resolvedLogPath -ArtifactLabel 'LogPath'

New-Item -ItemType Directory -Force -Path ([System.IO.Path]::GetDirectoryName($resolvedResultsXmlPath)) | Out-Null
New-Item -ItemType Directory -Force -Path ([System.IO.Path]::GetDirectoryName($resolvedSummaryJsonPath)) | Out-Null
New-Item -ItemType Directory -Force -Path ([System.IO.Path]::GetDirectoryName($resolvedLogPath)) | Out-Null

Remove-Item -LiteralPath $resolvedResultsXmlPath, $resolvedSummaryJsonPath, $resolvedLogPath -ErrorAction SilentlyContinue

$unityArguments = @(
    '-batchmode',
    '-projectPath', $resolvedProjectPath,
    '-runTests',
    '-testPlatform', $TestPlatform,
    '-testResults', $resolvedResultsXmlPath,
    '-logFile', $resolvedLogPath
)

if (-not $NoRunSynchronously) {
    $unityArguments += '-runSynchronously'
}

if ($resolvedTestFilter) {
    $unityArguments += @('-testFilter', $resolvedTestFilter)
}

if ($AdditionalUnityArgs) {
    $unityArguments += $AdditionalUnityArgs
}

$unityExitCode = $null
$maxAttempts = 2
for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
    Remove-Item -LiteralPath $resolvedResultsXmlPath, $resolvedLogPath -ErrorAction SilentlyContinue
    $unityExitCode = Invoke-UnityTestRun -ResolvedUnityPath $resolvedUnityPath -UnityArguments $unityArguments

    if (Test-Path -LiteralPath $resolvedResultsXmlPath) {
        break
    }

    if ($unityExitCode -ne 0 -or $attempt -ge $maxAttempts) {
        break
    }

    Write-Warning "Unity did not create test results XML on attempt $attempt. Retrying once."
}

if (-not (Test-Path -LiteralPath $resolvedResultsXmlPath)) {
    $markers = @()
    if (Test-Path -LiteralPath $resolvedLogPath) {
        $logText = Get-Content -LiteralPath $resolvedLogPath -Raw
        if ($logText -match 'Running tests for') {
            $markers += 'runner-started'
        }

        if ($logText -match 'Saving results to:') {
            $markers += 'runner-reported-save'
        }

        if ($logText -match 'Test run completed\. Exiting with code') {
            $markers += 'runner-completed'
        }
    }

    $markerSuffix = if ($markers.Count -gt 0) { " Log markers: $($markers -join ', ')." } else { '' }
    throw "Unity finished without creating test results XML after $maxAttempts attempt(s): $resolvedResultsXmlPath.$markerSuffix"
}

[xml]$xml = Get-Content -LiteralPath $resolvedResultsXmlPath
$summary = New-TestSummary -XmlDocument $xml -ResolvedResultsXmlPath $resolvedResultsXmlPath -ResolvedLogPath $resolvedLogPath -ResolvedFilter $resolvedTestFilter
$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resolvedSummaryJsonPath -Encoding UTF8

if ($unityExitCode -ne 0) {
    exit $unityExitCode
}

if ($summary.result -ne 'Passed') {
    exit 1
}

exit 0
