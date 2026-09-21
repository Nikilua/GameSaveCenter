# R22-01 任务单调时长与恢复（2026-09-21）

## 本子批次结论

提交 `538fcae9058c63cbc84d6f6e62e51e1f4ed48cdb` 完成 R22-01 的任务时长子批次，但 R22-01 整项仍为“部分满足，待继续”。相对时间/完整时间已在任务时间线接入；本批补齐新任务的单调计时、SQLite 快照和 Worker 重启收口，未把其他页面的旧时间入口或真实宿主验证提前写成完成。

## 实现边界

- Contracts 新增 `MonotonicTaskClock`，任务运行时记录 `Stopwatch.GetTimestamp()` 与频率，终态将计数器差值写入 `TaskStatusDto.ElapsedSeconds`；`DurationDisplay` 对新任务不再用 `StartedUtc`/`FinishedUtc` 计算运行时长。
- SQLite `tasks` 增加 `elapsed_seconds`、`monotonic_started_timestamp`、`monotonic_frequency` 三列；`EnsureColumn` 为旧库增量迁移，查询、写入、活动任务和最近任务读取均保留这些字段。旧任务没有单调字段时保留 legacy wall-time fallback，不伪造历史精度。
- `MarkInterruptedTasksAsync` 在 Worker 重启/手动协调时用保存的同一单调计数器收口运行时长，再清除起点并保留原有 `WORKER_RESTARTED*`、取消状态、错误和恢复保护语义。未修改任务排序、游戏选框、滚动条、命令绑定或有限列表策略。

## 实际验证

- `R22TaskDurationBehaviorTests` `2/2`：固定计数器差值/逆序负例，以及合成长墙钟年龄任务的终态时长仍为短单调运行时长。
- `TaskQueryPersistenceTests` `9/9`：单调字段 round-trip、重启收口、现有分页/摘要/进度/索引行为。
- `TaskCoordinatorFailureTests` `7/7`：成功、业务失败、取消、终态持久化失败和游戏锁语义保持。
- `DatabaseMigrationHarnessTests` `4/4`：新鲜库、旧字段库和更旧 fixture 增量升级并保留数据。
- 隔离真实 Worker 进程硬重启 `WorkerProcessRestartTests` `1/1`：任务仍按原 Worker 身份收口为失败并保留可重试错误码。
- 最终提交身份构建：Playnite `net462` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；Playnite Tests `net472` `0/0`；Worker Tests `net8` `0/0`。提交身份下 R22 时间显示 `5/5`、R15 时间线 `3/3`；source validation、XAML `24/24`、diff 通过。

## 未验边界

- 没有实际修改 Windows 系统时钟、跨重启系统启动周期或模拟 QPC 不连续；本批只用固定计数器、隔离 SQLite 和隔离 Worker 进程验证同一主机计数器路径。
- Maintenance、Save 等其他时间入口仍未全部迁移到共享相对/完整/时区提示；Overview 主要任务/活动入口已由 `b53ab44f` 接入。原始 UTC 尚无新的真实剪贴板动作。真实 Playnite/package-host、UIA/读屏、OS 输入/IME、DPI/跨屏、最终呈现、ETW 和宿主性能未验。
- Demo 原目录不可用，继续使用恢复生产基线；未写真实存档、媒体、云端或诊断，main 用户改动未碰、未合并。

## 下一步

继续 R22-01 的剩余小批量：先盘点 Maintenance/Save 其他日期入口和已有复制行为，复用当前 formatter/技术文本控件接入相对、完整和原始 UTC 显示；再补系统时钟跳变及真实宿主可执行边界，不把离屏/代理结果当成呈现证据。
