# R12-02 恢复校验结果解释：当前 main 复核

日期：2026-10-15
分支：`main`
源码提交：`d6640887`（只增加 WPF 行为测试；生产实现仍复用 `6cc3a618`）

## 复核范围

先复用 `RestoreReadinessService`、`RestoreReadinessDto`、SQLite JSON 持久化、`BackupVersionDto` 显示映射和 `SaveCenterView` 现有详情卡。本轮没有增加 Worker 服务、DTO、IPC、恢复命令或生产视觉改动。

此前证据有 Worker 的合成 ZIP/Manifest 负例和 Core 的显示映射断言；详情卡对新状态主要依靠 XAML 静态绑定检查。本轮新增 `R12RestoreReadinessViewBehaviorTests`，在隔离 STA 窗口直接加载生产 `SaveCenterView`，通过替换选中合成版本核对实际 TextBlock 内容和时间 Tooltip/Automation HelpText：

- 无 readiness 时显示“未验证”“未提供哈希（不等于校验成功）”“哈希覆盖：未提供”“尚未检查”，不出现“可恢复”状态。
- 部分覆盖且结果超过一天时显示 `1/2`、未覆盖文件说明和“结果较旧”；完整时间同时出现在 Tooltip 与 Automation HelpText。
- 损坏结果独立显示“疑似损坏”、ZIP 错误摘要、失败哈希和实际文件/大小计数，不与部分覆盖或无检查混为一类。

测试数据为合成 DTO；Worker 测试用本阶段隔离临时目录中的合成 ZIP/Manifest 与隔离 SQLite，既有“校验不写真实存档路径”负例继续运行。

## 构建与测试

当前执行主机只安装了 .NET SDK `9.0.302`；仓库 `global.json` 要求 `8.0.423`，从仓库目录启动会被 SDK 选择器拒绝。本轮从仓库外调用 SDK9，并通过绝对 `GscBuildOutputRoot`、源码根和提交身份构建；没有修改 `global.json`。VSTest `17.14.1`；Playnite 插件 `net462`、测试 `net472`、testhost .NET Framework `4.8.9181.0`；Core/Worker tests 目标 .NET 8、运行时 `8.0.18`。

- Worker `RestoreReadinessTests`：`15/15`，包含无哈希、部分覆盖、错误哈希、缺失/损坏归档、同大小内容变化、取消与隔离清理、SQLite 重启持久化。
- Core `UiDisplayMappingTests`：`40/40`，包含无哈希、部分覆盖、过期时间、文件/大小零值与其他显示映射。
- Playnite `R12RestoreReadinessViewBehaviorTests`、`R12RestoreWorkflowBehaviorTests`、`R11VersionSummaryBehaviorTests`：`11/11`。其中生产详情卡和恢复分步视图均在隔离 STA 实际加载。
- 合计 `66 passed / 0 failed / 0 skipped`；三个 TRX 的每项均执行并通过。
- 每个定向 `dotnet test -c Release` 同时构建相应测试项目及引用项目，输出未报告编译 warning/error；这不是完整 solution build。`scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py` 与 `git diff --check` 通过。

本机 SDK9 只作为本机复核；它不替代仓库 SDK8.0.423 或 CI SDK10.0.401。推送后的 Actions 必须另行跟踪至终态。

## 文件身份

| 文件 | SHA-256 | 说明 |
| --- | --- | --- |
| `R12RestoreReadinessViewBehaviorTests.cs` | `FBFDFBA4A5E9C5964C6D43084F472A53D2CF50EA59316371A6DD51D9D6535A8C` | 当前新增的实际视图行为检查 |
| `SaveCenterView.xaml` | `4A86836BB5FB4CDA36DD2E4085F43A345EB9F2ABDBFDD1C7C96CE5CA8DC718F9` | 被测试的生产视图，未修改 |
| `GameSaveCenter.Playnite.Tests.dll` | `52F16D696B3DECBAA633EEE80755295CB039A42975B474F71F1861E1DCF14C41` | MVID `9689701d-9c7d-48af-b2b7-0c81338f65a3`，隔离 Release 输出 |

| TRX | 结果 | SHA-256 |
| --- | --- | --- |
| [RestoreReadinessTests](tests/RestoreReadinessTests-sdk9302-d6640887.trx) | `15/15` | `8D1C786CE6C2FB7A962C04C4B3CDCC746E93F98BE9ABC56A060818B350888B1A` |
| [UiDisplayMappingTests](tests/UiDisplayMappingTests-sdk9302-d6640887.trx) | `40/40` | `E69272919E296B405901EB6B5FC7C1C0C25D822AF6A94E72A4BBB9F9921F9F29` |
| [R12 readiness view / workflow and R11 summary](tests/R12-readiness-view-and-workflow-sdk9302-d6640887.trx) | `11/11` | `E7454C3341027F04F9A407685DC9C99A38AE40B859F55A97FEE167A508678876` |

## 未验边界

没有启动真实 Playnite、Worker IPC、真实用户归档或恢复写入；没有改动用户存档、媒体或云端。隔离 STA 逻辑布局不代表真实宿主最终呈现、物理 DPI/跨屏、OS UIA/读屏/IME、ETW 或宿主性能。Demo 原目录不可用，本轮无生产视觉改动，继续沿用恢复生产基线。

恢复时指定的 #920/run `36677346317`（commit `77de7f4`）现为 Failure，7m04s；#919/run `36675309812` 也为 Failure，13m02s。两者完整的 SDK `10.0.401` job logs 已从仓库既有诊断归档中读取，压缩包及 SHA-256 见[R07-08 CI 失败复核记录](../CI-R07-08-RESIZE-FAILURE-20261013/README.md)。两个 run 都在 `ProductionShellChromeSourceTests` 的动画正例失败：旧测试只固定应用动效开关，没有固定 Windows 系统动效策略；当 runner 的系统策略关闭动画时，生产代码正确落到无活动时钟的 72 DIP 状态。测试夹具后来已增加实例级系统策略输入和显式前提断言（`48b78b3b`），生产动效策略未改。失败后的 `TextServicesHost.InvalidComObjectException` 是清理阶段输出，不是前述断言原因。该根因针对 #919/#920；不代表本机 SDK9 或后续 SDK10 CI 已通过。Actions 列表仍只显示到旧 #979 并报加载错误；`22e4b286`、`d6640887` 对应的新 Actions 终态未确认。CI SDK8/SDK10.0.401 必须分开追踪；不能凭本机 SDK9 结果代替。

下一项：R12-03“目标路径核对”；先复用既有路径映射、目标解析和预览能力，优先验证长路径完整复制、相似路径/不同盘符辨别及预览不写入用户目录。
