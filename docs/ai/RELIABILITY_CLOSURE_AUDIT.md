# 可靠性闭环 14 项最终验收

> 审计日期：2026-08-13
> 原始范围：`RELIABILITY-RESTORE-001` 至 `RCLONE-RELIABILITY-001`
> 原则：AUTO VERIFIED 与 MANUAL QA REQUIRED 分开；不得用单元测试冒充真实游戏、真实云端或真实多设备验证。

## 逐项结论

| 阶段 | 结论 | 主要权威证据 |
|---|---|---|
| RELIABILITY-RESTORE-001 | AUTO VERIFIED | `RestoreReadinessTests` 覆盖正常/损坏/Manifest/Hash/取消/清理；`RestoreOrchestratorTests` 10 个临时目录灾难场景覆盖 PreRestore、Restore、PostRestore、Rollback、Undo、权限/只读/目录缺失/部分写入。任何实际写入后的异常均统一回滚。 |
| HEALTH-001 | AUTO VERIFIED | `GameHealthAssessmentService` 四态；`GameHealthAssessmentTests` 覆盖近期成功、连续失败、长期未玩、云启用/禁用和异常 finding；Dashboard 使用已有聚合快照，不在 UI getter 做 IO。 |
| PROTECTION-001 | AUTO VERIFIED | `RecentProtectionAssessmentService` 复用 7/30/90 天窗口；测试覆盖未识别、从未备份、策略关闭、过旧、云异常与不可恢复；首页使用同一评估结果。 |
| POLICY-001 | AUTO VERIFIED | 内置五模板、用户模板 CRUD、内置不可删除、一次性复制应用；`BackupAnomalyProtectionLevel` 已纳入模型、克隆、快照比较和 UI，重要游戏模板为 Strict。 |
| ONBOARDING-001 | AUTO VERIFIED | Worker/IPC/Ludusavi/数据与备份目录/SQLite/Playnite 库/Rclone/remote/media/磁盘空间检查复用 Maintenance；未配置 Rclone 为可选；环境服务与 UI 门禁有测试。 |
| GAME-TOOL-003 | AUTO VERIFIED | 默认 Skip；`GameToolProcessGuard` 只按完整 EXE 路径匹配，路径不可读保守阻止；纯决策测试覆盖 Skip/Restart/AllowAnother，真实测试自有 EXE 证明同名不同路径不受影响且 Restart 只结束精确路径。 |
| GAME-TOOL-004 | AUTO VERIFIED | Trainer/CT 在反作弊下阻止；Custom Unknown/GameModification 阻止，GeneralUtility 允许；分类持久化并写审计，测试覆盖四类决策。 |
| SMART-PROTECT-001 | AUTO VERIFIED | 首次完整 session 后识别存档再提示；NeverShown/Deferred/Enabled/Dismissed 持久化，Deferred 冷却，三种显式选择有 UI 测试。 |
| SMART-PROTECT-002 | AUTO VERIFIED | 最近保护列表与 PROTECTION 共用评估服务；批量操作先预览后确认；已有自定义策略默认不选且测试证明其他字段不被覆盖。 |
| BACKUP-DIFF-001 | AUTO VERIFIED | `FileManifestDiffService` 按 Path+Hash 或 Path+Size+mtime 输出 Exact/Estimated 与增删改摘要；版本详情复用持久化 Manifest。 |
| BACKUP-GUARD-001 | AUTO VERIFIED | Off/Normal/Strict 比较阈值；文件大量删除产生 `BACKUP_FILE_REMOVAL_SPIKE`；Retention 永久保留用户 Lock、PreRestore 和健康恢复点，自动删除仍未启用。 |
| NOTIFY-001 | AUTO VERIFIED | `GameSessionSummaryBuilder` 从 Task Center 同一终态构建一次 session 摘要；本地成功/云失败正确显示部分成功，session 聚合器只发一次最终消息。通知级别是原文“建议”，现有全局启用开关保持不扩张。 |
| MULTI-DEVICE-001 | AUTO VERIFIED（真实双机待人工） | 稳定 DeviceId、父版本分叉检测、A3/B3 不自动选胜者、PreferLocal/PreferRemote/KeepBoth 持久化；远端只进入 staging→check→Ludusavi 校验→既有安全恢复链，决不自动覆盖/合并/删除。 |
| RCLONE-RELIABILITY-001 | AUTO VERIFIED（真实 remote 待人工） | 执行级命令白名单拒绝 sync/move/delete/purge；错误分类、有限 1/5/15/60/240/720 分钟退避、手动重试、重启中断任务收口、外部进程取消、staging checksum 与版本验证均有自动测试。本地备份不会因云失败被删除。 |

## 最终自动门禁

- Release solution build：0 warnings / 0 errors。
- Core：42/42。
- Worker：117/117。
- Playnite：203/203。
- `scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：13 个 XAML 通过。
- `scripts/render-qa.ps1`：多窗口页面渲染、滚动几何和 Settings 布局通过。
- `scripts/dev-install-run.ps1`：隔离构建、测试、打包、普通用户安装、版本验证和 Playnite 启动通过。
- 真实宿主日志：Playnite 加载 GameSaveCenter 0.6.70；安装目录 Worker 进程运行并完成 Dashboard IPC。

## MANUAL QA REQUIRED

- 用真实游戏执行 Restore A、Undo 回 B，以及人工确认存档内容。
- 用真实 Rclone remote 演练上传中断网、恢复网络、凭据失效、部分远端上传与 staging 下载。
- 用两台真实设备制造 V1→A2/B2→A3/B3，人工体验三种决策和保留双方。
- 真实 EXE/LNK/BAT/CMD/PS1 与反作弊游戏流程。
- 真实 1000～2000 游戏库的 IPC/渲染帧率。
- Light/Dark/Follow Playnite/高对比度、100%～200% DPI、键盘导航和连续缩放。
