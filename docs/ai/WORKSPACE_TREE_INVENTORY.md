# 页面树与响应式协调清单

更新时间：2026-08-26

## 结论

当前可见业务页面由 `DashboardView.xaml` 中的 `ProductionShellView` 承载，生产 Shell 的 `PageHost` 通过 `AcrylicProductionShellView.xaml.cs` 的 `pages` 字典创建并切换六个页面实例。旧页面树仍保留在同一个 Dashboard 中，但已经明确作为兼容面；本阶段没有删除它，也没有把离屏 RenderHarness 结果当作 Playnite 实机证明。

## 两棵树

| 区域 | 根节点 | 页面实例 | 当前职责 |
| --- | --- | --- | --- |
| 兼容树 | `DashboardDemoShell` / `MainShell` | `OverviewWorkspaceView`、`MediaWorkspaceView`、`MaintenanceWorkspaceView`、`SaveWorkspaceView`、`TrainerWorkspaceView`、`TaskWorkspaceView` | 保留旧资源、旧审计名称和现有兼容代码引用；根节点折叠，不作为生产页面导航宿主 |
| 生产树 | `ProductionShellView` | `OverviewView`、`SaveCenterView`、`TrainerCenterView`、`MediaCenterView`、`TaskCenterView`、`MaintenanceView` | 唯一可见的业务页面集合；`PageHost.Content` 是当前工作区的实际承载点 |

## 已迁移的业务路径

- Dashboard 导航、工作区加载和页面切换通过 `ProductionShellView.NavigateTo` 进入生产 `PageHost`。
- 响应式页面布局通过 `ProductionShellView.ApplyResponsiveLayout(width, height)` 分发到生产页面。
- 维护页定位、任务详情动画和工作区搜索焦点通过 `GetWorkspaceView<T>` 获取生产页面实例。
- Dashboard 主题与选中游戏背景资源同时覆盖兼容树和生产树，避免旧资源引用造成主题切换回退。
- 旧树只通过 `GetLegacyCompatibilityWorkspaceViews()` 参与兼容资源、主题和审计相关处理；不再接收生产页面的业务布局决策。

## 仍保留的引用与原因

- `DetailsTabControlForAudit`、旧页面的 `x:Name` 和旧资源入口仍存在，原因是当前没有真实 Playnite 宿主证据证明初始化、资源查找和外部审计引用均已脱离旧树。
- 删除旧树会扩大初始化和资源加载风险，且用户原始要求明确禁止在未证明安全前删除，因此本阶段只做兼容隔离，不做删除。
- `GscTableViewportHeight` 仍有资源字典、Dashboard 兼容资源和页面 XAML 引用；在完成外部引用清点前不删除或改名。

## 响应式协调入口

`ResponsiveLayoutCoordinator.Calculate(width, height)` 现在集中保存现有宽高断点及数值：960、980、1040、1080、1180、1200、1280 宽度相关决策，以及 650、700、760 高度相关决策。Dashboard 和生产 Shell 共用同一不可变状态；Shell 的 `SizeChanged` 延迟回调、导航和侧栏切换都只调用 `ApplyResponsiveLayout`，并在 `IsLoaded` 检查后执行。

本阶段只合并控制路径，没有改变既有断点、颜色、字体、间距、尺寸、动画、滚动模型或数据契约。Light/Dark 的现有 Render QA 覆盖仍是离屏证据；真实 Playnite 验证依照用户要求跳过 Phase 4，后续仍标记为人工验证项。
