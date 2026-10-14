# R11-05 保护操作解释当前 main 复核

更新：2026-10-15；提交 `884e826e`（只新增行为测试，无生产代码变更）。当前任务复用已有 DTO、保护说明、保留规划和安全删除流程，并补齐生产单元格模板绑定证据。

## 当前能力

- `BackupVersionDto` 按锁定、PreRestore、有效健康恢复点和普通版本计算保护状态及说明；`Ready` 但没有可用内容不会被误算为健康保护。
- SaveCenter 生产“状态”列使用 glyph、状态文字和保护解释 Tooltip；锁定/解锁保存后由现有保留预览重新评估。
- Core 保留规划始终跳过锁定、PreRestore 与健康恢复点。Worker 预览和应用都只处理未受保护候选，应用前继续校验预览时效和索引状态。

## 验证

本机 SDK `8.0.423`、VSTest `17.11.1`，Release 定向构建并串行运行：

| 范围 | 结果 | 关键行为 |
| --- | ---: | --- |
| Playnite `net462` / Tests `net472`：`R11ProtectionBehaviorTests` | 3/3 | 锁定、PreRestore、健康恢复点与未保护状态；无内容的 Ready 负例；从真实 `SaveCenterView` 取生产 `DataTemplate`，加载绑定并核对 glyph、状态文字、Tooltip 正反例 |
| Core：`RetentionPlannerTests` | 3/3 | 锁定/PreRestore/健康恢复点不成为候选；解锁后该旧版本重新成为候选，其他保护继续排除 |
| Worker：`RetentionSimulationServiceTests` | 12/12 | 合成 SQLite/归档预览与应用只包含未保护文件；取消、过期预览、索引/归档/策略变化、重复提交和二次确认边界 |

上述测试项目分别成功编译其相关项目引用；本阶段没有运行完整 solution build。`python scripts/validate-source.py` 和 `git diff --check` 通过。R11-05 总计 18/18，不把辅助字符串检查计为行为通过。

## 程序集与源身份

测试程序集均为 `0.6.73+884e826e`。SHA-256 / MVID：

| 程序集 | SHA-256 | MVID |
| --- | --- | --- |
| Playnite.Tests | `922B3F64028BC2B2086B87190DC6D105E887A143AC1A22DC9590E8077D335511` | `7aa94665-4e61-4124-a6ca-2e88cfbea5e2` |
| Playnite | `93F1DB4077141F7FBCC079BC4AA4335713F229D36D9E3F643153950704C481F5` | `dafdeff7-e58d-4237-8830-80d976456775` |
| Worker.Tests | `5AEEFEEEEA77CA1A3A36F6DD12A6089067CBDE7AB5D60407C7C1EDB842CE27EA` | `7f9d6797-0247-4374-b4df-73b278dd51c9` |
| Worker | `36708E4A647FB6865A971C85041AEEBF5FCBCF11BB088831666EEB74685D4B26` | `d3bd21b6-0e1b-4091-b080-0cb81f9a65c0` |
| Core.Tests | `B794F80BE422078415B307B2FC572732168CB5E377C3366685BFC26F5D35EFB3` | `85d114d6-8dba-4528-88ab-ab47153baf90` |

Source SHA-256: `R11ProtectionBehaviorTests.cs` `47C2D02308F046AB2B6B6E5E4B78FFCFF0643AB839CA0CF1B1FEBE00E6FD0F29`; `RetentionPlannerTests.cs` `CBD4974BFD297E45C1EB59F04C5409F9B65867E2F13F34558499B17F054B1571`; `RetentionSimulationServiceTests.cs` `47792CC43BD5FE2EDEFC0318C2EEBF6319996C797FB163DAAFF9CC854DDDC728`。

## 脱敏 TRX

- [Playnite 3/3](tests/R11ProtectionBehaviorTests-sdk8423-main-884e826e.trx)，SHA-256 `7C747D0264263BE1C7E8D8D7ACADF8EED7644511709FC3AD6FE550F49621036A`
- [Core 3/3](tests/RetentionPlannerTests-sdk8423-main-884e826e.trx)，SHA-256 `8B90DEC5AECAB007790967936BDB18540862232A231E6FFB3E4999BD7E5BDEE2`
- [Worker 12/12](tests/RetentionSimulationServiceTests-sdk8423-main-884e826e.trx)，SHA-256 `8DF46B5C0B01A7197419C2DAC51C39888FC5914C0787EB7FDCD1182994F6A39B`

## 范围与未验边界

Worker 只使用测试创建的 SQLite 数据库、合成版本和隔离归档目录；没有读取、修改或删除用户存档/媒体，没有写云端。STA WPF 模板行为与逻辑 DIP 不等于最终像素；未运行真实 Playnite/package host、Ludusavi IPC、OS UIA/读屏、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能。没有改视觉结构；Demo 原目录在此前检查中不可用，本轮无须以 Demo 重设计。

下一项：R11-06 备份前变更摘要。推送后须分别跟踪 SDK8 与 CI SDK `10.0.401` 的 Actions 到明确终态；本机 SDK8 不替代 CI。
