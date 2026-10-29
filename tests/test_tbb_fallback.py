import ctypes,os,sys
from pathlib import Path
folder=Path(sys.argv[1]).resolve()
assert not (folder/'tbbmalloc.dll').exists()
with os.add_dll_directory(str(folder)):
    lib=ctypes.CDLL(str(folder/'tbb.dll'))
    used=getattr(lib,'?is_malloc_used_v3@internal@tbb@@YA_NXZ');used.restype=ctypes.c_bool;used.argtypes=[]
    assert used(), 'TBB did not select standard malloc fallback'
    allocate=getattr(lib,'?allocate_via_handler_v3@internal@tbb@@YAPEAX_K@Z');allocate.argtypes=[ctypes.c_size_t];allocate.restype=ctypes.c_void_p
    free=getattr(lib,'?deallocate_via_handler_v3@internal@tbb@@YAXPEAX@Z');free.argtypes=[ctypes.c_void_p]
    buffer=allocate(4096);assert buffer;free(buffer)
print('PASS TBB standard malloc fallback selected; allocation and release work without tbbmalloc.dll')
