# U09-04 reconstructed U00 comparison and task scroll signal

## Scope and source identity

This supplements the [current-candidate timestamp/GC probe](../fixed-gc-rerun-20261030/BASELINE.md). It reconstructs the U00 source snapshot `90bf040567f013f3a8d9ed8667028a4edb2ec524` and compares it with frozen candidate `5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7`. Measurement HEAD was `4e214368a310ed0ef892b5e3eebb5ed46fc7adf9`; production files were not modified.

The U00 snapshot came from `git archive` (3,021 files; archive SHA-256 is in [build-run.json](build-run.json)). Its RenderHarness did not include `FakeSampleClock`, so the isolated probe copy redirected 45 `FakeDashboardData.DateTime.UtcNow` calls to the pinned fake clock. Both snapshots received the same temporary timing/action instrumentation. The instrumented `Program.cs`, `FakeDashboardData.cs`, helper, clock, source archive identities, and compiled harness hashes are retained here. These are reconstructed measurements, not recovered original U00 raw artifacts.

The U00 probe build and three fixed-cycle runs exited 0. A fourth run recorded per-page actions; a fifth added the selected scroller context. The candidate uses the three fixed-cycle runs already archived beside this folder and adds two corresponding action runs. Both action runs per source exited 0. Exact build logs, run reports, raw CSV, analysis output, and source hashes are retained in this folder and `../fixed-gc-rerun-20261030/`.

## Protocol

Both snapshots used SDK 8.0.423 Release, `GSC_RENDER_SAMPLE_UTC=2026-10-28T09:00:00Z`, a controlled WPF STA window at opacity `0.01`, seven workspaces, Light/Dark themes, 1040×700 DIP, 70 cycles, 297 completed actions, 30 seconds of post-action settling, and no forced GC. The build used the repository workload-resolver setting. Each fixed-cycle run completed without action failures.

Theme timing brackets the two `ApplyThemePalette` calls. Content-layout timing brackets page-host assignment, responsive layout and `UpdateLayout()`. The managed-allocation counter is `AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize` during the active interval; it excludes native and other-AppDomain allocations and does not isolate production-only allocations.

## Fixed-cycle comparison

Values below pool the three runs (210 samples per segment). Percentiles use the nearest-rank method; raw calculations and per-run results are in [comparison.json](comparison.json).

| Segment | U00 p50 | U00 p95 | U00 max | U00 >300 ms | Candidate p50 | Candidate p95 | Candidate max | Candidate >300 ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Theme application | 317.99 ms | 448.77 ms | 486.22 ms | 133/210 | 232.36 ms | 325.97 ms | 361.38 ms | 21/210 |
| Content layout | 47.87 ms | 87.52 ms | 268.24 ms | 0/210 | 46.12 ms | 97.49 ms | 225.14 ms | 0/210 |

All 210 theme operations in both snapshots had a Gen 0 count increase. For the candidate, theme duration and allocated bytes had Pearson `r=0.974`; this describes the measured windows and does not establish GC as the cause. The previous U00 content-layout samples above 300 ms did not recur in these candidate runs; candidate layout had nine samples above 100 ms, matching U00, and none above 300 ms.

During the active measurement window, mean managed allocations were 11,118,422,064 bytes for reconstructed U00 and 8,878,246,139 bytes for the candidate, about 20.15% lower in the candidate. This counter includes managed allocations from the probe's active AppDomain, not only product UI allocations.

Active `CompositionTarget.Rendering` callback intervals were:

| Snapshot | Samples | p50 | p95 | Maximum | >100 ms |
| --- | ---: | ---: | ---: | ---: | ---: |
| U00 reconstructed | 3,609 | 15.68 ms | 310.76 ms | 1,322.40 ms | 429 |
| Candidate | 2,831 | 15.74 ms | 286.91 ms | 734.37 ms | 416 |

These callbacks are not physical display, compositor or VSync frames.

## Focused task-page actions

The selected internal target was `DG_ScrollViewer` with `ScrollableHeight=54` in both sources. The action invokes `ScrollToVerticalOffset()` and then `UpdateLayout()`, so its duration includes forced layout work. Each focused run has 10 samples:

| Run pair | U00 task scroll p95 / allocation p95 | Candidate task scroll p95 / allocation p95 | Task synthetic search p95 |
| --- | ---: | ---: | ---: |
| Page-actions run (no scroller name recorded) | 29.36 ms / 3,141,632 B | 38.23 ms / 5,004,528 B | 5.98 ms / 6.34 ms |
| Scroll-context run (`DG_ScrollViewer:54`) | 29.73 ms / 3,194,880 B | 37.74 ms / 4,997,240 B | 7.05 ms / 6.62 ms |

The scroll timing/allocation signal moves in the slower direction in both distinct action runs; the search proxy does not. Each pair contains one run per source, so this directionally consistent signal needs more samples and is not a stable performance estimate or proof of a product regression.

Source inspection narrows what changed but does not identify a cause. The full XML subtrees for `GscRedesignWorkspaceDataGrid`, task-page `TaskDataGrid`, and the `TaskGrid` instance are identical between the two commits. The shared `GscStableDataGridRow` selected border changed from literal thickness `1` to `GscHairlineThickness`. `TaskCenterView.xaml.cs` also changed between these commits, including compact inspector transitions and responsive-layout handling. Therefore the slower scroll action cannot be attributed to a specific row template, visual token or code path from this evidence alone. The existing DataGrid scrolling system and virtualization settings remain in place.

## Status and limits

U09-04 remains **in progress**. The theme timing and active managed allocations improve in this controlled reconstruction; content-layout long-tail samples above 300 ms did not reproduce. The Task DataGrid scroll signal remains unexplained and needs a narrower repeated action comparison before choosing any source change. Original U00 frame/total-allocation artifacts are still unavailable. This RenderHarness does not verify Playnite host rendering, physical input or IME, physical DPI, or presented frames; no install/startup chain or `scripts/build.cmd` was run for this diagnostic evidence-only stage.

See [build and run identity](build-run.json), [analysis and action comparison](comparison.json), the copied probe sources under `probe-source/`, the [SHA-256 manifest](sha256.json), and [temporary-output cleanup record](cleanup.json).
