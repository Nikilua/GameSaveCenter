# 当前本地 UI 回归复核

日期：2026-10-21。SDK8 官方 Release 构建与二进制身份见 [S02 当前源码复核](../SDK8-S02-RECHECK-20261021/README.md)。本页记录本轮最后一组 UI/时间显示回归结果。

## 滚动视口与任务中心筛选行

- 已提交的页面高度约束修复仍以当前 ScrollViewer 有效 `ViewportHeight` 限制表格空间；存档历史、候选和任务队列的离屏 WPF 几何类在 SDK `8.0.423` 下 `7/7` 通过：[视口 TRX](tests/compact-table-viewport.trx)。它只证明当前源码的 STA/WPF 几何关系，不证明真实 Playnite 窗口或物理屏幕。
- 复核 Q14 紧凑宽度时发现另一个真实布局缺口：任务搜索框最小宽度为 `420 DIP`，旧的 `1216 DIP` 全筛选行断点在高 DPI/窄内容区下会让搜索框与状态筛选重叠。现在断点集中为 `TaskCenterView.CompactFilterBreakpoint = 1500 DIP`，更窄时将次要筛选保留在 disclosure 中；边界 `1499/1500 DIP`、浅/深主题、搜索框与状态/刷新区间距均由几何测试覆盖。工具栏布局 `1/1`、筛选标签/重挂接 `1/1`：[工具栏](tests/task-toolbar-geometry.trx)、[筛选标签](tests/task-filter-labels.trx)。

## 维护中心时间显示

保留策略对比中的版本明细改用相对时间；完整时间放在 Tooltip 和 UI Automation HelpText，比较生成时间也提供相对文本与完整时间。DTO 同时保留 raw UTC 投影。`R22TimeDisplayBehaviorTests` 当前 SDK8 `32/32` 通过：[TRX](tests/retention-time-display.trx)。该入口属于后续加入维护中心的保留策略明细；此项只调整显示，不改时间排序、策略、清理或恢复行为。

## 源码回归断言同步

- `R17FindingTriageSourceTests` 更新到当前查询的 `finding_id` 列偏移，2/2 通过。
- `WorkspaceStateSourceTests` 删除已不适用的 `selectedMediaId` 实现断言；其余 9 项通过，1 项保持既有历史跳过。
- `R21AutomationValueBehaviorTests` 更新存储指标的旧名称断言为当前准确语义“备份目录文件长度与卷容量对比”；完整类 24/24 通过。
- 这些维护只同步既有源码检查与当前实现；交付行为由滚动、筛选、时间投影和 UI Automation 定向行为结果覆盖，不以新增字符串检查代替实现。

相关 TRX：[R17](tests/finding-triage-source.trx)、[Workspace](tests/workspace-state-source.trx)、[R21](tests/r21-uia-values.trx)、[基线回归](tests/legacy-source-contracts.trx)。本轮未将模拟画面或合成数据称作真实屏幕证据。
