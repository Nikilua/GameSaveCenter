# ReportedWorkspace WPF testhost lifecycle repair (2026-09-27)

## Finding

`ReportedWorkspaceLayoutBehaviorTests` previously used a process-static lazy STA dispatcher. Its thread called `Dispatcher.Run()` indefinitely and was never shut down or joined. Two tests also created `Application` with `ShutdownMode.OnExplicitShutdown`, without closing the application after the collection. On this machine, VSTest could report all test cases as passed and xUnit `Finished`, yet fail to complete the test run.

The failure was reproduced before the fix at source/parent identity `d63a282f7f1cfc6f2d54f3b1568ab6e1104b435b` with the supported VSTest hang guard:

```text
dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~ReportedWorkspaceLayoutBehaviorTests" --blame-hang-timeout 120000ms --blame-hang-dump-type none
```

VSTest terminated the run after 2.4275 minutes with 10 passed tests and no assertion failures; xUnit had already emitted `Finished`. This establishes a host/fixture teardown hang, not a failing layout test.

## Repair

Commit `a07929b7014a2d55971bdf39a8a85ffa00ad5bdc` replaces the process-static dispatcher with an xUnit collection fixture. The fixture reuses an already-active `Application.Dispatcher` without claiming ownership. If it creates a fallback STA dispatcher, it owns that dispatcher, tracks any `Application` created by the tests on that dispatcher, requests shutdown at collection end, and joins the STA thread with a 10-second bound. Production XAML, code-behind, commands, and layout behavior were not changed.

## Verification

At the exact code commit identity:

- Release solution build: `0 warnings / 0 errors`.
- XAML structural validation: `24/24`.
- `ReportedWorkspaceLayoutBehaviorTests`: `10 passed / 0 failed`, VSTest exited `0` in `40.6731 s` with `--blame-hang-timeout 120000ms` enabled.
- Before committing, the same source also produced two consecutive clean exits: `10/10` in `40.4071 s` and `10/10` in `40.4766 s`.
- `git diff --check`: passed.

The output folders were confined to repository `.tmp/`, validated before cleanup, and removed after the runs. No Playnite host was started. This closes only the local WPF testhost lifecycle issue; it does not provide real Playnite rendering, DPI, or host validation. The R ledger remains 192 unique items with state totals `106/83/1/1/1`.
