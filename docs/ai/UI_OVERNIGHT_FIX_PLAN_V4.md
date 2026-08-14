# GameSaveCenter UI Overnight Fix Plan v4

> 来源：`GameSaveCenter_UI_Overnight_Fix_Pack_v4.zip`
> 原则：当前生产 main > 真实业务功能 > v4 实施包 > 旧 demo；`REMOVE = 0`；GamePicker HARD LOCK；按功能块 commit 并 push。

## A. 全项目折叠组件统一重做

- 问题：折叠 header 仍显得粗糙，图标与文字不对齐，存在多余符号残留。
- 受影响：`DesignTokens.xaml` → `GscDisclosureCard` / `GscDisclosureCardExpander`；所有使用 Expander 的页面。
- 修改策略：
  - 重构 `GscDisclosureCard` 模板：独立 28 DIP chevron 图标区，图标垂直居中，标题文字同基线；去掉任何 `>` 残留。
  - Collapsed / Hover / Expanded / Disabled 状态全部使用主题 DynamicResource；内容区与 header 用分隔线/间距区分。
  - 全项目 Expander 统一引用 `GscDisclosureCard`，不再混用旧样式。
- 保留：所有 Expander 内容、命令、绑定、AutomationProperties。
- 验收：静态扫描无旧样式引用、无尾部 `>`；展开/折叠截图；UI Audit 无 HIGH/MEDIUM。

## B. 存档中心备份自动化表单

- 问题：`SaveCenterView.xaml` 备份自动化卡中数字输入框缺少语义 label 和说明；模板参数区存在 4 个“裸输入框”。
- 受影响：`SaveCenterView.xaml` → `备份策略` Tab、`SavePolicyCardsLayout`、`SavePolicyTemplatesCard`；`DesignTokens.xaml` → 共享数值输入资源。
- 修改策略：
  - 新增共享 `GscNumericFieldRow` 风格结构：`Label + Input + Unit/Suffix + Helper text`。
  - 当前游戏“游玩中周期备份”输入改为完整表单行：label、输入框、分钟单位、`允许 1–1440` helper。
  - 策略模板参数区四个数值输入（周期备份间隔、最近版本保留小时、每日保留天、每周保留周）全部改为带 label/unit/helper 的表单行；窄窗自动单列。
  - 确认 `GscNumericTextBox` 在深色主题下使用 `GscPrimaryTextBrush`，数字清晰可见。
- 保留：`SelectedGame.Policy.DuringPlayIntervalMinutes`、`PolicyTemplateDraft.Policy.*` 绑定与业务语义。
- 验收：截图标准/窄窗；断言每个输入有 label/helper；深色主题像素/文本对比检查。

## C. 首页全局活动轻量表格

- 问题：活动行左右空隙过大、图标未垂直居中、时间列视觉不完整。
- 受影响：`OverviewView.xaml` → `OverviewActivityTimelineList` ItemTemplate。
- 修改策略：
  - 行高收敛到约 60 DIP，图标 32×32 且 `VerticalAlignment="Center"`。
  - 列宽调整为 `40 | * MinWidth=120 | Auto MaxWidth=180 | 112`，chips 紧跟主内容。
  - 时间列固定 112 DIP，`MM-dd HH:mm` 完整显示；窄窗 chips 移到摘要下方（保留 Compact Trigger）。
  - 行分隔、hover、状态色沿用 `GscActivityRowBorder` 与现有 DataTriggers。
- 保留：Glyph、GameName、Summary、KindDisplay、ResultDisplay、CreatedDisplay 全部绑定。
- 验收：render-qa 记录图标 Y/时间列宽；标准/窄窗截图。

## D. 维护中心折叠体不内滚

- 问题：`更多维护操作`、`首次环境检查` 展开后内部出现独立 ScrollViewer。
- 受影响：`MaintenanceView.xaml` → `EnvironmentCheckDisclosureScroller`、`MaintenanceActionsDisclosureScroller`；`MaintenanceView.xaml.cs` → MaxHeight 逻辑。
- 修改策略：
  - 删除折叠体内部 ScrollViewer，内容自然撑开。
  - 诊断页拆成二级 Tab 后，`诊断概览` 使用页面级 ScrollViewer 承载环境卡/操作卡/摘要；展开折叠时由该页面滚动上下文统一滚动。
- 保留：全部诊断命令、环境检查项、完整摘要、健康面板、路径迁移字段。
- 验收：展开态截图无折叠体内滚动条；render-qa 不报告内部滚动竞争。

## E. 维护中心主表可读性

- 问题：诊断页主表被环境/操作/摘要挤压，视觉像只剩一两列。
- 受影响：`MaintenanceView.xaml` 诊断 Tab 整体结构、`MaintenanceDiagnosticsLayout`、`FindingsGrid`、`MaintenanceDiagnosticsInspector`、`MaintenanceDiagnosticSummaryGrid`；`MaintenanceView.xaml.cs` 响应式逻辑；UI Audit 断言。
- 修改策略：
  - 引入二级 Tab：默认 `问题列表`，次项 `诊断概览`。
  - `问题列表` 独占有限工作区：FindingsGrid 五列 + Inspector，滚动条只在表格/详情内部，不超出边框。
  - `诊断概览` 承载环境卡、诊断操作卡、完整摘要、健康面板；页面滚动由该子页承担。
  - 更新代码后置与测试/审计断言以匹配新结构；保留 `FindingsGrid`、5 列、`SelectedFinding` 与全部命令。
- 验收：默认态 FindingsGrid 首屏可见且至少 4 行；展开折叠后无折叠体内滚动；UI Audit MAINT-001/002 按新结构重写。

## F. 主动自查范围

- 检查所有 Expander header 是否统一 `GscDisclosureCard`、无尾部 `>`、无内部滚动。
- 扫描数值输入是否都有 label/unit/helper；深色主题文字对比。
- 检查按钮组高度/基线一致（Overview、Save、Maintenance 工具栏）。
- 检查状态 badge 颜色分层与 DataGrid header/scrollbar 越界。
- 默认态页面滚动条数量与展开态滚动关系。
- 对发现的问题做最小兼容修正，不扩散重排其他页面。

## 实施顺序

1. Phase 0：本计划提交。
2. Phase 1：Disclosure 与 NumericField 共享样式收口。
3. Phase 2：首页活动行修正。
4. Phase 3：存档备份自动化表单。
5. Phase 4：维护中心二级 Tab 重构。
6. Phase 5：主动自查 + 测试/Audit 断言更新。
7. Phase 6：截图、UI audit、`UI_OVERNIGHT_FIX_REPORT_V4.md`、按功能块 commit/push。

## 验收产物

- `docs/ai/UI_OVERNIGHT_FIX_REPORT_V4.md`
- 截图：首页 5 张、存档 3 张、维护中心默认/展开/子页 4+ 张
- 新 UI audit zip
- before/after 问题闭环对照
- 每个功能块独立 commit SHA

## 完成状态（2026-08-15）

- 全项目折叠统一：`GscDisclosureCard` 独立 chevron 图标区，无旧 `GscExpander`、无尾部 `>`、无折叠体内滚动。
- 首页全局活动：行高 60 DIP，图标垂直居中，时间列 112 DIP，chips 紧跟主内容。
- 存档备份自动化：当前游戏与策略模板所有数值输入补齐 label/unit/helper；深色主题数字使用共享主文字色。
- 维护中心：诊断页二级 Tab（默认 `问题列表` / 次项 `诊断概览`），FindingsGrid 独占主工作区，完整摘要移入概览页。
- 功能保真：REMOVE=0；old command missing=0；old grid missing=0；old column missing=0；GamePicker 未改。
- 验证：Release 0 warning/0 error；Playnite `255/255`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/39 INFO/0 失败路由。
- 截图：`artifacts/ui-qa/v4-shots/`；命令 `scripts/capture-v4-shots.ps1`。
- 提交：`3015182`、`5131e4d`、`0201615`、`5196f4a`，均已 push。
- 未验证项：真实 Playnite 宿主主题、DPI 与连续缩放最终视觉仍为 `MANUAL QA REQUIRED`。
