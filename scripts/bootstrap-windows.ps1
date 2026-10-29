param(
    [Parameter(Mandatory=$true)][string]$BuildDirectory,
    [ValidateSet('Release','Debug')][string]$Configuration='Release'
)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'WindowsBuild.Common.ps1')
$base=Join-Path $BuildDirectory 'dependencies'
$lock=Get-Content (Join-Path $root 'cmake/dependencies.json') -Raw | ConvertFrom-Json
$prefix=Join-Path $base "$Configuration/install-suitesparse-$($lock.suitesparse.version)"
$sources=Join-Path $base 'sources'
$downloads=Join-Path $base 'downloads'
New-Item -ItemType Directory -Force $sources,$downloads,$prefix | Out-Null
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
$signatureInputs=@(
    (Get-ErodFileHash $PSCommandPath -Algorithm SHA256).Hash,
    (Get-ErodFileHash (Join-Path $PSScriptRoot 'WindowsBuild.Common.ps1') -Algorithm SHA256).Hash,
    (Get-ErodFileHash (Join-Path $root 'cmake/dependencies.json') -Algorithm SHA256).Hash,
    (Get-ErodFileHash (Join-Path $root 'cmake/windows-dependencies/clapack/CMakeLists.txt') -Algorithm SHA256).Hash,
    (Get-ErodFileHash (Join-Path $root 'cmake/windows-dependencies/clapack/lapack-config.cmake.in') -Algorithm SHA256).Hash,
    (Get-ErodFileHash (Join-Path $root 'cmake/SuiteSparseOptions.cmake') -Algorithm SHA256).Hash,
    $Configuration,
    (& cmake --version | Select-Object -First 1),
    (& "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -version '[17.0,18.0)' -products '*' -property installationVersion)
) -join '|'
$sha=[Security.Cryptography.SHA256]::Create()
try { $signature=([BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($signatureInputs)))).Replace('-','') } finally { $sha.Dispose() }
$completion=Join-Path $base "$Configuration/.erod-dependencies-complete.json"
if(Test-Path $completion) {
    $previous=Get-Content $completion -Raw | ConvertFrom-Json
    if($previous.signature -eq $signature -and
       (Test-Path (Join-Path $prefix 'lib/lapack.lib')) -and
       (Get-ChildItem (Join-Path $prefix 'bin') -Filter '*cholmod*.dll' -ErrorAction SilentlyContinue) -and
       (Get-ChildItem (Join-Path $prefix 'bin') -Filter '*umfpack*.dll' -ErrorAction SilentlyContinue)) {
        Write-Host "Using previously source-built dependencies in $prefix"
        return
    }
}
function Run([string]$exe,[string[]]$arguments) {
    & $exe @arguments
    if($LASTEXITCODE -ne 0) { throw "$exe failed with exit code $LASTEXITCODE" }
}
function Source([string]$name) {
    $entry=$lock.$name
    $archive=Join-Path $downloads "$name.tar.gz"
    $valid=(Test-Path $archive) -and ((Get-ErodFileHash $archive -Algorithm $entry.algorithm).Hash -eq $entry.hash)
    if(!$valid) {
        for($attempt=1;$attempt -le 3;$attempt++) {
            try {
                Write-Host "Downloading $name ($($entry.version)), attempt $attempt"
                Invoke-WebRequest -UseBasicParsing -Uri $entry.url -OutFile "$archive.part"
                if((Get-ErodFileHash "$archive.part" -Algorithm $entry.algorithm).Hash -ne $entry.hash) { throw "Checksum mismatch for $name" }
                Move-Item -LiteralPath "$archive.part" -Destination $archive -Force
                break
            } catch { if($attempt -eq 3) { throw }; Write-Warning $_ }
        }
    }
    $directory=Join-Path $sources $entry.directory
    $marker=Join-Path $directory '.erod-source-hash'
    if(!(Test-Path $marker) -or (Get-Content $marker -Raw).Trim() -ne $entry.hash) {
        Write-Host "Extracting $name"
        Push-Location $sources
        try { Run cmake @('-E','tar','xzf',$archive) } finally { Pop-Location }
        Set-Content -LiteralPath $marker -Value $entry.hash
    }
    return $directory
}
function Build-Install([string]$name,[string]$source,[string[]]$options) {
    Write-Host "Configuring and building $name ($Configuration)"
    $binary=Join-Path $base "$Configuration/$name"
    Run cmake (@('-S',$source,'-B',$binary,'-G','Visual Studio 17 2022','-A','x64',
        "-DCMAKE_INSTALL_PREFIX=$prefix", "-DCMAKE_BUILD_TYPE=$Configuration",
        '-DCMAKE_INSTALL_MESSAGE=NEVER', '-DCMAKE_SUPPRESS_REGENERATION=ON', '-DCMAKE_POLICY_VERSION_MINIMUM=3.5', '-DCMAKE_TRY_COMPILE_CONFIGURATION=Release',
        '-DCMAKE_C_FLAGS=/MP4', '-DCMAKE_CXX_FLAGS=/MP4 /EHsc')+$options)
    Run cmake @('--build',$binary,'--config',$Configuration,'--','/m:1','/nr:false','/v:minimal')
    Run cmake @('--install',$binary,'--config',$Configuration)
}
# No environment or package registry hints are used by these source builds.
$boost=Source boost
$clapack=Source clapack
$suite=Source suitesparse
# Upstream enables its extensive tests unconditionally; only the libraries are needed.
$clapackCmake=Join-Path $clapack 'CMakeLists.txt'
$text=[IO.File]::ReadAllText($clapackCmake).Replace('add_subdirectory(TESTING)','# Tests disabled by ERod bootstrap')
[IO.File]::WriteAllText($clapackCmake,$text)
Build-Install boost $boost @("-DBOOST_INCLUDE_LIBRARIES=$($lock.boost.libraries -join ';' )",'-DBUILD_SHARED_LIBS=OFF','-DBUILD_TESTING=OFF')
Build-Install clapack (Join-Path $root 'cmake/windows-dependencies/clapack') @("-DEROD_CLAPACK_SOURCE=$clapack")
# Explicit LP64 reference BLAS/LAPACK avoids finding a machine-specific BLAS.
$blasLibraries="$prefix/lib/blas.lib;$prefix/lib/libf2c.lib"
$lapackLibraries="$prefix/lib/lapack.lib;$prefix/lib/blas.lib;$prefix/lib/libf2c.lib"
Build-Install "suitesparse-$($lock.suitesparse.version)" $suite @(
    '-C',(Join-Path $root 'cmake/SuiteSparseOptions.cmake'),
    '-DBUILD_SHARED_LIBS=ON','-DBUILD_STATIC_LIBS=OFF','-DBLA_VENDOR=Generic','-DBLA_SIZEOF_INTEGER=4',
    "-DBLAS_LIBRARIES=$blasLibraries","-DLAPACK_LIBRARIES=$lapackLibraries")
Copy-Item (Join-Path $root 'cmake/dependencies.json') (Join-Path $prefix 'erod-dependencies.json')
@{signature=$signature} | ConvertTo-Json | Set-Content -LiteralPath $completion
Write-Host "Dependencies built from source in $prefix"
