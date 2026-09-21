# R22-01 比较版本下拉时间证据

日期：2026-09-21

## 本子批范围

本批只处理 SaveCenter 生产比较区域的 A/B 版本下拉条目。先核对 R11-02 已有的版本比较行为，再补时间显示契约；比较摘要、历史跳转状态消息、复制列和仅内部日志/导出字段不在本批范围。

## 已核对与实现

- `BackupVersionDto.ComparisonDisplay` 保留为兼容标签，`CreatedUtc` 仍是排序和比较选择的事实来源。
- 新增 `ComparisonRelativeDisplay`、`ComparisonFullDisplay` 和 `ComparisonRawUtcDisplay`，复用既有 `TimeDisplayFormatter`；未知 `DateTime.MinValue` 不伪造为当前时间。
- `SaveCenterView` 的 A/B 两个生产 `ComboBox` 保留 `ItemsSource`、双向 `SelectedItem`、A/B Automation Name、说明 Tooltip 和既有共享 ComboBox 样式；条目模板正文显示相对时间，条目 Tooltip/Automation HelpText 显示完整本地时间与 round-trip UTC。
- R11-02 已有 A/B 选择、交换、比较命令和同版本禁用负例保持；没有改动比较方向、差异语义、选框、滚动条或 Playnite/net462 兼容。

## 证据

- 合成 DTO 行为：`R22TimeDisplayBehaviorTests 22/22`，覆盖旧标签兼容、相对/完整/原始时间和未知时间负例。
- 实际隔离 STA WPF `SaveCenterView`：`R11VersionComparisonBehaviorTests 2/2`；从生产视图的两个 ComboBox 读取 A/B `SelectedItem`，并读取模板生成的相对文本、完整 Tooltip 和 Automation HelpText，同时保留比较/交换/同版本禁用断言；与 R22 定向合计 `24/24`。
- 隔离 Release 构建：XAML `24/24`；Playnite `net462` 与 Tests `net472` 均 `0 errors`；仅保留既有 `MediaCenterView.xaml.cs:671` 的 2 条 CS8602 warning。
- `scripts/validate-source.py`、`git diff --check` 通过；WPF 静态基线为 `0 errors / 27 warnings / 177 info`。

## 边界

验证只使用合成 `BackupVersionDto`、fake DataContext、隔离 STA WPF 和隔离构建目录，没有真实恢复、存档、媒体、云端或诊断写入。未声称真实 Playnite/package-host、Windows UIA/读屏、真实剪贴板、系统时钟跳变、DPI/跨屏、presented frame、ETW 或宿主性能已验证；Demo 原目录不可用。`CompareSelectionSummary`/`BuildComparisonSummary` 和历史状态消息仍可能使用旧 `ComparisonDisplay`，下一子批继续逐项核对。
