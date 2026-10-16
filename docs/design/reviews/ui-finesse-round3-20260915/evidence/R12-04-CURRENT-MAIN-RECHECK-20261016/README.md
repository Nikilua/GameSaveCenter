# R12-04 当前 main 复核

日期：2026-10-16。基线 `main` HEAD：`cdcd336a`；生产源码与 `b1e7b652c938463f2f50ac74cb8157c4899daa1d` 相同。本次在该基线上只补 Worker 编排测试。#1032/run `38838015414` 对 b1e7b652 的既有源码与测试已成功，SDK `8.0.423` 与 `10.0.401` 两个 job 均通过完整 Release build/test；它不含本次新增锁冲突负例，后者仍需随本次提交检查新 Actions。

## 本机验证

- 本机 SDK `8.0.423`、VSTest `17.11.1`，Playnite `net462` / 测试 `net472`，Worker `net8.0-windows`。隔离 Release 输出位于 `.tmp/R12-04-current-main-sdk8`。
- Worker `RestoreOrchestratorTests` 与 `GameOperationLockTests`：`26/26`。新用例持有同游戏 Backup 锁再请求 Restore，确认返回 `GAME_OPERATION_BUSY`、`BackupAsync` 调用数为 0、没有 Restore/Edit 调用且合成当前存档仍为 `A`。保护快照失败用例在同一 `GameOperationLock` 上重试，切换 fake 当前状态到 `A-latest` 后，确认该最新值进入新锁定的 PreRestore 快照，之后才恢复目标 `B`。
- Playnite `R12RestoreWorkflowBehaviorTests`：`8/8`。含保护备份失败的失败态/未完成状态投影，以及在 STA Window 中加载生产 `SaveCenterView`、检查绑定阶段、错误和活动状态。
- 两个项目及引用均成功构建。还原时 NuGet 漏洞数据源 `https://api.nuget.org/v3/index.json` 不可达，出现 `NU1900` 警告；还原、构建和目标测试均以 exit `0` 结束。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`（`24/24`）和 `git diff --check` 通过。

## 身份与结果

| 文件 | SHA-256 |
| --- | --- |
| `src/GameSaveCenter.Worker/Services/RestoreOrchestrator.cs` | `7C58FD0EF18C19E034ADAF992E2CE706654F876E31111D6D326AF8F9764CF76D` |
| `src/GameSaveCenter.Playnite/ViewModels/RestoreWorkflowProgress.cs` | `C520BD5A7F3909F43351142C67D44B6D3E5AAA3BE54270FC32001F5BC021AE4F` |
| `tests/GameSaveCenter.Worker.Tests/RestoreOrchestratorTests.cs`（新增锁竞争负例后） | `2100A6D46B55101762D411726F64C8310CB957CBDB1CFB007BEAE8855323B72B` |
| `tests/GameSaveCenter.Playnite.Tests/R12RestoreWorkflowBehaviorTests.cs` | `6C3FDAEB59156FB2713050DDC98E381C1597D725C3EE2F78C389942B4C3B2A3C` |
| `GameSaveCenter.Worker.Tests.dll` | `82A5551B30FF9B4775FC0E1F95A380DDCCCDA3D52BBFC621E8C14ED4FB9137B3` |
| `GameSaveCenter.Playnite.Tests.dll` | `10316A6EBA6E5B97E4EA53175799427CE821BA52CD8BF5D437AD434DE27001A0` |

脱敏结果文件：

- [Worker restore 与锁行为 TRX](tests/R12-04-restore-lock-sdk8.trx)，SHA-256 `520D689AB7B71E376FF6DFC4E79A699162F6C64B03D180AAB34C3D022F08C53E`。
- [Playnite 恢复阶段行为 TRX](tests/R12-04-restore-workflow-sdk8.trx)，SHA-256 `E9F16FFFA6CB4C29AB80BED89489D50966F6CC25594EF9EBD0F58084BBC3A09C`。

## 边界

仅使用 fake restore client、合成状态、临时目录/SQLite 与 STA WPF。没有访问或写入真实存档、Playnite 用户数据、媒体或云端。窗口用例与逻辑 DIP 不证明真实 Playnite/package-host 呈现、屏幕像素、物理 DPI/跨屏、外部 UIA/Narrator/IME、ETW 或宿主性能；本阶段没有重跑 RenderHarness。Demo 原目录不可用，沿用恢复生产基线。

下一项按账本顺序检查 R12-05 远端隔离下载进度的当前 main 证据；先复用现有 staging 服务和状态 DTO。
