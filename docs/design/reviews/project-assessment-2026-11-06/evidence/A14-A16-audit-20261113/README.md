# A14–A16 功能候选去重审查（2026-11-13）

## 范围

- 审查产品源码：`b667e22445fbf57aa591f39f3d968f5c166c7cb6`；后续提交截至本批前仅改文档。
- 按功能候选规则先记录当前入口、缺口与收益；本批不修改源码、测试、资源或脚本，不启动Playnite，也不执行恢复、删除、云写、下载或工具启动。
- 三项仍是候选发现，不因审查成为已确认缺陷。

## A14：备份可信度摘要

- 当前入口：Save Center版本详情已显示创建时间、来源设备/系统、文件数/大小、锁定与保留保护；“恢复可用性”继续显示状态、摘要、实际/预期文件和大小、哈希结果/覆盖率及相对/完整检查时间，并提供真实`ValidateRestoreReadinessCommand`。版本行已有`ProtectionAndReadinessDisplay`，维护中心另有完整性检查和隔离恢复巡检。
- 缺口：`BackupVersionDto`没有按单个版本持久化远端校验状态或“曾完成隔离恢复”的证明；现有`GameStatusDto.CloudState`是游戏级聚合，不能可靠归因到所选版本。直接拼统一分数会把未知误写成安全。
- 收益判断：现有信息已能指导“先验证可恢复性”；只有建立版本级远端/演练证据契约后，增加中性摘要才有净收益。
- 决策：暂不实现分数或新卡片，不复制现有字段；候选保持“数据契约待业务需求”。

## A15：支持诊断摘要

- 当前入口：Maintenance提供“复制诊断”“导出诊断包”“复制健康报告”“导出健康报告”。诊断包先返回包含/排除类别、脱敏方式和2 MiB/日志上限预览；包内`system.json`/`worker.json`包含插件、Worker、Playnite、协议和构建身份。健康报告按待处理/已验证/未知分段，并同样包含软件身份。
- 缺口核对：`DiagnosticsPackageService`统一脱敏，明确排除真实存档、媒体、SQLite内容、Rclone配置、凭据、令牌和自动上传；`MaintenanceReportService`也在输出前统一脱敏。未发现候选所述的身份、失败阶段或脱敏预览断层。
- 收益判断：再建一套“支持摘要”会与现有诊断包/健康报告重复，增加术语和维护成本。
- 决策：现有能力满足，不新增入口、不自动上传。

## A16：跨电脑迁移清单

- 当前入口：元数据灾备导出包含SQLite一致性快照、Worker设置、插件便携设置和manifest；恢复先解析包、校验schema与各项SHA并列出entries，确认执行前保留恢复前副本。批量路径迁移先返回类别、原路径、新路径、目标存在状态、影响行数与缺失目标数；确认前不写入、不移动文件，执行前自动生成元数据灾备。
- 缺口：当前没有把元数据包、Ludusavi/Rclone依赖状态和所有路径冲突合并成独立“跨电脑向导”；但也没有具体迁移失败或用户流程证据证明需要第二套入口。
- 收益判断：若未来有明确跨设备任务，汇总只读清单可减少遗漏；在此之前，另造向导会与现有预览、环境检查和路径迁移重复。
- 决策：不新建迁移引擎或写入路径；候选保持“具体跨设备需求出现后再评估”。

## 现有风险覆盖入口

- A14：`RestoreReadinessTests`、`R12RestoreWorkflowBehaviorTests`、`HealthInspectionServiceTests`。
- A15：`R17DiagnosticsPackagePreviewTests`（Worker/WPF/source）、`DiagnosticsPackageServiceTests`、`MaintenanceReportServiceTests`、`MaintenanceReportSourceTests`。
- A16：`MetadataBackupServiceTests`、`MetadataBackupSourceTests`、`PathRemapServiceTests`、`R12PathRemapBehaviorTests`。

当前完整门禁身份、SDK、TRX、安装身份和CI job仍引用[P1本地收口](../P1-close-b667e224-20261113/README.md)；本批纯文档不生成新的产品验证身份。
