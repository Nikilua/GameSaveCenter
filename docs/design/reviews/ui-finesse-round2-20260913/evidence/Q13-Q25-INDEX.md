# Q13–Q25 交互、页面、宿主与交付证据

采集日期：2026-09-15（Asia/Shanghai）。当前代码基线：`69e1f84`。证据来自共享 XAML/C# 源码、Playnite 设置迁移测试、RenderHarness Offscreen Regression Audit、受控 STA WPF Window 运行时测试、隔离 Playnite 真实宿主加载日志和已提交 HEAD 的打包/安装记录；本次已取得 `EmbeddedPlaynite` Dashboard 与 Settings 生产像素，修正 150% 宿主截图的 DPI 叠加和 Settings 入场动画中间帧，并补齐真实审计提交 SHA 元数据；同时保留 Overview 空活动/多风险/超长标题/离线边界夹具、Q25-03 受控 UI 动作耗时边界、六态生产状态夹具、重入动画接管、完成态时钟释放和生产侧栏窗口关闭卸载清理门禁；离屏/受控证据仍不冒充 IME、物理跨屏、读屏、ETW 帧时间或停顿期间调用栈。

## 本阶段真实修复

- `MediaCenterView` 的“待归类”主表在 compact/narrow 下曾实际只有 190/150/130 DIP，且页面纵向滚动被禁用，审计产生 `PRIMARY_VIEWPORT_TOO_SHORT`。现在主表/表格壳保留 212 DIP 阅读下限，页面 ScrollViewer 使用有限、可达的 Auto 通道，DataGrid 继续使用 Item ScrollUnit 和虚拟化；审计复跑后 Fidelity failures 为 0、失败路由为 0。
- 媒体 Inspector 是表格页面内的独立有限详情滚动面。审计器按实际布局将它记录为 `NESTED_VERTICAL_SCROLL` 信息，而不是把有明确职责边界的详情滚动误报为父子冲突；媒体主表的页面滚动仍单独记录 `PRIMARY_SCROLL_ACCESS`。
- 新增源代码门禁覆盖媒体 212 DIP 阅读底线；`UiAuditSourceTests` 同步验证页面滚动和 Inspector 证据分类。
- 生产壳层游戏选框补齐 Esc/已选游戏 Enter、点外部和选中后的关闭及焦点返回；导航、Header 动作、筛选器、游戏列表、footer 和 Overview 主要动作补充稳定 UI Automation 名称。真实 WPF STA 行为与源码契约见 [`Q09-Q24-KEYBOARD-FOCUS-AUTOMATION-20260915.md`](q13-q25/Q09-Q24-KEYBOARD-FOCUS-AUTOMATION-20260915.md)。
- Overview 云端队列整卡补充 `已上传 N · 已校验 M` 的独立保证级别显示，保留队列运行/暂停状态与单次导航命令；真实 WPF 视觉树和双主题 RenderHarness 证据见 [`Q20-07-CLOUD-GUARANTEE-20260915.md`](q13-q25/Q20-07-CLOUD-GUARANTEE-20260915.md)。
- Overview 全局活动空态补齐 `MinHeight=120` 阅读下限，并新增 `overviewedges` 夹具覆盖空活动、多风险、超长标题和离线首屏；双主题/1040×700 与 1600×900 视口、页面尾部空态和完整标准 RenderHarness 证据见 [`Q20-08-OVERVIEW-EDGE-STATES-20260915.md`](q13-q25/Q20-08-OVERVIEW-EDGE-STATES-20260915.md)。
- `WorkspaceStatePresenter` 的生产状态页新增当前 Release `statefixtures` 证据，覆盖媒体收件箱/详情、维护审计/下一步、六态、双主题和四个尺寸；Loading 覆盖层与 Stale 提示的静态可见性可追溯，动态进度时序仍保留宿主边界，详见 [`Q17-04-STATE-FIXTURES-20260915.md`](q13-q25/Q17-04-STATE-FIXTURES-20260915.md)。
- `GscMotion.AnimateEntrance` 在重入时读取当前有效 Transform/Opacity、移除旧时钟后从当前值继续；定向源测试锁定该接管契约，避免快速切换闪回起点。
- Q24-03 的生产浮层继续保持宿主内 `GameBrowserPanel`，共享 ComboBox Popup 继续使用模板定位与动态主题资源；真实宿主运行器新增 `DisplayTopology` 与 `Q24_03PhysicalCrossScreen` 前置字段。当前单屏边界和执行要求见 [`Q24-03-PHYSICAL-CROSS-SCREEN-20260915.md`](q13-q25/Q24-03-PHYSICAL-CROSS-SCREEN-20260915.md)。
- Settings 真实宿主审计修复：`RealHostUiAuditService` 等待最长入场动效结束后再取嵌入视口；`real-host-audit.ps1` 以非空 Git SHA 写入 runner/plugin metadata，避免 PowerShell `$LASTEXITCODE` 管道假阴性。最终可追溯证据见 [`REAL_HOST_AUDIT-20260914.md`](q13-q25/REAL_HOST_AUDIT-20260914.md#settings-入场时序与提交身份修复后的最终复核69e1f84)。

## 受控审计摘要

- `AUDIT_SUMMARY.md`：10 个 View、32 个 Tab、234 个 Button/ToggleButton、14 个 DataGrid、34 个 ScrollViewer、161 个运行时快照；`Fidelity 警告数量=0`、`失败路由=0`、`HIGH=无`、`MEDIUM=无`。Trainer Inspector 的五项设置选项是有意的内部设置簇，审计器按命名祖先边界排除它，不把 WrapPanel 的多行设置布局冒充页面工具栏。
- `UI_FIDELITY_MATRIX.md`：逐页面/Tab/尺寸的主元素、滚动、裁剪、虚拟化和关键控件结果。
- `LAYOUT_REPORT.md`：全尺寸布局 JSON/滚动链/表格端点记录；其中媒体待归类主表在短窗通过 `MediaInboxPageScrollViewer` 可达完整表格。
- `UI_MANIFEST.md`：静态入口、条件 UI、按钮、表格和滚动面盘点。

## 隔离 Playnite 真实宿主审计

- [REAL_HOST_AUDIT-20260914.md](q13-q25/REAL_HOST_AUDIT-20260914.md) 记录了 `69e1f84` 的最终 Release 构建、打包、隔离安装和真实 Playnite 启动：构建 0 warning/0 error、Core `83/83`、Worker `311/311`、Playnite `494/551`（57 skipped，0 failed）；非空隔离库实际观察到 3 个 Playnite 游戏，FusionX 主题复制成功，并由 Playnite 自身 `SelectSidebarViewCommand` 选择 GameSaveCenter。完整历史复核仍保留在同一文档。
- 最新 `summary.json`（`artifacts/ui-host-audit-round2-final-20260915`）取得 `EmbeddedDashboardCaptured=true`、`EmbeddedSettingsCaptured=true`、`EmbeddedDashboardOrigin=EmbeddedPlaynite`、`EmbeddedSettingsOrigin=EmbeddedPlaynite`、`ControlledDashboardCaptured=false`、`ProductionVisualSourceOfTruthAvailable=true`；修复截图渲染器的宿主 DPI 叠加与 Settings 入场时序后，150% DPI 下 29 个 Dashboard 视口、2 个滚动面和 1 个 Settings 视口均保持完整性校验通过，runner 与 metadata 绑定 `69e1f84` 完整 SHA。
- 运行器的 UIA 侧栏未定位警告仍被保留为非权威诊断信息；宿主原生命令日志、嵌入捕获元数据和 96-DPI 基线回归测试共同证明当前深色 Dashboard 的生产像素来源及截图边界。Q24-03 现在具备自动显示器拓扑前置记录，但第二物理屏上的窗口迁移与打开态 Popup 仍未执行；Q20 的完整状态/交互矩阵和 Q25-02～Q25-05 的性能/耐久证据仍保持未完成。

## Q13–Q25 覆盖映射

| 组 | 已落地/已复核的真实入口 | 自动证据 | 当前宿主边界 |
| --- | --- | --- | --- |
| Q13 | DataGridScrollDiagnostics、稳定 ID 锚点、Item ScrollUnit、表头 resize/sort 部件、媒体主表 212 DIP 修复 | `MediaWindowAnchorContractTests`、`UiAuditCaptureContractTests`、`WpfUiResourceDictionaryTests`、全审计 0 HIGH | 真实鼠标拖拽列宽、Ctrl/Shift 跨页手势仍需宿主操作 |
| Q14 | Dashboard/各页筛选、批量计数、刷新/更多筛选和响应式布局入口 | `DebouncedRefreshTests`、`TaskFilterOptionsSyncTests`、`ResponsiveLayoutCoordinatorTests`、`UiFinesseRound2ControlSourceTests`、布局矩阵 | 真实 760/980 DIP 屏幕输入序列待宿主复核；当前审计无工具栏 Medium |
| Q15 | 共享 Tooltip、Combo Popup、菜单/轻浮层资源和复制入口 | `GamePickerShellSourceTests`、`WpfUiResourceDictionaryTests`、`UiFinesseRound2ControlSourceTests`、`KeyboardFocusSourceTests`、源代码审计；Q15-03/Q15-07/Q15-08 自动门禁与窗口级资源隔离见 `Q15-TOOLTIP-POPUP-THEME-20260915.md`；游戏选框关闭/焦点返回见 `Q09-Q24-KEYBOARD-FOCUS-AUTOMATION-20260915.md` | Combo/菜单边缘定位、独立窗口真实主题 Owner/打开态切换和真实宿主时序仍待复核 |
| Q16 | Dialog/Inspector/Expander 层级、详情滚动、焦点返回和失败详情顺序 | `KeyboardFocusSourceTests`、`DetailsDisclosureSourceTests`、`DiagnosticSummaryNoClipTests`、`TaskCenterViewResponsiveTests`；`4414f05`/`68b49a1` 的对话框动画代际、完成/取消清理和受控终态门禁见 [`Q18-04-07-MOTION-CLEANUP-20260915.md`](q13-q25/Q18-04-07-MOTION-CLEANUP-20260915.md) | 真窗口模态 Tab 圈和快速开合时序待宿主复核 |
| Q17 | WorkspaceStatePresenter、ProgressBar、Toast 队列上限、Banner/错误摘要与复制 | `SessionNotificationAccumulatorTests`、`NotificationFeedbackSourceTests`、`UiFeedbackTests`、`BatchObservableCollectionTests`；[`Q17-04-STATE-FIXTURES-20260915.md`](q13-q25/Q17-04-STATE-FIXTURES-20260915.md) 记录当前 Release 六态生产页夹具与 160 张截图；`4414f05`/`68b49a1` 的 Toast 完成/容量淘汰/卸载逐卡清理和受控终态门禁见 [`Q18-04-07-MOTION-CLEANUP-20260915.md`](q13-q25/Q18-04-07-MOTION-CLEANUP-20260915.md) | 真实悬停暂停、动画终态和多任务并发像素待宿主复核 |
| Q18 | GscMotion token、资源 Host override、冻结 Transform 克隆、重入时当前值接管、完成态时钟清理、卸载释放和 reduced-motion 分支 | [`Q18-01-MOTION-HOST-20260915.md`](q13-q25/Q18-01-MOTION-HOST-20260915.md)、[`Q18-04-07-MOTION-CLEANUP-20260915.md`](q13-q25/Q18-04-07-MOTION-CLEANUP-20260915.md)；`UiFinesseFoundationTests`、`WpfUiResourceDictionaryTests`、`ProductionShellChromeSourceTests`、`UiFinesseRound2ControlSourceTests`；`8dfe7fa` 锁定真实视觉宿主时长解析，`4414f05` 锁定完成/取消/卸载清理和 Toast 逐卡释放，`68b49a1` 新增通用 STA WPF Window 终态回归，`39e37b1` 新增生产侧栏完成/窗口关闭卸载回归并修正位移归零 | 实机热切换动画偏好、100 次 Loaded/Unloaded 计数、Rendering/ETW 生命周期仍待宿主/性能工具复核 |
| Q19 | AsyncThumbnailImage/Loader generation、3 路并发、96 项 LRU、DecodePixelWidth、媒体占位/预览容器 | `AsyncThumbnailImageTests`、`AsyncThumbnailLoaderTests`、`MediaThumbnailConverterTests`、布局审计 | 真机视频/图片解码帧率与跨 DPI 清晰度待宿主复核 |
| Q20 | AcrylicProductionShell、Overview 六页壳层、footer/header/hero/metric/activity/云队列入口和边界状态 | `ProductionShellChromeSourceTests`、`OverviewInteractionTests`、`OverviewPriorityResolverTests`、`UiDisplayMappingTests`、`UiFinesseRound2ControlSourceTests`、`overviewedges`、全审计、隔离宿主加载日志；`69e1f84` 在非空 3 游戏隔离库取得 `EmbeddedPlaynite` 当前深色 150% Dashboard 视口与滚动面，并修正审计截图 DPI 叠加和 Settings 入场时序；Q20-07 的上传/远端校验分开显示见 `Q20-07-CLOUD-GUARANTEE-20260915.md`，Q20-08 边界夹具见 `Q20-08-OVERVIEW-EDGE-STATES-20260915.md` | 当前宿主像素已可追溯且右边界完整；Hover/Focus/单次导航、低于 560 DIP、空活动/多风险/超长标题/离线边界的真实宿主操作，以及其他主题仍需宿主复核 |
| Q21 | SaveCenter/TrainerCenter 状态胶囊、长路径、恢复确认、工具来源失败和风险选项 | `MetadataBackupSourceTests`、`MetadataRestoreCoordinatorTests`、`DeviceConflictStateSourceTests`、`WpfUiResourceDictionaryTests` | 危险恢复路径只允许隔离测试；真实文件/修改器动作不执行 |
| Q22 | TaskCenter/Maintenance 统计条、筛选、失败详情、云队列、诊断日志、短窗 Inspector | `TaskRetrySourceTests`、`FindingNavigationResolverTests`、`DiagnosticSummaryNoClipTests`、`MaintenanceReportSourceTests`、全审计 | 真窗口长日志复制、队列悬停和短窗输入序列待宿主复核 |
| Q23 | Settings 分类导航、字段校验、目录只读检测、保存/回滚/主题预览和底部动作 | `SettingsValidationSourceTests`、`SettingsPathValidationTests`、`PortableSettingsTests`、`WpfUiResourceDictionaryTests`；`a1f3cae` 的受控 STA WPF 夹具覆盖 Popup/ToolTip 打开态 Light→Dark 主题预览，`69e1f84` 真实 `EmbeddedPlaynite` Settings 在 150% DPI 下取得稳定可读的 `1278×762 DIP` 视口，并绑定完整 runner/plugin SHA | 独立设置窗口的真实键盘和保存回滚序列、宿主主题跟随和其他物理 DPI 仍待宿主复核；打开态截图见 [`Q21/Q23 页面视觉证据`](q13-q25/Q21-Q23-RENDER-20260915.md) 与 [`Q03-07 主题切换覆盖`](q03/Q03-THEME-SWITCH-COVERAGE.md) |
| Q24 | AutomationProperties、焦点视觉、Tab/Shift+Tab 源码契约、高对比/无玻璃路径和 1040/1100/1366 布局矩阵；Q24-03 运行器显示器拓扑与宿主内浮层边界 | `AccessibilitySourceTests`、`KeyboardFocusSourceTests`、`UiAuditTruthfulnessTests`、`DiagnosticsEvidenceSourceTests`、`UiFinesseRound2ControlSourceTests`、全审计；`Q24-03-PHYSICAL-CROSS-SCREEN-20260915.md` 当前分支前置复核源契约 `17/17` 通过 | 当前主机仍仅枚举到 `DISPLAY1` 单屏，100/125/150/175/200% 物理 DPI、完整六页 Tab/Shift+Tab、跨屏 Popup、中文 IME、真实读屏仍待满足宿主条件后复核；电脑自动化助手本轮不可用 |
| Q25 | 大库批量更新、缩略图并发/缓存边界、审计性能字段、构建身份、打包/安装验证 | `LargeLibraryPerformanceTests`、`AsyncThumbnailLoaderTests`、`DiagnosticsEvidenceSourceTests`、`BuildIdentityTests`；`Q25-01-HOT-RESPONSE-20260915.md` 记录当前干净提交 5 次预热、30 次输入到反馈采样及 p50/p95/max=45/46/60 ms，`Q25-02-RENDERING-PROXY-20260915.md` 记录当前干净提交 WPF Rendering 回调的 p95/最大间隔/60Hz 慢帧比例代理，`Q25-03-UI-HOTSPOT-20260915.md` 记录当前干净提交受控 WPF 动作周期 p95/max=`249.94/483.44 ms`、`>100ms`=`254/254` 及完成后栈边界，`Q25-04-ENDURANCE-20260915.md` 记录当前干净提交受控 WPF Window 的 1800.4 秒/3847 循环/13462 动作/176 资源样本，`Q25-05-LOW-COST-FALLBACK-20260915.md` 记录当前干净提交双主题六工作区禁玻璃/禁动画 24/24 视觉回退，`Q25-08-RELEASE-TEST-BASELINE-20260915.md` 同时记录历史动效/侧栏基线和 `69e1f84` 最终真实宿主构建，当前最终审计为 Core 83/83、Worker 311/311、Playnite 494/551（57 skip）；`Q25-PERFORMANCE-BENCHMARK-20260914.txt` 保留前一轮基线；`REAL_HOST_AUDIT-20260914.md` 记录 `69e1f84` 非空隔离包体、主题复制、宿主原生命令、29/2/1 嵌入捕获、DPI/Settings 时序修正、安装和 Playnite 加载；`Q25-ETW-TOOL-BOUNDARY-20260914.md` 记录 xperf DWM 会话被 `0x5 / Access denied` 拒绝；最新源门禁为 XAML 24/24、Release 0 warning/0 error、Core 83/83、Worker 311/311、Playnite 494/551（57 skip） | 受控代理、UI 动作边界、30 分钟窗口和动效终态释放已有证据，但真实屏幕呈现帧、Playnite 宿主耐久、停顿期间 `>100ms` 调用栈和低性能 Tier 尚未签收 |

> Q25-08 最新跟进：`69e1f84` 的 Release 宿主审计基线为 Core `83/83`、Worker `311/311`、Playnite `494/551`（57 skip），失败 `0`；同次完成打包、隔离安装、真实嵌入 Dashboard/Settings 捕获并把 runner/plugin metadata 绑定完整 SHA。历史 `39e37b1` 生产侧栏完成/窗口关闭卸载回归和动效清理跟进见 [`Q25-08-RELEASE-TEST-BASELINE-20260915.md`](q13-q25/Q25-08-RELEASE-TEST-BASELINE-20260915.md)。

## 运行身份与边界

- 运行命令：`scripts/capture-ui-audit.ps1 -Configuration Release -Output artifacts/ui-audit-round2`；构建 0 warning/0 error，审计退出 0。
- Q25-07 的干净 HEAD 隔离宿主流程已完成打包、程序集身份核对、安装验证、真实打开和嵌入 Dashboard 捕获；最新 `69e1f84` 运行是 Core 83/83、Worker 311/311、Playnite 494/551（57 skip，失败 0），确认隔离配置主题已复制，并由宿主原生命令成功进入 GameSaveCenter。非空库观察到 3 个游戏，Dashboard 29 个视口、2 个滚动面和 Settings 1 个视口均保留 `EmbeddedPlaynite` 来源，DPI 截图完整性回归已通过。Q25 ETW 工具边界见 [`Q25-ETW-TOOL-BOUNDARY-20260914.md`](q13-q25/Q25-ETW-TOOL-BOUNDARY-20260914.md)。
- 本索引只把 `69e1f84` 明确标记为 `EmbeddedPlaynite` 的当前宿主像素升级为生产视觉真值；不把它升级为 Hover/Focus、键盘/IME、读屏、物理跨屏、浅色/高对比、ETW 帧时间、30 分钟耐久或低 Tier 通过。阶段结束前仍须保留 Q24/Q25 的待验收项，不得为了填满 208 行而改成“完成”。
