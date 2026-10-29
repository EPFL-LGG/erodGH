param(
    [switch]$FastBuild,
    [ValidateSet('Release','Debug')][string]$Configuration='Release',
    [string]$BuildDirectory,
    [string]$OutputDirectory,
    [switch]$NativeOnly,
    [switch]$ManagedOnly,
    [string]$NativePackage,
    [switch]$Package,
    [string]$PackageDirectory,
    [string]$PythonExecutable='python',
    [string]$BoostRoot,
    [string]$SuiteSparseRoot,
    [string]$LapackBin
)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'WindowsBuild.Common.ps1')
if($Package -and ($ManagedOnly -or $NativePackage)) { throw '-Package requires a source build; use it with the full build or -NativeOnly.' }
if($NativeOnly -and $NativePackage) { throw 'NativePackage already supplies the native libraries; do not combine it with NativeOnly.' }
if($NativeOnly -and $ManagedOnly) { throw 'Choose NativeOnly or ManagedOnly, not both.' }
if($NativePackage) { $ManagedOnly=$true }
if(!$BuildDirectory) { $BuildDirectory=Join-Path $root 'build/windows-source' }
if(!$OutputDirectory) { $OutputDirectory=Join-Path $root 'bin/erod' }
$BuildDirectory=[IO.Path]::GetFullPath($BuildDirectory)
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
$native=Join-Path $BuildDirectory 'native'
$stage=Join-Path $BuildDirectory 'stage/native'
New-Item -ItemType Directory -Force $BuildDirectory,$OutputDirectory | Out-Null
function Run([string]$exe,[string[]]$arguments) {
    & $exe @arguments
    if($LASTEXITCODE -ne 0) { throw "$exe failed with exit code $LASTEXITCODE" }
}
if($Package) {
    if(!$PackageDirectory) { $PackageDirectory=Join-Path $root 'dist' }
    $PackageDirectory=[IO.Path]::GetFullPath($PackageDirectory)
    if(!(Get-Command $PythonExecutable -ErrorAction SilentlyContinue)) { throw 'Packaging needs Python 3.10+. Install Python or pass -PythonExecutable with its full path.' }
    Run $PythonExecutable @('-c','import sys; sys.exit(sys.version_info < (3,10))')
}
$names=@('BOOST_ROOT','BOOST_INCLUDEDIR','BOOST_LIBRARYDIR','BOOST_INCLUDE_DIRS','BOOST_LIBRARIES',
    'SUITESPARSE_ROOT','SUITESPARSE_INC','SUITESPARSE_LIB','SuiteSparse_DIR','CHOLMODDIR','UMFPACKDIR',
    'CMAKE_PREFIX_PATH','CMAKE_TOOLCHAIN_FILE','VCPKG_ROOT','NUGET_PACKAGES','NUGET_HTTP_CACHE_PATH','NUGET_SCRATCH',
    'DOTNET_CLI_HOME','DOTNET_GENERATE_ASPNET_CERTIFICATE','DOTNET_CLI_TELEMETRY_OPTOUT','MSBuildEnableWorkloadResolver')
$saved=@{}
foreach($name in $names) {
    $saved[$name]=[Environment]::GetEnvironmentVariable($name,'Process')
    [Environment]::SetEnvironmentVariable($name,$null,'Process')
}
try {
    $env:NUGET_PACKAGES=Join-Path $BuildDirectory 'nuget/packages'
    $env:NUGET_HTTP_CACHE_PATH=Join-Path $BuildDirectory 'nuget/http-cache'
    $env:NUGET_SCRATCH=Join-Path $BuildDirectory 'nuget/scratch'
    $env:DOTNET_CLI_HOME=Join-Path $BuildDirectory 'dotnet-home'
    $env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false'
    $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
    $env:MSBuildEnableWorkloadResolver='false'
    if(!$ManagedOnly) {
        if(!(Get-Command cmake -ErrorAction SilentlyContinue)) { throw 'Install CMake 3.20 or later and add it to PATH.' }
        $vswhere="${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
        $vs=if(Test-Path $vswhere) { & $vswhere -latest -version '[17.0,18.0)' -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath }
        if(!$vs) { throw 'Install Visual Studio 2022 Desktop development with C++, or use -NativePackage to skip C++ compilation.' }
        $msbuild=Join-Path $vs 'MSBuild/Current/Bin/amd64/MSBuild.exe'
        $external=!!($BoostRoot -or $SuiteSparseRoot -or $LapackBin)
        $cmakeArgs=@('-S',$root,'-B',$native,'-G','Visual Studio 17 2022','-A','x64',
            "-DCMAKE_BUILD_TYPE=$Configuration","-DEROD_FAST_BUILD=$($FastBuild.IsPresent)",
            "-DEROD_OUTPUT_DIRECTORY=$OutputDirectory",'-DEROD_BUILD_PYTHON=OFF','-DDISABLE_Knitro=ON',
            '-DCMAKE_SUPPRESS_REGENERATION=ON','-DCMAKE_POLICY_VERSION_MINIMUM=3.5','-DCMAKE_TRY_COMPILE_CONFIGURATION=Release',
            '-DCMAKE_FIND_USE_PACKAGE_REGISTRY=OFF','-DCMAKE_FIND_USE_SYSTEM_PACKAGE_REGISTRY=OFF',
            '-DEROD_WINDOWS_DEPS_PREFIX=')
        if($external) {
            if(!$BoostRoot -or !$SuiteSparseRoot) { throw 'External mode requires both BoostRoot and SuiteSparseRoot.' }
            $env:BOOST_ROOT=$BoostRoot
            $env:SUITESPARSE_ROOT=$SuiteSparseRoot
            $cmakeArgs+=@('-DEROD_DEPS_PREFIX=',"-DEROD_RUNTIME_SEARCH_DIRECTORIES=$SuiteSparseRoot/bin")
            Get-ChildItem (Join-Path $SuiteSparseRoot 'bin') -Filter '*.dll' | Copy-Item -Destination $OutputDirectory -Force
            if($LapackBin) { Get-ChildItem $LapackBin -Filter '*.dll' | Copy-Item -Destination $OutputDirectory -Force }
        } else {
            & (Join-Path $PSScriptRoot 'bootstrap-windows.ps1') -BuildDirectory $BuildDirectory -Configuration $Configuration
            $prefix=Join-Path $BuildDirectory "dependencies/$Configuration/install"
            $cmakeArgs+="-DEROD_DEPS_PREFIX=$prefix"
        }
        Run cmake $cmakeArgs
        Run $msbuild @((Join-Path $native 'src/erod.vcxproj'),"/p:Configuration=$Configuration",
            '/p:Platform=x64','/m:1','/nr:false','/v:minimal')
        # CMake stages the native dependency closure instead of copying every DLL
        # from a dependency installation (which may contain unrelated/debug DLLs).
        if(Test-Path $stage) {
            $checkedStage=[IO.Path]::GetFullPath($stage)
            $expectedStage=[IO.Path]::GetFullPath((Join-Path $BuildDirectory 'stage/native'))
            if($checkedStage -ne $expectedStage -or (Get-Item $stage).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Unexpected native staging directory.' }
            Remove-Item -LiteralPath $checkedStage -Recurse -Force
        }
        Run cmake @('--install',$native,'--config',$Configuration,'--prefix',$stage)
        Get-ChildItem $stage -Filter '*.dll' | Copy-Item -Destination $OutputDirectory -Force
        Copy-Item (Join-Path $stage 'native-build.json') $OutputDirectory -Force
    } elseif($NativePackage) {
        & (Join-Path $PSScriptRoot 'use-native-package.ps1') -Archive $NativePackage -Destination $OutputDirectory
    }
    if(!$NativeOnly) {
        if(!(Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Install .NET SDK 8 or later to compile the managed plugin.' }
        Run dotnet @('build',(Join-Path $root 'src/erod/ElasticRod.sln'),'-c',$Configuration,'-m:1','-nr:false',
            '-p:BuildInParallel=false','-p:RestoreDisableParallel=true',"-p:ErodOutputDirectory=$OutputDirectory",
            '-p:UseSharedCompilation=false')
    }
    $testHost=Join-Path $PSHOME $(if($PSVersionTable.PSVersion.Major -ge 6) {'pwsh.exe'} else {'powershell.exe'})
    Run $testHost @('-NoProfile','-ExecutionPolicy','Bypass','-File',(Join-Path $PSScriptRoot 'test-windows.ps1'),
        '-BinDirectory',$OutputDirectory)
    Write-Host "Build and native test passed: $OutputDirectory"
    if(!$ManagedOnly) { Write-Host "Native distribution stage: $stage" }
    if($Package) {
        $packageArgs=@((Join-Path $root 'tools/release.py'),'package','--native',$stage,
            '--build-dir',$BuildDirectory,'--output',$PackageDirectory,'--replace')
        Run $PythonExecutable ($packageArgs+@('--kind','native'))
        if(!$NativeOnly) { Run $PythonExecutable ($packageArgs+@('--kind','plugin','--plugin',$OutputDirectory)) }
        Write-Host "Distribution ZIPs and checksums: $PackageDirectory"
    }
} finally {
    foreach($name in $names) { [Environment]::SetEnvironmentVariable($name,$saved[$name],'Process') }
}
