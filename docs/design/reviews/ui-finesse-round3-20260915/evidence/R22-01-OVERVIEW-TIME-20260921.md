# R22-01 Overview 与任务入口时间显示（2026-09-21）

## 本子批次结论

本子批次提交 `b53ab44fd48c3e2a879cb886f410ffb155c3b2fb` 已把已有共享时间显示契约接入 Task Center 任务表/详情和 Overview 最近任务/全局活动两个入口。R22-01 整项仍为“部分满足，待继续”，没有把其他页面、真实剪贴板或真实宿主验证提前写成完成。

## 复用与实现

- 复用 Contracts 的 `TimeDisplayFormatter`、`TaskStatusDto` 和 `ActivityEntryDto`，为任务/活动 DTO 提供相对时间、带本地时区偏移的完整时间和原始 UTC round-trip 文本；未知任务开始时间仍为“未开始”。
- Task Center 任务表改用相对时间；已有任务表列宽、`DataGridClipboardBehavior`、分页/选择、命令绑定和滚动系统保持不变。时间单元格 Tooltip 提供完整时间，任务详情开始时间同时提供完整时间和 Automation HelpText。
- Overview 最近任务和全局活动继续使用有限/虚拟化列表，仅替换时间显示绑定为相对时间，并把完整时间放入 Tooltip 与 Automation HelpText；没有新增计时器、服务、真实数据写入或第二套时间格式化逻辑。

## 实际验证

- `R22TimeDisplayBehaviorTests` `6/6`：共享格式化器边界，以及任务/活动 DTO 的完整时间、原始 UTC、未开始负例。
- `R15TaskTimelineTests` `3/3`：时间线 UTC 顺序、未知时间负例、Task Center 与 Overview 的绑定契约。
- `OverviewInteractionTests` `1/1`、`R10RecentAccessBehaviorTests` `2/2`；本批选定 Playnite 行为测试合计 `12/12`。
- 提交身份 Release 构建：Playnite `net462` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；Playnite Tests `net472` `0 warnings / 0 errors`。`validate-source.py`、XAML `24/24`、`git diff --check` 通过；WPF 静态检查 `0 errors / 27 warnings / 177 info`，warning/info 为既有布局/主题资源规则提示。

## 未验边界

- Maintenance、Save 及其他日期/时间入口尚未全部迁移；相对时间随 DTO 刷新或绑定重建更新，没有新增系统时钟跳变模拟或跨系统启动周期验证。
- 本子批次没有新增真实剪贴板动作；完整时间/原始 UTC 只验证了 DTO、Tooltip、HelpText 和现有隔离 WPF 行为，未声称真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/跨屏、最终呈现、ETW 或宿主性能已验。
- Demo 原目录不可用，继续使用恢复生产基线；业务验证使用合成 DTO/fake/隔离测试宿主，未写真实存档、媒体、云端或诊断；main 用户改动未碰、未合并。

## 下一步

继续 R22-01：先盘点 Maintenance/Save 现有 `CreatedLocal`、`CreatedDisplay`、`StringFormat` 和复制入口，逐个复用同一 formatter 并补对应负例；随后再处理真实系统时钟与宿主边界。
