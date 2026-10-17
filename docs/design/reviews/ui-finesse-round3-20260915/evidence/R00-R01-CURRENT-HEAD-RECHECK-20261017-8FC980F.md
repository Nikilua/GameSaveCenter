# R00/R01 当前 main freshness 与外机 R09 日志对照

日期：2026-10-17
检查提交：`8fc980fce5262411fbb16f0299583c34074f7f61`（`main`）

## R00/R01 复核

- 当前 `scripts/check-ui-evidence-freshness.ps1 -HeadCommit HEAD -AsJson` 结果为 `14` 条登记记录、`0` 条需重跑、`0` 个匹配的登记源码路径；`packageIdentity=not-provided`。这是源码路径 freshness 检查，不代表生成包、安装或 Playnite 宿主已验证。
- 本次没有改生产 UI 源码，也没有证据支持新的 R00/R01 生产修复。2026-10-15 全量布局审计中的 `13 MEDIUM TOOLBAR_VERTICAL_EXPANSION` 仍作为独立风险保留；本次 CI 的 Render QA 和 WPF 静态审查不是该运行时审计的重跑，不能清除这些记录。
- Round2/Round3 canonical task IDs 为 `192/192` 唯一。重新按 R23-08 准入表核对后，尚未完成项仍分别依赖真实宿主/输入、第二物理显示器、外机同次身份或获准的系统跟踪；没有发现当前依赖已满足且存在明确产品代码缺口的 Q/R 项。不为凑进度新增功能。

## 当前提交 GitHub Actions

Actions [#1070 / run `39016699991`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39016699991) 对应上述精确提交，整体终态 `success`。

| Runner | SDK | Job | 结果 |
| --- | --- | --- | --- |
| Windows | .NET SDK `8.0.423` | `117350168944` | Release build `0 warning / 0 error`；Core `125/125`、Worker `362/362`、Playnite source `107` 类及 WPF 隔离 `125` 类通过，含 `Q06ButtonStateSequenceBehaviorTests` 与 `R09FocusOutlineBehaviorTests`；Render QA `render-qa OK`；静态审查 `30 XAML / 0 errors / 27 warnings / 177 info` |
| Windows | .NET SDK `10.0.401` | `117350169177` | Release build `0 warning / 0 error`；Core `125/125`、Worker `362/362`、Playnite source `107` 类及 WPF 隔离 `125` 类通过，含 Q06 与 R09 测试 |

Artifacts：SDK8 package `12096595339`（SHA-256 `90191acd305d52c2eb19b69316e7b3e4e96b2f63a03cc92a89462f675dc38573`）；SDK8 diagnostics `12096500597`（`c0160e9a830cbccf08e2dcd19a241c2e927843ae9da8a2bd1a45930ca2b0ca41`）；SDK10 diagnostics `12095108145`（`20246e80ff740e41dc206c1e43c215499ba70ba5ad4d1353fbdfa41948da6e77`）。本机此前 Q06-02 定向运行使用 SDK `8.0.423` / VSTest `17.11.1`，其结果和两种 CI SDK 分开记录。

## 用户提供的另一台机器 R09 输出

粘贴日志中 Core 的 `ActivityTimelineMapperTests.CloudFailureMapsToFailedCloudActivity` 是通过项；失败项为 `R09FocusOutlineBehaviorTests.SelectedTabAndInvalidTextBoxKeepFocusRingOutsideStateChromeAndRecoverAfterError`，`2/2` 中 `1` 项失败。厚度断言报告期望 `2,2,2,2`、实际 `1,1,1,1`，堆栈映射到源文件第 151 行。

这个行号和值与历史提交 `e12119bffc6d66faee44cfc89e555461fe89205b`（`f13b9e05` 的父提交）中的旧断言完全吻合：它要求 `errorChrome.BorderThickness == Thickness(2)`。提交 `f13b9e05` 后测试契约已拆开：外层 `Chrome` 为 `1 DIP`，独立 `ValidationStroke` 为 `2 DIP`；当前源第 151 行是错误色断言，第 152 行检查 Chrome `1 DIP`，第 154 行检查 ValidationStroke `2 DIP`。所以该日志不能由“当前 R09 源码和与其匹配的 PDB”映射而来；旧 checkout、测试 DLL 或 PDB/构建输入不一致是线索，但仍未由身份材料确认具体来源。

`libpng iCCP` 警告不在失败调用栈。`TextServicesContext` 的 `InvalidComObjectException` 出现在测试已报告失败后的 WPF 关闭阶段，不能解释厚度值。当前 #1070 的两个 CI lane 都运行 R09 并通过，只能证明提交 `8fc980f` 在各自 runner 输入下通过，不能覆盖另一台机器的这次真实失败。

外机源 commit、dirty paths、失败同次 `summary.json`/TRX、测试 DLL/PDB 与 plugin DLL 身份均未提供。用户暂时无法再提供资料；不要求重跑、不盲改当前模板，继续独立主线。若资料之后可用，只需对照同次身份。

## 未验证边界与下一步

本文件随后作为文档提交 `9feb88c057e2b800e24edb69bdd2a4d5546df550` 推送。Actions [#1071 / run `39020360750`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39020360750) 对该文档提交已整体终态 `success`：SDK8.0.423 job `117360775397` 的 Release build/tests、Render QA、WPF 静态审查、扩展包与上传步骤均成功；SDK10.0.401 job `117360775218` 的 Release build/tests 与诊断上传成功。Artifacts：SDK8 package `12097522989`（90,314,056 bytes，SHA-256 `63a7e4f04dece80583e479c7d3f8f4ad00cccb5161a3955bae40a01d764ee314`）、SDK8 diagnostics `12097701530`（1,629,165 bytes，`240250bd996a538b53622301f6b95e4b3d7273f5767923cd0983c965c20653ad`）、SDK10 diagnostics `12096373383`（773,429 bytes，`3a07125de9066430be2c753f5c014aabe72d3b80ab782ae8c43bc2fc09d49334`）。

提交 `9feb88c0` 仅改文档；R00/R01 登记源码路径没有变化，按 R01-07 规则无需因纯文档提交重建基线。freshness 结果仍明确绑定 `8fc980f`，而非宣称已在 `9feb88c` 重新运行。

Actions [#1072 / run `39023752118`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39023752118) 对前序文档提交 `6c8c85b0efae4d15996a2c7382255fcbaa8d2a2d` 已整体终态 `success`：SDK8.0.423 job `117370558805` 与 SDK10.0.401 job `117370558697` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite source `107` 类、WPF 隔离 `125/125` 类通过；SDK8 另外完成 `render-qa OK`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、扩展包生成与上传。Artifacts：package `12098304583`（90,314,080 bytes，SHA-256 `3ac418287e75d012c6917d80abb10cde0e0b42005725797a88112991ae0a4ffa`）、SDK8 diagnostics `12099205027`（1,630,261 bytes，`9f8d8503738a9990159787a1d83996b490d0e6832547345a5acfb334c7a0756d`）、SDK10 diagnostics `12098735372`（774,612 bytes，`36d6c6766ae8f879794c708ce19f66e459e389717ab70c1f1a7e3dd7e9d443e1`）。这是文档提交的 CI，不改变本报告代码 freshness 输入 `8fc980f`，也不是外机或真实 Playnite host 验收。

未验证真实 Playnite/package-host、外机当前 checkout 与程序集身份、物理 DPI/跨屏、OS UIA/IME、最终屏幕呈现帧，以及 ETW/宿主性能。#1072 已终态成功。下一可执行动作：获得外机失败同次 `summary.json`、R09 TRX/console 及 test DLL/PDB/plugin DLL 身份后逐项对照；无需重跑。若该 artifact 仍不可用，按 R23-08 重做准入，仅在出现身份材料、新可复现缺陷或环境前置变化后领取 Q/R 小批。
