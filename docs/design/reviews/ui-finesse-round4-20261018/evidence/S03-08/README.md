# S03-08 来源与恢复方案受控验收

## 范围

验收测试提交为 `37f42cc2e9920574d43d13cf8018ad6dcf114158`。本项只补验收用例，没有修改产品实现。

- Core 合成来源图验证父版本只按明确 `ParentBackupId` 建边、内容等价只按匹配指纹建立；父版本创建时间晚于子版本时，两种证据仍保持独立。
- Playnite 来源文字列表验证 Desktop、Laptop、Handheld 合成版本的分叉、来源父版本、同指纹关系和时钟偏差；关联项仍按创建时间排列，但时间不改变关系标签。
- 41 天前的 readiness 检查只显示检查年龄及“结果仅反映该次检查”，不额外宣布检查过期；冲突说明继续明确不按较新时间推荐覆盖。
- 展示器处理期间检查临时目录里的合成 `synthetic-save.bin` 字节与目录文件列表，均未变化。没有访问真实存档或调用恢复。
- 同批运行 `RestorePlanDraftRevalidationTests` 与 `R12RestoreRevalidationBehaviorTests`：就绪结果变化会要求重验；确认的游戏/备份身份不匹配时拒绝恢复，实际 RestoreCommand 仍在提交请求前调用原 `RestoreConfirmationGuard`。
- 缺失父版本在局部历史与完整索引中的不同说明，以及 R09 焦点环行为由现有用例覆盖。

## 构建与测试

仓库 `global.json` 要求 SDK `8.0.423`，当前机器只安装了 `9.0.302`。为继续验证，从仓库目录外启动 SDK9，并对 MSBuild 限制单节点；结果不冒充 SDK8 基线。

- 精确提交 Release solution build：成功，`0 warnings / 0 errors`。
- `BackupLineageBuilderTests`：`10/10` 通过，TRX：[S03-08-core-37f42cc2.trx](tests/S03-08-core-37f42cc2.trx)。
- `BackupLineageDisplayBehaviorTests`、`DeviceConflictComparisonDisplayBehaviorTests`、`RestorePlanDraftRevalidationTests`、`R12RestoreRevalidationBehaviorTests`、`R09FocusOutlineBehaviorTests`：`15/15` 通过，TRX：[S03-08-playnite-37f42cc2.trx](tests/S03-08-playnite-37f42cc2.trx)。其中 R09 `2/2` 通过。
- `python scripts/validate-source.py .` 与 `git diff --check` 通过。
- Playnite 测试在 .NET Framework `4.8.9181.0` 下运行。未运行完整 WPF 隔离循环或真实 Playnite。

## 输入身份

构建提交元数据为 `37f42cc2`，源码 HEAD 与测试程序集身份校验通过。SHA-256：

| 文件 | SHA-256 |
| --- | --- |
| `tests/GameSaveCenter.Core.Tests/BackupLineageBuilderTests.cs` | `169172D49D78D856DC9FFF9F7BAD49D836160F5A0B480FCF3E8B6D1CE4EDB6FF` |
| `tests/GameSaveCenter.Playnite.Tests/BackupLineageDisplayBehaviorTests.cs` | `2C38152BAB26458B23F8261AFC2B9D88324815CDFA0C55FBD942D8B96FB65807` |
| Core test DLL | `9C25355E85E8B4B2F364826D7F566967E5254A1BCC2179F4BBA7FB254A344055` |
| Playnite test DLL | `78BAEB7EDF4C0DD8935FCAFA824BE4A499CF3E082DC69A99150909D5E378C53D` |
| Playnite plugin DLL | `D7E8B34BB60EBB2B8B6709B439B2A6F492D11F76C32A3C23ED0A9843789089A2` |
| Core TRX | `238CE816F04D996F8F75EBC385B64ED304557347F8573AF57F09115B2648D0C9` |
| Playnite TRX | `D933B978BA5E9255576D0C3D80EA01BF393896A8DACCA57AE6932DEF6E9F92CB` |

用户先前 R09 失败的诊断目录、TRX 与 DLL/PDB 身份仍不可用。因此当前精确提交的 R09 `2/2` 仅说明当前输入通过，不能确定旧运行使用的源码或二进制。
