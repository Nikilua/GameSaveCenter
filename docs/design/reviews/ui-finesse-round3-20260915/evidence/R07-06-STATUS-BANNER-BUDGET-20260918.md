# R07-06 状态横幅预算

## 结论

R07-06 在当前分支已满足可控验收条件。最新测试提交为 `3551de81`；本项没有重建生产状态体系，而是复用当前已有的 `WorkspaceStatePresenter`、Stale 横幅、重试命令、安全模式恢复命令和表格最小高度契约，补上实际 WPF 状态/几何/命令证据。

## 行为证据

隔离 Release 输出：`.tmp\\r07-06-build-3551de81`，构建身份与提交一致；XAML `24/24`，solution `0 warning / 0 error`，Playnite 目标 `net462`。

- `R07StatusBannerBudgetBehaviorTests`：`4/4`。真实生产 `TaskCenterView`、`SaveCenterView`、`MaintenanceView` 在隔离 STA WPF Window 中使用合成 DTO/context：
  - Task 有旧任务记录时，加载失败和正在刷新均显示带 `RefreshCommand` 的横幅，表格 `MinHeight >=236 DIP` 且实际 viewport 不为零；没有旧记录时横幅收起、错误 `WorkspaceStatePresenter` 可见且重试命令仍可达。
  - Save Stale 横幅可见，`LoadDetailsCommand` 重试可达，历史表格 `MinHeight >=236 DIP` 且实际 viewport 保持。
  - Maintenance Stale 横幅可见，`RefreshDiagnosticsCommand` 可达，审计发现表格 `MinHeight >=260 DIP` 且实际 viewport 保持；安全模式告警显示明确的“恢复正常模式”命令，`ExitSafeModeCommand` 仍绑定。
- 相邻回归：`R06EmptyStateBehaviorTests 2/2`、`TaskCenterViewResponsiveTests 7/7`、`R06DetailsBudgetBehaviorTests 2/2`、`R07ScrollOwnershipBehaviorTests 2/2`；与本项合并运行 `17/17`，`0` 失败、`0` 跳过。
- `python scripts/validate-source.py` 与 `git diff --check` 通过；没有为本项修改服务/DTO、命令绑定、游戏选框或滚动条系统。

## 语义与边界

Stale/保留旧数据横幅只有重试入口，成功刷新后由状态绑定收起；安全模式告警用明确的恢复动作，不添加会掩盖失败的泛化关闭按钮。无旧数据的真实失败仍由错误 presenter 显示，不被空态伪装。

一次更宽的相邻合跑还观察到已有 `WorkspaceStateSourceTests.MediaInboxLoadingKeepsTheLatestModeAndIgnoresStaleSelections` 源码契约失败（期待的旧字符串不在当前 `DashboardViewModel.Media.cs`），以及一条已有 intentional skip；两者不在本项测试集合内、当前阶段未修改相关生产文件，未被改写为通过。

测试仅使用合成 DTO/fake、隔离 STA WPF 和 offscreen logical DIP；未启动真实 Playnite，不宣称物理 DPI/跨屏、OS 输入/触控板、UIA/读屏、presented frame、ETW、宿主帧率或性能，也未写真实存档、媒体、云端或诊断数据。Demo 原始目录仍不可用，继续沿用恢复生产基线。

下一项可执行任务：R07-07 触控板小增量。R07-06 未验真实宿主触控板增量、设备输入或 ETW/宿主性能。
