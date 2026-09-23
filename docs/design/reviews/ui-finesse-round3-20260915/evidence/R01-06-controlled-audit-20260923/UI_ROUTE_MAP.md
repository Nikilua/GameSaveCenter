# UI Route Map

生成时间：2026-09-23T02:07:06.6139880Z
Commit：5fbfc869ecddec852440ac82b3b0cc94343f3d60

路由来自真实 XAML 源码自动发现；新增页面加入 Dashboard 或 Views 目录后会自动出现在本文件。

## Dashboard 外壳 (`dashboard`)

- View：GameSaveCenter.Playnite.Views.DashboardView
- 文件：src\GameSaveCenter.Playnite\Views\DashboardView.xaml
- Tab 数量：6
  - 首页概览
    - ScrollViewer：HeaderScrollViewer、SidebarNavigationScrollViewer、TopActionsScroller、
    - 条件 UI：21 个
  - 媒体中心
  - 维护中心
  - 存档中心
  - 修改器中心
  - 任务工作台

## AcrylicProductionShellView (`acrylicproductionshellview`)

- View：GameSaveCenter.Playnite.Views.AcrylicProductionShellView
- 文件：src\GameSaveCenter.Playnite\Views\AcrylicProductionShellView.xaml
- Tab 数量：1
  - 页面
    - ScrollViewer：NavigationScrollViewer
    - 条件 UI：10 个

## Development 探针 (`dev-probe`)

- View：GameSaveCenter.Playnite.Views.Development.UiFrameworkProbeView
- 文件：src\GameSaveCenter.Playnite\Views\Development\UiFrameworkProbeView.xaml
- Tab 数量：1
  - 页面
    - DataGrid：ProbeGrid
    - ScrollViewer：

## 维护中心 (`maintenance`)

- View：GameSaveCenter.Playnite.Views.MaintenanceView
- 文件：src\GameSaveCenter.Playnite\Views\MaintenanceView.xaml
- Tab 数量：10
  - 诊断
    - DataGrid：FindingsGrid、PathRemapPreviewGrid
    - ScrollViewer：MaintenanceDiagnosticsInspector、MaintenanceDiagnosticsOverviewScrollSurface
    - 条件 UI：27 个
  - 云端队列
    - DataGrid：CloudTransferGrid
    - ScrollViewer：CloudTransferInspector
    - 条件 UI：10 个
  - 设备状态
    - DataGrid：MaintenanceDeviceGrid
    - ScrollViewer：MaintenanceDeviceInspectorScrollViewer
    - 条件 UI：7 个
  - 保留策略
    - ScrollViewer：、
    - 条件 UI：10 个
  - 异常与审计
    - DataGrid：MaintenanceAuditFindingsGrid、MaintenanceAuditLogGrid
    - ScrollViewer：MaintenanceAuditInspector
    - 条件 UI：13 个
  - 进程映射
    - DataGrid：MaintenanceProcessGrid
    - ScrollViewer：MaintenanceProcessInspectorScrollViewer
    - 条件 UI：4 个
  - 问题列表
    - DataGrid：FindingsGrid
    - ScrollViewer：MaintenanceDiagnosticsInspector
    - 条件 UI：10 个
  - 诊断概览
    - DataGrid：PathRemapPreviewGrid
    - ScrollViewer：MaintenanceDiagnosticsOverviewScrollSurface
    - 条件 UI：15 个
  - 发现的问题
    - DataGrid：MaintenanceAuditFindingsGrid
    - ScrollViewer：MaintenanceAuditInspector
    - 条件 UI：8 个
  - 审计记录
    - DataGrid：MaintenanceAuditLogGrid
    - 条件 UI：3 个

## 媒体中心 (`media-center`)

- View：GameSaveCenter.Playnite.Views.MediaCenterView
- 文件：src\GameSaveCenter.Playnite\Views\MediaCenterView.xaml
- Tab 数量：4
  - 待归类
    - DataGrid：MediaInboxGrid
    - ScrollViewer：MediaInboxPageScrollViewer、MediaInboxInspectorScrollViewer
    - 条件 UI：22 个
  - 当前游戏媒体
    - ScrollViewer：MediaInspectorScrollViewer
    - 条件 UI：18 个
  - 重复识别
    - ScrollViewer：MediaDuplicatePageScrollViewer
    - 条件 UI：4 个
  - 来源规则
    - ScrollViewer：MediaSourceFormScroller
    - 条件 UI：3 个

## 首页 (`overview`)

- View：GameSaveCenter.Playnite.Views.OverviewView
- 文件：src\GameSaveCenter.Playnite\Views\OverviewView.xaml
- Tab 数量：1
  - 页面
    - ScrollViewer：OverviewStackScrollSurface、OverviewRiskViewport、OverviewAttentionScrollViewer
    - 条件 UI：31 个

## 存档中心 (`save-center`)

- View：GameSaveCenter.Playnite.Views.SaveCenterView
- 文件：src\GameSaveCenter.Playnite\Views\SaveCenterView.xaml
- Tab 数量：4
  - 历史版本
    - DataGrid：SaveHistoryGrid
    - ScrollViewer：SaveHistoryActionsScrollViewer
    - 条件 UI：13 个
  - 路径与校验
    - DataGrid：SaveCandidateGrid
    - ScrollViewer：SaveCandidateInspectorScrollViewer
    - 条件 UI：4 个
  - 备份策略
    - ScrollViewer：、、
    - 条件 UI：11 个
  - 比较与保留
    - ScrollViewer：SaveComparePageScrollViewer、SaveCompareMainScrollViewer、SaveCompareRetentionScrollViewer

## 任务中心 (`task-center`)

- View：GameSaveCenter.Playnite.Views.TaskCenterView
- 文件：src\GameSaveCenter.Playnite\Views\TaskCenterView.xaml
- Tab 数量：1
  - 页面
    - DataGrid：TaskGrid
    - ScrollViewer：TaskDetailScrollViewer、
    - 条件 UI：21 个

## 修改器中心 (`trainer-center`)

- View：GameSaveCenter.Playnite.Views.TrainerCenterView
- 文件：src\GameSaveCenter.Playnite\Views\TrainerCenterView.xaml
- Tab 数量：4
  - 已绑定工具
    - ScrollViewer：TrainerToolsSettingsScrollViewer
    - 条件 UI：8 个
  - 导入确认
    - 条件 UI：4 个
  - FLiNG 在线库
    - 条件 UI：5 个
  - 可下载版本
    - ScrollViewer：TrainerReleaseInfoScrollViewer
    - 条件 UI：8 个

## 设置 (`settings`)

- View：GameSaveCenter.Playnite.Settings.GameSaveCenterSettingsView
- 文件：src\GameSaveCenter.Playnite\Settings\GameSaveCenterSettingsView.xaml
- Tab 数量：1
  - 页面
    - ScrollViewer：SettingsScroller
    - 条件 UI：2 个

