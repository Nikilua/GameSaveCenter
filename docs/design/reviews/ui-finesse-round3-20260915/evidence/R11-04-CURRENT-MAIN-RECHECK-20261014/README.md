# R11-04 版本备注当前 main 复核

更新：2026-10-14。构建时 main 基线 `40b0a980`；本阶段只补测试和证据，没有修改生产实现、XAML 或资源。现有 `BackupComment`、锁定元数据、Ludusavi `EditBackupAsync`、历史刷新与 SQLite `(PlayniteId, BackupId)` upsert 已满足任务方向，因此未重建服务或 DTO。

## 行为证据

- Playnite `R11VersionNoteBehaviorTests` 在隔离 STA Window 中加载生产 `SaveCenterView`。输入控件绑定到 `BackupComment`，取消按钮绑定走测试命令适配器并调用生产 `DashboardViewModel.CancelBackupMetadataEdit`；草稿被丢弃、锁定状态复原、`BackupId` 不变、取消状态可见。此夹具验证真实 View/Binding 和生产取消方法；取消按钮上的测试 `ICommand` 适配器不等同于生产构造器创建的 RelayCommand。
- 另一个行为用例直接调用生产取消方法，预置未保存备注/锁定草稿后检查 DTO 原值、字段 dirty 状态、选中版本引用和稳定 ID 均保留。
- 重复说明文字的两个版本通过生产 `SelectionAnchorResolver.Restore` 以 `BackupId` 还原选择；旧 ID 消失时用前一选中位置作确定性回退，不按重复说明误选。
- Worker 测试以合成 Ludusavi JSON 调用生产 `LudusaviResultParser.ParseBackupList`，将 `backup-a` / `backup-b` 和同名备注写入隔离 SQLite，再以新 `SqliteStateStore` 实例读取。两个 BackupId、各自锁定状态、归档路径与备注均保留。夹具不启动 Ludusavi，也不读取真实存档。
- SaveCenter 元数据编辑绑定、取消命令、IPC 元数据编辑与历史刷新仍有既有接线断言作为补充；它们不单独作为交互行为签收依据。

## 本机构建与测试

| 工具链 | 目标 | 结果 |
| --- | --- | --- |
| SDK `8.0.423`，VSTest `17.11.1` | Playnite `net462` / Tests `net472`，Release | `R11VersionNoteBehaviorTests` 4/4；定向项目与引用构建 0 warning / 0 error |
| SDK `8.0.423`，VSTest `17.11.1` | Worker Tests `net8.0-windows10.0.19041.0`，Release | `R11VersionMetadataPersistenceTests` 1/1；定向项目与引用构建 0 warning / 0 error |
| SDK `9.0.302`，VSTest `17.14.1` | 同上两个定向类 | Playnite 4/4、Worker 1/1 |
| SDK `8.0.423`，VSTest `17.11.1` | R09 焦点轮廓类，当前 `40b0a980` 生产/测试源码 | `R09FocusOutlineBehaviorTests` 2/2；用于复核用户新附外机日志，不改变 R11-04 的测试统计 |

上述均为定向项目构建，不是完整 solution 构建。SDK8 通过隔离路径 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423\dotnet.exe` 运行，保留仓库 `MSBuildEnableWorkloadResolver=false` 设置；本机 SDK 结果不替代推送后的 SDK8/SDK10.0.401 Actions。没有 XAML 变化，不适用本阶段截图/render QA；没有运行 Playnite 宿主。

## 当前身份

R11-04 测试源 SHA-256：

- `R11VersionNoteBehaviorTests.cs`: `9776A6697F8F88DA840FD36D454C4BA674F134712D60045667185A6143EC38AC`
- `R11VersionMetadataPersistenceTests.cs`: `CA800B37074BBA6F6905D5B3F67A7683A223C045954C1D58D33A047493BAD541`

SDK8 Playnite 程序集 ProductVersion 均为 `0.6.73+40b0a980`：

- 插件 DLL SHA-256 `DFF9392CB8F44EC34ACC42C1C684499FA21095DB6F2536C86D8780CCAEC17C43`，MVID `d2d7d5b5-359b-42d5-b616-9615e049240e`。
- Playnite Tests DLL SHA-256 `544BC3C5C124538B79D6C1178C84B2C9B41470259EE10DA42B316B68573D336B`，MVID `28b50be7-adfd-400c-bb7d-ebd709f2741c`；PDB SHA-256 `CFB5CC4EC4B3EF53FF1768AD3BC11BD59E8A371D12C63E01E5E054EB3EAD32D2`。
- Worker Tests DLL SHA-256 `D75C2403CBBE67BC611119D4C935D03778155192547D42D8B238B721DA89EFC6`，MVID `fa3b4a69-9281-43f6-9ab8-0d580808a2cb`；PDB SHA-256 `392E7CDD743445C1388F2F247FE2472D606432B70765EECE7E0A3DEEA36AE106`。

脱敏 TRX：[`tests/`](tests/)。SDK8 文件 SHA-256：Playnite `319290F8D3174BBE507793A5327648CFE7B70F55D4E6BB988F296FB58A02E73C`，Worker `152A2A64A9AE40EF7BE231ECE25784CB428DCDB08BBA1FB07E5A07FC18BCFBD5`；SDK9 对应 `9FF683E52D9BE1C386151BC46F92A7DEAFD2B8415283731D0236CB4D2E857800` 与 `531623475C4C555FEE1D6CFCCB4BA2519719A55AAFFB852138E0A8BA447ABCE7`。R09 当前 main SDK8 TRX 在 [外机失败复核包](../CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/tests/R09FocusOutlineBehaviorTests-sdk8423-main-40b0a980.trx)，SHA-256 `D886C0967EBE969BAC7F384AF1C8FB1EDECB2312636280374AA425BE33499D53`。

## 未验证边界与下一步

本地合成 DTO/SQLite 与隔离 WPF Window 不代表真实 Playnite 宿主、实际 Ludusavi 修改调用或真实备份恢复；没有读取或修改用户存档。代码与受控行为本地完成后推送 main，随后必须把新提交对应的 SDK8 和 SDK10.0.401 Actions 查到明确终态。下一项 R11-05：先核对现有保护状态、保留预览与执行逻辑，再补锁定版本跳过保留策略的正例/负例证据。
