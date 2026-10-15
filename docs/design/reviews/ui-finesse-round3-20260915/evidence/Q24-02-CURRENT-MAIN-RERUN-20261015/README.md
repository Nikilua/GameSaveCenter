# Q24-02 transient surface failures: current main rerun

Date: 2026-10-15

Source commit: `6b946540e634eacde4c64e50a5b187e8e085e9bc`

Parent code commit: `88b8eb57f13750b4f0a43c1f94a7dc1d59ae6acc`

## Scope

Rechecked the 16-test Popup/focus/virtualization group associated with the `74815b2b` failure report. The five R05 test source files are byte-identical to their `74815b2b` versions. Production source has changed since that report, so the current run rebuilt the full Release solution with the exact current source identity before testing.

No production or test source was changed. This package records behavior and environment limits; it does not claim a new fix.

## Build and isolated test results

- Local SDK `8.0.423`, MSBuild `17.11.48`, VSTest `17.11.1`; Playnite target `net462`, test target `net472`.
- Full Release solution build succeeded with `0 warnings / 0 errors`; XAML structural validation passed `24/24`.
- Ran each WPF class in a separate VSTest process, as `scripts/run-playnite-tests-isolated.ps1` does:
  - `R05PopupBoundaryBehaviorTests`: `2/2`.
  - `R05FocusBoundaryBehaviorTests`: `5/5`.
  - `R05OptionVirtualizationBehaviorTests`: `5/5`.
  - `R05PopupLifecycleBehaviorTests`: `1/1`.
  - `R05TooltipTimingBehaviorTests`: `2/3`. The two non-mouse tests passed. `QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt` stopped at line 132 because Win32 `GetCursorPos` returned false before the test window was shown. This run did not have usable system pointer coordinates; no cursor API bypass or skip was added.

The original Popup/selection-virtualization failures did not reproduce in the current class-isolated runs. The focused tests exercised popup creation/work-area bounds, selected-item visibility, keyboard focus containment and return, and 2,000-item list virtualization. The run does not establish real Playnite-host rendering or OS-level input behavior.

## Combined-process diagnostic

A separate diagnostic run put the same 16 tests from the five classes into one VSTest process. It produced `11 passed / 5 failed`: four WPF focus/window-activation assertions and the same `GetCursorPos` precondition. The three target classes that failed in the older report passed when each class ran in its own process. The repository's supported test runner isolates WPF classes; this combined-process probe is not the build gate. It demonstrates process/environment sensitivity, but does not establish the root cause of the focus failures.

TRX: [Popup boundary `2/2`](tests/R05PopupBoundaryBehaviorTests-isolated.trx), [focus boundary `5/5`](tests/R05FocusBoundaryBehaviorTests-isolated.trx), [option virtualization `5/5`](tests/R05OptionVirtualizationBehaviorTests-isolated.trx), [Popup lifecycle `1/1`](tests/R05PopupLifecycleBehaviorTests-isolated.trx), [Tooltip timing `2/3`](tests/R05TooltipTimingBehaviorTests-isolated.trx), and [combined process `11/16`](tests/R05TransientSurfaces-combined-process.trx). Test source, DLL and PDB hashes are in [`build/file-hashes.txt`](build/file-hashes.txt); the solution build log is [`build/solution-release-sdk8423.txt`](build/solution-release-sdk8423.txt).

## CI and remaining boundaries

Commit `6b946540` was pushed to `main`. The GitHub Actions list was returning a loading error and a stale run for a different SHA (`8546aca`); the anonymous API returned HTTP `403` rate limit. Therefore the run ID and terminal result for `6b946540` are unknown. Local SDK `8.0.423` is not a substitute for CI SDK `10.0.401`.

The local system-pointer API was unavailable for the Tooltip mouse test. Real Playnite, actual OS keyboard/mouse input, UI Automation/Narrator, physical DPI and cross-display behavior, presented frames, ETW, and host performance remain unverified. Q24-03 remains blocked on a second physical display; no system trace permissions were bypassed.

The external R09 thickness report is a separate issue. Its current-main same-output rerun and missing external DLL/PDB identity remain documented in [`CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014`](../CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md).
