# Product Hardening Layer C 最终审计

> 审计日期：2026-08-14
> 范围：Layer C 11 项
> 原则：逐项给出生产代码、测试、自动验证与人工验收边界；Layer C 全部有真实实现，不再以文档或按钮占位。

## 汇总

| Task | Status | 主要生产代码 | 主要测试 | AUTO VERIFIED | MANUAL QA REQUIRED | Commit |
|---|---|---|---|---|---|---|
| STORAGE-001 | COMPLETED | `StorageAnalysisService.cs`、`StorageAnalysisDtos.cs`、维护中心保留策略页 | `StorageAnalysisServiceTests.cs` | 是 | 真实大仓库下容量趋势与 Top 排行 | `2ef8f13` |
| RETENTION-SIM-001 | COMPLETED | `RetentionSimulationService.cs`、`RetentionSimulationDtos.cs`、维护中心保留策略页 | `RetentionSimulationServiceTests.cs` | 是 | 真实策略组合下候选明细与二次确认 | `a27f430` |
| LOCAL-MIRROR-001 | COMPLETED | `LocalMirrorService.cs`、`LocalMirrorDtos.cs`、`WorkerOptions.cs`、设置/维护 UI | `LocalMirrorServiceTests.cs` | 是 | 真实外置盘拔插与断盘状态 | `387149f` |
| ACTIVITY-001 | COMPLETED | `ActivityTimelineMapper.cs`、`DashboardDtos.cs`、`OverviewView.xaml` | `ActivityTimelineMapperTests.cs` | 是 | 真实业务事件顺序与时间线可读性 | `7f38b15` |
| PLAYNITE-QUICK-001 | COMPLETED | `GameSaveCenterPlugin.cs` `GetGameMenuItems` | `QuickActionSourceTests.cs` | 是 | 真实 Playnite 游戏右键菜单与 GameId 绑定 | `15ab1d7` |
| DRAGDROP-001 | COMPLETED | `DashboardViewModel.cs`、`TrainerCenterView.xaml/.cs` | `DragDropImportSourceTests.cs` | 是 | 真实 EXE/CT/LNK/BAT/CMD/PS1 拖拽 | `5d113c3` |
| UI-STATE-001 | COMPLETED | `GameSaveCenterSettings.cs`、`DashboardViewModel.cs`、`DebouncedRefresh.cs` | `UiStatePersistenceSourceTests.cs`、`PortableSettingsTests.cs` | 是 | 真实重启后筛选/Workspace/媒体状态恢复 | `e4513cb` |
| ACCESSIBILITY-001 | COMPLETED | `DashboardView.xaml/.cs`、Task/Media/Trainer 搜索框 | `AccessibilitySourceTests.cs` | 是 | 真实键盘焦点、高对比度与 200% DPI | `c907983` |
| UI-STATES-001 | COMPLETED | `WorkspaceStatePresenter.cs`、`Redesign.xaml`、Overview/Task 页面 | `WorkspaceStateSourceTests.cs`、`WpfUiResourceDictionaryTests.cs` | 是 | 真实 Loading/Empty/Error/Degraded/Offline/Disabled | `f21cba4` |
| SETTINGS-VALIDATION-001 | COMPLETED | `GameSaveCenterSettingsView.xaml/.cs` | `SettingsValidationSourceTests.cs` | 是 | 真实输入错误即时展示与严重错误禁止保存 | `4614bb8` |
| MAINTENANCE-REPORT-001 | COMPLETED | `MaintenanceReportService.cs`、`MaintenanceReportDtos.cs`、维护中心复制/导出命令 | `MaintenanceReportServiceTests.cs`、`MaintenanceReportSourceTests.cs` | 是 | 真实健康报告复制/导出 TXT/Markdown | `9d465d2` |

## 逐项验收

### STORAGE-001

- 生产证据：维护中心“保留策略”页新增只读备份存储分析卡；显示卷剩余/总容量、目录实测与索引体积、版本数、7/30/90 天增长趋势、Top 5 游戏占用排行与标注“估算”的容量耗尽预测；新增 IPC `storage.analysis`、Worker 服务与取消支持。
- 自动验证：`StorageAnalysisServiceTests` 覆盖卷统计、目录统计、趋势、Top N、预测与取消；`ProtocolCompatibilityTests` 覆盖能力。
- 人工验收：真实大仓库下核对容量数字、趋势和 Top 排行。

### RETENTION-SIM-001

- 生产证据：维护中心“保留策略”页新增全局保留策略模拟器；按每游戏策略复用 `RetentionPlanner` 计算现有/保留/候选清理/预计释放、用户锁定/健康保护/PreRestore 计数与候选明细；`retention.simulation.apply` 要求二次确认，只删除备份根目录下的 ZIP 候选并同步移除 SQLite 索引。
- 自动验证：`RetentionSimulationServiceTests` 覆盖预览、未确认拒绝、确认执行、锁定/PreRestore/健康保护永不进入候选与幂等。
- 人工验收：真实策略组合下核对候选明细与清理结果。

### LOCAL-MIRROR-001

- 生产证据：设置页新增“启用第二本地镜像”与镜像目录；维护中心“保留策略”页新增镜像状态与“同步镜像”入口。Worker `LocalMirrorService` 只复制和按大小校验，绝不删除镜像中多余文件；外置硬盘未连接时状态为 `Unavailable`；同步完成后写入镜像标记文件。
- 自动验证：`LocalMirrorServiceTests` 覆盖复制、校验、Unavailable、不删除多余文件与标记文件。
- 人工验收：真实外置盘拔插、断盘状态和同步结果。

### ACTIVITY-001

- 生产证据：首页新增“全局活动”时间线，由 `ActivityTimelineMapper` 把最近 100 条审计记录映射为备份/恢复/云端/媒体/工具/健康/冲突/完整性/仓库修复等业务事件；UI 最多显示 12 条，保持有限视口与虚拟化，不暴露原始日志。
- 自动验证：`ActivityTimelineMapperTests` 覆盖事件分类、结果、摘要、排序与上限；`UiLayoutRegressionTests` 覆盖首页布局。
- 人工验收：真实备份/恢复/云端事件后的时间线顺序与可读性。

### PLAYNITE-QUICK-001

- 生产证据：`GetGameMenuItems` 为游戏右键菜单提供“立即备份 / 查看备份历史 / 验证最新恢复点 / 游戏工具 / 打开设置”五个快捷操作，全部绑定当前所选游戏 ID，并复用 Worker 生产 IPC 链路。
- 自动验证：`QuickActionSourceTests` 覆盖菜单项数量、命令与 GameId 绑定。
- 人工验收：真实 Playnite 游戏右键点击每项快捷操作。

### DRAGDROP-001

- 生产证据：修改器中心支持单文件/目录拖拽导入；`.ct` 自动按 CheatTable，`.lnk/.bat/.cmd/.ps1` 按自定义启动项，`.exe` 弹出“修改器/普通启动项”二选一，`.zip`/目录进入既有主程序选择流程；未选择游戏时拒绝导入并提示；导入后默认 `AutoStart=false`。
- 自动验证：`DragDropImportSourceTests` 覆盖扩展名路由、未选游戏拒绝与默认 AutoStart。
- 人工验收：真实文件拖拽、EXE 选择框和导入后默认状态。

### UI-STATE-001

- 生产证据：设置持久化上次 Workspace、任务状态/游戏/类型筛选、任务搜索、媒体筛选与媒体搜索；VM 启动时恢复，变更经 500ms 防抖保存；运行中游戏优先与上次选择恢复；不保存 Loading/Busy/Error 等瞬态。
- 自动验证：`UiStatePersistenceSourceTests` 与 `PortableSettingsTests` 覆盖字段持久化、恢复与防抖。
- 人工验收：真实重启 Playnite 后核对筛选与 Workspace 恢复。

### ACCESSIBILITY-001

- 生产证据：`Ctrl+F` 按当前 Workspace 聚焦游戏/任务/媒体/FLiNG/进程映射搜索框并全选；任务、媒体、FLiNG 与游戏搜索框补充 `AutomationProperties.Name`；共享 `GscSharedFocusVisual` 与高对比度降级继续生效。
- 自动验证：`AccessibilitySourceTests` 覆盖快捷键、AutomationProperties 与焦点。
- 人工验收：真实键盘焦点、高对比度主题与 100%–200% DPI。

### UI-STATES-001

- 生产证据：新增共享 `WorkspaceStatePresenter`，统一 Loading/Empty/Error/Degraded/Offline/Disabled 六种状态；Overview 全局活动与 Task 空状态已接入共享控件，其余页面继续复用 `GscEmptyStateText`。
- 自动验证：`WorkspaceStateSourceTests` 与 `WpfUiResourceDictionaryTests` 覆盖共享控件与页面接入。
- 人工验收：真实状态切换与重试按钮复核。

### SETTINGS-VALIDATION-001

- 生产证据：设置页标题区新增 `SettingsValidationSummary`，文本框、下拉框与复选框变化时复用 `VerifySettings` 校验并内联展示最多 4 条错误；验证错误不再只等 Playnite 保存时出现。
- 自动验证：`SettingsValidationSourceTests` 覆盖即时校验与内联摘要。
- 人工验收：真实修改路径/数值后立即查看内联错误、修正后消失，以及保存时仍能阻止严重错误。

### MAINTENANCE-REPORT-001

- 生产证据：新增 IPC `maintenance.report.get` 与 Worker `MaintenanceReportService`，从 SQLite 计数、完整性自检、存储分析与本地镜像状态聚合用户可读健康文本；维护中心“诊断”操作带新增“复制健康报告/导出健康报告”，导出支持 TXT/Markdown；报告与开发者诊断 ZIP 明确区分，不包含日志、原始数据库或凭据。
- 自动验证：`MaintenanceReportServiceTests` 覆盖报告章节与真实状态；`MaintenanceReportSourceTests` 覆盖 IPC/命令/复制/导出接线；`ProtocolCompatibilityTests` 覆盖能力。
- 人工验收：真实维护中心点击复制/导出健康报告、长文本显示与内容完整性。

## Layer C 最终门禁

- Release 解决方案构建：0 warnings / 0 errors。
- Core：59/59。
- Worker：183/183。
- Playnite：231/231。
- `scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：13 个 XAML 通过。
- `.codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：0 errors。
- `scripts/render-qa.ps1`：五种窗口全部通过。
- 每个 C 阶段提交后均执行 `scripts/dev-install-run.ps1 -Configuration Release` 真实开发安装；Layer C 收尾后最新一次 `playnite.log` 04:02:36 记录插件加载，`extensions.log` 04:02:37 记录 `GameSaveCenter 0.6.70.0 loaded`，`worker-launch.log` 04:02:42 记录 `Application started`。

## 结论

Layer C 11/11 已完成并有真实代码、测试、UI/IPC 接入、文档与提交证据。真实人工场景（大仓库、外置盘拔插、真实右键/拖拽、主题/DPI 等）仍为 `MANUAL QA REQUIRED`；整体 Epic 最终结论见 `PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md`。
