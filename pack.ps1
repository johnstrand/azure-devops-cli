param (
  [Parameter(Position = 0, Mandatory)][ValidateSet("major", "minor", "patch")] $VersionType
)

dotnet build -c	Release

if($LASTEXITCODE -ne 0) {
  Write-Host "Build failed"
  exit 1
}

$content = [xml](Get-Content .\Ado.csproj)

$currentVersion = $content.Project.PropertyGroup.Version.Split('.')
$nextVersion = ""

if ($VersionType -eq 'major') {
  $nextVersion = "$([int]$currentVersion[0] + 1).0.0"
}
elseif ($VersionType -eq 'minor') {
  $nextVersion = "$($currentVersion[0]).$([int]$currentVersion[1] + 1).0"
}
else {
  $nextVersion = "$($currentVersion[0]).$($currentVersion[1]).$([int]$currentVersion[2] + 1)"
}

$content.Project.PropertyGroup.Version = $nextVersion

$content.Save((Resolve-Path .\Ado.csproj))

Write-Host "New version: $nextVersion"
dotnet pack -c Release
