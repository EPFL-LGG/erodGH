# Build output conventions

- The canonical runtime binary output is `bin/erod` on both Windows and macOS.
- Use the build helpers' default output directory for builds, fixes and validation. Do not redirect the final binaries to a testing folder unless the user explicitly requests a different location.
- Keep compiler intermediates, dependency caches and test executables under `build`. Distribution archives belong in `dist` or an explicitly requested delivery directory.
- After changing native or managed runtime code, update and validate the matching binaries in `bin/erod` before reporting completion.
