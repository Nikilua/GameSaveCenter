# GameSaveCenter UI Overnight Closure Report v6

> 生成日期：2026-08-15
> 依据：`GameSaveCenter_UI_Overnight_Closure_Pack_v6.zip`
> 原则：当前生产源码 > 用户最新要求 > 最新 UI Audit > v6 包 > 旧 v5/Demo；`REMOVE = 0`；GamePicker HARD LOCK；Tasks/Maintenance 不新增 GamePicker。

## 1. 修改文件

- `src/GameSaveCenter.Playnite/GameSaveCenterPlugin.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/DashboardView.xaml.cs`
- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`
- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml.cs`
- `src/GameSaveCenter.Playnite/Views/SaveCenterView.xaml`
- `src/GameSaveCenter.Playnite/Views/TaskCenterView.xaml`
- `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml`
- `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml`
- `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml.cs`
- `src/GameSaveCenter.Playnite/Themes/DesignTokens.xaml`
- `tests/GameSaveCenter.Playnite.Tests/UiStatePersistenceSourceTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/SessionNavigationStateTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/SettingsAndAutoSelectSourceTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/UiLayoutRegressionTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/OvernightV4SaveFormTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/OvernightClosureV6Tests.cs`
- `tests/GameSaveCenter.RenderHarness/Program.cs`
- `tests/GameSaveCenter.RenderHarness/FakeDashboardData.cs`
- `tests/GameSaveCenter.RenderHarness/UiAudit/UiLayoutAnalyzer.cs`
- `scripts/capture-v6-shots.ps1`
- `scripts/validate-source.py`

## 2. 明确未修改区域

- GamePicker：未删除/隐藏/复制，未改 Binding/Command/搜索/筛选/排序/自动定位/持久化/虚拟化；Tasks/Maintenance 继续保持不显示。
- ViewModel 业务语义、Worker、IPC、Ludusavi/Rclone、备份/恢复/PreRestore、媒体/修改器/任务逻辑、安全策略：未改。
- Trainer/Media/Task/Settings 的信息架构：未重排。

## 3. Navigation before/after

- Before：onboarding 未完成时启动强制 Maintenance；否则按持久化 `Settings.LastWorkspace` 恢复页面，跨 Playnite 重启仍停留旧页面。
- After：`GameSaveCenterPlugin` 持有进程级 `SessionLastWorkspace`；第一次打开 GSC 总是 Overview；同会话重开恢复会话内最后页面；Playnite 重启后重新从 Overview 开始。
- Onboarding：未完成时首页显示“首次环境检查尚未完成”CTA，点击 `OpenMaintenanceCommand` 显式进入 Maintenance；不再启动强制跳转。
- 自动验收：`SessionNavigationStateTests`、`UiStatePersistenceSourceTests`、`SettingsAndAutoSelectSourceTests`。

## 4. Numeric input root cause / fix

- Root cause：`GscTextBox` 模板的 `PART_ContentHost` 没有绑定 `VerticalContentAlignment`，数字框内文字未真正垂直居中，配合旧 padding 出现裁切。
- Fix：`PART_ContentHost` 增加 `VerticalContentAlignment="{TemplateBinding VerticalContentAlignment}"`；`GscNumericFieldInput` 使用 `Padding="8,4"`、`MinHeight="38"`、`TextAlignment="Center"`、`VerticalContentAlignment="Center"`，外框尺寸保持 112×42。
- 验收：v6 截图 `v6-numeric-{1,5,30,120,1440}.png` 均非空白且随数值增长；Playnite `261/261`。

## 5. Global Activity before/after

- Before：四列松散 ItemsControl，无 header，中间大面积空。
- After：轻量六列表格，header `对象/范围 | 事件 | 分类 | 结果 | 时间`；列 `40 | 150 | * | 88 | 76 | 112`；行高 52 DIP；图标完全居中；Summary 为主伸缩列；窄窗隐藏 header、chips 下移，字段不删。
- 验收：`UiLayoutRegressionTests.OverviewGlobalActivityUsesStableFourColumnRow` 更新为六列断言；v6 截图宽/窄。

## 6. Remaining scroll closure

- Overview：`OverviewPrimaryScrollSurface` / `OverviewSecondaryScrollViewer` 从 disabled ScrollViewer 改为普通 Grid；根页 `OverviewStackScrollSurface` 是唯一纵向滚动。
- Maintenance Device/Process、Media Current：外层 page ScrollViewer 改为有限 Grid，DataGrid/ListBox 与 Inspector 作为 sibling；`TRUE_PARENT_CHILD_SCROLL_CONFLICT = 0`。
- Audit 分类：`EXPECTED_SIBLING_SCROLL`（INFO）保留；真实父子冲突单独记为 HIGH。
- 验收：UI Audit `0 HIGH / 0 MEDIUM / 8 INFO`。

## 7. Proactive fixes

- Task/Media 筛选加语义前缀：`搜索任务…`、`状态:`、`游戏:`、`类型:`、`搜索当前游戏媒体…`、`类型:`。
- Maintenance Device/Process 主表最小视口提高到 252 DIP，避免窄窗只剩 3.7 行。
- 风险卡 padding/margin 收紧并减少渲染夹具 AttentionFindings 到生产同量；Audit 不再把自然内容高度误报为死空间。
- Maintenance header 继续使用共享 `GscDataGridColumnHeaderStyle` / `DataGridColumnHeadersPresenter` 主题，无白块。
- `scripts/validate-source.py` 更新为接受“根页滚动 + 内部滚动禁用”的新 Overview 架构。

## 8. Fidelity

- REMOVE：0
- old command missing：0
- old grid missing：0
- old column missing：0
- 虚拟化/Recycling：未退化（render-qa 探针全绿）

## 9. Audit before/after

- Before（v6 首轮）：HIGH 9（Device/Process 表格 3.7 行），INFO 24，含 `OV-005 RISK_DEAD_SPACE=826 DIP`。
- After：HIGH 0，MEDIUM 0，INFO 8（全部为 `EXPECTED_SIBLING_SCROLL`），`TRUE_PARENT_CHILD_SCROLL_CONFLICT = 0`。
- 汇总：`artifacts/ui-audit/v6/AUDIT_SUMMARY.md`

## 10. Theme / DPI matrix

- render-qa：10 档窗口尺寸全绿。
- 主题 QA：Light/Dark 各 4 尺寸 × 7 工作区共 56 场景全绿。
- Resize transition：2560×1440 → 1100×720 → 2560×1440 全恢复。
- 真实 Playnite 宿主 Light/Dark/Follow Playnite、100–200% DPI 仍需人工确认。

## 11. Screenshots

- `artifacts/ui-qa/v6-shots/v6-overview-standard.png`
- `artifacts/ui-qa/v6-shots/v6-overview-narrow.png`
- `artifacts/ui-qa/v6-shots/v6-overview-activity-wide.png`
- `artifacts/ui-qa/v6-shots/v6-overview-activity-narrow.png`
- `artifacts/ui-qa/v6-shots/v6-overview-risk-card.png`
- `artifacts/ui-qa/v6-shots/v6-overview-current-game.png`
- `artifacts/ui-qa/v6-shots/v6-save-automation-current.png`
- `artifacts/ui-qa/v6-shots/v6-save-automation-template.png`
- `artifacts/ui-qa/v6-shots/v6-numeric-1.png`
- `artifacts/ui-qa/v6-shots/v6-numeric-5.png`
- `artifacts/ui-qa/v6-shots/v6-numeric-30.png`
- `artifacts/ui-qa/v6-shots/v6-numeric-120.png`
- `artifacts/ui-qa/v6-shots/v6-numeric-1440.png`
- `artifacts/ui-qa/v6-shots/v6-maintenance-diagnostics.png`
- `artifacts/ui-qa/v6-shots/v6-maintenance-device.png`
- `artifacts/ui-qa/v6-shots/v6-maintenance-audit.png`
- `artifacts/ui-qa/v6-shots/v6-maintenance-process.png`

生成命令：`scripts/capture-v6-shots.ps1`。

## 12. Commit SHA

- `baa8f72` v6 计划
- 实施提交见最终 `git log`（Overview/Numeric/Save/Scroll/Filters/Audit 按功能块提交）
