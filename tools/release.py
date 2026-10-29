#!/usr/bin/env python3
"""Build-machine packaging and verification; end users need no Python/compiler."""
import argparse, ctypes, hashlib, json, os, platform, re, shutil, struct, subprocess, sys, tempfile, zipfile
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
ABI=2
def digest(path):
    h=hashlib.sha256()
    with Path(path).open('rb') as f:
        for block in iter(lambda:f.read(1024*1024),b''): h.update(block)
    return h.hexdigest()
def run(*args):
    return subprocess.check_output([str(a) for a in args],text=True).strip()
def host_os(): return {'win32':'windows','darwin':'macos'}.get(sys.platform,sys.platform)
def host_arch(): return {'amd64':'x64','x86_64':'x64','aarch64':'arm64','arm64':'arm64'}.get(platform.machine().lower(),platform.machine())
def binary_arch(path):
    with Path(path).open('rb') as f:
        head=f.read(64)
        if head[:2]==b'MZ':
            f.seek(struct.unpack_from('<I',head,0x3c)[0]); pe=f.read(6)
            if pe[:4]!=bytes.fromhex('50450000'): raise ValueError('Invalid PE header')
            return {0x8664:'x64',0xaa64:'arm64'}.get(struct.unpack_from('<H',pe,4)[0],'unsupported')
        if head[:4]==bytes.fromhex('cffaedfe'): return {0x1000007:'x64',0x100000c:'arm64'}.get(struct.unpack_from('<I',head,4)[0],'unsupported')
        raise ValueError(f'Not a supported single-architecture native library: {path}')

def smoke(directory):
    directory=Path(directory).resolve()
    name='erod.dll' if host_os()=='windows' else 'liberod.dylib'
    rod=None; lib=None
    dll_dir=os.add_dll_directory(str(directory)) if sys.platform=='win32' else None
    try:
        lib=ctypes.CDLL(str(directory/name))
        lib.erodGetApiVersion.restype=ctypes.c_int
        if lib.erodGetApiVersion()!=ABI: raise ValueError('Incompatible native ABI')
        lib.erodElasticRodBuild.argtypes=[ctypes.c_int,ctypes.POINTER(ctypes.c_double),ctypes.POINTER(ctypes.c_char_p)]
        lib.erodElasticRodBuild.restype=ctypes.c_void_p
        lib.erodElasticRodGetDoFs.argtypes=[ctypes.c_void_p,ctypes.POINTER(ctypes.POINTER(ctypes.c_double)),ctypes.POINTER(ctypes.c_int)]
        lib.erodFree.argtypes=[ctypes.c_void_p]
        lib.erodElasticRodSetMaterial.argtypes=[ctypes.c_void_p,ctypes.c_int,ctypes.c_double,ctypes.c_double,ctypes.POINTER(ctypes.c_double),ctypes.c_int,ctypes.c_int]
        lib.erodElasticRodGetEnergy.argtypes=[ctypes.c_void_p]; lib.erodElasticRodGetEnergy.restype=ctypes.c_double
        error=ctypes.c_char_p()
        rod=lib.erodElasticRodBuild(3,(ctypes.c_double*9)(0,0,0,1,0,0,2,0,0),ctypes.byref(error))
        if not rod: raise RuntimeError(error.value)
        count=ctypes.c_int(); values=ctypes.POINTER(ctypes.c_double)()
        lib.erodElasticRodGetDoFs(rod,ctypes.byref(values),ctypes.byref(count))
        try:
            if count.value!=11 or [values[i] for i in (0,3,6)]!=[0,1,2]: raise ValueError('DOF round trip failed')
        finally: lib.erodFree(values)
        lib.erodElasticRodSetMaterial(rod,0,200000,0.3,(ctypes.c_double*2)(0.02,0.01),2,0)
        if abs(lib.erodElasticRodGetEnergy(rod))>1e-8: raise ValueError('Rest energy must be zero')
        print('PASS: native ABI, rod/DOF round trip, material factorization, rest energy, buffer release')
    finally:
        if lib is not None and rod:
            lib.erodDestroyObject.argtypes=[ctypes.c_void_p]
            lib.erodDestroyObject(rod)
        if dll_dir: dll_dir.close()

def dylib_info(path):
    lines=run('otool','-L',path).splitlines()[1:]
    own=run('otool','-D',path).splitlines()[1:]
    deps=[line.strip().split(' (compatibility version')[0] for line in lines]
    return [dep for dep in deps if dep not in own]
def system_dylib(name): return name.startswith(('/usr/lib/','/System/Library/'))
def relocate_macos(folder,identity='-'):
    """The CMake install stage supplies the closure; rewrite every non-system edge."""
    folder=Path(folder)
    paths=list(folder.glob('*.dylib'))
    for path in paths:
        if path.is_symlink():
            content=path.read_bytes(); path.unlink(); path.write_bytes(content)
        path.chmod(0o755)
    for path in paths:
        subprocess.run(['codesign','--remove-signature',str(path)],stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
        for dep in dylib_info(path):
            if system_dylib(dep): continue
            target=folder/Path(dep).name
            if not target.is_file(): raise ValueError(f'Missing bundled macOS dependency: {dep}')
            run('install_name_tool','-change',dep,'@loader_path/'+target.name,path)
        run('install_name_tool','-id','@rpath/'+path.name,path)
    for path in paths:
        run('codesign','--force','--sign',identity,path)
        for dep in dylib_info(path):
            if not system_dylib(dep) and not dep.startswith('@loader_path/'):
                raise ValueError(f'Unrelocated dependency: {path}: {dep}')

def managed_closure(plugin, build):
    """Follow metadata references from both GHAs, including embedded resource readers."""
    build=Path(build or ROOT/'build/packaging').resolve()
    scanner=build/'dependency-scanner'; scanner.mkdir(parents=True,exist_ok=True)
    project=ROOT/'tools/DependencyScanner/DependencyScanner.csproj'
    env=os.environ.copy()
    env.update(DOTNET_CLI_HOME=str(build/'dotnet-home'),DOTNET_CLI_TELEMETRY_OPTOUT='1',
               DOTNET_GENERATE_ASPNET_CERTIFICATE='false',MSBuildEnableWorkloadResolver='false')
    subprocess.run(['dotnet','build',str(project),'-c','Release','-o',str(scanner),'-m:1','-nr:false',
                    '-p:UseSharedCompilation=false','-p:RestoreDisableParallel=true'],check=True,env=env)
    report=scanner/'assemblies.json'
    subprocess.run(['dotnet',str(scanner/'DependencyScanner.dll'),str(plugin),str(report)],check=True,env=env)
    assemblies=json.loads(report.read_text(encoding='utf-8'))
    by_name={a['name'].casefold():a for a in assemblies}
    if len(by_name)!=len(assemblies): raise ValueError('Ambiguous managed assembly identities')
    host={'rhinocommon','grasshopper','gh_io','rhino.ui','eto','mscorlib','netstandard',
          'system','system.core','system.data','system.drawing','system.numerics','system.runtime.numerics',
          'system.runtime.serialization','system.valuetuple','system.windows.forms','system.xml','system.xml.linq'}
    todo=['eroddata','erodmodel']; selected={}; provided=set()
    while todo:
        name=todo.pop().casefold()
        if name in selected: continue
        if name in host: provided.add(name); continue
        if name not in by_name: raise ValueError(f'Missing managed dependency: {name}')
        assembly=by_name[name]; selected[name]=assembly
        todo.extend(assembly['references']+assembly['resourceReferences'])
    audit={'roots':['ErodData.gha','ErodModel.gha'],'assemblies':list(selected.values()),
           'hostProvided':sorted(provided),'omitted':[a['file'] for a in assemblies if a['name'].casefold() not in selected]}
    return [plugin/a['file'] for a in selected.values()],audit


def copy_licenses(destination,build):
    """Preserve exact notices in a single text file; select only shipped NuGet binaries."""
    notices=[]; inventory=[]; nuget=[]; seen_notices={}
    extras=ROOT/'third_party/notices'
    extra_sources=json.loads((extras/'sources.json').read_text(encoding='utf-8'))
    def take(label,path,header_only=False):
        path=Path(path)
        if path.is_file():
            inventory.append(label+'/'+path.name)
            try: text=path.read_text(encoding='utf-8-sig')
            except UnicodeDecodeError: text=path.read_text(encoding='cp1252')
            if header_only: text=text[:text.index('*/')+2]
            sha=hashlib.sha256(text.encode('utf-8')).hexdigest()
            if sha in seen_notices:
                notices.append('\n'+label+' / '+path.name+': identical license text to '+seen_notices[sha]+'\n')
            else:
                seen_notices[sha]=label+' / '+path.name
                notices.append('\n'+'='*72+'\n'+label+' / '+path.name+'\n'+'='*72+'\n'+text)
    for p in ROOT.glob('LICENSE*'): take('ERod',p)
    mesh=ROOT/'dependencies/elastic_rods/3rdparty/MeshFEM/3rdparty'
    for name in ('eigen','tbb','optional','tinyexpr','triangle','spectra','json'):
        for pattern in ('LICENSE*','COPYING*','NOTICE*'):
            for p in (mesh/name).glob(pattern): take(name,p)
    take('json',mesh/'json/nlohmann/json.hpp',header_only=True)
    take('tbb',mesh/'tbb/third-party-programs.txt')
    take('triangle',mesh/'triangle/README')
    triangle_source={
        'url':'https://github.com/libigl/triangle/archive/d6761dd691e2e1318c83bf7773fea88d9437464a.tar.gz',
        'files':{name:digest(mesh/'triangle'/name) for name in ('triangle.c','triangle.h','README')},
    }
    notices.append('\nTriangle upstream source reference and local source SHA-256 hashes:\n'+json.dumps(triangle_source,indent=2)+'\n')
    for name in ('libigl','visvalingam_simplify','LBFGSpp'):
        for p in (ROOT/'dependencies/elastic_rods/3rdparty'/name).glob('LICENSE*'): take(name,p)
    if build:
        source=Path(build)/'dependencies/sources'
        for component in source.glob('*'):
            # The portable build wrapper is a build tool, not a distributed runtime.
            if not component.name.startswith(('boost-','clapack-','SuiteSparse-')): continue
            for pattern in ('LICENSE*','COPYING*'):
                for p in component.glob(pattern): take(component.name,p)
            if component.name.startswith('SuiteSparse-'):
                for name in ('AMD','CAMD','CCOLAMD','COLAMD','CHOLMOD','SuiteSparse_config'):
                    for p in (component/name/'Doc').glob('*[Ll]icense*'): take('SuiteSparse/'+name,p)
        shipped={p.name.casefold():digest(p) for p in destination.glob('*.dll')}
        for nuspec in (Path(build)/'nuget/packages').glob('*/*/*.nuspec'):
            binaries=[]
            for folder in ('lib','runtimes'):
                for binary in (nuspec.parent/folder).rglob('*.dll'):
                    if binary.name.casefold() in shipped and digest(binary)==shipped[binary.name.casefold()]: binaries.append(binary.name)
            if not binaries: continue
            tree=ET.parse(nuspec).getroot()
            fields={element.tag.split('}')[-1]:element for element in tree.iter()}
            lic=fields.get('license')
            record={'id':fields['id'].text,'version':fields['version'].text,'binaries':sorted(set(binaries))}
            for name in ('authors','copyright','projectUrl','licenseUrl'):
                if name in fields: record[name]=fields[name].text
            if lic is not None:
                record['license']={'type':lic.get('type'),'value':lic.text}
                if lic.get('type')=='file':
                    candidate=(nuspec.parent/(lic.text or '')).resolve()
                    if candidate.is_relative_to(nuspec.parent.resolve()): take('NuGet/'+record['id'],candidate)
            for candidate in nuspec.parent.glob('*'):
                if candidate.is_file() and candidate.name.upper().startswith(('LICENSE','COPYING','NOTICE','THIRD-PARTY-NOTICES')): take('NuGet/'+record['id'],candidate)
            key=record['id']+'-'+record['version']
            for label,entry in extra_sources.items():
                if label==key or label==key+'-thirdparty':
                    path=extras/entry['file']
                    if digest(path)!=entry['sha256']: raise ValueError('Changed upstream notice: '+label)
                    take(label,path)
                    record.setdefault('noticeSources',[]).append(entry)
            # Retain package author/copyright/SPDX metadata as well as license texts.
            notices.append('\nNuGet package metadata: '+json.dumps(record,indent=2)+'\n')
            nuget.append(record)
    text='Third-party notices for the binaries and compiled dependencies in this package.\nOriginal notices follow; consolidation does not change their terms.\n'
    (destination/'LICENSES.txt').write_text(text+''.join(notices),encoding='utf-8')
    return {'notices':inventory,'nuget':nuget,'triangleSource':triangle_source}


def package(args):
    native=Path(args.native).resolve(); meta=json.loads((native/'native-build.json').read_text(encoding='utf-8-sig'))
    version=args.version or (ROOT/'VERSION').read_text().strip()
    if not re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9.+-]{0,63}',version): raise ValueError('Invalid package version')
    if meta['os']!=host_os(): raise ValueError('Package on the target operating system so runtime verification is real')
    if meta['nativeAbi']!=ABI: raise ValueError('Native ABI mismatch')
    if args.release:
        if meta['configuration']!='Release' or meta['fastBuild']: raise ValueError('Release packages require optimized Release binaries')
        if not any(ROOT.glob('LICENSE*')): raise ValueError('A top-level ERod license is required before public release packaging')
        if not args.source_archive or not Path(args.source_archive).is_file(): raise ValueError('Public release packaging requires the corresponding source archive')
    out=Path(args.output).resolve(); out.mkdir(parents=True,exist_ok=True)
    suffix='' if args.release else '-preview'
    archive=out/f'ERod-{args.kind}-{meta["os"]}-{meta["architecture"]}-{version}{suffix}.zip'
    if archive.exists() and not args.replace: raise FileExistsError(f'{archive} exists; use --replace to regenerate')
    with tempfile.TemporaryDirectory(prefix='erod-package-',dir=out) as tmp:
        stage=Path(tmp)
        if meta['os']=='windows':
            forbidden={'kernel32.dll','kernelbase.dll','ntdll.dll','user32.dll','gdi32.dll','advapi32.dll'}
            if any(p.name.casefold() in forbidden for p in native.glob('*.dll')):
                raise ValueError('Native stage contains Windows system DLLs; regenerate the clean CMake install stage')
        for p in native.iterdir():
            # This TBB version supports std::malloc fallback; minimal Windows plugins omit the optional allocator.
            if args.kind=='plugin' and meta['os']=='windows' and p.name.casefold()=='tbbmalloc.dll': continue
            if p.is_file() and (p.suffix.lower() in ('.dll','.dylib') or p.name=='native-build.json'): shutil.copy2(p,stage/p.name)
        if args.kind=='native' and (native/'sdk').is_dir(): shutil.copytree(native/'sdk',stage/'sdk')
        managed_audit=None
        if args.kind=='plugin':
            plugin=Path(args.plugin or '').resolve()
            for name in ('ErodData.gha','ErodModel.gha','ErodDataLib.dll','ErodModelLib.dll'):
                if not (plugin/name).is_file(): raise FileNotFoundError(f'Missing managed plugin: {plugin/name}')
            managed_files,managed_audit=managed_closure(plugin,args.build_dir)
            for p in managed_files: shutil.copy2(p,stage/p.name)
            installers=ROOT/'scripts/installers'/meta['os']
            if installers.is_dir():
                for p in installers.iterdir(): shutil.copy2(p,stage/p.name)
        if binary_arch(stage/meta['library'])!=meta['architecture']: raise ValueError('Native architecture differs from package metadata')
        for path in list(native.glob('*.dll')) + list(native.glob('*.dylib')):
            if binary_arch(path)!=meta['architecture']: raise ValueError(f'Wrong dependency architecture: {path.name}')
        if meta['os']=='macos': relocate_macos(stage,args.identity)
        # Load a freshly staged copy in a separate process, with build-time paths absent.
        env=os.environ.copy()
        for name in ('DYLD_LIBRARY_PATH','DYLD_FALLBACK_LIBRARY_PATH','BOOST_ROOT','SUITESPARSE_ROOT'): env.pop(name,None)
        subprocess.run([sys.executable,__file__,'smoke',str(stage)],check=True,env=env,cwd=stage)
        licenses=copy_licenses(stage,args.build_dir)
        if args.kind=='plugin': (stage/'native-build.json').unlink()
        installation=(ROOT/'docs/INSTALL.md').read_text(encoding='utf-8')
        status='Public release candidate; see third-party notices and corresponding source archive.' if args.release else 'Preview: project/upstream redistribution permissions are not yet cleared.'
        (stage/'INSTALL.md').write_text(status+'\n\n'+installation,encoding='utf-8')
        if meta['os']=='macos':
            sums=''.join(digest(p)+'  '+p.relative_to(stage).as_posix()+'\n' for p in sorted(stage.rglob('*')) if p.is_file())
            (stage/'SHA256SUMS').write_text(sums)
        files={str(p.relative_to(stage)).replace(chr(92),'/'):digest(p) for p in sorted(stage.rglob('*')) if p.is_file()}
        manifest=dict(format=1,project='ERod',kind=args.kind,version=version,nativeAbi=ABI,os=meta['os'],architecture=meta['architecture'],
                      status='release' if args.release else 'preview',optimized=not meta['fastBuild'],files=files)
        manifest['nativeBuild']=meta
        manifest['dependencySources']=json.loads((ROOT/'cmake/dependencies.json').read_text(encoding='utf-8'))
        manifest['thirdParty']=licenses
        if managed_audit: manifest['managedDependencies']=managed_audit
        if args.source_archive: manifest['sourceArchive']={'name':Path(args.source_archive).name,'sha256':digest(args.source_archive)}
        (stage/'package.json').write_text(json.dumps(manifest,indent=2)+'\n')
        temporary=archive.with_suffix('.zip.part')
        with zipfile.ZipFile(temporary,'w',zipfile.ZIP_DEFLATED) as z:
            for p in sorted(stage.rglob('*')):
                if p.is_file():
                    info=zipfile.ZipInfo(p.relative_to(stage).as_posix())
                    info.create_system=3
                    info.external_attr=((0o100755 if p.suffix in ('.command','.sh','.dylib') else 0o100644)<<16)
                    info.compress_type=zipfile.ZIP_DEFLATED
                    z.writestr(info,p.read_bytes())
        temporary.replace(archive)
    archive.with_suffix('.zip.sha256').write_text(digest(archive)+'  '+archive.name+'\n')
    verify(archive)
    print(archive)

def inspect_archive(archive):
    z=zipfile.ZipFile(archive)
    names=z.namelist()
    if len(names)!=len({name.casefold() for name in names}): raise ValueError('Duplicate archive entries')
    if sum(i.file_size for i in z.infolist())>2*1024**3: raise ValueError('Archive is too large')
    for i in z.infolist():
        name=i.filename
        if name.startswith('/') or chr(92) in name or ':' in name or '..' in Path(name).parts or ((i.external_attr>>16)&0o170000)==0o120000:
            raise ValueError(f'Unsafe archive entry: {name}')
    manifest=json.loads(z.read('package.json'))
    if manifest['format']!=1 or manifest['nativeAbi']!=ABI: raise ValueError('Unsupported package format/ABI')
    expected=set(manifest['files'])|{'package.json'}
    if set(names)!=expected: raise ValueError('Unlisted or missing package files')
    for name,sha in manifest['files'].items():
        if hashlib.sha256(z.read(name)).hexdigest()!=sha: raise ValueError(f'Checksum mismatch: {name}')
    return z,manifest
def verify(archive):
    z,meta=inspect_archive(archive); z.close()
    print(f'PASS: {meta["kind"]} archive checksums and safe paths ({len(meta["files"])} files)')
def extract(archive,directory):
    z,meta=inspect_archive(archive)
    try:
        if meta['kind']!='native' or meta['os']!=host_os() or meta['architecture']!=host_arch(): raise ValueError('Choose the native package for this OS and architecture')
        destination=Path(directory); destination.mkdir(parents=True,exist_ok=True)
        z.extractall(destination)
    finally: z.close()
    smoke(destination)

def main():
    p=argparse.ArgumentParser(description=__doc__); sub=p.add_subparsers(dest='command',required=True)
    s=sub.add_parser('smoke'); s.add_argument('directory')
    s=sub.add_parser('verify'); s.add_argument('archive')
    s=sub.add_parser('extract'); s.add_argument('archive'); s.add_argument('directory')
    s=sub.add_parser('package'); s.add_argument('--kind',choices=['native','plugin'],required=True); s.add_argument('--native',required=True)
    s.add_argument('--plugin'); s.add_argument('--build-dir'); s.add_argument('--version'); s.add_argument('--output',default=str(ROOT/'dist'))
    s.add_argument('--release',action='store_true'); s.add_argument('--source-archive'); s.add_argument('--replace',action='store_true'); s.add_argument('--identity',default='-')
    a=p.parse_args()
    if a.command=='package': package(a)
    elif a.command=='smoke': smoke(a.directory)
    elif a.command=='verify': verify(a.archive)
    else: extract(a.archive,a.directory)
if __name__=='__main__': main()
