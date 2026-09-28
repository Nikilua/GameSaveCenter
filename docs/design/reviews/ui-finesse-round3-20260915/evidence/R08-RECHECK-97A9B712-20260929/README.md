# R08 motion reversal recheck on `97a9b712` (2026-09-29)

## Report and available evidence

The user reported a one-click Release run with `R08MotionReverseBehaviorTests` at `1 passed / 1 failed / 2 total`, taking about 9 seconds. The report did not include the failed test name, assertion, stack trace, TRX, or tested commit identity. The checkout's `artifacts/one-click-install.log` was last modified on 2026-09-24 and contains an unrelated host audit, so it cannot identify this failure.

## Current-main reproduction attempt

- Checkout identity: `97a9b7125da133d08271d66d3847fa88c026d6e9` on `main`.
- Build: `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r08-failure-97a9b712-20260929/build`; solution build succeeded with 0 warnings and 0 errors; XAML structural validation covered 24/24 files.
- Test assembly: `GameSaveCenter.Playnite.Tests.dll`, SHA-256 `E4C626CF41ED38055DC18ABC5C7FEFAAFBC234BD5B982329C3A02638E972FE58`.
- Ran the whole class in 12 sequential, independent VSTest processes from that build output (`--no-build --no-restore`, one process per class run). Every TRX reports 2 passed, 0 failed, 0 skipped; total 24/24 and all 12 process exit codes were 0.
- The two passing tests in each run were `TranslateReversalStartsAtRenderedValueAndFinishesAtLatestTarget` and `SidebarRapidReversalUsesLatestTargetAndReleasesOldClock`.
- Each console run also emitted a WPF `InvalidComObjectException` during testhost cleanup. The exception did not appear as a failed test in TRX and every VSTest process exited 0; its cause remains unknown.

TRX files: [01](R08-01.trx), [02](R08-02.trx), [03](R08-03.trx), [04](R08-04.trx), [05](R08-05.trx), [06](R08-06.trx), [07](R08-07.trx), [08](R08-08.trx), [09](R08-09.trx), [10](R08-10.trx), [11](R08-11.trx), [12](R08-12.trx).

## Conclusion and boundary

This recheck did not reproduce the user's 1/2 result. It does not show which assertion failed on the user's machine and is not grounds to change production motion or relax a behavior assertion without the missing failure details. Keep the report open; the next useful diagnostic is the failed test name plus its error message/stack from that run's log or TRX. No production source was changed, and no Playnite instance was launched.
