import importlib.util, json, hashlib, tempfile, zipfile
from pathlib import Path
repo=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('release',repo/'tools/release.py')
r=importlib.util.module_from_spec(spec); spec.loader.exec_module(r)
with tempfile.TemporaryDirectory(dir=Path(__file__).parent) as tmp:
    folder=Path(tmp)
    def archive(name, content=b'ok', extra=None):
        p=folder/name
        manifest={'format':1,'nativeAbi':r.ABI,'kind':'native','files':{'test.dll':hashlib.sha256(b'ok').hexdigest()}}
        with zipfile.ZipFile(p,'w') as z:
            z.writestr('test.dll',content); z.writestr('package.json',json.dumps(manifest))
            if extra: z.writestr(extra,b'bad')
        return p
    z,m=r.inspect_archive(archive('valid.zip')); z.close()
    for name,data,extra in [('corrupt.zip',b'bad',None),('traversal.zip',b'ok','../bad'),('unlisted.zip',b'ok','extra'),('duplicate.zip',b'ok','test.dll'),('case-duplicate.zip',b'ok','TEST.DLL')]:
        try: r.inspect_archive(archive(name,data,extra))
        except ValueError: print('PASS rejected',name)
        else: raise AssertionError(name)
print('PASS valid archive accepted')
