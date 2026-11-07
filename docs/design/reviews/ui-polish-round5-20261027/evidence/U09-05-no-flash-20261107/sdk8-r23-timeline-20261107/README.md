# R23 production tab animation test stabilization

The SDK 10.0.401 job for pre-fix source `e24a37faec8c2c971cb0d68132f7a2605a4048f4` failed only because its private WPF desktop did not yield both opacity midpoint samples within 250 ms. The original SDK 8.0.423 job passed. Run #1487, both job IDs, the failed test, and diagnostic artifact digest are recorded in `ci-before-fix.json`; the raw SDK 10 job log was read.

The test now reads the actual production `GscRedesignWorkspaceTabItem` trigger and verifies the entry/exit storyboard target, `Opacity` property, duration, easing, both theme states, settled selection, and reduced-motion endpoint. It does not sample the compositor's frame cadence. No production XAML, animation duration, business behavior, or skip rule changed.

With SDK 8.0.423 / Release, the focused test passed three independent runs (1 passed, 0 failed, 0 skipped each). Each runner used a unique WinSta0 private desktop; the input desktop remained `Default` before and after. The test assembly SHA, source blob, sanitized TRX paths and hashes are in `local-results.json`.

This evidence establishes test stability and window isolation for those runs. It does not identify or resolve the user's black-window report. U09-05 remains in progress pending a source-frozen independent build, the Chinese full entry chain, installation identity/startup checks, and the required post-push CI jobs. Do not interpret an isolated desktop run as proof that every manual `.cmd` launch path is flash-free.
