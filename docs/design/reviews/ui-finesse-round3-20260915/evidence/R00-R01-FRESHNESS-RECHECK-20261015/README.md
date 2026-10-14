# R00/R01 evidence freshness recheck

Date: 2026-10-15

Source commit: `4078f23d2d78a06ad1ff371978cebfee1931c309` (`main`)

Package identity: not provided

## Result

Ran `scripts/check-ui-evidence-freshness.ps1 -HeadCommit HEAD -AsJson` against the tracked source at the commit above. The baseline contains 14 R00/R01 records; all 14 are fresh and 0 require a rerun. No changed source path matched any registered R00/R01 source path.

This supersedes the intermediate freshness result at `0243c108` that identified four stale records. Those records were rechecked and the baseline was updated at `599d2fd3`; this pass confirms that later tracked changes through `4078f23d` do not stale the evidence again.

No R00/R01 behavior or visual source changed in this interval, so this freshness-only pass did not rebuild or rerun the earlier targeted suites. It does not supply a package identity or claim Playnite-host rendering, Windows IME/Narrator, physical DPI/cross-monitor behavior, or presented-frame verification. The prior audit's 13 MEDIUM narrow-layout notes remain open.

## Latest main recheck (2026-10-15)

After the Q24-04 keyboard-path commit and the current-main R06/Q24/R09 evidence commit, reran:

```powershell
scripts/check-ui-evidence-freshness.ps1 -HeadCommit HEAD -AsJson
```

At source HEAD `4eb778e992b6f6f76048309f42c2811d6a791815`, the result is again `14 records / 14 fresh / 0 stale`; every record has an empty `matchedSourcePaths`. Across their individual evidence commits the comparison considered 2,727 distinct changed paths, so the aggregate is not documentation-only; none matched a registered R00/R01 evidence source path. No R00/R01 source or test path therefore requires a rerun under the current baseline. Package identity remains `not-provided`.

The ledger remains: R00-01 through R00-07 satisfied; R00-08 controlled behavior satisfied with real Playnite/OS IME and physical input still unverified; R01-01 through R01-08 satisfied. R01-06 retains the current audit's `13 MEDIUM` layout notes rather than marking them cleared. This freshness result checks source-path invalidation only; it does not replace the archived behavior tests, create a package identity, or close host/OS verification.

The separate Actions failures #1006 (`38624032938`) and #1007 (`38625232841`) do not identify an R00/R01 regression: complete logs and diagnostic ZIP contents remain unavailable, so their causes are not inferred from the failure summaries.
