# Q13–Q25 交互、页面、宿主与交付证据

采集日期：2026-09-14（Asia/Shanghai）。代码基线：`6450f6e`。证据来自共享 XAML/C# 源码、Playnite 设置迁移测试、RenderHarness Offscreen Regression Audit 和已提交 HEAD 的打包/安装记录；离屏证据不冒充真实 Playnite 像素、IME、物理 DPI、读屏或 ETW 帧时间。

## 本阶段真实修复

- `MediaCenterView` 的“待归类”主表在 compact/narrow 下曾实际只有 190/150/130 DIP，且页面纵向滚动被禁用，审计产生 `PRIMARY_VIEWPORT_TOO_SHORT`。现在主表/表格壳保留 212 DIP 阅读下限，页面 ScrollViewer 使用有限、可达的 Auto 通道，DataGrid 继续使用 Item ScrollUnit 和虚拟化；审计复跑后 Fidelity failures 为 0、失败路由为 0。
- 媒体 Inspector 是表格页面内的独立有限详情滚动面。审计器按实际布局将它记录为 `NESTED_VERTICAL_SCROLL` 信息，而不是把有明确职责边界的详情滚动误报为父子冲突；媒体主表的页面滚动仍单独记录 `PRIMARY_SCROLL_ACCESS`。
- 新增源代码门禁覆盖媒体 212 DIP 阅读底线；`UiAuditSourceTests` 同步验证页面滚动和 Inspector 证据分类。

## 受控审计摘要

- `AUDIT_SUMMARY.md`：10 个 View、32 个 Tab、234 个 Button/ToggleButton、14 个 DataGrid、34 个 ScrollViewer、161 个运行时快照；`Fidelity 警告数量=0`、`失败路由=0`、`HIGH=无`、`MEDIUM=无`。Trainer Inspector 的五项设置选项是有意的内部设置簇，审计器按命名祖先边界排除它，不把 WrapPanel 的多行设置布局冒充页面工具栏。
- `UI_FIDELITY_MATRIX.md`：逐页面/Tab/尺寸的主元素、滚动、裁剪、虚拟化和关键控件结果。
- `LAYOUT_REPORT.md`：全尺寸布局 JSON/滚动链/表格端点记录；其中媒体待归类主表在短窗通过 `MediaInboxPageScrollViewer` 可达完整表格。
- `UI_MANIFEST.md`：静态入口、条件 UI、按钮、表格和滚动面盘点。

## Q13–Q25 覆盖映射

| 组 | 已落地/已复核的真实入口 | 自动证据 | 当前宿主边界 |
| --- | --- | --- | --- |
| Q13 | DataGridScrollDiagnostics、稳定 ID 锚点、Item ScrollUnit、表头 resize/sort 部件、媒体主表 212 DIP 修复 | `MediaWindowAnchorContractTests`、`UiAuditCaptureContractTests`、`WpfUiResourceDictionaryTests`、全审计 0 HIGH | 真实鼠标拖拽列宽、Ctrl/Shift 跨页手势仍需宿主操作 |
| Q14 | Dashboard/各页筛选、批量计数、刷新/更多筛选和响应式布局入口 | `DebouncedRefreshTests`、`TaskFilterOptionsSyncTests`、`ResponsiveLayoutCoordinatorTests`、`UiFinesseRound2ControlSourceTests`、布局矩阵 | 真实 760/980 DIP 屏幕输入序列待宿主复核；当前审计无工具栏 Medium |
| Q15 | 共享 Tooltip、Combo Popup、菜单/轻浮层资源和复制入口 | `GamePickerShellSourceTests`、`WpfUiResourceDictionaryTests`、`UiFinesseRound2ControlSourceTests`、源代码审计 | 边缘定位、Esc/点外部、独立窗口主题 Owner 待宿主复核 |
| Q16 | Dialog/Inspector/Expander 层级、详情滚动、焦点返回和失败详情顺序 | `KeyboardFocusSourceTests`、`DetailsDisclosureSourceTests`、`DiagnosticSummaryNoClipTests`、`TaskCenterViewResponsiveTests` | 真窗口模态 Tab 圈和快速开合时序待宿主复核 |
| Q17 | WorkspaceStatePresenter、ProgressBar、Toast 队列上限、Banner/错误摘要与复制 | `SessionNotificationAccumulatorTests`、`NotificationFeedbackSourceTests`、`UiFeedbackTests`、`BatchObservableCollectionTests` | 真实悬停暂停、动画终态和多任务并发像素待宿主复核 |
| Q18 | GscMotion token、资源 Host override、冻结 Transform 克隆、卸载清理和 reduced-motion 分支 | `UiFinesseFoundationTests`、`WpfUiResourceDictionaryTests`、`ProductionShellChromeSourceTests` | 实机热切换动画偏好、Rendering/ETW 生命周期待宿主/性能工具复核 |
| Q19 | AsyncThumbnailImage/Loader generation、3 路并发、96 项 LRU、DecodePixelWidth、媒体占位/预览容器 | `AsyncThumbnailImageTests`、`AsyncThumbnailLoaderTests`、`MediaThumbnailConverterTests`、布局审计 | 真机视频/图片解码帧率与跨 DPI 清晰度待宿主复核 |
| Q20 | AcrylicProductionShell、Overview 六页壳层、footer/header/hero/metric/activity/云队列入口和边界状态 | `ProductionShellChromeSourceTests`、`OverviewInteractionTests`、`OverviewPriorityResolverTests`、全审计 | 真实 Playnite 视觉树已完成安装启动，但当前未取得可签收的宿主截图 |
| Q21 | SaveCenter/TrainerCenter 状态胶囊、长路径、恢复确认、工具来源失败和风险选项 | `MetadataBackupSourceTests`、`MetadataRestoreCoordinatorTests`、`DeviceConflictStateSourceTests`、`WpfUiResourceDictionaryTests` | 危险恢复路径只允许隔离测试；真实文件/修改器动作不执行 |
| Q22 | TaskCenter/Maintenance 统计条、筛选、失败详情、云队列、诊断日志、短窗 Inspector | `TaskRetrySourceTests`、`FindingNavigationResolverTests`、`DiagnosticSummaryNoClipTests`、`MaintenanceReportSourceTests`、全审计 | 真窗口长日志复制、队列悬停和短窗输入序列待宿主复核 |
| Q23 | Settings 分类导航、字段校验、目录只读检测、保存/回滚/主题预览和底部动作 | `SettingsValidationSourceTests`、`SettingsPathValidationTests`、`PortableSettingsTests`、`WpfUiResourceDictionaryTests` | 独立设置窗口的真实键盘和保存回滚序列待宿主复核 |
| Q24 | AutomationProperties、焦点视觉、Tab/Shift+Tab 源码契约、高对比/无玻璃路径和 1040/1100/1366 布局矩阵 | `AccessibilitySourceTests`、`KeyboardFocusSourceTests`、`UiAuditTruthfulnessTests`、全审计 | 100/125/150/175/200% 物理 DPI、跨屏 Popup、中文 IME、真实读屏待宿主复核；电脑自动化助手本轮不可用 |
| Q25 | 大库批量更新、缩略图并发/缓存边界、审计性能字段、构建身份、打包/安装验证 | `LargeLibraryPerformanceTests`、`AsyncThumbnailLoaderTests`、`DiagnosticsEvidenceSourceTests`、`BuildIdentityTests`；`Q25-PERFORMANCE-BENCHMARK-20260914.txt` 记录 5 次预热、30 次输入到反馈采样及 p50/p95/max；`Q25-HOST-PERFORMANCE-BOUNDARY-20260914.txt` 记录 6450f6e 干净 HEAD 安装/启动和 WPR 策略拒绝；已提交 HEAD 一键包体身份一致 | 30 分钟耐久、ETW 呈现帧、>100ms 调用栈和低性能 Tier 尚未签收；Q25-08 需在最终阶段清理并回查 |

## 运行身份与边界

- 运行命令：`scripts/capture-ui-audit.ps1 -Configuration Release -Output artifacts/ui-audit-round2`；构建 0 warning/0 error，审计退出 0。
- Q25-07 的干净 HEAD 一键流程已完成打包、程序集身份核对、安装验证和 Playnite 启动；当前 6450f6e 运行是 468/525（57 skip，失败 0）。
- 本索引不把 `artifacts/ui-audit-round2` 的离屏事实升级为真实宿主视觉真值；阶段结束前仍须保留 Q24/Q25 的待验收项，不得为了填满 208 行而改成“完成”。
