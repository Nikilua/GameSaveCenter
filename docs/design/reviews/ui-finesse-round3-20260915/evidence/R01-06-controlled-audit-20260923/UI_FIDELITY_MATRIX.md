# UI Fidelity Matrix

| Route | Section | Element | Type | Command | Conditional | Screenshot visible |
|---|---|---|---|---|---|---|
| AcrylicProductionShellView | 页面 | SidebarProductionVersionText | TextBox |  | No | Yes |
| AcrylicProductionShellView | 页面 | SidebarCollapseButton | Button |  | No | Yes |
| AcrylicProductionShellView | 页面 | HeaderBackButton | Button | {Binding ReturnToNavigationSourceCommand} | No | No |
| AcrylicProductionShellView | 页面 | GameContextButton | Button |  | No | Yes |
| AcrylicProductionShellView | 页面 | HeaderKeyboardHelpButton | Button |  | No | Yes |
| AcrylicProductionShellView | 页面 | HeaderRefreshButton | Button | {Binding RefreshCommand} | No | Yes |
| AcrylicProductionShellView | 页面 | HeaderMediaButton | Button | {Binding SyncMediaCommand} | No | Yes |
| AcrylicProductionShellView | 页面 | HeaderBackupSelectedButton | Button | {Binding BackupSelectedCommand} | No | Yes |
| AcrylicProductionShellView | 页面 | HeaderBackupButton | Button | {Binding BackupAllCommand} | No | Yes |
| AcrylicProductionShellView | 页面 | GameSearchTextBox | TextBox |  | No | No |
| AcrylicProductionShellView | 页面 | GameSearchClearButton | Button |  | Yes | No |
| AcrylicProductionShellView | 页面 | GamePickerStatusComboBox | ComboBox |  | No | No |
| AcrylicProductionShellView | 页面 | GamePickerPlatformComboBox | ComboBox |  | No | No |
| AcrylicProductionShellView | 页面 | GamePickerSortComboBox | ComboBox |  | No | No |
| Development 探针 | 页面 |  | TextBox |  | No | Yes |
| Development 探针 | 页面 |  | ComboBox |  | No | Yes |
| Development 探针 | 页面 |  | Button |  | No | Yes |
| Development 探针 | 页面 |  | Button |  | No | Yes |
| Development 探针 | 页面 |  | Button |  | No | Yes |
| Development 探针 | 页面 |  | Button |  | No | Yes |
| Development 探针 | 页面 |  | CheckBox |  | No | Yes |
| 维护中心 | 诊断 | MaintenanceDiagnosticsCompactDetailsButton | Button |  | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding RefreshDiagnosticsCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding DataContext.CopyDiagnosticsCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding DataContext.OpenSelectedFindingNavigationCommand} | Yes | Yes |
| 维护中心 | 诊断 |  | Button | {Binding RefreshDiagnosticsCommand} | No | Yes |
| 维护中心 | 诊断 |  | Expander |  | Yes | Yes |
| 维护中心 | 诊断 |  | Button | {Binding DataContext.LoadMoreRetentionQuarantineCommand} | Yes | Yes |
| 维护中心 | 诊断 |  | Button | {Binding RunEnvironmentCheckCommand} | No | Yes |
| 维护中心 | 诊断 | EnvironmentCheckDisclosure | Expander |  | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding OnboardingTestBackupCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding CompleteOnboardingCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding SkipOnboardingCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding RefreshDiagnosticsCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding CopyDiagnosticsCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding CreateDiagnosticsPackageCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding CopyMaintenanceReportCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding ExportMaintenanceReportCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding RunIntegrityCheckCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding RunHealthInspectionCommand} | No | Yes |
| 维护中心 | 诊断 | MaintenanceActionsDisclosure | Expander |  | Yes | Yes |
| 维护中心 | 诊断 |  | Expander |  | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding OpenDataDirectoryCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding OpenBackupDirectoryCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding OpenMediaDirectoryCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding OpenWorkerLogCommand} | No | Yes |
| 维护中心 | 诊断 |  | Expander |  | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding RebuildRepositoryCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding ReconcileTasksCommand} | No | Yes |
| 维护中心 | 诊断 |  | Expander |  | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding CreateMetadataBackupCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding RestoreMetadataBackupCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding ExitSafeModeCommand} | No | Yes |
| 维护中心 | 诊断 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding RunPathRemapCommand} | No | Yes |
| 维护中心 | 诊断 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding DiagnoseGameCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding ClearGamePickerFiltersCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding SyncGameDescriptorCommand} | No | Yes |
| 维护中心 | 诊断 |  | Button | {Binding RetryGameMatchCommand} | No | Yes |
| 维护中心 | 诊断 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断 |  | TextBox |  | No | Yes |
| 维护中心 | 云端队列 |  | ComboBox |  | No | Yes |
| 维护中心 | 云端队列 |  | ComboBox |  | No | Yes |
| 维护中心 | 云端队列 |  | TextBox |  | No | Yes |
| 维护中心 | 云端队列 |  | TextBox |  | No | Yes |
| 维护中心 | 云端队列 |  | ComboBox |  | No | Yes |
| 维护中心 | 云端队列 |  | Button | {Binding RefreshCloudTransfersCommand} | No | Yes |
| 维护中心 | 云端队列 |  | Button | {Binding ClearCloudTransferFiltersCommand} | Yes | Yes |
| 维护中心 | 云端队列 | CloudTransferCompactDetailsButton | Button |  | No | Yes |
| 维护中心 | 云端队列 |  | Button | {Binding RefreshCloudTransfersCommand} | No | Yes |
| 维护中心 | 云端队列 |  | Expander |  | No | Yes |
| 维护中心 | 云端队列 |  | Button | {Binding OpenMaintenanceCommand} | Yes | Yes |
| 维护中心 | 云端队列 |  | Button | {Binding VerifyCloudTransferCommand} | No | Yes |
| 维护中心 | 云端队列 |  | Button | {Binding RetryCloudUploadCommand} | No | Yes |
| 维护中心 | 云端队列 |  | Button | {Binding LoadMoreCloudTransfersCommand} | No | Yes |
| 维护中心 | 设备状态 |  | Button | {Binding SyncDeviceStatesCommand} | No | Yes |
| 维护中心 | 设备状态 | MaintenanceDeviceCompactDetailsButton | Button |  | No | Yes |
| 维护中心 | 设备状态 |  | ComboBox |  | No | Yes |
| 维护中心 | 设备状态 |  | TextBox |  | No | Yes |
| 维护中心 | 设备状态 |  | Button | {Binding SaveDeviceDecisionCommand} | No | Yes |
| 维护中心 | 设备状态 |  | Button | {Binding StageRemoteBackupCommand} | No | Yes |
| 维护中心 | 设备状态 |  | Button | {Binding RestoreStagedRemoteBackupCommand} | No | Yes |
| 维护中心 | 设备状态 |  | Button | {Binding CancelRemoteBackupStageCommand} | Yes | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding PreviewRetentionCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding RefreshStorageAnalysisCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding DataContext.OpenStorageGameCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding DataContext.OpenStorageBackupCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding RefreshRetentionSimulationCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding ApplyRetentionSimulationCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding RefreshLocalMirrorStatusCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding SyncLocalMirrorCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding PreviewRetentionCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding RefreshStorageAnalysisCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding RefreshRetentionSimulationCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding ApplyRetentionSimulationCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding RefreshLocalMirrorStatusCommand} | No | Yes |
| 维护中心 | 保留策略 |  | Button | {Binding SyncLocalMirrorCommand} | No | Yes |
| 维护中心 | 异常与审计 |  | Button | {Binding RefreshDiagnosticsCommand} | No | Yes |
| 维护中心 | 异常与审计 |  | Button | {Binding DataContext.CopyDiagnosticsCommand} | No | Yes |
| 维护中心 | 进程映射 | ProcessMappingExecutableTextBox | TextBox |  | No | Yes |
| 维护中心 | 进程映射 | ProcessMappingTargetGameComboBox | ComboBox |  | No | Yes |
| 维护中心 | 进程映射 | ProcessMappingSaveButton | Button | {Binding SaveProcessMappingCommand} | No | Yes |
| 维护中心 | 进程映射 | MaintenanceProcessCompactDetailsButton | Button |  | No | Yes |
| 维护中心 | 进程映射 |  | Button | {Binding DataContext.DeleteProcessMappingCommand} | No | Yes |
| 维护中心 | 进程映射 |  | Button | {Binding DeleteProcessMappingCommand} | No | Yes |
| 维护中心 | 问题列表 | MaintenanceDiagnosticsCompactDetailsButton | Button |  | No | Yes |
| 维护中心 | 问题列表 |  | Button | {Binding RefreshDiagnosticsCommand} | No | Yes |
| 维护中心 | 问题列表 |  | Button | {Binding DataContext.CopyDiagnosticsCommand} | No | Yes |
| 维护中心 | 问题列表 |  | Button | {Binding DataContext.OpenSelectedFindingNavigationCommand} | Yes | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding RefreshDiagnosticsCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Expander |  | Yes | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding DataContext.LoadMoreRetentionQuarantineCommand} | Yes | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding RunEnvironmentCheckCommand} | No | Yes |
| 维护中心 | 诊断概览 | EnvironmentCheckDisclosure | Expander |  | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding OnboardingTestBackupCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding CompleteOnboardingCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding SkipOnboardingCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding RefreshDiagnosticsCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding CopyDiagnosticsCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding CreateDiagnosticsPackageCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding CopyMaintenanceReportCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding ExportMaintenanceReportCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding RunIntegrityCheckCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding RunHealthInspectionCommand} | No | Yes |
| 维护中心 | 诊断概览 | MaintenanceActionsDisclosure | Expander |  | Yes | Yes |
| 维护中心 | 诊断概览 |  | Expander |  | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding OpenDataDirectoryCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding OpenBackupDirectoryCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding OpenMediaDirectoryCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding OpenWorkerLogCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Expander |  | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding RebuildRepositoryCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding ReconcileTasksCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Expander |  | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding CreateMetadataBackupCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding RestoreMetadataBackupCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding ExitSafeModeCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断概览 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding RunPathRemapCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断概览 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断概览 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding DiagnoseGameCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding ClearGamePickerFiltersCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding SyncGameDescriptorCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | Button | {Binding RetryGameMatchCommand} | No | Yes |
| 维护中心 | 诊断概览 |  | TextBox |  | No | Yes |
| 维护中心 | 诊断概览 |  | TextBox |  | No | Yes |
| 维护中心 | 发现的问题 |  | Button | {Binding RefreshDiagnosticsCommand} | No | Yes |
| 维护中心 | 发现的问题 |  | Button | {Binding DataContext.CopyDiagnosticsCommand} | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding ReloadMediaInboxCommand} | No | Yes |
| 媒体中心 | 待归类 | MediaInboxClearSelectionButton | Button |  | No | Yes |
| 媒体中心 | 待归类 |  | Button |  | No | Yes |
| 媒体中心 | 待归类 | MediaInboxModeCombo | ComboBox |  | No | Yes |
| 媒体中心 | 待归类 |  | ComboBox |  | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding AssignInboxMediaBatchCommand} | No | Yes |
| 媒体中心 | 待归类 | MediaInboxCompactDetailsButton | Button |  | No | Yes |
| 媒体中心 | 待归类 | MediaFilterPresetComboBox | ComboBox |  | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding ApplyMediaFilterPresetCommand} | No | Yes |
| 媒体中心 | 待归类 | MediaFilterPresetNameBox | TextBox |  | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding SaveMediaFilterPresetCommand} | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding RenameMediaFilterPresetCommand} | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding DeleteMediaFilterPresetCommand} | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding OpenMaintenanceCommand} | Yes | Yes |
| 媒体中心 | 待归类 | ReloadMediaInboxButton | Button | {Binding ReloadMediaInboxCommand} | No | No |
| 媒体中心 | 待归类 | MediaInboxHistoryButton | Button |  | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding LoadMoreMediaInboxCommand} | Yes | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding IgnoreInboxMediaBatchCommand} | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding PreviewMediaClassificationCommand} | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding ApplyMediaClassificationCommand} | Yes | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding RestoreIgnoredMediaBatchCommand} | Yes | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding UndoMediaClassificationCommand} | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding RetryFailedMediaInboxBatchCommand} | No | Yes |
| 媒体中心 | 待归类 |  | TextBox |  | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding CopyPathCommand} | No | Yes |
| 媒体中心 | 待归类 |  | CheckBox |  | No | Yes |
| 媒体中心 | 待归类 |  | ComboBox |  | Yes | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding RefreshMediaClassificationHistoryCommand} | No | Yes |
| 媒体中心 | 待归类 | MediaClassificationHistoryStateCombo | ComboBox |  | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding LoadMoreMediaClassificationHistoryCommand} | Yes | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding UndoMediaClassificationCommand} | No | Yes |
| 媒体中心 | 待归类 |  | ComboBox |  | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding AssignInboxMediaCommand} | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding IgnoreInboxMediaCommand} | No | Yes |
| 媒体中心 | 待归类 |  | Button | {Binding RestoreIgnoredMediaBatchCommand} | Yes | Yes |
| 媒体中心 | 当前游戏媒体 | MediaSearchTextBox | TextBox |  | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button |  | Yes | Yes |
| 媒体中心 | 当前游戏媒体 |  | ComboBox |  | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding ClearMediaFiltersCommand} | Yes | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding ReloadMediaWindowCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 | ReloadMediaWindowButton | Button | {Binding ReloadMediaWindowCommand} | No | No |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding LoadMoreMediaCommand} | Yes | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding FavoriteSelectedMediaCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding UnfavoriteSelectedMediaCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding CommentSelectedMediaCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 | MediaCompactDetailsButton | Button |  | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | TextBox |  | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding CopyPathCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding PreviousMediaCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding NextMediaCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | TextBox |  | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding UpdateMediaMetadataCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding OpenSelectedMediaCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding RevealSelectedMediaCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | ComboBox |  | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding ReassignMediaCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding FavoriteSelectedMediaCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding UnfavoriteSelectedMediaCommand} | No | Yes |
| 媒体中心 | 当前游戏媒体 |  | Button | {Binding CommentSelectedMediaCommand} | No | Yes |
| 媒体中心 | 重复识别 |  | Button | {Binding ReloadMediaDuplicateGroupsCommand} | No | Yes |
| 媒体中心 | 来源规则 |  | TextBox |  | No | Yes |
| 媒体中心 | 来源规则 |  | TextBox |  | No | Yes |
| 媒体中心 | 来源规则 |  | Button | {Binding PreviewMediaSourceCommand} | No | Yes |
| 媒体中心 | 来源规则 |  | Button | {Binding AddMediaSourceCommand} | No | Yes |
| 媒体中心 | 来源规则 |  | Button | {Binding DataContext.DeleteMediaSourceCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding RefreshCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding BackupAllCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding SyncMediaCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding OverviewPriorityActionCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding BackupSelectedCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding LoadDetailsCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding OpenCloudQueueCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding DataContext.OpenRecentAccessCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding RefreshCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding DataContext.OpenActivityCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding OpenProtectionGamesCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding ApplyRecommendedProtectionCommand} | No | Yes |
| 首页 | 页面 |  | Button | {Binding OpenAttentionCenterCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding LoadDetailsCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding RetrySelectedGameCloudUploadCommand} | Yes | Yes |
| 存档中心 | 历史版本 |  | ComboBox |  | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding ClearBackupHistoryRangeCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding JumpToRecentBackupCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding JumpToEarlierBackupCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding PreviewBackupCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding DetectPathsCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding ValidateCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding LoadDetailsCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button |  | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding OpenMaintenanceCommand} | Yes | Yes |
| 存档中心 | 历史版本 | SaveHistoryCompactDetailsButton | Button |  | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding ValidateRestoreReadinessCommand} | No | Yes |
| 存档中心 | 历史版本 |  | TextBox |  | No | Yes |
| 存档中心 | 历史版本 |  | CheckBox |  | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding UpdateBackupMetadataCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding CancelBackupMetadataCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding CompareBackupCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding RestoreCommand} | No | Yes |
| 存档中心 | 历史版本 |  | Button | {Binding UndoRestoreCommand} | No | Yes |
| 存档中心 | 路径与校验 | SaveDetectPathsButton | Button | {Binding DetectPathsCommand} | No | Yes |
| 存档中心 | 路径与校验 | SaveValidateButton | Button | {Binding ValidateCommand} | No | Yes |
| 存档中心 | 路径与校验 | SaveLoadDetailsButton | Button | {Binding LoadDetailsCommand} | No | Yes |
| 存档中心 | 路径与校验 | SaveCandidateCompactDetailsButton | Button |  | No | Yes |
| 存档中心 | 路径与校验 |  | TextBox |  | No | Yes |
| 存档中心 | 路径与校验 |  | Button | {Binding CopyPathCommand} | No | Yes |
| 存档中心 | 路径与校验 |  | Button | {Binding DetectPathsCommand} | No | Yes |
| 存档中心 | 路径与校验 |  | Button | {Binding AcceptCandidateCommand} | No | Yes |
| 存档中心 | 路径与校验 |  | Button | {Binding RejectCandidateCommand} | No | Yes |
| 存档中心 | 备份策略 |  | Button | {Binding BackupSelectedCommand} | No | Yes |
| 存档中心 | 备份策略 |  | TextBox |  | No | Yes |
| 存档中心 | 备份策略 |  | ComboBox |  | No | Yes |
| 存档中心 | 备份策略 |  | Button | {Binding CancelPolicyDraftCommand} | No | Yes |
| 存档中心 | 备份策略 |  | Button | {Binding SavePolicyCommand} | No | Yes |
| 存档中心 | 备份策略 |  | Button | {Binding PreviewRetentionCommand} | No | Yes |
| 存档中心 | 备份策略 |  | ComboBox |  | No | Yes |
| 存档中心 | 备份策略 |  | Expander |  | No | Yes |
| 存档中心 | 备份策略 |  | TextBox |  | No | Yes |
| 存档中心 | 备份策略 |  | TextBox |  | No | Yes |
| 存档中心 | 备份策略 |  | TextBox |  | No | Yes |
| 存档中心 | 备份策略 |  | TextBox |  | No | Yes |
| 存档中心 | 备份策略 |  | TextBox |  | No | Yes |
| 存档中心 | 备份策略 |  | Expander |  | Yes | Yes |
| 存档中心 | 备份策略 |  | TextBox |  | No | Yes |
| 存档中心 | 备份策略 |  | CheckBox |  | No | Yes |
| 存档中心 | 备份策略 |  | Button | {Binding ApplyPolicyTemplateBatchCommand} | No | Yes |
| 存档中心 | 备份策略 |  | Button | {Binding DataContext.RetryPolicyTemplateBatchItemCommand} | Yes | Yes |
| 存档中心 | 备份策略 |  | Button | {Binding CreatePolicyTemplateCommand} | No | Yes |
| 存档中心 | 备份策略 |  | Button | {Binding SavePolicyTemplateCommand} | No | Yes |
| 存档中心 | 备份策略 |  | Button | {Binding ApplyPolicyTemplateCommand} | No | Yes |
| 存档中心 | 备份策略 |  | Button | {Binding DeletePolicyTemplateCommand} | No | Yes |
| 存档中心 | 比较与保留 |  | Button | {Binding CompareBackupCommand} | No | Yes |
| 存档中心 | 比较与保留 |  | ComboBox |  | No | Yes |
| 存档中心 | 比较与保留 |  | ComboBox |  | No | Yes |
| 存档中心 | 比较与保留 |  | Button | {Binding SwapCompareBackupCommand} | No | Yes |
| 存档中心 | 比较与保留 |  | TextBox |  | No | Yes |
| 存档中心 | 比较与保留 |  | ComboBox |  | No | Yes |
| 存档中心 | 比较与保留 |  | Button | {Binding ClearDiffPathFiltersCommand} | No | Yes |
| 存档中心 | 比较与保留 |  | TextBox |  | No | Yes |
| 存档中心 | 比较与保留 |  | Button | {Binding DataContext.CopyPathCommand} | No | Yes |
| 存档中心 | 比较与保留 |  | TextBox |  | No | Yes |
| 存档中心 | 比较与保留 |  | Button | {Binding DataContext.CopyPathCommand} | No | Yes |
| 存档中心 | 比较与保留 |  | TextBox |  | No | Yes |
| 存档中心 | 比较与保留 |  | Button | {Binding DataContext.CopyPathCommand} | No | Yes |
| 存档中心 | 比较与保留 |  | Button | {Binding LoadMoreDiffPathsCommand} | No | Yes |
| 存档中心 | 比较与保留 |  | Button | {Binding PreviewRetentionCommand} | No | Yes |
| 任务中心 | 页面 | TaskSearchTextBox | TextBox |  | No | Yes |
| 任务中心 | 页面 |  | Button |  | Yes | Yes |
| 任务中心 | 页面 | TaskStatusFilterComboBox | ComboBox |  | No | Yes |
| 任务中心 | 页面 | TaskTypeFilterComboBox | ComboBox |  | No | Yes |
| 任务中心 | 页面 | TaskHistoryScopeComboBox | ComboBox |  | No | Yes |
| 任务中心 | 页面 | TaskHistoryRangeComboBox | ComboBox |  | No | Yes |
| 任务中心 | 页面 | TaskRefreshButton | Button | {Binding RefreshCommand} | No | Yes |
| 任务中心 | 页面 | TaskFilterPresetComboBox | ComboBox |  | No | Yes |
| 任务中心 | 页面 | TaskFilterPresetApplyButton | Button | {Binding ApplyTaskFilterPresetCommand} | No | Yes |
| 任务中心 | 页面 | TaskFilterPresetNameBox | TextBox |  | No | Yes |
| 任务中心 | 页面 |  | Button | {Binding SaveTaskFilterPresetCommand} | No | Yes |
| 任务中心 | 页面 |  | Button | {Binding RenameTaskFilterPresetCommand} | No | Yes |
| 任务中心 | 页面 |  | Button | {Binding DeleteTaskFilterPresetCommand} | No | Yes |
| 任务中心 | 页面 |  | Button | {Binding ClearTaskNavigationContextCommand} | No | Yes |
| 任务中心 | 页面 | TaskMoreFiltersExpander | Expander |  | Yes | Yes |
| 任务中心 | 页面 | TaskClearFiltersButton | Button | {Binding ClearTaskFiltersCommand} | Yes | No |
| 任务中心 | 页面 | TaskGameFilterComboBox | ComboBox |  | No | No |
| 任务中心 | 页面 |  | Button | {Binding RetryAllTasksCommand} | No | Yes |
| 任务中心 | 页面 |  | Button | {Binding LoadMoreTasksCommand} | Yes | Yes |
| 任务中心 | 页面 |  | Button |  | No | Yes |
| 任务中心 | 页面 |  | Button | {Binding RefreshCommand} | No | Yes |
| 任务中心 | 页面 | TaskCompactDetailsButton | Button |  | No | Yes |
| 任务中心 | 页面 | TaskTechnicalDetailsExpander | Expander |  | No | Yes |
| 任务中心 | 页面 |  | TextBox |  | No | Yes |
| 任务中心 | 页面 |  | Button | {Binding OpenSelectedTaskGameCommand} | No | Yes |
| 任务中心 | 页面 |  | Button | {Binding CopyTaskErrorCommand} | No | Yes |
| 任务中心 | 页面 |  | Button | {Binding RetryTaskCommand} | No | Yes |
| 任务中心 | 页面 |  | Button | {Binding CancelTaskCommand} | No | Yes |
| 任务中心 | 页面 | TaskCompactCloseDetailsButton | Button |  | No | No |
| 任务中心 | 页面 |  | Button | {Binding DataContext.OpenSelectedTaskSourceCommand} | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding ImportTrainerCommand} | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding ImportToolFolderCommand} | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding ImportCheatTableCommand} | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding ImportCustomLaunchItemCommand} | No | Yes |
| 修改器中心 | 已绑定工具 |  | ComboBox |  | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding ConfirmGameToolImportCommand} | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding CancelGameToolImportCommand} | No | Yes |
| 修改器中心 | 已绑定工具 | TrainerToolsCompactDetailsButton | Button |  | No | Yes |
| 修改器中心 | 已绑定工具 |  | TextBox |  | No | Yes |
| 修改器中心 | 已绑定工具 |  | ComboBox |  | No | Yes |
| 修改器中心 | 已绑定工具 |  | TextBox |  | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding CopyPathCommand} | No | Yes |
| 修改器中心 | 已绑定工具 |  | TextBox |  | No | Yes |
| 修改器中心 | 已绑定工具 |  | TextBox |  | No | Yes |
| 修改器中心 | 已绑定工具 |  | ComboBox |  | No | Yes |
| 修改器中心 | 已绑定工具 |  | ComboBox |  | No | Yes |
| 修改器中心 | 已绑定工具 |  | TextBox |  | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding LaunchGameToolCommand} | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding SaveGameToolCommand} | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding OpenGameToolDirectoryCommand} | No | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding RelocateGameToolCommand} | Yes | Yes |
| 修改器中心 | 已绑定工具 |  | Button | {Binding DeleteGameToolCommand} | No | Yes |
| 修改器中心 | 导入确认 | TrainerImportEntryComboBox | ComboBox |  | No | Yes |
| 修改器中心 | 导入确认 |  | Button | {Binding ConfirmGameToolImportCommand} | No | Yes |
| 修改器中心 | 导入确认 |  | Button | {Binding CancelGameToolImportCommand} | No | Yes |
| 修改器中心 | FLiNG 在线库 | TrainerSearchTextBox | TextBox |  | No | Yes |
| 修改器中心 | FLiNG 在线库 |  | Button |  | Yes | Yes |
| 修改器中心 | FLiNG 在线库 |  | Button | {Binding SearchTrainerCatalogCommand} | No | Yes |
| 修改器中心 | FLiNG 在线库 |  | Button | {Binding SyncTrainerCatalogCommand} | No | Yes |
| 修改器中心 | FLiNG 在线库 |  | Button | {Binding DataContext.LoadTrainerReleasesCommand} | No | Yes |
| 修改器中心 | 可下载版本 |  | Button | {Binding DownloadTrainerCommand} | No | Yes |
| 修改器中心 | 可下载版本 |  | Button | {Binding CancelTrainerDownloadCommand} | Yes | Yes |
| 修改器中心 | 可下载版本 |  | Button | {Binding DownloadTrainerCommand} | No | Yes |
| 设置 | 页面 | SettingsSearchTextBox | TextBox |  | No | Yes |
| 设置 | 页面 | SettingsValidationLocateButton | Button |  | No | Yes |
| 设置 | 页面 | SettingsValidationDetails | Expander |  | No | Yes |
| 设置 | 页面 | SettingsResetFieldComboBox | ComboBox |  | No | Yes |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 | SettingsPathEditorComboBox | ComboBox |  | No | Yes |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 | WorkerExecutableTextBox | TextBox |  | No | Yes |
| 设置 | 页面 | LudusaviExecutableTextBox | TextBox |  | No | Yes |
| 设置 | 页面 | LudusaviBackupDirectoryTextBox | TextBox |  | No | Yes |
| 设置 | 页面 | RcloneExecutableTextBox | TextBox |  | No | Yes |
| 设置 | 页面 |  | TextBox |  | No | Yes |
| 设置 | 页面 | MediaArchiveDirectoryTextBox | TextBox |  | No | Yes |
| 设置 | 页面 |  | CheckBox |  | No | Yes |
| 设置 | 页面 | LocalMirrorPathTextBox | TextBox |  | No | Yes |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 |  | ComboBox |  | No | Yes |
| 设置 | 页面 |  | ComboBox |  | No | Yes |
| 设置 | 页面 | FullBackupLimitTextBox | TextBox |  | No | No |
| 设置 | 页面 | DifferentialBackupLimitTextBox | TextBox |  | No | No |
| 设置 | 页面 | CompressionLevelTextBox | TextBox |  | No | No |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 | ThemeModeSelector | ComboBox |  | No | No |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 |  | TextBox |  | No | Yes |
| 设置 | 页面 |  | TextBox |  | No | Yes |
| 设置 | 页面 |  | ComboBox |  | No | Yes |
| 设置 | 页面 | DefaultBackupIntervalMinutesTextBox | TextBox |  | No | No |
| 设置 | 页面 | ProcessPollingSecondsTextBox | TextBox |  | No | No |
| 设置 | 页面 | DashboardRefreshSecondsTextBox | TextBox |  | No | No |
| 设置 | 页面 | HealthInspectionIntervalMinutesTextBox | TextBox |  | No | No |
| 设置 | 页面 | HealthInspectionStaleAfterDaysTextBox | TextBox |  | No | No |
| 设置 | 页面 | RecentProtectionWindowComboBox | ComboBox |  | No | No |
| 设置 | 页面 |  | Button |  | No | Yes |
| 设置 | 页面 |  | Button |  | No | Yes |
