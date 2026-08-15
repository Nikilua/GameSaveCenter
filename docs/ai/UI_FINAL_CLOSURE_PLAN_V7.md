# GameSaveCenter UI Final Closure Plan v7

> 来源：`D:\Download\Brave\GameSaveCenter_UI_Final_Closure_Pack_v7.zip`
> 依据：当前生产 `main`（`570159c` 之后）、Audit 6 原始 ZIP（`artifacts/GameSaveCenter-ui-audit.zip`，Commit `570159c`）、项目 UI gate。
> 事实优先级：当前生产源码 > 用户最新要求 > Audit 6 截图/JSON > 本包 > 旧 prompt/demo。

## 0. 范围与锁定

- 本轮不做整体重设计；只收口 Audit 6 明确暴露的结构问题。
- `REMOVE=0`；不改 ViewModel 业务语义、Worker、IPC、Ludusavi、Rclone、备份恢复、安全机制。
- GamePicker 保持 v6 决定：Tasks / Maintenance 不新增；GamePicker 本体不动。
- Overview 今日工作台 / TODAY / 当前游戏 / 三按钮布局 / Global Activity 轻量表方向、Risk card 结构、Save Backup Policy 结构、现有 Disclosure 方向均只回归不推翻。
- 不使用整页 Viewbox / ScaleTransform；不按 2K/4K 写分辨率 if/else；不通过无限放大字号或行高填充空白。

## 1. Audit 路由与滚动分类（Phase 1，先修）

### Issue
Audit 6 `maintenance-tab0.png` / `maintenance-tab1.png` 文件名为“问题列表 / 诊断概览”，实际截图都是“进程映射”；
对应 layout JSON 的 DataGrid 是 `MaintenanceProcessGrid`，ParentChain 指向 `MaintenanceProcessScrollSurface`。

### 证据
- `.tmp/audit6/layout/2k/maintenance-tab0.json`、`tab1.json`：TabHeader 为“问题列表/诊断概览”，DataGrids[0].Name 均为 `MaintenanceProcessGrid`。
- 引用截图：`ReferenceScreenshots/01~08` 中 2K/standard 的 maintenance 截图。
- Audit Summary：failed routes 0、HIGH/MEDIUM 0，说明现有自动规则没有抓到这个路由错误。

### 根因
`tests/GameSaveCenter.RenderHarness/UiAudit/UiAuditRunner.cs` 的 `RenderRoute`：
1. 用 `FindVisualChildren<TabControl>(host)` 扁平遍历全部 TabControl，外层 Maintenance TabControl 和嵌套 Diagnostics/Audit TabControl 共用同一套 `tab0..tab4` 文件名；
2. 截图前没有把“外层 SelectedIndex + 内层 SelectedIndex”作为同一个 route 状态切换，导致内层 tab 渲染时外层仍停留在“进程映射”；
3. 没有 expected primary grid vs actual primary grid 断言，错误截图仍被当作成功结果保存。

### 精确修改（只改 Audit 工具，不改生产页面）
- `UiAuditModels.cs`：`UiRuntimeRoute` 增加 `List<UiRuntimeTabRoute> Tabs`，其中 `UiRuntimeTabRoute` 记录 `OuterTabIndex / InnerTabIndex / DisplayHeader / RouteSlug / ExpectedPrimaryElement`（如 `FindingsGrid`、`MaintenanceDeviceGrid`、`MaintenanceAuditFindingsGrid`、`MaintenanceAuditLogGrid`、`MaintenanceProcessGrid`）。
- `UiAuditRunner.cs`：对每个外层 Tab 先设置 `outerTabControl.SelectedIndex`；再在外层选中内容里找嵌套 TabControl，逐个设置内层 SelectedIndex；用 `maintenance-diagnostics-issues` 这类唯一 slug 命名截图/layout/visual-tree；截图前检查 `ExpectedPrimaryElement` 是否可见，不一致则记录 `ROUTE_EXPECTED_ACTUAL_MISMATCH` 到 FailedRoutes，不再保存“看起来成功”的截图。
- Settings 路由：`TabItem.Header` 取不到真实文字时，从 header 视觉树找第一个可读 TextBlock，fallback 到静态 manifest 标题（常规与目录 / 备份与恢复 / 外观与可访问性 / 自动化与媒体 / 设置迁移），禁止输出 `System.Windows.Controls.Grid`。
- `UiLayoutAnalyzer.cs`：滚动分类修正。页面级 ScrollViewer（非 DataGrid/ListBox 内部）`ContainsDataGridOrListBox=true` 且纵向可滚时，必须记为 `TRUE_PARENT_CHILD_SCROLL_CONFLICT`（HIGH）；只有 DataGrid 内部 DG_ScrollViewer 与旁边 Inspector 才允许 `EXPECTED_SIBLING_SCROLL`。

### 保留
Audit 工具不改任何生产 ViewModel / 页面；生产命令、绑定、虚拟化不受影响。

### 预期
- Audit 路由 map 出现 9 条维护中心真实子路由：`diagnostics/issues`、`diagnostics/overview`、`device`、`retention/...`、`audit/findings`、`audit/log`、`process`。
- `expected primary grid == actual primary grid` 全部成立；FailedRoutes=0。
- Settings 路由标题全部为真实分类名。

### Audit 断言
- 新增 code：`ROUTE_EXPECTED_ACTUAL_MISMATCH`（HIGH）。
- Task/Media 父子滚动：`TRUE_PARENT_CHILD_SCROLL_CONFLICT=0`。

## 2. 宽屏 DataGrid 列 Fill（Phase 2）

### Issue
Audit 6 2K（logical 2236×1200）列宽总和远小于 DataGrid 宽度，右侧出现大片空白：

| DataGrid | Grid Width | Columns Sum | Fill Ratio |
| --- | ---: | ---: | ---: |
| MaintenanceProcessGrid | 1859 | 330 | 0.18 |
| MediaInboxGrid | 2229 | 737 | 0.33 |
| SaveCandidateGrid | 1859 | 770 | 0.41 |
| MaintenanceAuditFindingsGrid | 1859 | 782 | 0.42 |
| SaveHistoryGrid | 1859 | 828 | 0.45 |
| TaskGrid | 1855 | 888 | 0.48 |

所有星号列运行时都停在 MinWidth；`MaintenanceProcessGrid` 的“目标游戏”`Width="*"` 运行时只有 20 DIP。

### 根因（实施时先做诊断探针再定最终改法）
- 共享 DataGrid 样式：`ScrollViewer.HorizontalScrollBarVisibility="Auto"` + `EnableColumnVirtualization="True"`；星号列还带 `MaxWidth="10000"`。
- WPF DataGrid 在有限 viewport 内，星号列首次测量/虚拟化时可能按 MinWidth/desired width 计算，之后不再重新分配剩余宽度。
- 需要先用探针确认是 `HorizontalScrollBarVisibility=Auto`、`EnableColumnVirtualization`、`MaxWidth=10000` 三者中的哪一个组合导致，再实施。

### 精确修改方向
- 共享 `DataGrid` 样式（`WpfUiProduction.xaml` 约 285-320 行）：`ScrollViewer.HorizontalScrollBarVisibility` 改为 `Disabled`（DataGrid 内部列超宽时靠 TextTrimming/Responsive 处理；不再默认开启横向滚动），保留 `VerticalScrollBarVisibility=Auto`、`CanContentScroll=True`、行虚拟化 Recycled。
- 星号列统一规则：metadata 列固定/Auto；primary content 列 `*`；secondary content 列按权重 `0.8*` / `1.2*`；action 固定；移除 `MaxWidth="10000"`，必要时给内容列合理上限（如 900）而不是让星号退化为 MinWidth。
- 各表精确列策略：
  - `SaveHistoryGrid`：时间 150 / 类型 110 / 文件数 82 / 大小 90 / 设备 120 / 状态 96；备注 `*` MinWidth 180（主伸缩）。
  - `SaveCandidateGrid`：可信度 130 / 状态 90；路径 `*` MinWidth 300；依据 `1.2*` MinWidth 250，两者共同吃满剩余宽度。
  - `MediaInboxGrid`：拍摄时间 110 / 类型 72 / 来源 115；文件 `*` MinWidth 220；原因 `1.1*` MinWidth 220。
  - `TaskGrid`：本地时间 128 / 任务 110 / 状态 110 / 进度 160；游戏 `1.25*` MinWidth 140；详情 `1.6*` MinWidth 240。
  - `MaintenanceAuditFindingsGrid`：等级 92 / 游戏 180 / 标题 190；详情 `*` MinWidth 320。
  - `MaintenanceProcessGrid`：EXE 220 / 操作 90；“目标游戏”`*` 且 `MinWidth=160`（不能只剩 20 DIP）。
- 代码后置检查：确认 `MaintenanceView.xaml.cs` / `TaskCenterView.xaml.cs` / `MediaCenterView.xaml.cs` 没有把星号列改回像素宽或设置 `DataGrid.ColumnWidth`。

### 保留
全部列绑定、HeaderStyle、命令、排序、虚拟化、TextTrimming/Tooltip。

### 预期
- Wide/2K/4K：`ColumnFillRatio >= 0.90`。
- Standard：`>= 0.88`。
- Narrow：允许响应式换行/Inspector 让位，不强制水平滚动。
- MaintenanceProcess 目标游戏：Standard 也至少 160 DIP，2K 吃满大部分剩余宽度。

### Audit 断言
- `UiRuntimeDataGrid` 增加 `ColumnFillRatio`（`sum(column.ActualWidth) / usable viewport width`，扣除 vertical scrollbar / row header）。
- 新增 code：`COLUMN_FILL_TOO_LOW`（HIGH），低于阈值即失败。

## 3. 纵向 Workspace Stretch（Phase 3）

### Issue
Audit 6 2K：`TaskGrid` 426 DIP、`MediaInboxGrid` 460 DIP、`MediaGrid` 460 DIP，工作区 1200 DIP；`SaveHistoryGrid` 1125、`SaveCandidateGrid` 1039、Maintenance 主表 1054+，说明不是所有页面都吃满高度。

### 根因
- Task：根节点仍是 `TaskPageScrollSurface`（ScrollViewer），内部 `TaskWorkspaceLayout VerticalAlignment="Top"`，且 `TaskCenterView.xaml.cs` 给 `TaskGrid.MaxHeight = tableViewportHeight`（上限 460/280）。
- Media Inbox：`MediaInboxScrollSurface`（ScrollViewer）包着固定 `MediaInboxGrid.Height = tableViewportHeight`（上限 460）。
- Media Current：外层已是 Grid，但 `MediaCurrentLayout VerticalAlignment="Top"`，且 `MediaGrid.Height = tableViewportHeight`；ListBox `VerticalAlignment="Top"`。

### 精确修改
- Task：根 `ScrollViewer` 改为有限 `Grid`（保留 `x:Name="TaskPageScrollSurface"` 兼容代码/测试，类型变 Grid），行结构 `Auto / Auto / * / Auto`；`TaskWorkspaceLayout VerticalAlignment="Stretch"`；`TaskGrid.MaxHeight=PositiveInfinity`、`Height=NaN`、`MinHeight=236/252`；`TaskDetailScrollViewer` 只在 stack 模式占 Auto 行并保持内部滚动。
- Media Inbox：`MediaInboxScrollSurface` 由 ScrollViewer 改为 Grid，行 `Auto / * / Auto`；`MediaInboxGrid.Height=NaN`、`MaxHeight=PositiveInfinity`、`MinHeight=236`；底部操作行留在 Auto 行。
- Media Current：`MediaCurrentLayout VerticalAlignment="Stretch"`；`MediaGrid`/ListBox `VerticalAlignment="Stretch"`；删除 `MediaGrid.Height=tableViewportHeight` 上限，保留 `MinHeight=236`。
- Maintenance：维持 v6.1/v6.2 结果，不回归。

### 保留
Summary 卡、筛选/操作行、Inspector 堆叠逻辑、全部命令绑定、虚拟化/Recycling。

### 预期
- 2K/4K：Task / Media Inbox / Media Current 主表 `VerticalFillRatio >= 0.92`（分母为真正 MainWorkspace 容器）。
- Standard：主表仍能展示 >=4 行；1040×700 不退化。

### Audit 断言
- `UiLayoutReport` 增加 `WorkspaceHeight / MainListHeight / VerticalFillRatio / TopExternalGap / BottomExternalGap`。
- 新增 code：`VERTICAL_FILL_TOO_LOW`（HIGH）。

## 4. Maintenance Header 白色块 + Progress 对比 + TextBox 指标（Phase 4）

### 4.1 白色 Header 块（Blocker）
- 证据：`ReferenceScreenshots/04_2k_...white_header...png`、`05_standard_...white_header.png`；Audit 6 `MaintenanceAuditFindingsGrid` 列只占 782/1859。
- 根因候选：星号列停在 MinWidth，生成大量 filler header；`DataGridColumnHeadersPresenter` 的共享样式在 Playnite 宿主下仍可能被生成的 filler header 绕过；`MaintenanceView.xaml` 里 `MaintenanceLastColumnHeader` / `GscLastColumnHeader` 的 `OverridesDefaultStyle` 需要逐列确认。
- 修改：先修列 Fill（第 2 节），消除大部分空白；再为 `DataGridColumnHeadersPresenter` 提供明确的 keyed style（`GscDataGridColumnHeadersPresenterStyle`），`Background=GscTableHeaderBrush`、`Foreground=GscPrimaryTextBrush`，在 `WpfUiProduction.xaml` 共享 DataGrid 样式和 `MaintenanceDataGrid` 上显式引用；所有列 HeaderStyle（含最后一列）统一 Background/Border；禁止 hardcode White/Black。
- 预期：Dark / Light / Follow Playnite 均无白块、无 blank header；2K/standard 截图右侧 header 与表格同色。
- Audit：新增 `HEADER_WHITE_BLOCK` 检查，采样 header 右端空白区像素，若与 `GscTableHeaderBrush` 差异过大即 HIGH。

### 4.2 ProgressBar 可视对比
- 证据：`ReferenceScreenshots/07_standard_task_progress_low_contrast.png`、`08_standard_save_candidate_progress_low_contrast.png`；共享 `ProgressBar` fill 用 `GscAccentBrush`、track 用 `GscControlFillBrush`，对比不足。
- 修改：DesignTokens 新增 `GscProgressTrackBrush` / `GscProgressFillBrush`；共享 ProgressBar `Background=GscProgressTrackBrush`、`Foreground=GscProgressFillBrush`（保留圆角 4、高度由模板决定，表格内保持 6-8 DIP）；Task/SaveCandidate 模板继续绑定 `ProgressPercent` / `Score`，不改业务；验证 normal / hover / selected / disabled 下 fill 不被 selection 背景吞掉。
- 预期：5% 明显短、50% 明显半条、100% 明显满格；Dark/Light 均清晰。
- Audit：新增 progress probe（0/5/25/50/75/100 六档）渲染 `v7-progress-probe.png`；自动采样 fill/track 亮度差，无法自动判断时至少产出截图供人工验收。

### 4.3 Settings 单行 TextBox
- 证据：Audit 6 `StorageNumericFields` / `AutomationIntervalFields` 的 `PART_ContentHost`：ActualHeight ~25.33、Viewport ~11.33、Extent ~16、Scrollable ~4.67。
- 根因候选：`GscWpfUiTextBoxTemplate` / `GscTextBox` 模板的 `PART_ContentHost` 对单行输入缺少明确 vertical center + 纵向滚动禁用；`GscNumericTextBox` 继承的模板指标不健康。
- 修改：先确认这些字段实际命中的模板；共享模板给 `PART_ContentHost` 增加 `VerticalContentAlignment="{TemplateBinding VerticalContentAlignment}"`；对 `AcceptsReturn=False` 的单行输入设置 `VerticalScrollBarVisibility="Disabled"`；`GscNumericTextBox` 局部语义样式保留；多行输入（备注/日志）不改变。
- 预期：`ScrollableHeight=0`，数字/文本完整、caret/IME 正常，100/125/150% 均不裁切。
- Audit：新增 `SINGLE_LINE_CONTENTHOST_VERTICAL_SCROLL`（AcceptsReturn=False 且 ScrollableHeight>0.5 即 MEDIUM/HIGH）。

## 5. Task 1040×700 外层滚动（Phase 5）

- 证据：TaskPageScrollSurface scrollable：standard 22、1100×720 23、1040×700 86；Audit 将链路标为 `EXPECTED_SIBLING_SCROLL`，实际是 `TaskPageScrollSurface -> TaskWorkspaceLayout -> TaskGrid -> DG_ScrollViewer` 父子链。
- 布局修复 = 第 3 节 Task 结构（有限 Grid + `*` 主行）。
- 让位顺序：缩小 summary card padding/间距 → 低频筛选进“更多筛选” → Inspector 收起 → 保证 TaskGrid >=4 行；不先开页面滚动。
- Audit 分类修复 = 第 1 节；最终 `TRUE_PARENT_CHILD_SCROLL_CONFLICT=0`，Task outer scroll = 0。

## 6. 锁定/通过区域（不改）

- Overview：TODAY hero、current game 卡、三按钮布局、Global Activity 轻量表方向、Risk card 结构。
- Save Backup Policy：当前表单、label/unit/helper、`GscNumericFieldInput`。
- Disclosure：统一 `GscDisclosureCard`，不发明第四套折叠样式。
- GamePicker：Tasks/Maintenance 不增加，本体不动。
- Navigation：v6 session-state 方案只验证不重写。

## 7. 全量验证与交付

1. `scripts/check-xaml.ps1`、`python scripts/validate-source.py`、`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`
2. Release 构建 + Playnite 测试（基线 `263/263`，新增 v7 断言后更新基线）。
3. render-qa：保留 1040/1100/1366/1600/2560，扩展 3840×2160；新增 ColumnFillRatio、VerticalFillRatio、Progress probe、TextBox ContentHost probe。
4. `capture-ui-audit.ps1`：按新 nested route 模型重新生成，必须 `expected == actual`、FailedRoutes=0。
5. 截图：`artifacts/ui-qa/v7-shots/` 至少覆盖：
   - 6 张 DataGrid 2K 列 Fill（Save History / Save Candidate / Media Inbox / Task / Maintenance Audit / Maintenance Process）
   - Task / Media Inbox / Media Current 2K+4K 纵向 Stretch
   - Maintenance Audit Dark/Light header
   - Progress 0/5/25/50/75/100
   - Settings 单行 TextBox 100/125/150%
6. 文档：`docs/ai/UI_FINAL_CLOSURE_REPORT_V7.md` 必须列 Fixed / Explicitly unchanged / Column fill before-after / Vertical fill before-after / Audit route before-after / Header white-block closure / Progress contrast / TextBox metrics / Task narrow / Theme/DPI / Tests / Commit SHA。
7. Audit 6 与 v7 的 DPI 证据：若本机只有 1.0 DPI，则 125/150% 与真实 4K 标 `MANUAL_REQUIRED`，不假 PASS。

## 8. 实施顺序

Phase 1 Audit 路由 → Phase 2 列 Fill → Phase 3 纵向 Stretch → Phase 4 blockers → Phase 5 Task narrow → Phase 6 全量回归/截图/Audit/文档。

每个 Phase 独立 commit；每个 Phase 后跑 XAML/source/WPF 门禁 + Playnite 测试 + render-qa，再进入下一 Phase。
