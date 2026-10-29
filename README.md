# ERod

X-Shell / C-Shell / Curved-Weaving tools for Rhino and Grasshopper.

## Build from source

Clone the repository with Git and run the commands below from its root folder.

| Tool | Windows | macOS |
|---|---|---|
| C++ compiler | Visual Studio 2022 with Desktop development with C++ | Xcode command-line tools (`xcode-select --install`) |
| Native build tools | CMake 3.23+ | CMake 3.23+ and Ninja |
| C# compiler | .NET SDK 8+ | .NET SDK 8+ |
| Python | Python 3.10+ for ZIP packaging only | Python 3.10+ for the build helper |

On macOS, use an updated Python maintenance release with `tarfile.data_filter`
(safe archive extraction). To select an interpreter, prefix any build command
with `PYTHON=/path/to/python3`.

The first build downloads dependencies, including Boost and SuiteSparse.
Rhino is required to run the plugin, but not to compile it. Build for your host's
CPU architecture and keep Intel and ARM build caches separate.

### Full build

Windows (PowerShell):

```powershell
.\build-windows.cmd
```

macOS (Terminal):

```sh
bash build-macos.sh
```

Both commands build C++ and C#, then run a native smoke test.

### Development: build only what changed

Run a full build once, then use:

| What changed | Windows (PowerShell) | macOS (Terminal) |
|---|---|---|
| C++ only | `.\build-windows.cmd -NativeOnly` | `bash build-macos.sh --native-only` |
| C# only | `.\build-windows.cmd -ManagedOnly` | `bash build-macos.sh --managed-only` |
| C++ and C# | `.\build-windows.cmd` | `bash build-macos.sh` |

**C++ only** rebuilds the native library and leaves existing C# assemblies in
place. It does not require the .NET SDK.

**C# only** rebuilds the managed solution and reuses the native libraries already
in `bin/erod`. It skips C++ compilation and dependency bootstrap, but still runs
the native smoke test, so those native libraries must be present. It requires
.NET SDK 8+ and, on macOS, Python for the helper.

Choose what to rebuild based on the change:

- **C++ only:** change solver calculations or other internal C++ implementation,
  without changing how C# calls it.
- **C# only:** change Grasshopper components, UI, or managed logic, while using
  the existing C++ functions and data formats.
- **Both:** change how C# and C++ exchange data, or make changes in both languages.

The **native/managed interface** is the boundary where C# calls the C++ library:
function names, arguments, return types, and shared data layouts. For example,
adding an argument to a C++ function called from C# requires updating the C#
declaration and rebuilding both. Mismatched binaries can cause errors or crashes.

Close Rhino before rebuilding, then restart it to load the updated plugin.
Keep the files from `bin/erod` together when updating your Grasshopper installation.

Useful options (append to the relevant command):

| Purpose | Windows | macOS |
|---|---|---|
| Faster C++ compilation with reduced optimization | `-FastBuild` | `--fast` |
| Debug build (default is Release) | `-Configuration Debug` | `--configuration Debug` |
| Build and create preview ZIPs | `-Package` | `--package` |

Packaging works with a full build or C++-only build, not C#-only mode.

### Build output

| Folder | Purpose | When to use it |
|---|---|---|
| `bin/erod/` | Compiled C++ libraries and C# plugin assemblies. | Use these binaries to test your latest changes in Rhino and Grasshopper. |
| `build/` | Downloaded dependencies, build caches and intermediate files. | Used by the build scripts; keep it between builds to speed up compilation. |
| `dist/` | ZIP packages ready for distribution. | Created with `-Package` / `--package`; share these with other users. |

## Live solver reports

Set a solver's **Live report (Log)** input to `True` before enabling **Run**.
Iteration rows appear in Terminal on macOS or a separate report window on
Windows. Closing the report window does not stop the solver.

## More information

- [Installation](docs/INSTALL.md)
- [Packaging and distribution](docs/DISTRIBUTION.md)
- [Live reports](docs/LIVE-REPORT.md)
- [Native/managed interop and regression tests](docs/INTEROP.md)
- [SuiteSparse configuration and compatibility](docs/SUITESPARSE.md)
