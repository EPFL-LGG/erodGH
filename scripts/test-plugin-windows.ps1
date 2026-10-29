param([string]$BinDirectory,[string]$BuildDirectory)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'test-managed-windows.ps1') -BinDirectory $BinDirectory
foreach($name in 'ErodData','ErodModel') {
    $assembly=[Reflection.Assembly]::LoadFrom((Join-Path $bin ($name+'.gha')))
    $manager=[Resources.ResourceManager]::new($name+'.Properties.Resources.Resources',$assembly)
    $set=$manager.GetResourceSet([Globalization.CultureInfo]::InvariantCulture,$true,$false)
    $count=0
    foreach($item in $set) { if($null -ne $item.Value){$count++} }
    if($count -eq 0){throw 'No embedded icons loaded'}
    "PASS: $name embedded resources ($count) decoded"
}
if(!$BuildDirectory) { $BuildDirectory=Join-Path $root 'build/windows-source' }
$referenceFacade=Join-Path $BuildDirectory 'nuget/packages/microsoft.netframework.referenceassemblies.net48/1.0.3/build/.NETFramework/v4.8/Facades/netstandard.dll'
if(!(Test-Path $referenceFacade)) { throw 'Reference assemblies not found. Pass -BuildDirectory with the cache used for the managed build.' }
$refs=@('System.dll','System.Core.dll','System.Drawing.dll',$referenceFacade)
foreach($name in 'Plotly.NET.dll','FSharp.Core.dll','DynamicObj.dll','Giraffe.ViewEngine.StrongName.dll','Newtonsoft.Json.dll') {$refs+=Join-Path $bin $name}
Add-Type -IgnoreWarnings -WarningAction SilentlyContinue -ReferencedAssemblies $refs -TypeDefinition @'
public static class ChartPackageSmoke {
    public static void Run() {
        var chart = Plotly.NET.Chart2D.Chart.Histogram<double,double,string>(X:new double[] {1,2,3});
        var html = Plotly.NET.GenericChart.toEmbeddedHTML(chart);
        if (!html.Contains("Plotly")) throw new System.Exception("Chart rendering failed");
    }
}
'@
[ChartPackageSmoke]::Run()
'PASS: chart creation and HTML rendering without opening a browser'
[Reflection.Assembly]::LoadFrom((Join-Path $bin 'Renci.SshNet.dll')) | Out-Null
$ssh=[Renci.SshNet.SshClient]::new('localhost','test','unused');$ssh.Dispose()
'PASS: SSH client construction (no connection attempted)'
