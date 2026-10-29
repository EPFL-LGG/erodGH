param([string]$Destination=(Join-Path $env:APPDATA 'Grasshopper/Libraries/ERod'),
      [string]$BackupDirectory=(Join-Path $env:LOCALAPPDATA 'ERod/Backups'))
$ErrorActionPreference='Stop'
$source=[IO.Path]::GetFullPath($PSScriptRoot)
$destination=[IO.Path]::GetFullPath($Destination)
if($source -eq $destination) { throw 'Run Install.cmd from the extracted archive, not the installed folder.' }
if((Split-Path $destination -Leaf) -ne 'ERod') { throw 'The installation folder must be named ERod.' }
if(Get-Process Rhino -ErrorAction SilentlyContinue) { throw 'Close Rhino before installing or updating ERod.' }
$manifest=Get-Content (Join-Path $source 'package.json') -Raw | ConvertFrom-Json
if($manifest.kind -ne 'plugin' -or $manifest.os -ne 'windows' -or $manifest.architecture -ne 'x64' -or $manifest.nativeAbi -ne 2) { throw 'Choose an ERod plugin package for Windows x64.' }
foreach($item in $manifest.files.PSObject.Properties) {
    $name=$item.Name
    if($name -match '(^/|\\|:|(^|/)\.\.(/|$))') { throw "Unsafe package path: $name" }
    $path=Join-Path $source $name
    $stream=[IO.File]::OpenRead($path); $sha=[Security.Cryptography.SHA256]::Create()
    try { $actual=([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','') } finally { $stream.Dispose(); $sha.Dispose() }
    if($actual -ne $item.Value) { throw "Damaged package file: $name. Download and extract again." }
}
if(Test-Path $destination) {
    if((Get-Item $destination).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'The installation folder cannot be a symbolic link.' }
    New-Item -ItemType Directory -Force $BackupDirectory | Out-Null
    $backup=Join-Path $BackupDirectory ((Get-Date -Format yyyyMMdd-HHmmss)+'-'+[Guid]::NewGuid().ToString('N'))
    $checked=[IO.Path]::GetFullPath((Resolve-Path $destination).Path)
    if($checked -ne $destination -or (Split-Path $checked -Leaf) -ne 'ERod') { throw 'Unexpected existing installation path.' }
    Move-Item -LiteralPath $checked -Destination $backup
    Write-Host "Previous installation backed up to $backup"
}
New-Item -ItemType Directory -Force $destination | Out-Null
foreach($item in $manifest.files.PSObject.Properties) {
    $target=Join-Path $destination $item.Name
    New-Item -ItemType Directory -Force (Split-Path $target -Parent) | Out-Null
    Copy-Item -LiteralPath (Join-Path $source $item.Name) -Destination $target
}
Copy-Item -LiteralPath (Join-Path $source 'package.json') -Destination $destination
Write-Host "ERod installed in $destination. Start Rhino and Grasshopper."
