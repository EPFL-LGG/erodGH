"""Integration test: stale DLLs are pruned, resources retained, missing dependencies rejected."""
import importlib.util,shutil,sys,tempfile
from pathlib import Path
root=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('release',root/'tools/release.py')
release=importlib.util.module_from_spec(spec);spec.loader.exec_module(release)
plugin=Path(sys.argv[1]).resolve()
build=Path(sys.argv[2]).resolve() if len(sys.argv)>2 else root/'build/windows-source'
files,audit=release.managed_closure(plugin,build)
names={p.name for p in files}
assert 'Giraffe.ViewEngine.StrongName.dll' in names
assert 'Giraffe.ViewEngine.dll' not in names
assert 'System.Resources.Extensions.dll' in names
assert 'RhinoCommon.dll' not in names
assert any('System.Resources.Extensions' in a['resourceReferences'] for a in audit['assemblies'] if a['file'].endswith('.gha'))
print('PASS dependency closure: chart dependencies and embedded resource reader retained; stale DLL/host excluded')
with tempfile.TemporaryDirectory(dir=root/'build',prefix='missing-managed-') as temp:
    for name in ('ErodData.gha','ErodModel.gha'): shutil.copy2(plugin/name,Path(temp)/name)
    try: release.managed_closure(Path(temp),build)
    except ValueError as error:
        assert 'Missing managed dependency:' in str(error)
        print('PASS missing dependency rejected:',error)
    else: raise AssertionError('Incomplete plugin accepted')
