# S06-08 跨会话清单回归

## 覆盖范围

提交 `09ec3c84` 增加原生 Playnite 插件设置的清单序列化行为回归：

- 写入并重载 `GameSaveCenterSettings` 后，清单/条目稳定身份、人工备注、手动状态、活动清单和无关设置仍保持不变；再次加入大小写/空白不同的同类稳定引用会被拒绝，数量仍为一项。
- 旧版 JSON 缺少 `Note` 和 `IsManuallyComplete` 字段时，读取使用默认值，规范化后可再次序列化并读取。
- `Worklists` 字段形状损坏时，恢复入口返回可用默认设置并生成 `.recovery-*.bak`；测试逐字节检查原配置和恢复备份，确认恢复读取没有改写源文件。

现有回归继续覆盖设置冲突三方合并/拒绝静默覆盖、其它损坏配置的恢复备份、清单模型去重/大小上限、便携设置旧字段默认值和保存失败反馈。本项没有修改存档、媒体、任务或校验源对象。

## 验证

- SDK `9.0.302`，提交 `09ec3c84` 的 `GameSaveCenter.Playnite.Tests` Release Rebuild：`0 warning / 0 error`。
- 受控跨会话/设置回归 `27/27`，包含新增 3 项行为测试和现有冲突、恢复、去重、兼容性用例。
- `scripts/validate-source.py .` 与 Round4 进度账本 `5/5` 通过；该项未改 XAML。
- 测试程序集带有 `GSC_BUILD_COMMIT=09ec3c84` 和当前源码根身份，源码测试的 checkout 一致性保护已通过。

## 原始证据与 SHA-256

| 文件 | SHA-256 |
| --- | --- |
| [Playnite.Tests Release Rebuild](build/S06-08-playnite-tests-rebuild-09ec3c84.txt) | `3002AF5FC501017D535994FF05449B16A830A7C501C2CCA2DB94828477A5A72C` |
| [跨会话回归 TRX](tests/S06-08-cross-session-regression-09ec3c84.trx) | `21C268BFFA1F69165BEA0205C19684687628137FD2837DD8060A8FE27707FBE7` |

定向测试筛选：`WorklistCrossSessionPersistenceBehaviorTests`、`PluginSettingsLoadRecoveryTests`、`WorklistDefinitionTests`、`R16SettingsConflictBehaviorTests`、`R16SettingsConflictSourceTests`、`PortableSettingsTests`、`SettingsSaveFeedbackTests`。

## 边界

损坏配置测试直接运行插件的恢复加载器，不启动真实 Playnite；它证明本插件能返回默认设置并保留原始配置副本，不证明真实宿主启动过程。工作清单变更路径即时调用 Playnite `SavePluginSettings` 并在异常时回滚，但真实应用退出生命周期、宿主 API 的磁盘原子性和掉电/半写恢复未验证。S06-08 不增加额外退出 flush，也不模拟宿主承诺。
