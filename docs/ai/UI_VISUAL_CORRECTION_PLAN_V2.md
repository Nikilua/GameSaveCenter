# GameSaveCenter UI Visual Correction Plan v2

> 来源：`GameSaveCenter_UI_Visual_Correction_Pack_v2.zip`
> 生成日期：2026-08-14
> 状态（2026-08-20）：本文件为历史收尾计划，已由 AcrylicFork Demo-first 总目标覆盖。真实生产功能、`REMOVE = 0` 和 GamePicker HARD LOCK 仍保留；后续视觉结构以 Demo 为准。

## P0 Overview：单页面滚动上下文

- 截图：`ReferenceScreenshots/07_overview_standard_full.png`、`08_overview_narrow_full.png`
- 当前节点：
  - `OverviewStackScrollSurface`：`OverviewView.xaml:54`（根 ScrollViewer）
  - `OverviewPrimaryScrollSurface`：`OverviewView.xaml:77`
  - `OverviewSecondaryScrollViewer`：`OverviewView.xaml:366`
  - `OverviewRiskScrollViewer`：`OverviewView.xaml:409`
  - `OverviewActivityList`：`OverviewView.xaml:246`
  - `OverviewActivityTimelineList`：`OverviewView.xaml:306`
- 当前滚动链：根页 Scroll → Primary Scroll → Risk/Secondary Scroll → Activity List/ItemsControl 内部 Scroll，存在多层纵向滚动。
- 精确改动：
  1. `OverviewStackScrollSurface` 始终保持 `VerticalScrollBarVisibility=Auto`，成为唯一页面纵向滚动所有者。
  2. `OverviewPrimaryScrollSurface` 保留名称但不再滚动：`VerticalScrollBarVisibility=Disabled`、`CanContentScroll=False`、无 `MaxHeight`，内容由根页自然延伸。
  3. `OverviewSecondaryScrollViewer` 同样改为 `Disabled`，移除 `MaxHeight`/`Auto`；宽屏右侧自然增高，不创建第二个滚动上下文。
  4. `OverviewRiskScrollViewer` 从 `ScrollViewer` 改为 `StackPanel`（保留名称），彻底删除风险卡内部滚动；`OverviewView.xaml.cs` 中相关 ScrollViewer API 改为 StackPanel/通用 `FrameworkElement`。
  5. `OverviewActivityList`：`ScrollViewer.VerticalScrollBarVisibility=Disabled`，移除 code-behind 的 `MaxHeight` 逻辑；最多 8 条由业务决定，高度自然。
  6. `OverviewActivityTimelineList`：移除 `MaxHeight=240` 与 `ScrollViewer.CanContentScroll=True`，设置 `ScrollViewer.VerticalScrollBarVisibility=Disabled`；活动行数由根页滚动承载。
- 操作：`RESTYLE / RESPONSIVE_MOVE`
- 保留：`OverviewTasks`/`Activities` 数据源、`SelectedTask`、全部 Command、ItemTemplate 内所有字段、虚拟化/Recycling。
- 预期尺寸：
  - 1440×900：根页滚动，主列内容完整；风险卡自然高度。
  - 1100×720 / 1040×700：根页滚动继续有效，无内部滚动竞争。
- 主题影响：无新增硬编码；滚动条继续走 `GscPageScrollViewer` 共享资源。
- Audit 断言：OV-001 / OV-002 / OV-003。

## P1 Overview：风险与提醒去内滚、去死空间

- 截图：`ReferenceScreenshots/02_overview_risk_dead_space.png`
- 当前节点：
  - 风险卡 Border：`OverviewView.xaml` “风险与提醒”区域（`OverviewSecondaryPanel` 内 Row 1 Card）
  - `OverviewRiskScrollViewer`：`OverviewView.xaml:409`
  - CTA：`OverviewView.xaml:465` `OpenAttentionCenterCommand`
- 当前滚动链：Card Grid Row1 使用 `*`，内部 `ScrollViewer Auto`，CTA 在 Row2 被空白推到底部。
- 精确改动：
  1. 风险卡 Grid 行全部 `Auto`；内部用 `StackPanel` 自然排列。
  2. CTA 紧跟最后一块内容，`HorizontalAlignment=Right`，不再 Stretch 全宽、不靠 `*` 占位。
  3. 为空/内容少时卡片自然缩短；不保留无业务理由的 `MinHeight`。
  4. 增加 `x:Name="OverviewRiskCard"` 供 OV-005 探针记录。
- 操作：`RESTYLE / RESPONSIVE_MOVE`
- 保留：`RecentProtection`、`AttentionFindings`、`OpenAttentionCenterCommand`、`OpenProtectionGamesCommand`、`ApplyRecommendedProtectionCommand`。
- 预期尺寸：1440×900 风险卡高度由内容决定；1040×700 无内部滚动。
- 主题影响：继续使用 `GscSafetyFillBrush`/`GscRedesignSubCard` 等 token。
- Audit 断言：OV-002 / OV-005。

## P2 Disclosure：专用 Expander 视觉

- 截图：`ReferenceScreenshots/03_overview_recent_protection_disclosure.png`
- 当前节点：
  - `GscExpander`：`DesignTokens.xaml:205`
  - Overview 使用点：`OverviewView.xaml:434` “展开最近游戏保护明细”
  - Maintenance 使用点：`MaintenanceView.xaml` “目录与日志 / 完整性、自愈与安全模式 / 元数据灾备”
- 精确改动：
  1. 新增共享 `GscDisclosureCardExpander`（局部专用，不覆盖全部 Expander）。
  2. Header 高 38～42 DIP，圆角 10～12，SurfaceSoft 背景 + 轻描边，Chevron 独立 22～26 DIP 图标区。
  3. Collapsed：ChevronRight；Expanded：ChevronDown；Header 整行可点；Hover 背景/描边轻变化；Focus 可见；Cursor=Hand。
  4. Expanded 时 Header 与内容之间有 separator/spacing；内容背景轻微变化；遵守 Reduce Motion。
  5. Overview 保护明细改用它；Maintenance 三个低频 Expander 改用它。
- 操作：`RESTYLE`
- 保留：全部 Header 文本、Content、Command、Binding。
- 预期尺寸：Header 稳定 38～42 DIP，不随内容膨胀。
- 主题影响：全部 DynamicResource，禁止 hardcode 白/黑/固定蓝灰。
- Audit 断言：OV-004 相关视觉由截图人工确认；新增 Disclosure 几何探针。

## P3 Overview：全局活动响应式行与边界

- 截图：`ReferenceScreenshots/01_overview_global_activity.png`
- 当前节点：`OverviewActivityTimelineList` 及其 ItemTemplate（`OverviewView.xaml:306` 起）
- 当前问题：四列 `38 | * MinWidth=220 | 110 | 92` 在窄窗硬挤，Main 被压后文本仍可能越界；无响应式换行。
- 精确改动：
  1. Wide/Standard：`42 | * | 110~126 | 104~116`，Main 列允许收缩，GameName/Summary `TextTrimming=CharacterEllipsis` + ToolTip。
  2. Compact/Narrow：通过 ItemsControl `Tag`（code-behind 设置 `"Compact"` / `"Wide"`）在 DataTemplate 内切换：Meta 列宽变 0，Kind/Result 移入 Main 下方第二行，时间列保留 88～96 DIP。
  3. 所有文本 `ToolTip` 保留完整值；不新增水平滚动。
  4. `OverviewActivityTimelineList` 不再自建纵向滚动。
- 操作：`RESPONSIVE_MOVE / RESTYLE`
- 保留：Glyph、GameName、Summary、KindDisplay、ResultDisplay、CreatedDisplay 全部字段。
- 预期尺寸：1440×900 四列；1040×700 三列响应式。
- 主题影响：无新硬编码。
- Audit 断言：OV-003 / OV-004。

## P4 Save：当前存档规则卡片与三按钮规格

- 截图：`ReferenceScreenshots/04_save_candidate_header_card.png`
- 当前节点：`SaveCenterView.xaml:160` 起（顶部 Border），按钮 `:181-183`。
- 当前问题：卡片过高、校验状态占大块、三按钮几何不一致。
- 精确改动：
  1. 顶部 Border 加 `x:Name="SaveCurrentRuleCard"`，Padding 收敛到 14～16。
  2. 布局改 `Auto icon | * identity | Auto status | Auto actions`；状态改紧凑 `GscRedesignContextPill` chip，不再使用大 InfoBand。
  3. 三个按钮统一 `MinHeight=38`、`MinWidth=104`、`VerticalAlignment=Center`、间距 8；Primary/Secondary 仅外观权重不同。
  4. Compact/Narrow：actions 移到第二行（`Grid.Row=1`），三按钮仍等高横排或 2+1 换行，禁止三行按钮墙。
- 操作：`RESTYLE / RESPONSIVE_MOVE`
- 保留：`DetectPathsCommand`、`ValidateCommand`、`LoadDetailsCommand`、`SaveCandidateGrid` 4 列与虚拟化。
- 预期尺寸：Standard/Wide 卡约 112～136 DIP；1040×700 不形成按钮墙，候选表仍 >=4 行。
- 主题影响：状态 chip 使用现有 token。
- Audit 断言：SAVE-001 / SAVE-002。

## P5 Maintenance / Diagnostics：删除父子双滚动

- 截图：`ReferenceScreenshots/05_maintenance_diagnostics_standard.png`、`06_maintenance_diagnostics_full_scroll.png`
- 当前节点：
  - `MaintenanceDiagnosticsScrollSurface`：`MaintenanceView.xaml:62`（外层 ScrollViewer）
  - `FindingsGrid`：`MaintenanceView.xaml:218`
  - `MaintenanceDiagnosticsInspector`：`MaintenanceView.xaml:220`
  - `EnvironmentCheckCard`：`MaintenanceView.xaml:66`
  - `MaintenanceDiagnosticsActionCard`：`MaintenanceView.xaml:99`
- 当前滚动链：外层页 Scroll → FindingsGrid 内 Scroll，形成父子双滚动。
- 精确改动：
  1. `MaintenanceDiagnosticsScrollSurface` 从 `ScrollViewer` 改为 `Grid`（保留名称），外层不再滚动。
  2. 页面结构：Row0 紧凑环境摘要；Row1 常用诊断工具栏；Row2 `*` 工作区（左 FindingsGrid，右 Inspector）。
  3. 环境检查压成紧凑摘要 + `首次环境检查 >` Disclosure，详细 ItemsControl 放入 Disclosure，保留全部检查项/按钮。
  4. 低频维护（目录与日志、完整性/安全模式、元数据灾备、批量路径迁移）移入始终可见的 Diagnostics Inspector Disclosure；Inspector 自身滚动，与 FindingsGrid 为 sibling scroll。
  5. 常用工具栏保留刷新/复制/导出/完整性等主按钮。
- 操作：`RESPONSIVE_MOVE / RESTYLE / COLLAPSE`
- 保留：FindingsGrid 全部列/命令、Inspector 详情、全部环境检查项与命令、元数据灾备/路径迁移字段与命令。
- 预期尺寸：1440×900 首屏可见 FindingsGrid Header + >=6 行；1040×700 首屏可见 Header + >=4 行；无外层滚动包表。
- 主题影响：沿用现有 Card/Expander/DataGrid token。
- Audit 断言：MAINT-001 / MAINT-002。

## P6 Maintenance / Audit：二级切换，单主表

- 当前节点：`MaintenanceAuditScrollSurface`（`MaintenanceView.xaml:461`）、`MaintenanceAuditFindingsGrid`、`MaintenanceAuditLogGrid`、`MaintenanceAuditInspector`
- 当前问题：Findings 表与 Audit 日志表上下堆叠，双表同时占高度。
- 精确改动：
  1. 外层 ScrollViewer 改为 `Grid`（保留名称），删除整页滚动。
  2. 内容顶部加二级 selector：`发现的问题` / `审计记录`（复用共享内部 Tab 或等价的 Segmented 选择器）。
  3. `发现的问题`：FindingsGrid + Inspector（sibling scroll）。
  4. `审计记录`：AuditLogGrid 单独主表。
  5. 同一时刻只有一张主 DataGrid 可见。
- 操作：`RESPONSIVE_MOVE / RESTYLE`
- 保留：两张表全部列/Binding/Command；空状态；Inspector。
- 预期尺寸：1440×900 / 1100×720 / 1040×700 均只有一张主表占 `*`。
- 主题影响：Header 修复沿用共享 DataGridColumnHeader token。
- Audit 断言：MAINT-003。

## P7 Audit 断言与测试更新

- 新增/更新 `RenderHarness` 与 UI Audit：
  - OV-001：Overview 页面级可滚 ScrollViewer 仅 `OverviewStackScrollSurface`。
  - OV-002：风险卡内部纵向可滚 ScrollViewer = 0。
  - OV-003：`OverviewActivityList`/`OverviewActivityTimelineList` 不产生独立纵向滚动。
  - OV-004：记录全局活动行 Main/Meta/Time 宽度与水平溢出。
  - OV-005：记录风险卡 ActualHeight/desired/content scroll count。
  - SAVE-001：记录 `SaveCurrentRuleCard` ActualHeight 阈值。
  - SAVE-002：记录三按钮 ActualHeight 差与顶部/底部对齐。
  - MAINT-001：`MaintenanceDiagnosticsScrollSurface` 不再作为 FindingsGrid 可滚 parent。
  - MAINT-002：1440×900 / 1100×720 / 1040×700 初始 viewport 内可见 FindingsGrid Header。
  - MAINT-003：Audit 页同一时刻可见主 DataGrid = 1。
- 分类：`EXPECTED_SIBLING_SCROLL`（表 + Inspector）、`POPUP_INTERNAL_SCROLL`；`TRUE_PARENT_CHILD_SCROLL_CONFLICT = 0`。
- 保留现有 10 档尺寸矩阵、56 主题场景、Resize 恢复探针；新增断言后全量重跑。

## 实施顺序

1. 共享 Disclosure style（P2）。
2. Overview 单滚动 + 风险卡 + 活动行（P0/P1/P3）。
3. Save 卡片（P4）。
4. Maintenance Diagnostics 重构（P5）。
5. Maintenance Audit 二级切换（P6）。
6. Audit/RenderHarness 断言（P7）。
7. 全量测试、render-qa、UI Audit、截图、文档与 commit。
