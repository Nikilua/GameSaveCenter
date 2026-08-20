# GameSaveCenter AI/Codex 长期项目记忆

> 维护时间：2026-08-21
> 本文件面向新的 AI/Codex 会话，目标是在几分钟内恢复项目状态，避免重复实现已完成的工作。

## 2026-08-20 当前总规则：Demo-first 覆盖旧视觉优先级

- 后续所有页面迁移以 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/DesignShellView.xaml`、`Pages/*.xaml`、`DesignTokens.xaml`、`DesignColorsLight.xaml`、`DesignColorsDark.xaml` 和 `DesignControls.xaml` 为唯一主要视觉基准；Demo 与旧生产页面、UiLab、历史计划或通用 Apple-inspired 建议冲突时，以 Demo 的整体结构、层级、空间、字体、颜色和控件为准。
- `wpf-apple-desktop-ui` 不再是视觉与实现路线的优先约束，只作为 WPF 质量检查依据，继续检查真实 Binding/Command、异步错误/取消/安全语义、虚拟化、键盘/UI Automation、可访问性、主题/DPI 和 Playnite 兼容性。
- 当前游戏选框、生产滚动条系统、真实运行时数据和 Demo 未覆盖但目标文件明确要求保留的功能继续保留；Demo Mock 数据、演示色板、窗口按钮和演示行为不得接入生产。
- 本段覆盖早期“当前生产 main > Demo”或“技能优先”的视觉排序；旧条目只用于历史追溯，不得阻止 Demo-first 的新页面迁移。

## 2026-08-21 UI-271 真实 Playnite 宿主审计边界复核

- Release `0.6.70+c6cb235ef446fbe6e0c12566a7920c92e2135af8` 已通过 `scripts/real-host-audit.ps1` 安装并启动 Playnite；`artifacts/ui-host-audit-ui271/summary.json` 明确记录 `EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`EmbeddedDashboardCaptured=false`、`ProductionVisualSourceOfTruthAvailable=false`。
- Settings 的 `settings/embedded-current/viewport/settings.png`、视觉树和资源快照来自真实 `EmbeddedPlaynite` 宿主，可作为 Settings 嵌入证据。Dashboard 自动 UI Automation 仍未定位左侧 GameSaveCenter 入口，`gates/REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED.json` 的 HIGH 门禁有效；受控 Dashboard 图像只能作为布局辅助，不能冒充生产视觉真值。
- Computer Use 观察到 Playnite 主窗口为 `EmptyWindowAutomationPeer`；未绕过该限制，也未停止另一个扩展目录中的旧 Worker。后续必须在用户可见、可交互的 Playnite 窗口中打开 GameSaveCenter 后重跑审计，才可补齐七页 Dashboard 的像素、DPI、键盘焦点、命中区域和真实操作证据。
- 审计内 Release 基线为 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过/0 失败；这不改变总 Demo-first 目标未完成的判断。

## 2026-08-21 UI-271 当前事实：共享表格使用 Demo 正文与表头字阶

- `Themes/DesignTokens.xaml` 当前提供 `GscBodyFontSize=13.5`、`GscCaptionFontSize=12`，分别对应 Demo `SizeBody` 和 `SizeCaption`；生产隐式 `DataGrid` 使用 UI 字体链和正文令牌，`DataGridColumnHeader` 使用 UI 字体链、表头令牌和 Medium 字重。
- 这只统一表格文本密度，不改变 `GscTableRowHeight=44`、`GscTableHeaderHeight=36`、排序箭头、列宽调整、选中态、内部滚动、Recycling 虚拟化或真实表格绑定。
- 当前证据：`artifacts/gsc-b/ui-271-table-typography-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui271-table-typography-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。真实 Playnite 宿主字号、DPI、键盘和列宽拖动验收仍未收口。

## 2026-08-21 UI-270 当前事实：共享折叠栏箭头动效对齐 Demo

- `Themes/DesignTokens.xaml` 的 `GscDisclosureCardExpander` 现在在 `IsChecked` 进入/离开时以 150ms 将 Chevron 在 `-90°` 与 `0°` 间旋转，匹配 Demo `LabDisclosure`；`GscDisclosureCard` 继续作为统一别名。
- 这只改变共享控件的视觉状态过渡，保留整行点击、键盘焦点、内容显隐、真实 Expander 绑定和页面滚动；没有改变业务命令、数据、虚拟化或项目 ScrollBar。
- 当前证据：`artifacts/gsc-b/ui-270-disclosure-animation-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 261 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui270-disclosure-animation-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。真实 Playnite 宿主的动效时间、键盘焦点和逐页视觉验收仍未收口。

## 2026-08-21 UI-269 当前事实：Demo 核心主题色不再被宿主中性刷覆盖

- `AdaptiveThemePaletteFactory.ApplyDemoCoreResources` 是生产 Shell 与 Settings 共用的核心色板入口，固定 Demo 的浅色/深色画布渐变、卡片、侧栏、顶栏、输入框、文字层级、表格、分段控件、滚动条、遮罩和语义状态色；宿主 Accent/focus 仍保留给非核心交互。
- 高对比度通过提前返回继续使用系统自适应路径；普通主题不再由 Playnite 背景/正文中性刷重写已迁移页面的核心表面。生产 Tab chrome、当前游戏选框、滚动条行为、虚拟化、命令/Binding 和真实业务数据没有改变。
- 当前证据：`artifacts/gsc-b/ui-269-demo-palette-v2` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 260 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui269-demo-palette-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。截图仍不能替代可识别 Playnite 宿主的逐页像素、DPI、键盘、主题和真实操作验收。

## 2026-08-21 UI-268 当前事实：标题字体接入独立 Display 字阶

- `src/GameSaveCenter.Playnite/Themes/DesignTokens.xaml` 当前同时提供 `GscUiFontFamily`（`Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI`）、`GscDisplayFontFamily`（`Segoe UI Variable Display, Segoe UI, Microsoft YaHei UI`）和 `GscCodeFontFamily`（`Cascadia Mono, Consolas, Microsoft YaHei UI`）。
- `GscRedesignHeroTitle`、`GscRedesignFeedbackDialogTitle`、`GscPageTitleStyle`、`AcrylicProductionShellView` 的 `PageTitleText` 和 `DashboardView` 的回退标题使用 Display 字阶；`GscRedesignSectionTitle` 继续使用继承的正文族，保持 Demo `LabTitle`/`LabSection` 的层级关系。
- 标题字体改动没有触及生产 Tab chrome、当前游戏选框、滚动条系统、虚拟化、真实命令/Binding 或业务数据；回归测试保护共享令牌和两个生产标题入口。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 259 通过/62 跳过；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui268-display-font-v1/render-qa-report.txt` 双主题、多尺寸、滚动和 resize 均为 `OK`。总目标仍需继续完成 Demo 七页结构/视觉逐项核对及可识别 Playnite 宿主的像素、DPI、键盘和真实操作证据。

## 2026-08-21 UI-267 当前事实：工作区表格测量与几何审计已收口

- `MediaCenterView.xaml` 的当前游戏媒体搜索操作区保持至少 `300 DIP`，搜索输入列保持至少 `160 DIP`；真实 `MediaSearchText`、媒体类型筛选、媒体卡片、预览 Inspector 和批量操作没有改变。
- `SaveCenterView.xaml.cs` 在工作区宽度低于 `1240 DIP` 时启用历史表紧凑列宽；这是为了在标准宿主的 Inspector 并列布局中保持状态列可达，不是删除列或隐藏操作。DataGrid 的 `Auto` 横向滚动、列宽拖动、排序和 Recycling 虚拟化继续由共享生产样式负责。
- 回归断言 `SharedWorkspaceBreakpointsKeepSearchAndHistoryEssentialsReadable` 保护上述两个空间契约；生产 Tab chrome、当前游戏选框、页面滚动、真实命令/绑定和异步安全语义均未改动。
- UI 审计已按主控件直接所属 Grid 行计算纵向填充，允许 Overview 的有限本地虚拟视口，排除列表内部媒体卡片误判工具栏；列宽超过视口但 `DG_ScrollViewer` 有真实 Auto 横向滚动时记为 `EXPECTED_HORIZONTAL_SCROLL`。最新 `artifacts/ui-audit-ui267-fix3` 为 Fidelity 0、HIGH 0、MEDIUM 0、失败路由 0。
- 当前验证证据：`artifacts/gsc-b/ui-audit-layout-fix-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 259 通过/62 跳过；`artifacts/ui-qa/ui267-layout-audit-fix-v1` 为 `render-qa OK`。WPF 静态检查保留 0 error、19 warnings、161 info。真实 Playnite 宿主的逐页像素、DPI、键盘焦点与真实操作仍是总目标的未收口边界。

## 2026-08-20 UI-266 当前事实：存档维护指标统一数值优先阅读

- 存档页“比较与保留”中的新增文件、修改文件、删除文件指标现在统一为“数值 → 标签”；真实 `LastBackupDiff` 绑定、差异文件列表、比较命令和只读保留预览均保持不变。
- 维护页的保留、容量、趋势、保留模拟、保护状态和本地镜像指标统一为“数值 → 标签 → 补充说明”，绑定仍来自真实运行时状态，不得用 Demo 示例数字替换。
- 本阶段没有改变生产 Tab chrome、页面滚动、DataGrid/列表虚拟化、Inspector、命令或安全语义；后续新增指标卡继续优先检查 Demo 的数值优先阅读节奏。
- 当前证据：`artifacts/gsc-b/metrics-rhythm-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/metrics-rhythm-v1` 双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主验收。

## 2026-08-20 UI-265 当前事实：维护诊断概览先显示环境健康

- `MaintenanceView.xaml` 的诊断概览现在按 Demo 顺序先显示 `DiagnosticHealthCard`/`DiagnosticHealthPanel`，再显示 `EnvironmentCheckCard` 与 `MaintenanceDiagnosticsActionCard`；不要把六项健康状态重新藏回“更多维护操作”展开区。
- 健康卡仍来自真实运行时绑定：Worker/Ludusavi/Rclone 状态、数据与媒体目录、待归类媒体数和设备比较数；环境检查、诊断复制/导出、自检、索引重建、任务协调、元数据灾备、路径迁移与安全模式命令没有改变。
- `DiagnosticHealthPanel` 的响应式列数仍由 `ApplyResponsiveLayout` 控制为宽屏 4 列、中等 2 列、窄屏 1 列；生产 Tab chrome 是用户明确例外，不迁移为 Demo 的 segmented UI。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/maintenance-health-order-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-264 当前事实：首页统计条已恢复 Demo 连续结构

- `OverviewView.xaml` 的 `OverviewStatStrip` 当前是一个 `GscRedesignSectionCard` 连续统计条，六个等宽指标使用五条 `GscTableDividerBrush` 分隔；数字在上、标签在下，不要恢复六张带间隙的独立 metric card 或旧的 `UniformGrid.Columns` 响应式换列。
- 六项显示继续绑定真实 `Snapshot.ManagedGames`、`Snapshot.MatchedGames`、`Snapshot.RunningGames`、`Snapshot.WarningGames`、`Snapshot.PendingCloudTasks`、`Snapshot.UnassignedMediaCount`；匹配/风险进度条与 `ManagedGames == 0` 时隐藏的防护仍有效，健康/注意/风险/未知明细也继续来自 Snapshot。
- `OverviewStatStrip` 的命名 XAML 元素已从 `UniformGrid` 调整为 `Border`，`ApplyResponsiveWidth` 不再调整统计列数；今日工作台、当前游戏选框、立即备份/全部备份、活动列表虚拟化、页面滚动和 hover render-only gate 均未迁移。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/overview-summary-strip-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-263 当前事实：任务统计条已恢复 Demo 连续结构

- `TaskCenterView.xaml` 顶部 `TaskSummaryPanel` 当前是一个 `GscRedesignSectionCard` 连续统计条，四个等宽指标之间使用 `GscTableDividerBrush` 分隔；不要恢复旧的可变列数 `UniformGrid` 或独立 metric card。
- 四项真实统计继续绑定 `Tasks.Count`、`RunningTaskCount`、`RetryableTaskCount`、`CompletedTaskCount`；运行中使用 `GscAccentBrush`、需要重试使用 `GscWarningBrush`、今日完成使用 `GscSuccessBrush`，与 Demo 的状态层级一致。
- `TaskSummaryPanelElement` 已从 `UniformGrid` 调整为 `Border`；`ApplyResponsiveLayout` 不再调整摘要列数，但仍负责任务表 236 DIP 最小视口、筛选重排、Inspector 堆叠与详情高度。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/task-summary-strip-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-262 当前事实：媒体统计条已恢复 Demo 连续结构

- `MediaCenterView.xaml` 顶部 `MediaSummaryPanel` 当前是一个 `GscRedesignSectionCard` 连续统计条，四个等宽指标之间使用 `GscTableDividerBrush` 分隔；不要恢复为四张带间隙的 `GscRedesignMetricBorder` 独立卡片。
- 四组显示继续绑定真实值：总媒体/截图/录像来自 `MediaSummary`，占用来自 `MediaSummary.TotalSizeDisplay`，收藏来自 `MediaSummary.FavoriteCount`，待归类来自 `Snapshot.UnassignedMediaCount`；Demo 示例数字没有进入生产。
- `MediaSummaryPanelElement` 已从 `UniformGrid` 调整为 `Border`，`ApplyResponsiveLayout` 不再设置不存在的 `Columns`；媒体来源字段仍独立使用 `UniformGrid` 的响应式列数。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过；XAML/source/WPF/diff 门禁通过；`artifacts/ui-qa/media-summary-strip-v1` 的双主题、多尺寸、滚动和 resize `render-qa OK`。截图仍属于离屏证据，不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-253 当前事实：修改器中心已切换为 Demo 分段面板

- `TrainerCenterView.xaml` 当前使用 `TrainerSegmentTabs` + `LabSegmented`，通过 `PanelTools`、`PanelImport`、`PanelCatalog`、`PanelReleases` 四个命名面板承载 Demo 页面结构；不要恢复旧 `TabControl/TabItem` 外壳作为主要导航。
- 分段切换只控制 `Visibility`，真实入口继续存在：`ImportTrainerCommand`、`ImportToolFolderCommand`、`ImportCheatTableCommand`、`ImportCustomLaunchItemCommand`、`ConfirmGameToolImportCommand`、`SearchTrainerCatalogCommand`、`LoadTrainerReleasesCommand` 和 `DownloadTrainerCommand`。
- `TrainerToolsList`、目录结果和发行版本列表继续使用项目现有回收虚拟化和 ScrollViewer 交互；工具设置 Inspector、窄宽详情抽屉和响应式布局仍由 `ApplyResponsiveLayout` 管理。`LabSegmented` 四项导航属于有限标签列表，源码门禁不得要求它承担大列表虚拟化契约。
- XAML 构造期分段事件必须保留面板字段空保护；WPF 页面初始化时 `SelectedIndex` 可能早于后续命名面板生成。
- 当前证据：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 252/252 通过、61 跳过；XAML/source/diff/WPF 静态门禁通过；`artifacts/ui-qa/trainer-segmented-final` 的 RenderHarness 双主题、多尺寸、resize 和四分段探针通过。仍不能把离屏 PNG 作为 Playnite 宿主逐页像素验收。

## 2026-08-20 UI-251 当前事实：存档规则卡与诊断概览按 Demo 第三轮收口

- 存档中心当前规则卡在常见工作区宽度下横向排列“当前存档规则 / 游戏名 / 状态 / 立即扫描 / 重新校验 / 刷新详情”；低于 700 DIP 才堆叠操作，避免正常宿主中规则信息和按钮被拉成多行。
- 维护中心诊断概览以 AcrylicFork 生产页面为结构基线：环境检查卡在前，诊断操作卡在后，健康指标位于“更多维护操作”内；真实检查、修复、导出、取消命令与 Binding 未移除。
- 共享生产表格排序箭头采用 Demo 的 14 DIP 表头保留列和完整路径几何；列宽拖拽、排序、固定行高和虚拟化契约继续有效。
- 设置页与生产壳体/工作区统一继承 `GscUiFontFamily`；`DashboardView` 是安全回退页，已有同一字体入口。
- 当前证据：Playnite 测试 251 通过、61 跳过、0 失败；`validate-source.py`、WPF UI 静态校验（0 error）、`git diff --check` 和 RenderHarness Release `render-qa OK` 均通过。仍无新的可识别 Playnite 生产宿主像素证据，不能把离屏结果写成真实宿主 1:1 验收。
- 已清理两条旧 AcrylicFork 基线门禁：它们曾要求首页没有“今日工作台”、媒体页必须存在已废弃的 `MediaSummaryTabStrip`/`MediaTabStrip`，与当前 Demo 结构和用户要求相反；现改为保护当前工作台、完整 TabControl、媒体 Inspector 和可预览入口。

## 2026-08-20 UI-252 当前事实：存档历史页补回 Demo 操作卡

- 存档中心默认“历史版本”页现在在表格上方显示 `SaveHistorySummaryCard`，包含真实 `Backups.Count` 版本数、当前规则/健康状态摘要以及“立即扫描 / 重新校验 / 刷新详情”三个真实命令入口；不再只有表格和选中后才出现的详情按钮。
- 摘要卡在 700 DIP 以下把操作区移到第二行，正常工作区保持标题、版本数与操作横向排列；历史表仍保持 DataGrid 的列宽拖动、排序、Item scrolling、行/列虚拟化和项目现有滚动条。
- 当前证据：XAML structural validation 18 个文件通过；Release 构建 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 251 通过/61 跳过/0 失败；RenderHarness Release `render-qa OK`，双主题、多尺寸和 resize transition 均通过。仍无新增可识别 Playnite 生产宿主像素证据，不能把离屏结果写成真机 1:1 验收。

## 2026-08-20 UI-250 当前事实：媒体页已恢复 AcrylicFork Tab 结构

- 媒体中心以 AcrylicFork 实际 `MediaCenterView.xaml` 为结构基线：摘要四卡在顶部，工作区 `TabControl` 负责自己的标题栏和内容，不能再恢复成独立 Tab 标题 + `MediaTabContentHost` 的拼接结构。
- 待归类 DataGrid 外部保留 `MediaInboxInspectorScrollViewer`，绑定 `SelectedInboxMedia`，侧栏包含截图/录像预览、来源/原因、目标游戏、`AssignInboxMediaCommand` 与 `IgnoreInboxMediaCommand`。常见 744 DIP 内容宽度下 Inspector 保持侧栏，低于 700 DIP 才堆叠。
- 生产壳体及 Overview/Save/Media/Maintenance/Task/Trainer 根节点显式继承 `GscUiFontFamily`；图标仍使用明确的 Segoe MDL2 Assets，不要用页面根字体覆盖图标。
- UI-248 的“独立 MediaTabContentHost”只保留作历史记录，当前实现以 UI-250 为准。

## 2026-08-20 UI-249 当前事实：AcrylicFork 视觉迁移已重新启动

- 本轮最终视觉参考以 `D:\workplace\github\GameSaveCenter.AcrylicFork` 为准。旧的“不要恢复/不要替换”页面迁移限制已经失效；页面级结构、共享模板、Tab/分段导航和滚动模型可以按参考实现重构，但真实命令、Binding、数据契约、错误/取消/安全语义、虚拟化和 Playnite 兼容性仍必须保留。
- 首页已补回独立“今日工作台”操作区；首页最近任务项固定为 54 DIP、消息单行省略并提供 Tooltip，不能再让多行 DetailMessage 撑高单条记录。
- 存档中心生产壳体现在同时显示“立即备份”（`BackupSelectedCommand`）和“全部备份”（`BackupAllCommand`），前者只作用于当前游戏。
- 共享 `GscRedesignTableFrame`、DataGrid 行/表头样式和 `GscDisclosureCard` 已按 AcrylicFork Demo 的紧凑密度首轮收口；后续页面阶段必须继续检查有限 Grid 行、Inspector 和操作区是否发生隐藏堆叠。
- 当前证据：RenderHarness Release `render-qa OK`、source/WPF 校验和 `git diff --check` 通过；尚未获得可识别的 Playnite 生产宿主像素证据。

## 2026-08-19 UI-247 当前事实：任务搜索区必须是单一输入面

- 任务中心的“搜索任务…”必须作为 `TaskSearchTextBox` 内部提示，搜索图标也在同一输入区内；禁止恢复独立标签列，否则真实宿主会把输入框测量成窄条。
- 当前响应式下限为桌面 `420 DIP`、中等 `300 DIP`、紧凑 `180 DIP`；该逻辑不改变 `TaskSearchText` Binding、状态/类型筛选或刷新命令。
- 如果宿主仍出现“外部搜索任务… + 旁边空框”，优先判断为旧 DLL/旧安装目录/未重启旧 Playnite 进程，不能据此判断当前源码已经回退。

## 2026-08-19 UI-248 当前事实：媒体摘要卡片与 Tab 导航已分区

- `MediaSummaryTabStrip` 现在只承载四张独立等宽统计卡片；统计卡片不再与 Tab 导航共用一个外框。
- `MediaTabStrip` 单独承载“待归类 / 当前游戏媒体 / 来源规则”导航，`MediaTabContentHost` 位于下一行并占据剩余空间。
- 该结构保留真实媒体 Binding、详情 Inspector、项目自身 ScrollBar 和虚拟化，不迁移 Demo 顶部彩色主题按钮；离屏回归通过，仍需可识别的 Playnite 宿主像素截图完成最终验收。

## 2026-08-19 UI-246 历史事实：媒体标签材质基线

- 该阶段曾将媒体标签栏尝试调整为紫色强调材质；后续 UI-248 已按页面信息架构将统计卡片与 Tab 导航拆为两个独立区域，当前实现以 UI-248 为准。

## 2026-08-19 UI-245 当前事实：任务搜索输入区宽度

- 任务中心的搜索提示必须属于 `TaskSearchTextBox` 内部内容，不能再用独立标签占用筛选栏列。
- `TaskSearchBoxHost` 的响应式最小宽度为桌面 `420 DIP`、中等 `300 DIP`、紧凑 `180 DIP`；这是为真实 Playnite 宿主的测量差异设置的布局下限，不改变搜索 Binding 或筛选命令。
- Release 包已重新安装并由 Playnite 日志确认加载生产扩展 DLL；若 Playnite 未出现可识别窗口，后续仍需重新取得宿主截图后再判断视觉结果。

## 当前有效 UI 方向（2026-08-17）

- 用户已明确授权页面级、整页 UI 重构。页面布局、信息架构、导航结构、Tab/Segmented 方案、控件类型、ControlTemplate、共享样式和滚动实现都可以重新设计；不再把上一轮 UiLab/AcrylicFork 迁移中的“不要恢复/不要替换/明确不迁移”当作当前硬性禁令。
- 旧阶段条目继续保留用于追溯当时的迁移决策，但它们不应阻止新的页面方案。新设计默认继续保护真实命令、Binding、数据契约、错误/取消/安全语义、可访问性、列表性能和 Playnite 兼容性；如果设计需要改变这些内容，必须在当前任务中明确并验证，而不是因为历史实现而回避布局或控件重构。
- 本段是当前方向声明，优先于下方 2026-08-17 之前的页面迁移偏好；后续每个独立 UI 阶段都要更新本段对应的当前事实和 `docs/ai/WORKLOG.md`。

## 2026-08-19 UI-244 当前事实：首页风险明细与最近任务有限视口

- 首页最近任务外层和列表不再设置固定最小高度，数据较少时按实际行数收缩；虚拟化、真实任务 Binding 和页面滚动仍保留。
- 首页风险内容只保留一套可见摘要/明细结构；重复预览和重复操作行隐藏，底部操作按钮位于风险视口之外。
- `OverviewRiskViewport` 在紧凑堆叠布局上限为 520 DIP，在侧栏布局上限为 420 DIP；`OverviewProtectionItemsScrollViewer` 在紧凑布局上限为 420 DIP，在侧栏布局上限为 340 DIP。两者都使用项目现有 `GscPageScrollViewer`，超出内容只在对应内部区域滚动。
- 2026-08-19 在可识别的 Playnite 生产宿主中重新展开首页风险明细，确认明细卡片和“查看”操作完整可见，底部保护操作未被内部滚动区域截断。本事实只覆盖首页紧凑布局，不能替代其他页面的宿主对照验收。

## 2026-08-19 UI-240 当前事实：风险栏有限视口与维护 Inspector 按选中状态出现

- 首页“风险与提醒”使用外层 `OverviewRiskViewport` 的 `330 DIP` 有限视口；列表超出后在自身区域使用项目 `GscPageScrollViewer` 滚动，不能让 Dashboard 根页面无限增高。展开的保护明细仍是独立 `190 DIP` 视口。
- 维护中心发现问题/审计列表无选中项时释放右侧 Inspector，表格跨满可用宽度；只有用户点击行后才显示详情 Inspector。`DashboardViewModel` 刷新时仅恢复原先仍存在的选中项，不自动选择第一条。
- 2026-08-19 已在实际 Playnite 生产版重新安装并核对：首页风险栏保持有限高度；维护中心先显示全宽表格，点击问题后显示右侧详情。该次证据来自可识别的 Playnite 宿主窗口，不能与 Preview 入口或 RenderHarness 混淆。
- 同次一键 Release 验证：Core 59/59、Worker 191/191、Playnite 249 通过/61 跳过/0 失败；source/WPF 校验和安装版本核对均通过。

## 2026-08-19 UI-227 当前事实：媒体 Tab 条与生产按钮文本由共享样式控制

- `GscRedesignWorkspaceTabControl` 的外层 Tab 条必须使用 `TemplateBinding Background`，不能把 `GscControlFillBrush` 写死在模板中；媒体中心通过 `MediaTabControl.Background=GscGlassFillBrush` 使用玻璃材质，其他工作区继续沿用各自样式。
- `GscWpfUiButtonTextTemplate` 是生产文本按钮的统一边界：`TextAlignment=Center`、`NoWrap`、`CharacterEllipsis`。新增生产操作按钮优先使用已有 `GscWpfUi*Button` 样式，不要重新创建不受边界约束的局部 Button 模板。
- 这只证明源码与 RenderHarness 的布局契约；必须在 Playnite 生产 `GameSaveCenter` 入口重新安装并人工查看媒体中心，不能把 Preview 入口或离屏截图当成宿主验收。
- 2026-08-19 已完成生产扩展落盘安装验证（目标目录中的 `extension.yaml` 与 DLL 为 `0.6.70`），但本机屏幕控制两次返回了 Codex/其他窗口而非 Playnite 内容，自动化树为 `EmptyWindowAutomationPeer`；后续不得把该次结果写成 Playnite 视觉通过，需改用能确认窗口内容的宿主截图路径。

## 2026-08-19 UI-226 当前事实：维护表格需按工作区宽度压缩固定列

- 维护诊断和异常审计页面在右侧 Inspector 可见时，1040 DIP 工作区的主表格只有约 660 DIP，不能直接沿用宽屏的游戏/标题/详情/动作最小宽度。
- `MaintenanceView.ApplyFindingsColumnLayout` 现在在 1180 DIP 以下压缩非必要固定列，保留详情和建议处理的弹性列；1366 及以上恢复更宽的 Demo 比例。长文本单元格统一字符省略并提供 Tooltip。
- 这只是视口布局策略，不改变真实 Findings、SelectedFinding、Inspector 或命令；维护表格仍使用项目现有 DataGrid、虚拟化和滚动条。
- 当前验证：RenderHarness Release `render-qa OK`，source/WPF 校验和 `git diff --check` 通过。直接 `dotnet build --no-restore` 的 MSB4276 是本机 SDK 9.0.302 缺少 Workload locator 的环境限制，后续优先使用仓库脚本的禁用 Workload resolver 构建路径或补齐 SDK 环境。

## 2026-08-18 当前构建事实：可选目录未配置不应阻断健康检查

- `IntegrityCheckService.CheckDirectory` 对空的可选目录按未启用处理；只有用户配置了路径后，该路径不存在或不可写才报告 Warning。关键目录的空路径仍按原有关键错误语义处理。
- 一键安装遇到 `Healthy` 变 `Warning` 或 `Skipped` 变 `Warning` 时，先检查是否把可选 `GameToolsDirectory`/`DownloadDirectory` 当成缺陷；不要通过修改测试期望值掩盖默认安装语义。
- `scripts/build.ps1 -OutputRoot` 的隔离目录适合编译/Core/Worker 验证，但当前 Playnite 源码测试会从测试运行目录向上定位仓库；Playnite 测试必须在仓库原目录运行，不能把隔离目录失败写成代码回归。

## 2026-08-18 UI-223 当前事实：首页状态语义与真实宿主复核

- 生产首页最近任务的进度条不是所有任务的通用装饰：只有 `执行中`、`等待中`、`等待确认` 显示进度；成功、失败、已取消项只显示状态和时间，避免偏离 Demo 语义。
- 首页最近任务状态徽标、全局活动类型/结果徽标、风险提醒徽标统一使用固定边界内的水平/垂直居中和不换行；风险徽标保持足够最小宽度，不使用省略号截断“风险”等状态文字。
- 生产首页 v0.6.70.0 已重新安装到 Playnite 并实际打开复核；确认侧栏“设置”、最近任务状态布局、全局活动“维护/信息”徽标、风险徽标均来自真实生产宿主。电脑控制已在复核后释放。
- 媒体虚拟化缩略图使用 96px 有界解码宽度；这是性能约束，不要为了视觉放大恢复到 240px 的逐卡解码。
- 当前验证基线：source 校验通过，隔离 Release 构建通过，包生成/安装通过，WPF 校验 0 error；Worker 仍有 2 个集成测试失败，Render QA 有 2 个媒体 resize 恢复用例待修，不能写成全量验证通过。
- 真实宿主本阶段只对首页重点区域做了截图复核；其他页面仍需按页面逐一进入 Playnite 对照 Demo，最终交付必须区分“实际宿主复核”和“RenderHarness/源码验证”。

## 2026-08-18 UI-225 当前事实：首页风险列表必须使用有限视口

- 首页“风险与提醒”中，`AttentionFindings` 和展开的 `RecentProtection.Items` 都不能直接让右栏 `StackPanel` 无限测量；页面使用两个独立的 190 DIP `ScrollViewer`，分别命名为 `OverviewAttentionScrollViewer` 和 `OverviewProtectionItemsScrollViewer`。
- 两个内部滚动面均引用生产 `GscPageScrollViewer`，竖向 `Auto`、横向 `Disabled`；风险卡标题、恢复提示、摘要和“打开维护中心”等操作不在内部列表视口内，始终可见。
- `DashboardViewModel` 仍把首页风险原因限制为前 4 项，`RecentProtectionSummary` 仍把保护明细限制为前 6 项；XAML 的有限视口是防止未来数据契约放宽或展开内容增长时撑高首页的第二道布局保护，不改变业务计数和完整详情入口。
- 本阶段 RenderHarness 构建、双主题多尺寸及 resize 通过；定向 Playnite 测试运行器再次无输出挂起，不能把该次测试写成通过。真实 Playnite 嵌入像素仍需单独人工验收。

## 2026-08-18 UI-224 当前事实：首页徽标模板与 Demo 行密度

- `ChipBase` 模板必须把 `HorizontalContentAlignment`/`VerticalContentAlignment` 传给内部 `ContentPresenter`；仅给外层控件设置对齐属性不够。
- 首页最近任务的进度条只属于执行中、等待中和等待确认项；成功/失败/取消项使用独立状态和时间列。全局活动的类型/结果徽标与风险徽标使用共享 Chip，分别保持 64 DIP 最小宽度与 11 DIP 风险字号，避免“信息/成功”偏移和“风险”裁切。
- 本轮真实 Playnite 只复核首页上述区域；0.6.70 已安装成功且控制已释放。Media resize 两项 RenderHarness 失败、旧 Playnite UI 结构测试失败仍是未清理的验证债务，不得宣称全量 UI 完成。

## 2026-08-17 UI-222 当前事实：生产入口与 Preview 必须区分

- Playnite 当前可能同时加载两个独立扩展：`GameSaveCenter Preview`（0.6.71，Demo/预览）和生产 `GameSaveCenter`（0.6.70，`AcrylicProductionShellView`）。验证生产 UI 时必须从侧栏的 `GameSaveCenter` 进入，不能把 Preview 入口的画面当作生产页面。
- 生产 `DashboardView` 的可见层是 `AcrylicProductionShellView`，其 `PageHost` 承载真实 Overview/Save/Trainer/Media/Task/Maintenance 页面；旧 `DashboardDemoShell` 保留字段但必须保持 `Collapsed`。
- 生产头部标识使用“生产版”，不要恢复 Demo 的“外观预览”文字；Demo 的主题色只进入生产令牌，不迁移 Demo 色板控件。
- Real Host 审计在 150% DPI 下必须把 `TransformToAncestor` 的设备像素坐标规范化到根元素 DIP，并按 `GetRenderScale(window)` 保存受控窗口截图；否则会产生右侧按钮假溢出和截图被裁剪的假象。
- 当前验证基线：source/WPF 校验无 error、Release 无 warning/error、Playnite 303/303、RenderHarness `render-qa OK`；嵌入式 Dashboard 仍需用户真实点击 Playnite 侧栏才能取得像素真值。

## 2026-08-17 UI-221 AcrylicFork 整页视觉迁移收口

- 权威参考仍为 `D:\workplace\github\GameSaveCenter.AcrylicFork` @ `b09cba6`；本轮确认生产 7 页骨架已与其一致，并补齐 Media/Trainer/Save/Maintenance 分段导航右侧说明与真实计数/状态：待归类数、已绑定工具数、当前规则校验状态、安全模式/云端状态。
- 评估过独立 `AcrylicParity.xaml` 别名层，但独立字典的 `BasedOn` 无法解析父级合并字典里的 Gsc 样式，最终未引入；页面继续直接引用生产 Gsc 共享样式，避免解析风险。不要为了“Lab 键”重新创建该字典。
- 继续保留生产滚动条、圆角表头、DataGrid/ListBox 虚拟化和真实绑定；demo 右上角色板、演示数据和样例滚动条未迁移。
- 验证基线：source/XAML 通过；WPF UI 校验 0 errors（16 条既有 warning）；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 304/304；render-qa OK（7 页 × Light/Dark × 1040/1100/1366/2560 + resize）。
- 真机：0.6.70 安装成功并加载，`extensions.log` 无新增 XamlParseException；real-host-audit 捕获 EmbeddedSettings/Controlled，`EmbeddedDashboardCaptured=false`（自动化无法点击侧栏），不得冒充完整宿主像素验收。

## 2026-08-16 UI-220 UiLab 几何对齐与虚拟媒体回滚修复

- 上一轮迁移留下的关键差异已收口：`GscRedesignWorkspaceTabItem` 的页面内容对齐改为 `Stretch`，模板只把分段标题居中；否则 WPF 会把真实表格/卡片内容按标题对齐方式压缩到中间，造成用户截图中的大面积空白和错位。
- 维护中心的诊断二级导航已按 UiLab 改为 `GscRedesignSegmented` + 命名面板宿主；“问题列表”和“诊断概览”都可见，原有诊断表格、详情 Inspector、绑定和命令保留。媒体、存档、维护页的本地 TabItem 样式同步改为 Stretch 内容。
- `VirtualizingWrapPanel` 不再把首次无限高度测量解释为零视口；现在优先使用 ScrollViewer/上一次视口高度，并在生成器刷新越界时排队重新测量，覆盖媒体列表切换、滚动到底后回顶的 WPF 时序。
- 首页 Hero/当前游戏列恢复 UiLab 的 `1.35*:1` 比例，744 DIP 紧凑视口仍自动堆叠并保持三枚操作按钮可用。RenderHarness 新增媒体 WrapPanel 滚动回顶探针，并支持 segmented 页面和维护二级 segmented 导航。
- 当前验证：source 校验通过；WPF UI 校验 0 errors（仅既有布局/主题资源提示）；Playnite Release 构建 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 303/303；`render-qa OK`，双主题、多尺寸、resize、维护二级导航和媒体滚动回顶均通过。离屏截图不能替代 Playnite 宿主像素验收；当前 Computer Use 仍无法稳定激活 Playnite，不能把本轮写成完整真机视觉验收。

## 2026-08-16 UI-219 UiLab 分段页面骨架直迁当前事实

- 当前媒体、存档、修改器、维护四个生产页已按 UiLab 的顶层页面骨架运行：`Grid` 的 Auto 导航行 + `GscRedesignSegmented` + `Grid` 面板宿主；不要再把“共用配色/卡片”当成完成迁移，也不要恢复外层 `TabControl` 作为这四页的主导航。维护诊断和审计内部嵌套页签是 UiLab 本身存在的层级，继续保留。
- 生产面板名称：Media=`MediaInboxScrollSurface`/`MediaCurrentScrollSurface`/`MediaSourcesPanel`；Save=`SaveHistoryPanel`/`SaveCandidatePanel`/`SavePolicyPanel`/`SaveComparePanel`；Trainer=`InstalledToolsLayout`/`TrainerImportPanel`/`TrainerCatalogPanel`/`TrainerReleasesPanel`；Maintenance=`MaintenanceDiagnosticsPanel`/`MaintenanceDevicePanel`/`MaintenanceRetentionPanel`/`MaintenanceAuditPanel`/`MaintenanceProcessPanel`。分段 `SelectionChanged` 在 `InitializeComponent` 未完成时必须容忍空字段。
- `Redesign.xaml` 的 `GscRedesignSegmented` 对齐 UiLab `LabSegmented` 的 `SegmentFillBrush`、选中项填充/描边、RadiusM=10 和 item 内边距；运行时浅色/深色主题在 `AdaptiveThemePalette` 同步这些资源。演示色板、窗口按钮、样例数据、样例滚动条不迁移。
- `VirtualizingWrapPanel` 现在对生成器插入位置做边界裁剪，遇到 WPF 生成器时序越界会清空当前生成子项并重新测量；不要改回无边界 `VisualCollection.Insert`，也不要以普通 `WrapPanel` 替换它。
- 当前自动基线：source/XAML 校验通过，Release 0 warning / 0 error，Core 59/59、Worker 191/191、Playnite 303/303；真实扩展版本为 `0.6.70.0`。启动日志未出现本轮新增崩溃，但 Computer Use 无法激活 Playnite（黑色捕获 + `EmptyWindowAutomationPeer`），所以真实页面像素和逐页切换仍是 `MANUAL QA REQUIRED`，不能写成已验收。

## 2026-08-16 UI-218 UiLab 页面骨架迁入生产

- 当前生产页面已按 `D:\workplace\github\GameSaveCenter.UiLab` 的工作区骨架收敛：Dashboard 只保留一套页面头部游戏上下文；`SelectedGameHeader`、`GameHeaderActions`、`RestoreSafetyBanner` 不得在页面外重新显示成重复的第二层。安全说明应放在备份策略/比较表面内。
- `SaveCenterView.xaml` 的策略页使用三栏 Demo 表面并保留真实 `SelectedGame.Policy`、模板、云端和命令绑定；比较指标使用可换行的固定最小宽度，兼顾用户指定的按钮/指标重叠例外。
- `TrainerCenterView.xaml` 的当前游戏工具栏和拖入导入区是两行；`TaskCenterView.xaml` 的搜索、状态、类型与游戏筛选是两行，`TaskGameFilterHost` 必须整体移动，不能只移动 ComboBox。
- 不迁移 UiLab 的演示色板、窗口控制、演示数据和样例滚动条；生产滚动条、DataGrid 虚拟化、键盘/绑定/命令和 Playnite 兼容性优先。窄宿主视口出现策略卡纵向堆叠属于响应式行为，不等于未迁移。
- 当前验证基线：source/XAML 校验通过，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 303/303；真实 Playnite 人工检查首页、备份中心、修改器中心、任务中心已加载，无重复全局安全横幅。自动审计仍保持 `EmbeddedDashboardCaptured=false`，人工截图不能冒充自动嵌入证据。

## 2026-08-16 UI-217 真实 Playnite 人工视觉复核

- `81fde54` Release 包在真实 Playnite 内加载成功；人工进入 GameSaveCenter 后确认首页的开放式头部、真实游戏上下文、单一六项指标带、工作区卡片和生产页脚均可见，演示色板/窗口按钮/样例滚动条没有进入生产。
- 真实任务中心 1303×673 宿主视口可见四项统计带、筛选区、圆角表头 DataGrid、真实状态胶囊和进度条；未选中任务时 Inspector 按 `SelectedTask` 为空隐藏，属于生产交互状态，不是页面布局缺失。
- 自动 `real-host-audit` 仍因 UIAutomation 找不到 Playnite 侧栏而记录 `EmbeddedDashboardCaptured=false`；人工截图只作为本轮观察，不得改写自动审计摘要或冒充完整宿主矩阵。

## 2026-08-16 UI-215 Task 统计栏与窄窗筛选迁移

- `TaskCenterView.xaml` 已把生产任务中心顶部四个独立指标卡改为 UiLab 同款的单一 `TaskSummaryBand`；`Tasks.Count`、`RunningTaskCount`、`RetryableTaskCount`、`CompletedTaskCount` 四个真实 OneWay 绑定保留，内部使用三条 `GscDividerBrush` 分隔线，不迁移 UiLab 的演示数据或滚动条。
- 指标按 UiLab 的“数值在上、标签在下”阅读节奏收敛到 26 DIP，保留任务队列、详情、筛选、DataGrid 表头/滚动条和虚拟化；任务摘要不再通过 `UniformGrid.Columns` 产生卡片换列。
- 修复窄宽度筛选迁移的残留标签错位：`TaskGameFilterHost` 将“游戏:”与真实下拉框作为一个控件组一起移入/移出“更多筛选”，避免 1040-DIP 工作区出现“游戏:”孤立在“类型:”前的拥挤布局。
- 当前自动验证：`validate-source.py`、XAML 检查、Release 0 warning/0 error、Playnite 303/303、RenderHarness 双主题/1040/1100/1366/1600/2560 及 resize 全部 `render-qa OK`；新证据在 `artifacts/ui-qa/task-summary-band-v2`。真实 Playnite Dashboard 仍未重新捕获，不能把离屏截图写成宿主视觉真值。
- 隔离安装构建曾暴露旧的响应式单元测试仍检查 `TaskGameFilterComboBox` 直接挂在筛选容器；测试已同步到新的 `TaskGameFilterHost` 结构，当前 Playnite 测试恢复 303/303。后续移动筛选控件时必须同时更新父容器契约测试。

## 2026-08-16 UI-214 Overview 统计栏与首页光晕收口

- `OverviewView.xaml` 已把原先六个独立圆角统计卡改为 UiLab 同款的单一 `OverviewStatBand`：六个真实 `Snapshot` 指标保留在同一表面内，使用五条 `GscDividerBrush` 分隔线；两个真实比例条、空库折叠保护和 OneWay 绑定均未改变。
- 删除统计卡专用的悬停位移动画，避免把非交互指标做成堆叠/错位卡片；Dashboard 的 `UiAnimationsEnabled` 合约仍保留给其它工作区。
- `DashboardView.xaml` 的根层环境光从三色椭圆收敛为单一 `GscAmbientAccentBrush` 磨玻璃晕影，保留首页喜欢的背景氛围；生产 ScrollBar、页面滚动、虚拟化和真实命令/绑定未迁移 UiLab 演示实现。
- 当前自动验证：`validate-source.py`、XAML 检查、WPF UI 校验（0 errors）、Release 0 warning/0 error、Playnite 303/303、RenderHarness 双主题/1040/1100/1366/1600/2560 及 resize 全部 `render-qa OK`。新证据在 `artifacts/ui-qa/overview-single-band-v1`。
- 本阶段未把离屏截图写成真实 Playnite 嵌入真值；真实宿主 Dashboard 仍受 UIAutomation 无法定位侧栏入口限制，必须继续保留 `EmbeddedDashboardCaptured=false` 的诚实边界。

## 2026-08-16 UI-213 真实宿主审计当前事实

- 当前提交 `420483f` 的 Release 包已通过 `scripts/real-host-audit.ps1` 安装并启动 Playnite；人工通过 Computer Use 进入真实 Playnite，确认 GameSaveCenter 实例和 Settings 宿主窗口可见，未出现立即的 XAML 解析崩溃。
- `artifacts/ui-host-audit/summary.json` 明确记录：`EmbeddedSettingsCaptured=true`、`EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`。Settings 的真实宿主截图/滚动证据已生成，但 Controlled Dashboard 证据不是生产嵌入像素。
- 因自动 UIAutomation 仍找不到 Playnite 左侧 GameSaveCenter 入口，本轮不能把 Media 缩略图网格写成“真实宿主已验收”；Media 仍以 `artifacts/ui-qa/media-grid-migration-v3` 的离屏多尺寸/主题结果作为自动证据，真实大媒体库、DPI、键盘和连续缩放继续是 `MANUAL QA REQUIRED`。

## 2026-08-16 UI-212 Media 网格当前事实

- `MediaCenterView.xaml` 的当前媒体 `MediaGrid` 已按 UiLab 的 164×142 DIP 卡片节奏改为 `ui:VirtualizingWrapPanel`，缩略图高度 96 DIP；卡片包含真实 `ArchivePath` 异步缩略图、录像/收藏标识、文件名、拍摄时间和云端状态。
- `VirtualizingWrapPanel` 位于 `src/GameSaveCenter.Playnite/Controls/VirtualizingWrapPanel.cs`，实现 `IScrollInfo`，只生成当前视口附近的容器，兼容 Recycling generator；不要替换成普通 `WrapPanel`，也不要迁移 UiLab 的滚动条模板。
- `MediaGrid` 仍是生产 `ListBox`，保留 `ItemsSource={Binding MediaView}`、Extended selection、`ScrollViewer.CanContentScroll=True`、生产滚动条、Inspector 抽屉和真实批量/编辑命令；窄窗仍使用 `MediaCompactDetailsButton`。
- 首次挂载时生成器可能尚未就绪，面板已对该 WPF 测量时序做空保护；Reset、窄宽切换、Light/Dark 离屏渲染均已通过。
- UI-212 自动验证：`validate-source.py`、XAML 检查、Release 0 warning/0 error、Playnite 303/303、RenderHarness 全量 `render-qa OK`；真实 Playnite 大媒体库/DPI/主题/键盘/连续缩放仍需人工验收。

## 2026-08-16 UI-208 Overview 全局活动当前事实

- `OverviewView.xaml` 的全局活动已按 UiLab 业务列表迁移：类型胶囊 → 对象/事件两行 → 结果胶囊 → 时间，不再额外显示 DataGrid 式表头，也不使用图标列。
- 生产仍绑定真实 `Activities`，保留 `ItemsControl` Recycling、`KindDisplay`/`ResultDisplay`、结果语义色和 `OverviewStackScrollSurface` 页面滚动；demo 的滚动条、演示数据和右上角色板没有迁移。
- 窄窗口只缩小 `ActivityKindColumn`、`ActivityTimeColumn` 并降低摘要最小宽度；不要重新添加内部滚动或把时间/结果挤进摘要列。
- UI-208 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，v6/v6.2 Overview 宽/窄截图通过；真实 Playnite 宿主主题/DPI/键盘/连续缩放仍需人工验收。

## 2026-08-16 UI-209 表头共享模板当前事实

- 生产 `DataGridColumnHeader` 已有独立圆角模板；`DataGridColumnHeadersPresenter` 现在必须保持 `Background=Transparent`，否则连续底色会吞掉列头之间的圆角间隙并恢复成硬矩形表头。
- `GscTableHeaderBrush`、`GscTableDividerBrush`、表头高度/内边距、排序 glyph 和页内 DataGrid 虚拟化仍由生产共享模板控制；不要为对齐 UiLab 而迁移 UiLab 滚动条或关闭 `VirtualizingPanel.ScrollUnit=Item`。
- UI-209 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，v6/v6.2 表格与 Overview 宽/窄离屏截图通过；真实 Playnite 宿主主题/DPI 仍需人工验收。

## 2026-08-16 UI-210 Media 当前事实

- `MediaCenterView.xaml` 顶部统计现在是一个 `GscRedesignSectionCard` 内的四段统计带，`MediaSummaryPanel` 仍是 `UniformGrid`，真实统计绑定未改变；默认 `TabControl.SelectedIndex=1` 展示当前游戏媒体。
- Media 当前媒体仍使用生产 `ListBox + VirtualizingStackPanel`，不是 UiLab 的非虚拟化 `WrapPanel` 缩略图网格；这是为大媒体库和现有滚动性能保留的生产适配边界，不要为了像素复制而关闭虚拟化或迁移 demo 滚动条。
- `ApplyResponsiveLayout` 在 700-720 DIP 常规窗口保持 236 DIP 表格下限；宽屏恢复会把已选择媒体的 Inspector 重新设为 Visible，窄屏继续由 `MediaCompactDetailsButton` 控制 Inspector。
- UI-210 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，Media Light/Dark 多尺寸和 resize transition 通过；全量 render-qa 仅剩 Save 候选表历史窄视口门禁。

## 2026-08-16 UI-211 Save 当前事实

- `SaveCenterView.ApplyResponsiveLayout` 的表格高度公式为 `Math.Max(180d, Math.Min(252d, height - 464d))`；常规 700-720 DIP 窗口的 SaveCandidateGrid/SaveHistoryGrid 不得回到 236 DIP 以下。
- Save 历史/候选 DataGrid 仍使用生产共享表头、Item scrolling、行/列虚拟化和现有 Inspector 抽屉；只修复视口下限，没有替换滚动条或绑定。
- UI-211 自动验证：Playnite 303/303，生产 Release 0 warning/0 error，全量 RenderHarness `render-qa OK`（Light/Dark、7 页面、多尺寸、resize transition）。

## 2026-08-16 UI-205-ACRYLIC-PARITY 当前事实

- 权威视觉来源是 `D:\workplace\github\GameSaveCenter.AcrylicFork`（当前参考提交 `b09cba6`），不是 `GameSaveCenter.UiLab`。生产已迁移其页面层级、颜色/表面层级、圆角尺度、按钮/标题比例、Dashboard 开放式页面头部和 Settings 分类栏；不要回头修复 AcrylicFork 样板自身的布局 bug。
- 明确排除 AcrylicFork 演示数据、右上角颜色/主题按钮和样例滚动条。生产继续使用真实数据/命令/绑定/虚拟化和现有带 Track 绑定的滚动条。
- AcrylicFork 顶部色板作为主题参考：靛蓝 `#7C8CF8`、天蓝 `#4FA3F0`、青碧 `#35B8C9`、薄荷 `#4CC08A`、紫罗兰 `#A07BF5`、琥珀 `#E8973C`、玫瑰 `#E56E8C`。生产默认使用靛蓝基线，同时继续尊重 Playnite/强制主题设置，不增加演示色板按钮。
- 共享几何：Shell 20、Card 16、Control 10；Settings 实际内容宽度 ≥700 DIP 使用左侧分类栏，<700 DIP 顶部横向分类，<620 DIP 收紧窄屏标题和字段；这三个断点与 `GameSaveCenterSettingsView.xaml.cs` 保持一致。
- Overview 的全局命令只在 Dashboard 页面头部显示；`OverviewHomeToolbar` 只有 `IsOnboardingPending=True` 时显示，不能重新改成普通状态下的重复命令卡。
- DataGrid 表头与 Button/Card 已按生产共享模板收敛圆角；不要把 AcrylicFork 的滚动条模板覆盖到生产，也不要给可选 `DataGridColumnHeader.Tag` 增加 `CornerRadius` 绑定，Playnite 生成 filler header 可能得到 `UnsetValue`。
- 安装器修订为 `DEV-INSTALL-008`：只停止当前生产扩展目录下的 Worker；其他扩展的 Worker 留在原处，路径不可读取时仍 fail-closed。一次点击安装已成功，生产 Worker 与 AcrylicFork 外来 Worker 可并存。
- 本阶段验证基线：Release 构建 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 302/302；`validate-source.py`、XAML 检查、render-qa（Light/Dark、7 页面、多个尺寸与 resize transition）全绿。
- 离屏截图不是真实 Playnite 嵌入式像素真值；本阶段已真实安装并启动 Playnite/生产 Worker，但没有把宿主窗口像素冒充为自动视觉证据，主题/DPI/键盘/连续缩放仍需人工确认。

## 2026-08-16 UI-205-REAL-HOST-MIGRATION-FIX 当前事实

- Playnite BAML 不应在生产 XAML 中直接写 `clr-namespace:GameSaveCenter.Contracts;assembly=GameSaveCenter.Contracts`：扩展程序集位于 Playnite 私有目录时，BAML resolver 可能从默认 AppDomain 解析失败，即使安装目录实际包含 Contracts.dll。需要使用生产程序集内的 `GameSaveCenter.Playnite.XamlValues` 包装属性；属性返回真实 Contracts enum object，因此不改变绑定/DataTrigger 语义。
- Dashboard 选中游戏标题栏必须在 Grid 重排后再次按 `TransformToAncestor` 的实际 X 坐标限制宽度；首次 measure 的旧 DesiredSize 会让 `SelectedGameHeaderLayout` 和按钮短暂超过页面右边界。`ApplicationIdle` 二次布局与审计的 ApplicationIdle 等待是配套约束，不能只修截图审计。
- `RealHostUiAuditService.CheckChildLayoutOverflow` 复用输出目录时，干净轮次必须删除旧 `CHILD_LAYOUT_OVERFLOW.json`；最终状态以当前 `overflow-classification.json` 为准，不能用旧门禁文件判断本轮。
- 本轮安装验证：`extension.yaml 0.6.70`、生产 DLL `0.6.70.0`，日志确认 `GameSaveCenter 0.6.70.0 loaded`，没有新的 `XamlParseException`/Contracts 缺失；外来 `GameSaveCenterPreview` Worker 不得结束。
- 本轮自动基线：Core 59/59、Worker 191/191、Playnite 303/303，Release 0 warning/0 error；受控矩阵的最终 `RealFixedLayoutOverflow=[]`。由于 UIAutomation 未定位到 Playnite 侧栏，`EmbeddedDashboardCaptured=false` 仍是诚实门禁，受控窗口不能替代真实嵌入像素。

## 2026-08-16 UI-REAL-HOST-AUDIT-BLOCKERS-FIX 当前事实

- Audit CommitSha 必须可追踪：脚本设置 `GSC_UI_AUDIT_COMMIT`，unknown 触发 `AUDIT_SOURCE_REVISION_MISSING` HIGH。
- Embedded 判定用 `IsGenuinelyEmbeddedDashboard`（IsLoaded + PresentationSource + Window 非 fallback）；headless 无人点击时必须诚实 false + HIGH gate。
- `SafeFileName` 不能再用 `string.Join("-", chars)`；已修复为仅替换非法字符并折叠连续 `-`。
- Overflow gate 必须分类：fixed/scroll/decorative；ScrollViewer 内容与装饰性越界不误报。
- Resize 后必须等待 DataBind/Loaded/Render/Idle 且连续两次几何差 ≤0.5 DIP 才截图（最多 3 pass）。
- Manifest 按 `Scope=Dashboard|Settings` 隔离；内部滚动器（DG_ScrollViewer/PART_ContentHost/TextBox/ComboBox）默认排除。
- 基线：Playnite 302/302、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-16 UI-HOST-AUDIT-TRUTHFULNESS-FIX 当前事实

- Real Host Audit 的 origin 必须显式：`DashboardView.AuditHostKind`（默认 EmbeddedPlaynite，fallback 专用窗口设 ControlledAuditWindow）；不要用 `auditDashboardWindow==null` 推断。
- Sidebar View 不能调用 `Activated`；真实 embedded Dashboard 只能由用户在 Playnite 点击侧栏后经 `Opened` 加载，`OnLoaded` 触发捕获。无人点击时输出必须写 `EmbeddedDashboardCaptured=false`。
- `AuditCaptureSession` 隔离三类 manifest：EmbeddedDashboard / ControlledDashboard / Settings；settings manifest 不允许混入 Dashboard entries。
- DataGrid（`CanContentScroll=true`/`ScrollUnit=Item`）是逻辑 item 单位，禁止复用像素 stitch；`DG_ScrollViewer`/`PART_ContentHost` 默认排除。
- `summary.json` 是硬门禁：`EmbeddedDashboardCaptured` / `EmbeddedSettingsCaptured` / `ControlledDashboardCaptured` / `VisualSourceOfTruthAvailable`。
- 基线：Playnite 294/294、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-16 UI-REAL-HOST-CAPTURE-COMPLETENESS-FIX 当前事实

- Real Host Audit 输出语义已重构为三类，不再用“最大 ScrollViewer”冒充整页：
  - `embedded-current/viewport/`：真实 Playnite 嵌入 Dashboard 的当前视口（production visual truth；本次 headless 会话不可用时会如实标注）。
  - `controlled/<profile>/<theme>/viewport/`：无边框审计窗口，profile 即 client size，Dashboard Stretch。
  - `scroll-surfaces/<route>__<name>.png`：每个 meaningful ScrollViewer 的完整 extent。
- 关键实现：无边框窗口（client == outer）、Dashboard `ClearValue(Width/Height)` + Stretch、`SaveViewport` 校验 `Actual*DpiScale` 输出尺寸、`CAPTURE_VIEWPORT_CLIPPED`/`CAPTURE_PROFILE_SIZE_MISMATCH` gates、`capture-manifest.json`。
- 元数据：`Mode` 只能是 `embedded-current` 或 `controlled-host-window`；`CaptureOrigin`、`DedicatedAuditWindowUsed`、`ProfileSizeApplied`、`ThemeOverrideApplied` 必填；PlayniteDesktopVersion 取宿主 exe 文件版本，失败写 `unknown`。
- 基线：Playnite 287/287、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-15 LUDUSAVI-DIAGNOSTICS-FIX 当前事实

- 备份失败常见根因之一是 Ludusavi 联网更新 manifest 超时（`raw.githubusercontent.com/.../manifest.yaml`）；这不是存档路径/ZIP 写入问题，网络恢复后重试即成功。
- 插件侧已修复三项放大问题：外部进程 stdout/stderr 按 UTF-8 解码；`LudusaviCommandResult.RawOutput` 在失败时保留原始输出；剪贴板复制带重试与失败降级（`CopyTextWithRetry`）。
- 相关代码：`ExternalProcessRunner.cs`、`LudusaviClient.cs`、`DashboardViewModel.CopyTextWithRetry`。
- 基线：Worker 191/191、Playnite 281/281；Release 0 warning/0 error。

## 2026-08-15 UI-REAL-HOST-AUDIT-NESTED-TABS-THEMES 当前事实

- 真机审计现在按 5 档窗口尺寸 × `Light`/`Dark` 双主题捕获；每个尺寸/主题目录含 Dashboard、6 个工作区、全部顶层 Tab 与嵌套 Tab（如“异常与审计”→“审计记录”）。
- 嵌套 Tab 捕获要点：选择父 Tab 后等待 `ApplicationIdle`，分别从 TabItem 视觉树和 `tab.Content` 两个路径找子 TabControl，再递归；单纯视觉树遍历会漏掉部分嵌套页。
- 整页截图统一渲染 Dashboard/Settings 根元素（含侧栏和外壳），高度按内容最大滚动范围撑高（maximized 下工作区 1707×1232、Overview 1707×1717）；不要改成只渲染内部滚动器内容，否则会缺左右外壳。
- 设置页兜底窗口必须注入 `GameSaveCenterSettings` 作为 DataContext，否则主题/玻璃设置是默认值。
- 产物：`artifacts/ui-host-audit/screenshots/<size>/<light|dark>/`，zip `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 基线：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。真实第三方主题/Playnite 窗口内观感仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-AUDIT-MULTI-SIZE 当前事实

- 真机审计按 5 档窗口尺寸捕获：`maximized`（WorkArea 1707×912 DIP）、`1600x1000`、`1366x768`、`1280x720`、`1024x768`；每档包含 Dashboard、6 个工作区、全部内层 Tab、窗口截图与 Settings 5 分类。
- Playnite 进程 DPI-unaware：窗口尺寸必须用 `SystemParameters.WorkArea`，不能用 `GetSystemMetrics`（会返回 39×24 虚拟值导致窗口 640×480）；窗口需 `SizeToContent.Manual` 并显式设置视图宽高。
- 多尺寸扫描不启用全页滚动拼接（避免大表格十几分钟卡死）；内容按逻辑分辨率输出防止 OOM。
- 产物：`artifacts/ui-host-audit/screenshots/<size>/` 与 `metadata-<size>.json`；zip 为 `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 清理规则已写入 AGENTS.md「文件清理规则」与 DEVELOPMENT_HANDOFF：每轮完成后删除旧 `dev-build`、`ui-audit-build`、`phase*`、`audit*`、旧 zip 与 `.tmp` 旧目录，只保留当前安装目录与审计证据。
- 基线：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。真实第三方主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-AUDIT-FULL-COVERAGE 当前事实

- 真实宿主审计已覆盖 Dashboard 全部页面/Tab 与 Settings 全部 5 个分类；`artifacts/ui-host-audit/` 是最新证据，zip 为 `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 无交互桌面（Playnite 主窗口不可见）时，审计通过专用窗口兜底：Dashboard 1440×900、Settings 同样 1440×900、左上锚定、ToolWindow 可关闭；不再出现越界不可关窗口。
- 设置页兜底的关键约定：输出根在 Dashboard 完成前缓存并传给 Settings 兜底，且窗口创建必须使用 Dashboard 的 UI Dispatcher（线程池 Dispatcher 不会显示窗口）。
- 设置分类 Header 为复杂 Grid，文件命名须从 Header 视觉树提取中文文本，不能用 `Header.ToString()`（会全部同名）。
- 默认 zip 被其他进程占用时审计会写 `GameSaveCenter-ui-host-audit-<时间戳>.zip`，不会中断 Settings 捕获。
- 基线：Release 0 warning/0 error；Playnite 281/281；`validate-source.py`、`check-xaml.ps1`、WPF UI 校验 0 errors。
- 真实第三方主题、连续缩放、用户实际 Playnite 窗口尺寸仍为 `MANUAL QA REQUIRED`；无交互桌面证据不能冒充真实窗口像素。

## 2026-08-15 UI-REAL-HOST-PARITY-CLOSURE 当前事实

- 来源：`GameSaveCenter_RealHost_UI_Parity_Audit_Prompt.zip`，计划 `docs/ai/REAL_HOST_UI_PARITY_CLOSURE_PLAN.md`，报告 `docs/ai/REAL_HOST_UI_PARITY_CLOSURE_REPORT.md`。
- Audit 定位：Tier A `capture-ui-audit.ps1` 是 Offscreen Regression Audit（几何/滚动/虚拟化/fidelity 门禁，不是视觉真值）；Tier B `real-host-audit.ps1` 才是真实 Playnite 视觉事实来源。
- 插件内 `RealHostUiAuditService`：`GSC_REAL_HOST_AUDIT` 或 `%LOCALAPPDATA%\GameSaveCenter\real-host-audit.request` 触发；从真实 Dashboard/Settings 捕获截图、visual tree、resource snapshot、style fingerprint、真实 DPI/bounds；不触发备份/恢复/删除等业务命令。
- `UiDiagnosticsExporters`：resource snapshot / style fingerprint / visual tree / PNG 导出；`AdaptiveThemePaletteContrastGuard`：palette 对比守卫。
- 本机证据：`artifacts/ui-host-audit/` + `artifacts/GameSaveCenter-ui-host-audit.zip`；DPI 1.5，Dashboard 1264×868，runtime palette（accent #0379FF、Glass alpha 0.78-0.94 等）。
- 离屏更漂亮的根因：离屏用 DesignTokens fallback palette；真实宿主用 AdaptiveThemePaletteFactory runtime palette + 真实 DPI/host bounds/data；当前无证据显示 surface hierarchy 被压平，故未改 palette。
- 协定：AGENTS.md / DEVELOPMENT_HANDOFF 已写明每轮完成后 Agent 自己 commit 并 push。
- 基线：Playnite `281/281`；render-qa 全绿；Offscreen UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 真实 125-200% DPI、第三方主题、连续缩放与 Settings paired evidence 仍需人工/下次脚本运行确认。

## 2026-08-15 UI-AUDIT11-RESIDUAL-CLOSURE 当前事实

- 来源：`GameSaveCenter_Audit11_Residual_UI_Closure_Prompt.zip`，计划 `docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_PLAN.md`，报告 `docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_REPORT.md`。
- SaveHistory 大小列使用 `SaveSizeValue`（`TextTrimming=None`，`Tag=SaveHistorySize`），列宽 116 DIP；narrow 状态列保留。
- Maintenance Device Inspector 在 Compact/Narrow 默认收起，独立“查看设备详情 ›”按钮，展开 viewport >= 180 DIP，表格 MinHeight 150（header + 2 行）。
- Audit fidelity 新增 `SHORT_SEMANTIC_VALUE_TRIMMING` 与 `INTERACTIVE_INSPECTOR_USABILITY`，均为 MEDIUM 且触发即失败。
- Settings 分类滚动目标整数取整；`ACTIVE_TAB_VISIBILITY` 保持 0。
- 基线：Playnite `276/276`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 审计 ZIP：`artifacts/audit11-final/GameSaveCenter-ui-audit.zip`（标准路径被外部进程锁定）。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FIDELITY-CLOSURE-AUDIT10 当前事实

- 来源：`GameSaveCenter_UI_Fidelity_Closure_Audit10_Prompt.zip`，计划 `docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_PLAN.md`，报告 `docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_REPORT.md`。
- Maintenance 不再有局部 implicit `DataGridColumnHeader` style；真实列统一走 `GscDataGridColumnHeaderStyle`，中间表头全部渲染。
- Media 搜索框为 `Auto/*(MinWidth=160)/Auto/150` Grid；narrow 内容宽约 390 DIP。
- Settings 选中分类在 SelectionChanged/ApplyResponsiveLayout 后同步 scroll-into-view（BringIntoView + 增量 delta 收敛）。
- Save History narrow 收起备注列保留状态列；完整备注在版本详情 Inspector。
- Audit fidelity 门禁：`HEADER_CONTENT_FIDELITY` / `ACTIVE_TAB_VISIBILITY` / `CONTROL_USABILITY_GEOMETRY` / `ESSENTIAL_COLUMN_VISIBILITY` 均为 MEDIUM 且触发即失败。
- 基线：Playnite `273/273`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 fidelity。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-POST-TYPOGRAPHY-GEOMETRY-CLOSURE 当前事实

- 来源：`GameSaveCenter_PostTypography_Geometry_Audit_Fix_Prompt.zip`，计划 `docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_PLAN.md`，报告 `docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_REPORT.md`。
- Maintenance 诊断与异常审计的“等级”列统一使用 `GscSeverityColumnWidth`（DataGridLength 92 DIP），不再有 72 DIP 挤压。
- UI Audit Text-Fit：`UiLayoutAnalyzer` 用 `FormattedText` 无约束宽度对比 `ActualWidth`；`TEXT_FIT`=MEDIUM 且 `UiAuditRunner` 遇任何 TEXT_FIT 返回失败码；wrap/ellipsis 文本不误报。
- visual-tree：exporter 不能用 `IsVisible`（离屏 host 无 PresentationSource 恒 false），改用 `Visibility == Visible`；当前 175 个 JSON 非空。
- 基线：Playnite `268/268`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 TEXT-FIT。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-TYPOGRAPHY-RESPONSIVE-CLOSURE 当前事实

- 来源：`GameSaveCenter_Final_UI_Typography_Prompt.zip`，计划 `docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_PLAN.md`，报告 `docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_REPORT.md`。
- 字体 token：`GscUiFontFamily = Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI`，`GscCodeFontFamily = Consolas, Microsoft YaHei UI`；普通 UI 无硬编码 UI 字体；图标字体 `Segoe MDL2 Assets` 与代码字体 `Consolas` 保留；通用按钮默认 Medium，Primary 保留 SemiBold。
- Settings Compact/Narrow：长说明/副标题/保存提示按断点隐藏，header 最小高度 56-76 DIP；render-qa 760×560 正文 viewport 300 DIP、880×560 285 DIP。
- Save Compare Narrow：主比较区 MinHeight 240、MaxHeight `max(300, height*0.52)`；1040×700 主比较 viewport 234 DIP，保留策略 246 DIP。
- Compact Inspector：Save/Trainer/Media/Task 五个详情按钮均为表格下方独立 `Grid.Row=1` 操作行，无 overlay。
- Media 待归类底栏与表格内容左边缘统一 12 DIP padding。
- 基线：Playnite `266/266`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-POLISH-V7.1 当前事实

- 来源：`GameSaveCenter_UI_Final_Polish_Pack_v7_1.zip`，计划 `docs/ai/UI_FINAL_POLISH_PLAN_V7_1.md`，报告 `docs/ai/UI_FINAL_POLISH_REPORT_V7_1.md`。
- 首页活动行五列：Icon/Scope/Message(*)/MetaChip/Time；chip 独立横向列组，Time 右留白 20 DIP。
- `POSSIBLE_CLIPPING=0`；Audit 消息含元素名/父元素/文本，且按 Margin 修正误报。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`（Commit `f6f17a8`）；提交 `702b0d5`、`f6f17a8`。

## 2026-08-15 UI-FINAL-CLOSURE-V7 当前事实

- 来源：`GameSaveCenter_UI_Final_Closure_Pack_v7.zip`，计划 `docs/ai/UI_FINAL_CLOSURE_PLAN_V7.md`，报告 `docs/ai/UI_FINAL_CLOSURE_REPORT_V7.md`。
- Audit 工具已支持嵌套子路由与 expected/actual 主表断言；Settings 标题解析为真实分类名。
- 共享 `DataGridStarFill` 附加行为修复宽屏星号列：2K 六张表 ColumnFillRatio=1.00，MaintenanceProcess 目标游戏 1549 DIP；横向滚动 Disabled，Save/Task <1200 DIP 时 Inspector 收起。
- Task 根改为有限 Grid；Media Inbox/Current 取消 460 上限；Task/Media 主行 VerticalFillRatio=1.00。
- Maintenance 表头白块清零；Progress 模板补 `PART_Track` 并新增专用 track/fill token；单行 TextBox `PART_ContentHost` Stretch + Padding 收口。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`（Commit `90738b7`）；Progress probe：`artifacts/ui-qa/v7-progress/`。
- 提交：`5cd0226`、`58191d5`、`494b402`、`87d0553`、`7eaaacd`。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FEEDBACK-GLOBAL-ACTIVITY-CHIP-CENTER

- 首页“全局活动”的 Kind/Result chip 文字已强制水平/垂直/文本三向居中（`OverviewView.xaml` 宽窄两套共 4 个 TextBlock），并有 `UiLayoutRegressionTests` 回归断言锁定。
- 该修正属于 v6.2 之后的用户反馈补丁，提交 `d962b4d`；Playnite `263/263`、XAML/source 门禁与截图均通过。

## 2026-08-15 UI-TABLE-AND-CHIP-CLOSURE-V6.2 当前事实

- 来源：`GameSaveCenter_UI_Table_and_Chip_Fix_Pack_v6_2.zip`，计划 `docs/ai/UI_TABLE_AND_CHIP_CLOSURE_PLAN_V6_2.md`，报告 `docs/ai/UI_TABLE_AND_CHIP_CLOSURE_REPORT_V6_2.md`。
- Chip 已统一为圆角矩形：`GscRedesignContextPill`（CornerRadius 7、MinHeight 26）、`GscRedesignTableStatusPill`（CornerRadius 7）。
- 共享 `DataGridCell` Padding `12,8,20,8`；Overview 时间列 `Margin=12,0,20,0`，六列 `40|150|*|96|84|112`。
- SaveCandidate 可信度列是 ProgressBar（Height 8、Maximum 1、`Value={Binding Score}`）+ `P0` 文本；Task/Overview 已有真实进度条，Settings 数值不是业务进度。
- Maintenance 四个主表 `MaxHeight=PositiveInfinity`；`MaintenanceDeviceLayout` / `MaintenanceProcessLayout` 为 `VerticalAlignment=Stretch`。2K/4K fill ratio：Diagnostics 0.89/0.93、Device 0.82/0.88、Audit 0.88/0.92、Process 0.90/0.93。
- 当前基线：Release 0 warning/0 error；Playnite `263/263`；render-qa 11 档（含 3840×2160）+ 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/8 EXPECTED INFO/0 失败路由。
- v6.2 截图：`artifacts/ui-qa/v6-2-shots/`，命令 `scripts/capture-v6-2-shots.ps1`。
- 提交：`c58b359`、`6a68a59`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-CLOSURE-V6 当前事实

- 页面历史已改为 Playnite 会话级：`GameSaveCenterPlugin.SessionLastWorkspace`；首次打开 Overview、同会话恢复、重启回 Overview；`Settings.LastWorkspace` 保留但不再作为启动依据。
- `GscNumericFieldInput` 根模板已修：`PART_ContentHost` 绑定垂直内容对齐；数字 1/5/30/120/1440 完整居中。
- 全局活动为轻量六列表格（40/150/*/88/76/112）+ header；Overview 主列/次列 disabled ScrollViewer 已改为 Grid；Maintenance Device/Process 与 Media Current 外层 ScrollViewer 已改为有限 Grid。
- Task/Media 筛选带语义前缀；Device/Process 主表最小视口 252 DIP。
- 基线：Release 0 warning/0 error；Playnite `261/261`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/8 INFO/0 TRUE_PARENT_CHILD_SCROLL_CONFLICT。
- v6 截图：`artifacts/ui-qa/v6-shots/`；命令 `scripts/capture-v6-shots.ps1`。
- 提交：`baa8f72` 计划及后续实施/文档提交见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-FIX-V4 当前事实

- 来源：`GameSaveCenter_UI_Overnight_Fix_Pack_v4.zip`，计划在 `docs/ai/UI_OVERNIGHT_FIX_PLAN_V4.md`，报告在 `docs/ai/UI_OVERNIGHT_FIX_REPORT_V4.md`。
- `GscDisclosureCard` 已升级：独立 chevron 图标区、垂直居中、无尾部 `>`；所有页面 Expander 统一引用且折叠体内不再内滚。
- 维护中心诊断页已拆成二级 Tab：默认 `问题列表`（FindingsGrid 独占），次项 `诊断概览`（环境/操作/摘要共用页面滚动）。旧内部 ScrollViewer 已删除。
- 存档备份自动化与策略模板的数值输入全部补齐 label/unit/helper；共享样式 `GscFormFieldLabel`、`GscFormFieldHelper`、`GscNumericFieldInput`。
- 首页全局活动行高 60 DIP、图标居中、列 `40/*/Auto(180)/112`。
- 当前基线：Release 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `255/255`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/39 INFO/0 失败路由。
- v4 截图：`artifacts/ui-qa/v4-shots/`；命令 `scripts/capture-v4-shots.ps1`。
- 用户后续反馈已修复：折叠 header 文字与图标垂直居中；`GscNumericFieldInput` 数字水平/垂直居中显示，框尺寸不变。
- 提交：`3015182`、`5131e4d`、`0201615`、`5196f4a`、`fc86ecc`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-REWORK-V3 当前事实

- 来源：`GameSaveCenter_UI_Design_and_Prompt_Pack_v3.zip`，计划在 `docs/ai/UI_VISUAL_REWORK_PLAN_V3.md`。
- Overview：当前游戏卡三按钮同排同几何；最近 30 天动作/统计/折叠三层分离；全局活动为轻量表四列，Time 固定 112 DIP，窄窗 chips 下移。
- Save：当前存档规则状态一行 badge 并按 `SelectedGame.HealthState` 着色，三按钮统一紧凑几何，卡片压高。
- Maintenance：环境卡摘要化，首次环境检查/更多维护操作统一 `GscDisclosureCard`（去尾部 `>`），FindingsGrid 五列最小宽度收敛为 72/120/160/*180/140；两个主 Disclosure 内容使用内部有限滚动，展开不挤压主表。
- Disclosure 统一入口：`GscDisclosureCard`（别名 `GscDisclosureCardExpander` 保留），Chevron 独立图标区、整行可点、Hover/Expanded 主题态；Media/Save/Task 的旧 `GscExpander` 引用已全部替换，页面不再引用旧样式。
- 颜色分层全部使用 DynamicResource/Design Token：正常绿、信息蓝、警告橙、错误红、中性灰蓝；未写死前景/背景。
- 功能保真：REMOVE=0；命令、绑定、DataGrid 5 列、EnvironmentCheckItems、虚拟化和 GamePicker HARD LOCK 均未改。
- Overview Hero/当前游戏列保持 1:1，确保 1536×864 等常用窗口下三个操作按钮同一行；render-qa 会检查 Overview/Save 三按钮的 Y 坐标与高度差。
- 当前自动化基线：Release 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `253/253`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/32 INFO/0 失败路由。
- 截图证据：`artifacts/ui-qa/v3-shots/` 10 张（当前游戏卡、保护折叠/展开、活动宽/窄、Save 标准/窄、Maintenance 初始/两个展开态），生成命令 `scripts/capture-v3-shots.ps1`。
- 提交：`5c3bdae`（v3 计划）、`9ee3660`（Overview）、`e8b8c31`（Save/Maintenance），最终补强与文档提交见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1（实施包 v1）当前事实

- 本轮是严格受控 WPF UI 重构，不是业务重写。事实来源优先级（当时记录，已由 2026-08-20 Demo-first 总规则覆盖）：当前生产 main > UI Audit（commit `4ab44fe`）> 实施包 v1 锁定/范围 > WPF Demo v6.1 > 旧布局。
- 完整功能保真计划在 `docs/ai/UI_REFACTOR_FIDELITY_PLAN.md`：覆盖 92 条命令、43 个 DataGrid 列、30 个 ScrollViewer、143 个条件 UI；默认禁止 `REMOVE`，只允许 `KEEP/MOVE/RESTYLE/COLLAPSE/RESPONSIVE_MOVE`。
- Dashboard 顶部全局 GamePicker 绝对锁定，必须是 Dashboard 单实例共享控件，在六个工作区永久常驻；首页“今日工作台 / TODAY / 当前游戏”只做布局、间距和响应式修正。
- 已知必须修复的 Audit 症状：SaveCandidateGrid 约 3.7 行、MaintenanceAuditLogGrid 约 1.6～1.9 行、MaintenanceDeviceGrid/ProcessGrid narrow 约 3.7 行、诊断 13 工具 narrow 138 DIP 按钮墙、多处 Page Scroll + DataGrid/List Scroll 嵌套。
- Phase 0 基线：Release 构建 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `238/238`，source/XAML/WPF/render-qa 全绿。后续按 Phase 1～8 分阶段独立提交并 push。
- Phase 1（共享布局基础）已交付：`Redesign.xaml` 新增 `GscInternalTabControl`、`GscInternalTabItem`、`GscToolbarActionRow`、`GscToolbarOverflowButton`，并由 `WpfUiResourceDictionaryTests` 锁定；未改任何 View 页面与业务。
- Phase 2（首页 Overview）已交付：六项 Snapshot 指标改为响应式紧凑 Summary Strip（6/3/2 列），最近 30 天保护明细默认折叠到共享 Expander，全局活动改为稳定四列；`OverviewStatStrip` 响应式列数、保护明细可达性、全局活动四列由新回归测试锁定。GamePicker 与首页锁定结构未改。
- Phase 3（存档中心）已交付：历史/候选窄窗 Inspector 默认收起为“查看详情”按钮，主表高度在 1040×700 分别提升到约 385/254 DIP；候选页头部压成单行；策略模板区默认折叠但全部命令可达。新增窄窗 Inspector 切换回归测试。
- Phase 4a（修改器中心 Trainer）已交付：已绑定工具页窄窗默认收起工具设置 Inspector 为详情按钮，1040×700 工具列表视口 236 DIP；新增 Trainer 窄窗切换回归测试。FLiNG/可下载版本/导入流程未改。
- Phase 4b（媒体中心 Media）已交付：当前媒体窄窗 Inspector 默认收起为详情按钮，来源规则添加表单默认折叠但字段可达；新增 Media 窄窗切换与来源表单折叠回归测试。待归类 DataGrid 与媒体异步缩略图未改。
- Phase 5（任务中心 Task）已交付：游戏筛选在 compact 进入“更多筛选”Expander、wide 回到主行；任务详情 Inspector 窄窗默认收起为详情按钮；操作行保持横向；任务表 1040×700 视口 252 DIP。新增 Task 窄窗切换与更多筛选移动回归测试。
- Phase 6（维护中心 Maintenance）已交付：诊断常用按钮收敛为主行 5 个，低频命令进入共享 Expander；审计日志表视口提升到 280 DIP（约 6 行）；设备/进程/Findings 主表保持 350 DIP。保留策略与全部维护命令未改。
- Phase 7（设置轻量统一）已交付：设置字段标签列宽 token 化为 `GscSettingsFieldColumnWidth`；五个设置分区与保存语义未改。
- Phase 8（最终回归）已交付：Audit HIGH 从 10 清零、MEDIUM 从 4 降到 0，失败路由 0；最终测试基线 Core 59/Worker 190/Playnite 250；真实宿主主题/DPI/连续缩放仍为 MANUAL QA REQUIRED。
- Phase 8 收口：`OverviewView.xaml` 把“当前游戏”卡片 3 个操作按钮底部边距从 8 收到 4 DIP，消除最后一个 Audit MEDIUM（未命名 WrapPanel 92 DIP）；顶部工作台工具栏使用 `Padding="14,10"`，1040×700 下由 91 降到 79 DIP。无 REMOVE，GamePicker 与 Dashboard 锁定区域未改。
- 扩档验证：render-qa 覆盖 10 档逻辑尺寸（1040×700 / 1100×720 / 1280×720 / 1366×768 / 1536×864 / 1600×900 / 1707×960 / 1920×1080 / 2048×1152 / 2560×1440）；UI Audit 新增 2K 与 1100×720 尺寸，快照 161，HIGH/MEDIUM 均 0，运行时警告 39。Audit 工作区高度已改为窗口高度，与生产 Dashboard 和 render-qa 一致。
- 主题 QA：RenderHarness 对 7 个工作区 × 4 尺寸 × Light/Dark 共 56 个离屏场景渲染并校验调色板与视口，全部通过；像素采样确认 Light/Dark 背景确实切换。真实 Playnite 宿主主题仍为 MANUAL QA REQUIRED。
- 页面级横向溢出门禁：render-qa 与主题 QA 要求 `*ScrollSurface` / `SettingsScroller` 的 `hbar=Disabled` 且无横向溢出；DataGrid 内部列滚动允许。10 档尺寸与 56 主题场景均通过。
- Resize 恢复：render-qa 新增 2560×1440 → 1100×720 → 2560×1440 同实例布局恢复探针，7 个工作区全部恢复；修复 Save/Task/Trainer Inspector 宽窗不恢复的缺陷，新增 3 条回归测试。
- 验收审计：`docs/ai/UI_REFACTOR_ACCEPTANCE_AUDIT.md` 已落盘，逐项映射实施包验收清单；真实 Playnite 宿主主题/DPI/连续缩放与大数据滚动仍为 MANUAL QA REQUIRED。
- 真实宿主 reload 已验证：`dev-install-run.ps1 -Configuration Release` 成功安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，扩展日志记录 `0.6.70.0 loaded`，Worker 从当前扩展目录运行，`18:10` 后无 ERROR/Exception/crash。
- Visual Correction v2 已完成：Overview 单滚动、风险卡去内滚、Disclosure、活动行响应式、Save 卡片、Diagnostics 去父子双滚动、Audit 二级切换；新增 OV/SAVE/MAINT 断言，最终 Audit HIGH 0、MEDIUM 0、运行时警告 33。
- Visual Correction v2 真实宿主 reload 已验证：Playnite 加载 `GameSaveCenter 0.6.70`，扩展日志确认 `0.6.70.0 loaded`，Worker 从当前扩展目录运行，`20:39` 后无 ERROR/Exception/crash。

## 当前事实覆盖（2026-08-14 Layer A 收口、Layer B 13 项与 Layer C 11 项）

- `UI-AUDIT-001` 已交付（提交见 `git log -1`）：开发专用 UI 自动审计工具由 `scripts/capture-ui-audit.ps1` / `GameSaveCenter-UI-Audit.cmd` 启动，复用 RenderHarness 渲染真实生产视图；自动扫描 XAML 生成路由/Manifest/保真矩阵，输出视觉树与布局 JSON；页面级滚动容器直接渲染完整内容，DataGrid/ListBox 逐段滚动拼接 `-scroll-*.png`；覆盖 maximized/2k/wide/standard/compact/narrow-1100/narrow，最终 ZIP 在 `artifacts/GameSaveCenter-ui-audit.zip`。后续新增页面只要放入 Dashboard 或 `Views` 目录并保持无参构造，静态盘点与运行时路由会自动纳入。
- 用户日志中的“编译解决方案”失败根因是旧 `dotnet/testhost` 或 Worker 锁住标准 `bin\Release` 输出，随后测试项目无法覆盖 DLL/PDB/XML；不是 `GameSaveCenter.Contracts` 编译失败。
- 一键开发安装器现在默认不请求管理员权限。`scripts/build.ps1`、`scripts/package.ps1` 和 `scripts/dev-install-run.ps1` 支持按运行生成 `artifacts\dev-build\<Configuration>\<guid>` 隔离的 bin/obj、Worker 发布和安装暂存目录，入口修订号为 `DEV-INSTALL-007`。Playnite 发现增加运行中进程、常见目录、卸载信息、App Paths 和 PATH；未发现 Playnite 且没有运行中的 Playnite 时允许继续构建/安装并提示无法自动启动。Playnite 正常退出超时后，仅当进程属于当前会话、可执行文件路径与本次发现结果完全一致且已经没有主窗口时，才结束该无窗口残留；路径不可确认、跨会话或仍有主窗口时继续停止安装。
- 真实宿主已验证：安装报告为 0.6.70 / DLL 0.6.70.0；Playnite `playnite.log` 记录插件加载，插件日志记录 0.6.70.0，`worker-launch.log` 记录存储初始化、过期任务整理和 `Application started`。不要再用 2026-08-12 的 PID 3896 历史日志判断当前安装器行为。
- 当前自动化基线为 Core `59/59`、Worker `190/190`、Playnite `250/250`，Release 构建 0 warnings / 0 errors；source、XAML、WPF 静态门禁与 10 档 `render-qa` 通过；扩档 UI Audit 161 快照、0 HIGH/0 MEDIUM/0 失败路由。真实开发安装已成功，Playnite 与 Worker 启动日志正常。
- `ATOMIC-IO-001` 已交付：新增共享 `AtomicFileWriter`，Worker 设置持久化与媒体复制统一使用“目标同目录临时文件 + 原子 Move”，失败自动清理 `.tmp/.partial` 后再抛出；`WorkerOptions.Persist()` 与 `MediaSyncService` 私有复制逻辑已委托给共享实现。
- `SOAK-001` 已交付：`SoakStabilityHarness` 加速压测任务协调、事件扇出、单游戏锁、原子写入和 SQLite 探针；`TaskEventBroadcaster.SubscriberCount` 与 `GameOperationLock.TrackedGameCount` 提供只读稳定性计数，`scripts/soak-test.ps1` 支持用 `GSC_SOAK_ITERATIONS` 扩展到最多 5000 轮长跑。
- `FAULT-INJECTION-001` 已交付：`FaultInjectionHarness` 注入原子写、外部进程、任务协调、事件广播、操作锁、损坏 ZIP 与损坏 SQLite 共 15 类边界故障，断言无残留、稳定终态、原始文件不被失败注入删除，且锁/订阅全部回收；`scripts/fault-injection-test.ps1` 可独立运行。
- `A-HARDEN-001` 通知级别主体已收口：`NotificationLevel` 持久化默认 `Summary`，`NotificationLevelPolicy` 控制仅重要事件/退出摘要/详细任务；`SessionNotificationAccumulator` 已抽出并覆盖同 Session 单次 final、期望任务数、重复投递等测试。非任务型重要事件（健康风险/冲突/完整性严重）仍由 Dashboard Findings 承载，未单独 toast。
- `A-HARDEN-002` 已交付：未分类 `CustomExecutable` 在普通游戏下按 AutoStart 正常启动；反作弊游戏下必须持久化 `AllowUnknownToolWithAntiCheat` 授权后才允许，Trainer/CT/GameModification 继续禁止；`game_tools` 新增授权列并纳入旧库升级测试。
- `A-HARDEN-003` 已审计收口：首次使用“测试备份”按钮复用真实 `MessageTypes.BackupGame` 生产链路，无独立假服务；无可用测试游戏时显示“可稍后在存档中心手动执行备份”，并有回归测试锁定命令链路。
- `DIAGNOSTICS-001` 已升级：诊断包包含 `system/worker/dependencies/database/recent-tasks/health/settings` JSON、审计与受限日志；`DiagnosticRedactor` 集中脱敏密码、Token、API Key、Authorization、URL query、UNC 凭据、邮箱和用户路径。
- `SAFE-MODE-001` 已升级：Worker 连续 3 次启动失败后请求安全模式，Playnite 询问确认；设置页支持“下次以安全模式启动”，维护中心安全模式提示条提供“恢复正常模式”。
- `INTEGRITY-001` 已补齐：自检覆盖孤儿归档、Manifest 无效/重复路径、磁盘剩余空间和未配置依赖状态；结果使用 `Healthy/Warning/Error/Skipped`，仍只读不自动修复。
- `DB-MIGRATION-001` 已补齐：两代旧库 Fixture 覆盖策略、模板、会话、设备决策、GameTool 与备份历史，并使用 `ReadScalar` 验证真实数据值而非仅检查表存在。
- `METADATA-BACKUP-001` 已补齐恢复流程：预览校验 manifest/哈希/路径越界，确认后备份当前元数据、原子替换数据库与设置、完整性校验并在失败时回滚；维护中心提供“恢复元数据灾备”入口。
- `REPOSITORY-REBUILD-001` 已补齐：只读扫描预览统计已确认/未归属/部分缺失/损坏归档，执行重建必须用户确认，未确认不写库。
- `PATH-REMAP-001` 已补齐：只读预览按类型列出受影响路径和目标存在状态；目标缺失默认跳过，可显式授权仍应用；执行前自动创建元数据灾备。
- `TASK-RECONCILE-001` 已补齐：任务持久化 `WorkerSessionId`，启动协调只处理旧 Worker 会话遗留任务；Backup/Media/Cloud 标记可重试中断，Integrity 标记普通中断，Restore 标记人工介入且不自动重试。
- `GAME-OP-LOCK-001` 已补齐：`GameOperationKind` 与显式兼容矩阵写入代码，备份/恢复/媒体/云端使用类型化锁；Restore 不与其他操作并发，同游戏双 Backup 禁止。
- `IPC-COMPAT-001` 已补齐：握手返回 `AppVersion` 与能力列表，协议版本独立于应用版本，能力包括 RestoreReadiness/MetadataBackup/RepositoryRebuild/PathRemap/TaskReconcile/GameOperationLock/AtomicIo。
- `ATOMIC-IO-001` 已审计补齐：共享原子写入覆盖设置/媒体/元数据恢复/启动失败计数，取消写入或替换失败时旧文件保持完整且无残留。
- `SOAK-001` 已补齐：DataScale Soak 默认小规模、`GSC_SOAK_DATA_SCALE=1` 全量规模；监控 Managed Memory/句柄/线程/订阅/临时文件并断言有界增长。
- `STORAGE-001` 已交付：维护中心“保留策略”页新增只读备份存储分析卡；显示卷剩余/总容量、目录实测与索引体积、版本数、7/30/90 天增长趋势、Top 5 游戏占用排行，并给出标注“估算”的简单容量耗尽预测；新增 IPC `storage.analysis`、Worker 服务与取消支持。
- `RETENTION-SIM-001` 已交付：维护中心“保留策略”页新增全局保留策略模拟器；按每游戏策略复用 `RetentionPlanner` 计算现有/保留/候选清理/预计释放、用户锁定/健康保护/PreRestore 计数与候选明细；`retention.simulation.apply` 要求二次确认，只删除备份根目录下的 ZIP 候选并同步移除 SQLite 索引，锁定/PreRestore/健康恢复点永不进入候选。
- `LOCAL-MIRROR-001` 已交付：设置页新增“启用第二本地镜像”与镜像目录；维护中心“保留策略”页新增镜像状态与“同步镜像”入口。Worker `LocalMirrorService` 只复制和按大小校验，绝不删除镜像中多余文件；外置硬盘未连接时状态为 `Unavailable` 而不是系统错误；同步完成后写入镜像标记文件。
- `ACTIVITY-001` 已交付：首页新增“全局活动”时间线，由 `ActivityTimelineMapper` 把最近 100 条审计记录映射为备份/恢复/云端/媒体/工具/健康/冲突/完整性/仓库修复等业务事件；只展示时间、游戏、分类、结果与摘要，不暴露原始日志，UI 最多显示 12 条并保持有限视口与虚拟化。
- `PLAYNITE-QUICK-001` 已交付：`GetGameMenuItems` 为游戏右键菜单提供“立即备份 / 查看备份历史 / 验证最新恢复点 / 游戏工具 / 打开设置”五个快捷操作，全部绑定当前所选游戏 ID，并复用 Worker 生产 IPC 链路。
- `DRAGDROP-001` 已交付：修改器中心支持单文件/目录拖拽导入，`.ct` 自动按 CheatTable，`.lnk/.bat/.cmd/.ps1` 按自定义启动项，`.exe` 弹出“修改器/普通启动项”二选一，`.zip`/目录进入既有主程序选择流程；未选择游戏时拒绝导入并提示。
- `UI-STATE-001` 已交付：设置持久化上次 Workspace、任务状态/游戏/类型筛选、任务搜索、媒体筛选与媒体搜索；VM 启动时恢复，变更经 500ms 防抖保存；运行中游戏优先与上次选择恢复继续复用既有 GamePicker 持久化，不保存 Loading/Busy/Error 等瞬态。
- `ACCESSIBILITY-001` 已交付：`Ctrl+F` 按当前 Workspace 聚焦游戏/任务/媒体/FLiNG/进程映射搜索框并全选；任务、媒体、FLiNG 与游戏搜索框补充 `AutomationProperties.Name`；共享 `GscSharedFocusVisual` 与高对比度降级继续生效。
- `UI-STATES-001` 已交付：新增共享 `WorkspaceStatePresenter`，统一 Loading/Empty/Error/Degraded/Offline/Disabled 六种状态的图标、标题、说明与可选重试按钮；Overview 全局活动与 Task 空状态已接入共享控件，其余页面继续复用 `GscEmptyStateText`。
- `SETTINGS-VALIDATION-001` 已交付：设置页在标题区显示即时验证摘要，文本框、下拉框与复选框变化时复用 `VerifySettings` 校验并内联展示最多 4 条错误；验证错误不再只等 Playnite 保存时出现。
- `MAINTENANCE-REPORT-001` 已交付：新增 IPC `maintenance.report.get` 与 Worker `MaintenanceReportService`，从 SQLite 计数、完整性自检、存储分析与本地镜像状态聚合用户可读健康报告；维护中心诊断操作带新增“复制健康报告/导出健康报告”，支持 TXT/Markdown；报告不含日志、原始数据库或凭据，与开发者诊断 ZIP 明确区分。
- 最终代码缺口已闭合：`RepositoryRebuildService` 现在可从空/新 SQLite 按磁盘 ZIP 与 Manifest 重建历史，按 Ludusavi 目录名创建 `recovered-*` 占位游戏，不猜 Parent，二次重建幂等；`MetadataBackupService` 灾备包新增 `settings/plugin-settings.json`，恢复后由 Playnite 侧导入插件设置并回滚；`WorkspaceStatePresenter` 已覆盖存档历史 Loading、修改器工具 Loading/Empty、媒体 Worker Offline、维护云端 Degraded；`LocalMirrorService` 同步改为 SHA256 内容校验，同大小但内容不同会重新复制。
- 崩溃修复：`GscWorkspaceStatePresenter` 模板内重试按钮从普通 `Button` 改为 `ui:Button`，修复真实 Playnite 切换存档页时 `“Button”TargetType 与元素“Button”的类型不匹配` 的 XamlParseException；已增加源码回归断言并在真实宿主复测。
- Metadata 原子回滚：恢复前用 `VACUUM INTO` 生成一致性 DB 快照（不再直接复制可能缺 WAL 的活库）；Worker 新增 `metadata.restore.rollback`，Playnite 侧新增 `MetadataRestoreCoordinator`，Plugin 设置导入/保存/应用任一步失败时先恢复旧插件设置，再调用 Worker 从 PreRestorePath 回滚 DB 与 Worker 设置，失败才进入人工介入。
- 本轮已修复 Layer A 审计缺口：多设备只有 Manifest 内容指纹相同才可判定等价；仅文件数/总大小相同改为保守的未知分歧；Restore Readiness 使用可取消的流式解压与增量 Hash；环境检查分别验证数据、存档和媒体所在磁盘；Manifest 重复路径不会抛异常或产生强指纹。
- `DIAGNOSTICS-001` 已完成：维护中心可导出有上限、只读、脱敏的 ZIP 诊断包；包含环境/任务/审计/Worker 日志摘要，不包含数据库、存档、媒体或凭据；新增 IPC 请求和 Worker 测试覆盖敏感字段与大小边界。
- Layer A 14 项、本轮审计补缺、A-HARDEN-001/002/003、Layer B 13 项（DIAGNOSTICS/SAFE-MODE/INTEGRITY/DB-MIGRATION/METADATA-BACKUP/REPOSITORY-REBUILD/PATH-REMAP/TASK-RECONCILE/GAME-OP-LOCK/IPC-COMPAT/ATOMIC-IO/SOAK/FAULT-INJECTION）与 Layer C 11 项已交付；逐项验收见 `docs/ai/PRODUCT_HARDENING_LAYER_B_AUDIT.md` 与 `docs/ai/PRODUCT_HARDENING_LAYER_C_AUDIT.md`，最终逐项审计见 `docs/ai/PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md`，人工验收清单见 `docs/ai/FINAL_MANUAL_QA_CHECKLIST.md`。由于真实场景人工验收未全部完成，整体 Epic 状态为 `PARTIALLY COMPLETED / MANUAL QA REQUIRED`，不能宣称全部任务完成。
- 通知级别已收口：`ImportantOnly` 只显示失败/取消任务与警告/失败摘要，`Summary` 保持一次退出摘要，`Verbose` 在最终摘要外逐任务显示；设置页新增通知级别选择，旧设置缺省归一为 `Summary`。
- 安全模式已交付：全局开关持久化到插件与 Worker 设置；开启后暂停自动退出/定时备份、自动媒体同步、自动工具启动、会话存档快照与保护提示、云端自动上传与自动重试，手动操作和恢复仍可用。维护中心诊断页与诊断摘要会显示当前状态。
- 完整性自检已交付：维护中心“完整性自检”通过 IPC 检查 SQLite 完整性/外键/表结构、目录可写性、配置程序存在性和索引文件引用；只报告不修复，数据库问题为 Critical，文件缺失为 Warning。
- 数据库迁移 Harness 已交付：`DatabaseMigrationHarness` 在临时目录创建旧版 Fixture 后执行当前 `SqliteStateStore.InitializeAsync`，覆盖旧库升级、全新库创建、重复初始化和失败报告；只操作临时数据库，不触碰用户数据。
- 元数据灾备已交付：维护中心“导出元数据灾备”生成 SQLite `VACUUM INTO` 一致性快照、脱敏 Worker 设置和版本清单 ZIP；不包含存档、媒体或凭据，超过 512 MiB 安全上限时失败并清理。
- 备份索引重建已交付：维护中心“重建备份索引”按 Ludusavi 磁盘列表重建 SQLite 版本索引，单游戏失败不中断，只读归档并保留失败游戏原索引。
- 批量路径迁移已交付：维护中心“批量路径迁移”按旧根/新根前缀批量改写 SQLite 与 Worker 设置中的已索引路径；只改引用不移动文件，服务端强制确认。
- 中断任务协调已交付：维护中心“协调中断任务”把 Worker 重启遗留的排队/运行中任务幂等标记为 `WORKER_RESTARTED`，启动时仍自动执行同一逻辑。
- 单游戏操作锁已交付：同一游戏的备份、云端重试、媒体同步、恢复预览/执行互斥，超时返回 `GAME_OPERATION_BUSY`；不同游戏并行不受影响。
- IPC handshake 已交付：`system.handshake` 返回协议版本、最低支持版本与 Worker 版本；客户端握手不兼容即拒绝，旧 Worker 回退 Ping 探测。
- Restore 在实际写入开始后的失败、异常或后校验失败必须尝试恢复锁定的 PreRestore；回滚本身失败才进入 `ManualInterventionRequired`。灾难演练现覆盖 A/B/Undo、部分写入、写后异常、权限、只读、目录缺失和回滚失败。
- 多设备云目录使用持久化 32 位不透明 `DeviceId`，机器名只用于显示与旧 sidecar 兼容；便携设置导入不得复制设备身份。远端恢复继续要求隔离下载、Rclone check、Ludusavi 版本确认和既有 PreRestore 恢复链。
- 每游戏策略新增 `BackupAnomalyProtectionLevel`（Off/Normal/Strict）；重要游戏模板默认 Strict。Manifest 大量删除参与异常检测，最后健康恢复点与用户 Lock 都不能成为 retention 候选。
- Rclone 每次执行都经过命令白名单 `copy/check/lsf/cat/version`，禁止 `sync/move/delete/purge`；外部进程日志不再记录完整参数。Worker 重启会把未完成任务转为 `WORKER_RESTARTED`，取消会终止子进程。
- 真实 Rclone 断网、真实两台设备、真实游戏 Restore/Undo、真实 EXE/LNK/BAT/PS1、1000+ 游戏库和完整主题/DPI 连续缩放仍为 `MANUAL QA REQUIRED`，不得由自动化结果冒充。

## UI-QA-REAL-006 设置分类 Tab 实际裁切修复（2026-08-13）

- 上一轮仅在 `TabPanel` 外增加底部留白没有解决用户截图中的直线底边。实际根因是 `GscRedesignSettingsTabItem` 让圆角 Border 直接充满 `TabItem` 模板布局槽，并开启 `ClipToBounds=True`；`TabPanel`/宿主布局取整后会把 Chrome 的底部圆角贴槽裁平。
- 当前共享模板使用不裁切的 `TabItemRoot` 包裹独立 Chrome；Chrome `VerticalAlignment=Top`、`Margin=0,0,0,2`，因此始终保留底部安全距离并移除 Chrome 的 `ClipToBounds=True`。
- 分类滚动内容使用真实 `SettingsHeaderBottomSafetyZone` 元素放在 `TabPanel` 后面形成内容 extent；顶部横向模式折叠该元素。RenderHarness 同时检查最后一项 `TabItem`、Chrome 的底部位置和 `chromeSafety >= 1`。
- 当前验证：5 种窗口渲染图通过，设置几何探针和 Playnite `210/210` 通过；真实 Playnite 主机的 DPI/主题/连续缩放依旧只能由人工验收确认。

## 2026-08-13 UI-QA-REAL-005 首页顶端对齐、当前游戏空间与设置圆角回归

- 首页宽屏 `OverviewSecondaryScrollViewer` 与其内容面显式使用 `VerticalAlignment/VerticalContentAlignment=Top`，并在响应式代码中重复设定，避免 Playnite 宿主模板刷新后“今日概览”落到工作区中部。
- 首页 Hero/当前游戏宽屏列由 `1.25* + 0.75*` 调整为 `1.1* + 0.9*`；离屏报告中的当前游戏/Hero 宽度比约 `0.82`，原约 `0.60`，没有改变 Hero/当前游戏的堆叠断点、命令或绑定。
- 设置共享分类栏模板在 `TabPanel` 外增加命名的底部安全 host，并设置顶部内容对齐、像素对齐和布局取整；滚动到末端时最后一个分类的底部仍落在 viewport 内，避免圆角被横向直线裁掉。
- RenderHarness 现在在截图前显式解除设置页入口动画的 `Opacity=0`，并检查 Overview 右栏 top delta、当前游戏宽度比和 Settings 最后一张 Tab 的底部几何，避免“空白 PNG/只测到布局没有测到可见性”。
- 验证：`python scripts/validate-source.py`、WPF 静态门禁、`git diff --check`、五种窗口尺寸 `render-qa` 全绿；Core `42/42`、Worker `117/117`、Playnite `210/210` 通过。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## AI/Codex 启动协议

开始 GameSaveCenter 开发前，请依次阅读：

1. `docs/ai/PROJECT_MEMORY.md`（本文件）
2. `docs/ai/WORKLOG.md`
3. `docs/DEVELOPMENT_HANDOFF.md`
4. `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`（如果存在）
5. `docs/design/UI_CHANGE_GATE.md`（如果存在）
6. `git log` 最近 15～30 个 commit
7. `git status`

然后才开始修改代码。不要仅凭历史对话假设当前项目状态；代码、文档和 Git 历史是唯一事实来源。

## 项目定位

- GameSaveCenter 是 Playnite 的 GenericPlugin，提供存档备份/恢复/校验、媒体同步、任务中心、维护中心、修改器与 CT 管理，以及新增的自定义游戏启动项能力。
- Playnite 是唯一主要 UI（WPF），后台 Worker 是独立 .NET 8 进程，两者通过 Named Pipe IPC 通信。
- `GameSaveCenter.Contracts`：Playnite/Worker 共享的 DTO、枚举、消息类型，netstandard2.0。
- `GameSaveCenter.Core`：Playnite 侧可复用逻辑（目前主要是启动/包装与少量辅助）。
- `GameSaveCenter.Worker`：持久化、Ludusavi、Rclone、媒体索引、任务编排、游戏 Session、GameTool 导入/启动/追踪。
- `GameSaveCenter.Playnite`：WPF Dashboard 外壳 + 六个 Workspace 页面 + 设置页。
- 数据持久化：SQLite（`SqliteStateStore`）+ 文件系统（存档、媒体归档、GameTools 目录）。
- 模块关系：Ludusavi 负责存档底层；Rclone 只允许 copy/check，不使用 sync/delete/purge；媒体为增量同步；GameTool 绑定在游戏级。

## 当前主要架构

### 程序集与入口
- Solution：`GameSaveCenter.sln`，版本 `0.6.70-development-preview`（`Directory.Build.props` 0.6.70）。
- 插件入口：`src/GameSaveCenter.Playnite/GameSaveCenterPlugin.cs`，扩展 ID `66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。
- Worker 入口：`src/GameSaveCenter.Worker`，IPC dispatcher 为 `IpcRequestDispatcher`。
- 测试：Core 42、Worker 117、Playnite 203（2026-08-13 当前基线；优先使用 `scripts/build.ps1 -OutputRoot <目录>`，避免本机旧 Worker/测试宿主锁住标准输出）。
- ONBOARDING-001（2026-08-13）新增 `environment.check`：检查服务驻留 Worker，使用临时 SQLite 表和目录临时文件做可逆探针；Rclone 未配置为 `Skipped`，不把可选云端能力误计为基础失败。当前基线为 Worker 70、Playnite 198；UI 仍复用 Maintenance 诊断页的单一外层滚动与有限表格视口。
- GAME-TOOL-003/004（2026-08-13）新增 `GameToolIfAlreadyRunning` 与 `GameToolRiskCategory` 持久化列。CustomExecutable 的已有实例策略只允许按解析后的 EXE 完整路径匹配；Skip 为默认，Restart 只重启再次确认过的同路径 PID，路径读取不完整时必须保守停止。反作弊游戏仅允许已分类为 `GeneralUtility` 的自定义工具自动启动；Unknown 与 `GameModification` 自动启动必须阻止并写审计，用户需在 TrainerCenter Inspector 明确分类后保存。
- SMART-PROTECT-001/002（2026-08-13）：完整游戏停止请求等待存档识别并以持久化提示状态驱动三选一保护提示；只在识别到候选/匹配存档时提示，未识别时写审计并等待后续识别。`Deferred` 有 7 天冷却，`Enabled`/`Dismissed` 不再弹出；停止 IPC 使用 3 分钟专用超时。Overview 最近游戏列表显示已保护、未匹配、存档未保护和风险，已保护项不可选，其余项可批量启用游戏中/退出后推荐保护并写审计。不要新增主导航页或绕过既有恢复安全边界。
- NOTIFY-001 / MULTI-DEVICE-001 / RCLONE-RELIABILITY-001（2026-08-13）：退出备份与媒体任务使用同一 SessionId，Playnite 依据 Task Center 的终态任务聚合为一条退出摘要；本地备份成功但云端失败时必须同时显示本地成功和云端可重试失败。设备摘要携带 `ParentBackupId`，同一父版本分叉只标记冲突并要求人工决策，禁止自动合并/覆盖/删除；下载远端仍必须进入隔离 staging、校验、归档检查后才能走既有安全恢复链。Rclone 仅允许 copy/check/lsf/cat/version；网络或不完整传输有限重试，凭据/权限/远端不存在明确失败并停止自动重试。

### Dashboard / Workspace
- `DashboardViewModel` 是大型聚合 ViewModel（技术债，暂不拆分），持有所有 Workspace 数据与命令。
- 六个 Workspace：Overview（首页）、Saves（存档中心）、Trainers（修改器中心）、Media（媒体中心）、Tasks（任务中心）、Maintenance（维护中心）；另有 Settings 页面。
- 工作区页面位于 `Views/`：DashboardView + 各 CenterView；共享资源在 `Themes/DesignTokens.xaml`、`Themes/WpfUiProduction.xaml`、`Themes/Redesign.xaml`。
- Dashboard 视图有响应式 code-behind 协调（`DashboardView.xaml.cs`），页面级滚动面 + 主表/主列表有限视口 + 内部虚拟化滚动。

### UI-207 当前约束（2026-08-12）

- Settings 的 `SettingsScroller` 位于共享 `GscRedesignSettingsTabControl` 模板内容区；`SettingsHeaderScroller` 是分类导航区。宽屏分类栏为 232 DIP 左侧有限滚动，紧凑布局为顶部横向 `Auto`，不能把根 UserControl 再包回第二个页面滚动器。
- `GscSelectedGameIconControl` 只用于当前游戏上下文表面（Dashboard、Overview、Save、Trainer、Media），GamePicker 虚拟化列表不得加载真实 Icon。
- GamePicker 选择可被当前筛选隐藏但不能静默丢失；必须保留 `SelectedItem`、显示恢复语义并保持 `GamePickerSelectedGameId` 持久化。默认筛选只对新用户/未知值归一为“已安装”。
- 事件驱动的 `PlayniteGameStarted` 自动定位优先于普通刷新；游戏停止不改变当前选择。不得为此新增轮询、进程扫描、IPC 或网络请求，也不得改动 DataGrid 滚动/虚拟化契约。
- 当前自动化结果：本阶段 Worker 相关 Release 构建 0 警告/0 错误，Worker 67/67 通过；上一阶段 Core 27/27、Playnite 197/197、render-qa 通过。真实 Playnite 宿主/DPI/主题人工验证仍待环境。

### 数据流
- Playnite → Worker：Named Pipe 请求（`GameSaveCenter.Playnite/Ipc`、`GameSaveCenter.Worker/Ipc`）。
- 任务状态：Worker `TaskCoordinator` 持久化 + `TaskEventBroadcaster` 事件流 + Dashboard 轮询兜底。
- 快照：`MessageTypes.GetDashboard` 返回 `DashboardSnapshotDto`；大库先渲染 SQLite 缓存，后台再同步。

### GameTool 模型
- `GameToolType`：Trainer / CheatTable / CustomExecutable（自定义启动项）。
- `GameToolDto` + `GameToolVersionDto`：DisplayName、Enabled、AutoStart、LaunchTiming、LaunchDelaySeconds、CloseOnGameExit、RequiresAdmin、ActiveVersionId、EntryPath、WorkingDirectory、Arguments、ResolvedTargetPath 等；`game_tool_versions` 已补 `resolved_target_path` 兼容列。
- Worker `GameToolService`：导入（Trainer/CT 复制进 GameTools 目录；自定义启动项默认保留外部路径引用）、更新、删除、启动、随游戏自动启动/延迟/关闭追踪。
- Session 追踪：`GameToolSessionTracker`（SessionId → PID + 实际 StartTime + CloseOnExit），关闭时要求 PID 与实际 StartTime 双向匹配，禁止按进程名杀。

### 任务系统
- `TaskCoordinator` 统一编排；`TaskStatusDto` 有 Progress/Message/ErrorCode/ErrorMessage/State/时间戳。
- Dashboard `TaskIndexedCollection` 按 TaskId 索引增量合并；`knownTaskStates` 去重通知。

### 媒体系统
- `MediaItemDto` 由 Worker 索引；列表与详情预览已改为 `AsyncThumbnailImage` 异步加载（`Task.Run` 强制后台、3 并发、LRU 96、Freeze 后回 UI、Unloaded 取消）；`MediaThumbnailConverter` 保留为兼容实现。
- Media 列表使用 ListBox + Recycling 虚拟化；页面滚动面与列表滚动分工明确。

### 缓存与性能机制
- `BatchObservableCollection<T>`：批量 Replace 只发一次 Reset（默认引用相等比较；PERF-005 起支持内容比较器跳过未变化）。
- GamePicker 有 180ms 搜索防抖、按 PlayniteId 缓存 `GamePickerItem`、平台指纹短路。
- Task 筛选指纹短路（`ComputeTaskFilterFingerprint`）、平台指纹短路（`ComputePlatformFingerprint`）。
- Dashboard 大库 cache-first + 延迟后台同步；`[PERF]` 日志设施见 `docs/ai/PERFORMANCE_BASELINE.md`。

## UI 设计原则

- 目标是 Apple-inspired 的原生 WPF 桌面工具：清晰层级、克制毛玻璃、圆角、统一设计令牌、自然微动效、深浅色、跟随 Playnite、高对比度、DPI 适配、响应式布局、不使用突兀的原生控件视觉。
- 所有 UI 修改必须先读 `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md` 与 `docs/design/UI_CHANGE_GATE.md`，并遵循 `.codex/skills/wpf-apple-desktop-ui/SKILL.md`。
- 常用窗口下限 1040×700 DIP；1080p/2K/4K 必须按 DPI 换算后的逻辑 DIP 检查全屏、窗口化、最大化；不把 4K 通过当作 1080p 通过。
- 页面级滚动只承载有限测量内容；DataGrid/ListBox 保留 236 DIP 最小视口、内部滚动和虚拟化；堆叠 Inspector 下限 160 DIP。
- 动态下拉框必须显示逻辑默认值（如“全部”）；TaskCenter 游戏/类型筛选通过 `TaskFilterOptionsSync` 增量同步，`全部` 稳定保留在 index 0，不再 Clear/Replace 集合。
- GamePicker 新用户默认筛选为“已安装”，已有明确配置值必须保留；Dashboard 打开时运行中游戏优先，否则恢复上次选择，普通刷新不得抢回用户手动选择。

## 已完成的大型重构 / 优化

- UI-001～UI-205、SKILL-001、QA-001～005：页面 Workspace 化、响应式断点、滚动分工、Inspector 下限、筛选默认值、离屏渲染 QA。
- UI-207（2026-08-12）：设置页 Header 不裁剪与分类栏滚动（920 DIP 断点）、运行中游戏自动定位、上次选择持久化复用、GamePicker 新用户默认“已安装”、当前游戏真实 Playnite Icon（事件驱动，无轮询/无网络，LRU 48）。
- `scripts/render-qa.ps1` + `tests/GameSaveCenter.RenderHarness`：7 页面 × 5 常用窗口离屏渲染回归，含自动失败门禁。
- PERF-001：`BatchObservableCollection` 批量 Reset。
- PERF-002/003：Task 筛选与 GamePicker 平台指纹短路。
- PERF-004（旧编号）：GamePickerItem 缓存复用（新任务编号体系中 PERF-004 是性能基线设施，不要混淆）。
- PERF-004/005/006（新编号）：`[PERF]` 基线日志、Snapshot 无变化 0 Reset、Task/Media 搜索防抖。
- PERF-007：媒体缩略图异步化（`AsyncThumbnailLoader` Task.Run 后台解码 + 3 并发 + LRU + Freeze + `[PERF]` 埋点，`AsyncThumbnailImage` 占位加载并 Unloaded 取消）。
- PERF-009/010：任务事件合并 TaskId 索引 O(1) 更新；命令状态刷新 Dispatcher 合帧。
- GAME-TOOL-001/002：自定义启动项正式支持 EXE/LNK/BAT/CMD/PS1，外部路径引用不复制文件；Session 级 PID 追踪与 CloseOnGameExit 安全关闭。
- UI-204/205：TaskCenter 与 GamePicker 下拉框默认值恢复（含真实 Playnite 异步物化重试）。
- UI-206（含回滚）：DataGrid 滚动几何修复。初版 `Pixel ScrollUnit` 经真实 Playnite A/B 验证会严重恶化空白，已撤回；最终采用 `Item` + `GscStableDataGridRow` 稳定行样式 + geometry probe（60 行 × 非整行高度，gap ≤4 DIP、末行完整、无跳变、Recycling 保持）；诊断摘要取消外层裁剪并由页面滚动负责可达性。

## 当前技术债

- `DashboardViewModel` 仍很大，包含命令、筛选、导入、诊断、设备状态等职责；只有性能实现被严重阻碍或 GAME-TOOL 无法接入时才拆（独立 `ARCH-xxx` 任务）。
- `DashboardView.xaml.cs` 仍承担部分响应式协调。
- 媒体列表/详情缩略图已异步化；真实大量截图滚动下的帧率仍需真机验证。
- 真实 Playnite 宿主、主题切换、DPI 真机、连续缩放流畅性尚未完整人工验收（UI-QA-REAL-001 仅完成冒烟）。

## 当前开发优先级

- P0：性能基础设施与真实热点优化（PERF-004～007、009/010 已完成）。
- P0：自定义游戏启动项（已完成，GAME-TOOL-001/002）。
- P1：媒体性能（PERF-007 异步缩略图，已完成）。
- P1：真实 Playnite / DPI / 大型游戏库 QA（UI-QA-REAL-001 冒烟已完成，完整人工验收待用户）。
- P2：架构进一步拆分（不主动做）。
- PERF-008：已评估收口，维持现状。详情已按激活 Workspace 分支加载，全量快照仅用于全局摘要且后台有 1 分钟 TTL；2000 规模合成 profiling 无 O(n^2)，待真实大库渲染 profiling 证明瓶颈后再评估。

## 2026-08-12 可靠性阶段补充

- `RELIABILITY-RESTORE-001` 已实现：备份历史版本支持非破坏性的恢复可用性检查，结果持久化在 `backup_versions.restore_readiness_json`，检查过程只在应用数据目录隔离提取，不接触真实存档目录。
- `d45f65c` 已补齐恢复校验安全闭环：Manifest 非法、重复/越界路径、Manifest 缺失文件现在不能得到 `Ready`；逐文件路径集合、大小、可用 Hash 和提取结果均纳入判定，验证目录创建失败返回 `Failed`，取消仍由调用方观察。
- `d45f65c` 为恢复编排增加窄接口测试边界，并用临时 SQLite + 内存假 Ludusavi 完成 A→PreRestore→B→失败回滚、成功恢复→Undo、运行中拒绝恢复等灾难演练；未启动 Playnite、未调用真实 Ludusavi、未接触真实存档。
- Ludusavi 备份版本的 `backupPath + backup ID` 已持久化为 `backup_versions.archive_path`；Simple 归档、缺失/损坏 ZIP、路径穿越、超大展开量、不一致统计与不支持压缩方式必须返回明确状态。
- 恢复可用性入口位于现有 Save Center 历史 Inspector，不能新建页面或改变 `SaveHistoryGrid` 的滚动/虚拟化骨架；新增内容必须留在 `SaveHistoryActionsScrollViewer` 内，并继续通过 `render-qa` 验证 1040×700 等窗口。
- 初始实现的历史基线为 Core 13、Worker 58、Playnite 197；当前阶段增量基线为 Worker 67/67，生产 Worker Release 构建 0 警告/0 错误。真实 Playnite 宿主、主题/DPI 人工验收仍待用户环境确认。
- 该恢复可用性阶段的下一项已在后续 `HEALTH-001` 完成；历史记录保留原阶段编号，当前开发顺序见下方 HEALTH-001 补充。

## 2026-08-12 HEALTH-001 阶段补充

- 每游戏健康状态已统一为四态：`Healthy`（健康）、`Attention`（注意）、`Risk`（风险）、`Unknown`（未知）。旧 `Ready / Warning / LudusaviUnavailable` 仅作为 UI/历史缓存兼容输入保留；新 Dashboard 快照输出四态。
- `GameHealthAssessmentService` 是 Core 纯计算服务，证据包括最近游玩、备份版本/时间、最近 30 天失败任务数、最近任务状态、最新 `RestoreReadinessStatus`、未解决 finding 严重度、按游戏策略启用的云端状态；不做磁盘、ZIP、网络或数据库访问。
- Worker 的 `GetDashboardGameRecordsAsync` 一次聚合最新备份可用性、任务失败、finding 和媒体/策略数据；`DashboardService` 只在内存中计算四态和理由，并把 `WarningGames = AttentionGames + RiskGames`，`UnknownGames` 不误计入需处理数。
- UI 改动只复用首页统计卡、Dashboard 游戏列表/选中头部和 Save Center 校验区；四态在有限宽度下不新增列或固定宽度，理由使用已有 Tooltip，旧 `Ready` 夹具继续显示绿色。Snapshot comparer 已比较健康摘要与理由列表。
- 当前测试基线为 Core 19、Worker 59、Playnite 197；源码门禁、XAML 门禁、WPF 静态门禁、隔离 Release 构建和 render-qa 已通过。真实 Playnite 宿主、主题/DPI 人工验收仍待用户环境确认。
- 当前已完成 Restore Readiness、Health、Protection 三项；下一项按附件顺序为 `POLICY-001`，不要重做上述功能，不新增主页面，继续采用小阶段、独立 commit、文档和 push。

## 2026-08-12 PROTECTION-001 阶段补充

- `RecentProtectionAssessmentService` 已在 Core 实现为无副作用纯计算：以 `GameStatusDto.LastPlayedUtc` 过滤最近 7/30/90 天，按未识别存档、从未备份、恢复点不可用、自动保护关闭、云同步异常、游玩后备份过旧和备份健康异常分类；每个游戏只显示一条最高优先级原因。
- `GameStatusDto` 现在带有 `LatestRestoreReadinessStatus`，由 Worker Dashboard 从已有聚合记录投影；Playnite 不增加 IPC、扫描或数据库查询，Overview 只从现有快照计算摘要。
- UI 复用现有 Overview 风险滚动面与 Settings 自动化分类。保护摘要最多展示 6 条；选择条目只改变当前游戏选择并提示用户确认，绝不因筛选/选择自动备份或恢复；没有新增页面，也没有修改 DataGrid、虚拟化或滚动骨架。
- 最近保护窗口设置默认 30 天，接受 7/30/90，便携设置导入会校验非法值，旧 JSON 缺少字段时保持默认值。
- 本阶段验证基线为 Core 27、Worker 59、Playnite 197；Worker/Playnite Release 构建、源码/XAML/WPF 门禁和 render-qa 均通过。真实 Playnite 主题/DPI/键盘/连续缩放验收仍待用户环境。
- 下一项按附件顺序为 `POLICY-001`；不要重做 `HEALTH-001` 或本阶段保护摘要。

## 2026-08-12 POLICY-001 阶段补充

- 策略模板复用 `BackupPolicyDto`，内置模板 ID 固定为 `default`、`important`、`high-frequency`、`exit-only`、`manual-only`；用户模板 ID 必须以 `custom-` 开头。模板应用是一次性复制，不建立继承关系。
- `BackupPolicyTemplateCatalog.ClonePolicy` 是模板的安全边界：周期间隔限制在 1–1440 分钟，保留值不小于 0，所有模板都强制关闭自动恢复。内置模板由 Worker 初始化幂等播种，禁止通过 IPC 修改/删除。
- Save Center 的模板区位于既有策略页滚动内容内，未新增页面、未改变 DataGrid/虚拟化骨架；创建副本时先保存当前选择再清空选择，避免名称丢失。
- Playnite 包必须同时包含 `GameSaveCenter.Core.dll` 与 Worker 的 self-contained Windows runtime；`scripts/package.ps1` 会验证 Core、hostfxr/hostpolicy/coreclr/System.Private.CoreLib 和 `includedFrameworks`，Worker 项目保持 `RuntimeIdentifiers=win-x64`，发布使用单节点/无 node reuse 参数。
- 当前自动验证：Core 29/29、Worker 69/69、Playnite 197/197；Worker/Playnite Release 隔离构建 0 警告/0 错误；source/XAML/WPF 门禁与 render-qa 通过；最终 `.pext` 打包成功。真实 Playnite 日志曾确认插件加载，但旧 Worker PID 3896 仍锁住用户安装目录，完整 Worker/IPC/UI 仍标记为 `MANUAL QA REQUIRED`，不能以隔离首启未进入扩展阶段冒充真实宿主通过。
- 以后每个代码阶段的验收顺序固定为：`dotnet test/build` → 源码/XAML/WPF/render-qa → `scripts/package.ps1` → 安装包内容断言 → 启动 Playnite 并检查 `ExtensionFactory`/扩展日志；若宿主被单实例或权限环境阻断，必须记录为人工验收，不得宣称加载成功。
- 本阶段完成后的下一项为 `ONBOARDING-001`；不要重做 Restore Readiness、Health、Protection 或本阶段策略模板。

## 一键安装器进程停止与权限约束

- `DEV-INSTALL-007` 允许可信 Playnite 候选为空，避免 PowerShell 将空数组绑定到停止函数时直接失败。没有运行中的 Playnite 时，安装器仍使用 `%APPDATA%\Playnite\Extensions`（或显式 `-PlayniteExtensionsPath`）完成安装；以后需要自动启动时应通过 `-PlayniteExecutable` 指定便携版/自定义目录中的 `Playnite.DesktopApp.exe`。
- `scripts/dev-install-run.ps1` 的 `Stop-PlayniteAndOwnedWorkerReliably` 必须先允许 Playnite 正常退出并等待插件回收 Worker，再处理残留；不能把 `Get-Process` 与停止之间的退出竞态误报为失败。
- 安装器不应默认请求管理员权限，也不应按进程名广泛终止 Worker。`DEV-INSTALL-004` 先调用 Playnite 的正常窗口关闭，让插件既有 `OnApplicationStopped`/`WorkerLauncher.StopOwnedWorker()` 回收自己创建的 Worker；只有 Playnite 已退出后仍存在、且路径明确属于当前扩展目录的残留 Worker 才可处理。
- 路径不可读取或残留 Worker 属于其他扩展时必须停止安装并要求用户手动处理，不能为了自动化验证提权或误杀其他用户进程。根目录入口同步检查 `DEV-INSTALL-004`，避免旧副本继续运行已经废弃的提权逻辑。
- `DEV-INSTALL-006` 补齐 Playnite 自身的无窗口残留：先等待 20 秒正常退出；仅对当前会话、精确可信路径且 `MainWindowHandle=0` 的实例执行强制结束，并把 `Refresh` 与停止之间的自然退出视为成功。不得退化为按进程名批量终止。

## 2026-08-12 Worker 生命周期清理补充

- Playnite 插件退出必须调用 `WorkerLauncher.StopOwnedWorker()`；Launcher 只允许停止当前实例记录的 `runningWorker`，不能按名称终止任意 `GameSaveCenter.Worker`。`shutdownRequested` 防止退出竞态重新启动子进程。
- 本阶段 `3f05e16 fix: stop owned worker on Playnite shutdown` 已通过 Playnite Release 全量 198/198、源码校验、Release 编译和 Release self-contained 包验证。
- 隔离 Playnite 的 `--userdatadir` 首次启动会停在 `FirstTimeStartupWindowFactory`，不能据此宣称扩展加载；真实宿主验证仍必须看 `ExtensionFactory:Loaded plugin: GameSaveCenter` 与扩展日志，并记录 `MANUAL QA REQUIRED` 直到用户环境实际通过。

已完成：见 WORKLOG.md 与 Git log；不要重复实现已完成的 UI/性能工作。

## 已知坑

- WPF `ICollectionView.Refresh()` 昂贵；不要在每个按键或每次快照都调用。
- `ObservableCollection` Reset 仍会触发 CollectionView 重建；数据没变时应跳过（PERF-005）。
- 动态 ComboBox Items 重建会清空 SelectedItem；要显式恢复逻辑默认值。
- 大库启动不要同步全量匹配/扫描；先渲染 SQLite 缓存。
- Worker 是独立进程：Playnite 启动早期 IPC 可能超时，要用失败快速降级 + 后台重试。
- 修改器/CT/自定义工具启动一律走 Worker；禁止在 Playnite UI 进程直接 Process.Start 外部程序。
- CloseOnGameExit 只能关闭本 Session 由 GameSaveCenter 启动且能确认 PID/StartTime 的进程；脚本（BAT/CMD/PS1）与系统默认程序打开的文件不可靠，UI 对这类入口禁用开关。
- 自定义启动项支持 EXE/LNK/BAT/CMD/PS1/普通文件：EXE 与导入/重定位时已解析并持久化的 LNK→EXE 目标可跟踪；未解析的 LNK、脚本和系统默认程序启动时 Trackable=false。
- 磁盘 IO、图片解码不要放 UI 线程；图片解码要限制并发并 freeze。
- 表格/列表虚拟化很容易被外层 ScrollViewer 或 DataGrid 嵌套破坏，改 XAML 后必须跑 render-qa。
- DataGrid 不要写死运行时 `Height`，用 `MinHeight/MaxHeight` 保持有限 viewport；`Pixel ScrollUnit` 已在真实 Playnite 验证会回归（轻微滚动即大空白），当前必须保持 `Item` + 稳定行样式，禁止重新改回 Pixel。
- `git push` 前确认没有 bin/obj、用户本地配置、密钥、测试临时文件和大压缩包（如 `GameSaveCenter.7z` 不要提交）。

### ONBOARDING-001 不可丢失约束

- 首次使用状态由 Playnite `GameSaveCenterSettings.OnboardingCompleted` 持久化；未完成时 Dashboard 首次打开定位 Maintenance，用户可“跳过首次检查”，之后仍可手动重新运行环境检查。
- 环境检查只允许读取、创建/删除自身临时探针和只读远端列举；禁止自动备份、上传、同步、删除或覆盖真实存档。测试备份必须由用户明确点击，并且要求当前游戏已匹配且 Ludusavi 可用。
- 真实宿主验收必须看到 `ExtensionFactory:Loaded plugin: GameSaveCenter` 与扩展日志；隔离 Playnite 只能证明进程启动，不可替代真实安装验证。当前安装器不再默认请求 UAC；用户桌面应确认普通双击入口即可完成“关闭 Playnite → 回收 Worker → 构建安装 → 启动 Playnite”链路。

## 文档导航

- `docs/DEVELOPMENT_HANDOFF.md`：跨电脑/跨模型交接入口，包含每轮 UI 基线。
- `docs/PROJECT_MEMORY.md`：长期不可丢失约束与 UI 决策历史（大文件，按章节检索）。
- `docs/DEVELOPMENT_PROGRESS.md`：按 UI 编号的实施历史与下一步线索。
- `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`、`docs/design/UI_CHANGE_GATE.md`：UI 方向与门禁。
- `.codex/skills/wpf-apple-desktop-ui/SKILL.md`：WPF/Playnite UI 专项技能。
- `docs/ai/WORKLOG.md`：每阶段开发流水记录。
- `docs/ai/PERFORMANCE_BASELINE.md`：性能基线与测量方法。

## 2026-08-13 UI-QA-REAL-002 当前事实

- 首页“今日工作台”状态区已改为标题下方全宽第二行，避免 2K 最大化时原窄列 `WrapPanel` 将状态胶囊挤成不可见/竖向圆点；`OverviewView.xaml.cs` 会根据实际或估算宽度调整英雄卡内边距。
- 维护中心首次使用环境检查复用现有检查项，采用响应式 `UniformGrid`：可用宽度 ≥ 900 DIP 为 3 列，620–899 DIP 为 2 列，更窄为 1 列；检查卡统一拉伸并设置最小高度，避免胶囊乱序和不规则空洞。
- 设置页共享文本框内容宿主已垂直居中，默认高度 42 DIP；设置分类卡片的共享模板增加底部安全间距并使用一致圆角，避免窗口底部裁切。
- 本阶段没有新增主导航页面，没有改动业务命令、绑定、DataGrid 虚拟化或 Worker/恢复体系；渲染夹具仅补充首次使用卡片的演示数据。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 206/206；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 真实开发安装完成，Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 正常运行。真实宿主日志/进程证明已记录；用户实际 2K 最大化、主题/DPI、Settings 连续缩放仍标记 `MANUAL QA REQUIRED`。
- 本阶段只完成用户反馈的三类 UI 修复，不引入第 15 个功能；下一步等待人工视觉反馈。

## 2026-08-13 UI-QA-REAL-003 当前事实

- 首页“最近 30 天玩过的游戏”风险卡片的两个操作按钮已经移到摘要下方的独立响应式行，避免 2K 或右侧窄栏中按钮与标题、统计文本挤压。
- 修改器中心启动延迟编辑器现在明确显示“启动延迟”和“秒”，继续绑定 `SelectedGameTool.LaunchDelaySeconds`，输入框高度收敛为 34 DIP。
- 媒体中心 `MediaGrid` 显式使用顶部对齐的虚拟化面板和内容对齐，已修复表头/筛选区下方到首条媒体记录之间的大段空白；未修改列表的虚拟化滚动模型。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 209/209；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 一键开发安装完成，Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 进程从当前扩展目录运行。
- `AUTO VERIFIED` 仅覆盖自动化、渲染、安装和真实宿主日志；用户实际 2K 最大化、主题/DPI、连续缩放及真实媒体数据滚动仍为 `MANUAL QA REQUIRED`。
- 本阶段只补充用户反馈的三个布局问题，不新增主导航页面，不改变业务绑定或 Worker/恢复体系；下一步等待人工反馈。

## 2026-08-13 UI-QA-REAL-004 当前事实

- 设置页左侧分类卡的“底部/边缘圆角被削掉”根因已确认是 `SettingsHeaderScroller` 的滚动条占用内容宽度，固定 232 DIP 的 `TabItem` 被 viewport 裁切，不是 CornerRadius 数值失效。
- `SettingsHeaderScroller` 已扩展到 248 DIP，分类 `TabItem` 仍保持 232 DIP 内容宽度；滚动条出现时为卡片边缘预留安全区，分类卡继续使用 14 DIP 圆角并开启自身边界裁剪。
- 设置页在可用高度低于 760 DIP 时使用更紧凑的 60 DIP 分类卡和 8 DIP 间距；左右设置滚动面保留底部安全留白，避免最后一项在宿主 viewport 边缘被直接截断。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 210/210；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 一键开发安装完成，真实 Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 进程从当前扩展目录运行。
- `AUTO VERIFIED` 仅覆盖自动化、渲染、安装和真实宿主日志；用户实际 2K/DPI 设置页最终视觉仍为 `MANUAL QA REQUIRED`。
- 本阶段只修复设置页现有分类卡和滚动 viewport 的裁切，不新增页面、不改变设置字段、绑定、保存语义或 Worker/恢复体系；下一步等待人工反馈。

## 2026-08-16 UI-206 Overview 页面级迁移事实

- UiLab 的关键骨架不是“右侧摘要从页面顶部开始”，而是顶部 Hero/当前游戏与六项指标占满整行，最近活动开始后才分成左主区与右侧风险/关注栏；生产 Overview 已按此层级重排。
- 生产右栏使用 330 DIP 固定宽度，宽屏与最近活动卡同一行起始（离屏探针偏移 0 DIP）；窄窗口由现有页面级 ScrollViewer 承载并把右栏下移。不要恢复成 1.2*/0.8* 的整页比例栏，也不要把 UiLab 演示滚动条迁入生产。
- 生产数据和行为保持真实：`Snapshot`、`RecentProtection`、`AttentionFindings`、OpenProtection/OpenAttention 命令、选择状态、虚拟化列表和页面滚动都未替换为 demo 假数据；UiLab 右上角颜色按钮没有迁移。
- 共享表头现在使用低对比度表头填充、8 DIP 圆角和 1/2 DIP 安全边距；普通活动行使用较弱 Divider，避免 DataGrid/活动表头看起来像尖锐矩形。
- 本阶段自动验证：源码/XAML 门禁通过，Playnite 303/303，生产 Release 0 warning/0 error，Overview 多尺寸与 Light/Dark 离屏渲染通过。全量 render harness 仍有 Save/Media 窄尺寸主表 `<236 DIP` 的历史门禁项，不能写成全量 render-qa 通过。
- 本阶段尚未完成真实 Playnite 2K/DPI/Follow/高对比度人工验收；后续优先在真实宿主检查页面级滚动、侧栏下移、键盘焦点和长中文文案，再继续迁移其他页面。

## 2026-08-17 AcrylicFork 全量页面重构事实

- 本轮确认没有任何提示词保护；先前外观不变的根因是生产页面与 AcrylicFork Demo 使用两套不同的页面树，同时旧 Dashboard 外壳重复渲染页面上下文和局部表格样式。
- 生产 Shell 现在只负责侧栏、Header/GameSwitcher、全局操作、PageHost 和 Footer；首页、媒体、任务、存档、修改器、维护页面继续持有真实 ViewModel/Command/Binding，Demo 顶部颜色按钮只作为主题令牌参考，生产滚动条保持项目实现。
- 首页与维护中心已经按 Demo 信息架构迁移；维护中心默认诊断概览显示六项健康卡、环境检查、诊断操作和完整摘要，发现问题表格通过独立问题列表 Tab 保留并验证。
- DataGrid 共享样式采用透明表头、稳定底部分隔线和明确文本对比度；不使用负 Margin、Canvas、透明占位或隐藏溢出来修复布局。首页零分母进度条折叠，关注入口提供可访问说明。
- 2026-08-17 自动事实：源码校验通过；WPF UI 静态校验 0 error；Playnite 测试 303/303；RenderHarness 全量 render-qa OK。真实 Playnite 宿主、DPI、Follow/高对比度、键盘和大库滚动仍需人工复核。
- `scripts/package.ps1` 已修复空 `dotnet` 参数问题；当前无 `BuildOutputRoot` 的标准打包流程可正常生成并校验安装包，且会保留 `GameSaveCenter.Contracts.dll`。
- 一键安装的隔离构建还必须把 `TEMP/TMP` 指向隔离输出盘；否则完整性测试会读取系统临时目录所在磁盘的真实剩余空间，在低于 512 MiB 时把健康夹具判为 `Warning`。
- 一键安装的隔离构建根目录使用短路径 `artifacts/gsc-b/<guid>`；过深的 `artifacts/dev-build/Release/<guid>` 会让 .NET Framework Playnite 测试适配器加载失败。当前完整 `dev-install-run.ps1 -NoStart` 已通过。

## 2026-08-18 UI-214 首页状态徽标事实

- 首页最近任务与全局活动徽标的文本必须显式设置 `HorizontalAlignment=Center` 和 `TextAlignment=Center`，不能依赖旧 Chip/Border 模板的默认测量。
- 风险与提醒徽标使用独立 `Border + TextBlock`，最小宽度 52 DIP、内边距 8/2，并通过 DataTemplate triggers 同时切换背景、边框和文字颜色；这样“风险/需关注/未知/已就绪”不会被裁切或错误显示为同一种颜色。
- 任务图标应引用 `GscAccentBrush` 等生产资源键，不能直接引用 Acrylic Demo 的裸 `AccentBrush`。
- 2026-08-18 Release 构建 0 warning / 0 error；Overview 离屏浅深色多尺寸探针通过。全量 RenderHarness 仍有既有 Media resize recovery 失败，不能标记全量通过。
- 本轮未重新进行 Playnite 宿主截图；屏幕控制此前已由用户物理 Escape 停止，离屏渲染结果不得替代真实宿主人工验收。
- 本轮重复的 Playnite 测试命令长时间无输出并被停止，不能据此宣称新增测试通过；保留上一阶段已记录的回归基线。

## 2026-08-18 UI-225 首页状态徽标实际宿主事实

- 首页纯文本状态徽标使用页面局部 `DataTemplate` 居中，不修改共享 Chip 的复杂内容承载方式；否则 Worker/Ludusavi 的 StackPanel 内容会被错误显示为控件类型字符串。
- 风险与提醒徽标位于独立 Grid `Auto` 列，游戏名称允许省略并提供 Tooltip，风险文本保留 72 DIP 最小宽度；这解决了名称挤压徽标和“风险”中文裁切。
- 最新 v28 生产包已经重新安装并在真实 Playnite 中启动。宿主截图明确显示：最近任务的“成功”徽标文本居中，风险徽标文本完整，侧栏设置项可见。
- 本阶段的 RenderHarness 与宿主验证只覆盖首页徽标修复；全量 RenderHarness 的 Media resize recovery 失败和 Playnite 迁移前结构测试失败仍是公开的后续工作项。
-
## 2026-08-18 UI-224 首页徽标与真实宿主截图核验事实

- 首页活动/任务状态徽标不能依赖 `LabSubCard` 的内边距和默认测量；固定徽标必须使用零内边距、固定宽度、子 `TextBlock` 显式 `HorizontalAlignment=Center` 与 `TextAlignment=Center`。当前生产首页的任务/活动徽标宽度为 58 DIP，风险徽标为 70 DIP。
- 风险徽标必须保留 Tooltip，中文状态不能用省略号替代；背景、边框和文字颜色继续由真实健康状态触发器驱动。
- 真实 Playnite 验收的证据要求提高：必须同时有 Playnite 日志中的 `ExtensionFactory:Loaded plugin: GameSaveCenter`、可识别的 Playnite 页面截图和必要时的交互结果。若 Computer Use 返回 `EmptyWindowAutomationPeer`、`MainWindowHandle=0` 或截图是其他桌面窗口，只能记录为“宿主已加载，视觉验证阻塞”，不得写成视觉通过。
- 2026-08-18 本轮 Release 编译/打包/安装通过，日志确认生产 DLL `0.6.70.0` 加载；但 Computer Use 截图不属于 Playnite 页面，真实宿主视觉验收仍为 `MANUAL QA REQUIRED`。Worker 两项环境状态测试和 Media resize recovery 两项 RenderHarness 失败继续保持公开记录。

## 2026-08-18 UI-226 首页短主题别名事实

- 生产 Acrylic 共享控件与 AcrylicFork Demo 共用一组短资源键：`AccentBrush`、`AccentHoverBrush`、`AccentPressedBrush`、`AccentStrokeBrush`、`AccentTintBrush`、`AccentTintStrongBrush`、`AccentWashBrush` 和 `TextOnAccentBrush`。
- 生产主题适配必须同时写入这些短键和 `Gsc*` 键；只写 `Gsc*` 会让 Playnite 宿主的未解析短键回退为黑色/透明，表现为任务图标、进度条、活动气泡和主按钮与 Demo 色彩不一致。
- 短键只能写入当前页面的局部 `ResourceDictionary`，不能修改 Playnite 全局资源；这样既能复现 Demo 的强调色层级，又不会污染宿主主题。
- 2026-08-18 已确认生产页面的实际资源注入缺口：`DashboardView` 之前只更新旧隐藏页面树，现已同时更新 `AcrylicProductionShellView` 及其 `PageHost` 页面实例；离屏深色 RenderHarness PNG 已确认首页最近任务图标/进度条、`全部` 链接、风险主按钮、全局活动分类和信息气泡恢复紫色或对应语义色。
- Playnite 日志确认新 DLL 已加载，但 Computer Use 返回 `EmptyWindowAutomationPeer`、`MainWindowHandle=0` 且截图不是 Playnite 页面，因此真实宿主视觉验收仍为 `MANUAL QA REQUIRED`，不得把离屏 PNG 写成真实宿主截图通过。全量 RenderHarness 仍有 Media resize recovery 两项失败。

## 2026-08-18 UI-227 媒体中心模式栏事实

- 媒体中心顶部模式栏已从灰色透明条切换为生产深色 `MediaModeStrip`，外层使用 `GscGlassStrongBrush` 与 `GscControlStrokeBrush`；选中 RadioButton 使用 `GscAccentTintStrongBrush`、`GscAccentBrush` 和 `GscSelectionTextBrush`。
- RadioButton 的三种模式、真实数据绑定、命令和项目自身滚动条没有改变；本次只修正页面级 Tab 承载样式，并确保悬停不会覆盖选中态。
- Release 构建与静态检查通过；RenderHarness 编译和主题/尺寸探针完成，但 Media resize recovery 仍有两项失败：回弹后 `MediaGrid` 尺寸不一致、`MediaInspectorScrollViewer` 从可见变为折叠。真实 Playnite 截图仍待可识别宿主窗口后复核。

## 2026-08-18 UI-228 页面基线回退修复事实

- `be5707d` 是一次页面基线回退：它把生产页面覆盖成“今日工作台 / 最近活动”架构，导致两台同步仓库的电脑同时显示数个版本前的页面。
- 当前恢复目标是 `be5707d^` 的 AcrylicFork 生产基线，首页必须包含“最近任务 / 全局活动 / 风险与提醒”；顶部 Demo 彩色按钮和 Demo 滚动条仍不迁移。
- 针对已撤销架构的 61 条 Playnite 源码契约断言必须保持显式跳过，不能用无业务意义的兼容控件让它们假通过；当前基线由 `RestoredAcrylicForkBaselineTests` 覆盖首页、存档、媒体和任务入口。
- 本轮验证：Release 0 warning / 0 error，Core 59/59，Worker 191/191，Playnite 246 通过、61 跳过、0 失败。真实 Playnite 宿主视觉仍需可识别窗口截图确认。

## 2026-08-18 UI-229 媒体模式栏交付验证边界

- 一键 Release 构建、Worker 发布和 Playnite 安装已重新通过，安装目录为 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，`extension.yaml` 为 `0.6.70`，生产 DLL 为 `0.6.70.0`。
- RenderHarness 不是 Playnite 真机截图；本轮工具上下文没有可用的 Playnite 鼠标/键盘控制，因此没有把离屏结果冒充真实宿主视觉验收。
- 当前提交只交付媒体模式栏颜色修正和对应基线断言，不代表首页、存档、修改器、任务、维护等页面已经完成 1:1 视觉迁移；Media resize recovery 两项失败仍是后续阻塞项。

## 2026-08-18 UI-230 首页风险列表滚动边界事实

- 首页风险卡片的两个可能增长列表必须使用独立的生产 `GscPageScrollViewer`：需关注列表与展开后的最近游戏保护明细均限制为 `MaxHeight=190`，垂直滚动 `Auto`，水平滚动 `Disabled`。
- 页面根滚动与风险列表内部滚动职责分离：普通首页内容由页面滚动承载，风险项超过视口后只在自身区域滚动，不能通过追加列表项把主页面高度无限撑大。
- 当前 Dashboard ViewModel 的需关注数据按真实严重度筛选后完整绑定，首页不再用前 4 条静默截断；保护明细继续使用真实 `RecentProtection.Items`。190 DIP 是 UI 层防护边界，不是业务截断替代品。
- RenderHarness 默认 fixture 没有使需关注列表溢出，因此其 `scrollable=false` 只说明当前 fixture 未超过 190 DIP；静态契约测试已强制检查滚动边界。不得据此宣称已完成真实 Playnite 滚动条交互验收。
- 首页清理了已撤销的“今日工作台”旧工具栏及其代码后置响应式引用；媒体中心模式栏继续使用生产控件底色和紫色选中色，不迁移 Demo 顶部彩色按钮或 Demo 滚动条。

## 2026-08-19 UI-231 首页风险项数量边界事实

- 首页“风险与提醒”及其关联明细必须采用有限视口：最近游戏保护明细、需关注事项列表分别使用生产 `GscPageScrollViewer`，`MaxHeight=190`、垂直滚动 `Auto`、水平滚动 `Disabled`。
- 数量很多时，风险明细在卡片内部滚动，不能把 Dashboard 根内容高度无限撑长；风险卡片本身不再包一层整卡滚动，避免双滚动条。
- 该视口不替代业务数据：真实绑定仍保留，当前展示条数限制只由现有 ViewModel 业务规则和列表视口共同决定。
- 真实 Playnite 截图验收仍未完成：本轮插件已由日志确认加载，但控制接口返回 `EmptyWindowAutomationPeer` 且截图错指 Codex 窗口；后续不得将 RenderHarness 或错误窗口截图称为宿主视觉通过。

## 2026-08-19 UI-232 首页风险区域回归事实

- 首页风险列表的正确边界是列表级有限视口，而不是整张风险卡片或首页根容器无限增长：`OverviewAttentionScrollViewer` 与 `OverviewProtectionItemsScrollViewer` 均使用 `GscPageScrollViewer`、`MaxHeight=190`、垂直 `Auto`、水平 `Disabled`。
- 首页宽布局的右侧风险栏必须与 `OverviewRecentActivityCard` 同行并跨越最近任务/全局活动两行；RenderHarness 现在直接按该卡片比较，避免使用整页滚动面导致错误告警。
- 2026-08-19 RenderHarness 已确认高数量风险探针在 190 DIP 视口内滚动，宽布局右侧栏偏移为 `0 DIP`；这只是离屏渲染验证，不等于 Playnite 真机视觉验收。
- 本轮 Playnite 测试命令因长时间无输出和低 CPU 子进程空转被停止，不能写成测试通过；后续需使用可完成的测试入口重新验证。

## 2026-08-19 UI-233 首页风险列表与 Demo 行结构事实

- 首页风险区域的稳定方案是列表级有限视口：`OverviewAttentionScrollViewer` 和 `OverviewProtectionItemsScrollViewer` 使用项目 `GscPageScrollViewer`，`MaxHeight=190`、垂直 `Auto`、水平 `Disabled`。风险数量很多时只滚动列表内部，不能让风险项把首页根高度无限推长；不要再给整张风险卡片叠加一层滚动。
- RenderHarness 的溢出探针已经确认 190 DIP 视口在 1040×700、1100×720 下保持固定且 `scrollable=True`；普通 fixture 未溢出时的 `scrollable=False` 只能表示当时数据不足，不表示没有滚动配置。
- 首页最近任务和全局活动已按 Demo 行结构重排：任务的类型/游戏名、详情/进度、结果/时间分别分层；活动不再保留额外表头和图标列，分类徽标文本使用紫色主题令牌并显式居中。
- 媒体模式栏的外层表面使用 `GscAccentTintBrush`，内部选中状态使用更强的紫色令牌；顶部 Demo 彩色按钮、Demo 滚动条仍不迁移。
- 仅安装裸 .NET SDK 的机器可能缺少 Workload Resolver 目录；`scripts/build.ps1` 和 `scripts/render-qa.ps1` 通过 `MSBuildEnableWorkloadResolver=false` 兼容本项目的 .NET Framework/WPF 构建，不代表项目依赖任何 SDK Workload。
- 2026-08-19 完成一次可复现验证：Playnite UI 测试 248 通过、61 跳过、0 失败；Playnite 与 RenderHarness Release 编译 0 警告/0 错误；RenderHarness 全量 `render-qa OK`。真实 Playnite 宿主视觉截图仍未完成，离屏证据不能替代宿主验收。

## 2026-08-19 UI-236 风险视口和紫色状态验证事实

- 首页“风险与提醒”必须限制列表视口，而不是限制业务集合：`OverviewAttentionScrollViewer` 与 `OverviewProtectionItemsScrollViewer` 使用 `GscPageScrollViewer`、`MaxHeight=190`、垂直滚动 `Auto`、水平滚动 `Disabled`；数量增加时主页面高度保持稳定，列表内部出现项目现有滚动条。
- 首页最近任务、媒体来源和可下载版本的长文本使用有限 Grid 测量、`CharacterEllipsis` 和 Tooltip，防止标题挤压状态徽标、按钮或 Inspector。
- 媒体中心模式栏使用更明确的 `GscAccentTintStrongBrush` 紫色生产资源；该资源变更已同步源码契约测试，Demo 顶部颜色按钮和 Demo 滚动条仍未迁移。
- 重新编译测试后 Playnite 测试为 248 通过、61 跳过、0 失败；之前 38 项失败来自旧测试二进制，不是当前源码结果。
- RenderHarness `render-current3` 全量 `render-qa OK`，但仍属于离屏证据；如果屏幕控制返回 `EmptyWindowAutomationPeer` 或错误窗口，必须记录为宿主视觉阻塞，不能写成 Playnite 真机验收通过。

## 2026-08-19 UI-237 Release 安装事实和宿主视觉边界

- 当前 `main` 的 `37ab9a6` 已完成 Release 一键安装；安装目录为 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，`extension.yaml` 为 `0.6.70`，DLL 为 `0.6.70.0`。
- 本轮 Release 验证结果为 Core 59/59、Worker 191/191、Playnite 248 通过/61 跳过/0 失败；安装报告保存在 `artifacts/last-dev-install.txt`。
- 首页“风险与提醒”按列表级有限视口实现：两个风险列表使用项目 `GscPageScrollViewer`、`MaxHeight=190`、垂直 `Auto`、水平 `Disabled`。风险数量增加时只在列表内部滚动，不会无限增加首页高度。
- 真实 Playnite 截图验证仍未通过：Computer Use 唯一返回的窗口标题是 Playnite，但截图内容是其他桌面窗口。此类结果只能记为宿主视觉阻塞，绝不能宣称页面已在 Playnite 中 1:1 验收。

## 2026-08-19 UI-238 首页风险与提醒视口事实

- 首页“风险与提醒”现在有独立的 `OverviewRiskViewport`，使用生产 `GscPageScrollViewer`，最大高度为 `330 DIP`，垂直滚动 `Auto`、水平滚动 `Disabled`。风险数量增加时，首页主内容高度不再被风险条目无限撑大。
- 风险区标题、说明和底部“打开维护中心”按钮位于外层视口之外；展开的最近游戏保护明细仍保留 `OverviewProtectionItemsScrollViewer` 的 `190 DIP` 内部视口。前者限制整个风险提醒栏，后者限制展开明细列表，不是无意义地叠加两个相同滚动条。
- `OverviewRiskScrollViewer` 仍然是兼容 `Panel` 节点，真实 `AttentionFindings` 与 `RecentProtection.Items` 绑定不变；不要把兼容节点直接改成同名 `ScrollViewer`，否则会破坏响应式代码和源码契约测试。
- 2026-08-19 已用单节点测试入口完成 Playnite 249/61、Core 59/59、Worker 191/191；RenderHarness 全量 `render-qa OK`，但这些结果仍不能替代可识别 Playnite 窗口的真实宿主截图。

## 2026-08-19 UI-239 媒体中心结构基线事实

- 媒体中心 Demo 的顶部结构是四张独立指标卡，下面是共享紫色分段 Tab；不能把统计数字、模式 RadioButton 和 Tab 再混合到一个横向条带中。
- 当前生产 `MediaCenterView` 使用 `UniformGrid MediaSummaryPanel` 承载四张 `GscRedesignMetricBorder` 卡片，宽度按 4/2/1 列响应式重排；`MediaTabControl` 基于 `GscRedesignWorkspaceTabControl`，其选中项使用生产紫色强调令牌。
- `MediaModeStrip`、`MediaModeRadio`、`MediaContentTabs` 和 `OnMediaModeChecked` 已从生产媒体页移除；真实 Tab 内容、Binding、Command、虚拟化列表和项目滚动条保持不变。
- 2026-08-19 的 Release RenderHarness 已确认媒体三 Tab 在浅色/深色、多尺寸和缩放过渡下可渲染；离屏 PNG 只能证明结构和布局探针通过，不能替代 Playnite 真机截图。

## 2026-08-19 UI-240 首页风险展开态验证事实

- 首页风险侧栏固定为 410 DIP 宽；`OverviewRiskViewport` 根据窗口高度在 500–720 DIP 之间限制，风险总列表使用生产滚动条，避免风险数量把首页无限撑高。
- 展开“最近游戏保护明细”时，隐藏同一数据源的只读预览列表；明细列表使用独立 300–420 DIP 视口。明细卡片按游戏名、状态、换行说明、查看操作纵向布局，原有选择和命令 Binding 不变。
- 2026-08-19 在新安装的真实 Playnite 窗口 `10621340` 中，展开后重新滚动并获取新截图，确认重复预览已隐藏，完整明细卡片和“查看”操作可见且无重叠。该证据仅覆盖首页风险侧栏，不代表其他页面完成宿主视觉验收。

## 2026-08-19 UI-241 任务中心搜索栏事实

- 任务中心搜索输入区不再使用独立的“搜索任务…”标签列；提示文字与搜索图标在输入框内部，`TaskSearchTextBox` 仍绑定真实 `TaskSearchText`。
- 桌面布局让搜索区占据筛选栏剩余宽度；紧凑布局时搜索区独占第一行，状态、类型、刷新在第二行，避免再次出现输入框被压成窄条或控件重叠。
- 2026-08-19 Release 构建、Core 59/59、Worker 191/191、Playnite 250/61/0 通过；安装已成功。但最终宿主截图验证被用户物理 Escape 中止，不能把本轮写成 Playnite 视觉验收完成。

## 2026-08-19 UI-242 真实宿主搜索栏复核事实

- 已重新启动并绑定真实 Playnite 生产窗口，确认截图目标为生产 `GameSaveCenter`，不是 AcrylicFork Preview。
- 任务中心真实宿主截图确认：搜索提示和搜索图标位于 `TaskSearchTextBox` 内部，输入框占据筛选栏剩余宽度；状态、类型和刷新控件各自保持独立边界。
- `scripts/validate-source.py` 已按当前 `OverviewActivityList` 的真实 Grid/页面滚动宿主结构修正有限视口判断，避免静态门禁把合法布局误报为无限测量。
- 本次真实宿主证据只覆盖首页入口和任务中心搜索区；媒体、存档、修改器、维护和首页风险展开态仍必须逐页截图复核，不能把离屏 RenderHarness 或单页截图写成全量 1:1 完成。

## 2026-08-19 UI-243 当前视觉验收边界

- 任务搜索提示已经和输入框合并；媒体页局部 Tab 已恢复共享紫色分段样式；首页风险侧栏和保护明细使用有限视口，保护明细采用纵向可读卡片。
- 本轮 Release 安装和 Core/Worker/Playnite 测试通过，但 Computer Use 未取得可识别 Playnite 窗口；离屏渲染、安装清单和测试不能替代宿主视觉验收。
- 后续逐页截图必须在同一 Playnite 宿主同时打开生产扩展和 AcrylicFork Preview，分别记录窗口、页面、分辨率、主题和滚动位置；若截图目标不是 Playnite 页面，立即记为阻塞并释放控制。

## 2026-08-20 UI-244 表头前景与媒体摘要卡事实

- 最近任务和任务中心表头不能只设置 `Foreground`：WPF 的 `DataGridColumnHeader` 内容还可能通过 `TextElement.Foreground` 继承宿主默认黑色。共享表头、表头呈现器和任务局部表头现在同时显式绑定生产主题文本令牌，首页任务模板的标题、游戏名、详情和结果也有明确前景色。
- 媒体中心四张摘要卡使用共享 `GscRedesignMetricBorder`，统一采用紧凑内边距、72 DIP 最小高度、14 DIP 圆角和 24 号数字；卡片仍由 `UniformGrid` 等宽承载，不混入 Tab 或来源规则布局。
- 2026-08-20 RenderHarness 最终报告 `artifacts/ui-qa/phase-home-media-cards-final/render-qa-report.txt` 为 `render-qa OK`，覆盖浅色/深色、多窗口尺寸和回弹过渡；Core 59/59，Playnite 251 通过、61 跳过。该证据属于离屏渲染，不能替代可识别 Playnite 宿主的逐页截图。

## 2026-08-20 UI-254 设置页分类栏与任务页 Demo 骨架事实

- 生产设置入口文件是 `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettingsView.xaml`，不是 `Views/SettingsView.xaml`。当前结构必须保持 `SettingsWorkspace` 的 190 DIP 分类栏、16 DIP 间距和右侧 `SettingsScroller`；分类 ListBox 名称是 `SettingsSectionTabs`，事件是 `OnSettingsTabSelectionChanged`。
- 设置页五个可见面板分别是 `SettingsGeneralPanel`、`SettingsBackupPanel`、`SettingsAppearancePanel`、`SettingsAutomationPanel`、`SettingsMigrationPanel`。切换只改变 `Visibility`，不得把真实字段 Binding、Validation、Playnite 保存按钮语义或导入/导出命令移入 Mock 数据。
- 设置页常见 1040px 逻辑窗口仍使用左侧分类栏；`ApplyResponsiveLayout` 的极窄分支为 `layoutWidth < 560`，窄标题阈值为 `layoutWidth < 520`。常见窗口必须让右侧 `GscPageScrollViewer` 获得有限视口，不能让五项分类栏占满第一屏。
- RenderHarness 的 ListBox 分段入口发现规则同时接受名称以 `SegmentTabs` 结尾的迁移页和生产设置的 `SettingsSectionTabs`；设置布局探针验证五个 `ListBoxItem` 可见可测和右侧内容视口，不再查找旧 `SettingsHeaderScroller`/TabControl。
- 当前 `artifacts/ui-qa/task-settings-final/render-qa-report.txt` 为 `render-qa OK`；这是离屏证据。Playnite 生产宿主 Light/Dark、Follow、DPI、键盘焦点与逐页真实截图仍需单独人工验收。

## 2026-08-20 UI-255 共享工作区表格事实

- `Themes/Redesign.xaml` 的 `GscRedesignWorkspaceDataGrid` 是 Save/Media/Maintenance/Task 四个提取页的显式 LabGrid-like 行为基类，集中保护 `RowHeight`/`ColumnHeaderHeight`、FullRow 单选、列宽调整、排序、`VirtualizingPanel.ScrollUnit=Item`、Recycling、行/列虚拟化和 Auto 内部滚动。
- 页面样式可以继续覆盖 `RowStyle`、`ColumnHeaderStyle`、Background 和媒体专用表头，但不能恢复各页复制一整套表格行为 setter 的分叉模式；新增工作区表格应优先基于该 key，并补充源契约测试。
- `GscCodeFontFamily` 当前为 `Cascadia Mono, Consolas, Microsoft YaHei UI`；维护诊断摘要已使用该 token。业务 Expander 已统一采用 `GscDisclosureCard`，当前没有引入 Demo 滚动条。
- 新自动证据：`artifacts/ui-qa/shared-grid-contract-final/render-qa-report.txt` 为 `render-qa OK`；Release 0 warning/0 error；Core 59/59；Worker 190/190（排除 Soak）；Playnite 251 通过/61 跳过/0 失败；WPF validator 0 error/20 warnings/161 info。
- 本阶段仍不能声称真实 Playnite 宿主逐页验收完成；宿主截图、Follow/高对比度、DPI、键盘/UI Automation、真实长文案和大数据量滚动仍是后续人工边界。

## 2026-08-20 UI-256 共享按钮与反馈资源事实

- Dashboard 的 Toast/Dialog 视觉资源现在位于 `Themes/Redesign.xaml`：`GscRedesignFeedbackToastCard`、`GscRedesignFeedbackDialogCard`、对应遮罩和文字样式；页面代码只负责真实事件、状态、动画、计时器和完成结果。
- Dashboard 不再声明与 `DesignTokens.xaml` 重复的原生 `GscButtonBase`/`GscPrimaryButton` 模板；Toast 关闭/详情按钮和确认 Dialog 按钮复用全局按钮契约。页面级 `ui:Button` 继续使用 `GscWpfUiToolbarButton`、`GscWpfUiActionButton`、`GscWpfUiContextButton` 等共享语义样式，不能新增局部按钮模板解决单页问题。
- `UiNotificationRequested`、`UiConfirmationRequested`、`UiChoiceRequested` 的事件与安全完成逻辑保持不变；设置页导入报告/错误仍使用原生 `MessageBox`，这是为避免 Playnite 共享 Window 中 Window-wide WPF-UI host 冲突的有意边界。
- UI-256 自动证据：XAML 18 文件通过，源码门禁通过，Release 0 warning/0 error，Core 59/59，Worker 190/190（排除 Soak），Playnite 252/61/0，`artifacts/ui-qa/feedback-surfaces-final/render-qa-report.txt` 为 `render-qa OK`，WPF validator 0 error/20 warnings/161 info。
- 仍未完成真实 Playnite 宿主逐页视觉验收；不要把 RenderHarness 的反馈资源加载或 PNG 结果写成 Playnite Light/Dark/Follow、DPI、高对比度和键盘/UI Automation 已验收。

## 2026-08-20 UI-257 首页有限视口事实

- `OverviewStackScrollSurface` 保持现有生产页面滚动条和 `HorizontalScrollBarVisibility=Disabled`；`OverviewLayoutGrid` 必须绑定 `ViewportWidth` 并使用有限宽度，否则 WPF 的无限横向测量会让星号列按内容期望宽度增长，裁切当前游戏卡片和真实按钮。
- 当前首页响应式证据：`artifacts/ui-qa/overview-responsive-ui257/render-qa-report.txt`。1366×768 的 workspace 为 1042 DIP，Hero 为 506 DIP、当前游戏卡片为 x=520..1026，操作按钮高度均为 38 DIP；1600×900 同样无横向溢出。RenderHarness 仅是受控 WPF 证据，不等同 Playnite 嵌入视觉验收。
- UI-257 最终门禁：XAML 18/18；Release 0 警告/0 错误；Core 59/59；Worker 191/191；Playnite 256/318（62 跳过）；`validate-source.py` 通过；WPF 静态审查 0 error、20 warnings、146 info。
- 三次真实宿主审计均确认生产扩展 0.6.70.0 可加载并读取真实数据，最新受控证据位于 `artifacts/ui-host-audit-ui257-final`；但 Playnite 返回 `EmptyWindowAutomationPeer`，未能取得可识别的嵌入页面像素截图。不得把受控窗口截图写成 Playnite 1:1 完成，七页 Demo-first 总目标仍处于进行中。

## 2026-08-20 UI-258 生产宿主七页人工嵌入事实

- 本轮已在真实 Playnite 生产扩展 `GameSaveCenter 0.6.70.0` 中打开七个目标页面；生产壳标题为 `GameSaveCenter 生产版`，当前游戏为 `Bongo Cat`。这是真实嵌入窗口的人工 Computer Use 复核，不是离屏或受控窗口截图。
- 首页、存档、媒体、任务、修改器、维护均从生产壳左侧导航实际进入。首页的当前游戏卡片和操作按钮完整可见；存档的立即备份/全部备份与四个标签可见；媒体显示 30 项、5.76 MiB、待归类 4468 项；任务显示 50 条任务、0 运行中、16 需关注、34 今日完成；修改器显示 Wo Long 与 Yakuza 3 工具及右侧工具设置；维护显示进程映射和诊断页。
- Media Inbox 已实际进入并选中截图，独立 Inspector 滚动后可见预览、归类游戏 ComboBox、“确认归类”和“忽略并保留副本”。本轮不执行这些动作，因此没有改变真实数据。
- 设置通过 Playnite 游戏右键菜单的 `GameSaveCenter → 打开设置` 实际打开，显示 `GameSaveCenter 设置` 的“常规与目录”页面及 Worker、Ludusavi、存档目录字段；关闭时未保存更改。
- 自动审计事实仍不变：Playnite 主窗口的 UIAutomation 树是 `EmptyWindowAutomationPeer`，脚本没有 `summary.json` 的嵌入逐页像素证据；人工截图可证明真实页面能进入和关键控件可达，但不能替代自动门禁，也不能外推到其他 DPI、主题/Follow、高对比度或完整操作回归。

## 2026-08-20 UI-259 媒体收件箱共享虚拟化事实

- `MediaInboxGrid` 现在只保留页面需要的 `ScrollUnit=Item` 与顶部对齐，行/列虚拟化和 `VirtualizationMode=Recycling` 统一从 `GscRedesignWorkspaceDataGrid` 继承；禁止在媒体实例上恢复 `Standard` 或关闭列虚拟化。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 的 `Media-Inbox` 探针使用 60 项真实形状的 `MediaItemDto` 夹具，覆盖 287/311/337/353/419 DIP 视口与 0/25/50/75/100% 滚动位置，检查 Recycling、列虚拟化、首行无 phantom gap 与末行可达。
- UI-259 证据：`artifacts/ui-qa/media-virtualization-fix/render-qa-report.txt` 为 `render-qa OK`；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 256/318（62 跳过）；WPF validator 0 error、19 warnings、146 info。
- 该阶段未改变真实媒体绑定、Inspector 或归类/忽略/保留副本命令；真实宿主七页人工证据沿用 UI-258，不能把本轮离屏探针写成新的 Playnite 视觉截图。

## 2026-08-20 UI-260 存档页标题必须跟随真实当前游戏

- 生产壳 `AcrylicProductionShellView.xaml.cs` 不得保留 Demo 游戏名；存档页副标题必须由 `SelectedGame.Name` 生成，空选择使用“未选择游戏”。
- `UpdatePageHeader` 同时由工作区切换和 `DashboardViewModel.SelectedGame` 属性变更调用，保证当前游戏选择器改变后标题副文案不会滞后。
- UI-260 安装验证：XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 257/319（62 跳过）；定向契约 14/14。
- 真实 Playnite 修复前复核已捕获 `Bongo Cat` 选择器与 `Elden Ring` 存档副文案不一致；修复后安装已完成，但重启后的 Computer Use 窗口暂时不可捕获，因此不把修复后截图写成宿主像素证据。
- GSC-086 常规宿主滚动复核已完成（4468 条媒体收件箱数据，顶部/中部/底部/快速滚轮/返回顶部无白色空视口）；DPI、窗口缩放、Follow/高对比度、键盘焦点和真实业务操作仍是人工边界。

## 2026-08-20 UI-261 工作区 Tab 栏视觉例外

- Demo-first 视觉基准不覆盖生产页 Tab 栏：用户明确要求继续使用项目当前 Tab chrome，因为它比 Demo 的外层连续分段胶囊更合适；后续迁移不能把该页签视觉重新替换为 Demo 样式。
- `GscRedesignWorkspaceTabControl`/`GscRedesignWorkspaceTabItem` 已在共享 `Themes/Redesign.xaml` 中恢复项目原有的透明 header 带、横向 HeaderScrollViewer、11 DIP 独立圆角页签、选中强调色、焦点视觉和内部 8 DIP 防裁切槽。页面仍保留 Demo 的周边布局以及真实 TabControl/TabItem、内容 Stretch、绑定和命令。
- Save、Media、Maintenance 的顶层页签和维护页内部页签均通过共享契约；不要在单页 XAML 复制一套 TabControl 模板来绕开该例外。
- RenderHarness 的 `SnapshotLayoutMetrics` 对重复模板部件名按出现顺序添加 `#2` 等稳定后缀，解决维护页嵌套 TabControl 的合法同名 `HeaderScrollViewer` 导致 `ToDictionary` 重复键的问题。
- UI-261 证据：源码/XAML/差异检查通过，定向契约 15/15，`artifacts/ui-qa/project-tab-chrome-rollback/render-qa-report.txt` 为 `render-qa OK`；代表离屏截图已确认 Save/Media/Maintenance 的项目 Tab chrome。Tab 回滚后的完整安装也通过：Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过、安装 0.6.70/DLL 0.6.70.0；WPF validator 0 error、19 warnings、161 info。
- 真实宿主重装后的稳定前台截图仍缺失；不要将离屏证据扩写为 Playnite 1:1 验收。DPI、窗口缩放、Follow/高对比度、键盘焦点和真实备份/媒体操作仍是总目标边界。
