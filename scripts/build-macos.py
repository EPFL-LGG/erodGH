#!/usr/bin/env python3
"""macOS native/plugin builder. Source dependencies are shared with Windows."""
import argparse, hashlib, json, os, platform, shutil, subprocess, sys, tarfile, time, urllib.request
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'tools'))
from release import extract, smoke
def run(*args,env=None): subprocess.run([str(a) for a in args],check=True,env=env)
def file_hash(path,algorithm='sha256'):
    h=hashlib.new(algorithm)
    with Path(path).open('rb') as f:
        for block in iter(lambda:f.read(1024*1024),b''): h.update(block)
    return h.hexdigest()
def source(name,lock,base):
    spec=lock[name]; archive=base/'downloads'/f'{name}.tar.gz'; archive.parent.mkdir(parents=True,exist_ok=True)
    if not archive.exists() or file_hash(archive,spec['algorithm'].lower())!=spec['hash'].lower():
        for attempt in range(3):
            try:
                print(f'Downloading {name} {spec["version"]}',flush=True)
                part=archive.with_suffix('.part')
                with urllib.request.urlopen(spec['url'],timeout=120) as response,part.open('wb') as f: shutil.copyfileobj(response,f)
                if file_hash(part,spec['algorithm'].lower())!=spec['hash'].lower(): raise ValueError(f'Checksum mismatch: {name}')
                part.replace(archive); break
            except Exception:
                if attempt==2: raise
                time.sleep(2)
    parent=base/'sources'; parent.mkdir(parents=True,exist_ok=True)
    folder=parent/spec['directory']; marker=folder/'.erod-source-hash'
    if not marker.exists() or marker.read_text().strip()!=spec['hash']:
        print('Extracting '+name,flush=True)
        with tarfile.open(archive) as tar: tar.extractall(parent,filter='data')
        marker.write_text(spec['hash'])
    return folder
def configure_build(source_dir,binary,env,arch,configuration,*options):
    run('cmake','-S',source_dir,'-B',binary,'-G','Ninja',f'-DCMAKE_BUILD_TYPE={configuration}',
        f'-DCMAKE_OSX_ARCHITECTURES={arch}','-DCMAKE_OSX_DEPLOYMENT_TARGET=13.0','-DCMAKE_POSITION_INDEPENDENT_CODE=ON',
        '-DCMAKE_POLICY_VERSION_MINIMUM=3.5',*options,env=env)
    run('cmake','--build',binary,'--parallel','2',env=env)
def dependencies(base,env,arch,configuration):
    lock=json.loads((ROOT/'cmake/dependencies.json').read_text())
    prefix=base/configuration/('install-suitesparse-'+lock['suitesparse']['version'])
    signature=hashlib.sha256((json.dumps(lock,sort_keys=True)+file_hash(__file__)+file_hash(ROOT/'cmake/SuiteSparseOptions.cmake')+arch+configuration+subprocess.check_output(['clang','--version'],text=True)).encode()).hexdigest()
    marker=base/configuration/'complete.txt'
    if marker.exists() and marker.read_text()==signature and (prefix/'lib/cmake/Boost-1.86.0/BoostConfig.cmake').exists():
        print('Using source-built dependencies: '+str(prefix)); return prefix
    boost=source('boost',lock,base); suite=source('suitesparse',lock,base)
    # Build upstream SuiteSparse directly; use Apple's LP64 Accelerate BLAS.
    for name,src,options in (
        ('boost',boost,['-DBOOST_INCLUDE_LIBRARIES='+';'.join(lock['boost']['libraries']),'-DBUILD_SHARED_LIBS=OFF','-DBUILD_TESTING=OFF']),
        ('suitesparse-'+lock['suitesparse']['version'],suite,['-C',ROOT/'cmake/SuiteSparseOptions.cmake',
            '-DBUILD_SHARED_LIBS=OFF','-DBUILD_STATIC_LIBS=ON','-DBLA_VENDOR=Apple','-DBLA_SIZEOF_INTEGER=4'])):
        binary=base/configuration/name
        configure_build(src,binary,env,arch,configuration,f'-DCMAKE_INSTALL_PREFIX={prefix}','-DCMAKE_INSTALL_MESSAGE=NEVER',*options)
        run('cmake','--install',binary,env=env)
    (prefix/'erod-dependencies.json').write_text(json.dumps(lock,indent=2))
    marker.write_text(signature); return prefix
def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--fast',action='store_true');p.add_argument('--configuration',choices=['Release','Debug'],default='Release')
    p.add_argument('--native-only',action='store_true');p.add_argument('--managed-only',action='store_true');p.add_argument('--native-package')
    p.add_argument('--build-dir',type=Path);p.add_argument('--output',type=Path)
    p.add_argument('--package',action='store_true',help='Create preview ZIPs and checksums after a source build')
    p.add_argument('--package-dir',type=Path,default=ROOT/'dist',help='Distribution ZIP directory (default: dist)')
    a=p.parse_args()
    if sys.platform!='darwin': p.error('Run this builder on macOS; use build-windows.cmd on Windows')
    if sys.version_info<(3,10): p.error('Python 3.10+ is required; select a newer interpreter with PYTHON=/path/to/python3 bash build-macos.sh')
    if not hasattr(tarfile,'data_filter'): p.error('This Python lacks safe tar extraction (tarfile.data_filter). Update to the latest maintenance release of Python 3.10+; archive extraction filters are required.')
    if a.package and (a.managed_only or a.native_package): p.error('--package requires a source build; combine it with the full build or --native-only')
    if a.native_only and a.native_package: p.error('Do not combine --native-only with --native-package')
    if a.native_only and a.managed_only: p.error('Choose native-only or managed-only')
    arch=platform.machine(); base=(a.build_dir or ROOT/'build'/('macos-'+arch)).resolve(); output=(a.output or ROOT/'bin/erod').resolve()
    base.mkdir(parents=True,exist_ok=True); output.mkdir(parents=True,exist_ok=True)
    env=os.environ.copy()
    for name in ('BOOST_ROOT','BOOST_INCLUDE_DIRS','BOOST_LIBRARIES','SuiteSparse_DIR','SUITESPARSE_ROOT','CMAKE_PREFIX_PATH','CMAKE_TOOLCHAIN_FILE','VCPKG_ROOT'): env.pop(name,None)
    env.update(NUGET_PACKAGES=str(base/'nuget/packages'),NUGET_HTTP_CACHE_PATH=str(base/'nuget/http-cache'),DOTNET_CLI_HOME=str(base/'dotnet-home'),DOTNET_GENERATE_ASPNET_CERTIFICATE='false',DOTNET_CLI_TELEMETRY_OPTOUT='1')
    stage=base/'stage/native'
    if a.native_package:
        extract(a.native_package,output)
    elif not a.managed_only:
        for tool in ('cmake','ninja','clang'):
            if not shutil.which(tool): p.error(f'Missing build tool: {tool}')
        prefix=dependencies(base/'dependencies',env,arch,a.configuration)
        binary=base/'native'
        configure_build(ROOT,binary,env,arch,a.configuration,f'-DEROD_DEPS_PREFIX={prefix}',f'-DEROD_OUTPUT_DIRECTORY={output}',
            '-DEROD_BUILD_PYTHON=OFF','-DDISABLE_Knitro=ON','-DEROD_FAST_BUILD='+('ON' if a.fast else 'OFF'))
        if stage.exists():
            if stage.is_symlink() or not stage.resolve().is_relative_to(base): raise ValueError('Unsafe native stage')
            shutil.rmtree(stage)
        run('cmake','--install',binary,'--prefix',stage,env=env)
        for f in stage.iterdir():
            if f.is_file(): shutil.copy2(f,output/f.name)
    if not a.native_only:
        if not shutil.which('dotnet'): p.error('Install .NET SDK 8 or later to build the managed plugin')
        run('dotnet','build',ROOT/'src/erod/ElasticRod.sln','-c',a.configuration,'-m:1','-nr:false',
            '-p:BuildInParallel=false','-p:RestoreDisableParallel=true','-p:UseSharedCompilation=false',f'-p:ErodOutputDirectory={output}',env=env)
    smoke(output)
    print('Build output: '+str(output))
    if not a.managed_only and not a.native_package: print('Native package stage: '+str(stage))
    if a.package:
        arguments=[sys.executable,ROOT/'tools/release.py','package','--native',stage,'--build-dir',base,
                   '--output',a.package_dir.resolve(),'--replace']
        run(*arguments,'--kind','native',env=env)
        if not a.native_only: run(*arguments,'--kind','plugin','--plugin',output,env=env)
        print('Distribution ZIPs and checksums: '+str(a.package_dir.resolve()))
if __name__=='__main__': main()
