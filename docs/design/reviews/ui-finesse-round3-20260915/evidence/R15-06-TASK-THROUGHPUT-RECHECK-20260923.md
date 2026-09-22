# R15-06 耗时与吞吐定向复核

日期：2026-09-23

复核提交：`d9dc4317`（`codex/ui-finesse-round2`，D 盘工作区）

## 结论

R15-06 的受控实现条件已满足，账本状态校正为“已满足，待环境验证”。本批没有新增生产代码，复用了 `6f65638e` 已提供的单调采样、可靠总量门控、SQLite/广播字段和 Task Center 显示条件，仅复测当前提交上的行为与 net462 构建。

## 实际复测

- Worker 采样、SQLite 查询、事件广播和失败回归：`TaskProgressMetricsTests`、`TaskQueryPersistenceTests`、`TaskEventBroadcasterTests`、`TaskCoordinatorFailureTests` 合计 `22/22` 通过。
  - 两个推进样本后才产生速率/ETA；进入普通未知阶段会清空采样。
  - 未知总量、等待确认和超过 10 秒无推进的任务不显示速率/ETA。
  - 可靠采样字段经过最近/分页查询和事件 clone 后仍保留。
- Playnite 进度/快照/任务详情相关套件：`R15TaskProgressMetricsTests`、`R06TaskProgressBehaviorTests`、`R22TaskDurationBehaviorTests` 合计 `5/5` 通过。速率变化会被快照比较器视为可见状态，任务耗时仍使用既有单调时钟语义，进度行和取消状态保持分离。
- Playnite `net462` 定向构建实际完成，无新增错误；保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。
- `python scripts/validate-source.py`、XAML 结构检查 `24/24`、`git diff --check` 通过。

## 保留能力与边界

- 继续复用 `TaskProgress`、`TaskStatusDto`、SQLite 任务查询、广播 clone、Task Center 详情、游戏选框、滚动容器、命令绑定、取消/错误/恢复保护和 Playnite/net462；不以阶段百分比猜测吞吐，不为远端 rclone、恢复写入或未知总量伪造 ETA。
- 测试使用合成任务、fake 服务和隔离 SQLite/测试宿主，没有读写真实存档、真实媒体、用户云端或对外诊断。
- Worker 全量历史门禁中 `MediaSyncService.cs:570` 的既有失败仍不改写为通过；本批只报告定向 `22/22`。未启动真实 Playnite/package-host，未验最终呈现、UIA/读屏、IME、物理 DPI/跨屏、presented frame、ETW 或宿主性能。
- Demo 原目录不可用，沿用已恢复生产基线，没有引入新的设计体系；离屏/测试宿主结果不冒充真实宿主呈现。

下一可执行任务：`R15-07 失败结果复制`。先核对现有摘要、错误码、脱敏详情和剪贴板失败重试语义；R15-06 保留真实宿主和 Worker 全量既有失败边界。
