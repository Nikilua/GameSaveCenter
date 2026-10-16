# Worker 取消回调测试复核（2026-10-17）

本轮从 `main` 提交 `ef5c8a7ffafc06cdbf9771ca6f74f90d7435f510` 继续。只修改 Worker 测试夹具，没有修改生产取消/任务协调代码。

## CI 失败与修正

Actions #1055/run `38951586638` 在旧代码提交上终态失败。SDK10.0.401 job `117144037233` 成功。SDK8.0.423 job `117144037407` 的首次 Release build、测试、Render QA 与静态审查通过；打包阶段再次调用 build 时，Worker `361/362`，`TaskCoordinatorFailureTests.CancellationIsIdempotentAndPublishesSafeFinalizationBeforeCancelled` 回调计数期望 `1`、实得 `0`，随后包步骤失败。完整 job log 已通过 GitHub 连接取得；SDK8 诊断 artifact SHA-256 为 `b8bf4da5441171091db4cb9c57627e6e79ece5e7a0632f28ea8268df8d6fef7e`。

根因在测试夹具的回调注册生命周期：取消 `Task.Delay` 时，异步操作可能在 `CancellationTokenSource` 继续枚举注册前内联恢复，并提前释放较早注册的断言回调。测试现在在捕获预期取消后，等待回调信号（最多 2 秒）再释放注册，并继续检查回调只执行一次及终态/安全收尾。超时仍会使测试失败；未弱化断言。

## 本机验证与边界

- Windows 本机 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`。仓库 `scripts/build.ps1 -Configuration Release` 的 Release solution build 为 `0 warning / 0 error`；Core `125/125`；Worker 共 `362`，`361` passed、`1` NotExecuted、`0` failed。
- 从同一隔离输出对修正后的取消测试连续运行 `50` 次，`50/50` 通过。测试 DLL SHA-256 `175BE963DA020F07C68B283F50114C44492A6E0482E0F381BBC1983C2E73C496`，MVID `57a064e4-1e7c-4b54-8c61-c5fd3f70f582`，ProductVersion `0.6.73+ef5c8a7ffafc06cdbf9771ca6f74f90d7435f510`。修改后测试源 SHA-256 `A3E78FA7559DCD7C60E3FC9638391857A5238AFE13B84E43F376F615FA575352`。
- 同一输出的 R09 定向复跑 `2/2`。Playnite 测试 DLL SHA-256 `3A7C485870BA3AF36D2FCB8061214E962B4A74286E47B77A598934B48D419B6B`、MVID `0512033a-bfe4-4f1b-a30a-405efacba904`；生产插件 DLL SHA-256 `EE67CC8CF7BEA6F753E66467815E17C9948A96ED8C75231B6F61347CA300F356`、MVID `cc231442-7aed-4e15-ba27-b8da56f7154c`。R09 测试源 SHA-256 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`。这些是本机同一构建输出的局部证据，不抵消用户另一台机器的 R09 `1/2` 失败。
- 官方完整构建脚本在 WPF 隔离类 `50/116` 停止：`R05TooltipTimingBehaviorTests` 无法读取测试开始前的系统鼠标位置（`GetCursorPos` 前置条件失败）。所以本机完整 WPF 门禁未通过/未跑完；R09 是另行定向复跑。
- 发现数也需谨慎解释：本机当前隔离脚本报告 `116` 个 WPF 类，#1055 同一代码提交的 SDK8 GitHub job log 报告 `125` 个 WPF 类，外机粘贴日志同为 `116`。计数差异不能单独证明外机源错配，需同次测试源、DLL/PDB、生产 DLL 身份解释；具体发现差异仍未定位。

## 脱敏 TRX

- [Worker SDK8.0.423 结果](tests/worker-sdk8423-ef5c8a7.trx)，SHA-256 `E2B3886CDCF5433C9C9E5377C87F960C5B37A476DE17DBAB2462CDE4908BF8D6`。
- [R09 SDK8.0.423 定向结果](tests/R09FocusOutline-sdk8423-ef5c8a7.trx)，SHA-256 `7B496BAE47779517463D6B0AFD330E99426B07DC66BCC825D1BE23574A59DABA`。
- [R05 Tooltip 系统指针前置条件失败](tests/R05TooltipTiming-sdk8423-ef5c8a7.trx)，SHA-256 `F2A6E7ED3CC07CE2291CBBAA862D99A96152F1407CE662E6D9A91689D12ACE07`。

## 推送后 CI 与后续复核

代码提交 `4a98148caf2b0909998824acc73119272fc0cb37` 对应 Actions #1056/run `38962119739` 已在两个 job 成功终态：[GitHub Actions](https://github.com/Nikilua/GameSaveCenter/actions/runs/38962119739)。SDK8.0.423 job `117179503173` 和 SDK10.0.401 job `117179503250` 均 Release build `0 warning / 0 error`，Core `125/125`、Worker `362/362`、Playnite source `107` 类和 WPF 隔离 `125` 类通过。SDK8 另 `render-qa OK`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、`0.6.73` 扩展包生成及上传成功。SDK8 诊断 artifact `12076070827`（SHA-256 `ca2bc811fa05e227f2e2f7972a5f2b84b8cacb5ab98638b7189b0bf35774efce`），SDK10 诊断 artifact `12073339339`（SHA-256 `04fcc7a8fb76bc88a50c48f648a6f72431f523e74bfd0c6eac761a692bf4d129`）。该结果验证 GitHub runner 上这次提交；不代表用户另一台电脑的安装/测试输入一致或成功。

R12-04 相关文件在当前 main 的 SHA 与 2026-10-16 复核包一致：`RestoreOrchestrator.cs` `7C58FD0E...4CF76D`、`RestoreWorkflowProgress.cs` `C520BD5A...21AE4F`、`RestoreOrchestratorTests.cs` `2100A6D4...23B72B`、`R12RestoreWorkflowBehaviorTests.cs` `6C3FDAEB...3B2A3C`。#1056 的完整 Worker 与 Playnite test groups 也已通过；无需重建 R12-04 服务或重复局部运行，本项继续标“已满足（当前 main 复核）”。

外机 R09 的同次源码/DLL/PDB/插件身份和真实 Playnite 呈现仍未取得/未验。最新粘贴文本 SHA-256 与先前附件相同，仍是实际 `1/2` 失败；#1056 两 SDK 通过不能覆盖它。下一步按 backlog 与 R23-08 准入表重扫依赖已满足的 Q/R 工作，外机身份材料等待期间继续独立任务。
