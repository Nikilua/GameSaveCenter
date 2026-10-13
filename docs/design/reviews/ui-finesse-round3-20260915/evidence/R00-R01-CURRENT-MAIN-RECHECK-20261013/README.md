# R00/R01 当前 main 与 CI 复核

## 当前检查点

- 当前源代码/测试身份：`599d2fd38dd5c9af523a0890a33e73fc78ac0bfa`（main）。当前日志、测试 TRX 与审计元数据均绑定此身份；package identity `not-provided`。
- 历史门禁：#968 / run `38366158855` 对 `4aa1ec7` 的 SDK8 `8.0.423` 和 SDK10 `10.0.401` jobs 均成功；不代表当前 `599d2fd` 的 CI。当前代码与测试修正推送后需单独跟新 run 到终态。
- CI SDK 分开记录：此前 SDK8 job 选中 `8.0.423`、SDK10 job 选中 `10.0.401`；当前本机验证仅使用隔离 SDK `8.0.423`（VSTest `17.11.1`），不能替代新 run 的 SDK10 job。
- 本机当前身份复核已重跑 Playnite 测试与 RenderHarness audit；测试构建 `0 warning / 0 error`。完整类过滤 `UiFinesseFoundationTests + WpfUiResourceDictionaryTests + UiAuditSourceTests` 为 `154 passed / 39 skipped / 0 failed`，分项见下文。

## #967 根因与修正范围

#967 / run `38362229559` 的 SDK8 job `115178885652` 在“源码结构检查”失败。用户提供的 `GameSaveCenter-sdk10-build-diagnostics.zip` 是同一 run 的 SDK10 绿色 job `115178885908`，SHA-256 为 `A4D13BB58AC343DFDCFFA6FB4DA31257080F1E5128AF151C946B0AEB0E3BC409`；它记录 SDK `10.0.401`、117 个 TRX、1566 passed / 0 failed，不是 SDK8 失败日志。SDK8 原始日志匿名接口为 HTTP 403/404，run 没有 SDK8 diagnostics artifact。

在当前 checkout 上执行 CI 相同的 `python scripts/validate-source.py`，重现了 `scripts/verify-sdk-build.ps1` 缺 UTF-8 BOM 触发的源码结构检查失败。只给该脚本补了 BOM，校验随即通过；修正已包含在 `4aa1ec7`。这是一条本机同 guard 复现证据，不伪称读取到了原始 SDK8 job log。

## R00/R01 新鲜度

`freshness-summary.json` 按当前代码提交 `599d2fd38dd5c9af523a0890a33e73fc78ac0bfa` 生成。初检发现 4 条 stale：R00-01-02、R00-05 匹配共享 `WpfUiProduction.xaml`；R01-03、R01-06 匹配 `RenderHarness/Program.cs`。本轮按受影响范围重跑并生成当前 audit 后，才把这 4 条 baseline evidence sourceCommit 更新至 `599d2fd`；随后 14 条全部 `FRESH`，没有只改哈希冒充验证。

- 受影响行为类在当前身份完整复核：`UiFinesseFoundationTests 9/9`、`WpfUiResourceDictionaryTests 139 passed / 39 skipped`、`UiAuditSourceTests 6/6`。R00-02 的 1000 次稳定组合缩放测试通过；R00-05 的 Context/RemoteRestore/MediaBatch disabled chrome Light/Dark 实测 `2/2`。39 个 skipped 保持单独列出，未计通过。完整 TRX：[当前共享控件与审计测试](tests/R00-R01-shared-audit-tests-599d2fd.trx)。
- 2026-10-14 为 R00-08 重新核对键盘回归文件变化：提交 `87070909` 精确身份下，`GamePickerKeyboardBehaviorTests` 6/6 与增强后的 `R21KeyboardNavigationTraceTests` 2/2 同批通过。当前可见交互控件双向可达与反向顺序由[Q24-04 当前 main 证据](../Q24-04-CURRENT-MAIN-KEYBOARD-RECHECK-20261014/README.md)记录；本证据仍只代表受控 WPF Window，不代表 OS IME、真实 Playnite 输入或完整业务数据状态。仅将 R00-08 freshness source 更新到已实测提交 `87070909`，其他 R00/R01 基线仍按各自证据 SHA 保留。

- R00-01/02 与 R00-05 按既有条件精确复跑：算法/状态/ScaleTransform `4/4`（渐变整组透明度与非等距 stop `1`、状态矩阵 Light/Dark `2`、1000 次组合缩放树 `1`）；disabled shared chrome 的 Light/Dark `2/2`。合计 `6/6`，[TRX](tests/R00-01-02-R00-05-targeted-599d2fd.trx)。
- 当前 identity 的 Light/Dark `finesseprobe` 各验证 semantic button 状态/stop `88` 个、违规 `0`，palette `11` checks、effective text contrast `12` samples、numeric readability `4/4`；窄列裁切和黑字深底负例均 `must-fail=passed`。报告与合成窗口图：[Light report/image](probes/light/finesse-light-report.txt)、[Light PNG](probes/light/finesse-light.png)、[Dark report/image](probes/dark/finesse-dark-report.txt)、[Dark PNG](probes/dark/finesse-dark.png)。探针记录 `WorkingTreeClean=False`，当时工作树仅有文档/证据和其他任务未跟踪文件；报告 source/build SHA 均明确绑定 `599d2fd`，不将其冒称 clean tree。

## 当前审计与证据边界
- RenderHarness 当前身份生成审计：`168` snapshots、`116` warnings，其中 `0 HIGH / 13 MEDIUM / 0 Fidelity / 0 failed routes`；E01–E20 引用、身份、样本和边界校验 `20/20`。13 条 MEDIUM 是窄布局动作工具栏自然换行/垂直扩展提示，保留风险，不写成零问题。精选图、summary、manifest、layout report、route map 与脱敏日志在[`audit/`](audit/)。
- 当前复核为离屏合成 WPF、DPI `1.0`，不表示真实 Playnite、用户安装包、物理 DPI/跨屏、OS 输入/IME、Narrator/UIA、presented frame、ETW 或宿主性能。Demo 原目录不可用，沿用恢复的生产基线。
- #970 用户诊断包定位 SDK10 Worker 取消回调时序测试问题，#971 用户诊断包定位 SDK8 Q14 字符串 fixture 与 `DisplayMemberPath=Name` 不匹配；测试修正已在 `599d2fd` 本机通过。#971 SDK10 diagnostics 仍缺；用户暂不可回复期间不阻塞其他任务，推送后检查新 Actions 两个 SDK lane 到终态。
- 下一项可执行任务 Q24-01：先核验已有 fractional-DPI 字形/边线能力，再做有限逻辑缩放行为验证；真实物理 DPI/跨屏继续独立待验。
