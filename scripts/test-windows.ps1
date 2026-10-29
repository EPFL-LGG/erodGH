param([string]$BinDirectory)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$bin = if($BinDirectory) { [IO.Path]::GetFullPath($BinDirectory) } else { Join-Path $root 'bin/erod' }
$library = Join-Path $bin 'erod.dll'
if (!(Test-Path $library)) { throw 'Build erod.dll first with scripts/build-windows.ps1.' }
$env:PATH = "$bin;$env:PATH"
$code = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'NativeSmoke.cs')).Replace('EROD_LIBRARY_PATH', $library.Replace('\','/'))
Add-Type -TypeDefinition $code
[ErodNativeSmoke]::SetDllDirectory($bin) | Out-Null
[ErodNativeSmoke]::Run()
