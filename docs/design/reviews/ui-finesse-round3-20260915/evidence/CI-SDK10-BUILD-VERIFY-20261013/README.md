# SDK 10.0.401 CI build/test verification

## Purpose

The repository baseline remains pinned to .NET SDK `8.0.423` in `global.json`. The user's CI gate also requires an actual SDK `10.0.401` build/test result; a local SDK 9 run or the existing SDK 8 Actions job cannot satisfy it. This change adds a separate Windows Actions job for SDK `10.0.401` and preserves the baseline job.

The SDK 10 job creates a detached temporary worktree at the checked-out commit, changes only that worktree's `global.json` to pin `10.0.401`, verifies `dotnet --version`, then runs the existing Release `scripts/build.ps1` build and full tests. Build diagnostics and TRX files are uploaded even when the step fails. The primary checkout and its SDK8 baseline are not modified by the helper.

## Local verification

Source identity: `5eaa12bfaee40e7a254813ea1df6d67c8a3dcbce` (`main`). The helper ran end-to-end with the only locally installed SDK, `9.0.302` (MSBuild `17.14.13`): Release restore/build succeeded; Core tests `125/125`; Worker tests `357/357`; Playnite tests `1566` passed, `0` failed; `ReportedWorkspaceLayoutBehaviorTests` `54/54`. Across the TRX files, 57 legacy UI architecture assertions were `NotExecuted`, each carrying the existing reason that the assertion belongs to a retired dashboard UI architecture. They are reported separately from passed tests.

The PowerShell AST parsed cleanly, the workflow YAML parsed successfully, an output path outside the permitted temporary roots was rejected before worktree creation, and `git diff --check` passed. The SDK9 local run validates the helper and current Release suite only; it does not establish SDK10 compatibility.

## CI status and limits

Run #967 (`38362229559`) for commit `983fee5cae51f0f9fbe4b100620ab156e3a723f1` reached `Failure`. The SDK8 baseline job (`115178885652`) stopped at “源码结构检查”; its raw job log was not available from the public log endpoint (HTTP 404) or anonymous API (HTTP 403), and the run exposed no SDK8 diagnostics artifact.

The user-provided `GameSaveCenter-sdk10-build-diagnostics.zip` is the separate SDK10 job artifact (`11795654027`, 737,570 bytes; SHA-256 `A4D13BB58AC343DFDCFFA6FB4DA31257080F1E5128AF151C946B0AEB0E3BC409`). Its `summary.json` identifies source commit `983fee5cae51f0f9fbe4b100620ab156e3a723f1`, SDK `10.0.401`, outcome `passed`, and exit code 0 for every recorded step. It contains 117 TRX files with 1,566 passed and 0 failed tests. This confirms the SDK10 build/test job only; it does not include the failed SDK8 source-check output.

The exact SDK8 check was reproduced locally on commit `983fee5`: `python scripts/validate-source.py` reported that `scripts/verify-sdk-build.ps1` lacked the UTF-8 BOM required by the repository's Windows PowerShell 5.1 source guard. Adding only that BOM made the same validator pass. The raw CI log remains unavailable, so the diagnosis is supported by reproducing its exact failing command and guard locally. The correction is pending commit/push and a fresh Actions run.

This work uses isolated synthetic test fixtures. It does not verify a live Playnite host, physical DPI or multi-monitor behavior, a presented frame, UI Automation/IME, ETW, or host performance. It does not access real saves, media, cloud data, or send diagnostics externally.
