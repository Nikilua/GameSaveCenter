# 紧凑窗口换行行距与 R00-06 当前源码复核

日期：2026-09-29。源码提交：`4121a47e`（`修复筛选工具栏换行行距`）。

## 修复

- 任务中心筛选预设控件在 `657 DIP` 以下换行时，原来各子项底边距为 `0`。复用 `WrapPanelRowGapController`，紧凑宽度的行距设为 `8 DIP`；恢复至单行后恢复原始边距。
- 媒体中心筛选预设在 `520/620/700 DIP` 窗口宽度下出现两行且行距为 `0`，`720 DIP` 起恢复单行。加入相同的动态 `8 DIP` 行距控制，宽态还原原 margin。
- 没改筛选值、命令、绑定、媒体游戏选择、DataGrid 虚拟化和页级滚动行为。

## 验证

- 最终提交的 Release solution 构建通过：XAML 结构 `24/24`，`0 warning / 0 error`。使用 `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/wa-gap-audit-20260929/final`，测试输出以完整源码 SHA `4121a47e` 构建。
- 隔离 testhost 逐类运行并归档四份 TRX：`Q14ToolbarAlignmentBehaviorTests 1/1`、`CompactSaveAndInboxActionWrapsKeepAnEightDipRowGapAndRestoreWideMargins 2/2`、`MediaInboxGeometryTests 3/3`、`MediaWindowAnchorContractTests 10/10`，合计 `16/16`，失败 `0`、跳过 `0`。
- 最终 `GameSaveCenter.Playnite.Tests.dll` SHA-256：`BF8B067C480921D5BEE4406F2F37F8A0EC666C97D91FDB124AF0E2DBD40B96B4`。VSTest 按 WPF 类隔离运行；合并多个 STA/UI 类的一次运行未完成，没有将其记为通过。
- Q14 用例在浅/深主题与多个窗口尺寸实测任务预设行距及 `620→660→620 DIP` 尺寸往返；报告包括两行 `8 DIP`、宽态恢复原边距。媒体筛选预设覆盖 `520/620/700 DIP` 两行净距，以及 `720/800/900/1040 DIP` 单行原边距；`700→720→700 DIP` 往返验证再次恢复 `8 DIP`。
- 同一变化下的 R00-06 生产媒体几何回归改在 `700×600 DIP` 窄窗运行：表格高度预算包含实际表头、行高及滚动条，框高度包含 padding/border；保留“四行完整可见或页级滚动可达”的门禁。短窗负例和页尾 fallback 门禁仍通过。稳定分页锚点契约 `10/10` 通过。
- 初次把多组 WPF 类合在一个 VSTest host 的运行无输出停滞，未生成结果；按 WPF 隔离规则改为每类新进程，最终四份 TRX 均明确通过。一次手动测试程序集编译因未传 `GSC_BUILD_COMMIT` 被源码身份门禁拒绝；按最终提交重新完整构建后再运行，未绕过身份校验。

## R00/R01 新鲜度与边界

- R00-06 的证据基线 source commit 更新到 `4121a47e`，本次复核针对自 `9c906cc0` 以来唯一命中的 `MediaCenterView.xaml.cs` 变化，确认新增的 footer 行距没有压掉窄窗表格下限或页级回退。之前的跨密度几何探针、RenderHarness 与 shell QA 报告仍保留在 [R00-06 历史证据](R00-06-MEDIA-FOUR-ROWS-20260916.md) 和 [9 月 29 日 R00/R01 复核](R00-R01-CURRENT-RECHECK-20260929/README.md)；本次没有重跑完整 RenderHarness 矩阵。
- `R01-07-freshness-report-20260929-wrap-rows.json` 在最终提交身份下核对全部 14 条 R00/R01，`14 FRESH / 0 STALE`；新鲜度自测的 docs-only/shared-control/package-identity 三门均通过。package commit 未提供，因此不推断用户安装包身份。
- 行为测试运行在合成数据、隔离 STA WPF 窗口和逻辑 DIP；没有启动 Playnite、验证物理 DPI、OS 输入或最终呈现帧。真实 Media Inbox 滚动后空白仍需在安全 Playnite 宿主中取得同次 `[GSC-GRID-DIAGNOSTIC]`。

## 可复现资料

- `q14-row-gap-postcommit.trx`
- `reported-row-gap-postcommit.trx`
- `media-geometry-700-postcommit.trx`
- `media-anchor-postcommit-final.trx`
