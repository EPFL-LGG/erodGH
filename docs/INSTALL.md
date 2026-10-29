# Install a prebuilt ERod plugin

Choose the **plugin** ZIP for your OS and CPU. The tested Windows package targets
x64. macOS Intel (x64) and Apple Silicon (arm64) packages must be built and tested
on their respective Macs. A **native** ZIP contains the C++ runtime for developers;
it is not a complete Grasshopper plugin.

## Manual installation

1. Extract the entire plugin ZIP into a folder named `ERod`.
2. In Grasshopper, open File → Special Folders → Components Folder.
3. Close Rhino and move the `ERod` folder into that Components folder.
4. Restart Rhino and open Grasshopper.

Keep the GHA files and DLL dependencies together. Replace an old installation
instead of keeping a second copy in another Components/Libraries folder.

## Optional installers (packaged plugin ZIPs only)

These helpers are added when creating a **plugin ZIP**. They are not copied to
`bin/erod` by a normal build and are not included in native ZIPs.

| Platform | Helper in the extracted plugin ZIP | Source in this repository |
|---|---|---|
| Windows | `Install.cmd` and `Install.ps1` | `scripts/installers/windows/` |
| macOS | `Install.command` | `scripts/installers/macos/` |

Run the helper from the extracted plugin ZIP, not its source folder: it needs
the packaged binaries and metadata beside it. For local development builds,
use the binaries in `bin/erod` as described in the README.

On Windows, close Rhino and double-click `Install.cmd` from the extracted folder.
It verifies the package, copies files to your user's Grasshopper Libraries folder,
and backs up an existing ERod installation outside that folder. It does not
compile code or require administrator access.

On macOS, first locate the Components folder through Grasshopper's File menu,
then close Rhino. Run `Install.command` and select that folder when asked. The
installer does not remove quarantine attributes or change OS security settings.

## Requirements and package contents

Rhino/Grasshopper is required. Visual Studio, CMake, Python and a compiler are not.
The managed assemblies target .NET Framework 4.8. On Windows Rhino 8, use
`Rhino.exe /netfx` for the compatibility runtime if needed.

Windows native libraries use Microsoft's C++ runtime. If Windows reports a missing
`VCRUNTIME140.dll`, install Microsoft's supported x64 Redistributable:
https://aka.ms/vs/17/release/vc_redist.x64.exe

The Windows plugin has a flat layout. The DLL/GHA files are runtime code;
`INSTALL.md`, `LICENSES.txt` and `package.json` provide instructions, notices and
verified package metadata. `Install.cmd` / `Install.ps1` are optional helpers.
No SDK, PDBs, translation folders, Windows system DLLs or nested source ZIP are
included. The native developer ZIP has a separate SDK folder.

`LICENSES.txt` contains the original third-party license/attribution notices and
Triangle's pinned source URL and local source hashes. Keep it with redistributed
binaries; it does not need to be opened for Grasshopper to run.

Preview packages have not been cleared for public redistribution. Project and
dependency permissions still need confirmation. Official macOS releases also
need appropriate signing/notarization. See the repository's `docs/DISTRIBUTION.md`.
