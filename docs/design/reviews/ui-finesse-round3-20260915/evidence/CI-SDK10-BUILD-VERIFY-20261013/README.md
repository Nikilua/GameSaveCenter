# SDK 10.0.401 CI build/test verification

## Purpose

The repository baseline remains pinned to .NET SDK `8.0.423` in `global.json`. The user's CI gate also requires an actual SDK `10.0.401` build/test result; a local SDK 9 run or the existing SDK 8 Actions job cannot satisfy it. This change adds a separate Windows Actions job for SDK `10.0.401` and preserves the baseline job.

The SDK 10 job creates a detached temporary worktree at the checked-out commit, changes only that worktree's `global.json` to pin `10.0.401`, verifies `dotnet --version`, then runs the existing Release `scripts/build.ps1` build and full tests. Build diagnostics and TRX files are uploaded even when the step fails. The primary checkout and its SDK8 baseline are not modified by the helper.

## Local verification

Source identity: `5eaa12bfaee40e7a254813ea1df6d67c8a3dcbce` (`main`). The helper ran end-to-end with the only locally installed SDK, `9.0.302` (MSBuild `17.14.13`): Release restore/build succeeded; Core tests `125/125`; Worker tests `357/357`; Playnite tests `1566` passed, `0` failed; `ReportedWorkspaceLayoutBehaviorTests` `54/54`. Across the TRX files, 57 legacy UI architecture assertions were `NotExecuted`, each carrying the existing reason that the assertion belongs to a retired dashboard UI architecture. They are reported separately from passed tests.

The PowerShell AST parsed cleanly, the workflow YAML parsed successfully, an output path outside the permitted temporary roots was rejected before worktree creation, and `git diff --check` passed. The SDK9 local run validates the helper and current Release suite only; it does not establish SDK10 compatibility.

## CI status and limits

The preceding run #966 (`38352437420`) for commit `5eaa12bfaee40e7a254813ea1df6d67c8a3dcbce` completed successfully using SDK `8.0.423`. The new SDK `10.0.401` job has not run yet; this evidence must be updated after its Actions run reaches a terminal state. If it fails, retain the complete job log and uploaded diagnostic ZIP before changing code.

This work uses isolated synthetic test fixtures. It does not verify a live Playnite host, physical DPI or multi-monitor behavior, a presented frame, UI Automation/IME, ETW, or host performance. It does not access real saves, media, cloud data, or send diagnostics externally.
