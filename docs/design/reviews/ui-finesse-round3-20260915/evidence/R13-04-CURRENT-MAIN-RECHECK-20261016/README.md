# R13-04 当前 main 暂停设置持久化复核

更新：2026-10-16。代码提交 `ccdbacb683afe6b0a745640d545cc9876332391c`（`ccdbacb6 补充云端暂停设置重启验证`）。本复核只补足 R13-04 的磁盘持久化行为证据，没有修改生产服务或 XAML。

## 验证内容

- 新增 `DeviceIdentityTests.CloudQueuePauseAndWindowSurviveWorkerRestartAndResume`。测试在独立临时目录和合成配置中，保存 `CloudUploadQueuePaused=true` 及跨午夜时段 `1320/120`，通过新建 `WorkerOptions` 从磁盘重载后检查值；随后保存恢复状态 `false` 与时段 `60/300`，再次重载并验证。夹具退出时只删除自己创建的临时目录。这是选项持久化/重建对象的行为测试，不启动真实 Worker Windows Service，也不验证当前上传的取消或真实远端时序。
- R13-04 对应组的定向行为：Worker 暂停/时段策略与持久化 `4/4`；Core 队列摘要 `1/1`；Playnite 云端设置与行为/设置迁移 `24/24`。Playnite 插件编译目标 `net462`，测试程序集 `net472`。
- 同一个隔离 Playnite 测试输出上的 `R09FocusOutlineBehaviorTests` `2/2`。这只能证明当前源码提交和下面标识的程序集在本机通过，不能消除另一台机器的失败，也不能替代完整 Playnite 测试集。
- 本机使用仓库锁定 .NET SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`。Worker、Core 和 Playnite 受影响项目及其引用构建通过；这是定向隔离项目构建，不是完整 solution build。没有 XAML 变化，无需新的视觉截图。

脱敏 TRX： [Worker `4/4`](tests/R13-04-worker-policy-persistence.trx)、[Core `1/1`](tests/R13-04-core-queue-state.trx)、[Playnite `24/24`](tests/R13-04-playnite-settings.trx)、[R09 `2/2`](tests/R09FocusOutlineBehaviorTests-sdk8423-main-ccdbacb6.trx)。源码 freshness 对代码提交 `ccdbacb6` 重核为 `14/14` 条 R00/R01 记录无需重跑、`0` 条登记源码路径命中；package identity 未提供，详见 [精简 freshness 报告](R00-R01-freshness-ccdbacb6.json)。

## 本机产物身份

所有以下 DLL 的 `ProductVersion` 为 `0.6.73+ccdbacb6`。测试程序集和对应 PDB 的 SHA-256 可把 TRX 与本次构建对应起来；源码哈希用于核对测试/生产输入。

| 文件 | SHA-256 |
|---|---|
| `GameSaveCenter.Worker.Tests.dll` | `57CB6EFA0B25314637F8B4DBFF54AB35F40A43A6D57C04343A576C63DEC28AEA` |
| `GameSaveCenter.Worker.Tests.pdb` | `6BF433D1854A63015B7729E743F034FE119B5013AEDB8590F27DFF33C289E182` |
| `GameSaveCenter.Worker.dll` | `F28FF60ED6BA010422833DE6C90EDBB56BE74C642375F88C4E83A0BD98328E7D` |
| `GameSaveCenter.Worker.pdb` | `28421FBAC89D3F53C78B54EAA91D67403B521CBE34C21E4BC1AE30E462742039` |
| `GameSaveCenter.Core.Tests.dll` | `679C3DC3D66534B1D8BE09AB8EDBA05814ECCD8E38C3504EF6D1DB71024640C9` |
| `GameSaveCenter.Core.Tests.pdb` | `915590ADA505620BE425244F9C299738DC8906ACDF11CB3F5AC2375E8CE9AF55` |
| `GameSaveCenter.Playnite.Tests.dll` | `A0D6FA3D6E25CC940AA0B69D08993CF8DE6FB9261723DEE3A8BAF428F30DB7A2` |
| `GameSaveCenter.Playnite.Tests.pdb` | `C5857DF9B80FC9878D1A8F0F186138FABCB97D770380DE8250764A871DF25570` |
| `GameSaveCenter.Playnite.dll` | `B6903A1A2B50533A98C5B7A7C937F4F607645FC226AA4B015F43330335BC1BB5` |
| `GameSaveCenter.Playnite.pdb` | `BD8A1027DE0F0C8398A93380C777C61996453A671327064ED582A913BF7F7E92` |

| 源文件 | SHA-256 |
|---|---|
| `DeviceIdentityTests.cs` | `E3022223598E8B3ED75537F723788B12254BB10AD9E4890EC8FC3E722AF37015` |
| `UiDisplayMappingTests.cs` | `0AC6DDF8BA38E343C319273AB7ED4385233EC49DC97627D7C5C18FBDE88947C8` |
| `R13CloudTransferStageBehaviorTests.cs` | `444BA64816F4AB21CCED6C6033699AD2756CFA6F419E008340ACE164E69667EA` |
| `PortableSettingsTests.cs` | `4853A27DF3B0C8A6FDF7474A810AFF20F2AE98E903EBA6D11486279544CF945F` |
| `R09FocusOutlineBehaviorTests.cs` | `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98` |
| `WorkerOptions.cs` | `B01F9A467EE4082C20F8BF1B4B770F732C066D7687000BFA086A6B4613A483DB` |
| `CloudRetryService.cs` | `0965B14F1CF08E4F7CBF354FEFDF3C506AE608A9152CF628F7E2142FBF002F56` |
| `CloudTransferDtos.cs` | `E1D7C24385F37AE367A7C3C80B24ECF67E10B81DFE3D9F4AE8EA112E91E9684C` |

## 与外机 R09 失败的关系和边界

外机粘贴日志中的厚度失败确实发生过；当前 main 的定向 R09 测试在此处通过，不等于外机“没问题”。本次本机测试源第 151 行检查错误色，第 152 行检查 1 DIP 内容 Chrome，第 154 行检查独立的 2 DIP `ValidationStroke`。外机摘要报告第 151 行期望/实际均为厚度且 WPF 测试类数为 `116`；其失败时的 HEAD、工作树、测试源、DLL/PDB 和实际加载插件资源仍缺失，因此无法确认两边是否运行同一输入或给出外机根因。不要用本机成功覆盖外机失败。

Actions #1038/run `38867768405` 对提交 `e6b77ade`：SDK `10.0.401` job 已成功；SDK8 job 当时仍在“生成 Playnite 扩展包”。该 run 不是含本次测试的 `ccdbacb6`，不作本阶段 CI 门禁。`ccdbacb6` 及本证据提交推送后，应跟踪其 SDK8 和 SDK10.0.401 jobs 到终态再签收本阶段。

未运行完整 Playnite 测试集；本机此前全类扫描在 `R05TooltipTimingBehaviorTests` 因 `GetCursorPos` 返回 Win32 error 5 停止。没有启动真实 Playnite、真实 Worker Service、真实上传或用户数据。下一项继续按积压清单当前 READY 项推进。