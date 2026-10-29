# Live solver reports on Windows and macOS

All eight solver components have an optional Boolean **Live report** (`Log`) input, default `false`: EquilibriumSolver, EquilibriumSolverMulti, CableActuation, SliderActuation, SupportActuation, TorqueActuation, TorqueActuationMulti and TorqueActuationSteps. Enable Log before Run.

On Windows, reports appear in the existing separate WinForms window. On macOS, reports open in the system Terminal app. A background writer copies the same native iteration rows into a bounded temporary log, and an executable `.command` reader displays updates in Terminal. This keeps the viewer independent of Rhino's blocked UI without pumping Rhino messages or creating Cocoa controls on a background thread. It requires no Python installation or AppleScript automation. Apple's [shell-script guide](https://support.apple.com/en-mide/guide/terminal/apd53500956-7c5b-496b-a362-2845f2aab4bc/mac) explains the underlying executable-script mechanism.

Rows contain iteration, energy, gradient norm, free-gradient norm, step length and Hessian indefinite status (0/1). Model numbers and deployment steps identify batches. Iteration counts restart for each native solve. This streams optimizer samples and component completion/errors, not every native stdout/stderr diagnostic. The ordinary Report outputs and verbosity/report options remain available.

Closing the viewer does not cancel the solve. The solve remains synchronous; this feature does not add solver cancellation or process isolation. Toggle Log off/on, reset, or stop/restart to open another viewer. Reset starts a fresh report. Disabling Log, removing the component, or closing/unloading its document closes the Windows window; on macOS it signals the reader to finish, leaving its completed Terminal window according to Terminal preferences. The reader also stops if Rhino exits. Closing Terminal itself leaves background logging active until the component closes its stream. Temporary session files live under the OS temporary directory's `ERod-reports` folder and can be removed after closing Rhino. Per-session log history is bounded to approximately 2 MiB.

Callbacks queue copied strings and do not touch Rhino geometry or Grasshopper data. Registration is local to the calling solver thread and cleared after exceptions. Do not mutate native models from a logging callback. Native dependencies are not assumed safe for simultaneous solves. Viewer-launch errors are contained; the component reports detected launch failures as warnings.

Install matching updated managed and native code. The optional ABI-2 export `erodSetSolverLogCallback` must be present; old ABI-2 DLLs without it cannot supply live rows. The original managed Optimize overload and component GUIDs/input indices/outputs are preserved.

On a Mac, build the updated source with:

```sh
bash build-macos.sh
```

Its binaries go to `bin/erod`. The Windows files built on this machine are not macOS native binaries.

## Validation

Windows: 18 managed interop groups, all eight component schema checks, and standalone window paint/close/disposal tests pass. Mac backend tests run with a substituted launcher: streaming before disposal, complete pending-row drain, history bounds, stop signaling, literal shell quoting and containment of launch failure all pass. A managed build with the non-Windows configuration also compiles.

Actual Terminal launch, macOS shell behavior and an interactive Rhino/macOS solve remain unverified on this Windows host. The POSIX reader integration test was added, but Git Bash cannot start inside this host's sandbox (access denied creating MSYS named objects). These checks are not a substitute for testing the reader and LaunchServices on a Mac.

Developer tests:

```powershell
dotnet build tests/live-console/Erod.LiveConsole.Tests.csproj -c Release -o build/live-console-tests
.\build\live-console-tests\Erod.LiveConsole.Tests.exe
powershell -File tests/test-solver-components-windows.ps1
```

`tests/test-mac-report-reader.py` can additionally exercise the generated reader under a POSIX shell, using `--tests-exe`, `--shell` and (when needed for net48) `--mono`. It uses filenames containing spaces, apostrophes and dollar signs, then checks live rows and clean stream shutdown.
