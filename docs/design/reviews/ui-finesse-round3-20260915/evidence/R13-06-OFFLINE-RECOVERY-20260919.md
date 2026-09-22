# R13-06 离线恢复反馈证据

日期：2026-09-19
代码提交：`e4244329`（已推送 `origin/codex/ui-finesse-round2`）

## 实施事实

- 先查明现有 `CloudRetryPolicy` 已提供有界退避：1、5、15、60、240、720 分钟，最多 6 次自动重试；`CloudRetryService` 已提供 30 秒轮询、每轮最多处理 10 项和顺序处理逻辑。
- 本阶段没有重建队列或改变上传、校验、取消、错误和恢复语义，仅在共享 `CloudTransferStatusDto` 增加 `NetworkRecoveryDisplay`，并将它接入维护页现有详情区域。
- `RetryScheduled` 且错误为 `RCLONE_NETWORK_FAILED`/`RCLONE_TRANSFER_INCOMPLETE` 时显示等待网络恢复和退避/批次边界；`Transferring` 时显示网络已恢复、按批次上传中；认证失败保持空文案，不误报为网络恢复。
- 维护页继续使用既有详情滚动容器、共享卡片样式、命令绑定和选择模型，没有改游戏选框、滚动条、取消/错误/恢复保护、有限列表或 Playnite `net462` 兼容路径。

## 实际验证

- Core 定向行为：`CloudRecoveryExplainsBackoffAndOnlineBatchTransition`，`1/1`。
- Worker 既有退避、持久化与限制行为：`CloudRetryPersistenceTests`，`10/10`。
- Playnite 阶段行为：`R13CloudTransferStageBehaviorTests`，`11/11`；包含维护页绑定、30 秒轮询、每轮最多 10 项和不逐条通知的源码边界检查。
- Release 外部隔离 solution 构建：`0 warning / 0 error`，包含 Playnite `net462`；XAML 结构检查 `24/24`；`python scripts/validate-source.py` 和 `git diff --check` 通过。
- 本批 `r13-06-source`、`r13-06-build` 临时目录已清理；旧 `.tmp/r12-07-build-final` 因 Access denied 仍保留，未扩大清理范围。

## 未验证边界

- 以上证据来自合成 DTO、fake/隔离 SQLite、源码边界测试和外部隔离构建；没有连接真实网络/rclone、真实远端、真实存档/媒体或发送诊断。
- 未运行真实 Playnite/package-host、RenderHarness、最终呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能测试，因此不把“按批次”静态/测试证据写成真实网络恢复时序或物理性能证明。
- Demo 原目录不可用，WPF 技能仅按 Demo-first 基准做共享详情容器、样式和绑定质量检查；沿用恢复生产基线。
- main 保持用户已有脏改动和 `src.zip`，本阶段未合并、未覆盖、未修改。

## 下一步

下一可执行任务是 `R13-07 队列筛选与汇总`：先查现有状态筛选、全局计数、分页和选中项联动，再补筛选负例与汇总证据。
