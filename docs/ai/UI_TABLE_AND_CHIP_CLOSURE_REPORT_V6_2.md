# GameSaveCenter UI Table and Chip Closure Report v6.2

> 来源：`GameSaveCenter_UI_Table_and_Chip_Fix_Pack_v6_2.zip`
> 范围：只做表格细节收口；`REMOVE=0`；未改 ViewModel 业务语义、Worker、IPC、Ludusavi、Rclone、备份恢复、安全机制；GamePicker HARD LOCK 未触碰。

## 1. 修改文件

- `src/GameSaveCenter.Playnite/Themes/Redesign.xaml`
- `src/GameSaveCenter.Playnite/Themes/WpfUiProduction.xaml`
- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`
- `src/GameSaveCenter.Playnite/Views/SaveCenterView.xaml`
- `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml`
- `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml.cs`
- `tests/GameSaveCenter.Playnite.Tests/OvernightClosureV6Tests.cs`
- `tests/GameSaveCenter.Playnite.Tests/UiLayoutRegressionTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`
- `tests/GameSaveCenter.RenderHarness/Program.cs`
- `scripts/capture-v6-2-shots.ps1`

## 2. 共享样式变更

- `GscRedesignContextPill`：CornerRadius `12 → 7`，Padding `10,5 → 11,5`，新增 `MinHeight=26`。
- `GscRedesignTableStatusPill`：CornerRadius `9 → 7`，Padding `8,3 → 10,3`；继承 Base 的 26 DIP 最小高度。
- 共享 `DataGridCell`：Padding `12,8 → 12,8,20,8`，所有表格最后一列内容右侧保留至少 20 DIP。
- 颜色全部继续走 `DynamicResource` / Design Token，未写死任何关键前景色或背景色。

## 3. 时间列与列宽

- Overview Global Activity 列模型从 `40|150|*|88|76|112` 调整为 `40|150|*|96|84|112`。
- 时间列 TextBlock 增加 `Margin="12,0,20,0"`，时间不再贴表格右边框。
- Summary 仍是唯一主伸缩列；Kind / Result 保持固定槽位；窄窗 chip 下移结构不变。
- 保留 `CreatedDisplay`、`KindDisplay`、`ResultDisplay`、`Summary` 等全部绑定。

## 4. 可视进度条

- `SaveCandidateGrid` 的“可信度”列由纯文本 `StringFormat=P0` 改为 `DataGridTemplateColumn`：
  - `ProgressBar Height=8 MinWidth=60 Maximum=1 Value={Binding Score, Mode=OneWay}`
  - 右侧百分比文本仍使用 `StringFormat=P0`
- 全项目扫描确认 Task / Overview 已有真实进度条；Settings 数值不是业务进度，本轮不改。
- 保留 `Score`、`StatusDisplay`、`Path`、`ReasonsDisplay` 等全部列与绑定，行高未被抬高。

## 5. Maintenance 2K/4K 撑满

- `MaintenanceView.xaml.cs` 移除 `tableViewportHeight` 上限；`FindingsGrid`、`MaintenanceDeviceGrid`、`MaintenanceAuditFindingsGrid`、`MaintenanceProcessGrid` 的 `MaxHeight` 改为 `double.PositiveInfinity`，保留 `MinHeight` 与 `Height=NaN`。
- `MaintenanceDeviceLayout` / `MaintenanceProcessLayout` 的 `VerticalAlignment` 从 `Top` 改为 `Stretch`，让 `*` 行真正吃到父级剩余高度。
- 保留 Inspector 堆叠、内部滚动、虚拟化与全部命令绑定。

| 页面 | 2K before | 2K after | 4K before | 4K after |
| --- | ---: | ---: | ---: | ---: |
| Diagnostics 问题列表 | 460/1200 = 0.38 | 1066/1200 = 0.89 | 460/1920 = 0.24 | 1786/1920 = 0.93 |
| Device 主表 | 460/1200 = 0.38 | 979/1200 = 0.82 | 460/1920 = 0.24 | 1699/1920 = 0.88 |
| Audit 发现的问题 | 460/1200 = 0.38 | 1054/1200 = 0.88 | 460/1920 = 0.24 | 1774/1920 = 0.92 |
| Process 主表 | 458/1200 = 0.38 | 1075/1200 = 0.90 | 460/1920 = 0.24 | 1795/1920 = 0.93 |

- before 值来自 v6 旧上限公式（`Math.Min(460, height*0.50)`）与 v6-final 2K render-qa；4K before 按同一公式推算。
- after 值来自 v6.2 render-qa（`artifacts/ui-qa/v6-2/render-qa-report.txt`）。
- 1040×700 仍保持最小视口：Findings/Audit 236 DIP，Device/Process 252+ DIP。

## 6. 验证结果

- `scripts/check-xaml.ps1`：13 个 XAML 文件通过。
- `python scripts/validate-source.py`：通过。
- `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：0 error。
- Playnite 测试：`263/263` 通过。
- render-qa：11 档窗口（含 3840×2160）+ Light/Dark 主题 + 连续缩放探针全绿。
- UI Audit：`artifacts/ui-audit/v6-2-final/AUDIT_SUMMARY.md`，HIGH 0、MEDIUM 0、失败路由 0，8 条滚动上下文均为 EXPECTED INFO。
- v6.2 截图：`artifacts/ui-qa/v6-2-shots/`，命令 `scripts/capture-v6-2-shots.ps1`。

## 7. 约束检查

- REMOVE：0。
- GamePicker：Dashboard 单实例共享控件，未复制、未替换、未改搜索/筛选/排序/持久化/图标/绑定/命令/下拉/虚拟化。
- 业务语义：未改 ViewModel、Worker、IPC、Ludusavi、Rclone、备份恢复、安全机制。
- 主题：全部沿用 DynamicResource / Design Token，未引入整页 Viewbox 或 ScaleTransform。

## 8. Commit

- `c58b359` feat: v6.2 chip, table padding, time column and progress bar closure
- `6a68a59` feat: v6.2 maintenance tables fill 2k/4k workspaces

真实 Playnite 宿主主题 / DPI / 连续缩放仍为 `MANUAL QA REQUIRED`。
