# R06-03 选中焦点区分证据

日期：2026-09-18  
实现提交：`d83c73785c15b72af11c6b8897f359129ba66c26`（`区分表格选中焦点与错误状态`）

## 结论

R06-03 在当前可控范围内已满足。共享生产资源现在明确区分：

- 活动选中：Demo-first accent surface 与 1 DIP accent outline；
- 键盘当前行/项：2 DIP accent outline，保留可见内容表面；
- 失焦选中：`GscSelectionInactiveBrush` 与 muted outline，文字回到主文本色；
- 错误行：`TaskState.Failed` 使用 `GscErrorTintBrush`/`GscErrorBrush`，不会被失焦选中底色吞掉；
- 悬停：继续使用既有共享 hover surface；Media Inbox 移除本地会覆盖共享状态的 hover/selected trigger；
- DataGridCell 的选中内容面保持透明，状态徽章、进度和危险动作不被整行选中底色遮蔽。

原始 Demo 目录中的 `DesignShellView.xaml`/`Pages` 在当前 checkout 不存在；本阶段记录该事实，继续以当前已恢复生产基线 `DesignTokens.xaml`、`WpfUiProduction.xaml` 和既有 RenderHarness 为视觉基准，没有自行替换新的设计体系。

## 实现范围

- `WpfUiProduction.xaml`：共享 `ListBoxItem` 和 `DataGridRow`/`GscStableDataGridRow` 补齐 active/inactive/keyboard/error 状态；失败态复用 `GameSaveCenter.Contracts.TaskState.Failed`，避免字符串状态比较漂移。
- `DesignTokens.xaml`、`AdaptiveThemePalette.cs`：增加失焦选中令牌，并同时覆盖静态 Demo light/dark 与活动主题调色板路径。
- `MediaCenterView.xaml`：删除 `MediaInboxStableRowStyle` 的本地 selected/hover 覆盖，回到共享行状态模板。
- `R06SelectionStateBehaviorTests.cs`：使用合成 `TaskStatusDto`、真实生产资源字典和隔离 STA Window，验证行为而非只做源码字符串断言。

未改游戏选框、现有滚动条系统、命令/Binding、取消/错误/安全恢复语义、有限列表性能或 Playnite/net462 目标；未写真实存档、媒体、云端或诊断数据。

## 自动验证

- `R06SelectionStateBehaviorTests`：`2/2`。
  - 失败行的错误底色/边框实际命中；
  - 成功行键盘焦点 2 DIP 边框实际命中；
  - 失焦选中实际使用 inactive brush/muted border；
  - CellChrome 透明内容面、共享状态资源和 Media 本地 trigger 清理均有源契约检查。
- `WpfUiResourceDictionaryTests`：`137 passed / 39 skipped / 0 failed`。
- R06-02 `R06SortingBehaviorTests`：`4/4`；R06-01 `R06ColumnWidthPersistenceBehaviorTests`：`5/5`。
- `scripts/build.ps1 -Configuration Release -SkipTests`：XAML `24/24`，Release 编译 `0 warning / 0 error`，Playnite 目标含 `net462`。
- `scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py`：通过；`git diff --check`：通过。

R06-03 构建目录在阶段完成后按 `.tmp` 清理规则删除；本证据绑定提交、命令和结果，不保留未引用的中间构建副本。

## 视觉证据

最终报告：[`.tmp/r06-03-render-final/render-qa-report.txt`](../../../../../../.tmp/r06-03-render-final/render-qa-report.txt)。报告记录：

- Commit=`d83c7378...`，`WorkingTreeClean=True`；
- Light/Dark，357 张 PNG，最终 `render-qa OK`；
- `DpiScale=1.00`，明确为 offscreen logical DIP；
- 数据量包含 `50/400/2000/4468`，既有纵横向滚动和虚拟化探针通过；
- 2560×1440 → 1100×720 → 2560×1440 resize 恢复通过；
- Light/Dark 的 Overview、Save、Trainer、Media、Maintenance、Task、Settings 1040×700 均 OK。

人工抽查代表图：

- [Light Task 1040×700](../../../../../../.tmp/r06-03-render-final/theme/light/Task-1040x700.png)：失败行保留红色危险边界，运行/成功/取消徽章和进度条可读；
- [Dark Task 1040×700](../../../../../../.tmp/r06-03-render-final/theme/dark/Task-1040x700.png)：深色主题下错误行、状态徽章、表头和滚动区域仍可辨识；
- [Light Media 1040×700](../../../../../../.tmp/r06-03-render-final/theme/light/Media-1040x700.png)：Media Inbox 批量栏、行内容和现有页级滚动未被本地状态覆盖破坏；
- [Dark Save 1040×700](../../../../../../.tmp/r06-03-render-final/theme/dark/Save-1040x700.png)：历史表格的状态徽章、锁定提示、列头和横向/页级层次保持。

## 未验边界与下一步

行为夹具使用合成 DTO、隔离 WPF Window/STA 和离屏 logical DIP；渲染图不是真实屏幕呈现帧。尚未在真实 Playnite 嵌入宿主中验证鼠标悬停、键盘导航、UI Automation/屏幕阅读器、物理 DPI/跨屏、IME、宿主字体替换、presented frame、ETW 或宿主性能，因此不将本阶段写成这些维度已通过。继续保持不写真实存档、媒体、云端或对外诊断。

下一可执行小批量为 R06-04“复制单元格与整行”：先盘点现有复制命令、DTO/诊断字段和隔离剪贴板测试能力，明确完整原值、稳定分隔符、批量去重以及凭据排除边界后再实现。
