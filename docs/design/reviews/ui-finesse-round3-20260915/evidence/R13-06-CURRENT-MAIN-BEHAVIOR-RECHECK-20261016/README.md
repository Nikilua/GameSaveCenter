# R13-06 当前 main 离线恢复反馈行为复核

复核日期：2026-10-16。构建开始时的 `main` HEAD 为 `2341ca59e8bca692c9e778c2e31e62ba59016768`。本阶段只扩展 `R13CloudTransferStageBehaviorTests` 的合成 WPF 行为覆盖；没有改生产 DTO、维护页、Worker、通知策略、命令或视觉资源。测试源码 SHA 与本次产物身份见 [hash 清单](hashes-current-main-sdk9302.txt)。

## 当前行为与实际覆盖

- 复用 `CloudTransferStatusDto.NetworkRecoveryDisplay` 和生产 `MaintenanceView` 已有绑定。新的 STA 行为步骤选择网络失败的 `RetryScheduled` 合成项，读取已显示的 `TextBlock.Text`，核对退避说明、`2/6` 次重试和每轮最多 10 项。
- 在同一个生产视图中切换到网络恢复后的 `Transferring` 项，确认所选 DTO 与绑定文本一起切换为“网络已恢复；按批次上传中”。再切到 `RCLONE_AUTH_FAILED` 项，确认恢复文本为空；重新选择网络重试项后恢复等待文案。该验证运行了真实 WPF `Window.Show`、DataGrid 选中与 Dispatcher 布局，数据均为合成 DTO。
- 当前 `CloudRetryService` 仍以 30 秒轮询、每批最多 10 项、逐项 `await` 的顺序执行；没有引入并发扇出。通知监视器启动时对旧终态快照调用 deduper 记账，后续变更再交给终态通知路径；`TaskNotificationDeduperTests` 验证同一失败证据只认领一次。当前没有启动真实 Playnite 通知监视器，未声称真实宿主通知体验已验。

## 本机验证

仓库锁定 SDK 是 `8.0.423`，此环境只安装 SDK `9.0.302`。以下均以 SDK `9.0.302` / VSTest `17.14.1`，独立 `GscBuildOutputRoot` 和 `GSC_SOURCE_ROOT`/`GSC_BUILD_COMMIT` 运行；不是完整 solution build，也不替代 SDK8 或 SDK10.0.401 CI。

| 测试 | 结果 | 覆盖 |
| --- | ---: | --- |
| `R13CloudTransferStageBehaviorTests` | `14/14` | 生产维护页阶段/详情绑定，含新增长的等待→恢复切换和认证失败空文案负例 |
| `CloudRetryPersistenceTests` | `12/12` | 当前 Worker 退避、错误分类、SQLite 持久化、到期/延后查询和重启恢复 |
| `CloudRecoveryExplainsBackoffAndOnlineBatchTransition` | `1/1` | Core DTO 正向与认证失败负例 |
| `TaskNotificationDeduperTests` | `6/6` | 进度不认领通知、同一失败去重、不同错误证据仍可见 |
| `R09FocusOutlineBehaviorTests` | `2/2` | 当前 main 的 R09 模板焦点/验证描边断言；与外机失败对照，详见 [R09 复核](../CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md) |

本阶段 Core、Worker、Playnite 相关测试项目与引用均在独立输出中完成 Release 编译及选定测试。`python scripts/validate-source.py` 与 `git diff --check` 通过。没有运行 RenderHarness、完整 Playnite 扫描或真实 Playnite；也没有连真实网络/rclone/云端、写用户存档或发送通知。

对本轮唯一代码变化 `tests/GameSaveCenter.Playnite.Tests/R13CloudTransferStageBehaviorTests.cs` 运行 `scripts/check-ui-evidence-freshness.ps1` 后，R00/R01 为 `14/14 FRESH / 0 STALE / 0 matched source paths`；`packageIdentityStatus=not-provided`。JSON 是源码路径失效检查，不表示测试重跑、安装包或宿主验收，文件 SHA 见清单。

## 另一台机器上的 R09 失败

新提供的日志位于诊断 run `20261016T052146719Z-d9d99ac4`，构建目录显示为 `artifacts/gsc-b/fd7672ef`。它报告旧值形态 `Expected 2,2,2,2 / Actual 1,1,1,1`，映射到 line 151。当前 main（包括祖先 `983fee5`）的 line 151 是错误色断言，line 152 才断言 Chrome 为 `1 DIP`，line 154 单独断言 `ValidationStroke` 为 `2 DIP`。本机从当前 checkout 新建隔离目录编译后，R09 类 `2/2` 通过。基于断言类型、源码行号和值，外机日志中的测试源/符号与当前源码不一致；它还不足以分辨外机实际 checkout、构建脚本版本或测试 DLL/PDB。

`libpng iCCP` 是图像 profile 警告，不在失败调用栈；`TextServicesContext` 的 COM 收尾异常发生在 xUnit 断言失败之后，不能解释该失败。没有改生产模板或放宽断言。

若要确定外机实际输入，请从那台机器的仓库目录执行并提供输出：

```powershell
git rev-parse HEAD
git status --short --branch
Get-FileHash .\tests\GameSaveCenter.Playnite.Tests\R09FocusOutlineBehaviorTests.cs -Algorithm SHA256
Get-FileHash .\artifacts\gsc-b\fd7672ef\bin\GameSaveCenter.Playnite.Tests\Release\net472\GameSaveCenter.Playnite.Tests.dll, .\artifacts\gsc-b\fd7672ef\bin\GameSaveCenter.Playnite.Tests\Release\net472\GameSaveCenter.Playnite.Tests.pdb, .\artifacts\gsc-b\fd7672ef\bin\GameSaveCenter.Playnite\Release\net462\GameSaveCenter.Playnite.dll -Algorithm SHA256
Get-Content .\artifacts\build-diagnostics\20261016T052146719Z-d9d99ac4\summary.json -Raw
```

如果文件仍在，完整的 `20261016T052146719Z-d9d99ac4` 诊断目录最有用。缺少这些外机身份前，根因保持未确认；本机通过只证明本机这份明确源码/程序集的结果。

## 边界与下一步

本批证明的是合成数据下的 WPF 绑定行为、Worker 现有持久化用例和代码中的有限顺序批次。真实联网恢复时序、网络抖动、真实通知、rclone 远端、Playnite/package-host、物理 DPI/跨屏、独立 OS UIA/IME、最终呈现帧、ETW 和宿主性能仍未验。Demo 原目录不可用；未更换设计体系，WPF 技能只作质量检查。

下一项为 `R13-07 队列筛选与汇总`：先核对现有筛选、全局计数、分页、过期 token 和选中项联动，再补真实维护页行为与负例；仅用合成数据和隔离状态。
