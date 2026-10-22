# Build a source-only zip for submission (excludes bin/obj/.data/.nuget/.dotnet)
$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$staging = Join-Path $env:TEMP "mdlib-package"
$destination = Join-Path (Split-Path $root -Parent) "MarkdownLibrary-src.zip"

if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
if (Test-Path $destination) { Remove-Item $destination -Force }
New-Item -ItemType Directory -Path $staging | Out-Null

$directories = @(
    "src\MarkdownLibrary",
    "src\MarkdownLibrary.Library",
    "tests\MarkdownLibrary.UnitTest",
    "samples",
    "docs",
    "tools\Smoke"
)

foreach ($directory in $directories) {
    $source = Join-Path $root $directory
    if (-not (Test-Path $source)) { continue }

    Get-ChildItem $source -Recurse -File |
        Where-Object { $_.FullName -notmatch '\\bin\\|\\obj\\|\\TestResults\\' } |
        ForEach-Object {
            $relative = $_.FullName.Substring($root.Length + 1)
            $target = Join-Path $staging $relative
            $parent = Split-Path $target -Parent
            New-Item -ItemType Directory -Path $parent -Force | Out-Null
            Copy-Item $_.FullName $target -Force
        }
}

foreach ($file in @("README.md", "MarkdownLibrary.sln", ".gitignore")) {
    $source = Join-Path $root $file
    if (Test-Path $source) { Copy-Item $source (Join-Path $staging $file) -Force }
}

Compress-Archive -Path (Join-Path $staging "*") -DestinationPath $destination -CompressionLevel Optimal
Remove-Item $staging -Recurse -Force

$size = (Get-Item $destination).Length
Write-Host ("Created {0} ({1:N2} MB)" -f $destination, ($size / 1MB))
if ($size -gt 10MB) {
    Write-Warning "Archive exceeds 10MB. Check whether build output was included."
}
