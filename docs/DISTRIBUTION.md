# Build and distribute binaries

## One-command preview packages

From the repository root, after installing the source-build prerequisites:

```powershell
.\build-windows.cmd -Package
```

```sh
bash build-macos.sh --package
```

The helpers build optimized Release binaries, run the native smoke test, and
generate **native** and **plugin** preview ZIPs plus external SHA-256 checksum
files in `dist`. Same-version archives are replaced. Add `-NativeOnly` or
`--native-only` to produce only the native ZIP. Do not combine the convenience
packaging flag with managed-only or native-package reuse modes.

Windows packaging requires Python 3.10+ (`-PythonExecutable` selects an explicit
interpreter); macOS uses the builder's Python 3.12+. Plugin packaging also needs
the .NET SDK to run its metadata scanner. End users need none of those tools.
`-PackageDirectory` / `--package-dir` overrides `dist`.

Build output and distribution output are different: `bin/erod` can contain debug
symbols and optional DLLs. The packager stages a fresh copy and follows assembly
references from both GHAs, including embedded resource-reader dependencies.
It excludes host-provided Rhino/Grasshopper/.NET Framework assemblies and unused
build-output DLLs. CMake collects the native dependency closure.

## ZIP contents

The Windows **plugin ZIP** is flat and contains:

- Both GHA assemblies, ERod's managed bindings, and required managed/native DLLs.
- `Install.cmd` and `Install.ps1` for optional installation and update backups.
- `INSTALL.md`, `LICENSES.txt` and `package.json`.

It omits PDBs, SDK files, optional translations, Windows system DLLs and an
embedded Triangle source ZIP. Optional `tbbmalloc.dll` is omitted on Windows;
TBB's standard-allocator fallback has been tested. The macOS package retains
its allocator and adds `Install.command` and `SHA256SUMS`; Mac validation remains
outstanding.

The **native ZIP** contains `erod.dll` or `liberod.dylib`, its non-system runtime
dependencies (including TBB's allocator), `native-build.json`, `INSTALL.md`,
`LICENSES.txt`, `package.json`, and an `sdk` folder where available. It is intended
for C++ integration, not direct Grasshopper installation.

`LICENSES.txt` consolidates original notices without changing their terms.
NuGet notices are selected by the hashes of the DLLs actually shipped. Used
SuiteSparse components' notices are included; unused components' notices are
omitted. Pinned additional upstream notices are in `third_party/notices`.
Triangle's upstream URL and local source SHA-256 hashes are recorded in
`LICENSES.txt` and `package.json`; no Triangle source ZIP is embedded.

`package.json` records OS, CPU, ABI, version, optimization status, dependency
information and file checksums. External `.zip.sha256` files let recipients
verify the download. Checksums detect corruption; obtain them from a trusted
publisher. Runtime file counts can change when dependencies change.

## Package an existing source build

Use a native stage and managed output from the same build. Windows:

```powershell
python tools/release.py package --kind native --native build/windows-source/stage/native --build-dir build/windows-source
python tools/release.py package --kind plugin --native build/windows-source/stage/native --plugin bin/erod --build-dir build/windows-source
```

macOS Apple Silicon:

```sh
python3 tools/release.py package --kind native --native build/macos-arm64/stage/native --build-dir build/macos-arm64
python3 tools/release.py package --kind plugin --native build/macos-arm64/stage/native --plugin bin/erod --build-dir build/macos-arm64
```

Use `build/macos-x86_64` on Intel. Pass the actual paths when using custom build
locations. `--output` selects the ZIP directory, `--version` overrides `VERSION`,
and `--replace` permits overwriting same-version archives. Native staging is
produced by the source build; a raw managed-only output folder is not a substitute.

Packaging validates architecture and loads the staged native runtime in a fresh
process before writing the ZIP. On macOS it also relocates dylib dependencies
and signs them ad hoc. `--identity` selects a signing identity; notarization is
separate. Build and validate each platform on its matching host.

## Verify packages and test the plugin

```sh
python tools/release.py verify path/to/ERod-plugin-windows-x64-VERSION-preview.zip
```

With Rhino 8 installed on Windows:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-plugin-windows.ps1 -BinDirectory "$PWD/bin/erod"
```

This checks component types, embedded resources, chart HTML generation and SSH
client construction. It does not make a remote connection or open a browser.
Use `-BuildDirectory` when the framework reference-assembly cache is elsewhere.
Also test real Grasshopper workflows and any SSH features before publication.

The regression scripts `tests/test_package_validation.py`,
`tests/test_managed_packaging.py <managed-output-folder>` and
`tests/test_tbb_fallback.py <minimal-plugin-folder>` cover archive validation,
dependency selection and the Windows allocator fallback.

## Release and license status

Archives are previews by default. Standalone `--release` packaging requires
optimized Release binaries, a top-level ERod license and a corresponding source
archive passed with `--source-archive`. The source archive is a separate release
asset; it is not nested inside the plugin ZIP. These checks do not determine
whether every redistribution condition has been satisfied.

This checkout has no top-level ERod license. Permissions for the forked MeshFEM
and ElasticRods code also need confirmation; licenses from other revisions must
not silently be assigned to these forks.

Triangle's bundled README requires preserving notices, receiving no compensation,
making modified source/object code freely available without charge and identifying
modifications. Commercial-system distribution requires an arrangement with the
author: https://www.cs.cmu.edu/~quake/triangle.html

The Triangle C source/header used for the tested Windows build match the pinned
upstream archive. Supply matching source separately whenever redistribution terms
require it, including after modifications. SuiteSparse components have their own
terms; inspect `LICENSES.txt` and provide applicable corresponding source with
public releases. Removing the nested source ZIP does not remove these obligations.

Before publication, confirm project/dependency permissions and supply matching
complete source. The build helpers do not publish or upload. No automated
publishing is configured, and the macOS scripts still need validation on Intel
and Apple Silicon Macs.

## ABI compatibility

The 0.2.0 preview uses native ABI 2 and managed assembly version 2.0.0.0. Distribute the matching package as a unit. See [interop ownership and validation](INTEROP.md). The Windows developer import library is `erod-abi2.lib`.
