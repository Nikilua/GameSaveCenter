# S07-06 测试计数口径

## 改动

`scripts/build-diagnostics.ps1` 在测试诊断结束时读取本次 run 的原始 TRX，并把 `testSummary` 写入 `summary.json`。统计以每个 TRX 的 `<UnitTestResult>` 为测试用例数，单独报告 Passed、Failed、Error、Timeout、Aborted、NotExecuted（skip）、NotRunnable、Inconclusive、Disconnected、Warning 与未知状态；每个文件同时保留 TRX `Counters/@total`，并标出两者是否匹配。

报告按 `core`、`worker`、`playnite-source`、`playnite-wpf-isolated` 和未分类分组，逐文件列出相对路径、结果数和实际测试类数。WPF 隔离类运行数来自每类一个 TRX 的隔离运行布局，与 WPF 用例数分列；诊断步骤数、测试类数和测试用例数不再共用一个计数口径。摘要不复制类名或绝对路径，原始 TRX 继续在 CI diagnostics artifact 中保留。

## 验证

- `pwsh -NoProfile -File scripts/tests/Test-BuildDiagnostics.ps1 -OutputRoot .tmp/gsc-s07-06-test`：通过。
  - 合成 TRX 11 条结果：7 Passed、1 Failed、2 NotExecuted、1 未知 outcome；WPF 2 个隔离类运行。
  - 计数不匹配夹具故意将一个文件的 `Counters/@total` 写成 2、实际结果写成 1，summary 检出差异。夹具中的 Failed/skip 是预期输入，不是产品回归。
- `python scripts/validate-source.py .`：通过。
- 对既有完整 SDK8 TRX 集合做只读复算：[历史 summary](../../../../../ai/evidence/close-sdk-20261007/local-sdk8-full/summary.json) 的 `sourceCommit` 为 `c7f2a3689293c35e1ad98faff05e821b12d22070`。27 个 TRX 含 1,073 条结果：1,054 Passed、1 Failed、18 NotExecuted；逐文件 `Counters/@total` 全部一致。分组为 Core 125 用例/19类、Worker 357/60类、Playnite source 488/112类、WPF isolated 103/24类。WPF 失败是该历史记录中的 Q14 错误态尺寸断言，完整原始证据在 [CLOSE-SDK-01](../../../../../ai/evidence/close-sdk-20261007/README.md)。这个复算只验证解析器，不是当前产品提交的新构建或测试结果。

## 边界

没有运行 Playnite 完整构建或 SDK8 UI 门禁；未改测试执行条件、失败结果、workflow 和现存历史 TRX。`summary.json` 里的 `testCaseCount` 是 TRX 结果条目数；每个源/隔离组的 `distinctTestClassCount` 和 WPF `wpfIsolatedClassRunCount` 是独立的类维度统计。
