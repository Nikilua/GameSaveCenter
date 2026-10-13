# Actions #970 / #971 诊断包与测试修正（2026-10-13）

## 用户提供的 CI 诊断

附件按内部 `summary.json` 的源码提交与 SDK 映射到 run；没有复制 ZIP 到仓库。

2026-10-14 用户再次附加的三个文件均与本证据包已校验的 ZIP 内容相同：`GameSaveCenter-sdk10-build-diagnostics (1).zip` SHA-256 `155612D3A083821531E04D07F57CCAE2CF403647741996395B335FD98AE43EAE`（#970 SDK10）、`GameSaveCenter-build-diagnostics (4).zip` SHA-256 `598C2CA619F0CAC37E3E6877B5CBC0EE45AEFC3371DAAACA0F122AE0BC64A8B6`（#970 SDK8）、`GameSaveCenter-build-diagnostics (3).zip` SHA-256 `6AC19F269A3B2FDF3618BF9153C313245AE742EADB6E389A5DACDAEE486ADA4C`（#971 SDK8）。ZIP 未复制进仓库，也没有新增 #971 SDK10 lane 日志。

| Run / lane | 来源提交、CI SDK | 诊断结论 | ZIP SHA-256 |
| --- | --- | --- | --- |
| #970 SDK10（run `38386326043`） | `c9f9179d9b8d38e506dbb1402ae7030eafebea6c`，`10.0.401` | Release build `0 warning / 0 error`；Worker 全量测试中 `CancellationIsIdempotentAndPublishesSafeFinalizationBeforeCancelled` 失败，期望回调数 `1`、实测 `0`，栈指向测试第 128 行。 | `155612D3A083821531E04D07F57CCAE2CF403647741996395B335FD98AE43EAE` |
| #970 SDK8 | `c9f9179d9b8d38e506dbb1402ae7030eafebea6c`，`8.0.423` | 两个完整 solution-build summary 均为 `passed`。这不替代同一 run 的 SDK10 失败结果。 | `598C2CA619F0CAC37E3E6877B5CBC0EE45AEFC3371DAAACA0F122AE0BC64A8B6` |
| #971 SDK8（run `38393317198`） | `5a6647cbf8b40b30bc11bf8c6c32015545896967`，`8.0.423` | solution-build 在 `Q14ToolbarAlignmentBehaviorTests.ProductionTaskToolbarsKeepMixedControlGeometryAndTextCenters` 失败。Light `980×640 DIP` 的报告为 preset text centers `6,18,18,18,18,18`。 | `6AC19F269A3B2FDF3618BF9153C313245AE742EADB6E389A5DACDAEE486ADA4C` |

附件里没有 #971 SDK10 lane 的完整 diagnostics ZIP。两条 GitHub run 页在未登录视图中仅显示登录提示和通用状态，因此这里不推断整次 workflow 的当前终态；新提交的 Actions 必须另行跟到 success/failure。

## 根因与修正

- #970 的取消测试在 `started` 信号后才注册 token callback，测试线程可以先发起取消。测试现在在 callback 注册完成后才发出 `cancellationReady`，继续用并行双重 `CancelAsync`、回调计数和安全终态验证幂等性。生产 `TaskCoordinator` 没有改动。
- #971 的 toolbar fixture 把生产 ComboBox 的 `TaskFilterPreset` 对象集合替换成字符串；XAML 使用 `DisplayMemberPath="Name"`，因此没有实际选中显示文本。测量辅助方法随后可能取到隐藏的零尺寸 presenter。fixture 现在选择真实的 `TaskFilterPreset` 对象，断言显示文本为“默认”，并只测量可见且有尺寸的选中文本或 presenter。生产 XAML、样式和布局没有改动。

## 本机精确提交复核

- 源码提交：`599d2fd38dd5c9af523a0890a33e73fc78ac0bfa`。本机隔离 SDK `8.0.423`、VSTest `17.11.1`；Playnite 插件 `net462`、测试 `net472`、Worker `net8.0-windows10.0.19041.0`。
- `Q14ToolbarAlignmentBehaviorTests`：`1 passed / 0 failed / 0 skipped`；[TRX](tests/Q14ToolbarAlignmentBehaviorTests-599d2fd3.trx)，SHA-256 `F35A1F8859BFFCDBFD776CBAB011ACBA12F92F37C1CDDFCE266E92279A955B0A`。
- 完整 `GameSaveCenter.Worker.Tests`：`357 passed / 0 failed / 0 skipped`；[TRX](tests/Worker.Tests-599d2fd3.trx)，SHA-256 `0A8343699B4F1F77C762D2E4703877879B4D690BD6CCCF0CE02578FC54A50BF4`。
- 插件程序集 SHA-256 `3BDCF2BEE4982100D4FF5D5A94E4C809733A765CB09F6F8224BDF36DE9E877C5`；Playnite 测试程序集 `0D303B233AA65AD48FEA175099EAFC261A915BA14558DBC5C352A15E4E05CE0C`；Worker 程序集 `657F6516BD9FDD2A0D526D39D7E17DEDB10ACBA5AD088EF8911BD232B53F5F60`；Worker 测试程序集 `51C85A61D672020818D39BC2D0DC400E83585182904A04D77546AF8201452A78`。四个 `ProductVersion` 均绑定上述完整提交 SHA。

本机 SDK8 结果不能代替 SDK10 CI。未在本机复现 SDK10；修正推送后必须确认 Actions SDK10 lane 的最终状态。没有启动真实 Playnite 或触碰真实存档、媒体、配置和云端。
