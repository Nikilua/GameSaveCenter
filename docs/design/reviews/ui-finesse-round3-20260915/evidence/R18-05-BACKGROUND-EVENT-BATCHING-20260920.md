# R18-05 后台事件合并证据（2026-09-20）

## 结论

R18-05 在 `91947336` 已满足受控验证条件：复用现有 `TaskEventBroadcaster`、任务变更 durable feed、`TaskIndexedCollection`、`BatchObservableCollection` 和 Dashboard 卸载取消路径；只把高频任务/媒体任务事件的 UI 入口改为有界合并，并保留终态优先和恢复刷新语义。

- Playnite `TaskEventUiBatcherTests`：`3/3`。
- Worker `TaskEventBroadcasterTests`：`5/5`。
- 与任务进度、时间线、索引和通知去重相关 Playnite 回归：`19/19`。
- `python scripts/validate-source.py`：通过；XAML 结构门禁 `24/24`；`git diff --check`：通过。
- Release 隔离 solution：`0 errors / 2 warnings`，两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:671` nullable warning；`.tmp/r18-05-solution` 和本阶段 TestResults 已清理。

## 实现与行为

### Playnite UI 批次

- `TaskEventUiBatcher` 以 TaskId 合并进度快照，待处理任务数上限为 `128`，每次最多向 WPF 投递 `32` 条；同一任务只保留序列较新的进度。
- 完成、失败、取消不进入进度队列，而是以 `DispatcherPriority.DataBind` 立即投递；同 TaskId 已排队的旧进度先移除，避免迟到 Running 覆盖终态。
- `DashboardViewModel.ApplyTaskEventBatchOnUi` 复用 `Tasks.ApplyBatch`、`TaskIndexedCollection` 和现有 Overview/选择/时间线/命令状态更新；因此一批事件只触发一次集合 Reset，而不是每个事件一次 Dispatcher/布局入口。
- `DashboardView.OnUnloaded` 已有 `StopTaskEventSubscription`；本轮在该路径释放批处理器并清空待处理队列，已排队的回调执行时变为空操作。现有请求取消、错误和恢复刷新语义未改。

### Worker 事件队列

- 每个订阅仍固定容量 `128`，不改当前用户管道、并发客户端上限和 durable `TaskChangeFeed`。
- 满载时手动淘汰最早的非终态事件；完成/失败/取消优先保留，不再因随后大量进度而被 `DropOldest` 淘汰。若订阅者完全不读取且终态自身超过固定容量，durable change feed/快照仍是恢复来源；本证据不把瞬时事件管道写成永久日志。
- `TerminalOutcomeSurvivesLaterProgressPressure` 实测在“先失败、再 200 条进度”的压力下仍保留失败终态，队列总数保持 `128`，最后进度序列为 `201`。

## 原始受控样本

### UI 批处理

| 场景 | 结果 |
|---|---:|
| 200 个不同 TaskId 的 Running 事件 | 待处理最多 `128` |
| 进度批次最大大小 | `32` |
| 200 条输入最终应用 | `128`（序列 `73..200`，早期进度按有界策略淘汰） |
| 终态后旧 Running 是否回写 | 否；最终只应用 Failed |
| Dispose 后已排队回调 | 不再触发应用 |

### Worker 扇出

| 场景 | 结果 |
|---|---:|
| 双订阅独立快照、深拷贝 | 通过 |
| 重复订阅/释放 200 次 | 无残留订阅 |
| 200 条普通进度压力 | `128` 条窗口，保留序列 `72..199` |
| 失败终态后 200 条进度 | `128` 条窗口，失败序列 `1` 保留 |

## 门禁与未验边界

- 测试只使用合成 `TaskChangeEventDto`、fake 调度器、隔离 testhost 和现有内存/SQLite 测试夹具；没有修改真实存档、媒体、云端或用户目录。
- 已验证的是 WPF Dispatcher 投递/集合批次和 Worker 有界内存扇出，不是最终 presented frame、DWM/60fps、物理 DPI/跨屏、UIA/读屏、真实 Playnite 嵌入宿主或 ETW。
- Demo 原目录不可用；本轮未改页面视觉体系、游戏选框或滚动条系统。真实 Playnite 页面卸载/重载时序和大规模真实任务流仍待宿主验收。
- 下一可执行任务：`R18-06 页面重访成本`，先复用现有 workspace 生命周期、请求 generation、取消和缓存快照，测量页面切换后是否停止无用刷新并保持旧数据/错误语义。
