# R15-03 任务详情时间线证据

日期：2026-09-20
代码提交：`fe0c05a9 增加任务详情时间线`
分支：`codex/ui-finesse-round2`

## 实际完成

- 先核对现有 `TaskChangeEventDto`、`TaskCoordinator`、Worker 事件广播、Task Center 详情和任务页滚动容器；没有另建任务历史服务，也没有把任务行刷新快照直接当成重试记录。
- 变更流增加 Worker 观察到的 `OccurredUtc`。`TaskCoordinator` 在成功持久化后发布带 UTC 时间的快照，广播 clone 同步 `StageMessage` 和 `CancellationState`，避免实时事件丢失 R15-01/R15-02 的真实阶段。
- Contracts 的 `TaskTimelineBuilder` 仅整理已知创建、开始、阶段、取消和结束记录；按真实 UTC 时间再按序号稳定排序，同时显示本地时间和 UTC。旧事件缺少时间时显示“时间未知”，没有可关联的重试事件时不猜测、不生成重试条目。
- Dashboard 只保留最多 64 条/任务、最多 200 个任务的内存事件窗口；Task Center 详情新增有限高度的时间线卡，沿用现有外层 ScrollViewer 和主题资源，不改变游戏选框、滚动条、命令绑定、取消/错误/恢复保护或有限列表性能边界。

## 证据

- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1 -ProjectRoot (Get-Location)`：24/24 通过。
- `git diff --check`：通过。
- Worker Release 隔离项目构建并执行 `TaskCoordinatorFailureTests|TaskEventBroadcasterTests`：11/11 通过，退出码 0。
- Playnite Release 构建目标为 `net462`，使用 `GscBuildCommit=fe0c05a9` 执行 R06 取消回归、R15-01 阶段、R15-02 取消和 R15-03 时间线夹具：11/11 通过，退出码 0。构建保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning，无错误。
- WPF 静态质量检查：0 errors / 28 warnings / 162 info；XAML 没有新增结构错误。本批 4 个隔离输出目录已清理。

## 边界与未验项

- 本批使用 Worker/Playnite 项目级隔离构建与测试，没有把完整 solution、RenderHarness 或真实 Playnite 宿主结果写成通过。
- 只使用合成 DTO、fake/内存事件和隔离输出；未写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线。
- 未验真实 Worker 重启后历史事件的持久化时间线（当前只保留任务本身的持久字段和运行期有界事件窗口）、真实 Playnite/RenderHarness/最终 presented frame、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能；未从同名任务猜测重试关系。
- main 分支的用户改动 `DashboardView.xaml.cs`、对话框基础设施和 `src.zip` 未触碰、未合并。

下一可执行任务：`R15-04 重复通知归并`，先查现有任务通知、会话摘要和失败历史入口，再补去重键、重要失败保留和屏幕通知负例。
