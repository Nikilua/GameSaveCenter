# R22-01 比较选择与结果摘要时间证据

日期：2026-09-21

## 本子批范围

本批只处理 SaveCenter 比较区域中 `CompareSelectionSummary` 和比较完成后的 `DiffComparedSummary` 两个用户可见摘要。比较下拉条目已在上一子批收口；历史版本快速操作、历史跳转状态消息、复制列和仅内部日志/导出字段留作下一边界。

## 已核对与实现

- `DashboardViewModel.BuildComparisonSelectionSummary` 和 `BuildComparisonSummary` 不再把本地短日期直接写入正文，正文复用已有相对时间投影；A/B 方向、版本 ID、新增属于 B、删除属于 A 以及同版本负例保持。
- 新增对应的完整摘要投影，生产 SaveCenter 两个摘要 `TextBlock` 的 Tooltip/Automation HelpText 使用完整本地时区和 round-trip UTC；摘要没有新增服务、IPC、比较请求或差异列表路径。
- `BackupVersionDto.CreatedUtc` 仍是比较和排序事实来源；原有比较命令、交换命令、禁用门控、选框、滚动条、取消/错误语义和 Playnite/net462 兼容保持。

## 证据

- `R22TimeDisplayBehaviorTests 23/23` 覆盖相对正文、完整摘要、同版本和缺少版本负例。
- 隔离 STA WPF `R11VersionComparisonBehaviorTests 2/2` 实际读取生产 `SaveCenterView` 绑定的选择摘要 TextBlock、Tooltip 和 Automation HelpText，并保留 A/B SelectedItem、比较/交换和同版本禁用行为；与 R22 定向合计 `25/25`。
- 隔离 Release 构建：XAML `24/24`；Playnite `net462` 与 Tests `net472` `0 errors`；仅既有 `MediaCenterView.xaml.cs:671` 的 2 条 CS8602 warning。`scripts/validate-source.py`、`git diff --check` 通过；WPF `0 errors / 27 warnings / 177 info`。

## 边界

验证仅使用合成 DTO、fake DataContext、隔离 STA WPF 和隔离构建目录，没有真实存档、媒体、云端或诊断写入。未声称真实 Playnite/package-host、Windows UIA/读屏、真实剪贴板、系统时钟跳变、DPI/跨屏、presented frame、ETW 或宿主性能已验证；Demo 原目录不可用。历史跳转状态消息和 `GameSaveCenterPlugin` 快速历史摘要仍可能直显完整本地时间，下一子批逐项核对并区分用户可见、复制和内部日志语义。
