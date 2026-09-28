# Q11-07 工作区导航状态往返（2026-09-29）

## 结果

该任务的生产实现已存在：`AcrylicProductionShellView` 为各工作区缓存 `UserControl` 实例；重复导航时只在目标页实例变化时替换 `PageHost.Content`。Save、Media、Maintenance 页签通过双向绑定保存索引，Task Center 搜索框和筛选器也通过双向绑定写入共享导航状态。本阶段没有改生产代码、服务或 DTO，只补实测缺失的 WPF 往返行为证据。

新增 `CachedWorkspacePagesRestoreTabsAndTaskFiltersWithoutRefreshing`，使用生产 shell 和生产 UserControl/TabControl，绑定到隔离的 `NavigationViewState` fixture：

- Task Center 的搜索、状态、类型、历史范围、时间范围控件先从初始值改为非默认值，实测 TwoWay Binding 写回 fixture；离开页面再返回后控件值仍保持。
- Media、Save、Maintenance 的实际生产 TabControl 分别改到索引 `2/3/5`，绑定写回 fixture；依次切页、切到 Overview 再返回，索引仍为 `2/3/5`。
- 96 条合成任务中选择 `q11-07-task-55`，垂直滚动到 `18 DIP`；往返切页后任务集合引用/计数 `96`、选中对象和滚动偏移 `18→18 DIP` 均不变。
- 离开/返回后工作区页对象与 Task 页 DataContext fixture 保持同一引用，shell 中的 DashboardViewModel 引用也保持不变；绑定的合成 Task `RefreshCommand` 执行计数为 `0`。

既有 `CachedProductionPagesPreserveBindingSelectionAndScrollAcrossNavigation` 再实测一个 `10→10 DIP` 的偏移往返和 Measure/Arrange 布局运行；`PurposeNavigationSourceTests` 核对实际 sidebar 路由使用单例页面与 TwoWay Tab 索引绑定；`R10NavigationBehaviorTests` 实测 route stack 以 LIFO 恢复游戏、页签、筛选、任务 ID/index、列表选择和滚动，并拒绝 NaN/Infinity offset。

隔离 fixture 没有连接 Worker、仓库、Playnite profile 或真实查询服务；只观测合成 RefreshCommand 未被导航自动调用、合成任务集合未被替换。它证明受控视图往返状态，不代表实际业务查询计数或真实 Playnite 操作。

## 运行身份与环境

- 代码提交：`f9fa47f6aa93dc93951d27e3abf8cecbe8a771e5`。
- 行为测试源码 SHA-256：`604756CA372285B58359BDA4E5CCF7717A4A69AE051C1D8F669C539F06F26784`。
- 生产 shell `AcrylicProductionShellView.xaml.cs` SHA-256：`8B3FF716D0AEF9E1DE3FB563201D1E8CA83049613B003BDD86BB3D62C7081E81`。
- 测试窗口请求 `1366×900 DIP`，`WindowStyle=None`、不显示任务栏、`Opacity=0.01`；使用 STA WPF `.NET Framework 4.8.9181.0` testhost。没有强制 Light/Dark/FollowPlaynite，未读取窗口有效 Per-Monitor DPI，未固定字体。
- 页面/窗口实际尺寸没有保存为截图或几何转储；本用例没有屏幕捕获，窗口几乎透明。物理输入、主题视觉、DPI 行为和 Playnite host 仍未验证。

## 构建与测试

- 精确提交身份 Release Playnite 测试项目构建：`0 warning / 0 error`。
- `R08PageSwitchBehaviorTests`：`3/3` passed、0 failed/skipped；其中新往返测试通过。TRX：[页切换行为](Q11-07-PAGE-STATE-f9fa47f6.trx)。
- `PurposeNavigationSourceTests` 与 `R10NavigationBehaviorTests`：合计 `6/6` passed、0 failed/skipped。TRX：[路由与导航栈](Q11-07-NAV-STATE-f9fa47f6.trx)。

Q11-07 的自动行为门禁已通过；测试只使用合成 WPF 状态，视觉与宿主栏仍待验，因此 Round2 最终状态不签收。下一独立任务为 Q11-08 导航过渡。Media Inbox 滚动缺陷仍需真实 Playnite 同进程诊断，不能由本测试推断根因。
