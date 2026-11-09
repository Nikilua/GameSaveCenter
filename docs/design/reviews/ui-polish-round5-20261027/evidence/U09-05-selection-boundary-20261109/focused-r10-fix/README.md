# R10 selected-frame expectation correction

## Result

The first full local build on `3527fb1d9c2c701539ca727a05b2ab6157cd94c3` and GitHub run #1498 both reached the same stale assertion: the Overview-specific selected `TaskItemChrome` expected the old 4-DIP right margin after the frame was expanded. The geometry and content-position assertions remain enabled. Only the Overview-specific expected margin changed to zero; the shared ListBox margin assertion remains 4 DIP.

The corrected `R10RecentAccessBehaviorTests` class was rebuilt and run with SDK 8.0.423 on `WinSta0\GscWpfTest_a654a84222c344cfa986bab5ea8280d8`. The input desktop was `Default` before and after. Result: 3 passed, 0 failed, 0 skipped, process exit 0. The command started through a hidden PowerShell process with redirected output. This focused class result does not replace the required full build or the final install/start chain.

## Evidence

- [`console.txt`](console.txt): SDK selection and normal-verbosity test output.
- [`desktop-result.json`](desktop-result.json) and [`parent-result.json`](parent-result.json): child/helper exit codes and private desktop identity.
- [`tests/R10RecentAccessBehaviorTests.trx`](tests/R10RecentAccessBehaviorTests.trx): all three class tests.
- [`specification.json`](specification.json): exact focused test arguments.
- [`run-summary.json`](run-summary.json) and [`manifest.json`](manifest.json): source blob, SDK, counts, process identity and evidence hashes.
- The previous full-build failure and its original TRX are retained in [`../diagnostics/sdk8-r10-stale-margin/`](../diagnostics/sdk8-r10-stale-margin/).
- GitHub run #1498 (`42553664674`) failed on the same assertion in both applicable jobs. Raw job diagnostics: [SDK baseline `128954520959`](https://github.com/Nikilua/GameSaveCenter/actions/runs/42553664674/job/128954520959) and [SDK 10.0.401 `128954521177`](https://github.com/Nikilua/GameSaveCenter/actions/runs/42553664674/job/128954521177). Diagnostic artifact IDs and digests are listed in [`../diagnostics/sdk8-r10-stale-margin/ci-run-1498.json`](../diagnostics/sdk8-r10-stale-margin/ci-run-1498.json).

The stale expectation is corrected in the working tree; a new committed SHA and its CI jobs, full local build, and root installation chain remain pending.
