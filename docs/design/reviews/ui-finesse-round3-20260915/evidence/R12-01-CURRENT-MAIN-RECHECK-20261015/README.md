# R12-01 恢复分步摘要：当前 main 复核

日期：2026-10-15
分支：`main`
测试源码身份：`3f6b355ff586904dcd0cf2bdfdd875346e1e01d9`

## 复核范围

四阶段恢复摘要已由 `e1a8da0c` 实现。本轮没有改生产 ViewModel、XAML、Worker、DTO、命令或恢复安全路径；只给现有回归补一项实际 `SaveCenterView` 绑定行为验证。继续复用 `BackupVersionDto.RestoreReadiness`、`TaskStatusDto`、`RestoreWorkflowProgress` 和原有恢复命令，没有重建服务或状态契约。

新增 WPF 行为用例在隔离 STA 窗口加载生产 `SaveCenterView`，使用合成选中版本及 `RestoreWorkflowProgress.Build` 输出。目标核对失败时，视图实际显示失败阶段与完整错误码/消息，执行行仍显示“待处理”和“尚未进入写入当前存档的阶段”；切换到活动状态后，集合与摘要绑定更新，视图分别显示可恢复性警告、目标核对/执行进行中以及 `PreRestore` 保护说明。测试夹具提供选中的合成备份，因为生产检查器在没有选中版本时会按设计收起。

原有模型行为继续覆盖未检查时执行保持 pending、readiness warning、目标失败停在写入前、执行失败保留回滚信息、PreRestore 失败中止危险恢复，以及成功任务完成四阶段。静态 XAML 检查确认原恢复命令、可恢复性检查命令和四阶段列表仍在。

## 构建与测试

本机 SDK `8.0.423`，VSTest `17.11.1`；Playnite 目标 `net462`，测试目标 `net472`，testhost 为 .NET Framework `4.8.9181.0`。在 `.tmp/r12-01-current-main-3f6b355f-20261015` 的新隔离输出中，对测试项目及引用执行 Release 构建，`0 warning / 0 error`；这不是完整 solution build。

- `R12RestoreWorkflowBehaviorTests`：`8/8`。
- `R12RestoreConflictExplanationBehaviorTests`：`4/4`。
- `R12RestoreRevalidationBehaviorTests`：`2/2`。
- 合计 `14 passed / 0 failed / 0 skipped`。3 个过滤类分别串行运行，TRX 中每个 case 都是 executed/pass，没有未执行项。
- `scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py` 与 `git diff --check` 通过。

## 文件身份

| 文件 | SHA-256 | MVID / 说明 |
| --- | --- | --- |
| `R12RestoreWorkflowBehaviorTests.cs` | `6C3FDAEB59156FB2713050DDC98E381C1597D725C3EE2F78C389942B4C3B2A3C` | 本轮增加生产视图运行时绑定检查 |
| `SaveCenterView.xaml` | `4A86836BB5FB4CDA36DD2E4085F43A345EB9F2ABDBFDD1C7C96CE5CA8DC718F9` | 被加载的生产视图，未改动 |
| `GameSaveCenter.Playnite.Tests.dll` | `ECF8E3169F8F9C4D666F1BF7D2BBDADFEA3111BCF2C24DFB2ED59071D8B0A461` | MVID `d8ad6a80-9374-475b-86f3-452e113fba67`；版本 `0.6.73+3f6b355f` |
| `GameSaveCenter.Playnite.Tests.pdb` | `AD652F45222A9D8186CE8D1854466AAFBE72657F91C925F870A8120FA70C24FC` | 测试符号 |
| `GameSaveCenter.Playnite.dll` | `CC04C582C45384019D9176B84745DCFEC70F94C9208876243D4C65FC5078AF48` | MVID `3cfa8438-e0d6-4858-8b7d-75c94c10c846` |

| TRX | 结果 | SHA-256 |
| --- | --- | --- |
| [R12RestoreWorkflowBehaviorTests](tests/R12RestoreWorkflowBehaviorTests-sdk8423-3f6b355f.trx) | `8/8` | `00286868A619A18296E976CBAC5425FC04521D8E25C41488F2B45461D1AAC788` |
| [R12RestoreConflictExplanationBehaviorTests](tests/R12RestoreConflictExplanationBehaviorTests-sdk8423-3f6b355f.trx) | `4/4` | `E5C2B182805D3163A0447B9C496E9FA2D87ADC7611AF10CAB2C87923554DB304` |
| [R12RestoreRevalidationBehaviorTests](tests/R12RestoreRevalidationBehaviorTests-sdk8423-3f6b355f.trx) | `2/2` | `D31EE419D141D735124CCBCC37464E20697F2399C6E10DBD2832913345106FD6` |

## 未验边界

视图探针使用合成 DTO 和逻辑 DIP 的 STA 窗口；没有启动真实 Playnite、Worker IPC 或真实恢复，也没有写入用户存档、媒体或云端。没有据此声称真实宿主呈现、物理 DPI/跨屏、OS UIA/读屏/IME、ETW 或宿主性能已验证。未产生或引用截图。Demo 原目录不可用；本轮没有视觉源码变更，继续沿用恢复生产基线。

本机 SDK8 结果不替代 GitHub Actions 的 SDK8/SDK10.0.401 job。`3f6b355f` 对应 CI 尚未取得终态；公开 Actions 页面数据仍停在 #979，匿名 API 返回 rate limit `403`。推送后的 run 仍需继续核查。

下一项：R12-02“校验结果解释”。先复用已有 `RestoreReadinessDto` 状态、文件计数、哈希覆盖和检查时间字段，再核对部分覆盖、旧结果和损坏结果的行为负例。
