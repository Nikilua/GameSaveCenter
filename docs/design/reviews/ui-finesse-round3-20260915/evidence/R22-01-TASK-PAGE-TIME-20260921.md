# R22-01 任务页更新时间

日期：2026-09-21  
代码提交：`c96130a2`（`统一任务页更新时间显示`）

## 本批范围

先核对 `DashboardViewModel.TaskPageState` 的现有状态机和 `TaskCenterView` 的真实绑定。本批只处理任务页“最近更新”与失败保留旧数据提示中的用户可见时间，不改变任务查询、刷新/重试命令、列表筛选、滚动、取消、错误保护或 Playnite/net462 兼容。

复用 Contracts 中已有 `TimeDisplayFormatter`：

- `TaskPageStatusSummary` 正文使用相对时间；加载、无数据失败、保留旧数据和重试语义保持原样。
- `TaskPageStatusSummaryFullDisplay` 提供同一状态的完整本地时区时间和 round-trip UTC 原值，任务队列摘要与 stale banner 的 Tooltip/Automation HelpText 均绑定它。
- `TaskPageLastUpdatedRelativeDisplay`、`TaskPageLastUpdatedFullDisplay` 和 `TaskPageLastUpdatedRawUtcDisplay` 提供任务页更新时间的相对/完整/原始 UTC 投影；旧 `TaskPageLastUpdatedDisplay` 兼容属性保留。
- 时间状态变化会同时通知正文、完整提示和原始值投影，避免刷新后 UI 继续显示旧提示。

## 行为证据

`R21AsyncCompletionAnnouncementBehaviorTests.TaskPageLoadingCompletionReplacesTheReadableStatusSurface` 使用实际 STA WPF `TaskCenterView`，读取生产命名的 `TaskQueueLastUpdatedSummary`：

- 加载态正文为“正在加载任务记录…”；完成态正文使用“最近更新：刚刚”。
- 完成态 Tooltip 和 `AutomationProperties.HelpText` 均为完整本地时间，并带 UTC 原值；两者实际读取结果一致。

`R22TimeDisplayBehaviorTests.TaskPageUpdatedTimeUsesSharedRelativeFullAndRawContract` 覆盖合成 UTC 时间和 null 负例：相对时间不回退为未知，完整/原始值分别复用共享 formatter；无时间仍为“未知”和“未记录 UTC 时间”。`TaskCenterViewResponsiveTests` 同时把既有过时的 `SelectedTask.ErrorMessage` 源码断言校正为当前真实 `FailureSummary` 绑定，确认用户失败摘要仍先于折叠技术详情。

## 验证结果

- `R22TimeDisplayBehaviorTests`：`24/24`
- `R21AsyncCompletionAnnouncementBehaviorTests`：`2/2`
- `TaskCenterViewResponsiveTests`：`7/7`
- 定向合计：`33/33`
- Release 隔离构建：XAML `24/24`，Playnite/Tests/Worker `0 errors`；本阶段首轮完整编译仅报告既有 `MediaCenterView.xaml.cs:671 CS8602`，未改写该基线。测试证据校正后的重编仍为 `0 errors`。
- `python scripts/validate-source.py`：通过。
- WPF 静态审查：`0 errors / 27 warnings / 177 info`，与既有基线一致。
- `git diff --check`：通过。

## 未验边界

证据只来自合成时间、fake DataContext、隔离 STA WPF、隔离 Release 构建和隔离测试目录；没有真实存档、媒体、云端、报告/日志、剪贴板或诊断写入。未宣称真实 Playnite/package-host、Windows UIA/读屏、系统时钟跳变、DPI/物理跨屏、最终呈现帧、ETW 或宿主性能已验；Demo 原目录不可用，参考已恢复生产基线。残余用户可见旧 `ToLocalTime` 入口（如快照更新时间、远端暂存有效期及其他实际绑定）仍需逐项核对。

