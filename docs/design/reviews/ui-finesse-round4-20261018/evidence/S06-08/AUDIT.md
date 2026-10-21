# S06-08 跨会话回归等价能力审计

日期：2026-10-21
审计基线：`96a3eab3`

## 计划范围

S06-08 要求覆盖设置保存冲突、损坏配置恢复、重复加入、旧数据升级和退出时保存；损坏清单不能阻止启动，并要求保存或恢复边界可说明、源业务数据不被清单整理动作修改。

## 已有能力与证据

- **保存冲突：** `GameSaveCenterSettings.EndEdit` 在写入前调用 `DetectAndReportSettingsConflict`；相同字段冲突会保留草稿并抛出 `SettingsConflictException`。`R16SettingsConflictBehaviorTests` 验证非冲突合并、冲突保留和拒绝静默保存；`R16SettingsConflictSourceTests` 验证设置页接线。
- **损坏配置：** `PluginSettingsLoadRecovery` 在恢复前把原始配置复制为 `.recovery-*.bak`；解析失败时以默认设置继续，备份失败则禁止自动回写。`PluginSettingsLoadRecoveryTests` 已验证损坏视图字段保留其他设置及原文备份，也验证畸形 JSON 的默认恢复和备份。
- **重复引用：** `WorklistDefinition.NormalizeMany` 按对象类型与稳定 ID 去重，`TryAdd` 返回 `AlreadyPresent`；`DashboardViewModel.Worklists` 在确认前后重新检查重复项，且清单操作只改人工引用。`WorklistDefinitionTests` 已验证模型去重和重复加入结果。
- **升级兼容：** 插件设置通过新增字段默认值与各定义的 `Normalize`/`NormalizeMany` 归一化旧数据；便携设置 v1 有默认值升级测试。原生 Playnite 设置没有单独的清单 SchemaVersion。
- **保存时点：** 工作清单增加、手动状态、排序、移除与活动清单选择均直接调用 Playnite `SavePluginSettings`；保存异常时恢复先前设置。退出回调只停止插件自有 Worker，没有延后到退出阶段才写清单的队列。

## 差距与准入

现有验证分散在模型、设置恢复和冲突测试中，没有一项行为测试串起原生 `GameSaveCenterSettings.Worklists` JSON 序列化、跨会话读取、旧字段默认值、重载后的重复拦截；也没有专门输入“清单字段形状损坏”的恢复用例。补充小型序列化/恢复行为测试后即可覆盖可控边界，依赖 S06-01 至 S06-07 均已满足，准入 READY。

Playnite 的真实关闭生命周期及 `SavePluginSettings` 对磁盘写入的原子性属于宿主实现；本机未安装/启动真实 Playnite。受控验证不模拟宿主行为；恢复备份测试只证明本插件在读取损坏配置时保留原始配置，不宣称能覆盖任意掉电或半写故障。
