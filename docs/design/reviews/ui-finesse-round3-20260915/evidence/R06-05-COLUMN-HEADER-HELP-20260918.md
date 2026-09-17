# R06-05 列头说明证据

日期：2026-09-18  
任务：`R06-05 | 列头说明`  
结论：在当前可控范围内已满足；代码已提交并推送到 `codex/ui-finesse-round2`。

## 1. 现有能力核对

- 生产 DataGrid 已有共享 `GscDataGridHeaderTextTemplate`，使用 `Wrap` + `TextTrimming=None`，共享列头模板还保留 22 DIP 排序箭头槽和透明 8 DIP 列宽拖拽 Thumb。本项不替换表头模板，不把说明做成会吞掉排序事件的嵌套按钮。
- `BackupVersionDto`、`MediaItemDto`、存储分析/保留策略 DTO 和诊断格式化路径已经统一使用 `B / KiB / MiB / GiB` 的 1024 进制显示；本项只补充列头对单位的解释，不重建容量格式化逻辑，也不虚构 `TiB` 运行时输出。
- 原始 Demo `DesignShellView.xaml`/`Pages` 目录在当前 checkout 不存在；按项目事实沿用恢复的生产 `WpfUiProduction.xaml`/`Redesign.xaml` 基线，没有把 main 的旧实现覆盖到当前分支。

## 2. 实现

提交：`7527e6381e531331d658166f0917487c03a0e7e3`（`补充表格列头语义说明`）。

新增 `DataGridColumnHeaderHelpBehavior`：

- 说明挂在 `DataGridColumn` 的 attached `Description` 上，生成的 `DataGridColumnHeader` 在 Loaded 时把说明同步到 Tooltip 和 `AutomationProperties.HelpText`。
- 列头仍保留原始字符串 Header；排序、键盘导航和列宽拖拽继续由原生 `DataGridColumnHeader`/共享模板负责。没有引入 header 内按钮、独立点击路由或改变 `DataGridStableSortController`。
- Save History/Candidates、Task Queue、Media Inbox、Maintenance Findings 的时间、数量/百分比、容量、路径、类型/来源、状态/等级和详情摘要列均补充短说明；长内容继续由共享 Wrap/None 策略或详情区承载。
- 说明明确区分“备份锁定状态”“候选处理状态”“任务生命周期状态”“媒体来源/归类原因”“诊断严重级别”，避免同名“状态”误解；百分比范围和 `B/KiB/MiB/GiB` 单位在相邻列头可达。

## 3. 自动验证

- `R06ColumnHeaderHelpTests 2/2`：真实生成的 WPF `DataGridColumnHeader` 收到 Tooltip/Automation HelpText，Header 仍为“大小”字符串，列仍可排序/重排，header visual tree 没有嵌套 Button；源契约验证共享行为、Wrap/None、五类生产表说明和无绑定 Header。
- 相邻回归：`R06ClipboardBehaviorTests 3/3`、R06-03 选中焦点 `2/2`、R06-02 排序 + R06-01 列宽 `9/9`。
- `scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py`、`git diff --check` 通过；标准 Release 构建 `0 warning / 0 error`。

## 4. 视觉与布局证据

保留的最新渲染证据：`.tmp/r06-05-render-final/render-qa-report.txt`。

- 报告绑定 `7527e6381e531331d658166f0917487c03a0e7e3`，`WorkingTreeClean=True`，Light/Dark 双主题共 357 张 PNG。
- 覆盖 50/400/2000/4468 数据量、纵向/横向滚动、虚拟化以及 `2560×1440 → 1100×720 → 2560×1440` resize；结果为 `render-qa OK`。
- Save History 7 列、Task 6 列、Media Inbox 5 列、Maintenance Findings 3 列的多尺寸 header contract 均为 `resize=true`、`sort-arrow=visible`；表格首屏行数和内部滚动继续通过。
- 已人工抽查 `theme/light/Save-1040x700.png`、`theme/dark/Save-1040x700.png`、`theme/light/Task-1040x700.png`、`theme/dark/Maintenance-1040x700.png`：表头换行不挤压状态徽章/进度/危险区域，浅深主题层级正常。
- `DpiScale=1.00` 仅代表离屏 logical DIP；Tooltip 说明的实际悬停出现时序不由静态 PNG 证明。

## 5. 未验边界与下一步

本阶段未启动真实 Playnite 宿主，也未验真实鼠标悬停、键盘排序、列宽拖拽、UI Automation、屏幕阅读器、IME、物理 DPI/跨屏、呈现帧、ETW 或宿主性能。受控 WPF 行为只证明生成列头属性和视觉树契约，不替代真实宿主输入链；既有 Q02 数值/单位证据仍属于离屏范围，真实八入口宿主盘点边界未被改写为通过。

未写真实存档、媒体、云端或诊断数据。本阶段新增的 `.tmp/r06-05-render-final` 已被本证据引用并保留；没有保留其他未引用的一次性目录，R06-03、R06-04 当前渲染目录继续作为现阶段证据保留。

下一可执行任务：`R06-06 行内进度稳定`。先核对 `TaskStatusDto.ProgressPercent/ProgressValue/ProgressDisplay` 和任务刷新/选择/滚动路径，验证进度只更新必要单元格、未知总量和取消状态不造假、不重置表格上下文。
