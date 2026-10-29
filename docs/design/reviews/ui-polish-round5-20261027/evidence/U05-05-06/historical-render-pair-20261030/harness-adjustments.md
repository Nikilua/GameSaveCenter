# Historical probe adjustments

The probe is temporary RenderHarness instrumentation; no tracked production source or resource changed in this comparison.

- The first before build tried to read `IsLoadingFeedbackReady` from the older snapshot, where that property does not exist. The helper was reduced to the common production surface visibility and measured-slot contract, then both snapshots built with 0 errors.
- An early after build used `--no-restore` before that extracted snapshot had `project.assets.json`; the failed command is retained in `diagnostics/after-initial-no-restore-build.txt`. The snapshot was restored, then rebuilt successfully.
- The first after capture kept the delayed surface Hidden because the harness waited with a higher-priority `Send` dispatcher timer. The wait timer was changed to `ContextIdle`, allowing the production Background loading timer to run. The same corrected probe then captured both before and after snapshots with exit code 0.
- Archived diagnostic paths are normalized to `<phase-root>` so the temporary build tree can be removed without leaving stale references. The initial failure logs remain in `diagnostics/`; screenshots and CSVs from those failed runs are not retained. The accepted pair is the `*-v2` capture recorded in `run.json`.

The observed slow-loading screenshots arrive around 264–278 ms in this harness because it samples after a 260 ms dispatcher wait. The configured production delay remains 180 ms; these samples confirm that the timer has fired by the later observation point, not a precise 180 ms wall-clock threshold.
