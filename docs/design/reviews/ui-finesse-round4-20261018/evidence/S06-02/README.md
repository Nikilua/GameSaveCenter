# S06-02 加入待办入口

日期：2026-10-21

实现提交：`85400645`（已推送至 `main`）

## 实现范围

在存档版本、媒体项、后台任务和维护问题详情中加入“加入待办”入口。入口先以现有 `plugin.ConfirmAsync` 展示对象摘要和目标清单；取消不写入。确认后只持久化对象种类和稳定编号，不保存路径、运行时 DTO 或业务操作授权，也不执行备份、恢复、校验、重试或其他源对象命令。已有对象、单清单 200 项上限、清单总数 20 份上限、无效编号和保存失败均给出状态反馈。

身份来源沿用现有主键：媒体 `MediaId`、任务 `TaskId`、问题 `finding_id`。备份编号以版本号 `v1` 和长度前缀组合 `PlayniteId` 与 `BackupId`，避免分隔符歧义。问题主键从 Worker 查询投影传至 `ValidationFindingDto`；缺少稳定编号时入口禁用/拒绝加入，不按标题或路径匹配。清单和当前清单 ID 使用现有插件设置持久化。

相关改动：

- Contracts 与 Worker 查询投影：暴露校验发现主键。
- Playnite 设置/视图模型：有界清单保存、稳定引用预览、确认后加入、取消/重复/满额/失败反馈。
- SaveCenter、MediaCenter、TaskCenter、Maintenance：在现有详情操作区接入命令。
- 行为测试：覆盖清单去重/容量、四种来源的稳定编号、不持久化路径、问题缺少 ID、设置 JSON 往返，以及数据库重新 upsert 后发现 ID 稳定。

本项不实现清单浏览、搜索、分组、重排、删除或续接上下文，这些由后续 S06-03/S06-04 处理。

## 验证

- SDK 9.0.302、VSTest 17.14.1，Release 全解决方案 `--no-restore` 构建：0 warning、0 error。仓库 `global.json` 锁定 SDK 8.0.423，本机未安装该版本；从系统临时目录启动以使用 SDK 9，未改 `global.json`。
- Playnite 工作清单定向测试：8/8 通过。
- Worker `R17FindingPersistenceTests`：2/2 通过。
- `python scripts/validate-source.py .` 与 Round4 进度文件校验通过；`git diff --check` 通过。
- WPF UI 静态审查通过，0 errors、27 warnings、179 info。warning 是既有页面布局探针提示，本项未改共享控件模板或页面滚动布局。
- 未运行 RenderHarness/真实 Playnite 屏幕验证。改动是现有详情操作区内的轻量入口；先前可选 RenderHarness 试构建因本地 `project.assets.json` 缺少 `net472/win-x86` target 未能启动。该结果不作为布局或屏幕证据。隔离 Playnite 的既有 CEF/权限阻塞未重试、未绕过。

原始记录：[solution build](release-solution-build.txt)、[Playnite tests](playnite-tests.txt)、[Worker tests](worker-tests.txt)、[Playnite TRX](worklists.trx)、[Worker TRX](findings.trx)。

## SHA-256

| 文件/产物 | SHA-256 |
| --- | --- |
| `release-solution-build.txt` | `0446145DFCF7562F0B3A8DC89263B13FA92A242949342049C8D35408F511DF30` |
| `playnite-tests.txt` | `6C0533DFC46A106E6CB5046AD83656E86F67FA4537620BCAA59443A58EB1C87A` |
| `worker-tests.txt` | `7BF212111D13C7435A8A3E01AD98F0BDE593E39AB615F4B9408145E84392415A` |
| `worklists.trx` | `0882F6B084F69CB6205FB48589BE71E3F782AA34D067D4D9BAFA59829E028400` |
| `findings.trx` | `7443A53F7888B3F6B1C90309FCF175D449F3431452CBBD9A0090160EB04B2104` |
| `GameSaveCenter.Playnite.dll` | `CA33DBF773CE3E3983804DDC7A3C7BBA65413A08B9D1E2659EF5E70462012E81` |
| `GameSaveCenter.Playnite.Tests.dll` | `E02B2EE78FD30A559C26C7E6B85771C56007F4DC8FA94C43B6B10A8826838D58` |
| `GameSaveCenter.Worker.dll` | `A2C3C1E6962DF7CABE83E5DCCEF49B47C11325E84804834334840DDB34D1B26A` |
| `GameSaveCenter.Worker.Tests.dll` | `E3F7DFC7B610069BA3D9500524A45FEA9966092FD1F47ACD18D1D1A8C06557EA` |
