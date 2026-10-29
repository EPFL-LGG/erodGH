param([string]$BinDirectory)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$bin=if($BinDirectory) { [IO.Path]::GetFullPath($BinDirectory) } else { Join-Path $root 'bin/erod' }
$search=@($bin, "$env:ProgramFiles\Rhino 8\System", "$env:ProgramFiles\Rhino 8\Plug-ins\Grasshopper")
[AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($sender,$eventArgs)
    $name=([Reflection.AssemblyName]::new($eventArgs.Name)).Name+'.dll'
    foreach($folder in $search) {
        $path=Join-Path $folder $name
        if(Test-Path $path) { return [Reflection.Assembly]::LoadFrom($path) }
    }
    return $null
})
foreach($name in @('ErodData.gha','ErodModel.gha')) {
    $assembly=[Reflection.Assembly]::LoadFrom((Join-Path $bin $name))
    try {
        $types=$assembly.GetTypes()
        $components=@($types | Where-Object { !$_.IsAbstract -and $_.IsSubclassOf([Grasshopper.Kernel.GH_Component]) })
        "PASS: $name - $($types.Length) types, $($components.Length) Grasshopper components"
    } catch [Reflection.ReflectionTypeLoadException] {
        $_.Exception.LoaderExceptions | ForEach-Object { Write-Error $_ }
        throw
    }
}
