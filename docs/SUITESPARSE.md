# SuiteSparse build and compatibility

ERod pins upstream SuiteSparse 7.14.1 in `cmake/dependencies.json`, with a
SHA-256-verified source archive. Both platforms build its native CMake project;
the former `suitesparse-metis-for-windows` archive and source overlay are no
longer used. CMake 3.23 or newer is required.

`cmake/SuiteSparseOptions.cmake` selects CHOLMOD and UMFPACK and their required
ordering/configuration libraries. CUDA, OpenMP, Fortran, demos and METIS
partitioning are disabled. macOS builds static libraries against Apple's
32-bit-integer Accelerate BLAS. Windows builds DLLs against the existing
source-built reference CLAPACK/BLAS (also 32-bit BLAS integers). Sparse matrix
indices remain 64-bit; BLAS integer size is a separate setting.

Dependencies install into `dependencies/<configuration>/install-suitesparse-7.14.1`
under the build directory. This separates the new headers, CMake packages and
libraries from previous SuiteSparse 5.8 installations. Old caches are left in
place. Runtime binaries still go to `bin/erod`.

MeshFEM uses the `cholmod_l_*` and `umfpack_dl_*` APIs. SuiteSparse 7 retains
`SuiteSparse_long` as an alias for `int64_t`. No production C++ API changes were
needed for this migration. The CMake integration now consumes the individual
upstream packages and `SuiteSparse::CHOLMOD` / `SuiteSparse::UMFPACK` targets.

## Validation

On macOS ARM64, the optimized native and managed build passed using Python
3.10.22. Tests cover both MeshFEM wrappers, supernodal BLAS operations, numeric
refactorization, non-positive-definite failure handling, and repeated UMFPACK
solves. Native hardening tests and nine dynamic-library interop cases passed,
including a stretched rod relaxing from energy 1.25 to zero, allocation/free
cycles, cloning, error handling and material assignment.

After a normal macOS build:

```sh
cmake -S . -B build/macos-arm64/native \
  -DEROD_BUILD_SUITESPARSE_TESTS=ON -DEROD_BUILD_HARDENING_TESTS=ON
cmake --build build/macos-arm64/native \
  --target erod-suitesparse-tests erod-hardening-tests --parallel 2
ctest --test-dir build/macos-arm64/native --output-on-failure

cmake -S tests/native-interop -B build/macos-arm64/interop-tests -G Ninja \
  -DEROD_TEST_ASAN=OFF -DEROD_TEST_BINARY_DIRECTORY="$PWD/bin/erod"
cmake --build build/macos-arm64/interop-tests
ctest --test-dir build/macos-arm64/interop-tests --output-on-failure
```

Use `macos-x86_64` instead of `macos-arm64` on Intel Macs. The interop checks above
are normal runtime tests, not a sanitizer-instrumented validation of the library.

Windows scripts are updated but require an actual Windows build and test run.
After `build-windows.cmd`, enable the same two CMake test options in
`build/windows-source/native`, build with `--config Release`, then run CTest with
`-C Release`. The portable interop harness can also be built on Windows with
`EROD_TEST_BINARY_DIRECTORY` set to the absolute `bin/erod` path.

The macOS native preview ZIP also passed relocation, runtime smoke tests, and
archive checksum/path validation.

The macOS plugin ZIP was subsequently built and validated with .NET SDK 8.0.427
(ARM64). Managed dependency closure and missing-dependency rejection passed.
The extracted plugin passed checksum validation, installer syntax and executable
permission checks, ad-hoc signature verification, native dependency relocation,
and all nine native interop cases with build-time library search paths removed.
The archive contains 30 files and no subfolders. Installer execution against a
real Grasshopper folder and interactive Rhino testing remain outstanding.

To repeat the managed packaging integration checks after a build:

```sh
python3 tests/test_managed_packaging.py bin/erod build/macos-arm64
python3 tests/test_package_validation.py
```

The SDK is a build/packaging prerequisite; it is not bundled in the plugin ZIP.
