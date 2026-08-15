# GameSaveCenter UI Overnight Closure Plan v6

> 来源：`GameSaveCenter_UI_Overnight_Closure_Pack_v6.zip`
> 原则：当前生产源码 > 用户最新要求 > 最新 UI Audit > v6 包 > 旧 v5/Demo；`REMOVE = 0`；GamePicker HARD LOCK；Tasks/Maintenance 不新增 GamePicker；页面历史改为 Playnite 会话级。

## 1. Navigation：Playnite 会话级页面历史

- 问题：当前首次打开 GSC 时若 onboarding 未完成会强制 Maintenance；否则按持久化 `LastWorkspace` 恢复页面，跨 Playnite 重启仍保留旧页面。
- 源码位置：
  - `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs` 构造函数（约 136-143）
  - `CurrentWorkspace` setter（约 471-480）
  - `SaveUiStateSettings`（约 2752-2763）
  - `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettings.cs` `LastWorkspace`（88）
  - `src/GameSaveCenter.Playnite/Views/DashboardView.xaml.cs` `OnLoaded`（82-84）
- 根因：每次打开 GSC 都会 `new DashboardView` / `new DashboardViewModel`，而页面历史被写进持久化 Settings。
- 修改方式：
  - 在 `GameSaveCenterPlugin` 增加进程级 `WorkspaceKind? SessionLastWorkspace`（可命名为 `SessionWorkspaceState`）。
  - `DashboardViewModel` 构造函数改为：`currentWorkspace = plugin.SessionLastWorkspace ?? WorkspaceKind.Overview`；删除 onboarding 强制跳转与 `LastWorkspace` 解析。
  - `CurrentWorkspace` setter 改为只更新 `plugin.SessionLastWorkspace`，不再写 Settings；保留 `uiStateSave?.Schedule()`。
  - `SaveUiStateSettings` 删除 `LastWorkspace` 写入。
  - `DashboardView.OnLoaded` 不再因 onboarding 自动勾选 Maintenance；Onboarding 未完成时首页显示 CTA，点击显式进入 Maintenance。
  - 新增 `OpenMaintenanceCommand`（纯导航）：`CurrentWorkspace = Maintenance` 并触发既有 `AttentionCenterRequested` 导航事件。
- 保留：GamePicker last selected game、筛选/排序、用户设置、备份策略等持久化；`Settings.LastWorkspace` 属性保留但不再作为启动依据。
- 标准/窄窗：导航行为与尺寸无关。
- 主题：与主题无关。
- 自动验收：
  - 单测断言构造函数不解析 `Settings.LastWorkspace`、setter 不写 Settings、插件持有 Session 状态。
  - RenderHarness 模拟“同会话重开”恢复当前 workspace；模拟“新插件实例”回到 Overview。
  - 更新 `UiStatePersistenceSourceTests`。

## 2. Numeric Input：`GscNumericFieldInput` 数字裁切

- 问题：`游玩中周期备份间隔` 等数字框内数字被垂直裁切。
- 源码位置：`DesignTokens.xaml` → `GscTextBox`（707-757）、`GscNumericTextBox`（760-774）、`GscNumericFieldInput`（791-795）；`SaveCenterView.xaml` 5 处使用。
- 根因：`PART_ContentHost` 只设置 `VerticalAlignment="Center"`，没有把 `VerticalContentAlignment` 绑定进模板；配合 `Padding=9,7` 时数字内容未真正在内容宿主内居中，出现裁切。
- 修改方式：
  - `PART_ContentHost` 增加 `VerticalContentAlignment="{TemplateBinding VerticalContentAlignment}"`。
  - `GscNumericFieldInput` 设为 `Padding="8,4"`、`MinHeight="38"`、`Height="42"`（外尺寸不变）、`TextAlignment="Center"`、`VerticalContentAlignment="Center"`。
  - 不改 FontSize、不缩绑定数值、不使用负 Margin/TranslateTransform。
- 保留：`SelectedGame.Policy.DuringPlayIntervalMinutes` 等全部绑定与 ToolTip。
- 标准/窄窗：框宽保持 112 DIP，窄窗自然换行。
- 主题：Dark/Light/Follow Playnite 数字均使用 `GscPrimaryTextBrush`。
- 自动验收：
  - 静态断言模板绑定与 padding。
  - v6 截图覆盖 1/5/30/120/1440 并记录非空白。
  - render-qa 全尺寸通过。

## 3. Save Automation Form 语义分组

- 问题：`游玩中周期备份间隔` 与 `异常保护` 平级，视觉上没有从属于 `游玩中周期备份` 开关。
- 源码位置：`SaveCenterView.xaml` → `SaveBackupAutomationCard`（约 332）。
- 根因：间隔输入被放在开关组下方的独立 StackPanel，没有归属缩进。
- 修改方式：
  - 在 `游玩中周期备份` ToggleSwitch 后放入一个缩进子面板（`Margin="24,0,0,0"`），包含 Label、Input、单位、范围 helper、业务说明。
  - `异常保护` 保持独立小节，但补充 label/helper（已具备）。
  - 模板参数区的四个数值输入继续使用 `GscNumericFieldInput` 与 label/unit/helper。
- 保留：所有 Policy 绑定、命令、保存语义。
- 标准/窄窗：标准两列卡片；窄窗 `SavePolicyCardsLayout` 已堆叠单列。
- 主题：helper/label 使用 `GscSecondaryTextBrush`。
- 自动验收：静态断言间隔字段位于开关之后且带 `Margin="24,0,0,0"`；截图标准/窄窗。

## 4. Overview Global Activity 轻量表格

- 问题：活动列表仍显松散，左右分离，无列头，不够像成熟表格。
- 源码位置：`OverviewView.xaml` → `OverviewActivityTimelineList`（约 308-372）；`UiLayoutRegressionTests.OverviewGlobalActivityUsesStableFourColumnRow`。
- 根因：当前是四列 ItemsControl，无 header，列宽让 chips 与时间远离摘要。
- 修改方式：
  - 在表格 Frame 内增加轻量 header 行：`对象/范围 | 事件 | 分类 | 结果 | 时间`。
  - 标准/宽行改为六列：`40 | 150 | * | 88 | 76 | 112`；只有 `Summary` 使用 `*`。
  - 行高收敛到 52 DIP，图标 30×30 完全居中，GameName SemiBold、Summary SecondaryText + Tooltip。
  - 窄窗（Compact Trigger）仍保留 icon/main/time 一行，chips 下移，header 简化或隐藏；字段不删。
  - 不加独立 ScrollViewer，仍由 Overview 根页滚动；禁止水平滚动。
- 保留：Glyph、GameName、Summary、KindDisplay、ResultDisplay、CreatedDisplay 全部绑定。
- 标准/窄窗：如上。
- 主题：header 使用 `GscTableHeaderBrush`/`GscSecondaryTextBrush`；Dark/Light 一致。
- 自动验收：更新列宽/header 回归断言；v6 截图标准/窄窗；render-qa。

## 5. Overview Risk Card 大空白与滚动链

- 问题：`OverviewRiskCard` 仍有约 826 DIP 高度；Overview 存在 disabled nested ScrollViewer 链。
- 源码位置：`OverviewView.xaml` → `OverviewSecondaryScrollViewer`（395-496）、`OverviewRiskCard`（434-494）；`OverviewView.xaml.cs`。
- 根因：secondary 列仍由 ScrollViewer 包裹并参与 measure/arrange，导致风险卡随 ScrollViewer 内容高度撑大；Audit 将 disabled ScrollViewer 也计入 nested。
- 修改方式：
  - 将 `OverviewSecondaryScrollViewer` 改为普通 `Grid`（保留 x:Name），代码字段类型同步改为 `Panel`/`Grid`；根页 `OverviewStackScrollSurface` 是唯一纵向滚动。
  - `OverviewRiskCard` 内部全部 `Auto` 行、`VerticalAlignment="Top"`、无 star spacer、无业务外 MinHeight；CTA 紧跟内容。
  - `OverviewPrimaryScrollSurface` 同样改为普通 Grid 或保持 disabled 但更新 Audit 分类，优先改为 Grid。
  - 更新 `OverviewView.xaml.cs` 属性与 responsive 逻辑、RenderHarness 探针、`UiLayoutAnalyzer` 分类（disabled wrapper 不再算 TRUE_PARENT_CHILD_SCROLL_CONFLICT）。
- 保留：Overview 业务内容、命令、绑定、风险卡全部块。
- 标准/窄窗：根页滚动；风险卡按内容自然高度。
- 主题：不变。
- 自动验收：Audit 中 `OV-005 RISK_DEAD_SPACE` 清零；nested scroll 分类只保留 TRUE 冲突；截图风险卡。

## 6. Maintenance / Media 真父子滚动

- 问题：`MaintenanceDeviceScrollSurface`、`MaintenanceProcessScrollSurface`、`MediaCurrentScrollSurface` 外层 ScrollViewer 包裹 DataGrid/ListBox，Audit 记为 nested。
- 源码位置：`MaintenanceView.xaml`（262-286、539-571）、`MediaCenterView.xaml`（183-207）、`MaintenanceView.xaml.cs`、`MediaCenterView.xaml.cs`。
- 根因：页面级 ScrollViewer 与表格/列表内部滚动形成父子竞争。
- 修改方式：
  - Device/Process/Media Current 改为：Header/Form/Filters 为 Auto 行，主区域 `*`，DataGrid/ListBox 直接占用有限行；Inspector 为 sibling 内部滚动。
  - 移除或禁用外层 page ScrollViewer（Device/Process/Media Current）；保留 Audit/Diagnostics 概览等已明确允许的页面滚动。
  - 更新 code-behind 对 `*ScrollSurface` 的处理与 render-qa/audit 断言。
- 保留：DataGrid/ListBox 虚拟化、Inspector、全部命令。
- 标准/窄窗：主表有限视口；窄窗 Inspector 堆叠。
- 主题：不变。
- 自动验收：`TRUE_PARENT_CHILD_SCROLL_CONFLICT = 0`；render-qa 表格高度 ≥236。

## 7. Maintenance Header 主题

- 问题：Audit 要求 last/filler header 白块=0，middle header 不消失。
- 源码位置：`MaintenanceView.xaml` 顶部 header styles（24-44）；`MaintenanceView.xaml.cs` `ApplyGridHeaderTheme`。
- 根因：Playnite host 可能用默认 header 覆盖 keyed style。
- 修改方式：
  - 确认 `MaintenanceFirstColumnHeader` / `MaintenanceLastColumnHeader` / `GscLastColumnHeader` 均 `OverridesDefaultStyle=True` 并使用 `GscTableHeaderBrush`。
  - `ApplyGridHeaderTheme` 对所有列包括中间列显式设置 `GscDataGridColumnHeaderStyle`，filler 使用同一主题资源。
  - 增加 header 主题回归断言（Light/Dark 均无 White/Black 硬编码）。
- 保留：排序、列宽、HeaderStyle 绑定。
- 验收：Audit header 白块=0；主题截图。

## 8. Filter / Search 语义补强

- 问题：Task/Media 搜索框与“全部”筛选缺少语义前缀，用户看不出用途。
- 源码位置：`TaskCenterView.xaml`（98）、`MediaCenterView.xaml`（201-202）。
- 修改方式：
  - Task：搜索框前加紧凑 `搜索任务…` 语义前缀或复用现有 ToolTip + 加 `AutomationProperties.Name`；三个筛选 ComboBox 的 ToolTip 已存在，再给选项/默认值加 `状态: 全部`、`游戏: 全部`、`类型: 全部` 显示前缀（通过 ItemContainer 或本地样式，不改 ViewModel）。
  - Media：搜索框加 `搜索当前游戏媒体…` 前缀；筛选 ComboBox 显示 `类型: 全部`。
  - 不增加第二行 label；不改变筛选逻辑与持久化。
- 保留：全部 Binding、筛选逻辑、持久化。
- 标准/窄窗：紧凑，不增加高度。
- 主题：次级文本。
- 自动验收：静态断言前缀；截图 Task/Media 窄窗。

## 9. Proactive Polish

- 范围：Disclosure 图标/文字对齐扫描、按钮组几何、深色可读性、DataGrid scrollbar 边框、无内部滚动。
- 方式：复用 `OvernightV4SharedTests`、render-qa 按钮探针、v6 截图；只做小而确定修正，不重排未点名页面。

## 10. 验收产物

- `docs/ai/UI_OVERNIGHT_CLOSURE_REPORT_V6.md`
- v6 截图：Overview 标准/窄/Activity 宽/窄/Risk/Current Game、Save 当前规则/自动化当前/模板/numeric 1/5/30/120/1440、Maintenance Diagnostics/Device/Audit/Process
- UI audit：HIGH=0、MEDIUM=0、TRUE_PARENT_CHILD_SCROLL_CONFLICT=0
- render-qa 全绿、Playnite 全量测试、静态门禁
- 每个功能块独立 commit 并 push

## 11. 基线证据

- 最新 `GameSaveCenter-ui-audit(4).zip` 未在 `D:\Download\Brave` 与工作区发现；以仓库最新 `artifacts/ui-audit/v4-final/AUDIT_SUMMARY.md` 作为基线，实施结束后重新生成 v6 audit。

## 12. 完成状态（2026-08-15）

- 页面历史：Playnite 会话级已完成，首次 Overview、同会话恢复、重启回 Overview；Onboarding 首页 CTA 显式进入 Maintenance。
- 数字输入：`GscNumericFieldInput` 根模板修复，1/5/30/120/1440 完整居中。
- 存档表单：当前游戏与策略模板均带 label/unit/helper，间隔字段从属 `游玩中周期备份`。
- 全局活动：轻量六列表格 + header，标准/窄窗截图完成。
- 风险卡与 Overview 滚动：风险卡内容收紧；Primary/Secondary 改为 Grid，根页唯一滚动。
- Maintenance/Media：Device/Process/Media Current 外层 ScrollViewer 改为有限 Grid，`TRUE_PARENT_CHILD_SCROLL_CONFLICT=0`。
- 筛选：Task/Media 语义前缀已补齐。
- Audit：0 HIGH / 0 MEDIUM / 8 INFO；render-qa 全绿；Playnite `261/261`。
- 截图：`artifacts/ui-qa/v6-shots/`；报告：`docs/ai/UI_OVERNIGHT_CLOSURE_REPORT_V6.md`。
