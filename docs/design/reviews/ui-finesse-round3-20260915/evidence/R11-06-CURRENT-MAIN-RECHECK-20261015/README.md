# R11-06 备份前变更摘要：当前 main 复核

日期：2026-10-15

测试提交：`40af39c80d904bd6306a4d5433e1356e367925ca`
范围：SaveCenter 只读备份预览、识别路径摘要、未扫描/空扫描状态、预览与立即备份命令区分

## 本次复核

现有生产能力已经满足该任务，本次只增强行为证据，没有改生产 XAML、ViewModel、DTO、Worker 服务或 Ludusavi 参数。

- 在实际 `SaveCenterView` 和 STA Window 中，Ready 摘要卡显示两个合成路径和 `384 B` 摘要。
- 从实际预览按钮创建 WPF Automation Peer 并调用 Invoke；预览命令执行一次，立即备份命令执行次数仍为零。该夹具验证生产视图的按钮到命令绑定，不代表系统级 UI Automation 或 Worker IPC。
- 未生成过预览时摘要卡保持收起；Loading 状态显示“扫描中”和“正在扫描本次备份范围”，不把未完成扫描显示成 `0 个路径`；NoData 状态显示已扫描但没有可纳入路径。
- Worker `BackupPreviewBehaviorTests` 用合成 Ludusavi JSON 验证两条路径/384 B 的解析和错误信号与空扫描的区别。
- 代码复核确认：`BackupOrchestrator.PreviewAsync` 调用 Ludusavi preview；`LudusaviClient` 在 preview 模式跳过备份目录创建。立即备份仍经 `BackupSelectedAsync` 发送 `BackupGame`；Worker 真实执行会重新读取当前游戏/匹配并用非 preview 参数扫描。现有测试没有启动真实 Ludusavi，也没有端到端执行 IPC 或文件系统副作用检查。

## 验证结果

本机使用隔离安装 SDK `8.0.423`（`D:\GameSaveCenterTools\dotnet-sdk-8.0.423\dotnet.exe`）、VSTest `17.11.1`。Playnite 目标 `net462`、测试目标 `net472`，测试宿主 .NET Framework `4.8.9181.0`；Worker 测试目标 `net8.0-windows10.0.19041.0`、测试运行时 .NET `8.0.29`。

| 验证 | 结果 |
| --- | ---: |
| Playnite `R11BackupPreviewBehaviorTests` | 4/4 |
| Worker `BackupPreviewBehaviorTests` | 2/2 |
| `scripts/check-xaml.ps1` | 24/24 |
| `python scripts/validate-source.py` | 通过 |
| `git diff --check` | 通过 |

两个定向 `dotnet test` 都在隔离 `.tmp/r11-06-40af39c8-20261015` 下从 Release 项目及引用重建。不是完整 solution build，也不是 Playnite 安装/宿主验收。本机 SDK8 结果不能替代 CI SDK `10.0.401`；代码提交 `40af39c8` 推送后的 Actions 状态须单独追到终态。

## 程序集与文件身份

以下 SHA-256 均以本次隔离输出或归档 TRX 实际计算。

| 文件 | SHA-256 | MVID / 说明 |
| --- | --- | --- |
| `R11BackupPreviewBehaviorTests.cs` | `79D6C4DEA2B1EDD0B18986108E89B9C5B67E022CA81D46662992C129603AD560` | 测试源 |
| Playnite.Tests.dll | `9E1D90099AA0EADC3DEA41B0F1E23DE3DA74A2E537DBFDC87B3F3FB99445EC08` | `2a40c019-4ca5-4eeb-a0c2-d7ec258109c2` |
| Playnite.dll | `6EC5B577F53C62100C2CDE6E5990434FAA97BBD290153E969780270E12FCA409` | `ffe10655-5608-4de6-8428-74195d34e780` |
| Worker.Tests.dll | `5451FA4B37FE3A3E0984ECA4774F4415B5DBF582060A96E657C0E50768BADC3D` | `d6c78833-6438-4d39-b890-e18a125252c5` |
| Worker.dll | `25BFF0E0A4C749DC72F4F278D2B4FD74E1634136325FE3F723950C32317B5F22` | `1165282f-2079-425c-8e4a-00bb7a37fe5d` |
| Playnite TRX | `C77559804E65810E807698D946278A74BDCDB5007A9771E6EF39C66129E7375F` | [4/4](tests/R11BackupPreviewBehaviorTests-sdk8423-40af39c8.trx) |
| Worker TRX | `682CB2058F797F291D5BFA5FE426DFCA847F9B6864F8FF3079FDE0EAEEA5AD3C` | [2/2](tests/BackupPreviewBehaviorTests-sdk8423-40af39c8.trx) |

All four assemblies report `0.6.73+40af39c8`.

## 边界与后续

本轮仅使用合成 DTO/JSON 和隔离 WPF 测试，不访问用户存档、备份媒体、Playnite 配置或云端。真实 Ludusavi 版本输出、Worker IPC、归档目录前后变化、真实 Playnite/package-host、系统 UIA/读屏/IME、最终呈现帧、物理 DPI/跨屏、ETW 与宿主性能仍未验证。Demo 原目录不可用，沿用恢复生产基线。

当前 R00/R01 freshness 扫描为 `13 fresh / 1 stale`：唯一 stale 是 R00-08，因 `R10KeyboardShortcutHelpBehaviorTests.cs` 路径命中其键盘证据范围。下一步按用户顺序复跑 R00-08 相关行为类并只在证据支持时更新该 baseline；本轮不据此修改其它 R00/R01 记录。R11-06 新提交的 Actions 尚待推送后跟踪。
