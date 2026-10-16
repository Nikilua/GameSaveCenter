# R13-04 当前 main 暂停设置持久化复核

更新：2026-10-16。代码提交 `ccdbacb683afe6b0a745640d545cc9876332391c`（`ccdbacb6 补充云端暂停设置重启验证`）。本复核只补足 R13-04 的磁盘持久化行为证据，没有修改生产服务或 XAML。

## 验证内容

- 新增 `DeviceIdentityTests.CloudQueuePauseAndWindowSurviveWorkerRestartAndResume`。测试在独立临时目录和合成配置中，保存 `CloudUploadQueuePaused=true` 及跨午夜时段 `1320/120`，通过新建 `WorkerOptions` 从磁盘重载后检查值；随后保存恢复状态 `false` 与时段 `60/300`，再次重载并验证。夹具退出时只删除自己创建的临时目录。这是选项持久化/重建对象的行为测试，不启动真实 Worker Windows Service，也不验证当前上传的取消或真实远端时序。
- R13-04 对应组的定向行为：Worker 暂停/时段策略与持久化 `4/4`；Core 队列摘要 `1/1`；Playnite 云端设置与行为/设置迁移 `24/24`。Playnite 插件编译目标 `net462`，测试程序集 `net472`。
- 同一个隔离 Playnite 测试输出上的 `R09FocusOutlineBehaviorTests` `2/2`。这只能证明当前源码提交和下面标识的程序集在本机通过，不能消除另一台机器的失败，也不能替代完整 Playnite 测试集。
- 本机使用仓库锁定 .NET SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`。Worker、Core 和 Playnite 受影响项目及其引用构建通过；这是定向隔离项目构建，不是完整 solution build。没有 XAML 变化，无需新的视觉截图。

脱敏 TRX： [Worker `4/4`](tests/R13-04-worker-policy-persistence.trx)、[Core `1/1`](tests/R13-04-core-queue-state.trx)、[Playnite `24/24`](tests/R13-04-playnite-settings.trx)、[R09 `2/2`](tests/R09FocusOutlineBehaviorTests-sdk8423-main-ccdbacb6.trx)。在当前推送 HEAD `a81e7abd` 重核 R00/R01 freshness：`14/14` 条记录无需重跑、`0` 条登记源码路径命中；package identity 未提供，见 [a81e7abd freshness 报告](R00-R01-freshness-a81e7abd.json)。

## GitHub Actions 对当前推送候选的验证

GitHub Actions [#1039 / run `38872163664`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38872163664) 的 HEAD 为文档提交 `a81e7abd219e136cf784359ac48174e154f157be`，其父级代码提交为 `ccdbacb683afe6b0a745640d545cc9876332391c`。SDK8 主 job `116878566799` 与独立 `.NET SDK 10.0.401` job `116878566530` 均终态成功。两边的完整 job log 都报告 Core `125`、Worker `362`，Playnite source `108` 类、WPF `124` 类；“All Playnite tests passed with WPF classes isolated by process”。R09 焦点类在两边均为 WPF 隔离序号 `68/124`，没有失败。SDK8 主 job 继续完成 `render-qa OK`、WPF UI 静态审查和 `scripts/package.ps1` 包生成；SDK10 job单独验证 Release build/test。该 run 覆盖的是 GitHub hosted Windows runner，不是用户另一台电脑。

当前 run 的远端产物身份：`GameSaveCenter-windows-preview` artifact `12036106836`，90,313,108 bytes，SHA-256 `dff07e2145b3eec7a733417398bc31af01f43536eec81a7cdbdc4b31d478527c`；SDK8 diagnostics artifact `12035717811`，SHA-256 `01eaf6b8bb6e9859fd4d2f30eb3527bc9ccc16141609b97e55e122bc000ccc14`；SDK10 diagnostics artifact `12034633506`，SHA-256 `1dbd383b2c91a50205b7870b6cf03719695e09f3003ca5fbf15aacf96543e41a`。这些只记录在 GitHub，无需下载到本机工作区。

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

外机粘贴日志中的厚度失败确实发生过；当前 main 的定向 R09 测试在此处通过，不等于外机“没问题”。本次新附完整输出记录 source classes `111`、WPF classes `116`，并在 `R09FocusOutlineBehaviorTests` line 151 断言 `Chrome.BorderThickness == 2 DIP`。当前 main line 151 检查错误色、line 152 检查内容 Chrome `1 DIP`、line 154 才检查外圈 `ValidationStroke 2 DIP`；历史提交 `3f7d0b30` 的 line 151 与外机数值断言相同，`f13b9e05` 已拆分 Chrome 与独立外圈检查。故本次日志中的测试源/符号映射与当前 main 不同。输出目录 `artifacts/gsc-b/fd7672ef` 是 `dev-install-run.ps1` 生成的随机构建目录，不是 commit 标识；粘贴文本未包含 `sourceCommit`、dirty paths、测试源/DLL/PDB SHA 或实际安装的插件 DLL 身份。可确认测试输入版本不同，但无法仅凭该片段判断是旧 checkout、旧测试程序集或安装包身份造成，也不推断机器具体 HEAD。当前候选 GitHub Actions #1039 两个 SDK lane 完整 WPF `124` 类均通过，包含 R09；这证明当前推送候选在 hosted runner 通过，不覆盖外机失败。

前一 Actions #1038/run `38867768405` 对 `e6b77ade` 的 SDK8 与 SDK10.0.401 jobs 现均终态成功，但该 HEAD 不含本次测试。当前阶段由 #1039 的两个成功 job 完成本次 CI 门禁。

本机完整 Playnite 扫描仍因受限桌面的 `GetCursorPos` 返回 Win32 error 5 停止；没有绕过该限制。GitHub hosted Windows runner 上 SDK8/SDK10 两个完整 Playnite 类隔离入口均成功，因而将其区分为本机环境限制，不再阻塞自动化门禁。没有启动真实 Playnite、真实 Worker Service、真实上传或用户数据。R09 外机 `Expected 2 / Actual 1` 仍未复现或归因：外机同次 HEAD、工作树、测试源、测试 DLL/PDB 和加载插件 DLL 身份都未提供。R10-02 至 R10-08 的既有受控证据已由 Actions #1039 双 SDK 完整 WPF 门禁补齐。下一项按活动积压清单复核 R11-01 当前 main 证据。
