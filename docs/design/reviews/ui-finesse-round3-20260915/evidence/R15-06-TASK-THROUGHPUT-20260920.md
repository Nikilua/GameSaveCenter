# R15-06 耗时与吞吐

日期：2026-09-20  
代码提交：`6f65638e`（补充任务耗时与吞吐采样）  
分支：`codex/ui-finesse-round2`

## 结论

R15-06 的可控实现已完成，状态记为“已实现，待环境验证”。任务详情继续使用已有 `StartedUtc`/`FinishedUtc` 计算耗时；速率和剩余时间只有在 Worker 明确报告“已完成单位、总单位、单位名称”且存在推进样本时才出现。未知总量、等待确认、普通阶段、长时间停顿和切换到非工作阶段均显示未知，不用阶段百分比猜测 ETA。

## 实现事实

- `TaskProgress.ReportWorkAsync` 使用单调时钟记录最多 5 个推进样本；至少两个推进样本才计算速率，15 秒以上没有推进会重置采样窗口，10 秒没有新推进时隐藏速率和 ETA。
- `TaskStatusDto` 增加可选的完成单位、总单位、单位、平滑速率、ETA 和最后推进时间；`Task Center` 详情新增“可靠进度采样”卡片，只有 `HasReliableProgressMetrics` 为真时显示。现有任务表、选框、滚动容器、命令绑定和取消/错误语义未替换。
- SQLite `tasks` 追加采样列并通过 `EnsureColumnAsync` 迁移；最近、活动和分页查询均回读。旧库使用 `-1`/空值，不会把未知任务显示为零。
- 只在可靠来源接入：整库备份使用明确的游戏总数，游戏专属媒体同步使用明确的候选文件总数，FLiNG 下载使用 HTTP 已知总字节；远端 rclone 下载、恢复写入和无总量的阶段仍不推算。
- `SnapshotComparers.Task` 纳入采样字段，刷新时不会因百分比未变而吞掉速率变化；广播 clone 和任务协调器 clone 均保留采样字段。

## 行为证据

- Worker `TaskProgressMetricsTests` 验证采样需要推进样本、产生速率/ETA，进入未知阶段后清空；`TaskQueryPersistenceTests` 验证 SQLite 最近/分页回读，以及未知总量、等待确认和 11 秒停顿负例均不显示速率/ETA。
- Playnite `R15TaskProgressMetricsTests` 验证任务快照比较器把采样变化视为可见状态。
- 最终提交身份定向测试：Worker 任务查询/采样/广播/失败回归 `20/20`；Playnite R15 `11/11`。
- `python scripts/validate-source.py`、XAML `24/24`、`git diff --check` 通过；WPF 静态审查 `0 errors / 28 warnings / 162 info`。warning/info 是当前共享 Canvas、滚动容器和颜色令牌提示，本批未新增模板警告。

## 构建与边界

- 当前提交外部隔离源码副本的 Release solution 构建实际到达 Playnite `net462`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:664` nullable warning；Core `106/106` 通过。
- Worker 全量门禁为 `342 passed / 1 skipped / 1 failed / 344 total`，失败是既有 `MediaSyncServiceTests.ClassificationApplyUsesSelectedStableIdsAndValidatedTargetOverride`，位置为 `tests/GameSaveCenter.Worker.Tests/MediaSyncServiceTests.cs:570`，与本批任务采样无关；脚本按门禁停止，未写成全量通过。定向 Worker `20/20` 和 Playnite R15 `11/11` 使用同一最终提交产物复跑通过。
- linked worktree 直接构建仍受既有 `obj` 写入 `Access denied` 影响，因此采用当前分支外部源码副本和独立输出根；未宣称真实 Playnite/package-host、RenderHarness、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能通过。
- 证据只来自合成数据、fake 服务、隔离 SQLite/目录和隔离构建；未写真实存档、媒体、用户云端或诊断。Demo 原目录不可用，沿用恢复生产基线。

下一可执行小批：`R15-07 失败结果复制`；同时保留上述 Worker 全量既有失败和真实宿主呈现边界，先核对现有复制命令、错误摘要和脱敏策略。
