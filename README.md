# ERod

X-Shell / C-Shell / Curved-Weaving tools for Rhino and Grasshopper.

## Install a binary plugin

Use the **plugin ZIP** matching your OS and CPU. Extract it and keep its GHA and
DLL files together in one `ERod` folder in Grasshopper's Components folder
(File → Special Folders → Components Folder). Close Rhino before copying and
restart it afterwards. On Windows, the included `Install.cmd` can do the copying
and back up an existing installation for you; using the installer is optional.

End users need Rhino/Grasshopper, not Python, CMake or a compiler. Windows users
may also need Microsoft's Visual C++ x64 Redistributable. See
[installation instructions](docs/INSTALL.md) for runtime requirements.

The Windows plugin ZIP has a flat layout: the ERod assemblies, their required
DLL dependencies, `Install.cmd`, `Install.ps1`, `INSTALL.md`, `package.json`, and
`LICENSES.txt`. It contains no developer SDK, debug symbols, translation folders,
Windows system DLLs, or embedded Triangle source ZIP. `LICENSES.txt` preserves
third-party notices and records the pinned Triangle source reference.

The **native ZIP** is for developers integrating the C++ library. It contains
the native runtime, an `sdk` folder with an import library/header where available,
and license/build information. It is not a complete Grasshopper plugin.

Windows x64 packages have been tested locally. The macOS scripts are provided
for Intel and Apple Silicon builds but still need validation on Mac hardware.
No public release has been published; current packages are previews pending
project/dependency redistribution permissions. See [distribution details](docs/DISTRIBUTION.md).

## Build from source

Clone with Git, open a terminal in the repository root, and install:

| Requirement | Windows | macOS |
|---|---|---|
| Native compiler | Visual Studio 2022, Desktop development with C++ | Xcode command-line tools |
| Build tools | CMake 3.20+ | CMake 3.20+, Ninja, Python 3.12+ |
| Managed compiler | .NET SDK 8+ | .NET SDK 8+ |
| ZIP packaging | Python 3.10+ | Uses the builder's Python 3.12+ |

Rhino is needed to run the components, but not to compile them. Dependencies are
downloaded on the first build, so an internet connection is required. Boost and
SuiteSparse are built from pinned source archives. Windows also builds reference
BLAS/LAPACK; macOS uses Accelerate. Preinstalled Boost/SuiteSparse, a Fortran
compiler, Mono, Visual Studio for Mac and a Knitro license are not required.

### Windows

```powershell
# Optimized native and managed build, followed by the native smoke test
.\build-windows.cmd

# Build and create cleaned native + plugin preview ZIPs in dist/
.\build-windows.cmd -Package

# Faster compilation for development (reduced native optimization)
.\build-windows.cmd -FastBuild

# C++ only; add -Package to create only the native ZIP
.\build-windows.cmd -NativeOnly
```

Python is required only when packaging. If it is not available as `python`, pass
`-PythonExecutable "C:\path\to\python.exe"`. The standard binary output is always `bin/erod`; intermediate files stay under `build`. Only when explicitly needed, use `-OutputDirectory` for a separate
build-output folder, `-BuildDirectory` for native/dependency caches, and
`-PackageDirectory` for ZIPs. `-Configuration Debug` selects a debug build;
optimized `Release` is the default.

### macOS

```sh
bash build-macos.sh
bash build-macos.sh --package
bash build-macos.sh --fast
bash build-macos.sh --native-only
```

Install Xcode command-line tools with `xcode-select --install` and ensure CMake,
Ninja, Python and the .NET SDK are on PATH. `--build-dir`, `--output` and
`--package-dir` select custom locations. Build Intel and ARM libraries on their
matching hosts; do not mix architectures in one build directory.

### Where files go

| Folder | Contents |
|---|---|
| `bin/erod` | Raw development build output; may include PDBs or optional DLLs |
| `build/windows-source` | Windows dependencies, caches and CMake build |
| `build/macos-arm64` or `build/macos-x86_64` | macOS dependencies, caches and CMake build |
| `build/<platform>/stage/native` | CMake native install used by the packager |
| `dist` | Cleaned distribution ZIPs and `.zip.sha256` files |

Distribute the generated ZIPs rather than the raw `bin/erod` folder. The packager
follows managed assembly references, including embedded resource readers, and
selects only the required runtime DLLs. The minimal Windows plugin uses TBB's
standard-allocator fallback and omits optional `tbbmalloc.dll`; the native
developer ZIP retains that allocator. Package file counts can change when
dependencies change.

`-Package` / `--package` regenerates same-version preview ZIPs in the chosen
package directory. It does not publish them. For public release checks, signing,
source availability and standalone packaging, see [binary distribution](docs/DISTRIBUTION.md).

## Build against a prebuilt native library

```powershell
.\build-windows.cmd -NativePackage path\to\ERod-native-windows-x64-VERSION-preview.zip
```

```sh
bash build-macos.sh --native-package path/to/ERod-native-macos-arm64-VERSION-preview.zip
```

These commands verify the archive and build only the managed projects using the
.NET SDK. The native ABI, OS and CPU must match. `-ManagedOnly` / `--managed-only`
instead reuses native libraries already present in the selected output folder.
The convenience packaging flag requires a full source build (or native-only
source build), rather than either of these reuse modes.

## Validation

Both build helpers run native smoke tests. With Rhino 8 installed on Windows,
run these checks against your selected output folder:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-plugin-windows.ps1 -BinDirectory "$PWD/bin/erod"
```

This loads the component types, decodes embedded icons, renders a chart to HTML
without opening a browser, and constructs an SSH client without connecting.
Full interactive Grasshopper workflows and actual remote SSH transfers still
need their own testing. Custom native/dependency cache locations can be passed
to this check with `-BuildDirectory`.

## Advanced CMake and notebooks

The top-level CMake project uses standard Debug/Release configurations and
exposes `EROD_BUILD_PYTHON`, `EROD_FAST_BUILD`, `EROD_DEPS_PREFIX` and
`EROD_OUTPUT_DIRECTORY`. Native staging uses `cmake --install`; the helpers also
handle source dependency bootstrap and managed compilation.

Python bindings/notebooks are an optional legacy workflow, disabled by default.
Their historical notebook package pins are not maintained by the plugin build.

### Native interop changes in 0.2.0

Use matching ABI 2 native and managed binaries. Models and materials now support deterministic disposal and independent duplication. See [ownership, compatibility and regression tests](docs/INTEROP.md).

### Live equilibrium report

Enable **Live report (Log)** on any solver component to view live optimization rows: a separate report window on Windows or the system Terminal app on macOS. See [usage and limitations](docs/LIVE-REPORT.md).
