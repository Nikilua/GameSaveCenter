# U09-05 right inset regression repair

This candidate restores the shared DataGrid selection frame to a DPI-rounded 4 DIP gap from the visible viewport. Normal and inactive selection use `ViewportWidth - 8` with a 4 DIP left offset; the 2 DIP keyboard outline uses `ViewportWidth - 7` with a 3 DIP left offset. The background follows the outline. The viewport clip, horizontal scroll anchor, content position and behavior remain intact.

## Focused validation

- SDK 8.0.423, Release, explicit source commit/root metadata, isolated output, full rebuild: exit 0, 0 warnings, 0 errors.
- R23 production state: 12/12 passed, 0 failed, 0 skipped.
- Task Grid frame: 1/1 passed, 0 failed, 0 skipped.
- Save History frame: 1/1 passed, 0 failed, 0 skipped.
- Selection states: 3/3 passed, 0 failed, 0 skipped.
- Production resource dictionary: 140 passed, 0 failed, 39 NotExecuted. All 39 carry the existing `LegacyProductionUiBaselineFact` reason for the reverted “今日工作台” architecture; none are counted as passes.
- Each WPF class ran in its own `WinSta0` private desktop. The input desktop remained `Default` before and after all five runs.

## Failure and repair trail

The exact base commit `a74287e7ecd240a0fbd4fa394a9befb8fd796748` had reduced the shared selection frame's right inset to 1 DIP. The full local `scripts/build.cmd` and both applicable GitHub jobs reproduced the 1-DIP R23 failure; their sanitized TRX/logs and original CI artifact IDs/digests are retained here. The repair returns the shared right gap to 4 DIP without reducing the frame below the visible task content. The focused 5-class run verifies both viewport spacing and content containment.

The full build failure on the base commit is diagnostic history only. The repaired source still requires a committed SHA, a fresh complete `scripts/build.cmd`, the Chinese root install/start chain and both applicable GitHub jobs. Real Playnite host rendering and the user's black-window identity remain unverified.
## Render QA resource failure

The shared Render Harness compiled with SDK 8.0.423, 0 warnings and 0 errors, then rendered 289 images on a private WinSta0 desktop before `RenderTargetBitmap.Render` raised `OutOfMemoryException`. The run returned exit 1 and is not a passing gate. The sanitized report and launch metadata are [renderqa-failure-report.txt](renderqa-failure-report.txt) and [renderqa-failure.json](renderqa-failure.json); no render cases or resolution points were skipped. The incomplete image output was removed after archiving the failure, with space and path checks in [renderqa-partial-cleanup.json](renderqa-partial-cleanup.json).

A QA-only change clears encoder frame references and forces WPF/WIC resource collection every eight saved images. This is a test-tool-only change; the complete rerun below confirmed the full 394-image report.

## Static checks

The candidate passed scripts/validate-source.py (JSON/XML/YAML, XAML semantics, C# delimiters, solution, delivery and safety guards) and the WPF skill static review. It scanned 398 XAML files with 0 errors, 218 existing warnings and 1440 info findings; the modified production resource has no finding. Full output and source hashes are in [source-validation-final.stdout.txt](source-validation-final.stdout.txt), [wpf-static-final.stdout.txt](wpf-static-final.stdout.txt) and [source-static-summary.json](source-static-summary.json).

## Complete Render QA rerun

After the QA-only resource collection change, `scripts/render-qa.ps1 -Configuration Release` completed on the exact changed source files: SDK 8.0.423, exit 0, 0 build warnings/errors, all 394 PNGs and 0 `PROBLEM` lines. The Render Harness ran on a private `WinSta0` desktop; the input desktop remained `Default`. The report, launch output and file hashes are [renderqa-success-report.txt](renderqa-success-report.txt), [renderqa-success-launch.stdout.txt](renderqa-success-launch.stdout.txt) and [renderqa-success.json](renderqa-success.json). The latest complete image audit remains locally at `artifacts/ui-qa/round5-u09-05-right-inset-complete-20261109/`; it is not included in Git.
