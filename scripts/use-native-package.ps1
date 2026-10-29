param([Parameter(Mandatory=$true)][string]$Archive,[Parameter(Mandatory=$true)][string]$Destination)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'WindowsBuild.Common.ps1')
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[IO.Compression.ZipFile]::OpenRead([IO.Path]::GetFullPath($Archive))
try {
    $names=@{}
    $size=0L
    foreach($entry in $zip.Entries) {
        $name=$entry.FullName
        $size+=$entry.Length
        if($name -match '(^/|\\|:|(^|/)\.\.(/|$))' -or $names.ContainsKey($name) -or (($entry.ExternalAttributes -shr 16) -band 0xf000) -eq 0xa000) {
            throw "Unsafe or duplicate archive entry: $name"
        }
        $names[$name]=$true
    }
    if($size -gt 2GB) { throw 'Native archive is too large.' }
    $reader=[IO.StreamReader]::new($zip.GetEntry('package.json').Open())
    try { $manifest=$reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
    if($manifest.format -ne 1 -or $manifest.nativeAbi -ne 2 -or $manifest.kind -ne 'native' -or $manifest.os -ne 'windows' -or $manifest.architecture -ne 'x64') {
        throw 'Choose an ERod ABI 2 native package for Windows x64.'
    }
    $expected=@($manifest.files.PSObject.Properties.Name)+@('package.json')
    if($expected.Count -ne $names.Count) { throw 'Unlisted or missing package files.' }
    $temp=Join-Path ([IO.Path]::GetTempPath()) ('erod-native-'+[Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory $temp | Out-Null
    foreach($property in $manifest.files.PSObject.Properties) {
        $entry=$zip.GetEntry($property.Name)
        if(!$entry) { throw "Missing archive entry: $($property.Name)" }
        $target=Join-Path $temp $property.Name
        New-Item -ItemType Directory -Force (Split-Path $target -Parent) | Out-Null
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,$target,$true)
        if((Get-ErodFileHash $target).Hash -ne $property.Value) { throw "Checksum mismatch: $($property.Name)" }
    }
    New-Item -ItemType Directory -Force $Destination | Out-Null
    Get-ChildItem $temp -File | Where-Object { $_.Extension -eq '.dll' -or $_.Name -eq 'native-build.json' } | Copy-Item -Destination $Destination -Force
} finally {
    $zip.Dispose()
    if($temp -and (Test-Path $temp)) {
        $full=[IO.Path]::GetFullPath($temp); $parent=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
        if($full.StartsWith($parent,[StringComparison]::OrdinalIgnoreCase) -and (Split-Path $full -Leaf) -like 'erod-native-*') { Remove-Item -LiteralPath $full -Recurse -Force }
    }
}
Write-Host 'Verified and installed native package; C++ compilation was skipped.'
