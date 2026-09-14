# Q20-08 首页边界状态证据（2026-09-15）

## 结论

Q20-08 的共享实现与受控离屏证据已完成；真实 Playnite 宿主的状态切换与物理窗口交互仍不签收。

本轮发现并修复了一个真实布局缺口：Overview 的全局活动为空时，`WorkspaceStatePresenter` 虽然已根据 `Activities.Count == 0` 显示，但所在 `Auto` 行没有阅读下限，空态可能被压成不可见区域。生产 XAML 现在为 `OverviewActivityEmptyState` 设置 `MinHeight="120"`；新夹具实测实际高度为 `160 DIP`。

## 受控夹具

- 代码提交：`5a2ed09817bf492bef9c0bd0c67a8eda84292eac`（`补齐首页边界状态夹具`）。
- 专用命令：`tests/GameSaveCenter.RenderHarness/bin/Release/net472/GameSaveCenter.RenderHarness.exe overviewedges .tmp/overviewedges-clean-20260915`。
- 报告：[`.tmp/overviewedges-clean-20260915/overviewedges-report.txt`](../../../../../../.tmp/overviewedges-clean-20260915/overviewedges-report.txt)。报告记录 `Commit=5a2ed09`、`WorkingTreeClean=True`、`DpiScale=1.00`（离屏逻辑 DIP）。
- 覆盖 4 个夹具：空最近任务/空全局活动、多风险、96 字符超长当前游戏标题、Worker 离线；浅色/深色各覆盖 `1040×700` 与 `1600×900` 窗口，共 16 张首屏截图，空活动另生成 4 张页面尾部截图。

## 断言与视觉复核

- 16/16 输出为 `overviewedges OK`；每个视口均有 `surfaces=6/6`（Hero、当前游戏、六项统计条、最近任务、风险、需关注事项），Hero 主动作可见，页面无水平溢出。
- 空活动：`tasks=0 activities=0`，任务空态和全局活动空态均可见；`emptyHeight=160`，页面纵向 `Auto` 滚动到底部，1600×900 深色尾部图已实际查看，空态标题、说明和图标完整。
- 多风险：Hero 优先级为 `Attention`，主动作为“查看关注项”；风险区保留 6 条真实可见项目，风险视口 `434/434 DIP` 且没有隐藏滚动条，全部已渲染项目可达，额外风险通过“查看需要处理的游戏”入口继续进入业务工作区。
- 超长标题：当前游戏名长度 `96`，视觉树中的标题保持 `CharacterEllipsis`，原文 Tooltip 仍存在；1600×900 浅色图已实际查看，标题没有挤压状态行、指标胶囊或备份/刷新动作。
- 离线：`priority=Worker`，Hero 主动作固定为“打开维护中心”，`WorkerHealthy=False`；1600×900 深色和 1040×700 深色图已实际查看，错误色层级与当前游戏卡仍完整。

## 回归

- 标准干净 RenderHarness：[`.tmp/ui-qa-overview-empty-clean-20260915/render-qa-report.txt`](../../../../../../.tmp/ui-qa-overview-empty-clean-20260915/render-qa-report.txt)，提交 `5a2ed09`、`WorkingTreeClean=True`、Release 构建 `0 warning/0 error`、无 `PROBLEM`、`render-qa OK`。
- Core 全量：`83/83` 通过、0 跳过、0 失败。
- Playnite 全量：`474/537` 通过、`63` 跳过、0 失败；本轮定向 Overview/边界静态契约 `8/8` 通过。
- `python scripts/validate-source.py` 通过；WPF 静态审查退出 0，`326` 个 XAML、`0 errors / 151 warnings / 548 info`；警告主要来自 `.tmp` 中复制的 FusionX 主题与既有共享资源。
- `git diff --check` 通过。

## 保留边界

上述结果是生产 XAML 在 STA RenderHarness 中的双主题、逻辑 DIP 和视觉树证据，不替代真实 Playnite 中的状态切换、Hover/Focus、键盘滚动、用户主题、物理 DPI/多屏、中文 IME 或读屏操作。真实宿主边界继续由 [Q13–Q25 证据索引](../Q13-Q25-INDEX.md#q13q25-覆盖映射) 统一记录。
