# S02-01 通知记录契约

日期：2026-10-18。实现提交：`83194c50`（`新增持久通知记录契约`）。

## 现有能力与持久化边界

现有 `NotificationLevelPolicy` 和插件通知事件只负责决定/呈现临时提示；它们不保存已读状态或重启后通知记录。Worker 的 `SqliteStateStore` 已负责持久任务和审计数据，因此通知摘要由同一 SQLite 数据库持有。任务表仍是任务状态唯一权威来源；通知只保存关联 ID 和短 UI 摘要快照。

新增 `NotificationRecordDto`，字段包括通知 ID、稳定去重键、可选任务/游戏 ID、级别、标题/摘要、UTC 时间和已读状态。`NotificationRecordPolicy` 将未知级别降为 Information，限制 ID/文本长度，压平换行并拒绝空/超长去重键。摘要是本地用户可读文本，不用于外发诊断。

`notification_records` 使用唯一 `deduplication_key`，写入采用 `INSERT OR IGNORE`；同一事件重放不会覆盖首条记录。SQLite 写入事务同时删除超过 30 天的记录并将集合限制为最近 500 条。查询最多返回 500 条。S02-02 后续接入通知生产者与收件箱入口；本阶段尚未把临时 toast 接入记录表。

## 验证状态

- `python scripts/validate-source.py .`：通过。
- `git diff --check`：通过。
- 新增 `NotificationRecordPersistenceTests` 覆盖同去重键重放、Worker 重启读回、30 天过期拒绝、500 条上限、文本归一化和未知 severity 回退；尚未运行。
- 当前工作区只安装 SDK `9.0.302`，仓库要求精确 SDK `8.0.423`。本阶段未运行 .NET build/test；不使用 SDK9 冒充仓库门禁。
- 未修改 XAML，未发送外部通知，也未验证 Playnite host。

## 下一步

取得 SDK8.0.423 后运行 Worker `NotificationRecordPersistenceTests` 和目标构建。随后 S02-02 可将现有通知生产路径接入 `TryAddNotificationAsync`，并提供收件箱入口。Round4 总计划仍进行中。
