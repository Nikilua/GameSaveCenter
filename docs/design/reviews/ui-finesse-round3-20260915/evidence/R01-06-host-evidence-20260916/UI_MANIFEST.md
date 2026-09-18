# UI Manifest

本文件是页面功能完整性的事实来源之一；截图没有显示不代表元素不存在。

## 汇总

- View 数量：10
- Tab 数量：32
- Button/ToggleButton 数量：246
- DataGrid 数量：14
- ScrollViewer 数量：34
- 条件 UI 数量：247
- Expander 数量：16
- ComboBox 数量：34
- CheckBox 数量：3
- TextBox/PasswordBox 数量：54
- TextBlock 数量：857

## Dashboard 外壳

文件：src\GameSaveCenter.Playnite\Views\DashboardView.xaml

### 首页概览

#### 滚动容器

- ScrollViewer | HeaderScrollViewer
- ScrollViewer | SidebarNavigationScrollViewer
- ScrollViewer | TopActionsScroller
- ScrollViewer | V=Auto, H=Disabled

#### 条件显示 UI

- Border | DashboardDemoShell | Style={StaticResource GscShellStyle}
- Border | SidebarChrome | Style={StaticResource GscRedesignSidebarSurface}
- Border | SidebarWorkerStatusCard | Style={StaticResource GscRedesignStatusCard}
- Border | SidebarWorkerStatusDot
- Border | SidebarLudusaviStatusCard | Style={StaticResource GscRedesignStatusCard}
- Border | SidebarLudusaviStatusDot
- Border | GameBrowserPanel | Style={StaticResource GscRedesignFloatingPickerCard}
- Border | 当前选择不在当前筛选结果中
- Border | GameDetailCard | Style={StaticResource GscWorkspaceHostSurface}
- Border | SelectedGameHeader | Style={StaticResource GscRedesignSectionCard}
- Border | SelectedGameHealthPill | Style={StaticResource GscRedesignContextPill}
- Border | 
- Border | DemoFooter
- Border | StatusPill
- Border | 
- Border | 后台更新
- Button | 
- ListBox | {Binding Initials}
- TextBlock | 
- TextBlock | 
- TextBlock | 搜索游戏…
- TextBlock | 没有符合条件的游戏
请调整搜索或筛选条件。


### 媒体中心


### 维护中心


### 存档中心


### 修改器中心


### 任务工作台


## AcrylicProductionShellView

文件：src\GameSaveCenter.Playnite\Views\AcrylicProductionShellView.xaml

### 页面

#### 操作与输入

- Button | SidebarCollapseButton | Style={StaticResource AcrylicSidebarBoundaryButton}
- Button | GameContextButton | Style={DynamicResource GscRedesignGameContextButton}
- Button | HeaderRefreshButton | Command={Binding RefreshCommand} | Style={DynamicResource GscIconOnlyToolbarButton}
- Button | HeaderMediaButton | Command={Binding SyncMediaCommand} | Style={DynamicResource GscRedesignHeaderButton}
- Button | HeaderBackupSelectedButton | Command={Binding BackupSelectedCommand} | Style={DynamicResource GscRedesignPrimaryHeaderButton}
- Button | HeaderBackupButton | Command={Binding BackupAllCommand} | Style={DynamicResource GscRedesignPrimaryHeaderButton}
- Button | GameSearchClearButton
- ComboBox | GamePickerStatusComboBox | Style={DynamicResource GscWpfUiPickerFilterComboBox}
- ComboBox | GamePickerPlatformComboBox | Style={DynamicResource GscWpfUiPickerFilterComboBox}
- ComboBox | GamePickerSortComboBox | Style={DynamicResource GscWpfUiPickerFilterComboBox}
- RadioButton | NavOverview | Style={StaticResource AcrylicNavItem}
- RadioButton | NavSaves | Style={StaticResource AcrylicNavItem}
- RadioButton | NavTrainers | Style={StaticResource AcrylicNavItem}
- RadioButton | NavMedia | Style={StaticResource AcrylicNavItem}
- RadioButton | NavTasks | Style={StaticResource AcrylicNavItem}
- RadioButton | NavMaintenance | Style={StaticResource AcrylicNavItem}
- RadioButton | NavSettings | Style={StaticResource AcrylicNavItem}
- TextBox | GameSearchTextBox | Style={DynamicResource GscWpfUiTextBox}

#### 滚动容器

- ScrollViewer | NavigationScrollViewer

#### 条件显示 UI

- Border | DemoShell
- Border | PickerPanel | Style={DynamicResource GscRedesignFloatingPickerCard}
- Border | FooterSurface
- Button | GameSearchClearButton
- TextBlock | 搜索游戏
- TextBlock | 
- TextBlock | 


## Development 探针

文件：src\GameSaveCenter.Playnite\Views\Development\UiFrameworkProbeView.xaml

### 页面

#### 操作与输入

- Button |  | Style={StaticResource GscWpfUiPrimaryButton}
- Button |  | Style={StaticResource GscWpfUiSecondaryButton}
- Button |  | Style={StaticResource GscWpfUiSecondaryButton}
- Button |  | Style={StaticResource GscIconOnlyToolbarButton}
- CheckBox |  | Style={StaticResource GscCheckBox}
- ComboBox |  | Style={StaticResource GscWpfUiComboBox}
- Slider |  | Style={StaticResource GscSlider}
- TextBox | 可编辑文本 | Style={StaticResource GscWpfUiTextBox}

#### 数据表

- DataGrid | ProbeGrid

#### 滚动容器

- ScrollViewer | V=Auto, H=Disabled


## 维护中心

文件：src\GameSaveCenter.Playnite\Views\MaintenanceView.xaml

### 诊断

#### 操作与输入

- Button | MaintenanceDiagnosticsCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding RefreshDiagnosticsCommand} | Style={DynamicResource GscIconOnlyToolbarButton}
- Button |  | Command={Binding DataContext.CopyDiagnosticsCommand} | Style={DynamicResource GscIconOnlyButtonBase}
- Button |  | Command={Binding DataContext.OpenSelectedFindingNavigationCommand}
- Button |  | Command={Binding RefreshDiagnosticsCommand} | Style={DynamicResource GscIconOnlyToolbarButton}
- Button |  | Command={Binding DataContext.LoadMoreRetentionQuarantineCommand}
- Button |  | Command={Binding RunEnvironmentCheckCommand} | Style={DynamicResource GscWpfUiToolbarPrimaryButton}
- Button |  | Command={Binding OnboardingTestBackupCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding CompleteOnboardingCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding SkipOnboardingCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RefreshDiagnosticsCommand} | Style={DynamicResource GscWpfUiToolbarPrimaryButton}
- Button |  | Command={Binding CopyDiagnosticsCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding CreateDiagnosticsPackageCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding CopyMaintenanceReportCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ExportMaintenanceReportCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RunIntegrityCheckCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RunHealthInspectionCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding OpenDataDirectoryCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding OpenBackupDirectoryCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding OpenMediaDirectoryCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding OpenWorkerLogCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RebuildRepositoryCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ReconcileTasksCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding CreateMetadataBackupCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RestoreMetadataBackupCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ExitSafeModeCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RunPathRemapCommand} | Style={StaticResource GscWpfUiActionButton}
- Button |  | Command={Binding DiagnoseGameCommand} | Style={DynamicResource GscWpfUiToolbarPrimaryButton}
- Button |  | Command={Binding ClearGamePickerFiltersCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding SyncGameDescriptorCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RetryGameMatchCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- TextBox | {Binding PathRemapOldRoot, UpdateSourceTrigger=LostFocus} | Style={StaticResource GscWpfUiTextBox}
- TextBox | {Binding PathRemapNewRoot, UpdateSourceTrigger=LostFocus} | Style={StaticResource GscWpfUiTextBox}
- TextBox | {Binding GameDiagnosticPlayniteId, UpdateSourceTrigger=PropertyChanged} | Style={DynamicResource GscWpfUiTextBox}
- TextBox | {Binding GameDiscoveryDiagnosticSummary, Mode=OneWay} | Style={DynamicResource GscWpfUiTextBox}
- TextBox | {Binding DiagnosticSummary, Mode=OneWay} | Style={DynamicResource GscWpfUiTextBox}

#### 数据表

- DataGrid | FindingsGrid

#### 滚动容器

- ScrollViewer | MaintenanceDiagnosticsInspector
- ScrollViewer | MaintenanceDiagnosticsOverviewScrollSurface

#### 折叠区域

- Expander | {Binding OverflowHeader}
- Expander | EnvironmentCheckDisclosure
- Expander | MaintenanceActionsDisclosure
- Expander | 目录与日志
- Expander | 完整性、自愈与安全模式
- Expander | 元数据灾备

#### 条件显示 UI

- Border | MaintenanceDiagnosticsTable
- Border | 问题详情
- Border | DiagnosticsInspectorSeverity
- Border | MaintenanceNextStepsCard | Style={DynamicResource GscReadingCardStyle}
- Border | {Binding Title} | Style={DynamicResource GscRedesignSubCard}
- Border | DiagnosticHealthCard | Style={DynamicResource GscReadingCardStyle}
- Border | Rclone | Style={DynamicResource GscRedesignMetricBorder}
- Border | MaintenanceDiagnosticsActionCard | Style={DynamicResource GscReadingCardStyle}
- Border | 安全模式已开启：自动备份、自动媒体同步、云端上传和工具自动启动已暂停；手动操作仍可用。
- Button |  | Command={Binding DataContext.OpenSelectedFindingNavigationCommand}
- Button |  | Command={Binding DataContext.LoadMoreRetentionQuarantineCommand}
- DataGrid | FindingsGrid | Style={StaticResource MaintenanceDataGrid}
- Expander | 
- Expander | MaintenanceActionsDisclosure | Style={StaticResource GscDisclosureCard}
- ItemsControl | {Binding Title}
- ScrollViewer | MaintenanceDiagnosticsInspector
- ScrollViewer | MaintenanceDiagnosticsOverviewScrollSurface | Style={DynamicResource GscPageScrollViewer}
- TabControl | MaintenanceDiagnosticsSubTabs | Style={StaticResource GscInternalTabControl}
- TabItem | 发现的问题
- TabItem | 发现的问题
- TabItem | 下一步运维
- TextBlock | 当前没有需要处理的诊断项。
Worker、备份和媒体状态正常时，这里会保持为空。
- TextBlock | 


### 云端队列

#### 操作与输入

- Button |  | Command={Binding RefreshCloudTransfersCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button | CloudTransferCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding OpenMaintenanceCommand}
- Button |  | Command={Binding VerifyCloudTransferCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding RetryCloudUploadCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding LoadMoreCloudTransfersCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- ComboBox | {Binding Display} | Style={DynamicResource GscWpfUiComboBox}
- ComboBox | {Binding Display} | Style={DynamicResource GscWpfUiComboBox}

#### 数据表

- DataGrid | CloudTransferGrid

#### 滚动容器

- ScrollViewer | CloudTransferInspector

#### 条件显示 UI

- Border | CloudTransferTableFrame | Style={DynamicResource GscRedesignTableFrame}
- Border | 队列详情 | Style={DynamicResource GscRedesignSectionCard}
- Border | {Binding CloudTransferAvailabilityHint, Mode=OneWay} | Style={DynamicResource GscActionAvailabilityHintBubble}
- Button |  | Command={Binding OpenMaintenanceCommand}
- ScrollViewer | CloudTransferInspector | Style={DynamicResource GscInspectorScrollViewer}
- TabItem | 待处理
- TextBlock | 暂无符合筛选条件的云端传输记录。
上传成功不代表远端已校验，请选择记录查看保证级别。


### 设备状态

#### 操作与输入

- Button |  | Command={Binding SyncDeviceStatesCommand} | Style={DynamicResource GscWpfUiContextButton}
- Button | MaintenanceDeviceCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding SaveDeviceDecisionCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding StageRemoteBackupCommand} | Style={DynamicResource GscWpfUiRemoteRestoreButton}
- Button |  | Command={Binding RestoreStagedRemoteBackupCommand} | Style={DynamicResource GscWpfUiRemoteRestoreButton}
- ComboBox |  | Style={DynamicResource GscWpfUiComboBox}
- TextBox | {Binding DeviceDecisionComment, UpdateSourceTrigger=PropertyChanged} | Style={DynamicResource GscWpfUiTextBox}

#### 数据表

- DataGrid | MaintenanceDeviceGrid

#### 滚动容器

- ScrollViewer | MaintenanceDeviceInspectorScrollViewer

#### 条件显示 UI

- Border | 设备对比 | Style={DynamicResource GscTableFrame}
- TabItem | 多设备云目录
- TextBlock | 暂无设备冲突记录。
同步设备摘要后，有差异的游戏会显示在这里。


### 保留策略

#### 操作与输入

- Button |  | Command={Binding PreviewRetentionCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RefreshStorageAnalysisCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RefreshRetentionSimulationCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ApplyRetentionSimulationCommand} | Style={DynamicResource GscWpfUiContextButton}
- Button |  | Command={Binding RefreshLocalMirrorStatusCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding SyncLocalMirrorCommand} | Style={DynamicResource GscWpfUiContextButton}
- Button |  | Command={Binding PreviewRetentionCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding RefreshStorageAnalysisCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding RefreshRetentionSimulationCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ApplyRetentionSimulationCommand} | Style={DynamicResource GscWpfUiContextButton}
- Button |  | Command={Binding RefreshLocalMirrorStatusCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding SyncLocalMirrorCommand} | Style={DynamicResource GscWpfUiContextButton}

#### 滚动容器

- ScrollViewer | V=Auto, H=Disabled
- ScrollViewer | V=Auto, H=Disabled

#### 条件显示 UI

- Border | MaintenanceRetentionKeepCard | Style={DynamicResource GscReadingCardStyle}
- Border | MaintenanceRetentionDeleteCard | Style={DynamicResource GscReadingCardStyle}
- Border | 备份存储分析 | Style={DynamicResource GscReadingCardStyle}
- Border | 全局保留策略模拟器 | Style={DynamicResource GscReadingCardStyle}
- ScrollViewer | 当前游戏保留预览 | Style={DynamicResource GscPageScrollViewer}
- TabItem | 当前游戏保留预览
- TextBlock | 暂无保留明细，点击“刷新预览”生成。
- TextBlock | 当前无候选清理。
- TextBlock | 暂无排行数据。点击“刷新存储分析”生成。
- TextBlock | 暂无候选明细。点击“刷新全局预览”生成。


### 异常与审计

#### 操作与输入

- Button |  | Command={Binding RefreshDiagnosticsCommand} | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding DataContext.CopyDiagnosticsCommand} | Style={DynamicResource GscWpfUiToolbarButton}

#### 数据表

- DataGrid | MaintenanceAuditFindingsGrid
- DataGrid | MaintenanceAuditLogGrid

#### 滚动容器

- ScrollViewer | MaintenanceAuditInspector

#### 条件显示 UI

- Border | MaintenanceAuditFindingsTable
- Border | MaintenanceStaleBanner
- Border | 问题详情
- Border | AuditInspectorSeverity
- Border | 暂无审计记录。
同步、备份和媒体操作完成后会在这里留下时间线。 | Style={StaticResource GscTableFrame}
- DataGrid | MaintenanceAuditFindingsGrid | Style={StaticResource MaintenanceDataGrid}
- ScrollViewer | MaintenanceAuditInspector
- TabControl | {Binding MaintenanceStateTitle} | Style={StaticResource GscInternalTabControl}
- TabItem | {Binding MaintenanceStateTitle}
- TabItem | {Binding MaintenanceStateTitle}
- TabItem | 最近审计
- TextBlock | 当前没有需要处理的诊断项。
Worker、备份和媒体状态正常时，这里会保持为空。
- TextBlock | 暂无审计记录。
同步、备份和媒体操作完成后会在这里留下时间线。


### 进程映射

#### 操作与输入

- Button | ProcessMappingSaveButton | Command={Binding SaveProcessMappingCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button | MaintenanceProcessCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding DataContext.DeleteProcessMappingCommand} | Style={DynamicResource GscWpfUiContextButton}
- Button |  | Command={Binding DeleteProcessMappingCommand} | Style={DynamicResource GscWpfUiContextButton}
- ComboBox | ProcessMappingTargetGameComboBox | Style={DynamicResource GscWpfUiComboBox}
- TextBox | ProcessMappingExecutableTextBox | Style={DynamicResource GscWpfUiTextBox}

#### 数据表

- DataGrid | MaintenanceProcessGrid

#### 滚动容器

- ScrollViewer | MaintenanceProcessInspectorScrollViewer

#### 条件显示 UI

- Border | MaintenanceProcessTable
- Border | MaintenanceProcessInspector
- TabItem | 映射编辑器
- TextBlock | 暂无进程映射。
绑定 EXE 文件名后，MOD 启动器等进程会被识别为对应游戏。


### 问题列表

#### 操作与输入

- Button | MaintenanceDiagnosticsCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding RefreshDiagnosticsCommand} | Style={DynamicResource GscIconOnlyToolbarButton}
- Button |  | Command={Binding DataContext.CopyDiagnosticsCommand} | Style={DynamicResource GscIconOnlyButtonBase}
- Button |  | Command={Binding DataContext.OpenSelectedFindingNavigationCommand}

#### 数据表

- DataGrid | FindingsGrid

#### 滚动容器

- ScrollViewer | MaintenanceDiagnosticsInspector

#### 条件显示 UI

- Border | MaintenanceDiagnosticsTable
- Border | 问题详情
- Border | DiagnosticsInspectorSeverity
- Button |  | Command={Binding DataContext.OpenSelectedFindingNavigationCommand}
- DataGrid | FindingsGrid | Style={StaticResource MaintenanceDataGrid}
- ScrollViewer | MaintenanceDiagnosticsInspector
- TabItem | 发现的问题
- TextBlock | 当前没有需要处理的诊断项。
Worker、备份和媒体状态正常时，这里会保持为空。


### 诊断概览

#### 操作与输入

- Button |  | Command={Binding RefreshDiagnosticsCommand} | Style={DynamicResource GscIconOnlyToolbarButton}
- Button |  | Command={Binding DataContext.LoadMoreRetentionQuarantineCommand}
- Button |  | Command={Binding RunEnvironmentCheckCommand} | Style={DynamicResource GscWpfUiToolbarPrimaryButton}
- Button |  | Command={Binding OnboardingTestBackupCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding CompleteOnboardingCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding SkipOnboardingCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RefreshDiagnosticsCommand} | Style={DynamicResource GscWpfUiToolbarPrimaryButton}
- Button |  | Command={Binding CopyDiagnosticsCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding CreateDiagnosticsPackageCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding CopyMaintenanceReportCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ExportMaintenanceReportCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RunIntegrityCheckCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RunHealthInspectionCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding OpenDataDirectoryCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding OpenBackupDirectoryCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding OpenMediaDirectoryCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding OpenWorkerLogCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RebuildRepositoryCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ReconcileTasksCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding CreateMetadataBackupCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RestoreMetadataBackupCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ExitSafeModeCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RunPathRemapCommand} | Style={StaticResource GscWpfUiActionButton}
- Button |  | Command={Binding DiagnoseGameCommand} | Style={DynamicResource GscWpfUiToolbarPrimaryButton}
- Button |  | Command={Binding ClearGamePickerFiltersCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding SyncGameDescriptorCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding RetryGameMatchCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- TextBox | {Binding PathRemapOldRoot, UpdateSourceTrigger=LostFocus} | Style={StaticResource GscWpfUiTextBox}
- TextBox | {Binding PathRemapNewRoot, UpdateSourceTrigger=LostFocus} | Style={StaticResource GscWpfUiTextBox}
- TextBox | {Binding GameDiagnosticPlayniteId, UpdateSourceTrigger=PropertyChanged} | Style={DynamicResource GscWpfUiTextBox}
- TextBox | {Binding GameDiscoveryDiagnosticSummary, Mode=OneWay} | Style={DynamicResource GscWpfUiTextBox}
- TextBox | {Binding DiagnosticSummary, Mode=OneWay} | Style={DynamicResource GscWpfUiTextBox}

#### 滚动容器

- ScrollViewer | MaintenanceDiagnosticsOverviewScrollSurface

#### 折叠区域

- Expander | {Binding OverflowHeader}
- Expander | EnvironmentCheckDisclosure
- Expander | MaintenanceActionsDisclosure
- Expander | 目录与日志
- Expander | 完整性、自愈与安全模式
- Expander | 元数据灾备

#### 条件显示 UI

- Border | MaintenanceNextStepsCard | Style={DynamicResource GscReadingCardStyle}
- Border | {Binding Title} | Style={DynamicResource GscRedesignSubCard}
- Border | DiagnosticHealthCard | Style={DynamicResource GscReadingCardStyle}
- Border | Rclone | Style={DynamicResource GscRedesignMetricBorder}
- Border | MaintenanceDiagnosticsActionCard | Style={DynamicResource GscReadingCardStyle}
- Border | 安全模式已开启：自动备份、自动媒体同步、云端上传和工具自动启动已暂停；手动操作仍可用。
- Button |  | Command={Binding DataContext.LoadMoreRetentionQuarantineCommand}
- Expander | 
- Expander | MaintenanceActionsDisclosure | Style={StaticResource GscDisclosureCard}
- ItemsControl | {Binding Title}
- ScrollViewer | MaintenanceDiagnosticsOverviewScrollSurface | Style={DynamicResource GscPageScrollViewer}
- TabItem | 下一步运维
- TextBlock | 


### 发现的问题

#### 操作与输入

- Button |  | Command={Binding RefreshDiagnosticsCommand} | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding DataContext.CopyDiagnosticsCommand} | Style={DynamicResource GscWpfUiToolbarButton}

#### 数据表

- DataGrid | MaintenanceAuditFindingsGrid

#### 滚动容器

- ScrollViewer | MaintenanceAuditInspector

#### 条件显示 UI

- Border | MaintenanceAuditFindingsTable
- Border | MaintenanceStaleBanner
- Border | 问题详情
- Border | AuditInspectorSeverity
- DataGrid | MaintenanceAuditFindingsGrid | Style={StaticResource MaintenanceDataGrid}
- ScrollViewer | MaintenanceAuditInspector
- TabItem | {Binding MaintenanceStateTitle}
- TextBlock | 当前没有需要处理的诊断项。
Worker、备份和媒体状态正常时，这里会保持为空。


### 审计记录

#### 数据表

- DataGrid | MaintenanceAuditLogGrid

#### 条件显示 UI

- Border | 暂无审计记录。
同步、备份和媒体操作完成后会在这里留下时间线。 | Style={StaticResource GscTableFrame}
- TabItem | 最近审计
- TextBlock | 暂无审计记录。
同步、备份和媒体操作完成后会在这里留下时间线。


## 媒体中心

文件：src\GameSaveCenter.Playnite\Views\MediaCenterView.xaml

### 待归类

#### 操作与输入

- Button |  | Command={Binding ReloadMediaInboxCommand} | Style={DynamicResource GscWpfUiSecondaryButton}
- Button | MediaInboxClearSelectionButton | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding AssignInboxMediaBatchCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button | MediaInboxCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding OpenMaintenanceCommand}
- Button | ReloadMediaInboxButton | Command={Binding ReloadMediaInboxCommand}
- Button | MediaInboxHistoryButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding LoadMoreMediaInboxCommand}
- Button |  | Command={Binding IgnoreInboxMediaBatchCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding PreviewMediaClassificationCommand} | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding ApplyMediaClassificationCommand}
- Button |  | Command={Binding RestoreIgnoredMediaBatchCommand}
- Button |  | Command={Binding UndoMediaClassificationCommand} | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding CopyPathCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding RefreshMediaClassificationHistoryCommand} | Style={DynamicResource GscIconOnlyButtonBase}
- Button |  | Command={Binding LoadMoreMediaClassificationHistoryCommand}
- Button |  | Command={Binding UndoMediaClassificationCommand} | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding AssignInboxMediaCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding IgnoreInboxMediaCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding RestoreIgnoredMediaBatchCommand}
- ComboBox | MediaInboxModeCombo | Style={DynamicResource GscWpfUiFilterComboBox}
- ComboBox | {Binding Name} | Style={DynamicResource GscWpfUiComboBox}
- ComboBox | MediaClassificationHistoryStateCombo | Style={DynamicResource GscWpfUiComboBox}
- ComboBox | {Binding Name} | Style={DynamicResource GscWpfUiComboBox}
- TextBox | {Binding SelectedInboxMedia.ArchivePath, Mode=OneWay, TargetNullValue=未生成归档路径} | Style={StaticResource GscWpfUiPathDetailTextBox}

#### 数据表

- DataGrid | MediaInboxGrid

#### 滚动容器

- ScrollViewer | MediaInboxPageScrollViewer
- ScrollViewer | MediaInboxInspectorScrollViewer

#### 条件显示 UI

- Border | MediaInboxTableFrame | Style={StaticResource MediaTableFrame}
- Border | MediaInboxStaleBanner
- Border | {Binding MediaInboxAvailabilityHint, Mode=OneWay} | Style={DynamicResource GscActionAvailabilityHintBubble}
- Border | MediaInboxInspectorFrame | Style={DynamicResource GscReadingCardStyle}
- Border | MediaInboxPreviewPanel
- Border | 归类建议预览 | Style={DynamicResource GscRedesignInfoBand}
- Button |  | Command={Binding OpenMaintenanceCommand}
- Button |  | Command={Binding LoadMoreMediaInboxCommand}
- Button |  | Command={Binding ApplyMediaClassificationCommand}
- Button |  | Command={Binding RestoreIgnoredMediaBatchCommand}
- Button |  | Command={Binding LoadMoreMediaClassificationHistoryCommand}
- Button |  | Command={Binding RestoreIgnoredMediaBatchCommand}
- ListBox | MediaClassificationPreviewItems
- ScrollViewer | MediaInboxPageScrollViewer | Style={DynamicResource GscPageScrollViewer}
- ScrollViewer | MediaInboxInspectorScrollViewer | Style={DynamicResource GscInspectorScrollViewer}
- TabItem | {Binding MediaInboxTitle, Mode=OneWay}
- TextBlock | {Binding MediaInboxEmptyText, Mode=OneWay}
- TextBlock | 


### 当前游戏媒体

#### 操作与输入

- Button | 
- Button |  | Command={Binding ClearMediaFiltersCommand}
- Button |  | Command={Binding ReloadMediaWindowCommand} | Style={DynamicResource GscWpfUiSecondaryButton}
- Button | ReloadMediaWindowButton | Command={Binding ReloadMediaWindowCommand}
- Button |  | Command={Binding LoadMoreMediaCommand}
- Button |  | Command={Binding FavoriteSelectedMediaCommand} | Style={DynamicResource GscWpfUiMediaBatchButton}
- Button |  | Command={Binding UnfavoriteSelectedMediaCommand} | Style={DynamicResource GscWpfUiMediaBatchButton}
- Button |  | Command={Binding CommentSelectedMediaCommand} | Style={DynamicResource GscWpfUiMediaBatchButton}
- Button | MediaCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding CopyPathCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding UpdateMediaMetadataCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding OpenSelectedMediaCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding RevealSelectedMediaCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding ReassignMediaCommand} | Style={DynamicResource GscWpfUiContextButton}
- Button |  | Command={Binding FavoriteSelectedMediaCommand} | Style={DynamicResource GscWpfUiMediaBatchButton}
- Button |  | Command={Binding UnfavoriteSelectedMediaCommand} | Style={DynamicResource GscWpfUiMediaBatchButton}
- Button |  | Command={Binding CommentSelectedMediaCommand} | Style={DynamicResource GscWpfUiMediaBatchButton}
- ComboBox |  | Style={DynamicResource GscWpfUiFilterComboBox}
- ComboBox | {Binding Name} | Style={DynamicResource GscWpfUiComboBox}
- TextBox | MediaSearchTextBox | Style={DynamicResource GscWpfUiTextBox}
- TextBox | {Binding SelectedMedia.ArchivePath, Mode=OneWay, TargetNullValue=未生成归档路径} | Style={StaticResource GscWpfUiPathDetailTextBox}
- TextBox | {Binding MediaComment, UpdateSourceTrigger=PropertyChanged} | Style={DynamicResource GscWpfUiTextBox}

#### 滚动容器

- ScrollViewer | MediaInspectorScrollViewer

#### 条件显示 UI

- Border | 媒体 | Style={StaticResource MediaTableFrame}
- Border | 录像
- Border | MediaCurrentStaleBanner
- Border | MediaInspectorFrame
- Border | MediaInspectorPanel | Style={DynamicResource GscReadingCardStyle}
- Border | MediaPreviewPanel
- Button | 
- Button |  | Command={Binding ClearMediaFiltersCommand}
- Button |  | Command={Binding LoadMoreMediaCommand}
- ListBox | MediaGrid
- ScrollViewer | MediaInspectorScrollViewer
- TabItem | 媒体
- TextBlock | 搜索媒体
- TextBlock | 录像
- TextBlock | ★
- TextBlock | 当前游戏还没有媒体
导入截图或录像后，它们会显示在这里。


### 来源规则

#### 操作与输入

- Button |  | Command={Binding AddMediaSourceCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding DataContext.DeleteMediaSourceCommand} | Style={DynamicResource GscWpfUiContextDangerButton}
- TextBox | {Binding CustomMediaSourcePath, UpdateSourceTrigger=PropertyChanged} | Style={DynamicResource GscWpfUiTextBox}
- TextBox | {Binding CustomMediaPattern, UpdateSourceTrigger=PropertyChanged} | Style={DynamicResource GscWpfUiTextBox}

#### 滚动容器

- ScrollViewer | MediaSourceFormScroller

#### 条件显示 UI

- Border | MediaSourceRulesFrame | Style={StaticResource MediaTableFrame}
- TabItem | 添加截图或录像来源
- TextBlock | 暂无媒体来源规则
添加公共截图或录像目录后，来源会显示在这里。


## 首页

文件：src\GameSaveCenter.Playnite\Views\OverviewView.xaml

### 页面

#### 操作与输入

- Button |  | Command={Binding RefreshCommand} | Style={DynamicResource GscWpfUiCompactButton}
- Button |  | Command={Binding BackupAllCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding SyncMediaCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding OverviewPriorityActionCommand} | Style={DynamicResource GscWpfUiPrimaryButton}
- Button |  | Command={Binding BackupSelectedCommand} | Style={DynamicResource GscWpfUiCompactButton}
- Button |  | Command={Binding LoadDetailsCommand} | Style={DynamicResource GscWpfUiCompactButton}
- Button | {Binding Snapshot.CloudTransfers.QueueControlDisplay, Mode=OneWay} | Command={Binding OpenCloudQueueCommand} | Style={StaticResource OverviewCloudQueueCardButton}
- Button |  | Command={Binding RefreshCommand} | Style={DynamicResource GscWpfUiCompactButton}
- Button | {Binding KindDisplay, Mode=OneWay} | Command={Binding DataContext.OpenActivityCommand} | Style={StaticResource OverviewActivityRowButton}
- Button |  | Command={Binding OpenProtectionGamesCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ApplyRecommendedProtectionCommand} | Style={DynamicResource GscWpfUiToolbarPrimaryButton}
- Button |  | Command={Binding OpenAttentionCenterCommand} | Style={StaticResource OverviewFindingActionButton}

#### 滚动容器

- ScrollViewer | OverviewStackScrollSurface
- ScrollViewer | OverviewRiskViewport
- ScrollViewer | OverviewAttentionScrollViewer

#### 条件显示 UI

- Border | OverviewTodayHeroCard | Style={DynamicResource GscRedesignSectionCard}
- Border |  | Style={DynamicResource GscRedesignContextPill}
- Border |  | Style={DynamicResource GscRedesignContextPill}
- Border | {Binding Snapshot.WarningGames, Mode=OneWay, TargetNullValue=0, FallbackValue=0} | Style={DynamicResource GscRedesignContextPill}
- Border | OverviewStatStrip | Style={DynamicResource GscRedesignSectionCard}
- Border | {Binding Snapshot.MatchedGames, Mode=OneWay} | Style={StaticResource OverviewStatCard}
- Border | 健康 | Style={StaticResource OverviewStatCard}
- Border | OverviewRecentActivityCard | Style={StaticResource OverviewReadingCard}
- Border | {Binding TaskTypeDisplay, Mode=OneWay} | Style={StaticResource OverviewActivityFrame}
- Border | 全局活动 | Style={StaticResource OverviewReadingCard}
- Border | {Binding KindDisplay, Mode=OneWay} | Style={StaticResource OverviewActivityFrame}
- Border | OverviewRiskCard | Style={StaticResource OverviewReadingCard}
- Border | OverviewFindingsCard | Style={StaticResource OverviewReadingCard}
- ItemsControl | OverviewActivityTimelineList
- ItemsControl | !
- ListBox | OverviewActivityList
- ListBox | OverviewProtectionPreviewItems
- ProgressBar | 
- ProgressBar | 
- ScrollViewer | OverviewStackScrollSurface | Style={DynamicResource GscPageScrollViewer}
- ScrollViewer | OverviewRiskViewport | Style={DynamicResource GscPageScrollViewer}
- ScrollViewer | OverviewAttentionScrollViewer | Style={DynamicResource GscPageScrollViewer}
- TextBlock | 
- TextBlock | 
- TextBlock | 
- TextBlock | {Binding Snapshot.WarningGames, Mode=OneWay, TargetNullValue=0, FallbackValue=0}
- TextBlock | 无需处理
- TextBlock | 暂无任务记录
完成备份、同步或维护操作后会显示在这里。


## 存档中心

文件：src\GameSaveCenter.Playnite\Views\SaveCenterView.xaml

### 历史版本

#### 操作与输入

- Button |  | Command={Binding LoadDetailsCommand} | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding DetectPathsCommand} | Style={DynamicResource GscWpfUiCompactButton}
- Button |  | Command={Binding ValidateCommand} | Style={DynamicResource GscWpfUiCompactButton}
- Button |  | Command={Binding LoadDetailsCommand} | Style={DynamicResource GscWpfUiCompactButton}
- Button |  | Style={DynamicResource GscWpfUiCompactButton}
- Button |  | Command={Binding OpenMaintenanceCommand}
- Button | SaveHistoryCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding ValidateRestoreReadinessCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding UpdateBackupMetadataCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding CompareBackupCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding RestoreCommand} | Style={DynamicResource GscWpfUiDangerActionButton}
- Button |  | Command={Binding UndoRestoreCommand} | Style={DynamicResource GscWpfUiContextButton}
- CheckBox |  | Style={DynamicResource GscCheckBox}
- TextBox | {Binding BackupComment, UpdateSourceTrigger=PropertyChanged} | Style={DynamicResource GscWpfUiTextBox}

#### 数据表

- DataGrid | SaveHistoryGrid

#### 滚动容器

- ScrollViewer | SaveHistoryActionsScrollViewer

#### 条件显示 UI

- Border | {Binding SelectedGame.MatchStateDisplay, Mode=OneWay, TargetNullValue=当前规则未匹配} | Style={StaticResource SaveTableFrame}
- Border | SaveHistorySummaryCard | Style={DynamicResource GscReadingCardStyle}
- Border | SaveDetailsStaleBanner
- Border | {Binding LockStateDisplay, Mode=OneWay, Converter={StaticResource GscStatusGlyphConverter}}
- Border | {Binding RestoreAvailabilityHint, Mode=OneWay} | Style={DynamicResource GscActionAvailabilityHintBubble}
- Button |  | Command={Binding OpenMaintenanceCommand}
- DataGrid | SaveHistoryGrid | Style={StaticResource SaveDataGrid}
- ScrollViewer | SaveHistoryActionsScrollViewer
- TabItem | {Binding SelectedGame.MatchStateDisplay, Mode=OneWay, TargetNullValue=当前规则未匹配}
- TextBlock | SaveHistoryEmptyStateText


### 路径与校验

#### 操作与输入

- Button | SaveDetectPathsButton | Command={Binding DetectPathsCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button | SaveValidateButton | Command={Binding ValidateCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button | SaveLoadDetailsButton | Command={Binding LoadDetailsCommand} | Style={DynamicResource GscIconOnlyButtonBase}
- Button | SaveCandidateCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding CopyPathCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding DetectPathsCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding AcceptCandidateCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding RejectCandidateCommand} | Style={DynamicResource GscWpfUiActionButton}
- TextBox | {Binding SelectedCandidate.Path, Mode=OneWay, TargetNullValue=选择候选路径后查看详情} | Style={StaticResource GscWpfUiPathDetailTextBox}

#### 数据表

- DataGrid | SaveCandidateGrid

#### 滚动容器

- ScrollViewer | SaveCandidateInspectorScrollViewer

#### 条件显示 UI

- Border | {Binding Score, Mode=OneWay, StringFormat=P0} | Style={StaticResource SaveTableFrame}
- ScrollViewer | SaveCandidateInspectorScrollViewer
- TabItem | {Binding SelectedGame.MatchStateDisplay, Mode=OneWay, TargetNullValue=未匹配}
- TextBlock | SaveCandidateEmptyStateText


### 备份策略

#### 操作与输入

- Button |  | Command={Binding BackupSelectedCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding SavePolicyCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding PreviewRetentionCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding CreatePolicyTemplateCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding SavePolicyTemplateCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding ApplyPolicyTemplateCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding DeletePolicyTemplateCommand} | Style={DynamicResource GscIconOnlyDangerButton}
- ComboBox | {Binding Display} | Style={DynamicResource GscWpfUiComboBox}
- ComboBox | {Binding Name} | Style={DynamicResource GscWpfUiComboBox}
- TextBox |  | Style={StaticResource GscNumericFieldInput}
- TextBox | {Binding PolicyTemplateNameDraft, UpdateSourceTrigger=PropertyChanged} | Style={DynamicResource GscWpfUiTextBox}
- TextBox |  | Style={StaticResource GscNumericFieldInput}
- TextBox |  | Style={StaticResource GscNumericFieldInput}
- TextBox |  | Style={StaticResource GscNumericFieldInput}
- TextBox |  | Style={StaticResource GscNumericFieldInput}

#### 滚动容器

- ScrollViewer | V=Auto, H=Disabled

#### 折叠区域

- Expander | 模板参数

#### 条件显示 UI

- Border | SavePolicyMediaCard | Style={DynamicResource GscReadingCardStyle}
- Border |  | Style={DynamicResource GscRedesignSubCard}
- ScrollViewer | 备份自动化 | Style={DynamicResource GscPageScrollViewer}
- TabItem | 备份自动化
- TextBlock | 


### 比较与保留

#### 操作与输入

- Button |  | Command={Binding CompareBackupCommand} | Style={DynamicResource GscWpfUiCompactButton}
- Button |  | Command={Binding PreviewRetentionCommand} | Style={DynamicResource GscIconOnlyButtonBase}

#### 滚动容器

- ScrollViewer | SaveComparePageScrollViewer
- ScrollViewer | SaveCompareMainScrollViewer
- ScrollViewer | SaveCompareRetentionScrollViewer


## 任务中心

文件：src\GameSaveCenter.Playnite\Views\TaskCenterView.xaml

### 页面

#### 操作与输入

- Button | 
- Button | TaskRefreshButton | Command={Binding RefreshCommand} | Style={DynamicResource GscIconOnlyToolbarButton}
- Button | TaskClearFiltersButton | Command={Binding ClearTaskFiltersCommand}
- Button |  | Command={Binding RetryAllTasksCommand} | Style={DynamicResource GscWpfUiContextButton}
- Button |  | Command={Binding LoadMoreTasksCommand}
- Button |  | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding RefreshCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button | TaskCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding CopyTaskErrorCommand} | Style={DynamicResource GscIconOnlyButtonBase}
- Button |  | Command={Binding RetryTaskCommand} | Style={DynamicResource GscIconOnlyButtonBase}
- Button |  | Command={Binding CancelTaskCommand} | Style={DynamicResource GscIconOnlyDangerButton}
- Button | TaskCompactCloseDetailsButton | Style={DynamicResource GscIconOnlyButtonBase}
- ComboBox | TaskStatusFilterComboBox | Style={DynamicResource GscWpfUiFilterComboBox}
- ComboBox | TaskTypeFilterComboBox | Style={DynamicResource GscWpfUiFilterComboBox}
- ComboBox | TaskHistoryScopeComboBox | Style={DynamicResource GscWpfUiFilterComboBox}
- ComboBox | TaskHistoryRangeComboBox | Style={DynamicResource GscWpfUiFilterComboBox}
- ComboBox | TaskGameFilterComboBox | Style={DynamicResource GscWpfUiFilterComboBox}
- TextBox | TaskSearchTextBox | Style={DynamicResource GscWpfUiTextBox}

#### 数据表

- DataGrid | TaskGrid

#### 滚动容器

- ScrollViewer | TaskDetailScrollViewer

#### 折叠区域

- Expander | TaskMoreFiltersExpander
- Expander | TaskTechnicalDetailsExpander

#### 条件显示 UI

- Border | TaskFilterBar | Style={DynamicResource GscRedesignSectionCard}
- Border | TaskQueuePanel | Style={DynamicResource GscRedesignSectionCard}
- Border | TaskStaleDataBanner
- Border | {Binding StateDisplay, Mode=OneWay, Converter={StaticResource GscStatusGlyphConverter}} | Style={StaticResource TaskTableFrame}
- Border | TaskDetailCard
- Border | TaskInspectorErrorCard
- Button | 
- Button | TaskClearFiltersButton | Command={Binding ClearTaskFiltersCommand}
- Button |  | Command={Binding LoadMoreTasksCommand}
- DataGrid | TaskGrid | Style={StaticResource TaskDataGrid}
- Expander | TaskMoreFiltersExpander | Style={StaticResource GscDisclosureCard}
- ScrollViewer | TaskDetailScrollViewer
- TextBlock | 搜索任务…
- TextBlock | TaskQueueFilterSummary
- TextBlock | TaskCancellationStatusText


## 修改器中心

文件：src\GameSaveCenter.Playnite\Views\TrainerCenterView.xaml

### 已绑定工具

#### 操作与输入

- Button |  | Command={Binding ImportTrainerCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ImportToolFolderCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ImportCheatTableCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ImportCustomLaunchItemCommand} | Style={DynamicResource GscWpfUiToolbarButton}
- Button |  | Command={Binding ConfirmGameToolImportCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding CancelGameToolImportCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button | TrainerToolsCompactDetailsButton | Style={DynamicResource GscWpfUiSecondaryButton}
- Button |  | Command={Binding CopyPathCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding LaunchGameToolCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding SaveGameToolCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding OpenGameToolDirectoryCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding RelocateGameToolCommand}
- Button |  | Command={Binding DeleteGameToolCommand} | Style={DynamicResource GscWpfUiContextButton}
- ComboBox | {Binding Display} | Style={DynamicResource GscWpfUiComboBox}
- ComboBox | {Binding VersionName} | Style={DynamicResource GscWpfUiComboBox}
- ComboBox | {Binding Display} | Style={DynamicResource GscWpfUiComboBox}
- ComboBox | {Binding Display} | Style={DynamicResource GscWpfUiComboBox}
- TextBox | {Binding SelectedGameTool.DisplayName, UpdateSourceTrigger=LostFocus} | Style={DynamicResource GscWpfUiTextBox}
- TextBox | {Binding SelectedGameToolVersion.EntryPath, Mode=OneWay, TargetNullValue=未选择版本} | Style={StaticResource GscWpfUiPathDetailTextBox}
- TextBox | {Binding SelectedGameToolVersion.WorkingDirectory, UpdateSourceTrigger=LostFocus} | Style={DynamicResource GscWpfUiPathTextBox}
- TextBox | {Binding SelectedGameToolVersion.Arguments, UpdateSourceTrigger=LostFocus} | Style={DynamicResource GscWpfUiTextBox}
- TextBox |  | Style={StaticResource TrainerCompactNumericTextBox}

#### 滚动容器

- ScrollViewer | TrainerToolsSettingsScrollViewer

#### 条件显示 UI

- Border | 选择修改器主程序
- Border | TrainerToolsTable | Style={StaticResource TrainerTableFrame}
- Border | 工具设置 | Style={DynamicResource GscReadingCardStyle}
- Button |  | Command={Binding RelocateGameToolCommand}
- ListBox | TrainerToolsList
- ScrollViewer | TrainerToolsSettingsScrollViewer
- TabItem | {Binding TypeDisplay, Mode=OneWay}
- TextBlock | {Binding SelectedGameTool.ExternalReferenceHint}


### 导入确认

#### 操作与输入

- Button |  | Command={Binding ConfirmGameToolImportCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding CancelGameToolImportCommand} | Style={DynamicResource GscWpfUiActionButton}
- ComboBox | TrainerImportEntryComboBox | Style={DynamicResource GscWpfUiComboBox}

#### 条件显示 UI

- Border | 确认导入 | Style={DynamicResource GscReadingCardStyle}
- Border | 请选择要启动的 EXE，然后确认导入；取消会清理本次临时导入，不影响已有工具。 | Style={DynamicResource GscRedesignInfoBand}
- TabItem | 确认导入
- TextBlock | 


### FLiNG 在线库

#### 操作与输入

- Button | 
- Button |  | Command={Binding SearchTrainerCatalogCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding SyncTrainerCatalogCommand} | Style={DynamicResource GscWpfUiActionButton}
- Button |  | Command={Binding DataContext.LoadTrainerReleasesCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- TextBox | TrainerSearchTextBox | Style={DynamicResource GscWpfUiTextBox}

#### 条件显示 UI

- Border |  | Style={DynamicResource GscReadingCardStyle}
- Border | TrainerCatalogResultsPanel | Style={StaticResource TrainerTableFrame}
- Button | 
- TabItem | {Binding SourceDisplay, Mode=OneWay}
- TextBlock | 没有匹配的 FLiNG 条目
先搜索本地缓存，或点击“刷新目录”同步最新目录。


### 可下载版本

#### 操作与输入

- Button |  | Command={Binding DownloadTrainerCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}
- Button |  | Command={Binding CancelTrainerDownloadCommand}
- Button |  | Command={Binding DownloadTrainerCommand} | Style={DynamicResource GscWpfUiPrimaryActionButton}

#### 滚动容器

- ScrollViewer | TrainerReleaseInfoScrollViewer

#### 条件显示 UI

- Border | TrainerCatalogReleasesPanel | Style={StaticResource TrainerTableFrame}
- Border | TrainerReleaseInfoPanel | Style={DynamicResource GscRedesignSectionCard}
- Border | {Binding TrainerDownloadStatus} | Style={DynamicResource GscReadingCardStyle}
- Button |  | Command={Binding CancelTrainerDownloadCommand}
- ProgressBar | 
- ScrollViewer | TrainerReleaseInfoScrollViewer
- TabItem | {Binding OptionCountDisplay, Mode=OneWay}
- TextBlock | 选择 FLiNG 搜索结果后查看可下载版本。


## 设置

文件：src\GameSaveCenter.Playnite\Settings\GameSaveCenterSettingsView.xaml

### 页面

#### 操作与输入

- Button | SettingsValidationLocateButton | Style={StaticResource GscWpfUiToolbarButton}
- Button |  | Style={StaticResource GscWpfUiSecondaryButton}
- Button |  | Style={StaticResource GscWpfUiPrimaryButton}
- CheckBox |  | Style={StaticResource GscCheckBox}
- ComboBox |  | Style={StaticResource GscWpfUiComboBox}
- ComboBox |  | Style={StaticResource GscWpfUiComboBox}
- ComboBox | ThemeModeSelector | Style={StaticResource GscWpfUiComboBox}
- ComboBox |  | Style={StaticResource GscWpfUiComboBox}
- ComboBox | RecentProtectionWindowComboBox | Style={StaticResource GscWpfUiComboBox}
- Slider | GlassStrengthSlider | Style={StaticResource GscSlider}
- TextBox | WorkerExecutableTextBox | Style={StaticResource GscWpfUiPathTextBox}
- TextBox | LudusaviExecutableTextBox | Style={StaticResource GscWpfUiPathTextBox}
- TextBox | LudusaviBackupDirectoryTextBox | Style={StaticResource GscWpfUiPathTextBox}
- TextBox | RcloneExecutableTextBox | Style={StaticResource GscWpfUiPathTextBox}
- TextBox | {Binding RcloneDestination, UpdateSourceTrigger=PropertyChanged} | Style={StaticResource GscWpfUiPathTextBox}
- TextBox | MediaArchiveDirectoryTextBox | Style={StaticResource GscWpfUiPathTextBox}
- TextBox | LocalMirrorPathTextBox | Style={StaticResource GscWpfUiPathTextBox}
- TextBox | FullBackupLimitTextBox | Style={StaticResource GscNumericTextBox}
- TextBox | DifferentialBackupLimitTextBox | Style={StaticResource GscNumericTextBox}
- TextBox | CompressionLevelTextBox | Style={StaticResource GscNumericTextBox}
- TextBox |  | Style={StaticResource GscNumericTextBox}
- TextBox |  | Style={StaticResource GscNumericTextBox}
- TextBox | DefaultBackupIntervalMinutesTextBox | Style={StaticResource GscNumericTextBox}
- TextBox | ProcessPollingSecondsTextBox | Style={StaticResource GscNumericTextBox}
- TextBox | DashboardRefreshSecondsTextBox | Style={StaticResource GscNumericTextBox}
- TextBox | HealthInspectionIntervalMinutesTextBox | Style={StaticResource GscNumericTextBox}
- TextBox | HealthInspectionStaleAfterDaysTextBox | Style={StaticResource GscNumericTextBox}

#### 滚动容器

- ScrollViewer | SettingsScroller

#### 折叠区域

- Expander | SettingsValidationDetails

#### 条件显示 UI

- Border | 暂停云端自动重试队列 | Style={DynamicResource GscRedesignSubCard}
- Border | 通知级别 | Style={DynamicResource GscRedesignSubCard}


