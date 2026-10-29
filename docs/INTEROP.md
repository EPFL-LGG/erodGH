# Managed/native interop and ownership

The 0.2.0 preview requires native ABI **2** and matching managed assemblies (assembly version 2.0.0.0). Replace the complete plugin package together. Older native DLLs are rejected before model calls.

Models and materials implement `IDisposable` and own native objects through `SafeHandle`. Use `using` or `Dispose()` when their useful lifetime ends; finalizers provide fallback cleanup. Model and material `Duplicate()` operations create independent native copies. A copied Material struct shares its handle; call `Duplicate()` for independent ownership.

Joint and segment views borrow their parent model. They keep it alive through garbage collection, but explicit disposal of the parent invalidates them. Native calls retain and lock the owning handle for their duration so concurrent disposal cannot free an active object. The legacy `Model` pointer is a borrowed, read-only address, not an ownership transfer. Kernel object parameters now use `NativeHandle`.

Native constructors register owned objects. Release those objects with `erodDestroyObject`, never `erodFree`. Returned arrays use `erodFree`; `erodGetBufferSize` reports their byte length. Managed buffer scopes free returned arrays even if conversion or a later operation throws. Solver reports use the actual returned sample count (`ConvergenceReport.SampleCount`), including the initial sample.

Native entry points translate C++ exceptions into thread-local error text. Direct C callers must check and copy `erodGetLastError()` immediately after each operation. Managed wrappers raise `NativeInteropException`. Pointer/count pairs remain a contract for direct native callers; arbitrary invalid memory addresses cannot be made safe by this interface.

The changes also correct signature/count-width mismatches, fractional strain results, custom contour offsets and box coordinate packing. Arbitrary custom contour interpolation between adjacent joints remains unsupported by the upstream material implementation and reports an exception. A failed mutating operation can have partially changed the model; discard it if recovery is uncertain.

## Regression checks on Windows

After building the plugin:

```powershell
dotnet build tests/managed-interop/Erod.Interop.Tests.csproj -c Release -o build/managed-interop-tests
.\build\managed-interop-tests\Erod.Interop.Tests.exe <absolute-plugin-output-directory>
```

The 18 managed groups cover errors, buffer cleanup, ownership, cloning, borrowed views, finalizers, concurrent disposal, solver history, periodic rods, vector ABI, geometry packing and unsupported contour errors. They check that tracked native object and buffer counts return to zero.

`tests/native-interop` contains a Windows loader harness with AddressSanitizer enabled by default. Build it with CMake and run its executable with an instrumented native DLL directory followed by `buffers`, `error`, `solver`, `bad-dofs`, `strain` or `lifetime`. Instrument the native library as well as the harness; an instrumented harness alone does not check library accesses. Make the Visual C++ ASan runtime available on PATH. These checks do not prove that every upstream allocation is leak-free.

Windows was tested. macOS builds and interactive Rhino/Grasshopper sessions still require platform validation. The developer import library is now `erod-abi2.lib`; the runtime remains `erod.dll`.

## In-process hardening

The rod/linkage core uses explicit `EROD_REQUIRE` runtime checks in place of 81 diagnostic assertions. These evaluate once, remain active with `NDEBUG`, and throw descriptive C++ errors containing the condition and source location. The existing ABI boundary copies the error into a managed exception. The checks cover core indices, dimensions, topology assumptions, sparse-matrix contracts and target-surface state. Compile-time `static_assert` checks are unchanged.

Centerline validation rejects nonfinite points/rotations, zero-length edges and singular antiparallel adjacent tangents before updating a deformed state. Callers should discard a model after an internal consistency failure; a multi-part mutation can have partially completed.

Build the native regressions with `-DEROD_BUILD_HARDENING_TESTS=ON`, build target `erod-hardening-tests`, then run `ctest -C Release --output-on-failure` in the CMake build directory. Keep TBB and the native dependency DLLs on PATH on Windows. The test deliberately violates core checks (including a TBB task), catches the errors, and verifies a subsequent valid operation. It must also pass with `NDEBUG` defined. The loader harness additionally accepts `hardening` to check errors and cleanup through the exported ABI. The managed harness now has 18 groups, including translation of these native failures.

This is targeted hardening, not a global redefinition of C/C++ or Eigen assertions. Other dependency assertions and termination paths (including Triangle's C `triexit`) remain outside this conversion. Throwing through arbitrary C frames, destructors or `noexcept` functions is not a safe general recovery mechanism. Unexpected memory corruption, access violations and dependency termination can still bring down Rhino. No process boundary or extra request/response overhead was introduced.
