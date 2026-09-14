# Q20-07 Overview 云端队列保证级别证据

采集日期：2026-09-15（Asia/Shanghai）。源码与回归测试已落地于提交 `f01dfe9cb73cc8dfad9ec5f93db4a6cffe348bd6`（`f01dfe9`）。本证据只签收云端卡的显示语义、焦点/命令入口和受控视觉，不把离屏结果扩大为真实宿主 Hover/Focus 走查。

## 实际修复

- `CloudTransferSummaryDto` 新增 `GuaranteeDisplay`，固定分别输出 `已上传 N · 已校验 M`；`RemoteVerified` 不再被文案当成普通上传的别名。
- Overview 云端队列整卡继续显示真实 `QueueControlDisplay`（运行中/暂停/不在允许时段），并在同一状态行追加独立的上传与远端校验计数；整卡仍绑定 `OpenCloudQueueCommand`，没有增加嵌套操作按钮。

## 证据与验证

- `UiDisplayMappingTests.CloudSummaryKeepsUploadedAndRemoteVerifiedCountersSeparate` 断言 `已上传 2 · 已校验 3`。
- `OverviewInteractionTests.OverviewActivityRowsKeepTheirVisualTreeAndCloudQueueCardExecutesOneClickCommand` 在真实 WPF STA 视觉树中读取绑定后的状态行，确认“已上传 2 · 已校验 3”可见，并继续通过 ButtonBase 的单次命令路径。
- `UiFinesseRound2ControlSourceTests.OverviewCloudCardKeepsQueueAndGuaranteeStatesDistinct` 锁定队列状态、保证级别和整卡 Automation Name 的 XAML 契约。
- Core 全量回归：`83 passed / 0 skipped / 0 failed`。
- Playnite 全量回归：`473 passed / 63 skipped / 0 failed`，进程退出码为 0。
- `scripts/render-qa.ps1 -Configuration Release -Output .tmp/ui-qa-cloud-card-clean-20260915`：干净 HEAD `f01dfe9cb73cc8dfad9ec5f93db4a6cffe348bd6`，`WorkingTreeClean: True`，RenderHarness 构建 `0 warning / 0 error`；双主题、多尺寸、页面滚动、Resize 和壳层 QA 均为 `render-qa OK`，报告无 `PROBLEM`。
- 已实际查看 1040×700 的 Light/Dark Overview PNG；深色与浅色状态行均完整显示，紧凑宽度下发生自然换行但无裁切。
- `python scripts/validate-source.py`、`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`（326 XAML、0 errors）及 `git diff --check` 通过。静态审查保留宿主 FusionX/历史资源的既有 warning/info，不将其当成本次缺陷。

## 尚未签收

- Q20-07 的真实 Playnite 鼠标 Hover、键盘 Focus、一次导航和不同云端状态的宿主像素走查仍需可操作的宿主窗口；本证据已覆盖源码、WPF 行为和受控双主题视觉。
