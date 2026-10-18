# S02-01 通知记录契约

日期：2026-10-18。实现提交：`83194c50`（`新增持久通知记录契约`）。

## 现有能力与持久化边界

现有 `NotificationLevelPolicy` 和插件通知事件只负责决定/呈现临时提示；它们不保存已读状态或重启后通知记录。Worker 的 `SqliteStateStore` 已负责持久任务和审计数据，因此通知摘要由同一 SQLite 数据库持有。任务表仍是任务状态唯一权威来源；通知只保存关联 ID 和短 UI 摘要快照。

新增 `NotificationRecordDto`，字段包括通知 ID、稳定去重键、可选任务/游戏 ID、级别、标题/摘要、UTC 时间和已读状态。`NotificationRecordPolicy` 将未知级别降为 Information，限制 ID/文本长度，压平换行并拒绝空/超长去重键。摘要是本地用户可读文本，不用于外发诊断。

`notification_records` 使用唯一 `deduplication_key`，写入采用 `INSERT OR IGNORE`；同一事件重放不会覆盖首条记录。SQLite 写入事务同时删除超过 30 天的记录并将集合限制为最近 500 条。查询最多返回 500 条。S02-02 后续接入通知生产者与收件箱入口；本阶段尚未把临时 toast 接入记录表。

## 验证状态

- `python scripts/validate-source.py .`：通过。
- `git diff --check`：通过。
- SDK `8.0.423` Release 解决方案构建成功（0 warning / 0 error）。新增 `NotificationRecordPersistenceTests` `3/3` 通过；完整 Worker suite `364` passed、`1` skipped。TRX：[notification-records-sdk8423.trx](tests/notification-records-sdk8423.trx)，SHA-256 `9D89AF7F28591FE57DBC4C63BBC217A4B788B2D72CA01B88187BCF88E16B93CF`。
- 为 SQLite 测试夹具在删除隔离数据库前清理连接池，避免文件句柄在 fixture 清理阶段仍被池化连接持有；生产数据库与存储语义未因此改变。另修复了 nullable 输入归一化告警并补齐 Worker 日志扩展命名空间。
- 未修改 XAML，未发送外部通知，也未验证 Playnite host。

## 下一步

受控构建与新增 Worker 测试现已通过，S02-01 可更新为已验收。通知生产者尚未接入；后续 S02-02 再将现有通知生产路径接入 `TryAddNotificationAsync` 并提供收件箱入口。Round4 总计划仍进行中。
