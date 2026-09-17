# R06-08 详情与行高预算验收证据

日期：2026-09-18  
分支：`codex/ui-finesse-round2`  
代码提交：`07376adb1d52a394137853896efe1b4589f983d1`

## 结论

R06-08 在当前可控范围内已满足。先复用现有 Task、Media、Save、Maintenance 详情区、选中绑定和滚动模型；只修正 Save 详情刷新后选中候选可能跳回第一个 Pending 项的缺口，并补真实生产视图的长详情/行高行为门禁。

## 既有能力核对与本阶段修正

- Task 已有独立 `TaskDetailScrollViewer`、折叠的 `TaskTechnicalDetailsExpander` 和失败详情优先级；长诊断留在详情区，不扩张所有 DataGrid 行。紧凑布局将表格行保持在 `36 DIP`，详情区使用独立高度预算；宽布局为侧栏详情。
- Media Inbox、Media、Maintenance 的列表与 Inspector/详情 ScrollViewer 已独立，选中对象使用既有 `Selected...` 绑定，紧凑详情打开时关闭前一个详情对象；没有替换游戏选框或滚动条系统。
- Save 刷新原先只按 `BackupId` 恢复备份，候选总是优先选第一个 Pending。`DashboardViewModel.RestoreSaveCandidateSelection` 现在先用稳定复合键 `PlayniteId + Path` 恢复原候选，再回退到 Pending/首项，保留 Accepted/Rejected 候选的详情对象。

## 行为与构建证据

- `R06DetailsBudgetBehaviorTests 2/2`：
  - Save 候选刷新后先恢复相同 `PlayniteId + Path`，找不到时才回退 Pending；大小写变化不丢失选中对象。
  - 真实生产 `TaskCenterView` 在 STA WPF Window 中由第一项切换到带 160 次长诊断文本的失败任务；详情对象更新为第二项，技术详情可展开，外层详情 ScrollViewer 可滚动，DataGrid 至少保留两行且行高不异常撑开。
- 相邻回归 `TaskCenterViewResponsiveTests`、`MediaInboxGeometryTests`、`DetailsDisclosureSourceTests` 合计 `10/10`。
- Release 构建：XAML `24/24`，生产 net462/测试 net472 编译 `0 warning / 0 error`；`validate-source.py` 与 `git diff --check` 通过。

## 离屏渲染与视觉核验

报告：`.tmp/r06-08-render-final/render-qa-report.txt`

- 报告绑定 `07376adb1d52a394137853896efe1b4589f983d1`，`WorkingTreeClean: True`，Light/Dark、1040×700/1100×720/1366×768/2560×1440 和 resize probe 均结束为 `render-qa OK`。
- Task 表格在 1040×700、1100×720、1366×768、2560×1440 分别报告 `5/4`、`5/4`、`4/4`、`8/4` 可读行；紧凑详情为 `Visible/160 DIP`，宽布局为 `Visible/360×516` 侧栏详情。
- Media Inbox 报告 `6/4` 或以上可读行；Maintenance Findings 报告 `5/4`、`6/4`、`8/4`；Save History/Candidates 报告至少 `4/4`。这些是合成数据的逻辑 DIP 门禁，不是物理屏幕像素或 presented frame 证明。
- 已查看代表图：`Task-1040x700.png`、`Task-1366x768.png`、`Save-1040x700-tab1.png`；任务宽窗显示独立详情卡与折叠技术详情，紧凑窗保留列表扫描密度，Save 候选表保留表头、行高和滚动。

## 边界与下一步

证据使用合成 DTO、fake/probe、隔离 STA WPF Window 和 offscreen logical DIP（`DpiScale=1.00`）。没有启动真实 Playnite/Worker，未验真实宿主的 UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能或真实服务失败时序；未写真实存档、媒体、云端或诊断数据。Demo 原始 `DesignShellView.xaml`/`Pages` 目录在当前 checkout 仍不存在，继续沿用恢复生产基线。

下一可执行任务为 R07-01「滚动所有权」：先盘点页面级、表格级、详情级和弹层级 ScrollViewer 的所有权与事件边界，再用实际长列表/详情夹具验证无重复滚动通道；保持当前游戏选框与滚动条系统。
