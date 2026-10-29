param([ValidateSet(7,8)][int]$RhinoVersion=8)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$bin=Join-Path $root 'bin/erod'
foreach($name in @('ErodData.gha','ErodModel.gha','erod.dll')) {
    if(!(Test-Path (Join-Path $bin $name))) { throw "Missing $name. Run build-windows.ps1 first." }
}
$rhino=Join-Path $env:ProgramFiles "Rhino $RhinoVersion/System/Rhino.exe"
if(!(Test-Path $rhino)) { throw "Rhino $RhinoVersion was not found at $rhino" }
$savedPath=$env:PATH
$savedPackages=$env:RHINO_PACKAGE_DIRS
try {
    $env:PATH="$bin;$savedPath"
    $env:RHINO_PACKAGE_DIRS=if($savedPackages) { "$bin;$savedPackages" } else { $bin }
    $rhinoArgs=@('/nosplash','/runscript=Grasshopper')
    if($RhinoVersion -eq 8) { $rhinoArgs=@('/netfx')+$rhinoArgs }
    Start-Process -FilePath $rhino -ArgumentList $rhinoArgs
} finally {
    $env:PATH=$savedPath
    $env:RHINO_PACKAGE_DIRS=$savedPackages
}
Write-Host 'If ERod is not visible, add bin/erod in GrasshopperDeveloperSettings and restart Rhino.'
