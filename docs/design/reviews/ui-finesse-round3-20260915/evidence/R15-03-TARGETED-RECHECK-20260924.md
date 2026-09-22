# R15-03 任务详情时间线定向复核

日期：2026-09-24  
工作区：`D:\\workplace\\github\\GameSaveCenter`  
分支：`codex/ui-finesse-round2`  
当前复核代码身份：`b6170ec9`  
实现提交：`fe0c05a9`；本批行为边界补测：`b6170ec9`

## 结论

R15-03 已按当前实现和隔离测试结果收口为“已满足，待环境验证”。已有时间线链路可以复用，不需要重建任务历史服务、DTO 或详情滚动体系。本批只补乱序事件、同刻排序和跨任务过滤的行为证据，生产实现未改。

## 已核对的现有能力

- `TaskTimelineBuilder` 复用 `TaskStatusDto` 的创建/开始/结束时间和 `TaskChangeEventDto` 的观察事件；已知事件按 UTC 时间、再按稳定序号排序，缺少时间显示“时间未知”，没有可关联事件时不生成重试记录。
- `TaskCoordinator` 在任务持久化成功后写入 `OccurredUtc` 并发布快照；`TaskEventBroadcaster` clone 保留 `StageMessage` 与 `CancellationState`，不丢失 R15-01/R15-02 的阶段和取消语义。
- Dashboard 运行期时间线窗口限制为最多 64 条/任务、最多 200 个任务；Task Center 详情时间线卡沿用现有外层滚动与主题资源，内部高度上限为 `220 DIP`，未替换游戏选框、滚动条、命令绑定、取消/错误/恢复保护或 `net462` 路径。

## 受控验证

- Worker `TaskCoordinatorFailureTests` + `TaskEventBroadcasterTests`：`12/12` 通过。
- Playnite `R15TaskTimelineTests`、`TaskEventUiBatcherTests`、`R15TaskStageTests`、`R15TaskCancellationTests`、`R06TaskProgressBehaviorTests` 合计：`15/15` 通过；其中 R15-03 时间线行为夹具为 `4/4`。
- 新增行为负例确认：乱序到达的事件按 `OccurredUtc` 排序，同一时间按 `Sequence` 稳定排序，其他任务事件不会混入；已有夹具继续确认不臆测重试、legacy 无 UTC 时间明确显示未知。
- 当前隔离 Release 构建：XAML `24/24`，solution `0 error / 2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite 目标 `net462`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1 -ProjectRoot (Get-Location)` 与 `git diff --check` 通过；WPF 静态检查（`src/GameSaveCenter.Playnite`）为 `0 errors / 28 warnings / 162 info`，warning/info 为仓库既有 StackPanel/Canvas/主题硬编码提示，未见本批新增 error。

## 视觉与边界

- 人工复查既有 `artifacts/ui-qa-r13-r14-clean-20260922/Task-1600x900.png` 与 `Task-1920x1080.png`：任务列表、阶段未知负例、取消/失败状态和详情当前阶段可见；两张图均未显示打开的时间线卡，因此不把它们写成时间线呈现或最终 presented frame 证据。
- 本批使用合成 DTO、fake/隔离 testhost 和隔离构建输出，没有写真实存档、媒体、云端或用户诊断；Demo 原目录不可用，沿用恢复生产基线。
- 未验真实 Worker 重启后历史事件恢复（当前窗口是运行期有界缓存，持久任务字段仍可用）、真实 Playnite/package-host、最终呈现帧、UIA/读屏/IME、物理 DPI/跨屏、ETW 或宿主性能；不从同名任务猜测重试关系。

下一可执行小批量：`R15-04 重复通知归并`，先核对已有通知、会话摘要和失败历史入口，补重复进度/失败负例并保留重要新失败。
