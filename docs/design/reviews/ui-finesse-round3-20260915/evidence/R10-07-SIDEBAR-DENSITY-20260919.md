# R10-07 侧栏信息密度证据

日期：2026-09-19
分支：`codex/ui-finesse-round2`
范围：生产 Shell 现有侧栏能力复核与实际 WPF 行为补证

## 结论

R10-07 已满足。本阶段没有重建导航体系或新增导航徽标：最新生产实现已经满足折叠侧栏保留图标、当前选中态、Tooltip、UI Automation 名称和键盘可达性的条件；展开态的固定侧栏列与主区星号列也已经把长名称风险限制在侧栏边界内。

## 现有实现核对

- `AcrylicProductionShellView.xaml` 的 `SidebarColumn` 展开为 `270 DIP`，折叠为 `72 DIP`；主内容使用相邻的 `*` 列，侧栏表面 `ClipToBounds=True`。
- 每个工作区入口仍是 `RadioButton`，保留独立 `ThemeAwareIcon`、当前选中模板、Tooltip、`AutomationProperties.Name` 和默认 Tab 键入口。折叠时只隐藏标签，将内容收窄并居中到图标槽位。
- `ApplySidebarLayout` 在收展后同步折叠按钮 glyph、Tooltip、Automation 名称和响应式布局；现有动画、Reduced Motion、清钟及状态恢复逻辑未改。
- 现有徽标是品牌版本徽标，不是覆盖导航图标的计数徽标；展开时与品牌图标分离，折叠时隐藏。没有发现需要另建的导航计数徽标。

## 实际行为验证

新增 `ProductionShellChromeSourceTests` 两个真实 STA WPF Window 测试：

1. `CollapsedSidebarPreservesSelectedWorkspaceAndAccessibleNavigation`：选中“任务中心”后折叠/展开，选中态保持；7 个入口逐项检查 Tab 键入口、Tooltip、Automation 名称和图标子节点；折叠宽度为 `72 DIP`，标签隐藏，图标保持可见，边界按钮名称切换为“展开导航栏”。
2. `ExpandedSidebarKeepsBrandBadgeClearAndLeavesMainAreaAvailableForLongLabels`：使用较长导航名称更新真实 TextBlock，检查展开宽度 `270 DIP`、品牌图标与版本徽标矩形不相交、侧栏裁剪开启、`MainPageHost.ActualWidth > 0`。

结果：

- `ProductionShellChromeSourceTests`：`12/12`；
- R10 组合（R10-01～R10-07 相关测试与生产 Shell）：`28/28`；
- Release/net462 solution：`0 warning / 0 error`；
- XAML structural validation：`24/24`；
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check`：通过。

本阶段只有测试与证据变更，没有修改生产 XAML、服务/DTO、命令绑定、游戏选框、滚动条、取消/错误语义、恢复保护或有限列表策略。

## 边界与清理

证据来自真实生产 `AcrylicProductionShellView`、隔离 STA WPF Window、合成长名称和逻辑 DIP；没有启动 Playnite/package-host，未宣称物理屏幕/DPI/跨屏、最终 presented frame、UIA/读屏、真实键盘/IME、ETW 或宿主性能通过。Demo 原目录仍不可用，继续沿用恢复生产基线。

本阶段没有新增可引用 `.tmp` 产物；清理旧 `.tmp` 时，已有历史构建目录仍受当前 worktree 的 Access denied/锁定句柄影响，未强杀未知进程。main 仍未触碰其用户文件：`DashboardView.xaml.cs`、`src.zip`、`DialogLifecycleStateMachine.cs`、`DialogOverlayMotion.cs`、`R08DialogOverlayBehaviorTests.cs`。

下一可执行任务：R10-08 最近操作续接；真实 Playnite 呈现、安装器 package-host 和上述未验边界继续保留。
