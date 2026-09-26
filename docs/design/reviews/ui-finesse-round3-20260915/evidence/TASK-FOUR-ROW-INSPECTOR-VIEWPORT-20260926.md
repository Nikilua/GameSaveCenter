# Task queue four-row viewport and compact Inspector — 2026-09-26

## Baseline and cause

The full Render QA after `741f81b8` had 13 remaining PROBLEMs. Task had related geometry failures: a 236-DIP minimum could not fit the 52-DIP header plus four 52-DIP desktop rows; compact resize with an open inspector left only three readable rows; and the 1040×700 PageHost's fractional layout rounding placed the stacked Inspector 0.8 DIP beyond the Task surface.

The QA report records source commit `741f81b867351edac9e2048abc4d7d2254f800be` and `WorkingTreeClean: False`, as expected while this stage's source changes were under test. All dimensions below are offscreen WPF logical DIP.

## Changes

- Desktop TaskGrid minimum: 264 DIP (52-DIP header + four 52-DIP rows plus rounding allowance).
- Compact TaskGrid minimum: 200 DIP (52-DIP header + four 36-DIP rows plus rounding allowance), including while the compact Inspector is open.
- When the Inspector is open on a short compact page, temporarily collapse the secondary metric strip to free vertical space; restore it when the Inspector closes. Filter controls and queue commands remain available.
- Base short-height decisions on `TaskPageScrollSurface.ActualHeight` when measured, rather than trusting a host callback that may carry the outer window height.
- Cap the stacked Inspector at 136 DIP and retain a 2-DIP bottom inset. Its own scroll surface remains responsible for detail content.

Bindings, task commands, selected-task behavior, real data, row recycling/virtualization, and the grid's internal scrolling remain intact.

## Verification

- Release RenderHarness build: `0 warnings / 0 errors`.
- Task responsive and compact-inspector tests: `9/9`, no skipped tests.
- `Task/step1:1100x720`: `4/4` readable rows at a 200-DIP viewport.
- Production shell Task at 1040×700: PageHost `726×556`, compact queue Grid `710×200`, Inspector `726×136`, four rows after opening details.
- Production shell Task at 1100×720: PageHost `786×576`, compact queue Grid `771×200`, Inspector `786×136`, four rows after opening details.
- No Task PROBLEM or compact-Inspector boundary escape. The full Render QA now has 8 PROBLEMs, all remaining in Settings at 560-DIP height; the project-wide gate is not yet green.
- Source validator, XAML structural validation `24/24`, WPF static review `0 errors / 28 warnings / 177 info`, and `git diff --check` passed. The static warnings/info are existing repository findings.

Build and RenderHarness used existing restore assets with `--no-restore`; the usual wrapper cannot read the user NuGet.Config on this machine. Playnite was not started. This is not host, UI Automation, or physical-DPI verification. The R ledger remains 192 IDs with counts `106/83/1/1/1`.
