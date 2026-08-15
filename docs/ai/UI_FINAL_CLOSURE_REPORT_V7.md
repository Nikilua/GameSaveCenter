# GameSaveCenter UI Final Closure Report v7

> 来源：`GameSaveCenter_UI_Final_Closure_Pack_v7.zip` + Audit 6 原始 ZIP（commit `570159c`）。
> 范围：最终结构性收尾；`REMOVE=0`；未改 ViewModel 业务语义、Worker、IPC、Ludusavi、Rclone、备份恢复、安全机制；GamePicker 与 Overview/Save Policy 锁定结构未动。

## Fixed

### 1. Audit 路由可信度
- before：`maintenance-tab0/tab1` 文件名与 TabHeader 是“问题列表/诊断概览”，截图和 layout JSON 实际是 `MaintenanceProcessGrid`。
- after：Audit 使用 `OuterTabIndex + InnerTabIndex + RouteSlug + ExpectedPrimaryElement` 子路由模型；截图前断言 expected == actual；失败路由 `0`。
- 路由文件名现在为 `maintenance-诊断-问题列表`、`maintenance-诊断-诊断概览`、`maintenance-设备状态`、`maintenance-异常与审计-发现的问题`、`maintenance-异常与审计-审计记录`、`maintenance-进程映射` 等。
- Settings 路由标题已解析为真实分类名，不再输出 `System.Windows.Controls.Grid`。

### 2. 宽屏列 Fill
- before（2K）：MaintenanceProcess 0.18、MediaInbox 0.33、SaveCandidate 0.41、MaintenanceAuditFindings 0.42、SaveHistory 0.45、Task 0.48；MaintenanceProcess 目标游戏只有 20 DIP。
- root cause：WPF DataGrid 星号列在滚动宿主/离屏测量下停在 MinWidth，星号分配不触发。
- fix：新增共享 `DataGridStarFill` 附加行为（`src/GameSaveCenter.Playnite/Infrastructure/DataGridStarFill.cs`），记录 XAML 星号权重，在 `SizeChanged/Loaded` 或 Audit 布局后按权重分配剩余宽度；共享 DataGrid 横向滚动改 `Disabled`；Save/Task 在 <1200 DIP 时 Inspector 收起让表格拿满整行。
- after（2K）：

| DataGrid | before | after |
| --- | ---: | ---: |
| MaintenanceProcessGrid | 0.18 | 1.00 |
| MediaInboxGrid | 0.33 | 1.00 |
| SaveCandidateGrid | 0.41 | 1.00 |
| MaintenanceAuditFindingsGrid | 0.42 | 1.00 |
| SaveHistoryGrid | 0.45 | 1.00 |
| TaskGrid | 0.48 | 1.00 |

- MaintenanceProcess 目标游戏：20 DIP → 1549 DIP（2K）。

### 3. 纵向 Workspace Stretch
- before（2K）：TaskGrid 426 DIP、MediaInboxGrid 460 DIP、MediaGrid 460 DIP，页面底部大面积空白。
- fix：Task 根 `TaskPageScrollSurface` 由 ScrollViewer 改为有限 Grid，`TaskWorkspaceLayout VerticalAlignment=Stretch`，TaskGrid `MaxHeight=PositiveInfinity`；Media Inbox 由 ScrollViewer 改为 Grid，Media Current `MediaCurrentLayout=Stretch`，MediaGrid `Height=NaN / MaxHeight=PositiveInfinity`。
- after（2K，真实 `*` 主行分母）：

| 页面 | before | after |
| --- | ---: | ---: |
| Task | 0.36（整页分母） | 1.00（主行分母） |
| Media Inbox | ~0.38（整页分母） | 1.00 |
| Media Current | ~0.38（整页分母） | 1.00 |

### 4. Maintenance 表头白块
- before：`MaintenanceAuditFindingsGrid` 最后一列 header 白块（2K 白色像素占比 0.43）。
- root cause：`GscLastColumnHeader` 别名带 `OverridesDefaultStyle=True`，在嵌套 Tab 作用域回退宿主白底。
- fix：删除该覆盖并统一改用 `MaintenanceLastColumnHeader`（XAML + code-behind `ApplyGridHeaderTheme`）。
- after：`HEADER_WHITE_BLOCK=0`；Dark/Light 由 DynamicResource token 控制。

### 5. Progress 可视对比
- before：fill/track 对比弱，5/100 视觉差异不明显。
- fix：新增 `GscProgressTrackBrush` / `GscProgressFillBrush`（DesignTokens + `AdaptiveThemePalette` 桥接）；共享 ProgressBar 模板补 `PART_Track` 命名，让 WPF 正确计算指示条宽度。
- probe（0/5/25/50/75/100）：fill 像素占比 `0.00 / 0.04 / 0.24 / 0.49 / 0.74 / 0.99`，肉眼可区分。

### 6. Settings 单行 TextBox
- before：`PART_ContentHost ScrollableHeight=4.67 DIP`（StorageNumericFields / AutomationIntervalFields）。
- fix：`GscTextBox` 模板 `PART_ContentHost VerticalAlignment=Stretch`，`GscNumericTextBox Padding=9,4`；`GscWpfUiTextBox` 模板补 `VerticalContentAlignment` 绑定。
- after：`SINGLE_LINE_CONTENTHOST_VERTICAL_SCROLL=0`。

### 7. Task narrow 滚动
- before：1040×700 `TaskPageScrollSurface` outer scroll 约 86 DIP，Audit 误标 `EXPECTED_SIBLING_SCROLL`。
- fix：Task 根改有限 Grid；Audit 分类改为“页面级 ScrollViewer 包 DataGrid 且内部可滚 = TRUE_PARENT_CHILD_SCROLL_CONFLICT”，内部 DG_ScrollViewer 记 `EXPECTED_INTERNAL_SCROLL`。
- after：standard 7.92 行、1100×720 4.17 行、1040×700 4.04 行；outer scroll=0；`TRUE_PARENT_CHILD_SCROLL_CONFLICT=0`。

## Explicitly unchanged

- Overview：TODAY hero、current game 卡、三按钮布局、Global Activity 轻量表方向、Risk card 结构。
- Save Backup Policy：当前表单、label/unit/helper、`GscNumericFieldInput`。
- Disclosure：统一 `GscDisclosureCard`。
- GamePicker：Tasks/Maintenance 不增加，本体不动。
- Navigation：v6 session-state 方案。
- 业务：ViewModel、Worker、IPC、Ludusavi、Rclone、备份恢复、安全机制；`REMOVE=0`。

## Theme / DPI

- render-qa Light/Dark 56 场景通过；Playnite Follow 模式由 DynamicResource/AdaptiveThemePalette 桥接。
- Audit metadata 仍为 `DpiScale=1.0`；真实 125/150% 与真 4K Windows scaling 标 `MANUAL_REQUIRED`，不假 PASS。

## Tests

- Playnite：`263/263`。
- render-qa：11 档窗口（含 3840×2160）+ 56 主题场景 + 7 Resize 全绿。
- UI Audit v7：0 HIGH / 0 MEDIUM / 0 失败路由；`COLUMN_FILL_TOO_LOW`、`VERTICAL_FILL_TOO_LOW`、`TRUE_PARENT_CHILD_SCROLL_CONFLICT`、`HEADER_WHITE_BLOCK`、`SINGLE_LINE_CONTENTHOST_VERTICAL_SCROLL` 均为 0。
- check-xaml / validate-source / validate_wpf_ui：通过（0 error）。
- Progress probe：`v7progress` 输出 `artifacts/ui-qa/v7-progress/`。

## Commit SHA

- `5cd0226` feat: v7 audit nested route traversal and scroll classification
- `58191d5` feat: v7 star column fill and wide table column sizing
- `494b402` feat: v7 task and media workspace vertical stretch
- `87d0553` feat: v7 maintenance header, progress contrast and single-line textbox closure
- `7eaaacd` feat: v7 progress track contract and progress probe

最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`（`artifacts/ui-audit/v7-final/`，Commit `7eaaacd`）。
