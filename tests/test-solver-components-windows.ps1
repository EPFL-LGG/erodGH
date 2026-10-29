param([string]$BinDirectory)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
if(!$BinDirectory) { $BinDirectory=Join-Path $root 'bin/erod' }
. (Join-Path $root 'scripts/test-managed-windows.ps1') -BinDirectory $BinDirectory
$assembly=[Reflection.Assembly]::LoadFrom((Join-Path $BinDirectory 'ErodModel.gha'))
$baseline=Get-Content (Join-Path $PSScriptRoot 'solver-components-schema.json') -Raw | ConvertFrom-Json
foreach($original in $baseline) {
    $type=$assembly.GetTypes() | Where-Object Name -eq $original.name
    $component=[Activator]::CreateInstance($type)
    if($component.ComponentGuid.ToString() -ne $original.guid) { throw "Component GUID changed: $($original.name)" }
    $expected=@($original.inputs | Where-Object { $_ -ne 'Live report' }) + @('Live report')
    $inputs=@($component.Params.Input | ForEach-Object Name)
    $outputs=@($component.Params.Output | ForEach-Object Name)
    if(($inputs -join '|') -ne ($expected -join '|')) { throw "Input order changed: $($original.name)" }
    if(($outputs -join '|') -ne ($original.outputs -join '|')) { throw "Outputs changed: $($original.name)" }
    $log=$component.Params.Input[$component.Params.Input.Count-1]
    $values=@($log.PersistentData.AllData($true))
    if($log.NickName -ne 'Log' -or $values.Count -ne 1 -or $values[0].Value -ne $false) { throw "Invalid Live report default: $($original.name)" }
    if($type.BaseType.Name -ne 'LiveReportSolverComponent') { throw "Missing common log lifecycle: $($original.name)" }
    "PASS: $($original.name) - Log defaults false; existing inputs, outputs and GUID preserved"
}
