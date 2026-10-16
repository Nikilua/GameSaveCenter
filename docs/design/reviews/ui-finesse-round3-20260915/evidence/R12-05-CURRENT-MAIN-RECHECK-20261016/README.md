# R12-05 当前 main 复核

日期：2026-10-16。源码基线：`86ba18a58c72d4599bbb9748b063f72ce108857c`（main）；构建源由下方源码 SHA-256 唯一标识。远端暂存原实现：`23e5b9d4`；后续 `0d1ff346` 给同一暂存任务附加稳定版本来源引用。本轮确认取消投影丢弃了 Worker 清理结果，修正现有 ViewModel 投影并补足负例；没有改 XAML 或重建业务服务。

## 验证结果

- 本机 SDK `8.0.423` / MSBuild `17.11.48` / VSTest `17.11.1`；本轮初始完整 Release solution build `0 warning / 0 error`。修正投影后，`dotnet test` 在 Release 下重新构建 Worker、Playnite、Contracts/Core 引用和两测试项目并运行相关类。
- Worker `RemoteBackupStagingSafetyTests + TaskQueryPersistenceTests`：`32/32`；新增负例锁住合成隔离文件，清理失败时确认目录/文件仍在，解锁后只删除该隔离目录；另外覆盖危险 rclone 命令拒绝与来源版本持久化回读。
- Playnite `R12RemoteStageProgressBehaviorTests + R15TaskSourceNavigationTests`：`8/8`；新增真实生产 `MaintenanceView` 的隔离 STA 行为用例：下载时进度 `43%` 且取消按钮可见/可用；取消并留下残留时保留 Worker 原始失败说明、Automation HelpText 同步，进度与取消入口隐藏/禁用。另验证清理成功/失败消息在状态投影中不同、失败与其他任务不冒充恢复完成。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1` `24/24`、`git diff --check` 均通过；WPF 静态审查 `0 errors / 27 warnings / 177 info`。没有 XAML 或页面布局变更，因此不运行页面级 Render QA。
- 前一基线提交的 Windows Actions #1034/run `38846116072` 已终态成功：SDK `8.0.423` job `116796913269` 与 SDK `10.0.401` job `116796913109` 均成功。#1034 不包含本轮取消状态修正；新代码提交的 Actions 必须单独追踪到终态。

## 行为范围

修正后取消任务优先展示 `TaskStatusDto.Message` 中 Worker 的真实清理结果；消息缺失时才使用指向任务详情的保守回退文本。Worker 文件锁负例和实际生产维护页绑定测试分别验证残留目录不会被误报为已清理、用户能看到残留提示且不能继续取消操作。远端数据仍只进入隔离区并校验；校验成功仍表示等待恢复确认。当前任务来源引用保留精确 BackupId。没有连接真实 rclone/Ludusavi、远端服务或 Playnite，也没有访问真实存档、用户媒体或云端。

没有改动页面 XAML，因此本次不运行页面级 Render QA。生产 ViewModel 与生产 `MaintenanceView` 在合成 DTO、隔离 STA 中覆盖。真实 Playnite/package-host、物理 DPI/跨屏、外部 UIA/读屏/IME、屏幕最终呈现、ETW 与宿主性能仍未验证。Demo 原目录不可用，沿用恢复生产基线。

## 归档文件与身份

脱敏 TRX：

- [Worker 定向测试](tests/R12-05-worker-sdk8.trx)，SHA-256 `17E6CAF49DF31452C3C721C0F01333E3CD59ABA83F43983CFEB66C33F1430CBA`
- [Playnite 定向测试](tests/R12-05-playnite-sdk8.trx)，SHA-256 `7AE2D563C19981173E44E60E3120B8FE1B2347D84838E7F1E1574CCFA1BA1A46`

SHA-256（源码 / 测试程序集）：

| 文件 | SHA-256 |
| --- | --- |
| `RemoteBackupStagingService.cs` | `A083ADE6FFC104D1EA78CF429286C40ACA25EF1AE54C6B1C4154EEE9D3C90155` |
| `DashboardViewModel.RemoteStage.cs` | `F915A7B6BF9AFC952D97064550927DC7F2A77CF0236894CFEA6AE9482CEDF772` |
| `R12RemoteStageProgressBehaviorTests.cs` | `57336F0F1193620DDF22986B2ED0DECE92D2056B652BE5CE5693BE7E3149436C` |
| `R15TaskSourceNavigationTests.cs` | `26F17B98916AD669C299A80A544372E2D06034F10653D45A65D4AA2D29539BDA` |
| `RemoteBackupStagingSafetyTests.cs` | `4F62FDA958B1EC902157BD6259D0BA31E3BBD8448AD11C88691FF9FA631DA85A` |
| `TaskQueryPersistenceTests.cs` | `04A2FF0C7007898E0E57D1E8652656992B17FFDCA1F21D364286A642C271A6F8` |
| Worker DLL | `8D0ED36614A13953DA3F97A398F0CF3886157D25B77129CB4A1D7FC1F45EBACF` |
| Playnite DLL | `7026D14D4B272B05745E927CBF8DFEE34C3ED36AE7EB397EAB898EE50E7E7241` |
| Worker test DLL | `C0DC0810D27AC9E720FE389A16728DEC2828912D1204D48991C50A7E766BD964` |
| Playnite test DLL | `889A86603F6D467873D5BE396D27DB30BD62E70C267EBF4394D55B17A58EDEAA` |

TRX 内的本机用户名、计算机名和绝对仓库路径已脱敏。R12 复核最初只定向重建受影响项目；随后在 R13-01 当前 main 复核中，用 SDK `8.0.423` 对含本修正的完整 Release solution 再构建，`0 warning / 0 error`、XAML `24/24`。该后续构建还包含 R13 测试夹具的本地增补，不替代本批 Actions。提交 `241bfb16` 的 R00/R01 freshness 为 `14 fresh / 0 stale / 0 source-path matches`，package identity `not-provided`；结果见 [摘要 JSON](R00-R01-freshness-241bfb16.json)，SHA-256 `51C7AEC2C92C47B2A138F7FC94D9A4F6921120DF205309F6FB447C43C2468D92`。归档 R13 证据后已删除 `.tmp/r12-05-current-main-sdk8` 与 `.tmp/r12-05-current-main-sdk8-diag`，释放 `170.9 MiB`。
