# GameSaveCenter UI Overnight Fix Report v4

> 生成日期：2026-08-15
> 依据：`GameSaveCenter_UI_Overnight_Fix_Pack_v4.zip`
> 原则：功能零损失；GamePicker HARD LOCK；`REMOVE = 0`；按功能块 commit 并 push。

## 1. 修改文件

- `src/GameSaveCenter.Playnite/Themes/DesignTokens.xaml`
- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`
- `src/GameSaveCenter.Playnite/Views/SaveCenterView.xaml`
- `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml`
- `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml.cs`
- `tests/GameSaveCenter.Playnite.Tests/UiLayoutRegressionTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/OvernightV4SharedTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/OvernightV4SaveFormTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/OvernightV4MaintenanceTests.cs`
- `tests/GameSaveCenter.RenderHarness/Program.cs`
- `scripts/capture-v4-shots.ps1`

## 2. 明确未修改区域

- GamePicker：未删除、未隐藏、未复制、未改 Binding/Command/搜索/筛选/排序/定位/持久化/虚拟化。
- ViewModel、Worker、IPC、备份/恢复/预恢复、媒体/修改器/任务业务语义、持久化与安全策略：未改。
- Dashboard 导航、Trainer/Media/Task/Settings 的信息架构：未重排。

## 3. 问题闭环

### A. 全项目折叠组件统一

- `GscDisclosureCard` 模板升级：独立 26 DIP chevron 图标区、垂直居中、Hover/Expanded 状态强化、无尾部 `>`。
- 所有 Expander 均引用 `GscDisclosureCard`；页面不再引用旧 `GscExpander`。
- 新增测试 `ExpandableCardsUseUnifiedDisclosureChromeWithoutInnerScroll`，锁定“无尾部符号、无折叠体内滚动”。

### B. 存档中心备份自动化表单

- `SaveBackupAutomationCard` 每个数值输入都有 label、输入框、单位、helper。
- 策略模板四个数值输入同样补齐 label/unit/helper 与 AutomationProperties。
- 新增 `GscFormFieldLabel`、`GscFormFieldHelper`、`GscNumericFieldInput` 共享样式。
- 深色主题下数字沿用 `GscTextBox` 的 `GscPrimaryTextBrush`，对比度不依赖局部颜色。

### C. 首页全局活动

- 行高收敛到 60 DIP；图标 32×32 且垂直/水平居中。
- 列宽调整为 `40 | * MinWidth=120 | Auto MaxWidth=180 | 112`。
- 时间列固定 112 DIP，完整显示 `MM-dd HH:mm`；窄窗 chips 继续下移到摘要下方。

### D. 维护中心折叠不内滚

- 删除 `EnvironmentCheckDisclosureScroller` 与 `MaintenanceActionsDisclosureScroller`。
- 折叠内容自然撑开；`诊断概览` 子页使用页面级 `MaintenanceDiagnosticsOverviewScrollSurface` 共享滚动上下文。

### E. 维护中心主表可读性

- 诊断 Tab 引入二级 Tab：默认 `问题列表`，次项 `诊断概览`。
- `问题列表` 让 FindingsGrid 独占有限工作区，五列保持 72/120/160/*180/140。
- 完整诊断摘要移到 `诊断概览`，不再挤压主表；Inspector 仍在表格右侧或窄窗堆叠。
- 默认态 FindingsGrid 首屏可见，render-qa 显示 8 行数据、236+ DIP 视口。

### F. 主动自查

- 折叠 header：全部统一，无 `>`，无内滚。
- 数值输入：存档中心与策略模板已补 label/helper；Trainer 启动延迟此前已有 label。
- 按钮组：render-qa 保留 Overview/Save 三按钮 Y 坐标与高度差探针，全尺寸通过。
- 状态 badge：v3 已统一，v4 未回归。
- 表格滚动条：FindingsGrid 滚动条位于表格内部，UI Audit 无 HIGH/MEDIUM。
- 深色主题：56 个 Light/Dark 场景全部通过。

## 4. 截图路径

- `artifacts/ui-qa/v4-shots/v4-overview-current-game-standard.png`
- `artifacts/ui-qa/v4-shots/v4-overview-protection-collapsed.png`
- `artifacts/ui-qa/v4-shots/v4-overview-protection-expanded.png`
- `artifacts/ui-qa/v4-shots/v4-overview-activity-wide.png`
- `artifacts/ui-qa/v4-shots/v4-overview-activity-narrow.png`
- `artifacts/ui-qa/v4-shots/v4-save-rule-standard.png`
- `artifacts/ui-qa/v4-shots/v4-save-automation-standard.png`
- `artifacts/ui-qa/v4-shots/v4-save-automation-narrow.png`
- `artifacts/ui-qa/v4-shots/v4-maintenance-diagnostics-default.png`
- `artifacts/ui-qa/v4-shots/v4-maintenance-problems-tab.png`
- `artifacts/ui-qa/v4-shots/v4-maintenance-overview-tab.png`
- `artifacts/ui-qa/v4-shots/v4-maintenance-environment-expanded.png`
- `artifacts/ui-qa/v4-shots/v4-maintenance-actions-expanded.png`

生成命令：`scripts/capture-v4-shots.ps1`。

## 5. UI Audit

- 路径：`artifacts/ui-audit/v4-final/AUDIT_SUMMARY.md`
- HIGH：0
- MEDIUM：0
- 失败路由：0
- 运行时警告：39 条 INFO（既有嵌套滚动/风险卡高度信息，无阻断项）

## 6. 功能保真

- REMOVE：0
- old command missing：0
- old grid missing：0
- old column missing：0

## 7. 是否引入二级 Tab

是。维护中心诊断页引入“问题列表 / 诊断概览”二级 Tab。原因：主表、环境/操作摘要和完整摘要同页叠加会把 FindingsGrid 压成一两列；拆分后主表独占有限工作区，概览展开折叠时由页面滚动上下文统一处理，满足 v4 “主表可读、折叠不内滚”的要求。

## 8. Commit SHA

- `3015182` v4 计划
- `5131e4d` 共享样式 + 首页活动
- `0201615` 存档备份自动化表单
- `5196f4a` 维护中心二级 Tab + 截图工具 + 回归测试

## 9. 验证

- Release 构建：0 warning / 0 error
- Playnite：255/255
- Core：59/59（本轮未触及）
- Worker：190/190（本轮未触及）
- render-qa：10 档尺寸 + 56 主题场景 + 7 Resize 全绿
- XAML/source/WPF 静态门禁通过

## 10. 未验证项

- 真实 Playnite 宿主下的主题、DPI、连续缩放最终视觉仍需人工确认。
