# Native memory isolation

This is a standalone Unity 6000.6.0f1 experiment, not a Luna runtime change. The generator writes a new marked project under `Logs/MemoryIsolation/<RunId>/Project`; it never overwrites an existing run or deletes a Library directory. No Luna, GameFramework, Addressables, Spine, or user save files are copied. The default pipeline is builtin; the explicit URP variant adds only URP and its declared package dependencies.

Only the root operator runs Unity. Every process uses batch mode and a hidden window, and raw logs remain unfiltered. `operationCompleted` means the requested build or finite player run completed. It does **not** assert that shutdown was free of native allocations. Read `closed-log-evidence.json` and its full `process.log` after process exit.

Initial experiment (3 seconds, empty scene, builtin engine, Mono development player matching Luna's Development-only build flags):

```powershell
./Tools/validation/Memory/New-MemoryIsolation.ps1 -RunId empty-ng-01
./Tools/validation/Memory/Invoke-MemoryIsolation.ps1 -RunId empty-ng-01 -Mode Build -NoGraphicsEditor
./Tools/validation/Memory/Invoke-MemoryIsolation.ps1 -RunId empty-ng-01 -Mode Player
./Tools/validation/Memory/Invoke-MemoryIsolation.ps1 -RunId empty-ng-01 -Mode Player -NoGraphicsPlayer
```

Compare a fresh run built with graphics enabled (omit `-NoGraphicsEditor`). The `-Scene builtin-shader` variant adds a camera, audio listener, serialized quad mesh and material without a physics module. It selects builtin Unlit/Color or URP/Unlit according to the project's pipeline.

After the builtin baseline, isolate URP 17.6.0 with public asset creation APIs:

```powershell
./Tools/validation/Memory/New-MemoryIsolation.ps1 -RunId urp-ng-01 -Pipeline urp -UrpVersion 17.6.0
./Tools/validation/Memory/Invoke-MemoryIsolation.ps1 -RunId urp-ng-01 -Mode Build -Scene builtin-shader -NoGraphicsEditor
./Tools/validation/Memory/Invoke-MemoryIsolation.ps1 -RunId urp-ng-01 -Mode Player
```

`MemoryIsolationUrpBuilder` persists a `UniversalRendererData` and `UniversalRenderPipelineAsset.Create(rendererData)`, then sets `GraphicsSettings.defaultRenderPipeline` and `QualitySettings.renderPipeline` before the shared build. No private fields or reflection are used. All Assets, Packages and ProjectSettings files are hashed at each launch; `created.json` remains the historical creation snapshot, while `launch.json` records the actual subsequently edited inputs.

For a shader-only Spine contrast, `New-MemoryIsolation.ps1 -Pipeline urp -SpineShaders` copies only the exact local `com.esotericsoftware.spine.urp-shaders-4.0-2019.1` package into the isolated project's embedded package folder. It rejects any C# source in that package and verifies every copied file hash. No Spine runtime or Luna scripts are copied. The original package's URP 7.1.5 minimum dependency is preserved; the explicit project manifest pins URP 17.6.0. Record the actually resolved version from the isolated `packages-lock.json` as well.

Build using `-Scene builtin-shader -ShaderName 'Universal Render Pipeline/Spine/Sprite'`. The existing material is explicitly rebound to the requested shader, so a warm project cannot silently keep the previous URP/Unlit shader. The operation report records the actual material shader. Reusing a project/library is allowed only by an operator copying the updated template and exact package deliberately; launch hashes retain those changes without rewriting earlier evidence.

Each attempt records executable/version, complete arguments, source hashes, managed operation report, and the full closed process log. The parser keeps **every** UTP record, all memory labels, diagnostic candidate lines and allocation call stacks. It distinguishes repeated reports of the same address/size/block from unique allocations without dropping either report. It does not sum UTP labels into `allocatedMemory` or assign those engine aggregates to managed gameplay.

Diagnostic controls:

- `-stackTraceLogType Full` is documented in the installed Unity 6.6 Editor manual.
- `-log-memory-performance-stats` is documented there and is used for Editor builds.
- `-native-leak-detection EnabledWithStackTrace`, `-diag-temp-memory-leak-validation` and `-diag-job-temp-memory-leak-validation` are engine diagnostic switches verified against the installed engine (the job flag already emitted shader native stacks in Luna).
- The code explicitly sets the public `NativeLeakDetection.Mode = EnabledWithStackTrace`. The installed Collections package's `CLILeakDetectionSwitcher.cs` documents `UNITY_JOBS_NATIVE_LEAK_DETECTION_MODE=2`; the isolated project does not need that package because it sets the public API directly.
- `-CaptureSnapshot` requests a public `Unity.Profiling.Memory.MemoryProfiler.TakeSnapshot` before quitting, without opening a window. This is opt-in because the capture itself changes the experiment. Installed 6.6 API documentation states native allocation sites and native stack traces require a source-engine build and are ignored when unsupported. The harness therefore requests only managed objects, native objects and native allocations, not unsupported full ownership stacks.

Authoritative local references are under `C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Data/Documentation/en/`: `Manual/EditorCommandLineArguments.html`, `ScriptReference/Unity.Collections.NativeLeakDetectionMode.EnabledWithStackTrace.html`, and `ScriptReference/Unity.Profiling.Memory.MemoryProfiler.TakeSnapshot.html`.

Boundaries: an empty project reproducing a warning demonstrates that Luna is not necessary for that warning, but does not by itself establish harmlessness. Different UTP aggregates are not a leak rate. Compare same-engine, same-build, same-graphics experiments and retain startup/shutdown infrastructure diagnostics independently from managed runtime warnings.

## Explicit closed-process gate and optional cache workaround

`Validate-Luna.ps1 -CheckNativeMemory` checks the entire log **after the owned Unity process exits**, before printing a passing result. It rejects explicit JobTempAlloc lifetime/leak diagnostics, persistent-allocation leak diagnostics, unfreed TLS allocations, undisposed Native Collections and ComputeBuffer finalizer warnings. Every occurrence and native allocation stack remains in the `.memory.json` evidence. UTP records and their memory labels are retained separately; they do not automatically fail this narrow gate, and passing it does not declare those allocations harmless or prove absence of all native leaks.

Each checked process keeps an immutable log/report snapshot under `Logs/Revival/NativeMemory/<unique UTC operation>/`, including failed processes. `Logs/Revival/<operation>.memory.json` points to the latest parsed evidence. Managed report freshness, exit code and exact expected-warning claims still have to pass independently.

```powershell
./Tools/validation/Validate-Luna.ps1 -Mode BuildWindows -CleanBuildCache -JobLeakDiagnostics -RebuildShaderCache -CheckNativeMemory
```

`-RebuildShaderCache` is an opt-in build workaround, not an engine fix. Immediately before BuildWindows it calls `Backup-ShaderCache.ps1` with a unique UTC run ID, moving only this project's `Library/ShaderCache` to a verified, reversible backup. If that directory is already absent, there is nothing to move. No other Library folder is deleted or changed by the workaround. The cold-build observation does not imply a later warm build is fixed; keep testing the full closed log. Neither option is enabled by default.

`Validate-Player.ps1 -CheckNativeMemory` applies the same gate after each seed/verify/diagnostic process closes. Its unique `PlayerValidation/<RunId>/attempts/` directory retains the closed log, managed report, memory evidence and exit metadata even on failure. The latest evidence is also available as `player-<mode>.memory.json` beside the live-run report.

`Final-Review.ps1 -CheckNativeMemory -RebuildShaderCache` forwards the memory gate to every Editor and Player step, and the cache option only to BuildWindows in pass 3. Both switches remain off by default. A RunId cannot mix completed passes with different values of these options. Final-review archives include each Editor `.memory.json` and the complete Player evidence directory.

Existing logs can be gated without launching Unity:

```powershell
./Tools/validation/Memory/Assert-MemoryLog.ps1 -LogPath 'path/to/closed.log' -OutputPath 'path/to/evidence.memory.json'
```
