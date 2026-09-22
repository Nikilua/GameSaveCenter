# R15-08 清理历史范围定向复核

日期：2026-09-23

复核提交：`569fa1d8`（`codex/ui-finesse-round2`，D 盘工作区）

## 结论

R15-08 的受控实现条件继续满足，账本状态保持“已满足”。本批没有新增生产代码，复用了 `88bde5de`、`a841e42c`、`77d5f346` 和 `a07f0518` 已提供的清理预览、保护门禁与隔离账本，仅复测当前提交上的 Worker、维护页契约和 net462 构建。

## 实际复测

- Worker `RetentionSimulationServiceTests` + `RetentionQuarantineRecoveryTests`：`17/17` 通过。覆盖日期/原因/影响摘要、锁定/健康恢复点/PreRestore 保护、二次确认、预览句柄过期与状态变化、游戏级共享锁忙碌跳过、归档身份校验、隔离账本恢复和人工确认负例。
- Playnite `MaintenanceReportSourceTests` + `R17QuarantineLedgerSourceTests`：`6/6` 通过。维护页继续绑定日期、原因和完整影响摘要，列表保持有限滚动，清理应用入口和隔离账本恢复入口仍有契约覆盖。
- Playnite `net462` 定向构建完成，无新增错误；保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。
- `python scripts/validate-source.py`、XAML 结构检查 `24/24`、`git diff --check` 通过。

## 保留能力与边界

- 执行仍只接受明确确认和有效预览句柄；候选文件限定在配置备份根目录 ZIP，运行中任务记录、锁定版本、PreRestore、健康恢复点和身份变化对象不进入误删路径；移动前写入隔离账本，失败状态保留恢复入口。
- 测试使用合成版本、fake/隔离 SQLite、临时归档和隔离锁，没有修改真实存档、媒体、用户云端、诊断或系统剪贴板，也没有删除真实历史。
- 未启动真实 Playnite/package-host，未验真实清理呈现、UIA/读屏、IME、物理 DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。Worker 全量既有 `MediaSyncService.cs:570` 失败不改写为通过。

下一可执行任务：`R16-01 设置搜索定位`。先核对现有设置页筛选/分组/滚动和命令绑定，再补最小定位行为证据。
